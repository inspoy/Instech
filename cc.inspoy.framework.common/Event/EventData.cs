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
        /// 回收到对象池，子类实现固定为<code>this.Recycle();</code>即可
        /// </summary>
        void RecycleData();
    }

    /// <summary>
    /// 实现了<see cref="IEventData"/>不常用的接口，减少代码量
    /// </summary>
    public abstract class BasicEventData : IEventData
    {
        public virtual void OnActivate()
        {
            // do nothing
        }

        public virtual void OnRecycle()
        {
        }

        public virtual void OnDestroy()
        {
            // do nothing
        }

        public abstract object Clone();
        public abstract void RecycleData();
    }

    /// <summary>
    /// 简单的事件数据，可以方便地传一个简单的数据
    /// </summary>
    public class SimpleEventData : BasicEventData
    {
        public object ObjVal { get; private set; }
        public bool BoolVal { get; private set; }
        public int IntVal { get; private set; }
        public float FloatVal { get; private set; }
        public string StringVal { get; private set; } = string.Empty;
        public Action<int> CallbackIntAction { get; private set; }

        public override object Clone()
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

        public override void RecycleData()
        {
            this.Recycle();
        }

        public override void OnRecycle()
        {
            ObjVal = null;
            BoolVal = false;
            IntVal = 0;
            FloatVal = 0;
            StringVal = string.Empty;
            CallbackIntAction = null;
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

    public class ValueChangeData<T> : BasicEventData
    {
        public T Value;
        public T OldValue;

        public override object Clone()
        {
            return GetNewOne(Value, OldValue);
        }

        public override void OnRecycle()
        {
            Value = default;
            OldValue = default;
        }

        public override void RecycleData()
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
