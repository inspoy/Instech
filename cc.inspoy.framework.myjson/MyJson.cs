/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.MyJson
 * FileName: MyJson.cs
 * Created on 2019/12/04 by inspoy
 * All rights reserved.
 */

using System;
#if UNITY_EDITOR
using UnityEngine;
#endif
using Utf8Json;
using Utf8Json.Resolvers;

namespace Instech.Framework.MyJson
{
    public static class MyJson
    {
        private static bool _resolverInited;

        public static void InitJsonResolvers(params IJsonFormatterResolver[] resolvers)
        {
            if (_resolverInited)
            {
                return;
            }
            var len = resolvers.Length;
            var combined = new IJsonFormatterResolver[len + 3];
            Array.Copy(resolvers, combined, len);
            combined[len + 0] = BuiltinResolver.Instance;
            combined[len + 1] = EnumResolver.Default;
            combined[len + 2] = Utf8Json.Unity.UnityResolver.Instance;
            CompositeResolver.RegisterAndSetAsDefault(combined);
            JsonSerializer.SetDefaultResolver(CompositeResolver.Instance);
            _resolverInited = true;
        }

        /// <summary>
        /// 将对象转换为JSON字符串
        /// </summary>
        /// <param name="obj">源对象</param>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>json字符串</returns>
        /// <exception cref="InvalidOperationException">尚未初始化</exception>
        public static string ToJson<T>(T obj)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return JsonSerializer.ToJsonString(obj);
            }
#endif
            if (!_resolverInited) throw new InvalidOperationException("Resolvers has not been set.");
            return JsonSerializer.ToJsonString(obj);
        }

        public static byte[] ToBytes<T>(T obj)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return JsonSerializer.Serialize(obj);
            }
#endif
            if (!_resolverInited) throw new InvalidOperationException("Resolvers has not been set.");
            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// 从字符串解析JSON
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>目标类型的对象</returns>
        /// <exception cref="InvalidOperationException">尚未初始化</exception>
        public static T FromJson<T>(string json)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return JsonSerializer.Deserialize<T>(json);
            }
#endif
            if (!_resolverInited) throw new InvalidOperationException("Resolvers has not been set.");
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// 从字节流解析JSON
        /// </summary>
        /// <param name="bytes">字节流</param>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>目标类型的对象</returns>
        /// <exception cref="InvalidOperationException">尚未初始化</exception>
        public static T FromJson<T>(byte[] bytes)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return JsonSerializer.Deserialize<T>(bytes);
            }
#endif
            if (!_resolverInited) throw new InvalidOperationException("Resolvers has not been set.");
            return JsonSerializer.Deserialize<T>(bytes);
        }
    }
}
