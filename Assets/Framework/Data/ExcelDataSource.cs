/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ExcelData.cs
 * Created on 2018/08/14 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEngine;

namespace Instech.Framework
{
    /// <inheritdoc />
    internal class ExcelDataSource : IConfigDataSource
    {
        private string _basePath;
        public void Init(string src = null)
        {
            _basePath = src ?? $"{Application.dataPath}/../GameConfig/";
        }

        public void Uninit()
        {
            // do nothing
        }

        public IConfigData GetData(string tableName)
        {
            var xlsPath = $"{_basePath}{tableName}.xlsx";
            if (!File.Exists(xlsPath))
            {
                throw new ConfigException($"找不到excel文件:{xlsPath}", tableName);
            }
            return new ExcelData(xlsPath, tableName);
        }
    }

    /// <summary>
    /// Excel数据项
    /// </summary>
    public class ExcelData : IConfigData
    {
        public string TableName { get; }
        private ExcelPackage _package;
        private ExcelWorksheet _ws;
        private readonly Dictionary<string, int> _dictFieldIdx = new Dictionary<string, int>();
        private readonly string[] _curLine;
        private int _curLineNumber;
        internal ExcelData(string xlsPath, string tableName)
        {
            TableName = tableName;
            var fileInfo = new FileInfo(xlsPath);
            _package = new ExcelPackage(fileInfo);
            _ws = _package.Workbook.Worksheets["Data"];
            if (_ws == null)
            {
                throw new ConfigException("xls文件中没有 'Data' Sheet页", tableName);
            }
            var columnCount = 1;
            while (true)
            {
                var fieldName = _ws.Cells[2, columnCount].Value?.ToString();
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    break;
                }
                _dictFieldIdx[fieldName] = columnCount;
                ++columnCount;
            }
            _curLine = new string[columnCount - 1];
            _curLineNumber = 4; // 从第四行开始
            if (!Next())
            {
                throw new ConfigException("xls文件中一行数据都没有", tableName);
            }
        }

        ~ExcelData()
        {
            _ws?.Dispose();
            _package?.Dispose();
        }

        public bool Next()
        {
            try
            {
                int.TryParse(_ws.Cells[_curLineNumber, 1].Value?.ToString(), out var id);
                if (id == 0)
                {
                    return false;
                }
                for (var i = 0; i < _curLine.Length; ++i)
                {
                    var val = _ws.Cells[_curLineNumber, i + 1].Value;
                    _curLine[i] = val != null ? val.ToString() : "";
                }
                _curLineNumber += 1;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int GetInt(string field)
        {
            if (!_dictFieldIdx.TryGetValue(field, out var fieldIdx))
            {
                Logger.LogWarning(LogModule.Data, $"找不到字段名称:{field}");
                return 0;
            }
            if (!int.TryParse(_curLine[fieldIdx - 1], out var ret) && !string.IsNullOrWhiteSpace(_curLine[fieldIdx - 1]))
            {
                Logger.LogWarning(LogModule.Data, $"不能转换为整数:{_curLine[fieldIdx - 1]}");
            }
            return ret;
        }

        public string GetString(string field)
        {
            if (!_dictFieldIdx.TryGetValue(field, out var fieldIdx))
            {
                Logger.LogWarning(LogModule.Data, $"找不到字段名称:{field}");
                return string.Empty;
            }
            return _curLine[fieldIdx - 1];
        }

        public void Close()
        {
            _ws.Dispose();
            _ws = null;
            _package.Dispose();
            _package = null;
        }
    }
}
