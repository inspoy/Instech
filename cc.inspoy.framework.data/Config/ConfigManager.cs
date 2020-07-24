/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Data
 * FileName: ConfigManager.cs
 * Created on 2019/12/15 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Instech.Framework.Core;
using Instech.Framework.Logging;
using JetBrains.Annotations;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Data
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        private DateTime _beginInitTime;

        private IConfigDataSource _dataSource;

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

        protected override void Init()
        {
            _dictConfigData = new Dictionary<Type, object>();
            _dictEmptyConfig = new Dictionary<Type, BaseConfig>();
        }

        protected override void Deinit()
        {
            _dataSource?.Deinit();
        }

        /// <summary>
        /// 加载数据库
        /// </summary>
        /// <param name="key"></param>
        public void LoadDataSource(byte[] key = null)
        {
            try
            {
                _beginInitTime = DateTime.Now;
#if UNITY_EDITOR
                string path = null;
                if (ForceUseBinary)
                {
                    _dataSource = new BinaryDataSource();
                    if (IsTesting)
                    {
                        path = $"{Application.dataPath}/Framework/Tests/TestData/TestBinData.bin";
                        _dataSource.Init(path);
                    }
                    else
                    {
                        _dataSource.Init(null, key);
                    }
                }
                else
                {
                    _dataSource = new ExcelDataSource();
                    if (IsTesting)
                    {
                        path = $"{Application.dataPath}/Framework/Tests/TestData/TestExcelData/";
                    }
                    _dataSource.Init(path);
                }
#else
                _dataSource = new BinaryDataSource();
                _dataSource.Init(null, key);
#endif
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
            if (_dataSource == null)
            {
                throw new ConfigException("数据源为空，是否没调用Init?");
            }
            _dataSource.Deinit();
            _dataSource = null;
            Logger.LogInfo(LogModule.Data, $"数据库加载耗时{(DateTime.Now - _beginInitTime).TotalMilliseconds:F2}ms");
        }

        private Dictionary<int, T> CreateAll<T>(string tableName) where T : BaseConfig, new()
        {
            if (_dictConfigData.ContainsKey(typeof(T)))
            {
                Logger.LogError(LogModule.Data, $"已经初始化过{typeof(T)}的数据了！");
                return null;
            }
            var data = GetData(tableName);
            if (data == null)
            {
                throw new ConfigException("数据没有被正确加载", tableName);
            }
            var ret = new Dictionary<int, T>();
            do
            {
                var item = new T();
                item.InitWithData(data);
                try
                {
                    item.CustomProcess(data);
                }
                catch (Exception e)
                {
                    Logger.LogError(LogModule.Data, $"CustomProcess Error, table={tableName}, id={item.Id}:");
                    Logger.LogException(LogModule.Data, e);
                }
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
            Logger.LogInfo(LogModule.Data, $"加载了配置表:{tableName}");
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
                if (!(_dictConfigData[typeof(T)] is Dictionary<int, T> allData))
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

        private IConfigData GetData(string tableName)
        {
            if (_dataSource == null)
            {
                throw new ConfigException("数据源为空");
            }
            return _dataSource.GetData(tableName);
        }
#if UNITY_EDITOR
        /// <summary>
        /// 编辑器中也强制使用二进制数据源
        /// </summary>
        public static bool ForceUseBinary { get; set; } = false;

        /// <summary>
        /// 单元测试开关
        /// </summary>
        public static bool IsTesting { get; set; } = false;
#endif
    }

    /// <summary>
    /// 配置相关异常
    /// </summary>
    [Serializable]
    public sealed class ConfigException : Exception
    {
        public ConfigException(string msg) : base($"配置加载或读取出错:\n{msg}")
        {
        }

        public ConfigException(string msg, string tableName) : base($"配置加载或读取出错:\n{msg}\nin Data Table: {tableName}")
        {
        }

        private ConfigException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
