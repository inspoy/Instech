// == Inspoy Technology ==
// Assembly: Instech.Framework.Data.Editor
// FileName: LocalizationImporter.cs
// Created on 2020/07/01 by inspoy
// All rights reserved.

using System.Collections.Generic;
using System.IO;
using Instech.Framework.Common;
using Instech.Framework.Common.Editor;
using Instech.Framework.Logging;
using Instech.Framework.Utils;
using OfficeOpenXml;
using UnityEngine;
using JsonSerializer = Utf8Json.JsonSerializer;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Data.Editor
{
    public static class LocalizationImporter
    {
        /// <summary>
        /// 从Excel导出JSON
        /// </summary>
        /// <returns>是否成功执行</returns>
        public static bool GenerateJsonFromExcel()
        {
            var xlsxPath = Application.dataPath + ProjectSettings.Instance.ExcelDataPath + "_i18n.xlsx";
            var dstPath = PathHelper.ResourceDataPath + "Localization/";
            if (!File.Exists(xlsxPath))
            {
                Logger.LogError(LogModule.Build, "Cannot find _i18n.xlsx");
                return false;
            }

            if (!Directory.Exists(dstPath))
            {
                Logger.LogError(LogModule.Build, "Cannot find target folder: " + dstPath);
                return false;
            }

            var languages = ReadExcelDocument(xlsxPath);
            if (languages == null)
            {
                return false;
            }

            foreach (var item in languages)
            {
                WriteJson(dstPath + item.Meta.FileName, item);
                Logger.LogInfo(LogModule.Build, $"Generated {item.Meta.FileName}");
            }

            return true;
        }

        private static List<LocalizationData> ReadExcelDocument(string xlsxPath)
        {
            var package = new ExcelPackage(new FileInfo(xlsxPath));
            var sheet = package.Workbook.Worksheets[package.Compatibility.IsWorksheets1Based ? 1 : 0];
            if (sheet == null)
            {
                Logger.LogError(LogModule.Build, "invalid worksheet");
                return null;
            }

            var ret = new List<LocalizationData>();
            var cols = 0;
            while (true)
            {
                var langId = sheet.Cells[1, cols + 2].Value?.ToString();
                if (string.IsNullOrWhiteSpace(langId))
                {
                    break;
                }

                var data = new LocalizationData
                {
                    Version = 1,
                    Meta = new LocalizationData.MetaData(),
                    Data = new Dictionary<string, string>()
                };
                data.Meta.LanguageId = langId;
                data.Meta.FileName = sheet.Cells[2, cols + 2].Value?.ToString() ?? string.Empty;
                data.Meta.DisplayName = sheet.Cells[3, cols + 2].Value?.ToString() ?? string.Empty;
                data.Meta.Authors = Utility.SplitToString(sheet.Cells[4, cols + 2].Value?.ToString() ?? string.Empty);
                data.Meta.Description = sheet.Cells[5, cols + 2].Value?.ToString() ?? string.Empty;
                ret.Add(data);
                cols += 1;
            }

            var curLine = 6; // 前五行都是MetaData
            while (true)
            {
                var key = sheet.Cells[curLine, 1].Value?.ToString();
                if (string.IsNullOrWhiteSpace(key))
                {
                    break;
                }

                if (!key.StartsWith("__"))
                {
                    for (var i = 0; i < cols; ++i)
                    {
                        var val = sheet.Cells[curLine, i + 2].Value?.ToString() ?? string.Empty;
                        val = val.Replace("\\n", "\n");
                        if (!string.IsNullOrEmpty(val))
                        {
                            ret[i].Data.Add(key, val);
                        }
                    }
                }

                curLine += 1;
            }

            return ret;
        }

        private static void WriteJson(string targetPath, LocalizationData data)
        {
            var json = JsonSerializer.ToJsonString(data);
            File.WriteAllText(targetPath, JsonSerializer.PrettyPrint(json));
        }
    }
}
