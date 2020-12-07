// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui
// FileName: ToggleGroupExtension.cs
// Created on 2018/05/06 by inspoy
// All rights reserved.

using System.Collections.Generic;
using Instech.Framework.Logging;
using Instech.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Ui
{
    /// <summary>
    /// 注意：这个组件是自动添加的
    /// 扩展ToggleGroup，实现可以方便监控当前选定项的单选框组
    /// 要给每个Toggle添加ToggleExtension，并设置Value
    /// 当组内的Toggle选择状态变化时，触发UiValueChange事件
    /// </summary>
    public class ToggleGroupExtension: MonoBehaviour
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
            var group = gameObject.GetComponent<ToggleGroup>();
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
                    Logger.LogWarning(LogModule.Ui, $"{gameObject.name}有重复的Toggle Value:{toggleEx.Value}, path={child.transform.GetHierarchyPath()}");
                    continue;
                }
                if (toggleEx.Value <= 0)
                {
                    Logger.LogWarning(LogModule.Ui, $"Toggle Value必须大于0: {child.transform.GetHierarchyPath()}");
                    continue;
                }
                _dictToggles.Add(toggleEx.Value, toggleEx);
                child.onValueChanged.AddListener(isChecked =>
                {
                    if (isChecked && _curVal != toggleEx.Value)
                    {
                        var oldVal = _curVal;
                        _curVal = toggleEx.Value;
                        ValueChangeListener?.Invoke(_curVal, oldVal);
                    }
                    else if (group.allowSwitchOff)
                    {
                        var oldVal = _curVal;
                        _curVal = 0;
                        ValueChangeListener?.Invoke(_curVal, oldVal);
                    }
                });
            }
        }
    }
}