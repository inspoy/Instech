/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Utils
 * FileName: Interpolation.cs
 * Created on 2018/12/03 by inspoy
 * All rights reserved.
 */

using UnityEngine;

namespace Instech.Framework.Utils
{
    public enum EaseType
    {
        /// <summary>
        /// 线性插值
        /// </summary>
        Linear,

        /// <summary>
        /// 三次方进入缓动
        /// </summary>
        CubeIn,

        /// <summary>
        /// 三次方退出缓动
        /// </summary>
        CubeOut,

        /// <summary>
        /// 三次方两侧缓动
        /// </summary>
        CubeInOut,

        /// <summary>
        /// 四分之一圆弧进入缓动
        /// </summary>
        CircleIn,

        /// <summary>
        /// 四分之一圆弧退出缓动
        /// </summary>
        CircleOut,

        /// <summary>
        /// 四分之一圆弧两侧缓动
        /// </summary>
        CircleInOut
    }

    /// <summary>
    /// 插值方法组，支持各种缓动函数
    /// </summary>
    public static class Interpolation
    {
        /// <summary>
        /// 计算插值
        /// </summary>
        /// <param name="t">插值参数</param>
        /// <param name="from">起点</param>
        /// <param name="to">终点</param>
        /// <param name="type">插值方法</param>
        /// <param name="unclamped">是否不裁剪t到0-1范围内（有些方法必须裁剪）</param>
        /// <returns></returns>
        public static float Calc(float t, float from, float to, EaseType type, bool unclamped = false)
        {
            switch (type)
            {
                case EaseType.Linear:
                    return Linear(t, from, to, !unclamped);
                case EaseType.CubeIn:
                    return CubeIn(t, from, to);
                case EaseType.CubeOut:
                    return CubeOut(t, from, to);
                case EaseType.CubeInOut:
                    return CubeInOut(t, from, to);
                case EaseType.CircleIn:
                    return CircleIn(t, from, to);
                case EaseType.CircleOut:
                    return CircleOut(t, from, to);
                case EaseType.CircleInOut:
                    return CircleInOut(t, from, to);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 线性插值
        /// </summary>
        /// <param name="t">参数</param>
        /// <param name="a">t=0时的值</param>
        /// <param name="b">t=1时的值</param>
        /// <param name="clamped">把参数t的范围裁剪到0-1</param>
        /// <returns>插值结果</returns>
        private static float Linear(float t, float a, float b, bool clamped = true)
        {
            if (clamped)
            {
                t = Mathf.Clamp01(t);
            }
            return a + (b - a) * t;
        }

        /// <summary>
        /// 三次方进入缓动
        /// </summary>
        private static float CubeIn(float t, float a, float b)
        {
            t = Mathf.Clamp01(t);
            var f = t * t * t;
            return a + (b - a) * f;
        }

        /// <summary>
        /// 三次方退出缓动
        /// </summary>
        private static float CubeOut(float t, float a, float b)
        {
            return CubeIn(1 - t, b, a);
        }

        /// <summary>
        /// 三次方两侧缓动
        /// </summary>
        private static float CubeInOut(float t, float a, float b)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f ? CubeIn(t * 2, a, (a + b) / 2) : CubeOut(t * 2 - 1, (a + b) / 2, b);
        }

        private static float CircleIn(float t, float a, float b)
        {
            t = Mathf.Clamp01(t);
            var f = Mathf.Sqrt(1 - (t - 1) * (t - 1));
            return a + (b - a) * f;
        }

        private static float CircleOut(float t, float a, float b)
        {
            return CircleIn(1 - t, b, a);
        }

        private static float CircleInOut(float t, float a, float b)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f ? CircleIn(t * 2, a, (a + b) / 2) : CircleOut(t * 2 - 1, (a + b) / 2, b);
        }
    }
}
