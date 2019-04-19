/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: BuildAssetBundle.cs
 * Created on 2018/07/08 by inspoy
 * All rights reserved.
 */

using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Instech.Framework.Editor
{
    public static class BuildAssetBundle
    {
        private static string _resourceRoot;
        private static string[] _files;
        private static DateTime _startTime;
        private static int _assetCount;
        private static int _abCount;
        private static StringBuilder _pathMap;
        private static bool _silentMode;

        /// <summary>
        /// 执行打包AB
        /// </summary>
        /// <param name="abRoot">ab根目录，null表示由编辑器调用，使用EditorPref的值</param>
        public static void DoBuild(string abRoot)
        {
            // 根据文件夹结构自动设置AssetBundleName
            _silentMode = true;
            if (string.IsNullOrEmpty(abRoot))
            {
                abRoot = ProjectSettings.Instance.ArtworkRootPath;
                if (string.IsNullOrWhiteSpace(abRoot))
                {
                    Logger.LogError(LogModule.Resource, "AssetPath配置无效");
                    return;
                }
                _silentMode = false;
            }
            _resourceRoot = Application.dataPath + abRoot;
            Logger.LogInfo(LogModule.Resource, "开始打包AssetBundle, root=" + _resourceRoot);
            _files = Directory.GetFiles(_resourceRoot, "*", SearchOption.AllDirectories);
            _startTime = DateTime.Now;
            _assetCount = 0;
            _abCount = 0;
            _pathMap = new StringBuilder();
            try
            {
                AutoSetAbName();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            AbBuild();
            RecoverStep();

            Logger.LogInfo(LogModule.Resource, "打包AssetBundle结束");
        }

        /// <summary>
        /// 自动设置AssetBundleName
        /// </summary>
        private static void AutoSetAbName()
        {
            Logger.LogInfo(LogModule.Resource, "自动设置AssetBundleName");
            for (var i = 0; i < _files.Length; i++)
            {
                var filePath = _files[i];
                if (!_silentMode)
                {
                    EditorUtility.DisplayCancelableProgressBar(
                        "自动设置AssetBundleName中",
                        filePath, 1.0f * i / _files.Length);
                }
                if (filePath.EndsWith(".meta")) continue;
                var path = filePath.Replace(_resourceRoot, "").Replace("\\", "/");
                var abName = path.Remove(path.LastIndexOf("/", StringComparison.Ordinal)).Replace("/", ".").ToLower();
                var assetPath = "Assets" + filePath.Replace(Application.dataPath, "").Replace("\\", "/");
                var ai = AssetImporter.GetAtPath(assetPath);
                if (ai.assetBundleName != abName)
                {
                    ai.assetBundleName = abName;
                    Logger.LogInfo(LogModule.Resource, $"新增资产:{abName}:{filePath}");
                }

                _assetCount += 1;
                _pathMap.AppendFormat("{0}|{1}|{2}\n", path, abName, Path.GetFileNameWithoutExtension(filePath));
            }
        }

        /// <summary>
        /// 完成ABName的修改，开始打包
        /// </summary>
        private static void AbBuild()
        {
            AssetDatabase.Refresh();
            if (Directory.Exists(Application.streamingAssetsPath + "/Bundles/"))
            {
                Directory.Delete(Application.streamingAssetsPath + "/Bundles/", true);
            }
            Directory.CreateDirectory(Application.streamingAssetsPath + "/Bundles/");
            File.WriteAllText(Application.streamingAssetsPath + "/Bundles/pathMap.txt", _pathMap.ToString());
            _pathMap = null;
            Logger.LogInfo(LogModule.Resource, "准备path完成，开始打包");
            var manifest = BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/Bundles", BuildAssetBundleOptions.UncompressedAssetBundle,
                BuildTarget.StandaloneWindows);
            if (manifest == null)
            {
                Logger.LogError(LogModule.Resource, "manifest is null");
                return;
            }
            var allAb = manifest.GetAllAssetBundles();
            Logger.LogInfo(LogModule.Resource, $"导出完成，导出了{allAb.Length}个AssetBundle");
            _abCount = allAb.Length;
        }

        /// <summary>
        /// 收尾工作，还原AssetBundleName
        /// </summary>
        private static void RecoverStep()
        {
#if RECOVER_AB_NAME
            Logger.LogInfo(LogModule.Resource, "收尾工作中");
            for (var i = 0; i < _files.Length; i++)
            {
                var filePath = _files[i];
                if (!_silentMode)
                {
                    EditorUtility.DisplayProgressBar("收尾工作中", filePath, 1.0f * i / _files.Length);
                }
                if (filePath.EndsWith(".meta")) continue;
                var assetPath = "Assets" + filePath.Replace(Application.dataPath, "").Replace("\\", "/");
                var ai = AssetImporter.GetAtPath(assetPath);
                ai.assetBundleName = "";
            }

            if (!_silentMode)
            {
                EditorUtility.ClearProgressBar();
            }
#endif
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
            var time = DateTime.Now;
            var resultStr = $"导出了{_abCount}个AssetBundle\n共{_assetCount}个Asset\n耗时{(time - _startTime).TotalSeconds:N2}秒";
            Logger.LogInfo(LogModule.Resource, resultStr);
            if (!_silentMode)
            {
                EditorUtility.DisplayDialog("完成", resultStr, "OK");
            }
        }
    }
}
