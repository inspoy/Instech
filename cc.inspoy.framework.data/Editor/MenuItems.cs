/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Data.Editor
 * FileName: MenuItems.cs
 * Created on 2019/12/15 by inspoy
 * All rights reserved.
 */

using UnityEditor;

namespace Instech.Framework.Data.Editor
{
    public static class MenuItems
    {
        /// <summary>
        /// 导入xlsx配置表
        /// </summary>
        [MenuItem("Instech/Data/从xlsx配置表生成二进制文件", false, 3101)]
        private static void ImportConfig()
        {
            ConfigImporter.ImportFromExcelToBinary(false, true);
        }

        [MenuItem("Instech/Data/生成Config代码", false, 3102)]
        private static void GenConfigCode()
        {
            ConfigImporter.ImportFromExcelToBinary(true, false);
        }

        [MenuItem("Instech/Data/导出本地化json", false, 3201)]
        private static void ImportLocalization()
        {
            LocalizationImporter.GenerateJsonFromExcel();
        }
    }
}
