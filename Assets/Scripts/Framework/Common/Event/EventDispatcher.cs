/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: EventDispatcher.cs
 * Created on 2018/05/03 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;

namespace Instech.Framework
{
    /// <summary>
    /// 事件监听回调委托
    /// </summary>
    /// <param name="e">事件数据</param>
    public delegate void ListenerSelector(Event e);

    /// <summary>
    /// 事件派发器
    /// </summary>
    public class EventDispatcher
    {
        private readonly Dictionary<string, List<ListenerSelector>> _dictListeners;
        private readonly object _target;

        /// <summary>
        /// 稍后要增加的监听
        /// </summary>
        private readonly List<KeyValuePair<string, ListenerSelector>> _listListenersToAdd;

        /// <summary>
        /// 正在派发中的事件层数
        /// </summary>
        private int _dispatching;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="target">发送者，即该Dispatcher所属对象</param>
        public EventDispatcher(object target)
        {
            _dictListeners = new Dictionary<string, List<ListenerSelector>>();
            _listListenersToAdd = new List<KeyValuePair<string, ListenerSelector>>();
            _dispatching = 0;
            _target = target;
        }

        ~EventDispatcher()
        {
            RemoveAllEventListeners();
        }

        /// <summary>
        /// 添加一个监听
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="sel">需要添加的监听</param>
        /// <returns>是否添加成功</returns>
        public bool AddEventListener(string eventType, ListenerSelector sel)
        {
            if (eventType == "" || sel == null)
            {
                // 判断有效性
                return false;
            }
            if (HasEventListener(eventType, sel))
            {
                Logger.LogWarning(LogModule.Framework, $"重复监听！type={eventType}");
            }
            if (!_dictListeners.ContainsKey(eventType))
            {
                // 不存在的话就新建一个
                _dictListeners[eventType] = new List<ListenerSelector>();
            }
            if (_dispatching > 0)
            {
                // 正在派发事件，等派发完成后再添加
                _listListenersToAdd.Add(new KeyValuePair<string, ListenerSelector>(eventType, sel));
                return true;
            }
            var selectors = _dictListeners[eventType];
            selectors.Add(sel);
            return true;
        }

        /// <summary>
        /// 检查是否已经添加了某事件的某监听
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="sel">需要检查的监听</param>
        /// <returns>是否已经添加</returns>
        public bool HasEventListener(string eventType, ListenerSelector sel)
        {
            if (!_dictListeners.ContainsKey(eventType))
            {
                return false;
            }
            var selectors = _dictListeners[eventType];
            var target = selectors.Find(src => sel == src);
            return target != null;
        }

        /// <summary>
        /// 移除指定监听
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="sel">要移除的监听</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveEventListener(string eventType, ListenerSelector sel)
        {
            if (!HasEventListener(eventType, sel))
            {
                return false;
            }
            var selectors = _dictListeners[eventType];
            foreach (var item in selectors)
            {
                if (item == sel)
                {
                    selectors.Remove(item);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 移除来自某对象的所有监听
        /// </summary>
        /// <param name="target">Target.</param>
        public void RemoveAllEventListenersWithTarget(object target)
        {
            foreach (var pair in _dictListeners)
            {
                var selectors = pair.Value;
                for (var i = selectors.Count - 1; i >= 0; --i)
                {
                    var item = selectors[i];
                    if (item.Target.GetHashCode() == target.GetHashCode())
                    {
                        selectors.Remove(item);
                    }
                }
            }
        }

        /// <summary>
        /// 删除指定事件的所有监听
        /// </summary>
        /// <param name="eventType">不为空时删除指定事件的监听，为空时删除所有监听。默认为空</param>
        /// <returns>删除是否成功</returns>
        public bool RemoveAllEventListeners(string eventType = "")
        {
            if (eventType == "")
            {
                // 删除所有
                _dictListeners.Clear();
                return true;
            }

            // 删除指定事件的
            if (_dictListeners.ContainsKey(eventType))
            {
                var selectors = _dictListeners[eventType];
                selectors.Clear();
            }
            return false;
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="data">事件数据</param>
        /// <returns>通知的订阅者数量</returns>
        public int DispatchEvent(string eventType, IEventData data = null)
        {
            var e = new Event(eventType, _target, data);
            return DispatchEvent(e);
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="e">事件</param>
        /// <returns>通知的订阅者数量</returns>
        public int DispatchEvent(Event e)
        {
            _dispatching += 1;
            var count = 0;
            if (_dictListeners.ContainsKey(e.EventType))
            {
                var selectors = _dictListeners[e.EventType];
                foreach (var item in selectors)
                {
                    try
                    {
                        item(e);
                    }
                    catch (Exception exc)
                    {
                        Logger.LogError(LogModule.Framework, "派发事件时出错");
                        Logger.LogException(LogModule.Framework, exc);
                    }
                    count += 1;
                }
            }
            _dispatching -= 1;
            if (_dispatching == 0 && _listListenersToAdd.Count > 0)
            {
                foreach (var sel in _listListenersToAdd)
                {
                    AddEventListener(sel.Key, sel.Value);
                }
                _listListenersToAdd.Clear();
            }
            return count;
        }
    }
}
