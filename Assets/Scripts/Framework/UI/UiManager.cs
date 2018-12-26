/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: UiManager.cs
 * Created on 2018/05/07 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Instech.Framework
{
    /// <summary>
    /// View缓存数据结构
    /// </summary>
    internal class UiCacheData
    {
        /// <summary>
        /// prefab路径，形如Prefabs/UI/vwTest
        /// </summary>
        public string PrefabPath;

        public Queue<BaseView> CachedViews = new Queue<BaseView>();
        public List<BaseView> ActiveViews = new List<BaseView>();
    }

    /// <summary>
    /// UI管理器，提供UI增删改查功能
    /// </summary>
    public class UiManager : MonoSingleton<UiManager>
    {
        // TODO: 分为多个canvas，根据配置表读取具体需要几个canvas，sleepingViews这个canvas固定存在
        public Canvas Canvas { get; private set; }
        public Camera UiCamera { get; private set; }
        private readonly Dictionary<Type, UiCacheData> _cacheData = new Dictionary<Type, UiCacheData>();
        private Transform _sleepingViews;
        private GraphicRaycaster _raycaster;
        private PointerEventData _pointerEventData;
        private readonly List<RaycastResult> _mouseCastList = new List<RaycastResult>();

        /// <summary>
        /// 创建一个UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent">父节点（可选）</param>
        /// <returns></returns>
        public T AddView<T>(Transform parent = null) where T : BaseView
        {
            var cacheData = GetCacheData(typeof(T));
            if (parent == null)
            {
                parent = transform;
            }
            BaseView ret;
            if (cacheData.CachedViews.Count > 0)
            {
                ret = cacheData.CachedViews.Dequeue();
                cacheData.ActiveViews.Add(ret);
                ret.transform.SetParent(parent);
                ret.Activate();
            }
            else
            {
                var prefab = AssetBundleManager.Instance.LoadAsset<GameObject>(cacheData.PrefabPath);
                var go = Instantiate(prefab, parent);
                AssetBundleManager.Instance.AddRecord(go, cacheData.PrefabPath);
                go.transform.localPosition = Vector3.zero;
                ret = go.GetComponent<T>();
                if (ret == null)
                {
                    Logger.LogError(LogModule.Ui, "Prefab未挂载View组件: " + typeof(T));
                    return null;
                }
                cacheData.ActiveViews.Add(ret);
            }
            return ret as T;
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
                if (view.Uid == uid && view is T result)
                {
                    return result;
                }
            }
            return cacheData.ActiveViews[0] as T;
        }

        /// <summary>
        /// 关闭所有UI，一般用于切换场景
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
                foreach (var item in delList)
                {
                    item.Close();
                }
            }
        }

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
            _mouseCastList.Clear();
            _raycaster.Raycast(_pointerEventData, _mouseCastList);
            return _mouseCastList.Count > 0;
        }

        protected override void Init()
        {
            gameObject.layer = 5; // UI Layer
            var eventSystemGo = gameObject.AddEmptyChild("EventSystem");
            var eventSystem = eventSystemGo.AddComponent<EventSystem>();
            _pointerEventData = new PointerEventData(eventSystem);
            eventSystemGo.AddComponent<StandaloneInputModule>();
            UiCamera = gameObject.AddEmptyChild("UICamera").AddComponent<Camera>();
            UiCamera.transform.localPosition = Vector3.back * 100;
            UiCamera.cullingMask = 1 << 5; // Only UI
            UiCamera.clearFlags = CameraClearFlags.Nothing;
            UiCamera.orthographic = true;
            Canvas = gameObject.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = UiCamera;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
            _raycaster = gameObject.AddComponent<GraphicRaycaster>();
            var sleepingViewsGo = gameObject.AddEmptyChild("SleepingViews");
            sleepingViewsGo.SetActive(false);
            _sleepingViews = sleepingViewsGo.transform;
        }

        /// <summary>
        /// 由BaseView调用，执行回收操作
        /// </summary>
        /// <param name="view"></param>
        internal void RecycleView(BaseView view)
        {
            var cacheData = GetCacheData(view.GetType());
            cacheData.CachedViews.Enqueue(view);
            cacheData.ActiveViews.Remove(view);
            view.transform.SetParent(_sleepingViews);
        }

        /// <summary>
        /// 由BaseView调用，执行关闭操作
        /// </summary>
        /// <param name="view"></param>
        internal void CloseView(BaseView view)
        {
            var cacheData = GetCacheData(view.GetType());
            cacheData.ActiveViews.Remove(view);
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
            cacheData.PrefabPath = "Prefabs/UI/vw" + typeStr.Substring(dotPos + 1, typeStr.Length - dotPos - 5) + ".prefab";
            _cacheData.Add(t, cacheData);
            return cacheData;
        }
    }
}
