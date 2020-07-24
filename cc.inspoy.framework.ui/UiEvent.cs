/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Ui
 * FileName: UiEvent.cs
 * Created on 2019/12/10 by inspoy
 * All rights reserved.
 */

using Instech.Framework.Common;
using Instech.Framework.Core;
using UnityEngine.EventSystems;

namespace Instech.Framework.Ui
{

    /// <summary>
    /// Unity事件系统的事件，这里做了一层包装以适配我们自己的事件系统
    /// </summary>
    public class UnityEventData : IEventData
    {
        public BaseEventData Content { get; set; }

        public object Clone()
        {
            return GetNewOne(Content);
        }

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