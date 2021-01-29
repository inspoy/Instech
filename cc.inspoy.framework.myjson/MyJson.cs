// == Inspoy Technology ==
// Assembly: Instech.Framework.MyJson
// FileName: MyJson.cs
// Created on 2019/12/04 by inspoy
// All rights reserved.

#define USE_LIT_JSON

using System;
#if USE_LIT_JSON
using LitJson;
#else
using Utf8Json;
using Utf8Json.Resolvers;
#endif
#if UNITY_EDITOR
using UnityEngine;

#endif

namespace Instech.Framework.MyJson
{
    public static class MyJson
    {
#if !USE_LIT_JSON
        private static bool _resolverInited;
#endif

#if !USE_LIT_JSON
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
#endif

        public static void PrewarmType(Type t)
        {
#if USE_LIT_JSON
            JsonMapper.AddObjectMetadata(t);
#endif
        }

        /// <summary>
        /// 将对象转换为JSON字符串
        /// </summary>
        /// <param name="obj">源对象</param>
        /// <returns>json字符串</returns>
        /// <exception cref="System.InvalidOperationException">尚未初始化</exception>
        public static string ToJson(object obj)
        {
#if USE_LIT_JSON
            return JsonMapper.ToJson(obj);
#else
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return JsonSerializer.ToJsonString(obj);
            }
#endif
            if (!_resolverInited) throw new InvalidOperationException("Resolvers has not been set.");
            return JsonSerializer.ToJsonString(obj);
#endif
        }

        /// <summary>
        /// 从字符串解析JSON
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>目标类型的对象</returns>
        /// <exception cref="System.InvalidOperationException">尚未初始化</exception>
        public static T FromJson<T>(string json)
        {
#if USE_LIT_JSON
            return JsonMapper.ToObject<T>(json);
#else
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return JsonSerializer.Deserialize<T>(json);
            }
#endif
            if (!_resolverInited) throw new InvalidOperationException("Resolvers has not been set.");
            return JsonSerializer.Deserialize<T>(json);
#endif
        }
    }
}
