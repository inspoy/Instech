// == Inspoy Technology ==
// Assembly: Instech.Framework.AssetHelper
// FileName: EditorManager.cs
// Created on 2019/12/17 by inspoy
// All rights reserved.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Instech.Framework.AssetHelper
{
    internal class EditorAssetManager : IAssetManager
    {
        private readonly Dictionary<Object, System.Action<Object>> _pendingAssets;
        private readonly List<Action> _invalidAssetRequest;

        public EditorAssetManager()
        {
            _pendingAssets = new Dictionary<Object, Action<Object>>();
            _invalidAssetRequest = new List<Action>();
        }

        /// <inheritdoc />
        public bool VerboseLog { get; set; }

        /// <inheritdoc />
        public GameObject InstantiatePrefab(string path, Transform parent = null)
        {
            var prefab = LoadAsset<GameObject>(path);
            return Object.Instantiate(prefab, parent);
        }

        /// <inheritdoc />
        public uint InstantiatePrefabAsync(string path, Transform parent, Action<ErrorCode, GameObject> callback)
        {
            return LoadAssetAsync<GameObject>(path, (code, o) => { callback(code, Object.Instantiate(o, parent)); });
        }

        /// <inheritdoc />
        public GameObject Clone(GameObject baseGo, Transform parent = null)
        {
            return Object.Instantiate(baseGo, parent);
        }

        /// <inheritdoc />
        public T LoadAsset<T>(string path) where T : Object
        {
            var ret = AssetDatabase.LoadAssetAtPath<T>(path);
            if (ret == null)
            {
                throw new AssetException(ErrorCode.LoadFailed, path);
            }
            return ret;
        }

        /// <inheritdoc />
        public uint LoadAssetAsync<T>(string path, Action<ErrorCode, T> callback) where T : Object
        {
            var ret = AssetDatabase.LoadAssetAtPath<T>(path);
            if (ret != null)
            {
                _pendingAssets.Add(ret, o => callback(ErrorCode.Success, o as T));
            }
            else
            {
                _invalidAssetRequest.Add(() => callback(ErrorCode.LoadFailed, null));
            }

            return 0;
        }

        /// <inheritdoc />
        public void UnloadAsset(Object asset)
        {
            // do nothing
        }

        /// <inheritdoc />
        public float QueryProgress(uint loadingId)
        {
            return 1;
        }

        /// <inheritdoc />
        public void UpdateFrame()
        {
            if (_pendingAssets.Count > 0)
            {
                foreach (var item in _pendingAssets)
                {
                    item.Value(item.Key);
                }

                _pendingAssets.Clear();
            }

            if (_invalidAssetRequest.Count > 0)
            {
                foreach (var item in _invalidAssetRequest)
                {
                    item();
                }

                _invalidAssetRequest.Clear();
            }
        }
    }
}
#endif
