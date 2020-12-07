// == Inspoy Technology ==
// Assembly: Instech.Framework.MyJson
// FileName: GenerateJsonResolverAttribute.cs
// Created on 2019/03/13 by inspoy
// All rights reserved.

using System;
using System.Runtime.CompilerServices;

namespace Instech.Framework.MyJson
{
    public class GenerateJsonResolverAttribute : Attribute
    {
        public readonly string FilePath;

        /// <summary>
        /// 标识要生成JsonResolver代码
        /// </summary>
        /// <param name="filePath">文件相对于Assets的路径</param>
        public GenerateJsonResolverAttribute([CallerFilePath]string filePath = "") => FilePath = filePath;
    }
}