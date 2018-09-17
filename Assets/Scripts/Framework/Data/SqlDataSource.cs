/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: SqlData.cs
 * Created on 2018/08/14 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using UnityEngine;

namespace Instech.Framework
{
    /// <summary>
    /// SQLite数据源
    /// </summary>
    public class SqlDataSource : IConfigDataSource
    {
        private SqlHelper _sqlClient;

        public void Init()
        {
            _sqlClient = new SqlHelper(Application.streamingAssetsPath + "/conf.db");
        }

        public void FinishInit()
        {
            _sqlClient.Dispose();
            _sqlClient = null;
        }

        public void Uninit()
        {
            _sqlClient?.Dispose();
        }

        public IConfigData GetData(string tableName)
        {
            var sql = $"SELECT * FROM `{tableName}`";
            var reader = _sqlClient.Query(sql);
            reader.Read();
            if (!reader.HasRows)
            {
                Logger.LogWarning(LogModule.Data, "查询不到记录：" + sql);
            }
            return new SqlData(reader, sql);
        }
    }

    /// <summary>
    /// SQLite数据项
    /// </summary>
    public class SqlData : IConfigData
    {
        public string QueryString { get; }
        private readonly SQLiteDataReader _reader;
        private bool _closed;
        private readonly Dictionary<string, int> _dictFieldId;

        public SqlData(SQLiteDataReader reader, string sql)
        {
            _reader = reader;
            QueryString = sql;
            _dictFieldId = new Dictionary<string, int>();
        }

        ~SqlData()
        {
            if (!_closed)
            {
                _reader.Close();
            }
        }

        public void Close()
        {
            _reader.Close();
            _closed = true;
        }

        /// <summary>
        /// 读取下一条记录
        /// </summary>
        /// <returns>返回false表示已经没有下一条记录了</returns>
        public bool Next()
        {
            if (_reader == null)
            {
                return false;
            }
            return _reader.Read();
        }

        /// <summary>
        /// 读取一个整数
        /// </summary>
        /// <param name="field">字段名</param>
        /// <returns>整数值</returns>
        public int GetInt(string field)
        {
            if (_reader == null)
            {
                return 0;
            }
            if (!_reader.HasRows)
            {
                return 0;
            }
            try
            {
                return _reader.GetInt32(GetFieldId(field));
            }
            catch (Exception e)
            {
                Logger.LogError(LogModule.Data, "数据类型不匹配，正在尝试获取一个非法的整数值:" + field + "\nSQL: " + QueryString + "\nException: " + e);
                return 0;
            }
        }

        /// <summary>
        /// 读取一个字符串
        /// </summary>
        /// <param name="field">字段名</param>
        /// <returns>字符串值</returns>
        public string GetString(string field)
        {
            if (_reader == null)
            {
                return "";
            }
            if (!_reader.HasRows)
            {
                return "";
            }
            try
            {
                return _reader.GetString(GetFieldId(field));
            }
            catch (Exception e)
            {
                Logger.LogError(LogModule.Data, "数据类型不匹配，正在尝试获取一个非法的字符串值:" + field + "\nSQL: " + QueryString + "\nException: " + e);
                return string.Empty;
            }
        }

        private int GetFieldId(string field)
        {
            if (!_dictFieldId.ContainsKey(field))
            {
                _dictFieldId[field] = _reader.GetOrdinal(field);
            }
            return _dictFieldId[field];
        }
    }
}
