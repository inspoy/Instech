/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.AssetHelper
 * FileName: AssetManager.cs
 * Created on 2019/12/10 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using Instech.Framework.Core;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Instech.Framework.AssetHelper
{
    internal interface IAssetManager
    {
        /// <summary>
        /// 详细日志输出
        /// </summary>
        bool VerboseLog { get; set; }

        /// <summary>
        /// 通过资产路径加载并实例化出一个GameObject
        /// </summary>
        /// <param name="path">资产路径</param>
        /// <param name="parent"></param>
        GameObject InstantiatePrefab(string path, Transform parent = null);

        /// <summary>
        /// 通过资产路径异步加载并实例化出一个GameObject
        /// </summary>
        /// <param name="path">资产路径</param>
        /// <param name="parent"></param>
        /// <param name="callback">实例化完成的回调，回调将在主线程LateUpdate中触发</param>
        /// <returns>LoadingID，用于查询加载进度</returns>
        uint InstantiatePrefabAsync(string path, Transform parent, Action<ErrorCode, GameObject> callback);

        /// <summary>
        /// 克隆一个GameObject，为了更好的管理资源，LoadAsset返回的GO请务必使用此方法进行复制
        /// </summary>
        /// <param name="baseGo"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        GameObject Clone(GameObject baseGo, Transform parent = null);

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径，必须以'Assets/Artwork'开头</param>
        T LoadAsset<T>(string path) where T : Object;

        /// <summary>
        /// 异步加载资源，回调在主线程LateUpdate中触发
        /// </summary>
        uint LoadAssetAsync<T>(string path, Action<ErrorCode, T> callback) where T : Object;

        /// <summary>
        /// 卸载资源
        /// </summary>
        void UnloadAsset(Object asset);

        /// <summary>
        /// 根据LoadingId查询加载进度
        /// </summary>
        /// <param name="loadingId"></param>
        /// <returns>负数表示LoadingId无效，0-1表示进度</returns>
        float QueryProgress(uint loadingId);

        /// <summary>
        /// LateUpdate
        /// </summary>
        void UpdateFrame();
    }

    /// <summary>
    /// 资产管理器，编辑器默认不用AssetBundle，非编辑器强制使用AssetBundle
    /// </summary>
    public class AssetManager : MonoSingleton<AssetManager>, IAssetManager
    {
#if UNITY_EDITOR
        /// <summary>
        /// 编辑器中也使用AssetBundle方式加载，用于测试
        /// </summary>
        public static bool UseBundleInEditor;

        private EditorAssetManager _editorManager;
#endif
        private BundleManager _bundleManager;
        private IAssetManager _active;

        protected override void Init()
        {
            try
            {
#if UNITY_EDITOR
                if (UseBundleInEditor)
                {
                    _bundleManager = new BundleManager();
                    _active = _bundleManager;
                }
                else
                {
                    _editorManager = new EditorAssetManager();
                    _active = _editorManager;
                }
#else
                _bundleManager = new BundleManager();
                _active = _bundleManager;
#endif
            }
            catch (Exception e)
            {
                throw new AssetException(ErrorCode.InitFailed, null, e.ToString());
            }
        }

        private void LateUpdate()
        {
            _active?.UpdateFrame();
        }

        private void Check()
        {
            if (_active == null)
            {
                throw new AssetException(ErrorCode.NotInited);
            }
        }

        /// <inheritdoc />
        public bool VerboseLog
        {
            get
            {
                Check();
                return _active.VerboseLog;
            }
            set
            {
                Check();
                _active.VerboseLog = value;
            }
        }

        /// <inheritdoc />
        public GameObject InstantiatePrefab(string path, Transform parent = null)
        {
            Check();
            return _active.InstantiatePrefab(path, parent);
        }


        /// <inheritdoc />
        public uint InstantiatePrefabAsync(string path, Transform parent, Action<ErrorCode, GameObject> callback)
        {
            Check();
            return _active.InstantiatePrefabAsync(path, parent, callback);
        }

        /// <inheritdoc />
        public GameObject Clone(GameObject baseGo, Transform parent = null)
        {
            Check();
            return _active.Clone(baseGo, parent);
        }

        /// <inheritdoc />
        public T LoadAsset<T>(string path) where T : Object
        {
            Check();
            return _active.LoadAsset<T>(path);
        }


        /// <inheritdoc />
        public uint LoadAssetAsync<T>(string path, Action<ErrorCode, T> callback) where T : Object
        {
            Check();
            return _active.LoadAssetAsync(path, callback);
        }

        /// <inheritdoc />
        public void UnloadAsset(Object asset)
        {
            Check();
            _active.UnloadAsset(asset);
        }

        /// <inheritdoc />
        public float QueryProgress(uint loadingId)
        {
            Check();
            return _active.QueryProgress(loadingId);
        }

        [Obsolete("Don't call this", true)]
        public void UpdateFrame()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 同步加载Resources目录中的资源，应当尽量少使用
        /// </summary>
        public T LoadResource<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        /// <summary>
        /// 获取调试信息，包含每个资产引用了多少次<br/>
        /// 应当仅用于调试
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetDebugInfo()
        {
            if (_active == _bundleManager)
            {
                return _bundleManager.GetDebugInfo();
            }

            return new Dictionary<string, int>();
        }
    }
}
