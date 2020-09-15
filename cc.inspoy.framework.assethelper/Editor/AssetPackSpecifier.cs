/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.AssetHelper.Editor
 * FileName: AssetPackSpecifier.cs
 * Created on 2019/12/24 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Instech.Framework.Logging;
using UnityEditor;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.AssetHelper.Editor
{
    public class AssetPackSpecifier : ScriptableObject
    {
        #region 无关内容

        /// <summary>
        /// 存放路径
        /// </summary>
        private const string SavePath = "Assets/StaticResource/AssetPackSpecifier.asset";

        private static AssetPackSpecifier _instance;

        public static AssetPackSpecifier Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<AssetPackSpecifier>(SavePath);
                }

                if (_instance == null)
                {
                    Logger.LogError(LogModule.Editor, "加载AssetPackSpecifier失败！");
                }

                return _instance;
            }
        }

        public static bool AssetExists()
        {
            var fullPath = Path.GetFullPath(SavePath);
            return File.Exists(fullPath);
        }

        public static void CreateNewAsset()
        {
            if (AssetExists())
            {
                Logger.LogWarning(LogModule.Editor, "已经存在AssetPackSpecifier.asset了，中断创建");
                return;
            }

            var asset = CreateInstance<AssetPackSpecifier>();
            AssetDatabase.CreateAsset(asset, SavePath);
            AssetDatabase.Refresh();
            Logger.LogInfo(LogModule.Editor, "创建成功");
        }

        public static void FocusOnAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<AssetPackSpecifier>(SavePath);
            if (asset != null)
            {
                Selection.activeObject = asset;
            }
        }

        #endregion

        [Serializable]
        public class PackItem
        {
            public string PackName;
            public List<string> Bundles;
        }

        public const string FallbackPackName = "Misc.assetpack";
        public string DefaultPackName = FallbackPackName;
        public List<PackItem> PackMap;

        /// <summary>
        /// 获取bundleName映射到packName的字典
        /// </summary>
        public Dictionary<string, string> GetReverseMap()
        {
            var ret = new Dictionary<string, string>();
            foreach (var packItem in PackMap)
            {
                foreach (var bundleName in packItem.Bundles)
                {
                    ret.Add(bundleName, packItem.PackName);
                }
            }
            return ret;
        }
    }
}
