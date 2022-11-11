// == Inspoy Technology ==
// Assembly: Instech.Framework.AssetHelper
// FileName: BundleManager.cs
// Created on 2019/12/17 by inspoy
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Instech.Framework.Common;
using Instech.Framework.Core;
using Instech.Framework.Logging;
using Instech.Framework.Utils;
using UnityEngine;
using UnityEngine.Profiling;
using Aes = Instech.EncryptHelper.Aes;
using Logger = Instech.Framework.Logging.Logger;
using Object = UnityEngine.Object;

namespace Instech.Framework.AssetHelper
{
    /// <summary>
    /// 从外部文件加载Bundle数据，支持同步或异步
    /// </summary>
    internal class AssetPackLoader
    {
        public class LoadTask : IPoolable
        {
            public bool IsCompleted => !_isRunning;
            public byte[] Result { get; private set; }
            public bool IsSuccessful { get; private set; }
            public string ErrorMessage { get; private set; }

            public string PackName;
            public string PackPath;
            public string BundleName;
            private bool _isRunning;
            private Thread _thread;

            public void Start()
            {
                _isRunning = true;
                _thread = new Thread(ThreadMain);
                _thread.Start();
            }

            private void ThreadMain()
            {
                try
                {
                    Result = FilePacker.FilePacker.ReadFileContent(PackPath, BundleName, "InstechSecretResource_" + PackName);
                }
                catch (Exception e)
                {
                    _isRunning = false;
                    IsSuccessful = false;
                    ErrorMessage = e.ToString();
                    return;
                }

                _isRunning = false;
                IsSuccessful = true;
            }

            public void Wait()
            {
                while (_isRunning)
                {
                    // do nothing
                }
            }

            public void OnRecycle()
            {
                if (_thread != null)
                {
                    _thread.Abort();
                    _thread = null;
                }
            }

            public void OnActivate()
            {
                Result = null;
                IsSuccessful = false;
                ErrorMessage = string.Empty;
                PackName = string.Empty;
                PackPath = string.Empty;
                BundleName = string.Empty;
                _isRunning = false;
                _thread = null;
            }

            public void OnDestroy()
            {
                // do nothing
            }
        }

        private readonly BundleManager _mgr;

        public AssetPackLoader(BundleManager mgr)
        {
            _mgr = mgr;
        }

        public byte[] LoadSync(string bundleName)
        {
            var packName = _mgr.GetPackNameByBundleName(bundleName);
            if (string.IsNullOrEmpty(packName))
            {
                throw new AssetException(ErrorCode.BundleNotFound, bundleName, "Cannot get packName");
            }

            var packPath = PathHelper.ResourceDataPath + packName;
            return FilePacker.FilePacker.ReadFileContent(packPath, bundleName, "InstechSecretResource_" + packName);
        }

        public LoadTask LoadAsync(string bundleName)
        {
            var packName = _mgr.GetPackNameByBundleName(bundleName);
            if (string.IsNullOrEmpty(packName))
            {
                throw new AssetException(ErrorCode.BundleNotFound, bundleName, "Cannot get packName");
            }

            var packPath = PathHelper.ResourceDataPath + packName;
            var task = ObjectPool<LoadTask>.GetNew();
            task.BundleName = bundleName;
            task.PackPath = packPath;
            task.PackName = packName;
            task.Start();
            return task;
        }
    }

    /// <summary>
    /// 已加载了的Bundle
    /// </summary>
    internal class LoadedBundle
    {
        /// <summary>
        /// 引用次数归零后多少秒卸载
        /// </summary>
        private const float MaxIdleTime = 1f;

        public readonly bool IsSync;
        public bool IsDone { get; private set; }

        /// <summary>
        /// AssetBundle唯一名称
        /// </summary>
        public readonly string BundleName;

        /// <summary>
        /// 加载完成的Bundle对象
        /// </summary>
        public AssetBundle Bundle { get; private set; }

        /// <summary>
        /// Bundle依赖列表
        /// </summary>
        public readonly List<LoadedBundle> Dependencies = new List<LoadedBundle>();

        /// <summary>
        /// 引用次数归零的时间(秒)，0表示引用次数尚未归零
        /// </summary>
        public float UnloadTime { get; private set; }

