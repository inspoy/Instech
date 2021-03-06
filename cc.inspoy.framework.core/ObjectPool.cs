// == Inspoy Technology ==
// Assembly: Instech.Framework.Core
// FileName: ObjectPool.cs
// Created on 2018/05/08 by inspoy
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Instech.Framework.Core
{
    /// <summary>
    /// 可被池化的对象须实现该接口
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 回收时调用
        /// </summary>
        void OnRecycle();

        /// <summary>
        /// 初始化时调用
        /// </summary>
        void OnActivate();

        /// <summary>
        /// 销毁时调用
        /// </summary>
        void OnDestroy();
    }

    /// <summary>
    /// 提供一个可池化对象的基类，这样就不需要每次都实现所有三个接口了
    /// </summary>
    public abstract class BasePoolable : IPoolable
    {
        public virtual void OnRecycle()
        {
        }

        public virtual void OnActivate()
        {
        }

        public virtual void OnDestroy()
        {
        }
    }

    /// <summary>
    /// 用于获得对象池的各种属性
    /// </summary>
    public interface IObjectPool
    {
        uint CreatedCount { get; }
        uint PooledCount { get; }
        uint ActiveCount { get; }
        uint MaxCount { get; }
        uint SavedCount { get; }
        string BaseType { get; }
    }

    /// <summary>
    /// 对象池扩展方法
    /// </summary>
    public static class ObjectPoolExtension
    {
        /// <summary>
        /// 回收该对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        public static void Recycle<T>(this T self) where T : class, IPoolable, new()
        {
            ObjectPool<T>.Instance.Recycle(self);
        }
    }

    /// <summary>
    /// 对象池管理器，用于输出对象池的使用情况
    /// </summary>
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        private Dictionary<string, IObjectPool> _dictPool;

        protected override void Init()
        {
            _dictPool = new Dictionary<string, IObjectPool>();
        }

        internal void OnObjectPoolCreated(IObjectPool pool)
        {
            _dictPool[pool.BaseType] = pool;
        }

        /// <summary>
        /// 获取调试用的字符串
        /// </summary>
        /// <returns></returns>
        public string GetDebugInformation()
        {
            var sb = new StringBuilder();
            sb.Append("===== ObjectPoolsStatistics =====\n");
            sb.Append("Name,Created,Pooled,Active,Saved,Max\n");
            foreach (var item in _dictPool)
            {
                sb.Append(
                    $"{item.Key},{item.Value.CreatedCount},{item.Value.PooledCount},{item.Value.ActiveCount},{item.Value.SavedCount},{item.Value.MaxCount}\n");
            }
            return sb.ToString();
        }
    }

    public class PoolingCallbackNotSetException : Exception
    {
        private static string MakeMessage(Type t)
        {
            return $"Type {t.FullName} cannot be used in a ObjectPool since not all callbacks are set";
        }

        public PoolingCallbackNotSetException(Type t) : base(MakeMessage(t))
        {
        }
    }

    /// <summary>
    /// 通用对象池，线程安全
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : AutoCreateSingleton<ObjectPool<T>>, IObjectPool where T : class, new()
    {
        private const uint DefualtMaxCount = 64;
        private readonly Queue<T> _pooledQueue = new Queue<T>((int)DefualtMaxCount);
        private readonly object _locker = new object();

        public uint CreatedCount { get; private set; }
        public uint PooledCount { get; private set; }
        public uint ActiveCount { get; private set; }
        public uint SavedCount { get; private set; }
        public uint MaxCount { get; set; }
        public string BaseType => typeof(T).ToString();

        /// <summary>
        /// 泛型参数T是否实现IPoolable接口
        /// </summary>
        private readonly bool _isPoolable = typeof(IPoolable).IsAssignableFrom(typeof(T));

        protected override void Init()
        {
            MaxCount = DefualtMaxCount;
            CreatedCount = 0;
            PooledCount = 0;
            ActiveCount = 0;
            SavedCount = 0;
#if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying && !ObjectPoolManager.HasSingleton())
            {
                ObjectPoolManager.CreateSingleton();
            }
#endif
            ObjectPoolManager.Instance.OnObjectPoolCreated(this);
        }

        #region 对非IPoolable对象的支持

        public readonly Action<T> EmptyCallback = _ => { };
        public Action<T> RecycleCallback { get; set; }
        public Action<T> ActivateCallback { get; set; }
        public Action<T> DestroyCallback { get; set; }

        private bool IsAllCallbacksValid()
        {
            return RecycleCallback != null && ActivateCallback != null && DestroyCallback != null;
        }

        #endregion

        /// <summary>
        /// 回收某对象
        /// </summary>
        /// <param name="obj"></param>
        public void Recycle(T obj)
        {
            lock (_locker)
            {
                if (_isPoolable)
                {
                    ((IPoolable)obj).OnRecycle();
                }
                else if (IsAllCallbacksValid())
                {
                    RecycleCallback(obj);
                }
                else
                {
                    throw new PoolingCallbackNotSetException(typeof(T));
                }
                ActiveCount -= 1;
                if (PooledCount >= MaxCount)
                {
                    // 已经满了
                    if (_isPoolable)
                    {
                        ((IPoolable)obj).OnDestroy();
                    }
                    else
                    {
                        // 如果上面没throw，那执行到这里回调一定是有效的
                        DestroyCallback(obj);
                    }
                    return;
                }
                _pooledQueue.Enqueue(obj);
                PooledCount += 1;
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
                ActiveCount += 1;
                if (_pooledQueue.Count > 0)
                {
                    ret = _pooledQueue.Dequeue();
                    PooledCount -= 1;
                    SavedCount += 1;
                    if (_isPoolable)
                    {
                        ((IPoolable)ret).OnActivate();
                    }
                    else if (IsAllCallbacksValid())
                    {
                        ActivateCallback(ret);
                    }
                    else
                    {
                        throw new PoolingCallbackNotSetException(typeof(T));
                    }
                    return ret;
                }
                ret = new T();
                CreatedCount += 1;
                if (_isPoolable)
                {
                    ((IPoolable)ret).OnActivate();
                }
                else if (IsAllCallbacksValid())
                {
                    ActivateCallback(ret);
                }
                else
                {
                    throw new PoolingCallbackNotSetException(typeof(T));
                }
                return ret;
            }
        }

        /// <summary>
        /// 获取一个该类型的新对象
        /// </summary>
        /// <returns></returns>
        public static T GetNew()
        {
            return Instance.Get();
        }

        /// <summary>
        /// 预先分配一些出来
        /// </summary>
        /// <param name="size">分配数量，0表示最大数量</param>
        public void Alloc(uint size = 0)
        {
            lock (_locker)
            {
                if (size == 0)
                {
                    size = MaxCount;
                }
                size = Math.Min(size, MaxCount - PooledCount);
                for (var i = 0; i < size; ++i)
                {
                    ActiveCount += 1;
                    Recycle(new T());
                    CreatedCount += 1;
                }
            }
        }

        /// <summary>
        /// 释放所有已回收的对象
        /// </summary>
        public void Clear()
        {
            lock (_locker)
            {
                if (_isPoolable)
                {
                    foreach (var item in _pooledQueue)
                    {
                        ((IPoolable)item).OnRecycle();
                        ((IPoolable)item).OnDestroy();
                    }
                }
                else if (IsAllCallbacksValid())
                {
                    foreach (var item in _pooledQueue)
                    {
                        RecycleCallback(item);
                        DestroyCallback(item);
                    }
                }
                else
                {
                    throw new PoolingCallbackNotSetException(typeof(T));
                }
                PooledCount = 0;
            }
        }
    }
}
