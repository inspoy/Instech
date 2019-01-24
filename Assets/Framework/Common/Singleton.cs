/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: Singleton.cs
 * Created on 2018/05/02 by inspoy
 * All rights reserved.
 */

using System;

namespace Instech.Framework
{
    /// <summary>
    /// 单例模式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        /// <summary>
        /// 获取单例的唯一实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateSingleton();
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
        /// 销毁单例实例，并调用Uninit方法
        /// </summary>
        public static void DestroySingleton()
        {
            if (_instance != null)
            {
                _instance.Uninit();
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
        protected virtual void Uninit()
        {
        }
    }
}
