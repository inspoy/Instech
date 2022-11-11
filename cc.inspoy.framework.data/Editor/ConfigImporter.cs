// == Inspoy Technology ==
// Assembly: Instech.Framework.Data.Editor
// FileName: ConfigImporter.cs
// Created on 2019/12/15 by inspoy
// All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Instech.EncryptHelper;
using Instech.Framework.Common;
using Instech.Framework.Common.Editor;
using Instech.Framework.Logging;
using Instech.Framework.Utils;
using Instech.Framework.Utils.Editor;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Data.Editor
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
        /// <param name="aesKey">AES加密密钥</param>
        /// <param name="silent">安静模式，不弹出对话框，用于打包</param>
        public static bool ImportFromExcelToBinary(bool genCode, bool genBin, byte[] aesKey = null, bool silent = false)
        {
            var src = Application.dataPath + ProjectSettings.Instance.ExcelDataPath;
            var dst = PathHelper.ResourceDataPath;
            if (!Directory.Exists(dst))
            {
                Directory.CreateDirectory(dst);
            }
            dst += "conf.bin";
            if (string.IsNullOrEmpty(src))
            {
                var errMsg = $"需要在{ProjectSettings.SavePath}里配置XLS配置表的路径";
                ShowInfo(errMsg, true, silent);
                return false;
            }
            if (!Directory.Exists(src))
            {
                const string errMsg = "XLS配置表的路径不存在，请检查";
                ShowInfo(errMsg, true, silent);
                return false;
            }
            var allFiles = Directory.GetFiles(src, "*.xlsx", SearchOption.TopDirectoryOnly);
            var data = genBin ? new ConfigStructure() : null;
            for (var i = 0; i < allFiles.Length; i++)
            {
                if (!silent)
                {
                    EditorUtility.DisplayProgressBar("导出配置", "正在导出配置，请稍后...", 1f * i / allFiles.Length);
                }
                var filePath = allFiles[i];
                var tableName = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrEmpty(tableName) || tableName.StartsWith("_") || tableName.StartsWith("~"))
                {
                    continue;
                }
                Logger.LogInfo(LogModule.Data, $"找到配置文件{tableName}");
                if (!GenSingleTable(genCode, genBin, silent, data, filePath, i, tableName))
                {
                    return false;
                }
            }

            if (!silent)
            {
                EditorUtility.ClearProgressBar();
            }
            if (genBin)
            {
                try
                {
                    WriteToFile(data, dst, aesKey);
                }
                catch (Exception e)
                {
                    var errMsg = $"写入config.bin文件失败，原因:{e.Message}，操作已取消";
                    if (silent)
                    {
                        Logger.LogError(LogModule.Data, errMsg);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", errMsg, "OK");
                    }
                    Logger.LogInfo(LogModule.Data, e.ToString());
                    return false;
                }
            }
            ShowInfo("操作成功完成", false, silent);
            return true;
        }

        private static bool GenSingleTable(bool genCode, bool genBin, bool silent, ConfigStructure data, string filePath, int tableIdx, string tableName)
        {
            try
            {
                if (genBin)
                {
                    ReadSingleFile(data, filePath, (ushort)tableIdx);
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
                if (!silent)
                {
                    EditorUtility.ClearProgressBar();
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 从单一xls文件中读取数据并写入ConfigStructure
        /// </summary>
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
                columnIdx += 1;
                if (fieldName.StartsWith("#"))
                {
                    // 忽略列
                    continue;
                }
                tableItem.ColumnCount += 1;
                tableItem.ColumnName.Add(fieldName);
            }
            var curLine = 4; // 数据从第四行开始
            while (true)
            {
                var nextId = ws.Cells[curLine, 1].Value?.ToString();
                if (nextId != null && nextId.StartsWith("#"))
                {
                    // 忽略行
                    continue;
                }
                var rowItem = new ConfigStructure.RowItem();
                for (var col = 1; col <= tableItem.ColumnCount; ++col)
                {
                    var fieldName = ws.Cells[2, col].Value?.ToString();
                    if (fieldName == null || fieldName.StartsWith("#"))
                    {
                        // 忽略列
                        continue;
                    }
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
        /// 配置字段代码生成
        /// </summary>
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
            if (!fieldType.Equals("int") && !fieldType.Equals("ints") &&
                !fieldType.Equals("float") && !fieldType.Equals("floats") &&
                !fieldType.Equals("text") && !fieldType.Equals("texts"))
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
                    fieldInit.Append($"            {fieldName} = data.GetInt(\"{fieldName}\");\n");
                    break;
                case "ints":
                    fieldDeclare.Append($"public int[] {fieldName} {{ get; private set; }} = EmptyIntArray;\n");
                    fieldInit.Append($"            var raw{fieldName} = data.GetString(\"{fieldName}\");\n");
                    fieldInit.Append($"            {fieldName} = Utility.SplitToInt(raw{fieldName});\n");
                    break;
                case "float":
                    fieldDeclare.Append($"public float {fieldName} {{ get; private set; }}\n");
                    fieldInit.Append($"            {fieldName} = data.GetFloat(\"{fieldName}\");\n");
                    break;
                case "floats":
                    fieldDeclare.Append($"public float[] {fieldName} {{ get; private set; }} = EmptyFloatArray;\n");
                    fieldInit.Append($"            var raw{fieldName} = data.GetString(\"{fieldName}\");\n");
                    fieldInit.Append($"            {fieldName} = Utility.SplitToFloat(raw{fieldName});\n");
                    break;
                case "text":
                    fieldDeclare.Append($"public string {fieldName} {{ get; private set; }} = string.Empty;\n");
                    fieldInit.Append($"            {fieldName} = data.GetString(\"{fieldName}\");\n");
                    break;
                case "texts":
                    fieldDeclare.Append($"public string[] {fieldName} {{ get; private set; }} = EmptyStringArray;\n");
                    fieldInit.Append($"            var raw{fieldName} = data.GetString(\"{fieldName}\");\n");
                    fieldInit.Append($"            {fieldName} = Utility.SplitToString(raw{fieldName});\n");
                    break;
                default:
                    throw new NotSupportedException();
            }
            fieldDeclare.Append('\n');
        }

        /// <summary>
        /// 从单一xlsx文件中获取数据结构并生成Config代码
        /// </summary>
        private static void GenSingleFile(string filePath)
        {
            var package = new ExcelPackage(new FileInfo(filePath));
            var ws = package.Workbook.Worksheets["Data"];
            if (ws == null)
            {
                throw new InvalidOperationException("xls文件中没有 'Data' Sheet页");
            }
            var packageRoot = ProjectSettings.GetPackageFullPath("cc.inspoy.framework.data");
            var templatePath = Path.Combine(packageRoot, "Editor/ConfigTemplate.txt");
            var extTemplatePath = Path.Combine(packageRoot, "Editor/ConfigExtTemplate.txt");
            if (!File.Exists(templatePath) || !File.Exists(extTemplatePath))
            {
                throw new FileNotFoundException("模板文件不存在:\n" + templatePath + "\n" + extTemplatePath);
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
                if (fieldName.StartsWith("#"))
                {
                    // 忽略列
                    continue;
                }
                var fieldType = ws.Cells[3, columnIdx].Value?.ToString();
                var fieldDesc = ws.Cells[1, columnIdx].Value?.ToString();
                hash.Add(fieldName);
                hash.Add(fieldType);
                hash.Add(fieldDesc);
                BuildFieldCode(fieldDeclare, fieldInit, fieldName, fieldType, fieldDesc);
            }
            var dst = Path.Combine(Application.dataPath, $"Scripts/Config/{tableName}Config.cs");
            var extDst = Path.Combine(Application.dataPath, $"Scripts/ConfigExtension/{tableName}Config.Ext.cs");
            var oldHash = string.Empty;
            if (File.Exists(dst))
            {
                var oldContent = File.ReadAllLines(dst);
                foreach (var line in oldContent)
                {
                    if (line.StartsWith("    /// Hash:"))
                    {
                        oldHash = line.Replace("    /// Hash:", "").Trim();
                        break;
                    }
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

            var template = File.ReadAllText(templatePath);
            template = template.Replace("#Hash#", hashString);
            template = template.Replace("#GenTime#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            template = template.Replace("#TableName#", tableName);
            template = template.Replace("#FieldDeclare#", fieldDeclare.ToString());
            template = template.Replace("#FieldInit#", fieldInit.ToString());
            File.WriteAllText(dst, template, Encoding.UTF8);
            ScriptHeaderGenerator.OnWillCreateAsset(dst + ".meta");

            if (!File.Exists(extDst))
            {
                // ext文件只会创建一次
                template = File.ReadAllText(extTemplatePath);
                template = template.Replace("#TableName#", tableName);
                File.WriteAllText(extDst, template, Encoding.UTF8);
                ScriptHeaderGenerator.OnWillCreateAsset(extDst + ".meta");
            }
            
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 写入二进制配置到文件
        /// </summary>
        private static void WriteToFile(ConfigStructure data, string dstPath, byte[] aesKey)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            var byteCount = data.Write(writer);
            Logger.LogInfo(LogModule.Data, $"数据大小：{byteCount}字节");
            var origin = ms.ToArray();
            ms.Dispose();
            writer.Dispose();
            origin = GzipHelper.Compress(origin);
            var encryted = origin;
            if (aesKey != null)
            {
                Logger.LogInfo(LogModule.Data, $"密钥长度：{aesKey.Length}");
                var aes = new Aes();
                aes.Init(aesKey);
                encryted = aes.Encrypt(origin);
            }
            Logger.LogInfo(LogModule.Data, $"压缩后实际写入大小({(aesKey == null ? "未加密" : "加密")})：{encryted.Length}字节");
            File.WriteAllBytes(dstPath, encryted);
        }

        private static void ShowInfo(string msg, bool isError, bool silent)
        {
            if (isError)
            {
                if (silent)
                {
                    Logger.LogError(LogModule.Data, msg);
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
                    Logger.LogInfo(LogModule.Data, msg);
                }
                else
                {
                    EditorUtility.DisplayDialog("成功", msg, "OK");
                }
            }
        }
    }
}
