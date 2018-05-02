/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: MiscEditor.cs
 * Created on 2018/05/01 by inspoy
 * All rights reserved.
 */

using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

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
            File.WriteAllText(path, fullText, new UTF8Encoding(true));
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
                Debug.LogFormat("{0} => {1}", obj.name, obj.GetInstanceID());
            }
        }

        /// <summary>
        /// 功能测试
        /// </summary>
        public static void FunctionTest()
        {
            // cool stuff...
        }
    }
}
