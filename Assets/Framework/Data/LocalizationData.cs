/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: LocalizationData.cs
 * Created on 2019/03/06 by inspoy
 * All rights reserved.
 */

using System.Collections.Generic;

namespace Instech.Framework
{
    /// <summary>
    /// 单个语言的数据结构
    /// </summary>
    [GenerateJsonResolver("Framework/Data/LocalizationData.cs")]
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
            public string[] Author;

            /// <summary>
            /// 本地化文件的描述
            /// </summary>
            public string Description;

            // Version 2

            /// <summary>
            /// Data的哈希值，用于校验
            /// </summary>
            // public string Hash;
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
