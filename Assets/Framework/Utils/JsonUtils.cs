/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: JsonUtils.cs
 * Created on 2019/03/13 by inspoy
 * All rights reserved.
 */

using System;
using System.IO;
using UnityEngine;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Instech.Framework
{
    /// <summary>
    /// 标识要生成JsonResolver代码
    /// </summary>
    public sealed class GenerateJsonResolverAttribute : Attribute
    {
        public readonly string FilePath;
        /// <summary>
        /// 标识要生成JsonResolver代码
        /// </summary>
        /// <param name="filePath">文件相对于Assets的路径</param>
        public GenerateJsonResolverAttribute(string filePath) => FilePath = filePath;

#if UNITY_EDITOR
        public string GetFileFullPath()
        {
            return Path.Combine(Application.dataPath, FilePath.TrimStart('/', '\\'));
        }
#endif
    }

    public static class JsonUtils
    {
        private static bool _resolverInited;

        public static void InitJsonResolver(params IJsonFormatterResolver[] resolvers)
        {
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

        public static string ToJson<T>(T obj)
        {
            if (!_resolverInited) throw new InvalidOperationException("Resolvers has not been set.");
            return JsonSerializer.ToJsonString(obj);
        }

        public static T FromJson<T>(string json)
        {
            if (!_resolverInited) throw new InvalidOperationException("Resolvers has not been set.");
            return JsonSerializer.Deserialize<T>(json);
        }

        public static T FromJson<T>(byte[] bytes)
        {
            if (!_resolverInited) throw new InvalidOperationException("Resolvers has not been set.");
            return JsonSerializer.Deserialize<T>(bytes);
        }
    }
}
