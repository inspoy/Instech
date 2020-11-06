/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.AssetHelper.Editor
 * FileName: AssetBuilder.cs
 * Created on 2019/12/10 by inspoy
 * All rights reserved.
 */

/*
 * 使用说明：
 * 将Assets/Artwork下的所有资源打成AB
 * 按文件夹划分，每个目录（不含子目录）一个Bundle，自动解决依赖关系
 * 对于图集，要求所有散图放到同一个目录中，图集的名字和目录相同，例如：
 * 散图为：xxx/icon/*.png，则图集的路径应当为：xxx/icon.spriteatlas
 *
 * 目前存在问题：UI图集的特殊处理，编辑UI时是拖的散图的引用，而这些散图不会打Bundle，就会导致散图打进UI的prefab里，造成资源冗余
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Instech.Framework.Common.Editor;
using Instech.Framework.Logging;
using Instech.Framework.Utils;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using Aes = Instech.EncryptHelper.Aes;
using BuildCompression = UnityEngine.BuildCompression;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.AssetHelper.Editor
{
    public static class AssetBuilder
    {
        private const BuildTarget Target = BuildTarget.StandaloneWindows64;
        private const BuildTargetGroup TargetGroup = BuildTargetGroup.Standalone;

        /// <summary>
        /// 静默模式，为true则不显示进度条和对话框提示
        /// </summary>
        private static bool _silentMode;

        /// <summary>
        /// 强制重新打包
        /// </summary>
        private static bool _rebuild;

        /// <summary>
        /// 打包AssetBundle
        /// </summary>
        /// <param name="rebuild">是否强制重新打包</param>
        /// <param name="silentMode">是否不显示弹出对话框</param>
        public static AssetBuildReport BuildAssetBundle(bool rebuild, bool silentMode)
        {
            _rebuild = rebuild;
            _silentMode = silentMode;
            var report = new AssetBuildReport();
            var failed = false;
            var resRoot =
                Path.GetFullPath(Path.Combine(Application.dataPath, ProjectSettings.Instance.ArtworkRootPath.TrimStart('/')))
                    .Replace('\\', '/');
            Logger.Assert(LogModule.Build, Directory.Exists(resRoot), "ResRoot does not exist: " + resRoot);
            Logger.LogInfo(LogModule.Build, "Start building AssetBundle, resRoot=" + resRoot);
            var sw = Stopwatch.StartNew();
            var resFiles = Directory
                .GetFiles(resRoot, "*", SearchOption.AllDirectories)
                .Where(el => !el.EndsWith(".meta"))
                .Select(Path.GetFullPath)
                .Select(el => el.Replace('\\', '/').Replace(resRoot, ""))
                .ToList();
            Logger.LogInfo(LogModule.Build, "File count: " + resFiles.Count);

            // 1. Collect bundles
            var content = CollectBundles("Assets/" + ProjectSettings.Instance.ArtworkRootPath.TrimStart('/'), resFiles);
            Logger.LogInfo(LogModule.Build, $"Collected {content.Count} Bundles, start building...");
            
            // 2. Build
            var (retCode, results) = DoBuild(content);
            Logger.LogInfo(LogModule.Build, $"Bulding bundles finished, retCode={retCode}");
            
            // 3. Generate resmeta & pack to assetpack
            if (retCode == ReturnCode.Success)
            {
                PostProcess(results, content);
                report.AssetPacks = GenerateReport(results, content);
            }
            else
            {
                failed = true;
            }

            if (failed)
            {
                var timeCost = sw.ElapsedMilliseconds / 1000f;
                report.CostTime = timeCost;
                Logger.LogInfo(LogModule.Build, $"Failed to build AssetBundle, cost: {timeCost:F3}s");
                if (!_silentMode)
                {
                    EditorUtility.DisplayDialog("失败", $"AssetBundle构建失败，耗时{timeCost:F1}秒\n原因：{retCode}", "OK");
                }
            }
            else
            {
                var timeCost = sw.ElapsedMilliseconds / 1000f;
                report.CostTime = timeCost;
                Logger.LogInfo(LogModule.Build, $"AssetBundle built, cost: {timeCost:F3}s");
                if (!_silentMode)
                {
                    EditorUtility.DisplayDialog("成功", $"AssetBundle构建成功，耗时{timeCost:F1}秒", "OK");
                }
            }
            report.IsSuccessful = !failed;
            return report;
        }

        /// <summary>
        /// 收集需要打Bundle的资源文件
        /// </summary>
        /// <param name="resRoot">资源根目录相对dataPath的路径，如Assets/Artwork/</param>
        /// <param name="resFiles">文件列表</param>
        /// <returns></returns>
        private static List<AssetBundleBuild> CollectBundles(string resRoot, List<string> resFiles)
        {
            var ret = new Dictionary<string, List<string>>();
            var ignoredBundleNames = new List<string>();
            foreach (var item in resFiles)
            {
                // item = prefab/Complex/p3.prefab
                // abName = prefab/Complex
                // fileName = p3.prefab
                var abName = Path.GetDirectoryName(item)?.Replace('\\', '/');
                var fileName = Path.GetFileName(item);
                if (string.IsNullOrEmpty(abName) || string.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                if (fileName.EndsWith(".spriteatlas"))
                {
                    // 记录和图集同名的目录
                    ignoredBundleNames.Add(abName + "/" + fileName.Replace(".spriteatlas", ""));
                }

                if (!ret.TryGetValue(abName, out var assets))
                {
                    assets = new List<string>();
                    ret.Add(abName, assets);
                }

                assets.Add(fileName);
            }

            foreach (var item in ignoredBundleNames)
            {
                // 移除和图集同名的目录，避免重复打包资源
                if (ret.ContainsKey(item))
                {
                    ret.Remove(item);
                }
            }

            return ret.Select(pair => new AssetBundleBuild
            {
                assetBundleName = pair.Key.Replace('/', '.'),
                assetNames = pair.Value
                    .Select(el =>
                        Path.Combine(resRoot, pair.Key + '/' + el).Replace('\\', '/'))
                    .ToArray(),
                addressableNames = pair.Value.ToArray()
            }).ToList();
        }

        /// <summary>
        /// 构建Bundle
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static (ReturnCode, IBundleBuildResults) DoBuild(List<AssetBundleBuild> content)
        {
            if (content.Count == 0)
            {
                return (ReturnCode.MissingRequiredObjects, null);
            }

            var targetPath = Path.Combine(Application.dataPath, "../RawBundles");
            var exist = Directory.Exists(targetPath);
            if (_rebuild && exist)
            {
                Directory.Delete(targetPath, true);
            }

            if (!exist)
            {
                Directory.CreateDirectory(targetPath);
            }

            var buildParams = new BundleBuildParameters(Target, TargetGroup, targetPath)
            {
                // 此选项将导致不兼容更新版本的Unity，但可以使Bundle尺寸变小
                // 注：如果在上线后需要升级Unity版本，则所有AssetBundle都需要强制更新
                ContentBuildFlags = ContentBuildFlags.DisableWriteTypeTree,
                // LZ4压缩相比LZMA包体要大不少，但解压更快，并且从硬盘加载时无需全部解压，内存占用小，有助于提升加载速度
                // 注：Unity官方也推荐使用LZ4
                BundleCompression = BuildCompression.LZ4
            };
            var buildContent = new BundleBuildContent(content);
            try
            {
                var retCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out var result);
                return (retCode, result);
            }
            catch (Exception e)
            {
                Logger.LogException(LogModule.Build, e);
                return (ReturnCode.Exception, null);
            }
        }

        /// <summary>
        /// 根据构建结果做自定义处理
        /// </summary>
        /// <param name="results">构建结果</param>
        /// <param name="content">资源清单</param>
        private static void PostProcess(IBundleBuildResults results, List<AssetBundleBuild> content)
        {
            var sb = new StringBuilder();
            // 1. generate dependency map
            foreach (var item in results.BundleInfos)
            {
                sb.Append(item.Key);
                var deps = item.Value.Dependencies;
                foreach (var depItem in deps)
                {
                    sb.Append(',');
                    sb.Append(depItem);
                }
                sb.Append('\n');
            }
            sb.Append("#\n");

            // 2. generate resmap
            foreach (var bundleInfo in content)
            {
                for (var i = 0; i < bundleInfo.assetNames.Length; ++i)
                {
                    sb.Append($"{bundleInfo.assetNames[i]}|{bundleInfo.addressableNames[i]}|{bundleInfo.assetBundleName}\n");
                }
            }
            sb.Append("#\n");

            // 3. generate packmap
            var sp = AssetPackSpecifier.Instance;
            var reverseMap = sp != null ? sp.GetReverseMap() : new Dictionary<string, string>();
            var packMap = new Dictionary<string, List<string>>();
            foreach (var bundleInfo in content)
            {
                if (!reverseMap.TryGetValue(bundleInfo.assetBundleName, out var packName))
                {
                    packName = sp.DefaultPackName;
                    reverseMap.Add(bundleInfo.assetBundleName, packName);
                }
                if (!packMap.ContainsKey(packName))
                {
                    packMap.Add(packName, new List<string>());
                }
                packMap[packName].Add(bundleInfo.assetBundleName);
            }
            foreach (var pair in reverseMap)
            {
                sb.Append($"{pair.Key}|{pair.Value}\n");
            }
            sb.Append("#\n");

            File.WriteAllText(Path.Combine(Application.dataPath, "../RawBundles/resmeta.txt"), sb.ToString());
            Logger.LogInfo(LogModule.Build, $"ResMeta Generated:\n{sb}");
            var aes = new Aes();
            var aesKey = Convert.FromBase64String("rgKbWVdP4G+4dsNB9Baxdtm/G5VgPWEVhCF/bZ+uNWxtezoocMUREEeLhxaPhmct");
            aesKey = SHA384.Create().TransformFinalBlock(aesKey, 0, aesKey.Length);
            aes.Init(aesKey);
            var bytes = aes.Encrypt(Encoding.UTF8.GetBytes(sb.ToString()));
            aes.UnInit();
            File.WriteAllBytes(Path.Combine(Application.dataPath, "../RawBundles/resmeta"), bytes);

            // 4. pack to ipk
            var basePath = Path.Combine(Application.dataPath, "../RawBundles/");
            var targetPath = Utility.ResourcesPath;
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            File.Copy(basePath + "resmeta", targetPath + "resmeta", true);
            foreach (var pair in packMap)
            {
                FilePacker.FilePacker.PackToFile(basePath, pair.Value, targetPath + pair.Key, "InstechSecretResource_" + pair.Key, false);
            }
            Logger.LogInfo(LogModule.Build, $"Generated {packMap.Count} packs, postprocessing completed.");
        }

        /// <summary>
        /// 生成构建报告
        /// </summary>
        private static AssetPackInfo[] GenerateReport(IBundleBuildResults results, List<AssetBundleBuild> content)
        {
            var sp = AssetPackSpecifier.Instance;
            var packInfos = new Dictionary<string, AssetPackInfo>(sp.PackMap.Count + 1);
            var packMap = sp.GetReverseMap();
            foreach (var bundle in content)
            {
                if (!packMap.TryGetValue(bundle.assetBundleName, out var packName))
                {
                    packName = sp.DefaultPackName;
                }
                if (!packInfos.TryGetValue(packName, out var packInfo))
                {
                    packInfo = new AssetPackInfo
                    {
                        PackName = packName,
                        Bundles = new List<BundleInfo>()
                    };
                    packInfos.Add(packName, packInfo);
                }
                var bundleInfo = new BundleInfo
                {
                    BundleName = bundle.assetBundleName,
                    Dependencies = results.BundleInfos[bundle.assetBundleName].Dependencies,
                    Assets = new AssetInfo[bundle.assetNames.Length]
                };
                for (var i = 0; i < bundle.assetNames.Length; ++i)
                {
                    bundleInfo.Assets[i] = new AssetInfo
                    {
                        AssetName = bundle.addressableNames[i],
                        AssetPath = bundle.assetNames[i]
                    };
                }
                packInfo.Bundles.Add(bundleInfo);
            }

            return packInfos.Values.ToArray();
        }

    }
}
