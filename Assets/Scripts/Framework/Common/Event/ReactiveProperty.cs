/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ReactiveProperty.cs
 * Created on 2018/08/06 by duoyi
 * All rights reserved.
 */

namespace Instech.Framework
{
    public class ReactivePropertyChanged<T> : IEventData
    {
        public T Val;
        public ReactivePropertyChanged(T val)
        {
            Val = val;
        }
    }

    public class ReactiveProperty<T>
    {
        private T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                Dispatcher.DispatchEvent(EventEnum.UiValueChange, new ReactivePropertyChanged<T>(_value));
                _value = value;
            }
        }

        public EventDispatcher Dispatcher { get; }

        public ReactiveProperty()
        {
            Dispatcher = new EventDispatcher(this);
        }
    }
}
