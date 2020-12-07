// == Inspoy Technology ==
// Assembly: Instech.Framework.Common.Editor
// FileName: MenuItems.cs
// Created on 2019/12/06 by inspoy
// All rights reserved.

using UnityEditor;

namespace Instech.Framework.Common.Editor
{
    public static class MenuItems
    {
        [MenuItem("Instech/Common/创建ProjectSettings文件", false, 1101)]
        private static void CreateProjectSettingsAsset()
        {
            ProjectSettings.CreateNewAsset();
        }

        [MenuItem("Instech/Common/定位到ProjectSettings文件", true)]
        private static bool FocusOnProjectSettingsAssetValidation()
        {
            return ProjectSettings.AssetExists();
        }
        
        [MenuItem("Instech/Common/定位到ProjectSettings文件", false, 1102)]
        private static void FocusOnProjectSettingsAsset()
        {
            ProjectSettings.FocusOnAsset();
        }
    }
}