        /// <summary>
        /// 引用计数，加载其中包含的Asset和作为依赖包被加载都会+1
        /// </summary>
        public int ReferenceCount
        {
            get => _refCount;
            set
            {
                _refCount = value;
                if (_refCount <= 0)
                {
                    UnloadTime = Time.realtimeSinceStartup;
                }
                else
                {
                    UnloadTime = 0;
                }
            }
        }

        private int _refCount;

        /// <summary>
        /// 从assetpack加载到内存的task
        /// </summary>
        private AssetPackLoader.LoadTask _loadTask;

        /// <summary>
        /// 从内存加载成AssetBundle的Task
        /// </summary>
        private AssetBundleCreateRequest _bundleRequest;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bundleName">Bundle名字</param>
        /// <param name="isSync">是否为同步加载</param>
        /// <param name="mgr">BundleManager对象</param>
        public LoadedBundle(string bundleName, bool isSync, BundleManager mgr)
        {
            BundleName = bundleName;
            IsSync = isSync;
            if (isSync)
            {
                if (mgr.VerboseLog)
                {
                    Logger.LogInfo(LogModule.Resource, "Load Bundle Sync: " + bundleName);
                }

                var content = mgr.PackLoader.LoadSync(bundleName);
                Bundle = AssetBundle.LoadFromMemory(content);
                if (Bundle == null)
                {
                    throw new AssetException(ErrorCode.LoadFailed, bundleName);
                }
                mgr.LoadDependencies(this);
                ReferenceCount = 0;
                IsDone = true;
            }
            else
            {
                if (mgr.VerboseLog)
                {
                    Logger.LogInfo(LogModule.Resource, "Load Bundle Async Start: " + bundleName);
                }

                _loadTask = mgr.PackLoader.LoadAsync(bundleName);
                mgr.LoadDependencies(this);
                IsDone = false;
            }
        }

        /// <param name="now">当前时刻</param>
        /// <returns>是否可以卸载了</returns>
        public bool OnUpdate(float now)
        {
            if (_loadTask != null && _loadTask.IsCompleted)
            {
                if (!_loadTask.IsSuccessful || _loadTask.Result == null)
                {
                    throw new AssetException(ErrorCode.LoadFailed, BundleName, "Load Bundle Async Failed: " + _loadTask.ErrorMessage);
                }

                // 加载完毕
                Logger.LogInfo(LogModule.Resource, "Load Bundle Async Finish: " + BundleName);
                var content = _loadTask.Result;
                _bundleRequest = AssetBundle.LoadFromMemoryAsync(content);
                _loadTask.Recycle();
                _loadTask = null;
            }

            if (_bundleRequest != null && _bundleRequest.isDone && IsDependenciesAllDone())
            {
                Bundle = _bundleRequest.assetBundle;
                _bundleRequest = null;
                IsDone = true;
            }

            return IsDone && UnloadTime > 0 && now > UnloadTime + MaxIdleTime;
        }

        /// <summary>
        /// 立即阻塞等待异步操作完成
        /// </summary>
        public void CompleteLoading()
        {
            if (IsSync || IsDone || _loadTask == null)
            {
                return;
            }

            if (!_loadTask.IsCompleted)
            {
                _loadTask.Wait();
                OnUpdate(-1);
            }
        }

