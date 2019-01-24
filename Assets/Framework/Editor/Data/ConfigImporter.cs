/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: ConfigImporter.cs
 * Created on 2018/11/15 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

namespace Instech.Framework.Editor
{
    /// <summary>
    /// 配置表导入
    /// </summary>
    public static class ConfigImporter
    {
        /// <summary>
        /// 从xlsx导入
        /// </summary>
        /// <param name="genCode">生成Config代码</param>
        /// <param name="genBin">生成二进制文件</param>
        /// <param name="silent">安静模式，不弹出对话框</param>
        public static void ImportFromExcelToBinary(bool genCode, bool genBin, bool silent = false)
        {
            var src = EditorPrefs.GetString(PrefWindow.XlsPath, string.Empty);
            var dst = Application.streamingAssetsPath + "/conf.bin";
            if (string.IsNullOrEmpty(src))
            {
                const string errMsg = "需要在Preference里配置XLS配置表的路径";
                ShowInfo(errMsg, true, silent);
                return;
            }
            if (!Directory.Exists(src))
            {
                const string errMsg = "XLS配置表的路径不存在，请检查";
                ShowInfo(errMsg, true, silent);
                return;
            }
            var allFiles = Directory.GetFiles(src, "*.xlsx", SearchOption.TopDirectoryOnly);
            ConfigStructure data = null;
            if (genBin)
            {
                data = new ConfigStructure();
            }
            for (var i = 0; i < allFiles.Length; i++)
            {
                var filePath = allFiles[i];
                var tableName = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrEmpty(tableName))
                {
                    continue;
                }
                if (tableName.StartsWith("_") || tableName.StartsWith("~"))
                {
                    continue;
                }
                Logger.LogInfo(LogModule.Data, $"找到配置文件{tableName}");
                try
                {
                    if (genBin)
                    {
                        ReadSingleFile(data, filePath, (ushort)i);
                    }
                    if (genCode)
                    {
                        GenSingleFile(filePath);
                    }
                }
                catch (Exception e)
                {
                    var errMsg = $"导出{tableName}失败，原因{e.Message}，操作已取消";
                    ShowInfo(errMsg, true, silent);
                    Logger.LogInfo(LogModule.Data, e.ToString());
                    return;
                }
            }
            if (genBin)
            {
                try
                {
                    WriteToFile(data, dst);
                }
                catch (Exception e)
                {
                    var errMsg = $"写入config.bin文件失败，原因:{e.Message}，操作已取消";
                    if (silent)
                    {
                        Logger.LogError(LogModule.Editor, errMsg);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", errMsg, "OK");
                    }
                    Logger.LogInfo(LogModule.Data, e.ToString());
                    return;
                }
            }
            const string msg = "操作成功完成";
            ShowInfo(msg, false, silent);
        }

