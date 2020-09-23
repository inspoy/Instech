/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Common
 * FileName: Scheduler.Timer.cs
 * Created on 2020/07/10 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using Instech.Framework.Core;
using Instech.Framework.Logging;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Common
{
    public partial class Scheduler
    {
        #region Static Interfaces

        /// <summary>
        /// 下一次LateUpdate中触发
        /// 注意，如果调用该方法的时机晚于LateUpdate，则需要多等一帧
        /// </summary>
        /// <param name="action"></param>
        public static uint NextFrame(Action action)
        {
            if (Instance._lateUpdateLock)
            {
                Logger.LogWarning(LogModule.Framework, "Cannot add timer during timer's callback");
                return 0;
            }

            if (action != null)
            {
                var item = ActionTimer.Create(action);
                item.RestFrame = 2; // 第一次LateUpdate还在当前帧，不触发
                item.RestLoop = 1; // 执行一次
                item.Mode = TimerMode.FrameMode;
                Instance._actionTimers.Add(item.Id, item);
                return item.Id;
            }
            return 0;
        }

        public static uint EveryFrame(Action action)
        {
            if (Instance._lateUpdateLock)
            {
                Logger.LogWarning(LogModule.Framework, "Cannot add timer during timer's callback");
                return 0;
            }

            if (action == null)
            {
                return 0;
            }

            var item = ActionTimer.Create(action);
            item.RestFrame = 2;
            item.Interval = 1;
            item.RestLoop = -1;
            item.Mode = TimerMode.FrameMode;
            Instance._actionTimers.Add(item.Id, item);
            return item.Id;
        }

        public static uint DelayCall(Action action, float seconds)
        {
            return AddTimer(action, seconds);
        }

        /// <summary>
        /// 添加定时器
        /// </summary>
        /// <param name="action">回调方法</param>
        /// <param name="interval">触发间隔，秒</param>
        /// <param name="loopTimes">循环次数，负数表示无限循环，0表示执行一次，正数表示执行N次</param>
        /// <param name="realTime">是否使用真实时间</param>
        /// <returns>定时器ID</returns>
        public static uint AddTimer(Action action, float interval = 1f, int loopTimes = 0, bool realTime = false)
        {
            if (Instance._lateUpdateLock)
            {
                Logger.LogWarning(LogModule.Framework, "Cannot add timer during timer's callback");
                return 0;
            }

            if (loopTimes == 0)
            {
                loopTimes = 1;
            }
            if (interval < 0)
            {
                interval = 0;
            }

            var timer = ActionTimer.Create(action);
            timer.RestTime = interval;
            timer.Interval = interval;
            timer.RestLoop = loopTimes;
            timer.Mode = realTime ? TimerMode.RealTimeMode : TimerMode.TimeMode;
            Instance._actionTimers.Add(timer.Id, timer);
            return timer.Id;
        }

        /// <summary>
        /// 通过回调移除定时器（建议使用定时器ID移除）
        /// </summary>
        /// <param name="action">回调方法</param>
        /// <returns>是否移除成功</returns>
        public static bool RemoveTimer(Action action)
        {
            if (Instance._lateUpdateLock)
            {
                Logger.LogWarning(LogModule.Framework, "Cannot remove timer during timer's callback");
                return false;
            }

            foreach (var timer in Instance._actionTimers.Values)
            {
                if (timer.Callback == action)
                {
                    timer.Recycle();
                    Instance._actionTimers.Remove(timer.Id);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 通过ID移除定时器（建议使用）
        /// </summary>
        /// <param name="timerId">定时器ID</param>
        /// <returns>是否移除成功</returns>
        public static bool RemoveTimer(uint timerId)
        {
            if (Instance._lateUpdateLock)
            {
                Logger.LogWarning(LogModule.Framework, "Cannot remove timer during timer's callback");
                return false;
            }

            if (Instance._actionTimers.TryGetValue(timerId, out var timer))
            {
                timer.Recycle();
                Instance._actionTimers.Remove(timerId);
                return true;
            }

            return false;
        }

        #endregion

        private enum TimerMode
        {
            Invalid,
            FrameMode,
            TimeMode,
            RealTimeMode
        }

        private class ActionTimer : IPoolable
        {
            private static uint _idCounter = 10000U;
            public uint Id { get; private set; }
            public Action Callback { get; private set; }
            public int RestFrame { get; set; }
            public float RestTime { get; set; }
            public float Interval { get; set; }
            public int RestLoop { get; set; }
            public TimerMode Mode { get; set; }

            public void OnRecycle()
            {
                Id = 0;
                Callback = null;
                RestFrame = 0;
                RestTime = 0;
                Interval = 0;
                RestLoop = 0;
                Mode = TimerMode.Invalid;
            }

            public void OnActivate()
            {
                Id = ++_idCounter;
            }

            public void OnDestroy()
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

        private void InitTimer()
        {
            _actionTimers = new Dictionary<uint, ActionTimer>();
            _removeList = new List<uint>();
        }

        /// <summary>
        /// Late Update
        /// </summary>
        private void UpdateTimer()
        {
            _removeList.Clear();
            foreach (var item in _actionTimers)
            {
                try
                {
                    var shouldTerminate = true;
                    var mode = item.Value.Mode;
                    if (mode == TimerMode.FrameMode)
                    {
                        shouldTerminate = HandleFrameTimer(item.Value);
                    }
                    else if (mode == TimerMode.TimeMode)
                    {
                        shouldTerminate = HandleTimer(item.Value, false);
                    }
                    else if (mode == TimerMode.RealTimeMode)
                    {
                        shouldTerminate = HandleTimer(item.Value, true);
                    }

                    if (shouldTerminate)
                    {
                        _removeList.Add(item.Key);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogException(LogModule.Framework, e);
                }
            }

            foreach (var item in _removeList)
            {
                _actionTimers[item].Recycle();
                _actionTimers.Remove(item);
            }
        }

        private bool HandleFrameTimer(ActionTimer item)
        {
            item.RestFrame -= 1;
            if (item.RestFrame <= 0)
            {
                item.Callback();
                item.RestLoop -= 1;
                if (item.RestLoop == 0)
                {
                    return true;
                }

                item.RestFrame += (int)item.Interval;
            }

            return false;
        }

        private bool HandleTimer(ActionTimer item, bool realTime)
        {
            var dt = realTime ? Time.unscaledDeltaTime : Time.deltaTime;
            item.RestTime -= dt;
            if (item.RestTime <= 0)
            {
                item.Callback();
                item.RestLoop -= 1;
                if (item.RestLoop == 0)
                {
                    return true;
                }

                item.RestTime += item.Interval;
            }

            return false;
        }
    }
}
