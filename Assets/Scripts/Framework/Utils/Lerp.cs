/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: Lerp.cs
 * Created on 2018/12/03 by inspoy
 * All rights reserved.
 */

using UnityEngine;

namespace Instech.Framework
{
    /// <summary>
    /// 插值方法组，支持各种缓动函数
    /// </summary>
    public static class Lerp
    {
        /// <summary>
        /// 线性插值
        /// </summary>
        /// <param name="t">参数</param>
        /// <param name="a">t=0时的值</param>
        /// <param name="b">t=1时的值</param>
        /// <param name="clamped">把参数t的范围裁剪到0-1</param>
        /// <returns>插值结果</returns>
        public static float Linear(float t, float a, float b, bool clamped = true)
        {
            if (clamped)
            {
                t = Mathf.Clamp01(t);
            }
            return a + (b - a) * t;
        }

        /// <summary>
        /// 四次方进入缓动
        /// </summary>
        public static float CubeIn(float t, float a, float b)
        {
            t = Mathf.Clamp01(t);
            var f = t * t * t;
            return a + (b - a) * f;
        }

        /// <summary>
        /// 四次方退出缓动
        /// </summary>
        public static float CubeOut(float t, float a, float b)
        {
            t = Mathf.Clamp01(t);
            t = 1 - t;
            var f = t * t * t;
            return a + (b - a) * (1 - f);
        }

        /// <summary>
        /// 四次方两侧缓动
        /// </summary>
        public static float CubeInOut(float t, float a, float b)
        {
            t = Mathf.Clamp01(t);
            if (t < 0.5f)
            {
                t = t * 2;
                b = a + (b - a) / 2;
                var f = t * t * t;
                return a + (b - a) * f;
            }
            else
            {
                t = 2 - 2 * t;
                a = a + (b - a) / 2;
                var f = t * t * t;
                return a + (b - a) * (1 - f);
            }
        }
    }
}
