/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: EnumLabel.cs
 * Created on 2018/05/20 by inspoy
 * All rights reserved.
 */

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endif

namespace Instech.Framework
{
    /// <summary>
    /// 用于显示枚举的别名
    /// 用法：在枚举定义处加入此属性，就可以在Inspector面板中看到相应的别名了
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class EnumLabelAttribute : PropertyAttribute
    {
        public string Label { get; }

        public EnumLabelAttribute(string label)
        {
            Label = label;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnumLabelAttribute))]
    public class EnumLabelDrawer : PropertyDrawer
    {
        private EnumLabelAttribute EnumLabelAttribute => (EnumLabelAttribute)attribute;

        private readonly Dictionary<string, string> _customEnumNames = new Dictionary<string, string>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetUpCustomEnumNames(property, property.enumNames);

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                EditorGUI.BeginChangeCheck();
                var displayedOptions = property.enumNames
                    .Where(enumName => _customEnumNames.ContainsKey(enumName))
                    .Select(enumName => _customEnumNames[enumName])
                    .ToArray();
                var selectedIndex = EditorGUI.Popup(position, EnumLabelAttribute.Label, property.enumValueIndex, displayedOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    property.enumValueIndex = selectedIndex;
                }
            }
        }

        private void SetUpCustomEnumNames(SerializedProperty property, string[] enumNames)
        {
            var type = property.serializedObject.targetObject.GetType();
            foreach (var item in type.GetFields())
            {
                var customAttributes = item.GetCustomAttributes(typeof(EnumLabelAttribute), false);
                ProcessSingle(item, customAttributes, enumNames);
            }
        }

        private void ProcessSingle(FieldInfo item, object[] attributes, string[] enumNames)
        {
            foreach (EnumLabelAttribute customAttribute in attributes)
            {
                var enumType = item.FieldType;
                foreach (var enumName in enumNames)
                {
                    var field = enumType.GetField(enumName);
                    if (field == null)
                    {
                        continue;
                    }
                    var attrs = (EnumLabelAttribute[])field.GetCustomAttributes(customAttribute.GetType(), false);

                    if (_customEnumNames.ContainsKey(enumName))
                    {
                        continue;
                    }
                    foreach (var labelAttribute in attrs)
                    {
                        _customEnumNames.Add(enumName, labelAttribute.Label);
                    }
                }
            }
        }
    }
#endif
}
