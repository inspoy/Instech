/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: BaseConfig.cs
 * Created on 2018/08/10 by inspoy
 * All rights reserved.
 */

namespace Instech.Framework
{
    /// <summary>
    /// 所有配置的积累
    /// </summary>
    public abstract class BaseConfig
    {
        /// <summary>
        /// 唯一ID
        /// 索引用
        /// 每个表都必须有这个字段
        /// </summary>
        public int Id;

        public abstract void InitWithData(IConfigData data);

        protected virtual void CustomProcess(IConfigData data)
        {
            Logger.Assert(LogModule.Data, Id != 0, $"{GetType()}表的Id字段不可以是0");
        }
    }
}
