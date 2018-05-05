/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ToggleExtension.cs
 * Created on 2018/05/06 by inspoy
 * All rights reserved.
 */

using UnityEngine;
using UnityEngine.UI;

namespace Instech.Framework
{
    public class ToggleExtension : MonoBehaviour
    {
        /// <summary>
        /// 用于ToggleGroup的值
        /// </summary>
        public int Value;

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
        }
    }
}
