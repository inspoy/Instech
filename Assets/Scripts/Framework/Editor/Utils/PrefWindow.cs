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

        private string _uiExportPath;

        [MenuItem("Instech/Preferences")]
        public static void OnShow()
        {
            var window = GetWindow<PrefWindow>("偏好设置");
            window.Show();
            window._uiExportPath = EditorPrefs.GetString(UiPath, "");
        }

        private void OnGUI()
        {
            _uiExportPath = EditorGUILayout.TextField("UI导出路径", _uiExportPath);
            if (GUILayout.Button("保存"))
            {
                OnSaveClicked();
            }
        }

        private void OnSaveClicked()
        {
            EditorPrefs.SetString(UiPath, _uiExportPath);
            EditorUtility.DisplayDialog("完成", "保存完成了！", "OK");
        }
    }
}
