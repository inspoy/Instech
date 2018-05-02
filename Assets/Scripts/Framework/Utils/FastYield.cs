/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: FastYield.cs
 * Created on 2018/05/02 by inspoy
 * All rights reserved.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace Instech.Framework
{
    /// <summary>
    /// Yield优化，缓存常用的对象，优化性能减少GC
    /// </summary>
    public class FastYield : Singleton<FastYield>
    {
        private WaitForEndOfFrame _waitForEndOfFrame;
        private WaitForFixedUpdate _waitForFixedUpdate;
        private Dictionary<float, WaitForSeconds> _waitForSeconds;
        private Dictionary<float, WaitForSecondsRealtime> _waitForSecondsRealtime;

        /// <summary>
        /// 获取缓存的WaitForEndOfFrame
        /// </summary>
        /// <returns></returns>
        public static WaitForEndOfFrame WaitForEndOfFrame()
        {
            return Instance._waitForEndOfFrame;
        }

        /// <summary>
        /// 获取缓存的WaitForFixedUpdate
        /// </summary>
        /// <returns></returns>
        public static WaitForFixedUpdate WaitForFixedUpdate()
        {
            return Instance._waitForFixedUpdate;
        }

        /// <summary>
        /// 获取缓存的WaitForSeconds，不存在则新建
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            WaitForSeconds ret;
            Instance._waitForSeconds.TryGetValue(seconds, out ret);
            if (ret != null)
            {
                return ret;
            }
            ret = new WaitForSeconds(seconds);
            Instance._waitForSeconds.Add(seconds, ret);
            return ret;
        }

        /// <summary>
        /// 获取缓存的WaitForSecondsRealtime，不存在则新建
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
        {
            WaitForSecondsRealtime ret;
            Instance._waitForSecondsRealtime.TryGetValue(seconds, out ret);
            if (ret != null)
            {
                return ret;
            }
            ret = new WaitForSecondsRealtime(seconds);
            Instance._waitForSecondsRealtime.Add(seconds, ret);
            return ret;
        }

        protected override void Init()
        {
            _waitForEndOfFrame = new WaitForEndOfFrame();
            _waitForFixedUpdate = new WaitForFixedUpdate();
            _waitForSeconds = new Dictionary<float, WaitForSeconds>(new FloatComparer())
            {
                {1.0f, new WaitForSeconds(1.0f)}
            };
            _waitForSecondsRealtime = new Dictionary<float, WaitForSecondsRealtime>(new FloatComparer())
            {
                {1.0f, new WaitForSecondsRealtime(1.0f)}
            };
        }
    }
}
