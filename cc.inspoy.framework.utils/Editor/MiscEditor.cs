/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Utils.Editor
 * FileName: MiscEditor.cs
 * Created on 2018/05/01 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Instech.Framework.Common.Editor;
using Instech.Framework.Logging;
using UnityEditor;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;
using Object = UnityEngine.Object;

namespace Instech.Framework.Utils.Editor
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

        private static string _userName;

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
            var assemblyName = GetAssemblyName(path);
            var productName = Application.productName;
            if (assemblyName.StartsWith("Instech."))
            {
                productName = "Inspoy Technology";
            }

            var filename = Path.GetFileName(path);
            fullText = fullText.Replace("##ProductName##", productName);
            fullText = fullText.Replace("##AssemblyName##", assemblyName);
            fullText = fullText.Replace("##FileName##", filename);
            fullText = fullText.Replace("##CreateDate##", DateTime.Now.ToString("yyyy/MM/dd"));
            fullText = fullText.Replace("##Author##", GetUserName());
            var original = File.ReadAllText(path);
            if (original.StartsWith("/**"))
            {
                // 防止重复添加
                fullText = original;
            }
            else
            {
                fullText += original;
                Logger.LogInfo(LogModule.Editor, "Generated script header for: " + path);
            }

            File.WriteAllText(path, fullText, new UTF8Encoding(false));
        }

        /// <summary>
        /// 默认返回git用户名，没装git的话返回windows用户名
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            if (!string.IsNullOrEmpty(_userName))
            {
                return _userName;
            }
            var psi = new ProcessStartInfo("git", "config user.name")
            {
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var p = Process.Start(psi);
            if (p != null)
            {
                p.WaitForExit();
                var ret = p.StandardOutput.ReadLine();
                p.Dispose();
                if (!string.IsNullOrWhiteSpace(ret))
                {
                    _userName = ret;
                    return _userName;
                }
            }

            _userName = Environment.UserName;
            return _userName;
        }

        private static string GetAssemblyName(string srcPath)
        {
            if (!File.Exists(srcPath))
            {
                return "Unknown";
            }
            var root = Path.GetPathRoot(srcPath);
            if (root == null)
            {
                root = string.Empty;
            }
            root = root.Replace("\\", "/");
            var curPath = Path.GetDirectoryName(Path.GetFullPath(srcPath));
            while (curPath != null && root != curPath.Replace("\\", "/"))
            {
                var files = Directory.GetFiles(curPath, "*.asmdef", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    return Path.GetFileNameWithoutExtension(files[0]);
                }
                curPath = Path.GetFullPath(Path.Combine(curPath, ".."));
            }
            return "Unknown";
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
                Logger.LogInfo(LogModule.Editor, $"{obj.name} => {obj.GetInstanceID()}(Type={obj.GetType()})");
            }
        }

        /// <summary>
        /// 复制选中资产的路径到剪贴板
        /// </summary>
        public static void CopyAssetPathToClipboard()
        {
            var asset = Selection.activeObject;
            var str = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrWhiteSpace(str))
            {
                str = str.Replace("Assets" + ProjectSettings.Instance.ArtworkRootPath, "");
                var t = new TextEditor { text = str };
                t.OnFocus();
                t.Copy();
            }
        }

        /// <summary>
        /// 打开Log文件所在的目录
        /// </summary>
        public static void OpenLogFolder()
        {
            var folderPath = Application.persistentDataPath + "/GameLog";
            if (!Directory.Exists(folderPath))
            {
                folderPath = Application.persistentDataPath;
                EditorUtility.DisplayDialog("错误", "GameLog目录不存在", "OK");
            }

            Process.Start("cmd.exe", "/c start " + folderPath.Replace('\\', '/'));
        }

        /// <summary>
        /// 打开Standalone崩溃目录
        /// </summary>
        public static void OpenCrashFolder()
        {
            var folderPath = Application.temporaryCachePath + "/Crashes";
            if (!Directory.Exists(folderPath))
            {
                folderPath = Application.temporaryCachePath;
                EditorUtility.DisplayDialog("错误", "Crashes目录不存在", "OK");
            }

            Process.Start("cmd.exe", "/c start " + folderPath.Replace('\\', '/'));
        }

        /// <summary>
        /// 切换激活状态
        /// </summary>
        public static void ToggleGameObjectActive()
        {
            var go = Selection.activeGameObject;
            if (go != null && go.scene.IsValid())
            {
                go.SetActive(!go.activeSelf);
                EditorUtility.SetDirty(go);
            }
        }

        public static void CheckScriptHeader()
        {
            var packageRoot = Path.Combine(Path.GetFullPath("Packages/cc.inspoy.framework.utils/"), "..");
            var files = Directory.GetFiles(Application.dataPath, "*.cs.meta", SearchOption.AllDirectories)
                .Union(Directory.GetFiles(packageRoot, "*.cs.meta", SearchOption.AllDirectories))
                .Select(path => path.Replace('\\', '/'))
                .Where(path => (path.Contains("Assets/Scripts/") || path.Contains("cc.inspoy.framework")) &&
                               !path.Contains("/GenCode/"))
                .ToList();
            try
            {
                for (var i = 0; i < files.Count; i++)
                {
                    var metaPath = files[i];
                    if (EditorUtility.DisplayCancelableProgressBar(
                        $"[{i}/{files.Count}]Processing...",
                        metaPath.Replace(".meta", ""),
                        1f * i / files.Count))
                    {
                        break;
                    }

                    ScriptHeaderGenerator.OnWillCreateAsset(metaPath);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// 功能测试
        /// </summary>
        public static void FunctionTest()
        {
            // cool stuff...
        }

        /// <summary>
        /// 随机数均匀性测试
        /// </summary>
        public static void RandomTest()
        {
            for (var d = 2; d <= 5; ++d)
            {
                var tex = new Texture2D(4096, 4096, TextureFormat.RGBA32, false);
                for (var i = 1; i <= 5000000; ++i)
                {
                    var x = Utility.GetRandom(1, 4096);
                    for (var j = 0; j < d - 2; ++j)
                    {
                        Utility.GetRandom(1, 4096);
                    }

                    var y = Utility.GetRandom(1, 4096);
                    tex.SetPixel(x, y, Color.black);
                }

                File.WriteAllBytes($"E:/_tmp/rand{d}.png", tex.EncodeToPNG());
            }
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
