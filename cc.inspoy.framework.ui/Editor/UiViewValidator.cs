/**
 * == CountryRailway ==
 * Assembly: 
 * FileName: UiViewValidator.cs
 * Created on 2020/07/13 by inspoy
 * All rights reserved.
 */

using System.Text;
using Instech.Framework.Utils;
using UnityEditor;
using UnityEngine.UI;

namespace Instech.Framework.Ui.Editor
{
    public static class UiViewValidator
    {
        public static void CheckRaycastTarget()
        {
            var go = Selection.activeGameObject;
            var coms = go.GetComponentsInChildren<Graphic>();
            var report = new StringBuilder("Components with raycast list:");
            foreach (var graphic in coms)
            {
                if (graphic.raycastTarget)
                {
                    report.Append('\n');
                    if (graphic is Text)
                    {
                        report.Append("[!]");
                    }
                    report.Append($"{graphic.transform.GetHierarchyPath(go.transform)} => {graphic.GetType()}");
                }
            }

            EditorUtility.DisplayDialog("Report", report.ToString(), "OK");
        }
    }
}
