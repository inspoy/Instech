/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: PrefWindow.cs
 * Created on 2018/05/02 by inspoy
 * All rights reserved.
 */

using UnityEditor;
using UnityEngine;

namespace Instech.Framework.Editor
{
    /// <summary>
    /// 偏好设置
    /// </summary>
    public class PrefWindow : EditorWindow
    {
        public const string UiPath = "Instech_EditorPrefs_UiExportPath";
        public const string AssetPath = "Instech_EditorPrefs_AssetsRootPath";

        private string _uiExportPath;
        private string _assetsRootPath;

        [MenuItem("Instech/Preferences")]
        public static void OnShow()
        {
            var window = GetWindow<PrefWindow>("偏好设置");
            window.Show();
            window._uiExportPath = EditorPrefs.GetString(UiPath, "");
            window._assetsRootPath = EditorPrefs.GetString(AssetPath, "");
        }

        private void OnGUI()
        {
            _uiExportPath = EditorGUILayout.TextField("UI导出路径", _uiExportPath);
            _assetsRootPath = EditorGUILayout.TextField("Artwork根目录", _assetsRootPath);
            if (GUILayout.Button("保存"))
            {
                OnSaveClicked();
            }
        }

        private void OnSaveClicked()
        {
            EditorPrefs.SetString(UiPath, _uiExportPath);
            EditorPrefs.SetString(AssetPath, _assetsRootPath);
            EditorUtility.DisplayDialog("完成", "保存完成了！", "OK");
        }
    }
}
