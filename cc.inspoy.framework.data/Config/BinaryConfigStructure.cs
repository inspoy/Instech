/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Data
 * FileName: BinaryConfigStructure.cs
 * Created on 2019/12/15 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Instech.Framework.Data
{
    /// <summary>
    /// 配置表的独立单位
    /// </summary>
    public interface IConfigStructureUnit
    {
        /// <summary>
        /// 将数据写入对应的Writer
        /// </summary>
        /// <returns>写入的字节数</returns>
        int Write(BinaryWriter writer);

        /// <summary>
        /// 从对应的Reader读取数据
        /// </summary>
        /// <returns>读取的字节数</returns>
        int Read(BinaryReader reader);
    }

    /// <summary>
    /// 可能用到的工具方法
    /// </summary>
    internal static class ConfigStructureUtils
    {
        public static readonly UTF8Encoding StringEncoding = new UTF8Encoding(false, true);

        public static int WriteString(BinaryWriter writer, string str)
        {
            var bytes = StringEncoding.GetBytes(str);
            var len = bytes.Length;
            writer.Write(len);
            writer.Write(bytes);
            return len + sizeof(int);
        }

        public static int ReadString(BinaryReader reader, out string str)
        {
            var len = reader.ReadInt32();
            var bytes = reader.ReadBytes(len);
            str = StringEncoding.GetString(bytes);
            return len + sizeof(int);
        }
    }

    /// <summary>
    /// 配置表数据结构
    /// </summary>
    public class ConfigStructure : IConfigStructureUnit
    {
        /// <summary>
        /// Row段
        /// </summary>
        public class RowItem : IConfigStructureUnit
        {
            public RowItem()
            {
            }

            public RowItem(uint columnCount)
            {
                _columnCount = columnCount;
            }

            public List<string> Fields { get; } = new List<string>();
            private readonly uint _columnCount;

            /// <inheritdoc />
            public int Write(BinaryWriter writer)
            {
                var byteCount = 0;
                foreach (var field in Fields)
                {
                    byteCount += ConfigStructureUtils.WriteString(writer, field);
                }
                return byteCount;
            }

            /// <inheritdoc />
            public int Read(BinaryReader reader)
            {
                var byteCount = 0;
                for (var i = 0; i < _columnCount; ++i)
                {
                    byteCount += ConfigStructureUtils.ReadString(reader, out var str);
                    Fields.Add(str);
                }
                return byteCount;
            }
        }

        /// <summary>
        /// Table段
        /// </summary>
        public class TableItem : IConfigStructureUnit
        {
            /// <summary>
            ///  魔术头，识别用
            /// </summary>
            public static readonly byte[] TableHead = { 0x7a, 0xb1, 0xe4 };

            /// <summary>
            ///  行分隔符
            /// </summary>
            public static readonly byte[] RowSep = { 0x80, 0x35, 0xed };

            /// <summary>
            /// 列（首行字段）分隔符
            /// </summary>
            public static readonly byte ColSep = 0xc5;

            /// <summary>
            /// 表名称
            /// </summary>
            public string TableName { get; set; }

            /// <summary>
            /// 列名称
            /// </summary>
            public List<string> ColumnName { get; } = new List<string>();

            public List<RowItem> RowData { get; } = new List<RowItem>();

            /// <summary>
            /// 数据项行数
            /// </summary>
            public uint RowCount { get; set; }

            /// <summary>
            /// 表序号，从0开始
            /// </summary>
            public ushort TableIdx { get; set; }

            /// <summary>
            /// 列个数
            /// </summary>
            public ushort ColumnCount { get; set; }

            /// <inheritdoc />
            public int Write(BinaryWriter writer)
            {
                var byteCount = 0;
                writer.Write(GetHashedBytes(TableHead, TableIdx));
                byteCount += TableHead.Length;
                byteCount += ConfigStructureUtils.WriteString(writer, TableName);
                writer.Write(ColumnCount);
                byteCount += sizeof(ushort);
                foreach (var item in ColumnName)
                {
                    byteCount += ConfigStructureUtils.WriteString(writer, item);
                    writer.Write(ColSep);
                    byteCount += sizeof(byte);
                }
                writer.Write(RowCount);
                byteCount += sizeof(uint);
                for (var i = 0; i < RowCount; i++)
                {
                    byteCount += RowData[i].Write(writer);
                    writer.Write(GetHashedBytes(RowSep, i));
                    byteCount += RowSep.Length;
                }
                return byteCount;
            }

            /// <inheritdoc />
            public int Read(BinaryReader reader)
            {
                var byteCount = 0;
                var header = reader.ReadBytes(TableHead.Length);
                byteCount += TableHead.Length;
                if (!CompareBytes(header, GetHashedBytes(TableHead, TableIdx)))
                {
                    throw new ConfigException("Invalid Table Header");
                }
                byteCount += ConfigStructureUtils.ReadString(reader, out var tableName);
                TableName = tableName;
                ColumnCount = reader.ReadUInt16();
                byteCount += sizeof(ushort);
                for (var i = 0; i < ColumnCount; ++i)
                {
                    byteCount += ConfigStructureUtils.ReadString(reader, out var colName);
                    ColumnName.Add(colName);
                    var colSep = reader.ReadByte();
                    if (colSep != ColSep)
                    {
                        throw new ConfigException("Invalid Column Separator");
                    }
                    byteCount += 1;
                }
                RowCount = reader.ReadUInt32();
                byteCount += sizeof(uint);
                for (var i = 0; i < RowCount; ++i)
                {
                    var rowItem = new RowItem(ColumnCount);
                    byteCount += rowItem.Read(reader);
                    RowData.Add(rowItem);
                    var sep = reader.ReadBytes(RowSep.Length);
                    byteCount += RowSep.Length;
                    if (!CompareBytes(sep, GetHashedBytes(RowSep, i)))
                    {
                        throw new ConfigException("Invalid Row Separator");
                    }
                }
                return byteCount;
            }
        }

        /// <summary>
        /// 魔术头，识别用
        /// </summary>
        public static readonly byte[] MagicHead = { 0xc0, 0x2f, 0x19 };

        /// <summary>
        /// 表分隔符
        /// </summary>
        public static readonly byte[] TableSeparator = { 0x3e, 0x9e, 0x2a };

        /// <summary>
        /// 魔术尾，校验用
        /// </summary>
        public static readonly byte[] MagicEnd = { 0xe2, 0xd0, 0xfc };

        public List<TableItem> TableList { get; } = new List<TableItem>();

        /// <summary>
        /// 表个数
        /// </summary>
        public ushort TableCount { get; set; }

        /// <inheritdoc />
        public int Write(BinaryWriter writer)
        {
            var byteCount = 0;
            // 文件头
            writer.Write(MagicHead);
            byteCount += MagicHead.Length;
            writer.Write(TableCount);
            byteCount += sizeof(ushort);
            for (var i = 0; i < TableCount; ++i)
            {
                byteCount += TableList[i].Write(writer);
                writer.Write(GetHashedBytes(TableSeparator, i));
                byteCount += TableSeparator.Length;
            }
            // 文件尾
            writer.Write(MagicEnd);
            byteCount += MagicEnd.Length;
            return byteCount;
        }

        /// <inheritdoc />
        public int Read(BinaryReader reader)
        {
            var byteCount = 0;
            var header = reader.ReadBytes(MagicHead.Length);
            byteCount += MagicHead.Length;
            if (!CompareBytes(header, MagicHead))
            {
                throw new ConfigException("Invalid Header");
            }
            TableCount = reader.ReadUInt16();
            byteCount += sizeof(ushort);
            for (ushort i = 0; i < TableCount; ++i)
            {
                var tableItem = new TableItem { TableIdx = i };
                byteCount += tableItem.Read(reader);
                TableList.Add(tableItem);
                var sep = reader.ReadBytes(TableSeparator.Length);
                byteCount += TableSeparator.Length;
                if (!CompareBytes(sep, GetHashedBytes(TableSeparator, i)))
                {
                    throw new ConfigException("Invalid Table Separator");
                }
            }
            var end = reader.ReadBytes(MagicEnd.Length);
            byteCount += MagicEnd.Length;
            if (!CompareBytes(end, MagicEnd))
            {
                throw new ConfigException("Invalid End");
            }
            return byteCount;
        }

        /// <summary>
        /// 将字节数组和指定盐进行散列运算
        /// </summary>
        /// <param name="original"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static byte[] GetHashedBytes(byte[] original, int salt)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            var ret = new byte[original.Length];
            for (var i = 0; i < original.Length; ++i)
            {
                var loops = (original[i] ^ (i * salt)) % 16;
                var hash = int.MaxValue;
                for (var j = 0; j < loops; ++j)
                {
                    hash = hash * 121321 % int.MaxValue;
                }
                ret[i] = (byte)hash;
            }
            return ret;
        }

        /// <summary>
        /// 比较两个字节数组是否相同
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <returns></returns>
        public static bool CompareBytes(byte[] arr1, byte[] arr2)
        {
            if (arr1 == null || arr2 == null)
            {
                return false;
            }
            if (arr1.Length != arr2.Length)
            {
                return false;
            }
            for (var i = 0; i < arr1.Length; ++i)
            {
                if (arr1[i] != arr2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
