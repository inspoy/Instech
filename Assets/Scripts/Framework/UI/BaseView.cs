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
    /// 所有UI View的基类
    /// </summary>
    public class BaseView : MonoBehaviour
    {
        /// <summary>
        /// 用于执行三种Update的委托
        /// </summary>
        /// <param name="dt"></param>
        public delegate void ViewUpdator(float dt);

        public bool IsViewRemoved { get; } = false;

        /// <summary>
        /// 深灰色UI遮罩
        /// </summary>
        public GameObject MaskGo = null;

        /// <summary>
        /// 是否禁止使用遮罩
        /// </summary>
        public bool NoMask = false;

        /// <summary>
        /// 该UI界面的RectTransform
        /// </summary>
        public RectTransform RectTransform;

        /// <summary>
        /// 对应的Presenter
        /// </summary>
        protected IBasePresenter Presenter;

        private readonly List<ListenerSelector> _selectors = new List<ListenerSelector>();
        private ViewUpdator _fixedUpdator = null;
        private ViewUpdator _lateUpdator = null;
        private ViewUpdator _updator = null;

        /// <summary>
        /// 添加UI事件监听
        /// </summary>
        /// <param name="widget">UI组件</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="selector">回调</param>
        public void AddEventListener(Component widget, string eventType, ListenerSelector selector)
        {
            // TODO: 添加监听
        }
        
        // TODO: 其他成员方法
    }
}
