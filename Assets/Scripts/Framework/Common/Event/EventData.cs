/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: EventData.cs
 * Created on 2018/05/03 by inspoy
 * All rights reserved.
 */

using System;
using UnityEngine.EventSystems;

namespace Instech.Framework
{
    /// <summary>
    /// 事件数据
    /// </summary>
    public interface IEventData : IPoolable
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
        public object ObjVal { get; set; }
        public bool BoolVal { get; set; }
        public int IntVal { get; set; }
        public float FloatVal { get; set; }
        public string StringVal { get; set; } = string.Empty;
        public Action<int> CallbackIntAction { get; set; }

        public void Dispose()
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

    /// <summary>
    /// Unity事件系统的事件，这里做了一层包装以适配我们自己的事件系统
    /// </summary>
    public class UnityEventData : IEventData
    {
        public BaseEventData Content { get; set; }

        public void OnRecycle()
        {
            Content = null;
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

        public static UnityEventData GetNewOne(BaseEventData content)
        {
            var ret = ObjectPool<UnityEventData>.Instance.Get();
            ret.Content = content;
            return ret;
        }
    }
}
