// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui.Editor
// FileName: UiCodeWidgetGenerator.cs
// Created on 2019/12/15 by inspoy
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Instech.Framework.UiWidgets;
using JetBrains.Annotations;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Instech.Framework.Ui.Editor
{
    #region Public

    public interface IWidgetGenerator
    {
        Type Generate(
            GameObject go, string pascalGoName,
            StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart);
    }

    public static class UiCodeWidgetGenerator
    {
        private static readonly Dictionary<string, IWidgetGenerator> Generators = new Dictionary<string, IWidgetGenerator>
        {
            ["btn"] = new ButtonGenerator(),
            ["tgg"] = new ToggleGroupGenerator(),
            ["tgb"] = new ToggleGenerator(),
            ["sli"] = new SliderGenerator(),
            ["txt"] = new TextGenerator(),
            ["img"] = new ImageGenerator(),
            ["inp"] = new InputGenerator(),
            ["scr"] = new ScrollRectGenerator(),
            ["lay"] = new LayerGenerator(),
            ["pro"] = new ProgressBarGenerator(),
            ["svw"] = new SubViewGenerator()
        };

        /// <summary>
        /// 根据控件前缀获取对应的代码生成器
        /// </summary>
        /// <param name="prefix">控件前缀</param>
        /// <returns></returns>
        [CanBeNull]
        public static IWidgetGenerator GetGenerator(string prefix)
        {
            Generators.TryGetValue(prefix, out var generator);
            return generator;
        }

        public static void GenerateNormalViewCode(string pascalGoName, string typeName, StringBuilder memberDeclarePart,
            StringBuilder memberCheckPart)
        {
            memberDeclarePart.Append(
                "\n" +
                "        /// <summary>\n" +
                $"        /// [{typeName}] {pascalGoName.Substring(3)}\n" +
                "        /// </summary>\n" +
                $"        public {typeName} {pascalGoName};\n");
            memberCheckPart.Append(
                "\n" +
                $"            if ({pascalGoName} == null)\n" +
                "            {\n" +
                $"                Logger.LogWarning(\"UI\", \"Widget not found: {pascalGoName}\");\n" +
                "            }\n");
        }
    }

    #endregion

    public sealed class ButtonGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "Button", memberDeclarePart, memberCheckPart);
            addListenerPart.Append(
                "            " +
                $"_view.AddEventListener(_view.{pascalGoName}, EventEnum.UiPointerClick, On{pascalGoName.Substring(3)}Clicked);\n");
            eventHandlerPart.Append(
                "\n\n" +
                $"        private void On{pascalGoName.Substring(3)}Clicked(Event e)\n" +
                "        {\n" +
                "            throw new NotImplementedException();\n" +
                "        }\n");
            return typeof(Button);
        }
    }

    public sealed class ToggleGroupGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "ToggleGroup", memberDeclarePart, memberCheckPart);
            addListenerPart.Append(
                "            " +
                $"_view.AddEventListener(_view.{pascalGoName}, EventEnum.UiValueChange, On{pascalGoName.Substring(3)}ToggleValueChange);\n");
            eventHandlerPart.Append(
                "\n\n" +
                $"        private void On{pascalGoName.Substring(3)}ToggleValueChange(Event e)\n" +
                "        {\n" +
                "            throw new NotImplementedException();\n" +
                "        }\n");
            return typeof(ToggleGroup);
        }
    }

    public sealed class ToggleGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "Toggle", memberDeclarePart, memberCheckPart);
            addListenerPart.Append(
                "            " +
                $"_view.AddEventListener(_view.{pascalGoName}, EventEnum.UiToggleChange, On{pascalGoName.Substring(3)}ToggleChange);\n");
            eventHandlerPart.Append(
                "\n\n" +
                $"        private void On{pascalGoName.Substring(3)}ToggleChange(Event e)\n" +
                "        {\n" +
                "            throw new NotImplementedException();\n" +
                "        }\n");
            return typeof(Toggle);
        }
    }

    public sealed class SliderGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "Slider", memberDeclarePart, memberCheckPart);
            addListenerPart.Append(
                "            " +
                $"_view.AddEventListener(_view.{pascalGoName}, EventEnum.UiValueChange, On{pascalGoName.Substring(3)}SliderChange);\n");
            eventHandlerPart.Append(
                "\n\n" +
                $"        private void On{pascalGoName.Substring(3)}SliderChange(Event e)\n" +
                "        {\n" +
                "            throw new NotImplementedException();\n" +
                "        }\n");
            return typeof(Slider);
        }
    }

    public sealed class TextGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "TextMeshProUGUI", memberDeclarePart, memberCheckPart);
            return typeof(TextMeshProUGUI);
        }
    }

    public sealed class ImageGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "Image", memberDeclarePart, memberCheckPart);
            return typeof(Image);
        }
    }

    public sealed class InputGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "TMP_InputField", memberDeclarePart, memberCheckPart);
            addListenerPart.Append(
                "            " +
                $"_view.AddEventListener(_view.{pascalGoName}, EventEnum.UiValueChange, On{pascalGoName.Substring(3)}InputChange);\n");
            addListenerPart.Append(
                "            " +
                $"_view.AddEventListener(_view.{pascalGoName}, EventEnum.UiSubmit, On{pascalGoName.Substring(3)}Submit);\n");
            eventHandlerPart.Append(
                "\n\n" +
                $"        private void On{pascalGoName.Substring(3)}InputChange(Event e)\n" +
                "        {\n" +
                "            throw new NotImplementedException();\n" +
                "        }\n");
            eventHandlerPart.Append(
                "\n\n" +
                $"        private void On{pascalGoName.Substring(3)}Submit(Event e)\n" +
                "        {\n" +
                "            throw new NotImplementedException();\n" +
                "        }\n");
            return typeof(TMP_InputField);
        }
    }

    public sealed class ScrollRectGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            if (go.GetComponent<ScrollRect>() != null)
            {
                UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "ScrollRect", memberDeclarePart, memberCheckPart);
                return typeof(ScrollRect);
            }
            if (go.GetComponent<LoopedScrollView>() != null)
            {
                UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "LoopedScrollView", memberDeclarePart, memberCheckPart);
                return typeof(LoopedScrollView);
            }
            return null;
        }
    }

    public sealed class LayerGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "RectTransform", memberDeclarePart, memberCheckPart);
            return typeof(RectTransform);
        }
    }

    public sealed class ProgressBarGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, "ProgressBar", memberDeclarePart, memberCheckPart);
            return typeof(ProgressBar);
        }
    }

    public sealed class SubViewGenerator : IWidgetGenerator
    {
        public Type Generate(GameObject go, string pascalGoName, StringBuilder memberDeclarePart, StringBuilder memberCheckPart,
            StringBuilder addListenerPart, StringBuilder eventHandlerPart)
        {
            var view = go.GetComponent<BaseView>();
            if (view == null)
            {
                EditorUtility.DisplayDialog("错误", $"请先为{go.name}生成脚本", "OK");
                return null;
            }
            UiCodeWidgetGenerator.GenerateNormalViewCode(pascalGoName, view.GetType().Name, memberDeclarePart, memberCheckPart);
            return view.GetType();
        }
    }
}
