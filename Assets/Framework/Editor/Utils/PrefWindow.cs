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
        public const string XlsPath = "Instech_EditorPrefs_XlsConfigPath";

        private string _uiExportPath;
        private string _assetsRootPath;
        private string _xlsConfigPath;

        public static void OnShow()
        {
            var window = GetWindow<PrefWindow>("偏好设置");
            window.Show();
            window._uiExportPath = EditorPrefs.GetString(UiPath, "");
            window._assetsRootPath = EditorPrefs.GetString(AssetPath, "");
            window._xlsConfigPath = EditorPrefs.GetString(XlsPath, "");
        }

        private void OnGUI()
        {
            _uiExportPath = EditorGUILayout.TextField("UI导出路径", _uiExportPath);
            _assetsRootPath = EditorGUILayout.TextField("Artwork根目录", _assetsRootPath);
            _xlsConfigPath = EditorGUILayout.TextField("xls表目录", _xlsConfigPath);
            if (GUILayout.Button("保存"))
            {
                OnSaveClicked();
            }
        }

        private void OnSaveClicked()
        {
            EditorPrefs.SetString(UiPath, _uiExportPath);
            EditorPrefs.SetString(AssetPath, _assetsRootPath);
            EditorPrefs.SetString(XlsPath, _xlsConfigPath);
            EditorUtility.DisplayDialog("完成", "保存完成了！", "OK");
        }
    }
}
