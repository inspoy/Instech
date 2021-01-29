// == Inspoy Technology ==
// Assembly: Instech.Framework.Common
// FileName: ThreadPool.cs
// Created on 2020/12/28 by inspoy
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using Instech.Framework.Core;
using Instech.Framework.Logging;

namespace Instech.Framework.Common
{
    public class ThreadPool : Singleton<ThreadPool>
    {
        #region Fields

        private static int _mainThreadId;
        private readonly Dictionary<int, int> _threadIdToListIndexMap = new Dictionary<int, int>();
        private readonly List<Thread> _runningThreads = new List<Thread>();
        private readonly Queue<Action> _pendingActionsOnMainThread = new Queue<Action>();

        #endregion

        #region Interfaces

        /// <summary>
        /// 当前是否运行在主线程上
        /// </summary>
        /// <returns></returns>
        public static bool IsOnMainThread() => _mainThreadId == Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        /// 新开线程执行指定任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns>线程ID</returns>
        public static int AddTask(Action task)
        {
            if (!IsOnMainThread())
            {
                throw new InvalidOperationException("Tasks can only be added in main thread");
            }
            return Instance.AddTaskImpl(task);
        }

        /// <summary>
        /// 中止指定任务的执行
        /// </summary>
        public static void RemoveTask(int threadId)
        {
            if (!IsOnMainThread())
            {
                throw new InvalidOperationException("Tasks can only be removed in main thread");
            }
            Instance.RemoveTaskImpl(threadId);
        }

        /// <summary>
        /// 指定任务是否还在运行？
        /// </summary>
        public static bool IsTaskRunning(int threadId)
        {
            lock (Instance)
            {
                return Instance._threadIdToListIndexMap.ContainsKey(threadId);
            }
        }

        /// <summary>
        /// 在主线程上运行指定任务
        /// </summary>
        public static void RunOnMainThread(Action task)
        {
            lock (Instance)
            {
                Instance._pendingActionsOnMainThread.Enqueue(task);
            }
        }

        #endregion

        protected override void Init()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            Scheduler.RegisterLogicUpdate(this, OnUpdate);
        }

        protected override void Deinit()
        {
            Scheduler.UnregisterLogicUpdate(this);
            foreach (var thread in _runningThreads)
            {
                thread.Abort();
            }
        }

        private void OnUpdate(float dt)
        {
            for (var i = 0; i < _runningThreads.Count; i++)
            {
                var thread = _runningThreads[i];
                if (thread.IsAlive)
                {
                    continue;
                }
                _runningThreads[i] = null;
                _threadIdToListIndexMap.Remove(thread.ManagedThreadId);
            }
            foreach (var task in _pendingActionsOnMainThread)
            {
                task?.Invoke();
            }
        }

        private int AddTaskImpl(Action task)
        {
            var thread = new Thread(() => task?.Invoke());
            var slot = FindAvailableSlot();
            _runningThreads[slot] = thread;
            _threadIdToListIndexMap.Add(thread.ManagedThreadId, slot);
            return thread.ManagedThreadId;
        }

        private void RemoveTaskImpl(int threadId)
        {
            if (!_threadIdToListIndexMap.TryGetValue(threadId, out var slot))
            {
                Logger.LogError(LogModule.Framework, "ThreadId does not exist: " + threadId);
            }
            _runningThreads[slot].Abort();
            _runningThreads[slot] = null;
            _threadIdToListIndexMap.Remove(threadId);
        }

        private int FindAvailableSlot()
        {
            for (var i = 0; i < _runningThreads.Count; i++)
            {
                if (_runningThreads[i] == null)
                {
                    return i;
                }
            }
            _runningThreads.Add(null);
            return _runningThreads.Count - 1;
        }
    }
}
