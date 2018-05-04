/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: UIExporter.cs
 * Created on 2018/05/02 by inspoy
 * All rights reserved.
 */

using UnityEditor;

namespace Instech.Framework
{
    /// <summary>
    /// 导出UI的View和Presenter的模板代码
    /// </summary>
    public static class UiExporter
    {
        internal static void ExportUi()
        {
            var prefab = Selection.activeGameObject;
            var viewName = prefab.name.Substring(2);

        }

        internal static bool ExportUiValidation()
        {
            var prefab = Selection.activeGameObject;
            return prefab != null && prefab.name.StartsWith("vw");
        }
    }
}
