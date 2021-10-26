// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui
// FileName: UiManager.cs
// Created on 2018/05/07 by inspoy
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Instech.Framework.AssetHelper;
using Instech.Framework.Core;
using Instech.Framework.Logging;
using Instech.Framework.Ui.Tweening;
using Instech.Framework.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Ui
{
    /// <summary>
    /// 单个View缓存数据结构，每个View一个
    /// </summary>
    internal class UiCacheData
    {
        /// <summary>
        /// prefab路径，形如Prefabs/UI/vwTest
        /// </summary>
        public string PrefabPath;

        public readonly List<BaseView> CachedViews = new List<BaseView>();
        public readonly List<BaseView> ActiveViews = new List<BaseView>();

        public BaseView GetView(Transform parent, IUiInitData initData, Type viewType, BaseView cloneFrom = null)
        {
            BaseView ret;
            if (CachedViews.Count > 0)
            {
                ret = CachedViews[CachedViews.Count - 1];
                CachedViews.Remove(ret);
                ActiveViews.Add(ret);
                ret.transform.SetParent(parent);
                ret.Activate(initData);
            }
            else
            {
                GameObject go;
                go = cloneFrom == null
                    ? AssetManager.Instance.InstantiatePrefab(PrefabPath, parent)
                    : AssetManager.Instance.Clone(cloneFrom.gameObject, parent);
                go.transform.localPosition = Vector3.zero;
                ret = go.GetComponent(viewType) as BaseView;
                if (ret == null)
                {
                    Logger.LogError(LogModule.Ui, "Prefab未挂载View组件: " + viewType);
                    return null;
                }
                ret.Activate(initData);

                ActiveViews.Add(ret);
            }
            return ret;
        }
    }

    /// <summary>
    /// UI激活时可以传入的一些初始化数据
    /// </summary>
    public interface IUiInitData
    {
    }

    /// <summary>
    /// UI管理器，提供UI增删改查功能
    /// </summary>
    public class UiManager : MonoSingleton<UiManager>
    {
        public Camera UiCamera { get; private set; }
        
        #region Fields

        private readonly Dictionary<Type, UiCacheData> _cacheData = new Dictionary<Type, UiCacheData>();
        private Transform _sleepingViews;
        private readonly Dictionary<string, Canvas> _dictCanvases = new Dictionary<string, Canvas>();
        private readonly List<GraphicRaycaster> _raycasters = new List<GraphicRaycaster>();
        private PointerEventData _pointerEventData;
        private readonly List<RaycastResult> _mouseCastList = new List<RaycastResult>();

        #endregion

        #region Canvas

        /// <summary>
        /// 从低到高添加Canvas
        /// </summary>
        /// <param name="canvasName"></param>
        public Canvas AddCanvas(string canvasName)
        {
            if (_dictCanvases.ContainsKey(canvasName)) return null;
            var canvasGo = gameObject.AddEmptyChild(canvasName + "Canvas");
            canvasGo.layer = 5; // UI Layer
            var canvas = canvasGo.AddComponent<Canvas>();
            _dictCanvases.Add(canvasName, canvas);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = UiCamera;
            canvas.sortingOrder = _dictCanvases.Count;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            _raycasters.Add(canvasGo.AddComponent<GraphicRaycaster>());
            return canvas;
        }

        /// <summary>
        /// 根据名字拿到对应的Canvas
        /// </summary>
        /// <param name="canvasName"></param>
        /// <returns></returns>
        public Canvas GetCanvas(string canvasName)
        {
            if (string.IsNullOrEmpty(canvasName))
            {
                return null;
            }

            _dictCanvases.TryGetValue(canvasName, out var ret);
            return ret;
        }

        #endregion

        #region Properties && Interfaces

        public Tweener Tweener { get; private set; }
        
        /// <summary>
        /// 创建一个UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="canvasName">Canvas名称（可选）</param>
        /// <param name="parent">父节点（可选）</param>
        /// <param name="initData">初始化数据（可选）</param>
        /// <returns></returns>
        public T AddView<T>(string canvasName = "Normal", Transform parent = null, IUiInitData initData = null) where T : BaseView
        {
            var cacheData = GetCacheData(typeof(T));
            if (parent == null)
            {
                parent = GetCanvas(canvasName)?.transform;
            }
            if (parent == null)
            {
                Logger.LogError(LogModule.Ui, "Cannot resolve canvas: " + canvasName);
                return null;
            }

            var ret = cacheData.GetView(parent, initData, typeof(T));
            ret.CanvasName = canvasName;
            return ret as T;
        }

        /// <summary>
        /// 克隆现有UI
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="parent">父节点（可选，默认和source同父节点）</param>
        /// <param name="initData">初始化数据（可选）</param>
        /// <returns></returns>
        public BaseView CloneView(BaseView source, Transform parent = null, IUiInitData initData = null)
        {
            Logger.Assert(LogModule.Ui, source != null, "source不可为空");
            var viewType = source.GetType();
            var cacheData = GetCacheData(viewType);
            if (parent == null)
            {
                parent = source.RectTransform.parent;
            }
            var ret = cacheData.GetView(parent, initData, viewType, source);
            ret.CanvasName = source.CanvasName;
            return ret;
        }

        /// <summary>
        /// 获取指定类型的UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uid">可以指定uid，没指定或者没找到的话返回第一个 </param>
        /// <returns></returns>
        public T GetView<T>(uint uid = 0) where T : BaseView
        {
            var cacheData = GetCacheData(typeof(T));
            if (cacheData.ActiveViews.Count == 0)
            {
                return null;
            }

            foreach (var view in cacheData.ActiveViews)
            {
                if ((uid == 0 || view.Uid == uid) && view is T result)
                {
                    return result;
                }
            }

            return cacheData.ActiveViews[0] as T;
        }

        public T AddItemToView<T>(RectTransform parent, BaseView parentView, IUiInitData initData = null) where T : BaseView
        {
            Logger.Assert(LogModule.Ui, parent != null, "parent不可为空");
            Logger.Assert(LogModule.Ui, parentView != null, "parentView不可为空");
            // 在父UI上添加子UI的记录，回收或关闭时需要一起处理
            var ret = AddView<T>(null, parent, initData);
            parentView.SubViews.Add(ret);
            ret.ParentView = parentView;
            return ret;
        }

        /// <summary>
        /// 关闭所有UI，一般用于切换场景<br/>
        /// 细节：忽略设置了无视CloseAll的UI，关闭所有ActiveViews且不缓存
        /// </summary>
        public void CloseAllViews()
        {
            foreach (var cacheData in _cacheData.Values)
            {
                var delList = new List<BaseView>();
                foreach (var item in cacheData.ActiveViews)
                {
                    if (!item.IgnoreCloseAll)
                    {
                        delList.Add(item);
                    }
                }

                foreach (var item in cacheData.CachedViews)
                {
                    if (!item.IgnoreCloseAll)
                    {
                        delList.Add(item);
                    }
                }

                foreach (var item in delList)
                {
                    item.Close();
                }
            }
        }

        #endregion

        #region Internal

        protected override void Init()
        {
            transform.position = new Vector3(0, 100, 0);
            gameObject.layer = 5; // UI Layer
            
            var eventSystemGo = gameObject.AddEmptyChild("EventSystem");
            var eventSystem = eventSystemGo.AddComponent<EventSystem>();
            _pointerEventData = new PointerEventData(eventSystem);
            eventSystemGo.AddComponent<StandaloneInputModule>();

            var cameraGo = gameObject.AddEmptyChild("UiCamera");
            UiCamera = cameraGo.AddComponent<Camera>();
            var additionalData = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            additionalData.renderType = CameraRenderType.Overlay;
            UiCamera.orthographic = true;
            UiCamera.cullingMask = LayerMask.GetMask("UI");

            _sleepingViews = AddCanvas("Sleeping").transform;
            _sleepingViews.gameObject.SetActive(false);

            Tweener = new Tweener();
        }

        private void Update()
        {
            Tweener.UpdateFrame(Time.deltaTime);
        }

        /// <summary>
        /// 由BaseView调用，执行回收操作
        /// </summary>
        /// <param name="view"></param>
        internal void RecycleViewFromBaseView(BaseView view)
        {
            var cacheData = GetCacheData(view.GetType());
            cacheData.CachedViews.Add(view);
            cacheData.ActiveViews.Remove(view);
            view.transform.SetParent(_sleepingViews);
        }

        /// <summary>
        /// 由BaseView调用，执行关闭操作<br/>
        /// </summary>
        /// <param name="view"></param>
        internal void CloseViewFromBaseView(BaseView view)
        {
            var cacheData = GetCacheData(view.GetType());
            if (view.IsActive)
            {
                cacheData.ActiveViews.Remove(view);
            }
            else
            {
                cacheData.CachedViews.Remove(view);
            }

            AssetManager.Instance.UnloadAsset(view.gameObject);
            Destroy(view.gameObject);
        }

        /// <summary>
        /// 根据View类型获取UI缓存对象
        /// </summary>
        /// <param name="t">要查找的类型</param>
        /// <returns></returns>
        private UiCacheData GetCacheData(Type t)
        {
            if (_cacheData.TryGetValue(t, out var cacheData))
            {
                return cacheData;
            }

            cacheData = new UiCacheData();
            var typeStr = t.ToString();
            var dotPos = typeStr.LastIndexOf('.');
            cacheData.PrefabPath =
                "Assets/Artwork/Prefabs/UI/vw" +
                typeStr.Substring(dotPos + 1, typeStr.Length - dotPos - 5) +
                ".prefab";
            _cacheData.Add(t, cacheData);
            return cacheData;
        }

        #endregion

        #region Utils

        /// <summary>
        /// 检测鼠标是否在任何UI组件上
        /// </summary>
        /// <param name="mouseX"></param>
        /// <param name="mouseY"></param>
        /// <returns></returns>
        public bool IsMouseOnUi(float mouseX, float mouseY)
        {
            _pointerEventData.position = new Vector2(mouseX, mouseY);
            _pointerEventData.pressPosition = new Vector2(mouseX, mouseY);
            foreach (var item in _raycasters)
            {
                _mouseCastList.Clear();
                item.Raycast(_pointerEventData, _mouseCastList);
                if (_mouseCastList.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获得缓存信息的调试字符串
        /// </summary>
        /// <returns></returns>
        public string GetCachedDebugString()
        {
            var ret = new StringBuilder();
            foreach (var item in _cacheData)
            {
                var data = item.Value;
                ret.Append($"{item.Key.Name},{data.ActiveViews.Count},{data.CachedViews.Count}\n");
            }

            return ret.ToString();
        }

        #endregion
    }
}
