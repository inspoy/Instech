/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Common.Editor
 * FileName: ProjectSettings.cs
 * Created on 2019/02/17 by inspoy
 * All rights reserved.
 */

using System.IO;
using Instech.Framework.Logging;
using UnityEditor;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Common.Editor
{
    /// <summary>
    /// Unity工程相关配置，仅编辑器使用
    /// </summary>
    public class ProjectSettings : ScriptableObject
    {
        /// <summary>
        /// 存放路径
        /// </summary>
        public const string SavePath = "Assets/StaticResource/ProjectSettings.asset";

        /// <summary>
        /// 美术资源根目录（相对Assets）
        /// </summary>
        public string ArtworkRootPath = "/Artwork/";

        /// <summary>
        /// UI代码导出路径（相对Assets）
        /// </summary>
        public string UiExportPath = "/Scripts/UI/";

        /// <summary>
        /// Excel配置表路径（相对Assets）
        /// </summary>
        public string ExcelDataPath = "/../../Documents/GameConfig/";

        private static ProjectSettings _instance;

        public static ProjectSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<ProjectSettings>(SavePath);
                }

                if (_instance == null)
                {
                    Logger.LogError(LogModule.Editor, "加载ProjectSettings失败！");
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
                Logger.LogWarning(LogModule.Editor, "已经存在ProjectSettings.asset了，中断创建");
                return;
            }

            var asset = CreateInstance<ProjectSettings>();
            AssetDatabase.CreateAsset(asset, SavePath);
            AssetDatabase.Refresh();
            Logger.LogInfo(LogModule.Editor, "创建成功");
        }

        public static void FocusOnAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<ProjectSettings>(SavePath);
            if (asset != null)
            {
                Selection.activeObject = asset;
            }
        }

        /// <summary>
        /// 返回指定框架package的绝对路径
        /// </summary>
        /// <param name="package">指定模块的packageName，如"cc.inspoy.framework.core"</param>
        /// <returns></returns>
        public static string GetPackageFullPath(string package)
        {
            if (string.IsNullOrEmpty(package))
            {
                return string.Empty;
            }
            var path = Path.GetFullPath($"Packages/{package}/");
            if (!Directory.Exists(path))
            {
                path = Path.GetFullPath($"Packages/cc.inspoy.framework/{package}/");
            }
            return Directory.Exists(path) ? path : string.Empty;
        }
    }
}