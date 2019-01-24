/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: BaseView.cs
 * Created on 2018/05/01 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Instech.Framework
{
    /// <summary>
    /// 三种Update的类型定义
    /// </summary>
    public enum ViewUpdateType
    {
        /// <summary>
        /// 每帧调用
        /// </summary>
        Update = 0,

        /// <summary>
        /// 每固定间隔调用
        /// </summary>
        FixedUpdate = 1,

        /// <summary>
        /// 每帧结束前调用
        /// </summary>
        LateUpdate = 2
    }

    /// <summary>
    /// 用于执行三种Update的委托
    /// </summary>
    /// <param name="dt"></param>
    public delegate void ViewUpdator(float dt);

    /// <summary>
    /// 所有UI View的基类
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class BaseView : MonoBehaviour
    {
        public bool IsViewRemoved { get; private set; }

        /// <summary>
        /// 标识对象的唯一ID
        /// </summary>
        public uint Uid { get; private set; }

        /// <summary>
        /// 忽略关闭所有UI的操作
        /// </summary>
        public bool IgnoreCloseAll { get; set; }

        public bool IsSleeping { get; private set; }

        public bool IsActive => !IsSleeping;

        /// <summary>
        /// 所属CanvasName，如果不是Canvas的直属View该值可能为空
        /// </summary>
        public string CanvasName { get; internal set; }

        /// <summary>
        /// 深灰色UI遮罩
        /// </summary>
        [HideInInspector]
        public GameObject MaskGo;

        /// <summary>
        /// 是否禁止使用遮罩
        /// </summary>
        public bool NoMask;

        /// <summary>
        /// 该UI界面的RectTransform
        /// </summary>
        [HideInInspector]
        public RectTransform RectTransform;

        /// <summary>
        /// 对应的Presenter
        /// </summary>
        protected IBasePresenter Presenter;

        private readonly HashSet<EventDispatcher> _dispatchers = new HashSet<EventDispatcher>();
        private ViewUpdator _fixedUpdator;
        private ViewUpdator _lateUpdator;
        private ViewUpdator _updator;

        /// <summary>
        /// 添加UI事件监听
        /// </summary>
        /// <param name="widget">UI组件</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="selector">回调</param>
        public void AddEventListener(Component widget, string eventType, ListenerSelector selector)
        {
            var dispatcher = widget.gameObject.GetDispatcher();
            dispatcher.AddEventListener(eventType, selector);
            if (!_dispatchers.Contains(dispatcher))
            {
                _dispatchers.Add(dispatcher);
            }
        }

        /// <summary>
        /// 设置Update函数
        /// </summary>
        /// <param name="updator">需要执行的方法</param>
        /// <param name="type">类型</param>
        public void SetUpdator(ViewUpdator updator, ViewUpdateType type = ViewUpdateType.Update)
        {
            switch (type)
            {
                case ViewUpdateType.Update:
                    _updator = updator;
                    break;
                case ViewUpdateType.FixedUpdate:
                    _fixedUpdator = updator;
                    break;
                case ViewUpdateType.LateUpdate:
                    _lateUpdator = updator;
                    break;
                default:
                    throw new ArgumentException("无效类型");
            }
        }

        /// <summary>
        /// 回收UI
        /// </summary>
        public void Recycle()
        {
            if (IsSleeping)
            {
                return;
            }
            IsSleeping = true;
            Presenter.OnViewRecycle(false);
            UiManager.Instance.RecycleView(this);
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public void Close()
        {
            InternalClose(false);
        }

        /// <summary>
        /// 子类调用完Awake后必须调用这个方法
        /// </summary>
        protected void OnAwakeFinish()
        {
            Uid = Utility.GetUniqueId();
            IgnoreCloseAll = false;
            RectTransform = transform as RectTransform;
            Presenter.InitWithView(this);
            IsSleeping = false;
            Presenter.OnViewActivate();
        }

        protected abstract void Awake();

        /// <summary>
        /// 重新激活UI，UiManager调用
        /// </summary>
        internal void Activate()
        {
            if (!IsSleeping)
            {
                return;
            }
            IsSleeping = false;
            Presenter.OnViewActivate();
        }

        /// <summary>
        /// 由UI管理器调用，关闭所有UI
        /// </summary>
        internal void CloseAllUiOperation()
        {
            InternalClose(true);
        }

        private void InternalClose(bool callByUiManager)
        {
            if (IsViewRemoved)
            {
                // 防止重复调用
                return;
            }
            IsViewRemoved = true;
            _updator = null;
            _fixedUpdator = null;
            _lateUpdator = null;
            Presenter.OnViewRecycle(true);
            if (callByUiManager)
            {
                UiManager.Instance.CloseView(this);
            }
            Presenter.OnViewRemoved();
            if (_dispatchers != null)
            {
                foreach (var item in _dispatchers)
                {
                    item.RemoveAllEventListeners();
                }
                _dispatchers.Clear();
            }
            Destroy(gameObject);
            if (!NoMask)
            {
                Destroy(MaskGo);
            }
        }

        private void Start()
        {
            if (RectTransform == null || Presenter == null)
            {
                throw new InvalidOperationException("View未正确初始化: " + GetType());
            }
        }

        private void Update()
        {
            _updator?.Invoke(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _fixedUpdator?.Invoke(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            _lateUpdator?.Invoke(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (!IsViewRemoved)
            {
                // 意外的情况，补调一下
                Close();
            }
        }
    }

    /// <summary>
    /// View初始化失败会抛出该异常
    /// </summary>
    public class ViewInitException : Exception
    {
        public ViewInitException(BaseView view) : base("Init Ui View Failed: " + view)
        {
        }
    }
}
