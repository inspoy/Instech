/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ToggleGroupExtension.cs
 * Created on 2018/05/06 by inspoy
 * All rights reserved.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Instech.Framework
{
    public class ToggleGroupExtension : MonoBehaviour
    {
        private int _curVal;
        private bool _inited;
        private Dictionary<int, ToggleExtension> _dictToggles;

        public event OnValueChangeListener ValueChangeListener;

        public static ToggleGroupExtension Get(GameObject go)
        {
            if (go == null)
            {
                return null;
            }
            var com = go.GetComponent<ToggleGroupExtension>();
            if (com == null)
            {
                com = go.AddComponent<ToggleGroupExtension>();
            }
            if (!com._inited)
            {
                com.Awake();
            }
            return com;
        }

        /// <summary>
        /// 获取当前选择项
        /// </summary>
        /// <returns></returns>
        public int GetValue()
        {
            var ret = 0;
            foreach (var item in _dictToggles.Values)
            {
                if (item.Toggle.isOn)
                {
                    ret = item.Value;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 手动设置当前选择项
        /// </summary>
        /// <param name="val"></param>
        public void SetValue(int val)
        {
            foreach (var item in _dictToggles.Values)
            {
                item.Toggle.isOn = item.Value == val;
            }
        }

        private void Awake()
        {
            if (_inited)
            {
                return;
            }
            _inited = true;
            _dictToggles = new Dictionary<int, ToggleExtension>();
            _curVal = 0;
            var children = gameObject.GetComponentsInChildren<Toggle>();
            foreach (var child in children)
            {
                var toggleEx = child.gameObject.GetComponent<ToggleExtension>();
                if (toggleEx == null)
                {
                    continue;
                }
                toggleEx.Awake();
                if (_dictToggles.ContainsKey(toggleEx.Value))
                {
                    Debug.LogWarningFormat("{0}有重复的Toggle Value:{1}", gameObject.name, toggleEx.Value);
                    continue;
                }
                _dictToggles.Add(toggleEx.Value, toggleEx);
                child.onValueChanged.AddListener(isChecked =>
                {
                    if (isChecked && _curVal != toggleEx.Value)
                    {
                        _curVal = toggleEx.Value;
                        ValueChangeListener?.Invoke(_curVal);
                    }
                });
            }
        }
    }
}
