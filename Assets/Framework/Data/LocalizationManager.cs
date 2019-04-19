/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: LocalizationManager.cs
 * Created on 2019/03/06 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Instech.Framework
{
    /// <summary>
    /// 本地化数据管理
    /// </summary>
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        private const int MaxDepth = 10; // 术语解析最大递归深度，防止无限递归导致栈溢出

        public string DataFolder { get; private set; }
        public EventDispatcher Dispatcher { get; private set; }
        public string CurLanguageId { get; private set; }
        private Dictionary<string, LocalizationData> _loadedLocalizationDatas;
        private Regex _termPattern;

        protected override void Init()
        {
            var sw = Stopwatch.StartNew();
            DataFolder = Application.streamingAssetsPath + "/Localization/";
            Dispatcher = new EventDispatcher(this);
            _loadedLocalizationDatas = new Dictionary<string, LocalizationData>();
            CurLanguageId = string.Empty;
            _termPattern = new Regex(@"#\w+#", RegexOptions.Compiled);

            var files = Directory.GetFiles(DataFolder, "*.json", SearchOption.TopDirectoryOnly);
            var count = 0;
            foreach (var filePath in files)
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var data = JsonUtils.FromJson<LocalizationData>(content);
                if (!CheckValidation(data))
                {
                    Logger.LogWarning(LogModule.Localization, $"Load {Path.GetFileName(filePath)} failed");
                    continue;
                }
                _loadedLocalizationDatas[data.Meta.LanguageId] = data;
                Logger.LogDebug(LogModule.Localization, $"语言 {data.Meta.LanguageId} 中有 {data.Data.Count} 条记录");
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
            return InternalGetString(key, 1, args);
        }

        private string InternalGetString(string key, int depth, params object[] args)
        {
            if (_loadedLocalizationDatas.TryGetValue(CurLanguageId, out var data))
            {
                if (data.Data.TryGetValue(key, out var str))
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
            }
            Logger.LogWarning(LogModule.Localization, "Localized string not found for key: " + key);
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
