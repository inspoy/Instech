// == Inspoy Technology ==
// Assembly: Instech.Framework.Common
// FileName: EventData.cs
// Created on 2018/05/03 by inspoy
// All rights reserved.

using System;
using Instech.Framework.Core;

namespace Instech.Framework.Common
{
    /// <summary>
    /// 事件数据
    /// </summary>
    public interface IEventData : IPoolable, ICloneable
    {
        /// <summary>
        /// 回收到对象池
        /// </summary>
        void RecycleData();
    }

    /// <summary>
    /// 简单的事件数据，可以方便地传一个简单的数据
    /// </summary>
    public class SimpleEventData : IEventData
    {
        public object ObjVal { get; private set; }
        public bool BoolVal { get; private set; }
        public int IntVal { get; private set; }
        public float FloatVal { get; private set; }
        public string StringVal { get; private set; } = string.Empty;
        public Action<int> CallbackIntAction { get; private set; }

        public object Clone()
        {
            var ret = ObjectPool<SimpleEventData>.Instance.Get();
            ret.ObjVal = ObjVal;
            ret.BoolVal = BoolVal;
            ret.IntVal = IntVal;
            ret.FloatVal = FloatVal;
            ret.StringVal = StringVal;
            ret.CallbackIntAction = CallbackIntAction;
            return ret;
        }

        public void OnDestroy()
        {
            // do nothing
        }

        public void RecycleData()
        {
            this.Recycle();
        }

        public void OnRecycle()
        {
            ObjVal = null;
            BoolVal = false;
            IntVal = 0;
            FloatVal = 0;
            StringVal = string.Empty;
            CallbackIntAction = null;
        }

        public void OnActivate()
        {
            // do nothing
        }

        public static SimpleEventData GetNewOne(bool val)
        {
            var ret = ObjectPool<SimpleEventData>.Instance.Get();
            ret.BoolVal = val;
            return ret;
        }

        public static SimpleEventData GetNewOne(int val)
        {
            var ret = ObjectPool<SimpleEventData>.Instance.Get();
            ret.IntVal = val;
            return ret;
        }

        public static SimpleEventData GetNewOne(float val)
        {
            var ret = ObjectPool<SimpleEventData>.Instance.Get();
            ret.FloatVal = val;
            return ret;
        }

        public static SimpleEventData GetNewOne(string val)
        {
            var ret = ObjectPool<SimpleEventData>.Instance.Get();
            ret.StringVal = val;
            return ret;
        }
    }

    public class ValueChangeData<T> : IEventData
    {
        public T Value;
        public T OldValue;

        public object Clone()
        {
            return GetNewOne(Value, OldValue);
        }

        public void OnRecycle()
        {
            Value = default;
            OldValue = default;
        }

        public void OnActivate()
        {
            // do nothing
        }

        public void OnDestroy()
        {
            // do nothing
        }

        public void RecycleData()
        {
            this.Recycle();
        }

        public static ValueChangeData<T> GetNewOne(T val, T oldVal)
        {
            var ret = ObjectPool<ValueChangeData<T>>.Instance.Get();
            ret.Value = val;
            ret.OldValue = oldVal;
            return ret;
        }
    }
}
