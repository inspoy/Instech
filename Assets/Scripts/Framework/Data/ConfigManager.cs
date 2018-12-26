/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ConfigManager.cs
 * Created on 2018/08/10 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using UnityEngine;

namespace Instech.Framework
{
    /// <summary>
    /// 配置表管理
    /// <para>使用方法：</para>
    /// <para>1.创建单例<code>CreateInstance</code></para>
    /// <para>2.注册所有类型<code>RegisterConfigType</code>（可通过代码生成或者反射的方式统一注册）</para>
    /// <para>3.调用<code>FinishInit</code></para>
    /// <para>4.使用<code>GetSingle</code>, <code>GetAllConfig</code>和<code>GetAll</code>来获取数据</para>
    /// </summary>
    public class ConfigManager : Singleton<ConfigManager>
    {
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

        /// <summary>
        /// 结束初始化
        /// </summary>
        public void FinishInit()
        {
            if (_dataSource == null)
            {
                throw new ConfigException("数据源为空，是否没调用Init?");
            }
            _dataSource.FinishInit();
            _dataSource.Uninit();
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
                item.CustomProcess(data);
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

        protected override void Init()
        {
            _dictConfigData = new Dictionary<Type, object>();
            _dictEmptyConfig = new Dictionary<Type, BaseConfig>();
            try
            {
                // 加载
                _beginInitTime = DateTime.Now;
#if UNITY_EDITOR
                string path = null;
                if (ForceUseBinary)
                {
                    _dataSource = new BinaryDataSource();
                    if (IsTesting)
                    {
                        path = $"{Application.dataPath}/Framework/Tests/TestData/TestBinData.bin";
                    }
                }
                else
                {
                    _dataSource = new ExcelDataSource();
                    if (IsTesting)
                    {
                        path = $"{Application.dataPath}/Framework/Tests/TestData/TestExcelData/";
                    }
                }
                _dataSource.Init(path);
#else
                _dataSource = new BinaryDataSource();
                _dataSource.Init();
#endif
            }
            catch (Exception e)
            {
                Logger.LogError(LogModule.Data, "加载数据库出错：" + e);
            }
        }

        protected override void Uninit()
        {
            _dataSource?.Uninit();
        }

        private IConfigData GetData(string tableName)
        {
            if (_dataSource == null)
            {
                throw new ConfigException("数据源为空");
            }
            return _dataSource.GetData(tableName);
        }
    }

    /// <summary>
    /// 单个配置表数据
    /// </summary>
    public interface IConfigData
    {
        /// <summary>
        /// 读取下一条记录
        /// </summary>
        /// <returns>返回false表示已经没有下一条记录了</returns>
        bool Next();

        /// <summary>
        /// 读取当前记录的整数值
        /// </summary>
        /// <param name="field">字段名</param>
        /// <returns></returns>
        int GetInt(string field);

        /// <summary>
        /// 读取当前记录的字符串值
        /// </summary>
        /// <param name="field">字段名</param>
        /// <returns></returns>
        string GetString(string field);

        /// <summary>
        /// 结束读取
        /// </summary>
        void Close();
    }

    /// <summary>
    /// 配置表数据源
    /// </summary>
    public interface IConfigDataSource
    {
        void Init(string src = null);
        void FinishInit();
        void Uninit();
        IConfigData GetData(string tableName);
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
