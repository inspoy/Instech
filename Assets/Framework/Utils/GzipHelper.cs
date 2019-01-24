/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: GzipHelper.cs
 * Created on 2018/05/23 by inspoy
 * All rights reserved.
 */


using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Instech.Framework
{
    /// <summary>
    /// 压缩字符串用
    /// </summary>
    public static class GzipHelper
    {
        /// <summary>
        /// 压缩为base64编码的字符串
        /// </summary>
        /// <param name="src">源字符串</param>
        /// <returns></returns>
        public static string CompressToBase64(string src)
        {
            var bytes = Compress(src);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 压缩为字节数组
        /// </summary>
        /// <param name="src">源字符串</param>
        /// <returns></returns>
        public static byte[] Compress(string src)
        {
            if (string.IsNullOrWhiteSpace(src))
            {
                throw new ArgumentException("参数为空", nameof(src));
            }
            var rawData = Encoding.UTF8.GetBytes(src);
            return Compress(rawData);
        }

        /// <summary>
        /// 压缩字节数组
        /// </summary>
        /// <param name="src">源字节数组</param>
        /// <returns></returns>
        public static byte[] Compress(byte[] src)
        {
            if (src == null || src.Length == 0)
            {
                throw new ArgumentException("参数为空", nameof(src));
            }
            var ms = new MemoryStream();
            var zs = new GZipStream(ms, CompressionMode.Compress, true);
            zs.Write(src, 0, src.Length);
            zs.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// 从base64字符串解压缩
        /// </summary>
        /// <param name="src">要解压的字符串</param>
        /// <returns></returns>
        public static string UnCompress(string src)
        {
            if (string.IsNullOrWhiteSpace(src))
            {
                throw new ArgumentException("参数为空", nameof(src));
            }
            var zippedBytes = Convert.FromBase64String(src);
            return UnCompress(zippedBytes);
        }

        /// <summary>
        /// 从字节数组解压缩
        /// </summary>
        /// <param name="src">要解压的字节数组</param>
        /// <returns></returns>
        public static string UnCompress(byte[] src)
        {
            if (src == null || src.Length == 0)
            {
                throw new ArgumentException("参数为空", nameof(src));
            }
            var unzippedBytes = UnCompressToBytes(src);
            return Encoding.UTF8.GetString(unzippedBytes);
        }

        /// <summary>
        /// 加压字节数组
        /// </summary>
        /// <param name="src">要解压的字节数组</param>
        /// <returns></returns>
        public static byte[] UnCompressToBytes(byte[] src)
        {
            if (src == null || src.Length == 0)
            {
                throw new ArgumentException("参数为空", nameof(src));
            }
            var ms = new MemoryStream(src);
            var zs = new GZipStream(ms, CompressionMode.Decompress);
            var outBuffer = new MemoryStream();
            var block = new byte[1024]; // 每次1KB
            while (true)
            {
                var bytesRead = zs.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                {
                    break;
                }
                else
                {
                    outBuffer.Write(block, 0, bytesRead);
                }
            }
            zs.Close();
            return outBuffer.ToArray();
        }
    }
}
