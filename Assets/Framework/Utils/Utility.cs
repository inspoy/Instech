/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: Utils.cs
 * Created on 2018/05/05 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Instech.Framework
{
    /// <summary>
    /// 实用工具方法
    /// </summary>
    public static class Utility
    {
        private static long _uidCount = 100000;
        private static readonly DateTime TimeStamp1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private static ulong _randomCurrent;

        static Utility()
        {
            ResetRandom();
        }

        public static void ResetRandom()
        {
            _randomCurrent = (uint)Environment.TickCount % 233280;
        }

        /// <summary>
        /// 获得一个随机数（使用线性同余法）
        /// </summary>
        /// <param name="min">下限</param>
        /// <param name="max">上限（不含）</param>
        /// <returns></returns>
        public static int GetRandom(int min, int max)
        {
            if (min == max || min == max - 1) return min;
            if (min > max)
            {
                var t = min;
                min = max;
                max = t;
            }
            var m = (ulong)(max - min);
            _randomCurrent = (_randomCurrent * 16807 + 49297) % int.MaxValue;
            return (int)(_randomCurrent % m) + min;
        }

        /// <summary>
        /// 获取当前时间的Unix时间戳
        /// </summary>
        /// <returns>时间戳</returns>
        public static uint GetTimeStampNow()
        {
            var ts = DateTime.UtcNow - TimeStamp1970;
            return Convert.ToUInt32(ts.TotalSeconds);
        }

        /// <summary>
        /// 获取当前时间的精确到毫秒的Unix时间戳
        /// </summary>
        /// <returns>时间戳</returns>
        public static ulong GetMiliTimeStampNow()
        {
            var ts = DateTime.UtcNow - TimeStamp1970;
            return Convert.ToUInt64(ts.TotalMilliseconds);
        }

        /// <summary>
        /// 查找某GO下的子物体，注意这个方法会消耗相当多的时间，最好不要在update里调用
        /// 如果有多个同名的子物体，只会返回第一个
        /// </summary>
        /// <returns>子物体</returns>
        /// <param name="parent">父物体</param>
        /// <param name="childName">子物体的name</param>
        public static GameObject FindChildWithName(this GameObject parent, string childName)
        {
            var parentTrans = parent.transform;
            foreach (Transform trans in parentTrans.GetComponentInChildren<Transform>())
            {
                if (trans.name == childName)
                {
                    return trans.gameObject;
                }
                var child = trans.gameObject.FindChildWithName(childName);
                if (child != null)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取字符串的MD5值
        /// </summary>
        /// <returns>The Md5.</returns>
        /// <param name="src">Source String.</param>
        public static string GetMd5(string src)
        {
            var md5 = new MD5CryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(src);
            var md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            var destString = new StringBuilder();
            foreach (var item in md5Data)
            {
                destString.Append(Convert.ToString(item, 16).PadLeft(2, '0'));
            }
            return destString.ToString().PadLeft(32, '0');
        }

        /// <summary>
        /// 随机返回数组的一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static T GetRandomItem<T>(this T[] arr)
        {
            if (arr == null || arr.Length == 0)
            {
                return default(T);
            }
            return arr[GetRandom(0, arr.Length)];
        }

        /// <summary>
        /// 随机返回列表的一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T GetRandomItem<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default(T);
            }
            return list[GetRandom(0, list.Count)];
        }

        /// <summary>
        /// 根据权重获得一个集合中的随机元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="all">集合</param>
        /// <param name="funcWeight">获取权重的函数</param>
        /// <returns></returns>
        public static T GetRandomWithWeight<T>(this ICollection<T> all, Func<T, int> funcWeight) where T : class
        {
            var totalWeight = 0;
            foreach (var item in all)
            {
                totalWeight += funcWeight(item);
            }
            var rand = GetRandom(0, totalWeight);
            T fallback = null;
            foreach (var item in all)
            {
                fallback = item;
                if (rand < funcWeight(item))
                {
                    return item;
                }
                rand -= funcWeight(item);
            }
            return fallback;
        }

        /// <summary>
        /// 返回指定概率的true
        /// </summary>
        /// <param name="p">概率, 0~1</param>
        /// <returns></returns>
        public static bool DoProbability(float p)
        {
            if (p > 1.0f)
            {
                return true;
            }
            if (p < 0.0f)
            {
                return false;
            }
            return (int)(p * 1000000) > GetRandom(0, 1000000);
        }

        /// <summary>
        /// 把字符串分割成整数数组
        /// </summary>
        /// <param name="str">要分割的字符串</param>
        /// <param name="sep">分隔符，默认为半角逗号','</param>
        /// <returns></returns>
        public static int[] SplitToInt(string str, char sep = ',')
        {
            if (string.IsNullOrEmpty(str))
            {
                return new int[0];
            }
            var items = str.Split(sep);
            var ret = new int[items.Length];
            for (var i = 0; i < items.Length; ++i)
            {
                try
                {
                    ret[i] = Convert.ToInt32(items[i]);
                }
                catch
                {
                    ret[i] = 0;
                }
            }
            return ret;
        }

        /// <summary>
        /// 把长字符串按照分隔符分割成字符串数组
        /// </summary>
        /// <param name="str">要分割的字符串</param>
        /// <param name="sep">分隔符，默认为半角逗号','</param>
        /// <returns></returns>
        public static string[] SplitToString(string str, char sep = ',')
        {
            if (string.IsNullOrEmpty(str))
            {
                return new string[0];
            }
            var items = str.Split(sep);
            return items;
        }

        /// <summary>
        /// 四舍五入
        /// </summary>
        /// <param name="f">F.</param>
        public static int Round(float f)
        {
            return (int)Math.Round(f, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 获取游戏进程中的一个全局唯一ID，线程安全
        /// </summary>
        /// <returns></returns>
        public static uint GetUniqueId()
        {
            return (uint)Interlocked.Increment(ref _uidCount);
        }

        /// <summary>
        /// 判断两个浮点数是否相等（足够接近）
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static bool FloatEqual(float f1, float f2)
        {
            return Mathf.Abs(f1 - f2) < Mathf.Epsilon;
        }

        /// <summary>
        /// 判断两个浮点数是否相等（足够接近）
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static bool FloatEqual(double f1, double f2)
        {
            return Mathf.Abs((float)(f1 - f2)) < Mathf.Epsilon;
        }

        /// <summary>
        /// 添加一个空的子对象
        /// </summary>
        /// <param name="go"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject AddEmptyChild(this GameObject go, string name)
        {
            var ret = new GameObject(name);
            ret.transform.SetParent(go.transform);
            return ret;
        }

        /// <summary>
        /// 计算一个集合的哈希值
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string CalcHash(this IEnumerable self)
        {
            if (self == null) return string.Empty;
            var ans = 0;
            foreach (var item in self)
            {
                if (item == null) continue;
                ans ^= item.GetHashCode();
            }

            return ((uint)ans).ToString("D10");
        }
    }
}
