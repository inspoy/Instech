/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: AssetBundleManager.cs
 * Created on 2018/05/02 by inspoy
 * All rights reserved.
 */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Instech.Framework
{
    /// <summary>
    /// 已经加载了的AssetBundle
    /// </summary>
    internal class LoadedAssetBundle
    {
        /// <summary>
        /// 引用计数
        /// </summary>
        public int ReferenceCount
        {
            get => _referenceCount;

            set
            {
                _referenceCount = value;
                if (_referenceCount <= 0)
                {
                    // 引用次数归零
                    UnloadTime = Time.realtimeSinceStartup;
                }
                else
                {
                    UnloadTime = 0;
                }
            }
        }

        /// <summary>
        /// 引用次数归零的时间
        /// </summary>
        public float UnloadTime { get; private set; }

        public string[] Dependencies { get; }

        public readonly string BundleName;
        public readonly AssetBundle Bundle;
        private int _referenceCount;

        public LoadedAssetBundle(string bundleName)
        {
            BundleName = bundleName;
            Bundle = AssetBundle.LoadFromFile(AssetBundleManager.AssetBundleRootPath + bundleName);
            ReferenceCount = 0;
            Dependencies = AssetBundleManager.MainManifest.GetAllDependencies(bundleName);
            if (Bundle == null)
            {
                Logger.LogWarning(LogModule.Resource, "加载AB失败: " + bundleName);
            }
            else
            {
                Logger.LogInfo(LogModule.Resource, "加载了AB: " + bundleName);
            }
        }
    }

    internal class PathMapItem
    {
        /// <summary>
        /// 资产路径，形如Prefab
        /// </summary>
        public string AssetPath;

        public string BundleName;
        public string AssetName;
    }

    /// <summary>
    /// 用于记录加载的Asset对应的AssetPath
    /// 便于直接用Object来回收AssetBundle
    /// </summary>
    public class LoadedBundleHelper
    {
        private readonly Dictionary<int, string> _cachedAssetPath = new Dictionary<int, string>();
        private readonly Dictionary<int, int> _cachedAssetCount = new Dictionary<int, int>();

        public void AddRecord(Object asset, string assetPath)
        {
            var objId = asset.GetInstanceID();
            if (_cachedAssetPath.ContainsKey(objId))
            {
                _cachedAssetCount[objId] += 1;
            }
            else
            {
                _cachedAssetCount.Add(objId, 1);
                _cachedAssetPath.Add(objId, assetPath);
            }
        }

        internal bool RemoveRecord(int objId, out string assetPath)
        {
            if (!_cachedAssetPath.TryGetValue(objId, out assetPath))
            {
                return false;
            }
            _cachedAssetCount[objId] -= 1;
            if (_cachedAssetCount[objId] <= 0)
            {
                _cachedAssetCount.Remove(objId);
                _cachedAssetPath.Remove(objId);
            }
            return true;
        }
    }

    /// <summary>
    /// AssetBundleManager的初始化参数
    /// </summary>
    public class AssetBundleManagerInitOption : Singleton<AssetBundleManagerInitOption>
    {
        public bool UseAssetBundle { get; set; }
        protected override void Init()
        {
#if UNITY_EDITOR
            UseAssetBundle = false;
#else
            UseAssetBundle = true;
#endif
        }
    }

    /// <summary>
    /// AssetBundle加载管理
    /// </summary>
    public class AssetBundleManager : MonoSingleton<AssetBundleManager>
    {
        private const float UpdateInterval = 10.0f; // 每10秒执行一次清理
        private const float UnloadThreshold = 30.0f; // 清理超过30秒未使用的AssetBundle
        public static string AssetBundleRootPath { get; private set; }
        public static AssetBundleManifest MainManifest { get; private set; }
#if UNITY_EDITOR
        /// <summary>
        /// 编辑器中用的缓存
        /// </summary>
        private readonly Dictionary<string, Object> _dictCache = new Dictionary<string, Object>();
#endif
        private readonly Dictionary<string, LoadedAssetBundle> _dictLoadedBundles = new Dictionary<string, LoadedAssetBundle>();
        private readonly Dictionary<string, PathMapItem> _pathMap = new Dictionary<string, PathMapItem>();
        private float _updateTimer;
        private LoadedBundleHelper _bundleHelper;
        private bool _useBundle;

        /// <summary>
        /// 根据类型获取资产
        /// </summary>
        /// <typeparam name="T">资产类型</typeparam>
        /// <param name="path">资产路径，相对Artwork</param>
        /// <returns></returns>
        public T LoadAsset<T>(string path) where T : Object
        {
            // 编辑器下不加载AB，用AssetDataBase加载
            // 非编辑器下根据BuildAB时生成的PathMap来找到AB路径，加载之
            T ret;
            if (_useBundle)
            {
                ret = LoadAssetFromAssetBundle<T>(path);
            }
            else {
            ret = LoadAssetInEditor<T>("Assets" + EditorPrefs.GetString("Instech_EditorPrefs_AssetsRootPath", "/Artwork/") + path);
            }
            if (ret == null)
            {
                Logger.LogWarning(LogModule.Resource, $"加载Asset<{typeof(T)}>失败: {path}");
            }
            return ret;
        }

        /// <summary>
        /// 添加一条缓存数据
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="assetPath"></param>
        public void AddRecord(Object asset, string assetPath)
        {
            // TODO: 之后看能不能搞成自动的
            _bundleHelper.AddRecord(asset, assetPath);
        }

        /// <summary>
        /// 通过路径回收资产
        /// </summary>
        /// <param name="path">资产路径</param>
        public void UnloadAsset(string path)
        {
            if (_useBundle)
            {
                RecycleAsset(path, false);
            }
        }

        /// <summary>
        /// 通过记录的对象回收资产
        /// </summary>
        /// <typeparam name="T">资产类型</typeparam>
        /// <param name="asset">要回收的Object</param>
        public void UnloadAsset<T>(T asset) where T : Object
        {
            if (!_bundleHelper.RemoveRecord(asset.GetInstanceID(), out var assetPath))
            {
                Logger.LogError(LogModule.Resource, $"找不到记录:{asset.name}({asset.GetInstanceID()})");
                return;
            }
            UnloadAsset(assetPath);
        }

        /// <summary>
        /// 清理内存，立即彻底卸载暂时用不到的AssetBundle
        /// 可以在非关键阶段（如切换关卡）手动调用
        /// </summary>
        public void CleanUnusedAssetBundles()
        {
            List<string> keysToRemove = null;
            foreach (var item in _dictLoadedBundles)
            {
                if (item.Value.ReferenceCount > 0 ||
                    !(Time.realtimeSinceStartup - item.Value.UnloadTime > UnloadThreshold)) continue;
                item.Value.Bundle.Unload(true);
                Logger.LogInfo(LogModule.Data, "卸载了AB: " + item.Key);
                if (keysToRemove == null)
                {
                    // 按需new
                    keysToRemove = new List<string>();
                }
                keysToRemove.Add(item.Key);
            }

            if (keysToRemove == null) return;
            foreach (var key in keysToRemove)
            {
                _dictLoadedBundles.Remove(key);
            }
        }

        protected override void Init()
        {
            _useBundle = AssetBundleManagerInitOption.Instance.UseAssetBundle;
            if (_useBundle)
            {
                AssetBundleRootPath = Application.streamingAssetsPath + "/Bundles/";
                var bundles = AssetBundle.LoadFromFile(AssetBundleRootPath + "Bundles");
                MainManifest = bundles.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                bundles.Unload(false);

                var filePath = AssetBundleRootPath + "pathMap.txt";
                var assets = System.IO.File.ReadAllLines(filePath);
                foreach (var line in assets)
                {
                    var split = line.Split('|');
                    var item = new PathMapItem
                    {
                        AssetPath = split[0],
                        BundleName = split[1],
                        AssetName = split[2]
                    };
                    _pathMap.Add(item.AssetPath, item);
                }

                Logger.LogInfo(LogModule.Resource, $"PathMap中有{assets.Length}条记录");
            }
            _bundleHelper = new LoadedBundleHelper();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器下通过AssetDatabase加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        private T LoadAssetInEditor<T>(string path) where T : Object
        {
            if (_dictCache.TryGetValue(path, out var asset))
            {
                return asset as T;
            }
            var ret = AssetDatabase.LoadAssetAtPath<T>(path);
            _dictCache.Add(path, ret);
            return ret;
        }
#endif

        /// <summary>
        /// 通过AssetBundle加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        private T LoadAssetFromAssetBundle<T>(string path) where T : Object
        {
            if (!_pathMap.TryGetValue(path, out var item))
            {
                Logger.LogWarning(LogModule.Resource, "Asset不存在于PathMap中: " + path);
                return null;
            }

            if (!_dictLoadedBundles.TryGetValue(item.BundleName, out var loadedBundle))
            {
                loadedBundle = new LoadedAssetBundle(item.BundleName);
                _dictLoadedBundles.Add(item.BundleName, loadedBundle);
            }
            LoadAllDependencies(item.BundleName);
            loadedBundle.ReferenceCount += 1;
            if (loadedBundle.Bundle == null)
            {
                return null;
            }
            return loadedBundle.Bundle.LoadAsset<T>(item.AssetName);
        }

        /// <summary>
        /// 加载指定AB包的所有依赖
        /// </summary>
        /// <param name="bundleName"></param>
        private void LoadAllDependencies(string bundleName)
        {
            if (!_dictLoadedBundles.TryGetValue(bundleName, out var loadedBundle))
            {
                loadedBundle = new LoadedAssetBundle(bundleName);
                _dictLoadedBundles.Add(bundleName, loadedBundle);
            }
            foreach (var item in loadedBundle.Dependencies)
            {
                LoadAllDependencies(item);
            }
            loadedBundle.ReferenceCount += 1;
        }
        
        /// <summary>
        /// 清理回收指定AB包，包括所有依赖在内的引用计数减1，并不会马上卸载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isBundle"></param>
        private void RecycleAsset(string path, bool isBundle)
        {
            var bundleName = path;
            if (!isBundle)
            {
                if (!_pathMap.TryGetValue(path, out var item))
                {
                    Logger.LogWarning(LogModule.Resource, "Asset不存在于PathMap中: " + path);
                    return;
                }
                bundleName = item.BundleName;
            }

            if (!_dictLoadedBundles.TryGetValue(bundleName, out var loadedBundle) || loadedBundle.ReferenceCount <= 0)
            {
                Debug.LogWarning("试图卸载未加载的AssetBundle:" + bundleName);
                return;
            }

            // 清理依赖
            foreach (var dep in loadedBundle.Dependencies)
            {
                RecycleAsset(dep, true);
            }
            loadedBundle.ReferenceCount -= 1;
        }

        private void Update()
        {
            // 更新LoadedAB的生命周期
            _updateTimer += Time.deltaTime;
            if (_updateTimer > UpdateInterval)
            {
                _updateTimer -= UpdateInterval;
                CleanUnusedAssetBundles();
            }
        }
    }
}
