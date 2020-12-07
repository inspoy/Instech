// == Inspoy Technology ==
// Assembly: Intech.Framework.Core
// FileName: Singleton.cs
// Created on 2018/05/02 by inspoy
// All rights reserved.

using System;

namespace Instech.Framework.Core
{
    /// <summary>
    /// 用于标识是否是自动创建的单例
    /// </summary>
    public sealed class AutoCreateSingletonTag { }

    /// <summary>
    /// 自动创建，无需手动调用CreateSingleton方法的单例
    /// </summary>
    public abstract class AutoCreateSingleton<T> : BaseSingleton<T, AutoCreateSingletonTag> where T : BaseSingleton<T, AutoCreateSingletonTag>, new()
    {

    }

    /// <summary>
    /// 普通单例，需要手动调用CreateSingleton方法来创建
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> : BaseSingleton<T, object> where T : BaseSingleton<T, object>, new()
    {

    }

    /// <summary>
    /// 单例模式
    /// </summary>
    public abstract class BaseSingleton<T, TAuto> where T : BaseSingleton<T, TAuto>, new() where TAuto : class
    {
        private static T _instance;

        /// <summary>
        /// 获取单例的唯一实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                if (typeof(AutoCreateSingletonTag).IsAssignableFrom(typeof(TAuto)))
                {
                    // 可自动创建
                    CreateSingleton();
                }
                else
                {
                    // 必须手动调用CreateSingleton()创建实例，避免在不恰当的时刻再次初始化本已被销毁的单例
                    throw new InvalidOperationException($"{typeof(T)} is not instantiated, please use CreateSingleton() to create.");
                }
                return _instance;
            }
        }

        /// <summary>
        /// 单例是否已经存在
        /// </summary>
        /// <returns></returns>
        public static bool HasSingleton()
        {
            return _instance != null;
        }

        /// <summary>
        /// 创建单例唯一实例，并调用Init方法
        /// </summary>
        public static void CreateSingleton()
        {
            if (_instance != null)
            {
                throw new MethodAccessException("重复创建" + typeof(T));
            }
            _instance = new T();
            _instance.Init();
        }

        /// <summary>
        /// 销毁单例实例，并调用Deinit方法
        /// </summary>
        public static void DestroySingleton()
        {
            if (_instance != null)
            {
                _instance.Deinit();
                _instance = null;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// 销毁
        /// </summary>
        protected virtual void Deinit()
        {
        }
    }
}
