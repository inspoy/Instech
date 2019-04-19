/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: ProjectSettings.cs
 * Created on 2019/02/17 by inspoy
 * All rights reserved.
 */

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Instech.Framework.Editor
{
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

        /// <summary>
        /// Utf8Json代码生成工具路径（相对Assets）
        /// </summary>
        public string Utf8JsonGeneratorPath = "/../../Tools/Utf8Json.UniversalCodeGenerator/Utf8Json.UniversalCodeGenerator.exe";

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

        public static void CreateNewAsset()
        {
            if (File.Exists(Path.Combine(Application.dataPath, "/../", SavePath)))
            {
                Logger.LogWarning(LogModule.Editor, "已经存在ProjectSettings.asset了");
            }
            var asset = CreateInstance<ProjectSettings>();
            AssetDatabase.CreateAsset(asset, SavePath);
            AssetDatabase.Refresh();
        }
    }
}
