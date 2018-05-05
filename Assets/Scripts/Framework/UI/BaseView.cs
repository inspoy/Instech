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
    public abstract class BaseView : MonoBehaviour
    {
        public bool IsViewRemoved { get; private set; }

        /// <summary>
        /// 深灰色UI遮罩
        /// </summary>
        [HideInInspector] public GameObject MaskGo;

        /// <summary>
        /// 是否禁止使用遮罩
        /// </summary>
        public bool NoMask;

        /// <summary>
        /// 该UI界面的RectTransform
        /// </summary>
        [HideInInspector] public RectTransform RectTransform;

        /// <summary>
        /// 对应的Presenter
        /// </summary>
        protected IBasePresenter Presenter;

        private readonly HashSet<EventDispatcher> _dispatchers = new HashSet<EventDispatcher>();
        private ViewUpdator _fixedUpdator;
        private ViewUpdator _lateUpdator;
        private ViewUpdator _updator;
        private bool _isHide;

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
        /// 隐藏UI（暂不关闭）
        /// </summary>
        public void Hide()
        {
            if (_isHide)
            {
                return;
            }
            _isHide = true;
            RectTransform.Translate(9999, 0, 0);
            Presenter.OnViewHide();
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        public void Show()
        {
            if (!_isHide)
            {
                _isHide = true;
            }
            RectTransform.Translate(-9999, 0, 0);
            Presenter.OnViewShow();
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public void Close()
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
            Presenter.OnViewHide();
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

        /// <summary>
        /// 子类调用完Awake后必须调用这个方法
        /// </summary>
        protected void OnAwakeFinish()
        {
            RectTransform = transform as RectTransform;
            Presenter.InitWithView(this);
            _isHide = false;
            Presenter.OnViewShow();
        }

        protected abstract void Awake();

        private void Start()
        {
            if (RectTransform == null || Presenter == null)
            {
                throw new Exception("View未正确初始化: " + GetType());
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
}
