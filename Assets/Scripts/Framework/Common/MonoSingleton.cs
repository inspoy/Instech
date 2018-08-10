/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: MonoSingleton.cs
 * Created on 2018/05/02 by inspoy
 * All rights reserved.
 */

using System;
using UnityEngine;

namespace Instech.Framework
{
    /// <summary>
    /// MonoBehavior的单例模式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DisallowMultipleComponent]
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        /// <summary>
        /// 获取唯一实例
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
        /// 创建唯一实例
        /// </summary>
        public static void CreateSingleton()
        {
            if (_instance != null)
            {
                throw new MethodAccessException("重复创建" + typeof(T));
            }
            if (FindObjectsOfType<T>().Length > 1)
            {
                throw new MethodAccessException("存在多个" + typeof(T));
            }
            _instance = FindObjectOfType<T>();
            if (_instance == null)
            {
                // 创建实例
                var singleGo = new GameObject("[Singleton]" + typeof(T));
                DontDestroyOnLoad(singleGo);
                _instance = singleGo.AddComponent<T>();
                _instance.Init();
            }
        }

        /// <summary>
        /// 销毁实例
        /// </summary>
        public static void DestroySingleton()
        {
            if (_instance != null)
            {
                _instance.Uninit();
                Destroy(_instance);
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
