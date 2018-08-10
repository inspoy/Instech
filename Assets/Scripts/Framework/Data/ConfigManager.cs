/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ConfigManager.cs
 * Created on 2018/08/10 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using JetBrains.Annotations;
using UnityEngine;

namespace Instech.Framework
{
    public sealed partial class ConfigManager : Singleton<ConfigManager>
    {
        /// <summary>
        /// 这里存储了所有的配置数据
        /// 值类型是Dictionary(int, BaseConfig)
        /// </summary>
        private Dictionary<Type, object> _dictConfigData;

        /// <summary>
        /// 这里存储了各种类型的空配置
        /// 当查询不到记录时就用这里的对象
        /// 节省了创建新对象的时间
        /// </summary>
        private Dictionary<Type, BaseConfig> _dictEmptyConfig;

        private SqlHelper _sqlClient;
        private DateTime _beginInitTime;

        protected override void Init()
        {
            _dictConfigData = new Dictionary<Type, object>();
            _dictEmptyConfig = new Dictionary<Type, BaseConfig>();
            try
            {
                // 加载
                _beginInitTime = DateTime.Now;
                _sqlClient = new SqlHelper(Application.streamingAssetsPath + "/conf.db");
            }
            catch (Exception e)
            {
                Logger.LogError(LogModule.Data, "加载数据库出错：" + e);
            }
        }

        /// <summary>
        /// 结束初始化
        /// </summary>
        public void FinishInit()
        {
            _sqlClient.Dispose();
            _sqlClient = null;
            Logger.LogInfo(LogModule.Data, $"数据库加载耗时{(DateTime.Now - _beginInitTime).TotalMilliseconds:F2}ms");
        }

        public Dictionary<int, T> CreateAll<T>(string tableName) where T : BaseConfig, new()
        {
            if (_dictConfigData.ContainsKey(typeof(T)))
            {
                Logger.LogError(LogModule.Data, $"已经初始化过{typeof(T)}的数据了！");
                return null;
            }
            var sql = "SELECT * FROM `{tableName}`";
            var data = GetData(sql);
            var ret = new Dictionary<int, T>();
            do
            {
                var item = new T();
                item.InitWithData(data);
                ret.Add(item.Id, item);
            } while (data.Next());
            data.Close();
            return ret;
        }

        /// <summary>
        /// 注册配置项，必须在<code>ConfigManager.CreateSingleton()</code>之后调用
        /// <para>而且在全部注册完之后必须调用<code>FinishInit()</code>方法</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RegisterConfigType<T>(string tableName) where T : BaseConfig, new()
        {
            _dictEmptyConfig[typeof(T)] = new T();
            _dictConfigData[typeof(T)] = CreateAll<T>(tableName);
        }

        /// <summary>
        /// 获取单个Config对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nId"></param>
        /// <returns></returns>
        public T GetSingle<T>(int nId) where T : BaseConfig
        {
            var ret = GetEmptyConfig<T>();
            do
            {
                if (!_dictConfigData.ContainsKey(typeof(T)))
                {
                    Logger.LogError(LogModule.Data, $"没有加载这个类型的配置数据：{typeof(T)}");
                    break;
                }
                var allData = _dictConfigData[typeof(T)] as Dictionary<int, T>;
                if (allData == null)
                {
                    Logger.LogError(LogModule.Data, $"没有加载这个类型的配置数据：{typeof(T)}");
                    break;
                }
                if (allData.ContainsKey(nId))
                {
                    ret = allData[nId];
                }
                else
                {
                    Logger.LogError(LogModule.Data, $"没有找到记录，id={nId}, table={typeof(T)}");
                }
            } while (false);
            return ret;
        }

        /// <summary>
        /// 获取某个表的全部数据（含Key）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [CanBeNull]
        public Dictionary<int, T> GetAllConfig<T>() where T : BaseConfig
        {
            if (!_dictConfigData.ContainsKey(typeof(T)))
            {
                return null;
            }
            return _dictConfigData[typeof(T)] as Dictionary<int, T>;
        }

        /// <summary>
        /// 获取某个表的全部数据（不含Key）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [CanBeNull]
        public ICollection<T> GetAll<T>() where T : BaseConfig
        {
            var all = GetAllConfig<T>();
            return all?.Values;
        }

        /// <summary>
        /// 获取某个类型的空对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetEmptyConfig<T>() where T : BaseConfig
        {
            if (!_dictEmptyConfig.ContainsKey(typeof(T)))
            {
                return null;
            }
            return _dictEmptyConfig[typeof(T)] as T;
        }

        /// <summary>
        /// 执行查询获取数据
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        private SqlData GetData(string sql)
        {
            var reader = _sqlClient.Query(sql);
            reader.Read();
            if (!reader.HasRows)
            {
                Logger.LogWarning(LogModule.Data, "查询不到记录：" + sql);
            }
            var ret = new SqlData(reader, sql);
            return ret;
        }

        protected override void Uninit()
        {
            _sqlClient?.Dispose();
        }
    }

    /// <summary>
    /// SQLite数据
    /// </summary>
    public class SqlData
    {
        private readonly SQLiteDataReader _reader;
        private readonly Dictionary<string, int> _dictFieldId;
        public string QueryString { get; }

        public SqlData(SQLiteDataReader reader, string sql)
        {
            _reader = reader;
            QueryString = sql;
        }

        public void Close()
        {
            _reader.Close();
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

        private int GetFieldId(string field)
        {
            if (!_dictFieldId.ContainsKey(field))
            {
                _dictFieldId[field] = _reader.GetOrdinal(field);
            }
            return _dictFieldId[field];
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
            catch (Exception)
            {
                Logger.LogError(LogModule.Data, "数据类型不匹配，正在尝试获取一个非法的整数值:" + field + "\nSQL: " + QueryString);
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
            catch (Exception)
            {
                Logger.LogError(LogModule.Data, "数据类型不匹配，正在尝试获取一个非法的字符串值:" + field + "\nSQL: " + QueryString);
                return "";
            }
        }
    }
}
