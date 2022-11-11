// == Inspoy Technology ==
// Assembly: Instech.Framework.AssetHelper.Editor
// FileName: AssetBuildReport.cs
// Created on 2020/08/24 by inspoy
// All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Text;
using Instech.Framework.Common;
using UnityEngine;

namespace Instech.Framework.AssetHelper.Editor
{
    public class AssetBuildReport
    {
        public bool IsSuccessful;
        public float CostTime;
        public AssetPackInfo[] AssetPacks;

        public void ToMarkdownString(StringBuilder sb)
        {
            sb.Append($"Result: {(IsSuccessful ? "Successful" : "Failed")}, timeCost:{CostTime}s\n\n");
            var packs = AssetPacks;
            foreach (var pack in packs)
            {
                var packName = pack.PackName.Replace(".assetpack", "");
                var packPath = Path.Combine(PathHelper.ResourceDataPath, pack.PackName);
                var packSize = new FileInfo(packPath).Length;
                sb.Append($"### {packName}({FormatSize((ulong)packSize)})\n\n");
                var bundles = pack.Bundles;
                foreach (var bundle in bundles)
                {
                    var bundlePath = Path.Combine(Application.dataPath, "../RawBundles/", bundle.BundleName);
                    var bundleSize = new FileInfo(bundlePath).Length;
                    sb.Append($"Bundle: {bundle.BundleName}({FormatSize((ulong)bundleSize)})\n\n");
                    sb.Append("> Dependencies:\n>\n");
                    if (bundle.Dependencies.Length == 0)
                    {
                        sb.Append("> * *None*\n");
                    }

                    for (var i = 0; i < bundle.Dependencies.Length; i++)
                    {
                        var dependency = bundle.Dependencies[i];
                        sb.Append($"> {i + 1}. {dependency}\n");
                    }

                    sb.Append(">\n");
                    sb.Append("> AssetList:\n>\n");
                    foreach (var asset in bundle.Assets)
                    {
                        sb.Append($"> * {asset.AssetName} - {asset.AssetPath}\n");
                    }

                    sb.Append('\n');
                }
            }
        }

        private static string FormatSize(ulong bytes)
        {
            if (bytes < 1024)
            {
                return $"{bytes}B";
            }

            if (bytes < 1024 * 1024)
            {
                return $"{bytes / 1024f:F1}KiB";
            }

            if (bytes < 1024 * 1024 * 1024)
            {
                return $"{bytes / 1024f / 1024f:F1}MiB";
            }

            return $"{bytes / 1024f / 1024f / 1024f:F1}GiB";
        }
    }

    public class AssetPackInfo
    {
        public string PackName;
        public List<BundleInfo> Bundles;
    }

    public class BundleInfo
    {
        public string BundleName;
        public string[] Dependencies;
        public AssetInfo[] Assets;
    }

    public class AssetInfo
    {
        public string AssetName;
        public string AssetPath;
    }
}
