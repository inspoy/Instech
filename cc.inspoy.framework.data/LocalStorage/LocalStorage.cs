// == Inspoy Technology ==
// Assembly: Instech.Framework.Data
// FileName: LocalStorage.cs
// Created on 2019/12/16 by inspoy
// All rights reserved.

using System.Collections.Generic;
using System.IO;
using Instech.EncryptHelper;
using Instech.Framework.Common;
using Instech.Framework.Core;
using Instech.Framework.Logging;
using Instech.Framework.MyJson;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Data
{
    /// <summary>
    /// Shortcut for LocalStorage
    /// </summary>
    public static class LS
    {
        /// <summary>
        /// 初始化LS模块
        /// </summary>
        /// <param name="key">加密密钥</param>
        public static void Init(byte[] key = null)
        {
            LocalStorage.CreateSingleton();
            LocalStorage.Instance.Load(key);
        }

        public static bool HasInt(string key)
        {
            return LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item) && item.Type == 1;
        }

        public static bool HasFloat(string key)
        {
            return LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item) && item.Type == 2;
        }

        public static bool HasBool(string key)
        {
            return LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item) && item.Type == 3;
        }

        public static bool HasString(string key)
        {
            return LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item) && item.Type == 4;
        }

        public static void Set(string key, int value)
        {
            if (LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item))
            {
                if (!item.IntValue.Equals(value))
                {
                    item.IntValue = value;
                    LocalStorage.Instance.Dirty = true;
                }
            }
            else
            {
                LocalStorage.Instance.CachedSetting.Add(key, new LocalStorage.CachedSettingItem { Type = 1, IntValue = value });
                LocalStorage.Instance.Dirty = true;
            }
        }

        public static void Set(string key, float value)
        {
            if (LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item))
            {
                if (!item.FloatValue.Equals(value))
                {
                    item.FloatValue = value;
                    LocalStorage.Instance.Dirty = true;
                }
            }
            else
            {
                LocalStorage.Instance.CachedSetting.Add(key, new LocalStorage.CachedSettingItem { Type = 2, FloatValue = value });
                LocalStorage.Instance.Dirty = true;
            }
        }

        public static void Set(string key, bool value)
        {
            if (LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item))
            {
                if (!item.BoolValue.Equals(value))
                {
                    item.BoolValue = value;
                    LocalStorage.Instance.Dirty = true;
                }
            }
            else
            {
                LocalStorage.Instance.CachedSetting.Add(key, new LocalStorage.CachedSettingItem { Type = 3, BoolValue = value });
                LocalStorage.Instance.Dirty = true;
            }
        }

        public static void Set(string key, string value)
        {
            if (LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item))
            {
                if (!item.StringValue.Equals(value))
                {
                    item.StringValue = value;
                    LocalStorage.Instance.Dirty = true;
                }
            }
            else
            {
                LocalStorage.Instance.CachedSetting.Add(key, new LocalStorage.CachedSettingItem { Type = 4, StringValue = value });
                LocalStorage.Instance.Dirty = true;
            }
        }

        public static int Get(string key, int fallback = 0)
        {
            if (LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item) && item.Type == 1)
            {
                return item.IntValue;
            }
            return fallback;
        }

        public static float Get(string key, float fallback = 0f)
        {
            if (LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item) && item.Type == 2)
            {
                return item.FloatValue;
            }
            return fallback;
        }

        public static bool Get(string key, bool fallback = false)
        {
            if (LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item) && item.Type == 3)
            {
                return item.BoolValue;
            }
            return fallback;
        }

        public static string Get(string key, string fallback = null)
        {
            if (LocalStorage.Instance.CachedSetting.TryGetValue(key, out var item) && item.Type == 4)
            {
                return item.StringValue;
            }
            return fallback ?? string.Empty;
        }

        public static void Save()
        {
            LocalStorage.Instance.Save();
        }
    }

    /// <summary>
    /// 本地存储相关
    /// 所有的数据都会存在presistent目录下存储，而非注册表
    /// </summary>
    public class LocalStorage : Singleton<LocalStorage>
    {
        [GenerateJsonResolver]
        public class CachedSettingItem
        {
            /// <summary>
            /// 1-int 2-float 3-bool 4-string
            /// </summary>
            public int Type;

            public int IntValue;
            public float FloatValue;
            public bool BoolValue;
            public string StringValue = string.Empty;
        }

        internal readonly Dictionary<string, CachedSettingItem> CachedSetting = new Dictionary<string, CachedSettingItem>();
        internal bool Dirty;
        private string _savePath;
        private byte[] _key = { 123, 12, 5, 4, 83, 211, 184, 0, 158, 66, 121, 212, 65, 41, 57, 61 };

        protected override void Init()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "LS.bin");
            Scheduler.AddTimer(Save, 10, -1, true); // 每10秒保存一次
            Scheduler.OnQuit += Save; // 游戏退出时保存一次
        }

        public void Load(byte[] key = null)
        {
            if (key != null && key.Length > 0)
            {
                _key = key;
            }
            CachedSetting.Clear();
            if (!File.Exists(_savePath))
            {
                return;
            }
            var encrypt = File.ReadAllBytes(_savePath);
            var aes = new Aes();
            aes.Init(_key);
            var real = aes.Decrypt(encrypt);
            var data = MyJson.MyJson.FromJson<Dictionary<string, CachedSettingItem>>(real);
            if (data == null)
            {
                throw new InvalidDataException("Cannot load LS.bin, wrong acess key?");
            }
            foreach (var kv in data)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key) && kv.Value.Type > 0)
                {
                    CachedSetting.Add(kv.Key, kv.Value);
                }
            }
        }

        public void Save()
        {
            if (!Dirty)
            {
                return;
            }
            var json = MyJson.MyJson.ToBytes(CachedSetting);
            var aes = new Aes();
            aes.Init(_key);
            var encrypt = aes.Encrypt(json);
            File.WriteAllBytes(_savePath, encrypt);
#if UNITY_EDITOR
            // 编辑器下，同时写入未加密的版本
            File.WriteAllText(_savePath + ".json", MyJson.MyJson.ToJson(CachedSetting));
#endif
            Logger.LogInfo(LogModule.Data, "LocalStorage Saved");
        }
    }
}
