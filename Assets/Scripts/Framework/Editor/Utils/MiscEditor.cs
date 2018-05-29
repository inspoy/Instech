/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: MiscEditor.cs
 * Created on 2018/05/01 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Instech.Framework.Editor
{
    public class ScriptHeaderGenerator : UnityEditor.AssetModificationProcessor
    {
        private const string Header =
            "/**\n" +
            " * == ##ProductName## ==\n" +
            " * Assembly: ##AssemblyName##\n" +
            " * FileName: ##FileName##\n" +
            " * Created on ##CreateDate## by ##Author##\n" +
            " * All rights reserved.\n" +
            " */\n\n";

        /// <summary>
        /// 当创建资产时由Unity调用
        /// </summary>
        /// <param name="path">文件路径，Assets/开头</param>
        public static void OnWillCreateAsset(string path)
        {
            if (!path.EndsWith(".cs.meta"))
            {
                // 只处理C#的meta文件
                return;
            }
            path = path.Replace(".meta", "");
            var fullText = Header;
            var productName = Application.productName;
            if (path.Contains("/Framework/"))
            {
                productName = "Inspoy Technology";
            }
            var assemblyName = CompilationPipeline
                .GetAssemblyNameFromScriptPath(path)
                .Replace(".dll", "");
            var filename = Path.GetFileName(path);
            fullText = fullText.Replace("##ProductName##", productName);
            fullText = fullText.Replace("##AssemblyName##", assemblyName);
            fullText = fullText.Replace("##FileName##", filename);
            fullText = fullText.Replace("##CreateDate##", DateTime.Now.ToString("yyyy/MM/dd"));
            fullText = fullText.Replace("##Author##", Environment.UserName);
            var original = File.ReadAllText(path);
            if (original.StartsWith("/**"))
            {
                // 防止重复添加
                fullText = original;
            }
            else
            {
                fullText += original;
            }
            File.WriteAllText(path, fullText, new UTF8Encoding(false));
        }
    }

    /// <summary>
    /// 编辑器扩展杂项
    /// </summary>
    public static class MiscEditor
    {
        /// <summary>
        /// [验证有效性]在控制台输出选定资源的InstanceID
        /// </summary>
        /// <returns></returns>
        public static bool ShowInstanceIdValidation()
        {
            return Selection.activeObject != null;
        }

        /// <summary>
        /// 在控制台输出选定资源的InstanceID
        /// </summary>
        public static void ShowInstanceId()
        {
            var obj = Selection.activeObject;
            if (obj != null)
            {
                Logger.LogInfo(LogModule.Editor, $"{obj.name} => {obj.GetInstanceID()}");
            }
        }

        /// <summary>
        /// 把所有代码的编码修改为UTF-8
        /// </summary>
        public static void ConvertToUtf8()
        {
            var allFiles = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            var encoding = new UTF8Encoding(false);
            for (var i = 0; i < allFiles.Length; ++i)
            {
                var filePath = allFiles[i];
                var allText = File.ReadAllText(filePath);
                File.WriteAllText(filePath, allText, encoding);
                if (EditorUtility.DisplayCancelableProgressBar("处理中", filePath, 1.0f * i / allFiles.Length))
                {
                    break;
                }
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 功能测试
        /// </summary>
        public static void FunctionTest()
        {
            // cool stuff...
        }
    }

    /// <summary>
    /// 查找选定资产的所有引用
    /// </summary>
    internal static class FindAllReferencesHelper
    {
        private static int _curIdx;
        private static int _refCount;
        private static string _guid;
        private static List<string> _files;

        public static void FindAllReferences()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            _guid = AssetDatabase.AssetPathToGUID(path);
            var extensions = new List<string> { ".anim", ".prefab", ".unity", ".mat", ".asset" };
            _files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Where(s =>
                {
                    var extension = Path.GetExtension(s);
                    return extension != null && extensions.Contains(extension.ToLower());
                }).ToList();
            _curIdx = 0;
            _refCount = 0;
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            var file = _files[_curIdx].Replace('\\', '/');

            var isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, 1.0f * _curIdx / _files.Count);

            if (Regex.IsMatch(File.ReadAllText(file), _guid))
            {
                Logger.LogInfo(LogModule.Editor, file, AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file)));
                _refCount += 1;
            }

            _curIdx++;
            if (isCancel || _curIdx >= _files.Count)
            {
                EditorUtility.ClearProgressBar();
                _curIdx = 0;
                Logger.LogInfo(LogModule.Editor, $"匹配结束, 共{_refCount}个引用");

                // ReSharper disable once DelegateSubtraction
                EditorApplication.update -= Update;
            }
        }

        /// <summary>
        /// 获得资源的相对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetRelativeAssetsPath(string path)
        {
            return "Assets" +
                   Path.GetFullPath(path)
                       .Replace(Path.GetFullPath(Application.dataPath), "")
                       .Replace('\\', '/');
        }
    }
}
