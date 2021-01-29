// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui
// FileName: BaseView.cs
// Created on 2018/05/01 by inspoy
// All rights reserved.

using System;
using System.Collections.Generic;
using Instech.Framework.Common;
using Instech.Framework.Utils;
using UnityEngine;

namespace Instech.Framework.Ui
{
    /// <summary>
    /// 三种Update的类型定义
    /// </summary>
    public enum ViewUpdateType
    {
        /// <summary>
        /// 每帧调用
        /// </summary>
        Update,

        /// <summary>
        /// 每固定间隔调用
        /// </summary>
        FixedUpdate,

        /// <summary>
        /// 每帧结束前调用
        /// </summary>
        LateUpdate,

        /// <summary>
        /// 每秒调用一次，如果卡顿时间超过2秒，则下一次的deltaTime为实际等待的时间
        /// </summary>
        SecondUpdate
    }

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
        /// 该UI界面的RectTransform
        /// </summary>
        [HideInInspector]
        public RectTransform RectTransform;

        internal BaseView ParentView;
        internal List<BaseView> SubViews;

        /// <summary>
        /// 对应的Presenter
        /// </summary>
        protected IBasePresenter Presenter;

        private readonly HashSet<EventDispatcher> _dispatchers = new HashSet<EventDispatcher>();
        public IUiInitData InitData { get; private set; }

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
        public void SetUpdator(Scheduler.UpdateHandler updator, ViewUpdateType type = ViewUpdateType.Update)
        {
            if (!Scheduler.HasSingleton())
            {
                return;
            }

            switch (type)
            {
                case ViewUpdateType.Update:
                    if (updator != null)
                    {
                        Scheduler.RegisterFrameUpdate(this, updator);
                    }
                    else
                    {
                        Scheduler.UnregisterFrameUpdate(this);
                    }

                    break;
                case ViewUpdateType.FixedUpdate:
                    if (updator != null)
                    {
                        Scheduler.RegisterLogicUpdate(this, updator);
                    }
                    else
                    {
                        Scheduler.UnregisterLogicUpdate(this);
                    }

                    break;
                case ViewUpdateType.LateUpdate:
                    if (updator != null)
                    {
                        Scheduler.RegisterLateUpdate(this, updator);
                    }
                    else
                    {
                        Scheduler.UnregisterLateUpdate(this);
                    }

                    break;
                case ViewUpdateType.SecondUpdate:
                    if (updator != null)
                    {
                        Scheduler.RegisterSecondUpdate(this, updator);
                    }
                    else
                    {
                        Scheduler.UnregisterSecondUpdate(this);
                    }

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
            InitData = null;
            if (UiManager.HasSingleton())
            {
                UiManager.Instance.RecycleViewFromBaseView(this);
            }

            if (ParentView != null)
            {
                ParentView.SubViews.Remove(this);
            }

            foreach (var subView in SubViews)
            {
                subView.ParentView = null;
                subView.Recycle();
            }

            SubViews.Clear();
        }

        /// <summary>
        /// 子类调用完Awake后必须调用这个方法
        /// </summary>
        protected void OnAwakeFinish()
        {
            Uid = Utility.GetUniqueId();
            SubViews = new List<BaseView>();
            IgnoreCloseAll = false;
            RectTransform = transform as RectTransform;
            Presenter.InitWithView(this);
            IsSleeping = true;
        }

        protected abstract void Awake();

        /// <summary>
        /// 重新激活UI，UiManager调用
        /// </summary>
        internal void Activate(IUiInitData initData)
        {
            if (!IsSleeping)
            {
                return;
            }

            IsSleeping = false;
            InitData = initData;
            Presenter.OnViewActivate();
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
            if (Scheduler.HasSingleton())
            {
                Scheduler.UnregisterFrameUpdate(this);
                Scheduler.UnregisterSecondUpdate(this);
                Scheduler.UnregisterLogicUpdate(this);
                Scheduler.UnregisterLateUpdate(this);
            }

            Presenter.OnViewRecycle(true);
            if (UiManager.HasSingleton())
            {
                UiManager.Instance.CloseViewFromBaseView(this);
            }


            Presenter.OnDestroyed();
            if (_dispatchers != null)
            {
                foreach (var item in _dispatchers)
                {
                    item.RemoveAllEventListeners();
                }

                _dispatchers.Clear();
            }

            if (ParentView != null)
            {
                ParentView.SubViews.Remove(this);
            }

            foreach (var subView in SubViews)
            {
                subView.ParentView = null;
                subView.Recycle();
            }

            SubViews.Clear();
        }

        private void Start()
        {
            if (RectTransform == null || Presenter == null)
            {
                throw new InvalidOperationException("View未正确初始化: " + GetType());
            }
        }

        private void OnDestroy()
        {
            if (!IsViewRemoved && UiManager.HasSingleton())
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
