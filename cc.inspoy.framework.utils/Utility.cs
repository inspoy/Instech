// == Inspoy Technology ==
// Assembly: Instech.Framework.Utils
// FileName: Utils.cs
// Created on 2018/05/05 by inspoy
// All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Instech.Framework.Utils
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
        public static Transform FindChildWithName(this Transform parent, string childName)
        {
            var parentTrans = parent.transform;
            foreach (Transform trans in parentTrans.GetComponentInChildren<Transform>())
            {
                if (trans.name == childName)
                {
                    return trans;
                }

                var child = trans.FindChildWithName(childName);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        public static Transform FindChildByPath(this Transform trans, string path)
        {
            if (trans == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(path))
            {
                return trans;
            }
            if (path.IndexOf('/') == -1)
            {
                return trans.Find(path);
            }
            var nodeNames = path.Split('/');
            var curNode = trans;
            foreach (var name in nodeNames)
            {
                curNode = curNode.Find(name);
                if (curNode == null)
                {
                    // 没找到
                    return null;
                }
            }
            return curNode;
        }

        public static T GetComponentByPath<T>(this GameObject go, string path) where T : Component
        {
            var trans = go.transform.FindChildByPath(path);
            return trans.GetComponent<T>();
        }

        /// <summary>
        /// 获取字符串的MD5值
        /// </summary>
        /// <returns>The Md5(32 bytes array).</returns>
        /// <param name="src">Source String.</param>
        public static byte[] GetMd5(string src)
        {
            var md5 = new MD5CryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(src);
            var ret = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();
            return ret;
        }

        /// <summary>
        /// 获取字符串的MD5值(字符串形式)
        /// </summary>
        /// <param name="src">Source String.</param>
        /// <returns>The Md5(32 bytes string).</returns>
        public static string GetMd5String(string src)
        {
            var rawMd5 = GetMd5(src);
            var destString = new StringBuilder();
            foreach (var item in rawMd5)
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
        /// 随机打乱
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list == null || list.Count < 2)
            {
                return;
            }
            for (var i = 0; i < list.Count - 1; i++)
            {
                var j = GetRandom(i, list.Count);
                if (i != j)
                {
                    var t = list[i];
                    list[i] = list[j];
                    list[j] = t;
                }
            }
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
        /// 把字符串分割成浮点数数组
        /// </summary>
        /// <param name="str">要分割的字符串</param>
        /// <param name="sep">分隔符，默认为半角逗号','</param>
        /// <returns></returns>
        public static float[] SplitToFloat(string str, char sep = ',')
        {
            if (string.IsNullOrEmpty(str))
            {
                return new float[0];
            }

            var items = str.Split(sep);
            var ret = new float[items.Length];
            for (var i = 0; i < items.Length; ++i)
            {
                try
                {
                    ret[i] = Convert.ToSingle(items[i]);
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
        /// 把多个元素用指定分隔符连接起来，功能和string.Join()类似
        /// </summary>
        /// <param name="source">元素的集合</param>
        /// <param name="separator">分隔符</param>
        /// <returns></returns>
        public static string CombineToString(IEnumerable source, string separator)
        {
            var sep = string.Empty;
            var ret = StringBuilderPool.Acquire();
            foreach (var item in source)
            {
                ret.Append(sep);
                ret.Append(item);
                sep = separator;
            }

            return ret.ToString();
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

        /// <summary>
        /// 指定枚举集合是否包含某一项或几项
        /// </summary>
        /// <param name="set"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlag(int set, int flags)
        {
            return (set & flags) > 0;
        }

        public static bool InheritsFrom<T>(this Type t) where T : class
        {
            return t != null && typeof(T).IsAssignableFrom(t);
        }

        /// <summary>
        /// 获取某个Transform的层级路径，主要用于打印调试信息
        /// </summary>
        /// <param name="self"></param>
        /// <param name="root">相对于</param>
        /// <returns></returns>
        public static string GetHierarchyPath(this Transform self, Transform root = null)
        {
            if (self == null)
            {
                return string.Empty;
            }

            var parent = self.parent;
            if (parent == null || parent == root)
            {
                return self.name;
            }

            return parent.GetHierarchyPath(root) + "/" + self.name;
        }

        /// <summary>
        /// Resources目录，编辑器下和Assets同级，Standalone下和exe文件同级，末尾包含斜杠
        /// </summary>
        public static readonly string ResourcesPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Resources/"));

        /// <summary>
        /// 复制文件夹
        /// </summary>
        /// <param name="srcPath">源目录的路径</param>
        /// <param name="targetPath">目标路径（应当不存在）</param>
        /// <param name="recursively">复制子目录中的文件</param>
        /// <returns>是否复制成功</returns>
        public static bool CopyTree(string srcPath, string targetPath, bool recursively = true)
        {
            if (!Directory.Exists(srcPath) || Directory.Exists(targetPath))
            {
                return false;
            }
            Directory.CreateDirectory(targetPath);
            var di = new DirectoryInfo(srcPath);
            var files = di.GetFiles();
            foreach (var item in files)
            {
                var path = Path.Combine(targetPath, item.Name);
                item.CopyTo(path);
            }
            if (recursively)
            {
                var folders = di.GetDirectories();
                foreach (var item in folders)
                {
                    CopyTree(Path.Combine(srcPath, item.Name), Path.Combine(targetPath, item.Name));
                }
            }
            return true;
        }

        /// <summary>
        /// 逐元素比较两个数组是否相同
        /// </summary>
        public static bool CompareArray<T>(T[] arr1, T[] arr2)
        {
            var comparer = EqualityComparer<T>.Default;
            if (arr1 == null || arr2 == null)
            {
                return false;
            }
            if (arr1.Length != arr2.Length)
            {
                return false;
            }
            for (var i = 0; i < arr1.Length; ++i)
            {
                if (!comparer.Equals(arr1[i], arr2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 十六进制RGB转颜色
        /// </summary>
        /// <param name="hex">24位颜色值，一般以十六进制表示如0x66ccff</param>
        public static Color AsRgb(this uint hex)
        {
            return new Color(
                (hex & 0xff0000) / 255f,
                (hex & 0x00ff00) / 255f,
                (hex & 0x0000ff) / 255f
            );
        }

        /// <summary>
        /// 十六进制RGBA转颜色
        /// </summary>
        /// <param name="hex">32位颜色值，一般以十六进制表示如0x66ccffaa</param>
        public static Color AsRgba(this uint hex)
        {
            return new Color(
                (hex & 0xff000000) / 255f,
                (hex & 0x00ff0000) / 255f,
                (hex & 0x0000ff00) / 255f,
                (hex & 0x000000ff) / 255f
            );
        }

        public static unsafe bool FastStartsWith(this string self, string prefix)
        {
            if (self == null || prefix == null || self.Length < prefix.Length)
            {
                return false;
            }
            var idx = 0;
            var maxLen = prefix.Length;
            fixed (char* pStr = self)
            fixed (char* pPrefix = prefix)
            {
                while (idx < maxLen)
                {
                    if (*(pStr + idx) != *(pPrefix + idx))
                    {
                        return false;
                    }
                    idx++;
                }
                return true;
            }
        }
    }
}
