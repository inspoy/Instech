// == Inspoy Technology ==
// Assembly: Instech.Framework.Data
// FileName: LocalizationManager.cs
// Created on 2019/12/16 by inspoy
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Instech.Framework.Common;
using Instech.Framework.Core;
using Instech.Framework.Logging;
using Instech.Framework.Utils;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Data
{
    #region 简化写法: L.N()

    /// <summary>
    /// Shortcut for LocalizationManager
    /// </summary>
    public static class L
    {
        /// <summary>
        /// Shortcut for LocalizationManager.GetString()
        /// </summary>
        public static string N(string key, params object[] args)
        {
            return LocalizationManager.Instance.GetString(key, args);
        }
    }

    #endregion

    public class LocalizationManager : Singleton<LocalizationManager>
    {
        public enum EnableLevel
        {
            /// <summary>
            /// 正常启用本地化翻译
            /// </summary>
            Normal,

            /// <summary>
            /// 禁用翻译，显示Key+参数
            /// </summary>
            KeyWithArgs,

            /// <summary>
            /// 禁用翻译，只显示Key
            /// </summary>
            KeyOnly
        }

        private const int MaxDepth = 10; // 术语解析最大递归深度，防止无限递归导致栈溢出

        public string DataFolder { get; private set; }
        public EventDispatcher Dispatcher { get; private set; }
        public string CurLanguageId { get; private set; }

        /// <summary>
        /// 调试模式，开启的话则所有本地化字符串返回空，用于检查是否有字符串硬编码在代码里
        /// </summary>
        public bool DebugMode { get; set; }

        /// <summary>
        /// 默认启用，禁用时将始终返回本地化Key
        /// </summary>
        public EnableLevel Enable { get; set; } = EnableLevel.Normal;

        private Dictionary<string, LocalizationData> _loadedLocalizationDatas;
        private Regex _termPattern;

        protected override void Init()
        {
            var sw = Stopwatch.StartNew();
            DataFolder = Utility.ResourcesPath + "Localization/";
            Dispatcher = new EventDispatcher(this);
            _loadedLocalizationDatas = new Dictionary<string, LocalizationData>();
            CurLanguageId = string.Empty;
            _termPattern = new Regex(@"#\w+#", RegexOptions.Compiled);

            var files = Directory.GetFiles(DataFolder, "*.json", SearchOption.TopDirectoryOnly);
            var count = 0;
            foreach (var filePath in files)
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var data = MyJson.MyJson.FromJson<LocalizationData>(content);
                if (!CheckValidation(data))
                {
                    Logger.LogWarning(LogModule.Localization, $"Load {Path.GetFileName(filePath)} failed");
                    continue;
                }

                _loadedLocalizationDatas[data.Meta.LanguageId] = data;
                Logger.LogInfo(LogModule.Localization, $"{data.Data.Count} records are found in language pack {data.Meta.LanguageId}");
                count += 1;
            }

            Logger.LogInfo(LogModule.Localization, $"Loaded {count} language pack, cost {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// 加载自定义的语言设置，否则根据系统语言初始化
        /// </summary>
        /// <param name="savedSetting">上次使用的语言</param>
        public void InitSetting(string savedSetting)
        {
            if (string.IsNullOrWhiteSpace(savedSetting))
            {
                // 没有设置（一般只会在一次启动游戏才会没有），自动检测系统语言
                savedSetting = CultureInfo.InstalledUICulture.Name;
                Logger.LogInfo(LogModule.Localization, "Auto detect system language: " + savedSetting);
            }

            if (!SetLanguage(savedSetting))
            {
                Logger.LogInfo(LogModule.Localization, $"Load l10n pack for {savedSetting} failed, use fallback en-US");
                // 对于不支持的语言，默认使用英语
                if (!SetLanguage("en-US"))
                {
                    Logger.LogError(LogModule.Localization, "Default language(English) failed to load.");
                }
            }
        }

        /// <summary>
        /// 设定语言
        /// </summary>
        /// <param name="languageId">符合ISO-639标准，形如zh-CN</param>
        /// <returns>是否设置成功</returns>
        public bool SetLanguage(string languageId)
        {
            if (_loadedLocalizationDatas.ContainsKey(languageId))
            {
                CurLanguageId = languageId;
                Dispatcher.DispatchEvent(EventEnum.LanguageChange);
                return true;
            }

            Logger.LogWarning(LogModule.Localization, "Language is not supported: " + languageId);
            return false;
        }

        /// <summary>
        /// 获取对应的本地化字符串
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetString(string key, params object[] args)
        {
            if (Enable == EnableLevel.KeyOnly)
            {
                return $"{{L10N:{key}}}";
            }

            if (Enable == EnableLevel.KeyWithArgs)
            {
                var sb = StringBuilderPool.Acquire();
                foreach (var item in args)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    if (item == null)
                    {
                        sb.Append("null");
                    }
                    else
                    {
                        sb.Append(item);
                    }
                }

                var argStr = sb.ToString();
                sb.ReleaseToPool();
                return $"{{L10N:{key}->({argStr})}}";
            }

            var ret = InternalGetString(key, 1, args);
            if (DebugMode)
            {
                Logger.LogDebug(LogModule.Localization, 0, $"GetString:<{key}>=><{ret}>");
                return string.Empty;
            }

            return ret;
        }

        private string InternalGetString(string key, int depth, params object[] args)
        {
            if (_loadedLocalizationDatas.TryGetValue(CurLanguageId, out var data) &&
                data.Data.TryGetValue(key, out var str) &&
                !string.IsNullOrEmpty(str))
            {
                if (args.Length > 0)
                {
                    try
                    {
                        str = string.Format(str, args);
                    }
                    catch (FormatException)
                    {
                        Logger.LogWarning(LogModule.Localization, "Parameter wrong, key=" + key);
                    }
                }

                str = ParseTerm(str, depth);
                return str;
            }

            Logger.LogWarning(LogModule.Localization, $"Localized string not found for key: {key}, curLang={CurLanguageId}");
            return $"{{L10N:{key}}}";
        }

        /// <summary>
        /// 处理术语
        /// </summary>
        /// <param name="original">待处理的字符串</param>
        /// <param name="depth">递归深度</param>
        /// <returns></returns>
        private string ParseTerm(string original, int depth)
        {
            if (depth > MaxDepth)
            {
                Logger.LogWarning(LogModule.Localization, "术语解析已达最大递归深度，请检查是否有死循环: " + original);
                return original;
            }

            var termMatches = _termPattern.Matches(original);
            foreach (Match termMatch in termMatches)
            {
                var term = termMatch.ToString();
                term = term.Substring(1, term.Length - 2);
                original = original.Replace(termMatch.ToString(), InternalGetString(term, depth + 1));
            }

            // 处理转义
            original = original.Replace("##", "#");
            return original;
        }

        /// <summary>
        /// 检查本地化对象的有效性
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool CheckValidation(LocalizationData data)
        {
            if (data?.Meta == null || data.Data == null)
            {
                return false;
            }

            return data.Version > 0 &&
                   !string.IsNullOrWhiteSpace(data.Meta.LanguageId) &&
                   !string.IsNullOrWhiteSpace(data.Meta.DisplayName);
        }
    }
}
