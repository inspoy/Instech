// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui.Editor
// FileName: MenuItems.cs
// Created on 2019/12/14 by inspoy
// All rights reserved.

using UnityEditor;

namespace Instech.Framework.Ui.Editor
{
    public static class MenuItems
    {
        [MenuItem("Assets/Instech/导出UI", true)]
        [MenuItem("Assets/Instech/检查RaycastTarget", true)]
        private static bool EnsureSelectingUiView()
        {
            return UiCodeGenerator.EnsureSelectingUiPrefab();
        }

        [MenuItem("Assets/Instech/导出UI", false, 2001)]
        private static void GenerateCode()
        {
            UiCodeGenerator.DoMenuGenerate();
        }

        [MenuItem("Assets/Instech/检查RaycastTarget", false, 2002)]
        private static void CheckRaycastTarget()
        {
            UiViewValidator.CheckRaycastTarget();
        }
    }
}
