// == Inspoy Technology ==
// Assembly: Instech.Framework.Common
// FileName: Event.cs
// Created on 2018/05/03 by inspoy
// All rights reserved.

using Instech.Framework.Core;
using Instech.Framework.Logging;

namespace Instech.Framework.Common
{
    /// <summary>
    /// 事件
    /// </summary>
    public class Event : IPoolable
    {
        /// <summary>
        /// 事件包含的自定义数据
        /// </summary>
        private IEventData _data;

        /// <summary>
        /// 事件类型
        /// </summary>
        public string EventType { get; private set; }

        /// <summary>
        /// 事件发送者
        /// </summary>
        public object Target { get; private set; }

        public T GetData<T>() where T : class, IEventData
        {
#if UNITY_EDITOR
            if (_data == null)
            {
                Logger.LogWarning(LogModule.Framework, $"Event has no EventData");
                return default;
            }
            var ret = _data as T;
            if (ret == null)
            {
                Logger.LogWarning(LogModule.Framework, $"EventData type mismatch, real: {_data.GetType().FullName}, expected: {typeof(T).FullName}");
                return default;
            }
            return ret;
#else
            return _data as T;
#endif
        }

        public void OnRecycle()
        {
            EventType = string.Empty;
            Target = null;
            _data?.RecycleData();
            _data = null;
        }

        public void OnActivate()
        {
            // do nothing
        }

        public void OnDestroy()
        {
            // do nothing
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="eventType">类型</param>
        /// <param name="target">发送者</param>
        /// <param name="data">[可选]附加数据</param>
        public void Init(string eventType, object target, IEventData data = null)
        {
            EventType = eventType;
            Target = target;
            _data = data;
        }

        public static Event GetNew()
        {
            return ObjectPool<Event>.Instance.Get();
        }
    }

    /// <summary>
    /// 事件类型定义
    /// 这里声明为partial，可自行扩展此类
    /// </summary>
    // ReSharper disable once PartialTypeWithSinglePart
    public static partial class EventEnum
    {
        /// <summary>
        /// UI鼠标点击
        /// </summary>
        public const string UiPointerClick = "Event_UiPointerClick";

        /// <summary>
        /// UI鼠标按下
        /// </summary>
        public const string UiPointerDown = "Event_UiPointerDown";

        /// <summary>
        /// UI鼠标抬起
        /// </summary>
        public const string UiPointerUp = "Event_UiPointerUp";

        /// <summary>
        /// UI鼠标移入
        /// </summary>
        public const string UiPointerEnter = "Event_UiPointerEnter";

        /// <summary>
        /// UI鼠标移出
        /// </summary>
        public const string UiPointerExit = "Event_UiPointerExit";

        /// <summary>
        /// UI鼠标悬停
        /// </summary>
        public const string UiPointerHover = "Event_UiPointerHover";

        /// <summary>
        /// UI选择
        /// </summary>
        public const string UiSelect = "Event_UiSelect";

        /// <summary>
        /// UI更新选择
        /// </summary>
        public const string UiUpdateSelected = "Event_UiUpdateSelected";

        /// <summary>
        /// UI提交
        /// </summary>
        public const string UiSubmit = "Event_UiSubmit";

        /// <summary>
        /// UI开始拖动
        /// </summary>
        public const string UiBeginDrag = "Event_UiBeginDrag";

        /// <summary>
        /// UI拖动中
        /// </summary>
        public const string UiDrag = "Event_UiDrag";

        /// <summary>
        /// UI结束拖动
        /// </summary>
        public const string UiEndDrag = "Event_UiEndDrag";

        /// <summary>
        /// UI值改变
        /// </summary>
        public const string UiValueChange = "Event_UiValueChange";

        /// <summary>
        /// UIToggleGroup更改选项
        /// </summary>
        public const string UiToggleChange = "Event_UiToggleChange";

        /// <summary>
        /// 语言设置发生变化
        /// </summary>
        public const string LanguageChange = "Event_LanguageChange";
    }
}
