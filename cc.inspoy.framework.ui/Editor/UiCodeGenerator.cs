/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Ui.Editor
 * FileName: UiCodeGenerator.cs
 * Created on 2019/12/14 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Instech.Framework.Common.Editor;
using Instech.Framework.Logging;
using Instech.Framework.Utils;
using UnityEditor;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;
using Object = UnityEngine.Object;

namespace Instech.Framework.Ui.Editor
{
    public static class UiCodeGenerator
    {
        /// <summary>
        /// 验证导出对象的有效性，确保当前选中了UI预设对象
        /// </summary>
        public static bool EnsureSelectingUiPrefab()
        {
            var prefab = Selection.activeObject;
            return prefab != null && prefab.name.StartsWith("vw") && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(prefab));
        }

        /// <summary>
        /// 编辑器菜单入口
        /// </summary>
        public static void DoMenuGenerate()
        {
            var exportPath = Path.Combine(Application.dataPath, ProjectSettings.Instance.UiExportPath.TrimStart('/'));
            if (string.IsNullOrWhiteSpace(exportPath))
            {
                EditorUtility.DisplayDialog("错误", $"请先在{ProjectSettings.SavePath}中设置UI导出路径", "OK");
                return;
            }

            var prefab = Selection.activeGameObject;
            var viewName = prefab.name.Substring(2);
            var baseFilePath = $"{exportPath.TrimEnd('/', '\\')}/{viewName}";
            var viewContentBuilder = new StringBuilder();
            var presenterContentBuilder = new StringBuilder();
            var components = new Dictionary<string, Type>();

            #region 生成代码

            GenerateContent(viewName, prefab, viewContentBuilder, presenterContentBuilder, components);

            // 写View
            var viewFilePath = baseFilePath + "View.cs";
            File.WriteAllText(viewFilePath, viewContentBuilder.ToString(), new UTF8Encoding(false));

            // 写Presenter
            var presenterFilePath = baseFilePath + "Presenter.cs";
            if (!File.Exists(presenterFilePath))
            {
                // 不存在presenter的话才会生成代码
                File.WriteAllText(presenterFilePath, presenterContentBuilder.ToString(), new UTF8Encoding(false));
            }

            AssetDatabase.Refresh();

            #endregion

            #region 给prefab挂载脚本

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
                // 已经存在，重新挂
                Object.DestroyImmediate(component, true);
                component = prefab.AddComponent(viewType);
            }

            #endregion

            #region 挂接控件的引用

            var needAgain = UpdateReference(component, components);
            if (needAgain)
            {
                EditorUtility.DisplayDialog("快完成了", "有新的控件，需要等待编译后再次执行一次！", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("完成", $"导出完成，该View有{components.Count}个控件！", "OK");
            }

            #endregion

            Logger.LogInfo(LogModule.Editor, $"Generated code for view: {viewName}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void GenerateContent(
            string viewName, GameObject prefab,
            StringBuilder viewContentBuilder, StringBuilder presenterContentBuilder,
            Dictionary<string, Type> components)
        {
            var packageRoot = Path.GetFullPath("Packages/cc.inspoy.framework.ui/");
            var viewTemplatePath = Path.Combine(packageRoot, "Editor/ViewTemplate.txt");
            var presenterTemplatePath = Path.Combine(packageRoot, "Editor/PresenterTemplate.txt");
            viewContentBuilder.Append(File.ReadAllText(viewTemplatePath));
            presenterContentBuilder.Append(File.ReadAllText(presenterTemplatePath));
            /*
             * View模板需要替换：
             * 1. $VIEWNAME$ UI名字
             * 2. $MEMBER_DECLARE$ 控件字段声明部分 \n注释\n声明\n 缩进=8
             * 3. $MEMBER_CHECK$ 控件检查部分 \nif...}\n 缩进=12
             *
             * Presenter模板需要替换：
             * 1. $VIEWNAME$ UI名字
             * 2. $ADD_LISTENERS$ 添加事件监听 不为空时追加\n _view.Add...;\n 缩进=12
             * 3. $EVENT_HANDLERS$ 事件处理 \n\nprivate...} 缩进=8
             */
            viewContentBuilder.Replace("$VIEWNAME$", viewName);
            var memberDeclarePart = new StringBuilder();
            var memberCheckPart = new StringBuilder();
            presenterContentBuilder.Replace("$VIEWNAME$", viewName);
            var addListenerPart = new StringBuilder();
            var eventHandlerPart = new StringBuilder();
            foreach (var transform in prefab.GetComponentsInChildren<RectTransform>(true))
            {
                var go = transform.gameObject;
                if (go.name.Length > 3)
                {
                    var prefix = go.name.Substring(0, 3);
                    GenerateWidgetItem(prefix, go, memberDeclarePart, memberCheckPart, addListenerPart, eventHandlerPart, components);
                }
            }
            if (addListenerPart.Length > 0)
            {
                addListenerPart.Append('\n');
            }
            viewContentBuilder.Replace("$MEMBER_DECLARE$", memberDeclarePart.ToString());
            viewContentBuilder.Replace("$MEMBER_CHECK$", memberCheckPart.ToString());
            presenterContentBuilder.Replace("$ADD_LISTENERS$", addListenerPart.ToString());
            presenterContentBuilder.Replace("$EVENT_HANDLERS$", eventHandlerPart.ToString());
        }

        private static void GenerateWidgetItem(
            string prefix, GameObject go,
            StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart,
            Dictionary<string, Type> components)
        {
            var handle = PrefabUtility.GetPrefabInstanceHandle(go);
            var parentHandle = handle != null ? PrefabUtility.GetPrefabInstanceHandle(go.transform.parent.gameObject) : null;
            if (handle != null && parentHandle != null)
            {
                // skip nested prefab
                return;
            }
            var pascalGoName = char.ToUpper(go.name[0]) + go.name.Substring(1);
            var generator = UiCodeWidgetGenerator.GetGenerator(prefix);
            var comType = generator?.Generate(go, pascalGoName, memberDeclarePart, memberCheckPart, addListenerPart, eventHandlerPart);
            if (comType == null)
            {
                return;
            }
            if (components.ContainsKey(go.name))
            {
                Logger.LogError(LogModule.Editor, "重名的控件：" + go.name);
            }
            else
            {
                components.Add(go.name, comType);
            }
        }

        private static Type GetTypeByName(string className)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .FirstOrDefault(type => type.Name == className);
        }

        /// <summary>
        /// 自动挂接控件引用
        /// </summary>
        /// <returns>是否需要重新编译</returns>
        private static bool UpdateReference(Component view, Dictionary<string, Type> components)
        {
            var needCompile = false;
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
                    needCompile = true;
                    continue;
                }
                var component = widgetGo.GetComponent(comType);
                field.SetValue(view, component);
            }
            return needCompile;
        }
    }
}
