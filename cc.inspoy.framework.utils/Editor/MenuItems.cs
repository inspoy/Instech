// == Inspoy Technology ==
// Assembly: Instech.Framework.Utils.Editor
// FileName: MenuItems.cs
// Created on 2019/12/06 by inspoy
// All rights reserved.

using Unity.CodeEditor;
using UnityEditor;

namespace Instech.Framework.Utils.Editor
{
    public static class MenuItems
    {
        /// <summary>
        /// [验证有效性]在控制台输出选定资源的InstanceID
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Instech/查看InstanceId", true)]
        private static bool ShowInstanceIdValidation()
        {
            return MiscEditor.ShowInstanceIdValidation();
        }

        /// <summary>
        /// 在控制台输出选定资源的InstanceID
        /// </summary>
        [MenuItem("Assets/Instech/查看InstanceId", false, 1001)]
        private static void ShowInstanceId()
        {
            MiscEditor.ShowInstanceId();
        }

        /// <summary>
        /// 查找选定资产的所有引用
        /// </summary>
        [MenuItem("Assets/Instech/查找资产的所有引用", false, 1002)]
        private static void FindAllReferences()
        {
            FindAllReferencesHelper.FindAllReferences();
        }

        /// <summary>
        /// 复制选中资产的路径到剪贴板
        /// </summary>
        [MenuItem("Assets/Instech/复制选中资产的路径到剪贴板", false, 1003)]
        private static void CopyAssetPath()
        {
            MiscEditor.CopyAssetPathToClipboard();
        }

        [MenuItem("Instech/Utils/打开日志文件夹", false, 5101)]
        private static void OpenLogFolder()
        {
            MiscEditor.OpenLogFolder();
        }

        [MenuItem("Instech/Utils/打开Crash文件夹", false, 5102)]
        private static void OpenCrashFolder()
        {
            MiscEditor.OpenCrashFolder();
        }

        [MenuItem("Instech/Utils/切换GameObject的激活状态 _F1", false, 5103)]
        private static void ToggleGameObjectActive()
        {
            MiscEditor.ToggleGameObjectActive();
        }

        [MenuItem("Instech/Utils/检查所有脚本的头部注释", false, 5104)]
        private static void CheckScriptHeader()
        {
            MiscEditor.CheckScriptHeader();
        }

        [MenuItem("Instech/Utils/生成cs工程文件", false, 5105)]
        private static void SyncSolution()
        {
            CodeEditor.CurrentEditor.SyncAll();
        }

        [MenuItem("Instech/Utils/编辑预定义宏", false, 5106)]
        private static void OpenDefineSymbolEditor()
        {
            DefineSymbolEditor.Open();
        }

        /// <summary>
        /// 功能测试
        /// </summary>
        [MenuItem("Instech/Utils/功能测试", false, 5999)]
        private static void FunctionTest()
        {
            MiscEditor.FunctionTest();
        }
    }
}
