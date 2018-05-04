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
        public static void FindAllReferences()
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
        /// 功能测试
        /// </summary>
        [MenuItem("Instech/功能测试")]
        private static void FunctionTest()
        {
            MiscEditor.FunctionTest();
        }
    }
}
