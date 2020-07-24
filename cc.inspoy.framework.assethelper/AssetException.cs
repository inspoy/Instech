/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.AssetHelper
 * FileName: AssetException.cs
 * Created on 2019/12/17 by inspoy
 * All rights reserved.
 */

using System;
using System.Text;

namespace Instech.Framework.AssetHelper
{
    public enum ErrorCode
    {
        Success,
        NotInited,
        InitFailed,
        UnknownPath,
        BundleNotFound,
        LoadFailed,
        Other
    }

    public class AssetException : Exception
    {
        private static readonly StringBuilder MessageBuilder = new StringBuilder();

        public AssetException(ErrorCode reason, string path = null, string msg = null) : base(MakeMessage(reason, path, msg))
        {
        }

        private static string MakeMessage(ErrorCode reason, string path, string msg)
        {
            MessageBuilder.Clear();
            MessageBuilder.Append($"Asset Exception: {reason}");
            if (!string.IsNullOrEmpty(path))
            {
                MessageBuilder
                    .Append("\n -AssetPath: ")
                    .Append(path);
            }

            if (!string.IsNullOrEmpty(msg))
            {
                MessageBuilder
                    .Append("\n -Message: ")
                    .Append(msg);
            }

            return MessageBuilder.ToString();
        }
    }
}