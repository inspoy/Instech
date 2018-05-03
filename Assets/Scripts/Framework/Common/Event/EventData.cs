/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: EventData.cs
 * Created on 2018/05/03 by inspoy
 * All rights reserved.
 */

using System;

namespace Instech.Framework
{
    /// <summary>
    /// 事件数据
    /// </summary>
    public interface IEventData
    {
    }

    /// <summary>
    /// 简单的事件数据，可以方便地传一个简单的数据
    /// </summary>
    public class SimpleEventData : IEventData
    {
        public object ObjVal = null;
        public bool BoolVal;
        public int IntVal;
        public float FloatVal;
        public string StringVal = string.Empty;
        public Action<int> CallbackIntAction = null;

        public SimpleEventData()
        {
        }

        public SimpleEventData(bool val)
        {
            BoolVal = val;
        }

        public SimpleEventData(int val)
        {
            IntVal = val;
        }

        public SimpleEventData(float val)
        {
            FloatVal = val;
        }

        public SimpleEventData(string val)
        {
            StringVal = val;
        }
    }
}
