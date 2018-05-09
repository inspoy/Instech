/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ObjectPool.cs
 * Created on 2018/05/08 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;

namespace Instech.Framework
{
    /// <summary>
    /// 可被池化的对象须实现该接口
    /// </summary>
    public interface IPoolable : IDisposable
    {
        void OnRecycle();
        void OnActivate();
    }

    /// <summary>
    /// 通用对象池，线程安全
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : Singleton<ObjectPool<T>> where T:class,IPoolable, new()
    {
        private const uint DefualtMaxCount = 50;
        public uint CurPooledCount { get; private set; }
        public uint MaxPooledCount { get; set; }
        private readonly Queue<T> _pooledQueue = new Queue<T>((int)DefualtMaxCount);
        private readonly object _locker = new object();
        protected override void Init()
        {
            MaxPooledCount = DefualtMaxCount;
        }

        /// <summary>
        /// 回收某对象
        /// </summary>
        /// <param name="obj"></param>
        public void Recycle(T obj)
        {
            lock (_locker)
            {
                obj.OnRecycle();
                if (CurPooledCount >= MaxPooledCount)
                {
                    // 已经满了
                    obj.Dispose();
                    return;
                }
                _pooledQueue.Enqueue(obj);
                CurPooledCount += 1;
            }
        }

        /// <summary>
        /// 获取一个该类型的新对象
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            lock (_locker)
            {
                T ret;
                if (_pooledQueue.Count > 0)
                {
                    ret = _pooledQueue.Dequeue();
                    CurPooledCount -= 1;
                    ret.OnActivate();
                    return ret;
                }
                ret = new T();
                ret.OnActivate();
                return ret;
            }
        }
    }
}