        /// <summary>
        /// 从单一xls文件中读取数据并写入ConfigStructure
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filePath"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        private static void ReadSingleFile(ConfigStructure data, string filePath, ushort idx)
        {
            var tableItem = new ConfigStructure.TableItem
            {
                TableName = Path.GetFileNameWithoutExtension(filePath),
                TableIdx = idx
            };
            var package = new ExcelPackage(new FileInfo(filePath));
            var ws = package.Workbook.Worksheets["Data"];
            if (ws == null)
            {
                throw new InvalidOperationException("xls文件中没有 'Data' Sheet页");
            }
            var columnIdx = 1;
            while (true)
            {
                var fieldName = ws.Cells[2, columnIdx].Value?.ToString();
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    break;
                }
                tableItem.ColumnCount += 1;
                tableItem.ColumnName.Add(fieldName);
                columnIdx += 1;
            }
            var curLine = 4; // 数据从第四行开始
            while (true)
            {
                var rowItem = new ConfigStructure.RowItem();
                for (var col = 1; col <= tableItem.ColumnCount; ++col)
                {
                    var fieldData = ws.Cells[curLine, col].Value?.ToString();
                    rowItem.Fields.Add(fieldData ?? string.Empty);
                }
                tableItem.RowData.Add(rowItem);
                tableItem.RowCount += 1;
                curLine += 1;
                if (string.IsNullOrWhiteSpace(ws.Cells[curLine, 1].Value?.ToString()))
                {
                    break;
                }
            }
            data.TableList.Add(tableItem);
            data.TableCount += 1;
        }

        /// <summary>
        /// 写入到文件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dstPath"></param>
        private static void WriteToFile(IConfigStructureUnit data, string dstPath)
        {
            using (var writer = new BinaryWriter(new FileStream(dstPath, FileMode.Create)))
            {
                var byteCount = data.Write(writer);
                Logger.LogInfo(LogModule.Data, $"写入大小：{byteCount}字节");
            }
        }

        /// <summary>
        /// 从单一xlsx文件中获取数据结构并生成Config代码
        /// </summary>
        /// <param name="filePath"></param>
        private static void GenSingleFile(string filePath)
        {
            var package = new ExcelPackage(new FileInfo(filePath));
            var ws = package.Workbook.Worksheets["Data"];
            if (ws == null)
            {
                throw new InvalidOperationException("xls文件中没有 'Data' Sheet页");
            }
            var templatePath = Path.Combine(Application.dataPath, "Framework/Editor/Data/ConfigTemplate.txt");
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("模板文件不存在: " + templatePath);
            }
            var tableName = Path.GetFileNameWithoutExtension(filePath);
            var fieldDeclare = new StringBuilder();
            var fieldInit = new StringBuilder();
            var hash = new List<string>();
            var columnIdx = 0;
            while (true)
            {
                columnIdx += 1;
                var fieldName = ws.Cells[2, columnIdx].Value?.ToString();
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    break;
                }
                var fieldType = ws.Cells[3, columnIdx].Value?.ToString();
                var fieldDesc = ws.Cells[1, columnIdx].Value?.ToString();
                hash.Add(fieldName);
                hash.Add(fieldType);
                hash.Add(fieldDesc);
                BuildFieldCode(fieldDeclare, fieldInit, fieldName, fieldType, fieldDesc);
            }
            var dst = Path.Combine(Application.dataPath, $"Scripts/Config/{tableName}Config.cs");
            var customCode = new StringBuilder();
            var oldHash = string.Empty;
            if (File.Exists(dst))
            {
                var oldContent = File.ReadAllLines(dst);
                var reading = false;
                foreach (var line in oldContent)
                {
                    if (line.StartsWith("    /// Hash:"))
                    {
                        oldHash = line.Replace("    /// Hash:", "").Trim();
                    }
                    if (line.Trim().StartsWith("#region ====="))
                    {
                        reading = true;
                        continue;
                    }
                    if (line.Trim().StartsWith("#endregion ====="))
                    {
                        reading = false;
                        continue;
                    }
                    if (!reading) continue;
                    customCode.Append(line);
                    customCode.Append('\n');
                }
            }

            var hashString = hash.CalcHash();
            if (hashString == oldHash)
            {
                // 表结构没变化
                Logger.LogInfo(LogModule.Data, $"{tableName}没有变化已跳过");
                return;
            }
            // 删去最后一个换行符
            if (fieldDeclare.Length > 0)
            {
                fieldDeclare.Remove(fieldDeclare.Length - 1, 1);
            }
            if (fieldInit.Length > 0)
            {
                fieldInit.Remove(fieldInit.Length - 1, 1);
            }
            if (customCode.Length > 0)
            {
                customCode.Remove(customCode.Length - 1, 1);
            }

            var template = File.ReadAllText(templatePath);
            template = template.Replace("#Hash#", hashString);
            template = template.Replace("#GenTime#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            template = template.Replace("#TableName#", tableName);
            template = template.Replace("#FieldDeclare#", fieldDeclare.ToString());
            template = template.Replace("#FieldInit#", fieldInit.ToString());
            template = template.Replace("#CustomCode#", customCode.ToString());
            File.WriteAllText(dst, template, Encoding.UTF8);
            ScriptHeaderGenerator.OnWillCreateAsset(dst + ".meta");
        }

        private static void BuildFieldCode(StringBuilder fieldDeclare, StringBuilder fieldInit, string fieldName, string fieldType, string fieldDesc)
        {
            if (fieldName.Equals("Id"))
            {
                // Id字段不自动生成
                return;
            }
            if (string.IsNullOrEmpty(fieldType))
            {
                throw new InvalidOperationException($"字段类型不能是空，fieldName={fieldName}");
            }
            if (!fieldType.Equals("int") && !fieldType.Equals("ints") && !fieldType.Equals("text") && !fieldType.Equals("texts"))
            {
                throw new NotSupportedException("不支持的字段类型: " + fieldType);
            }
            if (string.IsNullOrEmpty(fieldDesc))
            {
                fieldDesc = string.Empty;
            }
            var descLines = fieldDesc.Split('\n');
            fieldDeclare.Append("        /// <summary>\n");
            for (var i = 0; i < descLines.Length; i++)
            {
                var descLine = descLines[i];
                fieldDeclare.Append($"        /// {descLine}");
                if (i != descLines.Length - 1)
                {
                    fieldDeclare.Append("<br />");
                }
                fieldDeclare.Append('\n');
            }

            fieldDeclare.Append("        /// </summary>\n");
            fieldDeclare.Append("        ");
            switch (fieldType)
            {
                case "int":
                    fieldDeclare.Append($"public int {fieldName} {{ get; private set; }}\n");
                    break;
                case "ints":
                    fieldDeclare.Append($"public int[] {fieldName} {{ get; private set; }} = EmptyIntArray;\n");
                    break;
                case "text":
                    fieldDeclare.Append($"public string {fieldName} {{ get; private set; }} = string.Empty;\n");
                    break;
                case "texts":
                    fieldDeclare.Append($"public string[] {fieldName} {{ get; private set; }} = EmptyStringArray;\n");
                    break;
                default:
                    throw new NotSupportedException();
            }
            fieldDeclare.Append('\n');

            switch (fieldType)
            {
                case "int":
                    fieldInit.Append($"            {fieldName} = data.GetInt(\"{fieldName}\");\n");
                    break;
                case "ints":
                    fieldInit.Append($"            var raw{fieldName} = data.GetString(\"{fieldName}\");\n");
                    fieldInit.Append($"            {fieldName} = Utility.SplitToInt(raw{fieldName});\n");
                    break;
                case "text":
                    fieldInit.Append($"            {fieldName} = data.GetString(\"{fieldName}\");\n");
                    break;
                case "texts":
                    fieldInit.Append($"            var raw{fieldName} = data.GetString(\"{fieldName}\");\n");
                    fieldInit.Append($"            {fieldName} = Utility.SplitToString(raw{fieldName});\n");
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private static void ShowInfo(string msg, bool isError, bool silent)
        {
            if (isError)
            {
                if (silent)
                {
                    Logger.LogError(LogModule.Editor, msg);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", msg, "OK");
                }
            }
            else
            {
                if (silent)
                {
                    Logger.LogInfo(LogModule.Editor, msg);
                }
                else
                {
                    EditorUtility.DisplayDialog("成功", msg, "OK");
                }
            }
        }
    }
}
