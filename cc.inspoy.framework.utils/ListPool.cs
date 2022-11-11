using System;
using System.Collections.Generic;
using Instech.Framework.Core;

namespace Instech.Framework.Utils
{
    public static class ListPool
    {
        private static readonly Dictionary<Type, IObjectPool> Pools = new Dictionary<Type, IObjectPool>();

        public static List<T> Acquire<T>()
        {
            CheckPool<T>();
            return ObjectPool<List<T>>.GetNew();
        }

        public static void ReleaseToPool<T>(this List<T> sb)
        {
            ObjectPool<List<T>>.Instance.Recycle(sb);
        }

        private static void CheckPool<T>()
        {
            if (!Pools.ContainsKey(typeof(T)))
            {
                ObjectPool<List<T>>.CreateSingleton();
                var pool = ObjectPool<List<T>>.Instance;
                pool.RecycleCallback = OnRecycle;
                pool.ActivateCallback = ObjectPool<List<T>>.Instance.EmptyCallback;
                pool.DestroyCallback = ObjectPool<List<T>>.Instance.EmptyCallback;
                Pools.Add(typeof(T), pool);
            }
        }

        private static void OnRecycle<T>(List<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            list.Clear();
        }
    }
}
