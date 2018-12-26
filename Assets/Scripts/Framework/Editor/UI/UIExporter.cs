/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: UIExporter.cs
 * Created on 2018/05/02 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace Instech.Framework.Editor
{
    /// <summary>
    /// 导出UI的View和Presenter的模板代码
    /// </summary>
    public static class UiExporter
    {
        internal static bool ExportUiValidation()
        {
            var prefab = Selection.activeGameObject;
            return prefab != null && prefab.name.StartsWith("vw");
        }

        internal static void ExportUi()
        {
            var exportPath = EditorPrefs.GetString(PrefWindow.UiPath, string.Empty);
            if (exportPath == string.Empty)
            {
                EditorUtility.DisplayDialog("错误", "请先在Instech/Preferences中设置UI导出路径", "OK");
                return;
            }
            var prefab = Selection.activeGameObject;
            var viewName = prefab.name.Substring(2);
            var viewContentBuilder = new StringBuilder();
            var presenterContentBuilder = new StringBuilder();
            var components = new Dictionary<string, Type>();

            // Primary Method
            GenerateContent(prefab, viewContentBuilder, presenterContentBuilder, components);

            var baseFilePath = $"{exportPath.TrimEnd('/', '\\')}/{viewName}";

            var viewFilePath = baseFilePath + "View.cs";
            File.WriteAllText(viewFilePath, viewContentBuilder.ToString(), new UTF8Encoding(false));
            ScriptHeaderGenerator.OnWillCreateAsset(viewFilePath);

            var presenterFilePath = baseFilePath + "Presenter.cs";
            if (!File.Exists(presenterFilePath))
            {
                // 不存在presenter的话才会生成代码
                File.WriteAllText(presenterFilePath, presenterContentBuilder.ToString(), new UTF8Encoding(false));
                ScriptHeaderGenerator.OnWillCreateAsset(presenterFilePath);
            }

            AssetDatabase.Refresh();

            // 给prefab挂载脚本
            var componentName = viewName + "View";
            var component = prefab.GetComponent(componentName);
            var viewType = GetTypeByName(componentName);
            if (component == null)
            {
                if (viewType == null)
                {
                    EditorUtility.DisplayDialog("快完成了", "第一次导出，需要等待编译后再次执行一次！", "OK");
                    Logger.LogInfo(LogModule.Editor, $"Generated code for view: {viewName}");
                    return;
                }
                component = prefab.AddComponent(viewType);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(component, true);
                component = prefab.AddComponent(viewType);
            }
            var needAgain = UpdateComponentReference(component, components);
            if (needAgain)
            {
                EditorUtility.DisplayDialog("快完成了", "有新的控件，需要等待编译后再次执行一次！", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("完成", $"导出完成，该View有{components.Count}个控件！", "OK");
            }
            Logger.LogInfo(LogModule.Editor, $"Generated code for view: {viewName}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Type GetTypeByName(string className)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.Name == className);
        }

        /// <summary>
        /// 开始生成代码
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="viewCode"></param>
        /// <param name="presenterCode"></param>
        /// <param name="components"></param>
        private static void GenerateContent(
            GameObject prefab,
            StringBuilder viewCode,
            StringBuilder presenterCode,
            Dictionary<string, Type> components)
        {
            viewCode.Append(
                "using System;\n" +
                "using Instech.Framework;\n" +
                "using UnityEngine;\n" +
                "using UnityEngine.UI;\n" +
                "using TMPro;\n" +
                "using Logger = Instech.Framework.Logger;\n\n" +
                "namespace Game\n{\n");
            presenterCode.Append(
                "using System;\n" +
                "using Instech.Framework;\n\n" +
                "namespace Game\n{\n");
            var viewName = prefab.name.Substring(2);
            viewCode.Append($"    public class {viewName}View : BaseView\n    {{\n");
            presenterCode.Append(
                $"    public class {viewName}Presenter : IBasePresenter\n" +
                "    {\n" +
                $"        private {viewName}View _view;\n\n" +
                "        /// <inheritdoc />\n" +
                "        public void InitWithView(BaseView view)\n" +
                "        {\n" +
                $"            _view = view as {viewName}View;\n" +
                "        if (_view == null)\n" +
                "        {\n" +
                "            throw new ViewInitException(view);\n" +
                "        }\n");
            var viewPart1 = new StringBuilder();
            var viewPart2 = new StringBuilder();
            var presenterPart1 = new StringBuilder();
            var presenterPart2 = new StringBuilder();
            foreach (var trans in prefab.GetComponentsInChildren<RectTransform>())
            {
                var go = trans.gameObject;
                var prefix = go.name;
                if (prefix.Length > 3)
                {
                    prefix = prefix.Substring(0, 3);
                }
                else
                {
                    continue;
                }
                GenerateWidgetItem(prefix, go, viewPart1, viewPart2, presenterPart1, presenterPart2, components);
            }
            viewCode.Append(viewPart1);
            viewCode.Append(
                "        /// <summary>\n" +
                "        /// 获取对应的Presenter\n" +
                "        /// </summary>\n" +
                "        /// <returns></returns>\n" +
                $"        public {viewName}Presenter GetPresenter()\n" +
                "        {\n" +
                $"            return Presenter as {viewName}Presenter;\n" +
                "        }\n\n" +
                "        /// <summary>\n" +
                "        /// 初始化View\n" +
                "        /// </summary>\n" +
                "        protected override void Awake()\n" +
                "        {\n" +
                "#if UNITY_EDITOR\n" +
                "            var time1 = DateTime.Now;\n" +
                "#endif\n\n");
            viewCode.Append(viewPart2);
            viewCode.Append(
                $"            Presenter = new {viewName}Presenter();\n" +
                "            OnAwakeFinish();\n\n" +
                "#if UNITY_EDITOR\n" +
                "            var time2 = DateTime.Now;\n" +
                "            var diff = time2 - time1;\n" +
                $"            Logger.LogInfo(\"UI\", $\"View Created: vw{viewName}" +
                ", cost {diff.TotalMilliseconds}ms\");\n" +
                "#else\n" +
                $"            Logger.LogInfo(\"UI\", \"View Created: vw{viewName}\");\n" +
                "#endif\n" +
                "        }\n" +
                "    }\n" +
                "}\n");
            presenterCode.Append(presenterPart1);
            presenterCode.Append(
                "\n" +
                "            // Called when view is initialized\n" +
                "        }\n\n" +
                "        /// <inheritdoc />\n" +
                "        public void OnViewActivate()\n" +
                "        {\n" +
                "            // Called when view will be activated\n" +
                "        }\n\n" +
                "        /// <inheritdoc />\n" +
                "        public void OnViewRecycle()\n" +
                "        {\n" +
                "            // Called when view will be recycled\n" +
                "        }\n\n" +
                "        /// <inheritdoc />\n" +
                "        public void OnViewRemoved()\n" +
                "        {\n" +
                "            // Called when view will be destroyed\n" +
                "        }\n\n" +
                "        /// <summary>\n" +
                "        /// 获取对应的View\n" +
                "        /// </summary>\n" +
                "        /// <returns></returns>\n" +
                $"        public {viewName}View GetView()\n" +
                "        {\n" +
                "            return _view;\n" +
                "        }\n");
            presenterCode.Append(presenterPart2);
            presenterCode.Append(
                "    }\n}\n");
        }

        /// <summary>
        /// 生成单个控件的代码
        /// </summary>
        /// <param name="prefix">控件前缀</param>
        /// <param name="go">控件对象</param>
        /// <param name="viewPart1">声明部分，前导8个空格，后加换行</param>
        /// <param name="viewPart2">创建部分，前导12个空格，后加换行</param>
        /// <param name="presenterPart1">事件监听，前导12个空格，无需换行</param>
        /// <param name="presenterPart2">事件回调，前导8个空格，前加换行</param>
        /// <param name="components">控件列表</param>
        private static void GenerateWidgetItem(string prefix,
            GameObject go,
            StringBuilder viewPart1,
            StringBuilder viewPart2,
            StringBuilder presenterPart1,
            StringBuilder presenterPart2, Dictionary<string, Type> components)
        {
            var pascalGoName = char.ToUpper(go.name[0]) + go.name.Substring(1);
            Type comType = null;
            if (prefix.Equals("btn"))
            {
                // Button
                comType = typeof(Button);
                GenerateNormalViewCode(go.name, "Button", viewPart1, viewPart2);
                presenterPart1.Append(
                    "            " +
                    $"_view.AddEventListener(_view.{pascalGoName}, EventEnum.UiPointerClick, On{go.name.Substring(3)}Clicked);\n");
                presenterPart2.Append(
                    "\n" +
                    $"        private void On{go.name.Substring(3)}Clicked(Event e)\n" +
                    "        {\n" +
                    "            throw new NotImplementedException();\n" +
                    "        }\n");
            }
            else if (prefix.Equals("tgg"))
            {
                // Toggle Group
                comType = typeof(ToggleGroup);
                GenerateNormalViewCode(go.name, "ToggleGroup", viewPart1, viewPart2);
                presenterPart1.Append(
                    "            " +
                    $"_view.AddEventListener(_view.{pascalGoName}, EventEnum.UiValueChange, On{go.name.Substring(3)}ToggleValueChange);\n");
                presenterPart2.Append(
                    "\n" +
                    $"        private void On{go.name.Substring(3)}ToggleValueChange(Event e)\n" +
                    "        {\n" +
                    "            throw new NotImplementedException();\n" +
                    "        }\n");
            }
            else if (prefix.Equals("tgb"))
            {
                // Toggle Button
                comType = typeof(Toggle);
                GenerateNormalViewCode(go.name, "Toggle", viewPart1, viewPart2);
                presenterPart1.Append(
                    "            " +
                    $"_view.AddEventListener(_view.{pascalGoName}, EventEnum.UiToggleChange, On{go.name.Substring(3)}ToggleChange);\n");
                presenterPart2.Append(
                    "\n" +
                    $"        private void On{go.name.Substring(3)}ToggleChange(Event e)\n" +
                    "        {\n" +
                    "            throw new NotImplementedException();\n" +
                    "        }\n");
            }
            else if (prefix.Equals("sli"))
            {
                // Slider
                comType = typeof(Slider);
                GenerateNormalViewCode(go.name, "Slider", viewPart1, viewPart2);
                presenterPart1.Append(
                    "            " +
                    $"_view.AddEventListener(_view.{pascalGoName}, EventEnum.UiToggleChange, On{go.name.Substring(3)}SliderChange);\n");
                presenterPart2.Append(
                    "\n" +
                    $"        private void On{go.name.Substring(3)}SliderChange(Event e)\n" +
                    "        {\n" +
                    "            throw new NotImplementedException();\n" +
                    "        }\n");
            }
            else if (prefix.Equals("txt"))
            {
                // Text Mesh Pro
                comType = typeof(TextMeshProUGUI);
                GenerateNormalViewCode(go.name, "TextMeshProUGUI", viewPart1, viewPart2);
            }
            else if (prefix.Equals("img"))
            {
                // Image
                comType = typeof(Image);
                GenerateNormalViewCode(go.name, "Image", viewPart1, viewPart2);
            }
            else if (prefix.Equals("inp"))
            {
                // Input Field
                comType = typeof(TMP_InputField);
                GenerateNormalViewCode(go.name, "TMP_InputField", viewPart1, viewPart2);
            }
            else if (prefix.Equals("scr"))
            {
                // Scroll Rect
                comType = typeof(ScrollRect);
                GenerateNormalViewCode(go.name, "ScrollRect", viewPart1, viewPart2);
            }
            else if (prefix.Equals("pro"))
            {
                // Progress Bar
                comType = typeof(ProgressBar);
                GenerateNormalViewCode(go.name, "ProgressBar", viewPart1, viewPart2);
            }
            else if (prefix.Equals("lay"))
            {
                // Layer
                comType = typeof(GameObject);
                GenerateNormalViewCode(go.name, "GameObject", viewPart1, viewPart2);
            }
            if (comType == null)
            {
                return;
            }
            if (components.ContainsKey(go.name))
            {
                Logger.LogError(LogModule.Editor, "重名的控件: " + go.name);
            }
            else
            {
                components.Add(go.name, comType);
            }
        }

        private static void GenerateNormalViewCode(
            string goName, string typeName,
            StringBuilder part1,
            StringBuilder part2)
        {
            var pascalGoName = char.ToUpper(goName[0]) + goName.Substring(1);
            part1.Append(
                "        /// <summary>\n" +
                $"        /// [{typeName}] {goName.Substring(3)}\n" +
                "        /// </summary>\n" +
                $"        public {typeName} {pascalGoName};\n\n");
            part2?.Append(
                $"            if ({pascalGoName} == null)\n" +
                "            {\n" +
                $"                Logger.LogWarning(\"UI\", \"找不到控件: {goName}\");\n" +
                "            }\n\n");
        }

        private static bool UpdateComponentReference(Component view, Dictionary<string, Type> components)
        {
            var needAgain = false;
            foreach (var item in components)
            {
                var comName = item.Key;
                var comPascalName = char.ToUpper(comName[0]) + comName.Substring(1);
                var comType = item.Value;
                var widgetGo = view.gameObject.FindChildWithName(comName);
                if (widgetGo == null)
                {
                    continue;
                }
                var field = view.GetType().GetField(comPascalName);
                if (field == null)
                {
                    needAgain = true;
                    continue;
                }
                if (comType == typeof(GameObject))
                {
                    field.SetValue(view, widgetGo);
                }
                else
                {
                    var component = widgetGo.GetComponent(comType);
                    field.SetValue(view, component);
                }
            }
            return needAgain;
        }
    }
}
