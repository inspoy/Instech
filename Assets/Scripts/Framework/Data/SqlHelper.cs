/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: SqlHelper.cs
 * Created on 2018/08/10 by inspoy
 * All rights reserved.
 */

using System;
using System.Data.SQLite;

namespace Instech.Framework
{
    /// <summary>
    /// SQLite帮助类，管理数据库连接
    /// </summary>
    public class SqlHelper : IDisposable
    {
        private SQLiteConnection _dbConnection;
        private readonly string _dbPath;

        public SqlHelper(string connect)
        {
            _dbPath = connect;
            try
            {
                _dbConnection = new SQLiteConnection("data source=" + connect);
                _dbConnection.Open();
                Logger.LogInfo(LogModule.Data, "数据库已连接:" + _dbPath);
            }
            catch (Exception e)
            {
                Logger.LogWarning(LogModule.Data, "数据库连接失败" + e);
            }
        }

        /// <summary>
        /// 执行SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SQLiteDataReader Query(string sql)
        {
            var dbCommand = _dbConnection.CreateCommand();
            dbCommand.CommandText = sql;
            var ret = dbCommand.ExecuteReader();
            dbCommand.Dispose();
            return ret;
        }

        #region IDisposable Support
        private bool _disposedValue; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _dbConnection?.Close();
                    _dbConnection?.Dispose();
                    Logger.LogInfo(LogModule.Data, "数据库连接已关闭:" + _dbPath);
                }
                _dbConnection = null;

                _disposedValue = true;
            }
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
        }
        #endregion
    }

}
