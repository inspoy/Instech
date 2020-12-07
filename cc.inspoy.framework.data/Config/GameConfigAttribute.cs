// == Inspoy Technology ==
// Assembly: Instech.Framework.Data
// FileName: GameConfigAttribute.cs
// Created on 2018/08/10 by inspoy
// All rights reserved.

using System;

namespace Instech.Framework.Data
{
    /// <summary>
    /// 用于标明表名
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GameConfigAttribute : Attribute
    {
        public string TableName { get; }

        public GameConfigAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}
