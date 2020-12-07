// == Inspoy Technology ==
// Assembly: Instech.Framework.Utils.Editor
// FileName: VisibleInHierarchy.cs
// Created on 2019/01/19 by inspoy
// All rights reserved.


using UnityEditor;
using UnityEngine;

namespace Instech.Framework.Utils.Editor
{
    /// <summary>
    /// 扩展Hierarchy窗口，快速切换显隐
    /// </summary>
    [InitializeOnLoad]
    public static class VisibleInHierarchy
    {
        static VisibleInHierarchy()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowOnGui;
        }

        private static void HierarchyWindowOnGui(int instanceId, Rect selectionRect)
        {
            // 计算CheckBox的位置和尺寸
            var rectCheckBox = new Rect(selectionRect);
            rectCheckBox.x += rectCheckBox.width;
            rectCheckBox.width = 18;

            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                return;
            }
            // 绘制CheckBox
            var oldValue = go.activeSelf;
            var newValue = GUI.Toggle(rectCheckBox, oldValue, string.Empty);
            if (oldValue != newValue)
            {
                go.SetActive(newValue);
                EditorUtility.SetDirty(go);
            }
        }
    }
}