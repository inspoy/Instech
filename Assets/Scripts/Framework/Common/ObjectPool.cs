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
        /// <summary>
        /// 回收时调用
        /// </summary>
        void OnRecycle();

        /// <summary>
        /// 初始化时调用
        /// </summary>
        void OnActivate();
    }

    /// <summary>
    /// 通用对象池，线程安全
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : Singleton<ObjectPool<T>> where T : class, IPoolable, new()
    {
        private const uint DefualtMaxCount = 50;
        public uint CurPooledCount { get; private set; }
        public uint MaxPooledCount { get; set; }
        private readonly Queue<T> _pooledQueue = new Queue<T>((int)DefualtMaxCount);
        private readonly object _locker = new object();

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

        /// <summary>
        /// 预先分配一些出来
        /// </summary>
        /// <param name="size">分配数量，0表示最大数量</param>
        public void Alloc(uint size = 0)
        {
            if (size == 0)
            {
                size = MaxPooledCount;
            }
            size = Math.Min(size, MaxPooledCount - CurPooledCount);
            for (var i = 0; i < size; ++i)
            {
                Recycle(new T());
            }
        }

        /// <summary>
        /// 释放所有已回收的对象
        /// </summary>
        public void Clear()
        {
            lock (_locker)
            {
                foreach (var item in _pooledQueue)
                {
                    item.OnRecycle();
                    item.Dispose();
                }
            }
        }

        protected override void Init()
        {
            MaxPooledCount = DefualtMaxCount;
        }
    }
}
