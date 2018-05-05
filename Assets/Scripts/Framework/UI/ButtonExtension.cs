/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ButtonExtension.cs
 * Created on 2018/05/06 by inspoy
 * All rights reserved.
 */

using UnityEngine;
using UnityEngine.UI;

namespace Instech.Framework
{
    public class ButtonExtension : MonoBehaviour
    {
        /// <summary>
        /// ��ť�ӽڵ��а�����Text���
        /// </summary>
        public Text Text;

        public static ButtonExtension Get(GameObject go)
        {
            if (go == null)
            {
                return null;
            }
            var com = go.GetComponent<ButtonExtension>();
            if (com == null)
            {
                com = go.AddComponent<ButtonExtension>();
            }
            return com;
        }

        public void SetText(string str)
        {
            if (Text != null)
            {
                Text.text = str;
            }
        }

        public string GetText()
        {
            return Text != null ? Text.text : string.Empty;
        }

        private void Awake()
        {
            if (Text == null)
            {
                Text = GetComponentInChildren<Text>();
            }
        }
    }
}
