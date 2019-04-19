/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: MenuItems.cs
 * Created on 2018/05/01 by inspoy
 * All rights reserved.
 */

using UnityEditor;

namespace Instech.Framework.Editor
{
    /// <summary>
    /// 所有MenuItem的统一入口，避免多处实现
    /// </summary>
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
        [MenuItem("Assets/Instech/查看InstanceId")]
        private static void ShowInstanceId()
        {
            MiscEditor.ShowInstanceId();
        }

        /// <summary>
        /// 查找选定资产的所有引用
        /// </summary>
        [MenuItem("Assets/Instech/查找资产的所有引用")]
        private static void FindAllReferences()
        {
            FindAllReferencesHelper.FindAllReferences();
        }

        /// <summary>
        /// [验证有效性]导出UI代码
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Instech/导出UI", true)]
        private static bool ExportUiValidation()
        {
            return UiExporter.ExportUiValidation();
        }

        /// <summary>
        /// 导出UI代码
        /// </summary>
        [MenuItem("Assets/Instech/导出UI")]
        private static void ExportUi()
        {
            UiExporter.ExportUi();
        }

        /// <summary>
        /// 复制选中资产的路径到剪贴板
        /// </summary>
        [MenuItem("Assets/Instech/复制选中资产的路径到剪贴板")]
        private static void CopyAssetPath()
        {
            MiscEditor.CopyAssetPathToClipboard();
        }

        /// <summary>
        /// 把所有代码的编码修改为UTF-8
        /// </summary>
        [MenuItem("Instech/把所有代码的编码修改为UTF-8")]
        private static void ConvertToUtf8()
        {
            MiscEditor.ConvertToUtf8();
        }

        /// <summary>
        /// 打包AssetBundle
        /// </summary>
        [MenuItem("Instech/打包AssetBundle")]
        private static void BuildAssetBundles()
        {
            BuildAssetBundle.DoBuild(null);
        }

        /// <summary>
        /// 导入xlsx配置表
        /// </summary>
        [MenuItem("Instech/配置表/导入xlsx配置表")]
        private static void ImportConfig()
        {
            ConfigImporter.ImportFromExcelToBinary(false, true);
        }

        [MenuItem("Instech/配置表/生成Config代码")]
        private static void GenConfigCode()
        {
            ConfigImporter.ImportFromExcelToBinary(true, false);
        }

        [MenuItem("Instech/打开日志文件夹")]
        private static void OpenLogFolder()
        {
            MiscEditor.OpenLogFolder();
        }

        /// <summary>
        /// 功能测试
        /// </summary>
        [MenuItem("Instech/功能测试")]
        private static void FunctionTest()
        {
            MiscEditor.FunctionTest();
        }

        [MenuItem("Instech/创建新的ProjectSettings")]
        private static void CreateNewProjectSettings()
        {
            ProjectSettings.CreateNewAsset();
        }

        [MenuItem("Instech/生成Utf8Json的解析代码")]
        private static void GenerateUtf8JsonCode()
        {
            GenUtf8JsonCode.GenCode();
        }
    }
}
