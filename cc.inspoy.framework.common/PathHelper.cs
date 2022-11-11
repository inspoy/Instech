using System;
using System.IO;
using UnityEngine;

namespace Instech.Framework.Common
{
    public static class PathHelper
    {
        /// <summary>
        /// Resource资源目录，编辑器下和Assets同级，Standalone下和exe文件同级，末尾带斜杠
        /// </summary>
        public static string ResourceDataPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../Resources/"));

        /// <summary>
        /// 本地缓存目录
        /// </summary>
        public static string LocalCacheFolder => Application.persistentDataPath;

        /// <summary>
        /// 存档路径，可以给玩家备份用
        /// </summary>
        public static string SaveDataPath =>
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/My Games/{Application.productName}/";
    }
}
