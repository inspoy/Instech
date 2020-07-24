/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Data
 * FileName: LocalizedUiWidget.cs
 * Created on 2019/12/16 by inspoy
 * All rights reserved.
 */

using Instech.Framework.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Event = Instech.Framework.Common.Event;

namespace Instech.Framework.Data
{
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
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                LocalizationManager.Instance.Dispatcher.AddEventListener(EventEnum.LanguageChange, OnLanguageChanged);
                OnLanguageChanged(null);
            }
#else
            LocalizationManager.Instance.Dispatcher.AddEventListener(EventEnum.LanguageChange, OnLanguageChanged);
            OnLanguageChanged(null);
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                LocalizationManager.Instance.Dispatcher.RemoveEventListener(EventEnum.LanguageChange, OnLanguageChanged);
            }
#else
            LocalizationManager.Instance.Dispatcher.RemoveEventListener(EventEnum.LanguageChange, OnLanguageChanged);
#endif
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