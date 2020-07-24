/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Data
 * FileName: BaseConfig.cs
 * Created on 2019/12/15 by inspoy
 * All rights reserved.
 */

using System;
using Instech.Framework.Logging;

namespace Instech.Framework.Data
{
    /// <summary>
    /// 所有配置的基类
    /// </summary>
    public abstract class BaseConfig
    {
        public static readonly int[] EmptyIntArray = new int[0];
        public static readonly float[] EmptyFloatArray = new float[0];
        public static readonly string[] EmptyStringArray = new string[0];
        
        /// <summary>
        /// 唯一ID
        /// 索引用
        /// 每个表都必须有这个字段
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Id { get; protected set; }

        public abstract void InitWithData(IConfigData data);

        public virtual void CustomProcess(IConfigData data)
        {
            Logger.Assert(LogModule.Data, Id != 0, $"{GetType()}表的Id字段不可以是0");
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
        /// 读取当前记录的浮点数值
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        float GetFloat(string field);
        
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
        void Init(string src = null, byte[] encryptKey = null);
        void Deinit();
        IConfigData GetData(string tableName);
    }
}
