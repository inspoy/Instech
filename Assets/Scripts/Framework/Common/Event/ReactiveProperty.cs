/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ReactiveProperty.cs
 * Created on 2018/08/06 by inspoy
 * All rights reserved.
 */

using System;

namespace Instech.Framework
{
    public static partial class EventEnum
    {
        /// <summary>
        /// 响应式属性值变化
        /// </summary>
        public const string ReactivePropChange = "Event_ReactivePropChange";
    }

    public class ReactivePropertyChanged<T> : IEventData
    {
        public T Val { get; set; }

        public void OnRecycle()
        {
            // do nothing
        }

        public void OnActivate()
        {
            // do nothing
        }

        public void Dispose()
        {
            // do nothing
        }

        public void RecycleData()
        {
            this.Recycle();
        }

        public static ReactivePropertyChanged<T> GetNew(T val)
        {
            var ret = ObjectPool<ReactivePropertyChanged<T>>.Instance.Get();
            ret.Val = val;
            return ret;
        }
    }

    public class ReactiveProperty<T> where T : IEquatable<T>
    {
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (AlwaysTrigger || !value.Equals(_value))
                {
                    Dispatcher.DispatchEvent(EventEnum.ReactivePropChange, ReactivePropertyChanged<T>.GetNew(_value));
                }
                _value = value;
            }
        }

        /// <summary>
        /// 始终触发，若为false，赋一个一样的值则不会触发事件，默认为true
        /// </summary>
        public bool AlwaysTrigger { get; set; } = true;

        public EventDispatcher Dispatcher { get; }

        public ReactiveProperty()
        {
            Dispatcher = new EventDispatcher(this);
        }

        public void AddEventListener(ListenerSelector sel)
        {
            Dispatcher.AddEventListener(EventEnum.ReactivePropChange, sel);
        }

        public void RemoveEventListener(ListenerSelector sel)
        {
            Dispatcher.RemoveEventListener(EventEnum.ReactivePropChange, sel);
        }
    }
}
