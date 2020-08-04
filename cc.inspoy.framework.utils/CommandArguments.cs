/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Utils
 * FileName: CommandArguments.cs
 * Created on 2020/07/01 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;

namespace Instech.Framework.Utils
{
    /// <summary>
    /// 获取和解析命令行自定义参数的工具，以'/'开头的才会被解析
    /// </summary>
    public class CommandArguments
    {
        private static CommandArguments _current;
        private readonly Dictionary<string, string> _args;

        public static CommandArguments Get()
        {
            return _current ?? (_current = new CommandArguments());
        }

        private CommandArguments()
        {
            _args = new Dictionary<string, string>();
            var rawArgs = Environment.GetCommandLineArgs();
            foreach (var item in rawArgs)
            {
                if (!item.StartsWith("/"))
                {
                    continue;
                }

                var idx = item.IndexOf(':');
                if (idx == -1)
                {
                    _args.Add(item.Substring(1), string.Empty);
                }
                else
                {
                    var key = item.Substring(1, idx);
                    var val = item.Substring(idx + 1);
                    _args.Add(key, val);
                }
            }
        }

        public bool HasKey(string key)
        {
            return _args.ContainsKey(key);
        }

        public string GetValue(string key)
        {
            if (_args.TryGetValue(key, out var val))
            {
                return val;
            }

            return string.Empty;
        }
    }
}
