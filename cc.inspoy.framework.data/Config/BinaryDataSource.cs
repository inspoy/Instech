/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Data
 * FileName: BinaryDataSource.cs
 * Created on 2019/12/15 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Instech.EncryptHelper;
using Instech.Framework.Utils;
using UnityEngine;

namespace Instech.Framework.Data
{
    /// <inheritdoc />
    internal class BinaryDataSource : IConfigDataSource
    {
        private readonly Dictionary<string, BinaryData> _data = new Dictionary<string, BinaryData>();

        public void Init(string src = null, byte[] encryptKey = null)
        {
            if (string.IsNullOrEmpty(src))
            {
                src = Utility.ResourcesPath + "conf.bin";
            }
            if (!File.Exists(src))
            {
                throw new ConfigException("conf.bin does NOT exists");
            }
            Byte[] bytes;
            if (encryptKey == null)
            {
                bytes = File.ReadAllBytes(src);
            }
            else
            {
                var encrypted = File.ReadAllBytes(src);
                var aes = new Aes();
                aes.Init(encryptKey);
                bytes = aes.Decrypt(encrypted);
            }
            var ms = new MemoryStream(GzipHelper.UnCompressToBytes(bytes));
            var reader = new BinaryReader(ms);
            try
            {
                var binStructure = new ConfigStructure();
                binStructure.Read(reader);
                foreach (var item in binStructure.TableList)
                {
                    _data.Add(item.TableName, new BinaryData(item));
                }
            }
            catch (Exception e)
            {
                throw new ConfigException("读取配置表失败，msg=" + e);
            }
            finally
            {
                ms.Dispose();
                reader.Dispose();
            }
        }

        public void Deinit()
        {
            // do nothing
        }

        public IConfigData GetData(string tableName)
        {
            _data.TryGetValue(tableName, out var ret);
            return ret;
        }
    }

    /// <summary>
    /// 二进制数据项，单行
    /// </summary>
    public class BinaryData : IConfigData
    {
        private readonly Dictionary<string, int> _dictFiledNameToIdx = new Dictionary<string, int>();
        private readonly List<ConfigStructure.RowItem> _rows = new List<ConfigStructure.RowItem>();
        private readonly IEnumerator<ConfigStructure.RowItem> _enumerator;
        private readonly string _tableName;

        internal BinaryData(ConfigStructure.TableItem tableItem)
        {
            _tableName = tableItem.TableName;
            // 构建列名到索引的映射
            for (var i = 0; i < tableItem.ColumnName.Count; i++)
            {
                var item = tableItem.ColumnName[i];
                _dictFiledNameToIdx[item] = i;
            }
            foreach (var item in tableItem.RowData)
            {
                _rows.Add(item);
            }
            _enumerator = _rows.GetEnumerator();
            if (!_enumerator.MoveNext())
            {
                throw new ConfigException("表中一条数据都没有", _tableName);
            }
        }

        public bool Next()
        {
            return _enumerator.MoveNext();
        }

        public int GetInt(string field)
        {
            var idx = _dictFiledNameToIdx[field];
            if (_enumerator.Current != null)
            {
                var rawVal = _enumerator.Current.Fields[idx];
                if (string.IsNullOrWhiteSpace(rawVal))
                {
                    // 尝试获取一个空的整数，返回0
                    return 0;
                }
                if (!int.TryParse(rawVal, out var val))
                {
                    throw new ConfigException("无法转为整数值:" + rawVal, _tableName);
                }
                return val;
            }
            throw new ConfigException("数据不存在", _tableName);
        }

        public float GetFloat(string field)
        {
            var idx = _dictFiledNameToIdx[field];
            if (_enumerator.Current != null)
            {
                var rawVal = _enumerator.Current.Fields[idx];
                if (string.IsNullOrWhiteSpace(rawVal))
                {
                    // 尝试获取一个空值，返回0
                    return 0;
                }
                if (!float.TryParse(rawVal, out var val))
                {
                    throw new ConfigException("无法转为浮点数值:" + rawVal, _tableName);
                }
                return val;
            }
            throw new ConfigException("数据不存在", _tableName);
        }

        public string GetString(string field)
        {
            var idx = _dictFiledNameToIdx[field];
            if (_enumerator.Current != null)
            {
                return _enumerator.Current.Fields[idx];
            }
            throw new ConfigException("数据不存在", _tableName);
        }

        public void Close()
        {
            // do nothing
        }
    }
}
