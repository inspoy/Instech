/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.AssetHelper.Editor
 * FileName: AssetBuildReport.cs
 * Created on 2020/08/24 by inspoy
 * All rights reserved.
 */

using System.Collections.Generic;

namespace Instech.Framework.AssetHelper.Editor
{
    public class AssetBuildReport
    {
        public bool IsSuccessful;
        public float CostTime;
        public AssetPackInfo[] AssetPacks;
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