        /// <summary>
        /// 检查依赖项是否全部加载完毕
        /// </summary>
        /// <returns></returns>
        private bool IsDependenciesAllDone()
        {
            foreach (var dependency in Dependencies)
            {
                if (!dependency.IsDone)
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal interface IAssetLoadTask
    {
        LoadedBundle RequestBundle { get; set; }
        bool Update();
    }

    /// <summary>
    /// 异步加载任务
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    internal class AssetLoadTask<T> : IAssetLoadTask, IPoolable where T : Object
    {
        public uint TaskId;
        public Action<ErrorCode, T> Callback;
        public string RequestAssetName;
        public LoadedBundle RequestBundle { get; set; }
        public BundleManager Mgr;
        public bool DontRecord;
        private AssetBundleRequest _assetReq;

        public bool Update()
        {
            if (RequestBundle.IsDone && _assetReq == null)
            {
                _assetReq = RequestBundle.Bundle.LoadAssetAsync<T>(RequestAssetName);
            }

            if (_assetReq != null && _assetReq.isDone)
            {
                var asset = _assetReq.asset as T;
                if (!DontRecord)
                {
                    Mgr.AddRecord(asset, RequestBundle);
                }

                Callback(ErrorCode.Success, asset);
                return true;
            }

            return false;
        }

        public void OnRecycle()
        {
            TaskId = 0;
            Callback = null;
            RequestAssetName = null;
            RequestBundle = null;
            Mgr = null;
            DontRecord = false;
            _assetReq = null;
        }

        public void OnActivate()
        {
            // do nothing
        }

        public void OnDestroy()
        {
            // do nothing
        }
    }

    internal class BundleManager : IAssetManager
    {
        public bool VerboseLog { get; set; }
        public AssetPackLoader PackLoader;

        public static uint TaskIdSequence = 100;

        /// <summary>
        /// Bundle依赖关系，一对多
        /// </summary>
        private readonly Dictionary<string, List<string>> _dependencyMap;

        /// <summary>
        /// Asset路径对照表，Tuple为（BundleName，AssetName）
        /// </summary>
        private readonly Dictionary<string, ValueTuple<string, string>> _addressMap;

        /// <summary>
        /// Bundle对应的AssetPack
        /// </summary>
        private readonly Dictionary<string, string> _packMap;

        private readonly Dictionary<string, LoadedBundle> _cachedBundles = new Dictionary<string, LoadedBundle>();
        private readonly Dictionary<string, LoadedBundle> _loadedBundleRemoveList = new Dictionary<string, LoadedBundle>();
        private readonly Dictionary<uint, IAssetLoadTask> _pendingAssets = new Dictionary<uint, IAssetLoadTask>();
        private readonly List<uint> _loadingTaskRemoveList = new List<uint>();
        private readonly Dictionary<int, LoadedBundle> _assetRecord = new Dictionary<int, LoadedBundle>();
        private readonly Dictionary<int, int> _assetRefCount = new Dictionary<int, int>();
        private bool _pauseRecording;

        public BundleManager()
        {
            // load resmeta
            var sw = Stopwatch.StartNew();
            var resourcesRoot = PathHelper.ResourceDataPath;
            var resmetaPath = Path.Combine(resourcesRoot, "resmeta");
            var resMetaRaw = File.ReadAllBytes(resmetaPath);
            var aes = new Aes();
            var aesKey = Convert.FromBase64String("rgKbWVdP4G+4dsNB9Baxdtm/G5VgPWEVhCF/bZ+uNWxtezoocMUREEeLhxaPhmct");
            aesKey = SHA384.Create().TransformFinalBlock(aesKey, 0, aesKey.Length);
            aes.Init(aesKey);
            var resMetaReal = aes.Decrypt(resMetaRaw);
            aes.UnInit();
            var ms = new MemoryStream(resMetaReal);
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            {
                // 1. Dependency Map
                _dependencyMap = new Dictionary<string, List<string>>();
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == null || line.StartsWith("#"))
                    {
                        break;
                    }

                    var items = line.Split(',');
                    var abName = items[0];
                    _dependencyMap[abName] = new List<string>(items.Length - 1);
                    for (var i = 1; i < items.Length; ++i)
                    {
                        _dependencyMap[abName].Add(items[i]);
                    }
                }

                // 2. Address Map
                _addressMap = new Dictionary<string, ValueTuple<string, string>>();
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == null || line.StartsWith("#"))
                    {
                        break;
                    }

                    var items = line.Split('|');
                    var addr = items[0];
                    var assetName = items[1];
                    var abName = items[2];
                    _addressMap.Add(addr, (abName, assetName));
                }

                // 3. AssetPack Map
                _packMap = new Dictionary<string, string>();
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == null || line.StartsWith("#"))
                    {
                        break;
                    }

