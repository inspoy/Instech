/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Data
 * FileName: LocalizationData.cs
 * Created on 2019/12/16 by inspoy
 * All rights reserved.
 */

using System.Collections.Generic;
using Instech.Framework.MyJson;

namespace Instech.Framework.Data
{
    /// <summary>
    /// 单个语言的数据结构
    /// </summary>
    [GenerateJsonResolver]
    public class LocalizationData
    {
        /// <summary>
        /// 元数据
        /// </summary>
        public class MetaData
        {
            // Version 1

            /// <summary>
            /// 语言ID，用于<code>SetLanguage</code>
            /// </summary>
            public string LanguageId;

            /// <summary>
            /// 文件名
            /// </summary>
            public string FileName;

            /// <summary>
            /// 给用于的显示名
            /// </summary>
            public string DisplayName;

            /// <summary>
            /// 本地化文件的作者
            /// </summary>
            public string[] Authors;

            /// <summary>
            /// 本地化文件的描述
            /// </summary>
            public string Description;

            // Version 2

            // 考虑增加校验用的哈希值
        }

        /// <summary>
        /// 数据结构版本
        /// </summary>
        public int Version;

        /// <summary>
        /// 元数据
        /// </summary>
        public MetaData Meta;

        /// <summary>
        /// Key-本地化字符串 的键值对
        /// </summary>
        public Dictionary<string, string> Data;
    }
}