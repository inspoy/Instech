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
        private static int _idx;
        private static int _assetCount;
        private static int _abCount;
        private static StringBuilder _pathMap;
        private static bool _cleaning;

        /// <summary>
        /// 执行打包AB，同步操作，用于命令行打包APP
        /// </summary>
        public static void DoBuildQuietly(string abRoot)
        {
            // 根据文件夹结构自动设置AssetBundleName
            _resourceRoot = Application.dataPath + abRoot;
            Logger.LogInfo(LogModule.Resource, "开始打包AssetBundle, root=" + _resourceRoot);
            _files = Directory.GetFiles(_resourceRoot, "*", SearchOption.AllDirectories);
            _startTime = DateTime.Now;
            _idx = 0;
            _assetCount = 0;
            _abCount = 0;
            _pathMap = new StringBuilder();
            while (true)
            {
                var filePath = _files[_idx];
                if (!filePath.EndsWith(".meta"))
                {
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

                _idx++;
                if (_idx >= _files.Length)
                {
                    AssetDatabase.Refresh();
                    Directory.Delete(Application.streamingAssetsPath + "/Bundles/", true);
                    Directory.CreateDirectory(Application.streamingAssetsPath + "/Bundles/");
                    File.WriteAllText(Application.streamingAssetsPath + "/Bundles/pathMap.txt", _pathMap.ToString());
                    _pathMap = null;
                    Logger.LogInfo(LogModule.Resource, "准备path完成，开始打包");
                    var manifest = BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/Bundles", BuildAssetBundleOptions.UncompressedAssetBundle,
                        BuildTarget.StandaloneWindows);
                    var allAb = manifest.GetAllAssetBundles();
                    Logger.LogInfo(LogModule.Resource, $"导出完成，导出了{allAb.Length}个AssetBundle");
                    _abCount = allAb.Length;
                    _idx = 0;
                    break;
                }
            }

            Logger.LogInfo(LogModule.Resource, "收尾工作中");
            while (true)
            {
                var filePath = _files[_idx];
                if (!filePath.EndsWith(".meta"))
                {
                    var assetPath = "Assets" + filePath.Replace(Application.dataPath, "").Replace("\\", "/");
                    var ai = AssetImporter.GetAtPath(assetPath);
                    ai.assetBundleName = "";
                }

                _idx++;
                if (_idx >= _files.Length)
                {
                    // ReSharper disable once DelegateSubtraction
                    EditorApplication.update -= BuildOperation;
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.RemoveUnusedAssetBundleNames();
                    AssetDatabase.Refresh();
                    var time = DateTime.Now;
                    var resultStr = $"导出了{_abCount}个AssetBundle\n共{_assetCount}个Asset\n耗时{(time - _startTime).TotalSeconds:N2}秒";
                    Logger.LogInfo(LogModule.Resource, resultStr);
                    break;
                }
            }
        }

        /// <summary>
        /// 执行打包AB
        /// </summary>
        public static void DoBuild()
        {
            // 根据文件夹结构自动设置AssetBundleName
            _resourceRoot = Application.dataPath + EditorPrefs.GetString(PrefWindow.AssetPath, string.Empty);
            Logger.LogInfo(LogModule.Resource, "开始打包AssetBundle, root=" + _resourceRoot);
            _files = Directory.GetFiles(_resourceRoot, "*", SearchOption.AllDirectories);
            _startTime = DateTime.Now;
            _idx = 0;
            _assetCount = 0;
            _abCount = 0;
            _pathMap = new StringBuilder();
            _cleaning = false;
            EditorApplication.update += BuildOperation;
        }

        private static void BuildOperation()
        {
            var filePath = _files[_idx];
            if (_cleaning)
            {
                EditorUtility.DisplayProgressBar("收尾工作中", filePath, 1.0f * _idx / _files.Length);
                if (!filePath.EndsWith(".meta"))
                {
                    var assetPath = "Assets" + filePath.Replace(Application.dataPath, "").Replace("\\", "/");
                    var ai = AssetImporter.GetAtPath(assetPath);
                    ai.assetBundleName = "";
                }

                _idx++;
                if (_idx >= _files.Length)
                {
                    // ReSharper disable once DelegateSubtraction
                    EditorApplication.update -= BuildOperation;
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.RemoveUnusedAssetBundleNames();
                    AssetDatabase.Refresh();
                    var time = DateTime.Now;
                    var resultStr = $"导出了{_abCount}个AssetBundle\n共{_assetCount}个Asset\n耗时{(time - _startTime).TotalSeconds:N2}秒";
                    EditorUtility.DisplayDialog("导出完成",
                        resultStr,
                        "OK");
                }
            }
            else
            {
                var isCancel = EditorUtility.DisplayCancelableProgressBar("自动设置AssetBundleName中", filePath, 1.0f * _idx / _files.Length);

                if (!filePath.EndsWith(".meta"))
                {
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

                _idx++;
                if (isCancel || _idx >= _files.Length)
                {
                    AssetDatabase.Refresh();
                    if (!isCancel)
                    {
                        Directory.CreateDirectory(Application.streamingAssetsPath + "/Bundles/");
                        File.WriteAllText(Application.streamingAssetsPath + "/Bundles/pathMap.txt", _pathMap.ToString());
                        _pathMap = null;
                        var manifest = BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/Bundles", BuildAssetBundleOptions.UncompressedAssetBundle,
                            BuildTarget.StandaloneWindows);
                        var allAb = manifest.GetAllAssetBundles();
                        Logger.LogInfo(LogModule.Resource, $"导出完成，导出了{allAb.Length}个AssetBundle");
                        _abCount = allAb.Length;
                        _cleaning = true;
                        _idx = 0;
                    }
                }
            }
        }
    }
}
