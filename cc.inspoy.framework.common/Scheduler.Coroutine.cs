// == Inspoy Technology ==
// Assembly: Instech.Framework.Common
// FileName: Scheduler.Coroutine.cs
// Created on 2020/11/20 by inspoy
// All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Instech.Framework.Logging;

namespace Instech.Framework.Common
{
    public partial class Scheduler
    {
        public class Coroutine
        {
            public string Name { get; }
            public bool Finished { get; private set; }
            public Exception Exc { get; private set; }
            private readonly IEnumerator _co;

            public Coroutine(IEnumerator co, string name)
            {
                _co = co;
                Name = name;
            }

            public void Execute()
            {
                if (Finished)
                {
                    return;
                }
                try
                {
                    Finished = _co.MoveNext();
                }
                catch (Exception exc)
                {
                    Finished = true;
                    Exc = exc;
                }
            }
        }

        #region Static Interface

        public static void StartCoroutine(IEnumerator coroutine, out int handle, string name)
        {
            handle = ++_counter;
            if (string.IsNullOrEmpty(name))
            {
                name = handle.ToString();
            }
            Instance.AddCoroutine(coroutine, handle, name);
        }

        public static void StopCoroutine(int handle)
        {
            Instance.RemoveCoroutine(handle);
        }

        #endregion

        #region Fields

        private static int _counter;
        private Dictionary<int, Coroutine> _coroutineMap = new Dictionary<int, Coroutine>();
        private Dictionary<int, Coroutine> _delayAdd = new Dictionary<int, Coroutine>();
        private List<int> _delayRemove = new List<int>();

        #endregion

        private void AddCoroutine(IEnumerator co, int handle, string name)
        {
            if (_lateUpdateLock)
            {
                _delayAdd.Add(handle, new Coroutine(co, name));
                return;
            }
            _coroutineMap.Add(handle, new Coroutine(co, name));
        }

        private void RemoveCoroutine(int handle)
        {
            if (_lateUpdateLock)
            {
                _delayRemove.Add(handle);
                return;
            }
            _coroutineMap.Remove(handle);
        }

        private void UpdateCoroutines()
        {
            foreach (var kvp in _coroutineMap)
            {
                var co = kvp.Value;
                co.Execute();
                if (co.Finished)
                {
                    if (co.Exc != null)
                    {
                        Logger.LogError(LogModule.Framework, $"Exception occurred in coroutine{co.Name}");
                        Logger.LogException(LogModule.Framework, co.Exc);
                    }
                    _delayRemove.Add(kvp.Key);
                }
            }

            foreach (var keyValuePair in _delayAdd)
            {
                _coroutineMap.Add(keyValuePair.Key, keyValuePair.Value);
            }
            foreach (var handle in _delayRemove)
            {
                _coroutineMap.Remove(handle);
            }
        }
    }
}
