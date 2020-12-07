// == Inspoy Technology ==
// Assembly: Instech.Framework.Common
// FileName: ReactiveProperty.cs
// Created on 2018/08/06 by inspoy
// All rights reserved.

using System;

namespace Instech.Framework.Common
{
    public static partial class EventEnum
    {
        /// <summary>
        /// 响应式属性值变化
        /// </summary>
        public const string ReactivePropChange = "Event_ReactivePropChange";
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
                    Dispatcher.DispatchEvent(EventEnum.ReactivePropChange, ValueChangeData<T>.GetNewOne(value, _value));
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

        public ReactiveProperty(T value)
        {
            Dispatcher = new EventDispatcher(this);
            _value = value;
        }

        public static implicit operator T(ReactiveProperty<T> rp) => rp.Value;

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
