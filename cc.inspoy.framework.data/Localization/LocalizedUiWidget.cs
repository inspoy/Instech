// == Inspoy Technology ==
// Assembly: Instech.Framework.Data
// FileName: LocalizedUiWidget.cs
// Created on 2019/12/16 by inspoy
// All rights reserved.

using System;
using System.Linq;
using Instech.Framework.Common;
using Instech.Framework.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Event = Instech.Framework.Common.Event;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Data
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(LocalizedUiWidget))]
    public class LocalizedUiWidgetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("切换语言");
            var options = LocalizationManager.Instance.AllLoadedLanguage.ToArray();
            var idx = Array.IndexOf(options, LocalizationManager.Instance.CurLanguageId);
            var newIdx = EditorGUILayout.Popup(idx, options);
            if (newIdx != idx)
            {
                LocalizationManager.Instance.SetLanguage(options[newIdx]);
            }

            if (GUILayout.Button("重载本地化表"))
            {
                var count = LocalizationManager.Instance.ReloadConfig();
                Logger.LogInfo(LogModule.Editor, $"重载成功，加载了{count}个语言包");
                LocalizationManager.Instance.SetLanguage(LocalizationManager.Instance.CurLanguageId);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
#endif

    [ExecuteInEditMode]
    public class LocalizedUiWidget : MonoBehaviour
    {
        [Header("本地化Key")] public string Alias;

        private TextMeshProUGUI _text;
        private Image _img;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _img = GetComponent<Image>();
        }

        private void OnLanguageChanged(Event e)
        {
            if (_text != null)
            {
                if (string.IsNullOrWhiteSpace(Alias))
                {
                    _text.SetText("{L10N:NOT_SPECIFIED}");
                }
                else
                {
                    _text.SetText(L.N(Alias));
                }
            }

            if (_img != null)
            {
                // TODO
            }
        }

        private void OnEnable()
        {
            if (LocalizationManager.HasSingleton())
            {
                LocalizationManager.Instance.Dispatcher.AddEventListener(EventEnum.LanguageChange, OnLanguageChanged);
                OnLanguageChanged(null);
            }
        }

        private void OnDisable()
        {
            if (LocalizationManager.HasSingleton())
            {
                LocalizationManager.Instance.Dispatcher.RemoveEventListener(EventEnum.LanguageChange,
                    OnLanguageChanged);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_text == null)
            {
                _text = GetComponent<TextMeshProUGUI>();
            }

            if (_img == null)
            {
                _img = GetComponent<Image>();
            }

            if (!LocalizationManager.HasSingleton())
            {
                LocalizationManager.CreateSingleton();
                LocalizationManager.Instance.SetLanguage("zh-CN");
            }

            OnLanguageChanged(null);
        }
#endif
    }
}
