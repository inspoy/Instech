/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: Scheduler.cs
 * Created on 2019/01/06 by inspoy
 * All rights reserved.
 */


using System;
using System.Collections.Generic;

namespace Instech.Framework
{
    /// <summary>
    /// 常用异步操作
    /// </summary>
    public class Scheduler : MonoSingleton<Scheduler>
    {
        private class ActionTimer : IPoolable
        {
            public uint Id { get; private set; }
            public Action Callback { get; private set; }
            public int RestFrame { get; set; }
            public void OnRecycle()
            {
                Id = 0;
                Callback = null;
                RestFrame = 0;
            }

            public void OnActivate()
            {
                Id = Utility.GetUniqueId();
            }

            public void Dispose()
            {
                // do nothing
            }

            public static ActionTimer Create(Action callback)
            {
                var ret = ObjectPool<ActionTimer>.Instance.Get();
                ret.Callback = callback;
                return ret;
            }
        }

        private Dictionary<uint, ActionTimer> _actionTimers;
        private List<uint> _removeList;

        protected override void Init()
        {
            _actionTimers = new Dictionary<uint, ActionTimer>();
            _removeList = new List<uint>();
        }

        private void LateUpdate()
        {
            _removeList.Clear();
            foreach (var item in _actionTimers)
            {
                if (item.Value.RestFrame <= 0)
                {
                    _removeList.Add(item.Key);
                }
                item.Value.RestFrame -= 1;
            }
            foreach (var item in _removeList)
            {
                _actionTimers[item].Callback();
                _actionTimers[item].Recycle();
                _actionTimers.Remove(item);
            }
        }

        #region Static Interfaces

        /// <summary>
        /// 下一帧的LateUpdate中触发
        /// </summary>
        /// <param name="action"></param>
        public static void NextFrame(Action action)
        {
            if (action != null)
            {
                var item = ActionTimer.Create(action);
                item.RestFrame = 1;
                Instance._actionTimers.Add(item.Id, item);
            }
        }

        #endregion
    }
}
