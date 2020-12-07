// == Inspoy Technology ==
// Assembly: Instech.Framework.AssetHelper.Editor
// FileName: MenuItems.cs
// Created on 2019/12/24 by inspoy
// All rights reserved.

using UnityEditor;

namespace Instech.Framework.AssetHelper.Editor
{
    public static class MenuItems
    {
        [MenuItem("Instech/AssetHelper/生成 AssetBundle", false, 2101)]
        private static void BuildAssetBundles()
        {
            AssetBuilder.BuildAssetBundle(false, false);
        }

        [MenuItem("Instech/AssetHelper/重新生成 AssetBundle", false, 2102)]
        private static void RebuildAssetBundles()
        {
            AssetBuilder.BuildAssetBundle(true, false);
        }

        [MenuItem("Instech/AssetHelper/创建AssetPack配置文件", false, 2201)]
        private static void CreateAssetPackerSpecifierAsset()
        {
            AssetPackSpecifier.CreateNewAsset();
        }

        [MenuItem("Instech/AssetHelper/创建AssetPack配置文件", true)]
        private static bool CreateAssetPackerSpecifierAssetValidation()
        {
            return !AssetPackSpecifier.AssetExists();
        }

        [MenuItem("Instech/AssetHelper/定位到AssetPack配置文件", false, 2202)]
        private static void FocusOnAssetPackerSpecifierAsset()
        {
            AssetPackSpecifier.FocusOnAsset();
        }

        [MenuItem("Instech/AssetHelper/定位到AssetPack配置文件", true)]
        private static bool FocusOnAssetPackerSpecifierAssetValidation()
        {
            return AssetPackSpecifier.AssetExists();
        }
    }
}
