// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui
// FileName: ToggleExtension.cs
// Created on 2018/05/06 by inspoy
// All rights reserved.

using Instech.Framework.Logging;
using Instech.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Ui
{
    public class ToggleExtension:MonoBehaviour
    {
        /// <summary>
        /// 用于ToggleGroup的值
        /// </summary>
        public int Value;

        public GameObject ImageSelected;
        public GameObject ImageUnselected;

        /// <summary>
        /// 对应的Toggle控件
        /// </summary>
        [HideInInspector]
        public Toggle Toggle;

        private bool _inited;

        public void Awake()
        {
            if (_inited)
            {
                return;
            }
            _inited = true;
            Toggle = gameObject.GetComponent<Toggle>();
            if (Toggle == null)
            {
                Logger.LogWarning(LogModule.Ui, "Cannot get toggle component in go: " + transform.GetHierarchyPath());
                return;
            }
            Toggle.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(Toggle.isOn);
        }

        private void OnValueChanged(bool isOn)
        {
            if (ImageSelected != null)
            {
                ImageSelected.SetActive(isOn);
            }
            if (ImageUnselected != null)
            {
                ImageUnselected.SetActive(!isOn);
            }
        }
    }
}