                    var items = line.Split('|');
                    var abName = items[0];
                    var packName = items[1];
                    _packMap.Add(abName, packName);
                }
            }

            Logger.LogInfo(LogModule.Resource, $"Loaded {_packMap.Count} assetpack definitions");
            Logger.LogInfo(LogModule.Resource, $"Loaded {_addressMap.Count} asset definitions");
            Logger.LogInfo(LogModule.Resource, $"Load resmeta cost: {sw.ElapsedMilliseconds}ms");
            PackLoader = new AssetPackLoader(this);
        }

        /// <summary>
        /// 根据Bundle名字获取所在的AssetPack文件名
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public string GetPackNameByBundleName(string bundleName)
        {
            return !_packMap.TryGetValue(bundleName, out var packName) ? null : packName;
        }

        public void AddRecord(Object asset, LoadedBundle bundle)
        {
            if (_pauseRecording)
            {
                return;
            }

            bundle.ReferenceCount += 1;
            var hashCode = asset.GetHashCode();
            _assetRecord[hashCode] = bundle;
            if (_assetRefCount.ContainsKey(hashCode))
            {
                _assetRefCount[hashCode] += 1;
            }
            else
            {
                _assetRefCount[hashCode] = 1;
            }
        }

        /// <inheritdoc />
        public GameObject Clone(GameObject baseGo, Transform parent = null)
        {
            var hashCode = baseGo.GetHashCode();
            var go = Object.Instantiate(baseGo, parent);
            if (_assetRecord.TryGetValue(hashCode, out var bundle))
            {
                AddRecord(go, bundle);
            }

            return go;
        }

        /// <inheritdoc />
        public GameObject InstantiatePrefab(string path, Transform parent = null)
        {
            if (_addressMap.TryGetValue(path, out var vt))
            {
                var (bundleName, _) = vt;
                _pauseRecording = true;
                var prefab = LoadAsset<GameObject>(path);
                _pauseRecording = false;
                var go = Object.Instantiate(prefab, parent);
                AddRecord(go, _cachedBundles[bundleName]);
                return go;
            }

            Logger.LogError(LogModule.Resource, "Unknown asset: " + path);
            throw new AssetException(ErrorCode.UnknownPath, path);
        }

        /// <inheritdoc />
        public uint InstantiatePrefabAsync(string path, Transform parent, Action<ErrorCode, GameObject> callback)
        {
            if (_addressMap.TryGetValue(path, out var vt))
            {
                var (bundleName, _) = vt;
                _pauseRecording = true;
                var taskId = LoadAssetAsync<GameObject>(path, (code, prefab) =>
                {
                    var go = Object.Instantiate(prefab, parent);
                    AddRecord(go, _cachedBundles[bundleName]);
                    callback(code, go);
                });
                _pauseRecording = false;
                return taskId;
            }

            Logger.LogError(LogModule.Resource, "Unknown asset: " + path);
            throw new AssetException(ErrorCode.UnknownPath, path);
        }

        /// <inheritdoc />
        public T LoadAsset<T>(string path) where T : Object
        {
            if (_addressMap.TryGetValue(path, out var vt))
            {
                var (bundleName, assetName) = vt;
                var useCache = _cachedBundles.TryGetValue(bundleName, out var bundle);
                if (VerboseLog)
                {
                    Logger.LogInfo(LogModule.Resource, $"LoadingAsset{(useCache ? "(cached)" : string.Empty)}: {path}");
                }

                if (!useCache)
                {
                    bundle = new LoadedBundle(bundleName, true, this);
                    _cachedBundles.Add(bundleName, bundle);
                }

                if (!bundle.IsSync && !bundle.IsDone)
                {
                    bundle.CompleteLoading();
                }

                var asset = bundle.Bundle.LoadAsset<T>(assetName);
                if (asset == null)
                {
                    throw new AssetException(ErrorCode.LoadFailed, path, "type=" + typeof(T));
                }

                AddRecord(asset, bundle);
                return asset;
            }

            Logger.LogError(LogModule.Resource, "Unknown asset: " + path);
            throw new AssetException(ErrorCode.UnknownPath, path);
        }

        /// <inheritdoc />
        public uint LoadAssetAsync<T>(string path, Action<ErrorCode, T> callback) where T : Object
        {
            if (_addressMap.TryGetValue(path, out var vt))
            {
                var (bundleName, assetname) = vt;
                var useCache = _cachedBundles.TryGetValue(bundleName, out var bundle);
                if (VerboseLog)
                {
                    Logger.LogInfo(LogModule.Resource, $"LoadingAsset{(useCache ? "(cached)" : string.Empty)}: {path}");
                }

                if (!useCache)
                {
                    bundle = new LoadedBundle(bundleName, false, this);
                    _cachedBundles.Add(bundleName, bundle);
                }

                var task = ObjectPool<AssetLoadTask<T>>.GetNew();
                task.TaskId = ++TaskIdSequence;
                task.Callback = callback;
                task.RequestBundle = bundle;
                task.RequestAssetName = assetname;
                task.Mgr = this;
                task.DontRecord = _pauseRecording;
                _pendingAssets.Add(task.TaskId, task);
                return task.TaskId;
            }

            Logger.LogError(LogModule.Resource, "Unknown asset: " + path);
            throw new AssetException(ErrorCode.UnknownPath, path);
        }

        /// <inheritdoc />
        public void UnloadAsset(Object asset)
        {
            var hashCode = asset.GetHashCode();
            if (!_assetRefCount.ContainsKey(hashCode) || !_assetRecord.ContainsKey(hashCode))
            {
                Logger.LogWarning(LogModule.Resource, "Asset has no record(already unloaded?): " + asset.name);
                return;
            }

            _assetRefCount[hashCode] -= 1;
            _assetRecord[hashCode].ReferenceCount -= 1;
            if (_assetRefCount[hashCode] <= 0)
            {
                _assetRecord.Remove(hashCode);
            }
        }

        /// <inheritdoc />
        public float QueryProgress(uint loadingId)
        {
            if (_pendingAssets.TryGetValue(loadingId, out var task))
            {
                return task.RequestBundle.IsDone ? 1 : 0;
            }

            return -1;
        }

        /// <inheritdoc />
        public void UpdateFrame()
        {
            Profiler.BeginSample("BundleManager");
            UpdateLoadingAssets();
            
            // Update Bundles whether should be unloaded
            var now = Time.realtimeSinceStartup;
            foreach (var pair in _cachedBundles)
            {
                var shouldUnload = pair.Value.OnUpdate(now);
                if (shouldUnload)
                {
                    _loadedBundleRemoveList.Add(pair.Key, pair.Value);
                }
            }

            if (_loadedBundleRemoveList.Count > 0)
            {
                foreach (var pair in _loadedBundleRemoveList)
                {
                    UnloadBundle(pair);
                }

                _loadedBundleRemoveList.Clear();
            }

            Profiler.EndSample();
        }

        private void UpdateLoadingAssets()
        {
            foreach (var task in _pendingAssets)
            {
                if (task.Value.Update())
                {
                    _loadingTaskRemoveList.Add(task.Key);
                }
            }
            if (_loadingTaskRemoveList.Count <= 0)
            {
                return;
            }
            foreach (var item in _loadingTaskRemoveList)
            {
                _pendingAssets.Remove(item);
            }

            _loadingTaskRemoveList.Clear();
        }

        private void UnloadBundle(KeyValuePair<string, LoadedBundle> pair)
        {
            pair.Value.Bundle.Unload(true);
            if (_dependencyMap.TryGetValue(pair.Key, out var deps))
            {
                foreach (var dep in deps)
                {
                    if (_cachedBundles.TryGetValue(dep, out var cachedBundle))
                    {
                        cachedBundle.ReferenceCount -= 1;
                    }
                }
            }

            _cachedBundles.Remove(pair.Key);
            if (VerboseLog)
            {
                Logger.LogInfo(LogModule.Resource, "Unload Bundle: " + pair.Key);
            }
        }

        /// <summary>
        /// 加载依赖项
        /// </summary>
        /// <param name="loadedBundle"></param>
        public void LoadDependencies(LoadedBundle loadedBundle)
        {
            if (_dependencyMap.TryGetValue(loadedBundle.BundleName, out var deps))
            {
                if (VerboseLog)
                {
                    Logger.LogInfo(LogModule.Resource, $"Will load {deps.Count} dependencies for {loadedBundle.BundleName}");
                }

                foreach (var dep in deps)
                {
                    if (!_cachedBundles.TryGetValue(dep, out var depBundle))
                    {
                        depBundle = new LoadedBundle(dep, loadedBundle.IsSync, this);
                        _cachedBundles.Add(dep, depBundle);
                    }

                    // 作为依赖包被加载会触发引用+1
                    depBundle.ReferenceCount += 1;
                    // 同步加载的情况，等待所有依赖完成加载
                    if (loadedBundle.IsSync && !depBundle.IsDone)
                    {
                        depBundle.CompleteLoading();
                    }

                    loadedBundle.Dependencies.Add(depBundle);
                }
            }
        }

        public Dictionary<string, int> GetDebugInfo()
        {
            var ret = new Dictionary<string, int>();
            foreach (var item in _cachedBundles.Values)
            {
                if (item.IsDone)
                {
                    ret.Add(item.BundleName, item.ReferenceCount);
                }
                else
                {
                    ret.Add(item.BundleName + "(loading)", item.ReferenceCount);
                }
            }

            return ret;
        }
    }
}
