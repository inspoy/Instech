/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: UiExtension.cs
 * Created on 2018/05/06 by inspoy
 * All rights reserved.
 */

using UnityEngine.UI;

namespace Instech.Framework
{
    public delegate void OnValueChangeListener(int val);

    public static class UiExtension
    {
        #region 扩展Button
        /// <summary>
        /// 设置Button下的Text的文本（如果有的话）
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="str"></param>
        public static void SetText(this Button btn, string str)
        {
            ButtonExtension.Get(btn.gameObject)?.SetText(str);
        }

        /// <summary>
        /// 获取Button下的Text文本（没有则为空）
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        public static string GetText(this Button btn)
        {
            var com = ButtonExtension.Get(btn.gameObject);
            return com != null ? com.GetText() : string.Empty;
        }
        #endregion

        #region 扩展ToggleGroup
        /// <summary>
        /// 为其增加一个状态变化的监听
        /// </summary>
        /// <param name="group"></param>
        /// <param name="lis"></param>
        public static void AddValueChangeListener(this ToggleGroup group, OnValueChangeListener lis)
        {
            var com = ToggleGroupExtension.Get(group.gameObject);
            if (com != null)
            {
                com.ValueChangeListener += lis;
            }
        }

        /// <summary>
        /// 为其删除一个状态变化的监听
        /// </summary>
        /// <param name="group"></param>
        /// <param name="lis"></param>
        public static void RemoveValueChangeListener(this ToggleGroup group, OnValueChangeListener lis)
        {
            var com = ToggleGroupExtension.Get(group.gameObject);
            if (com != null)
            {
                com.ValueChangeListener -= lis;
            }
        }

        /// <summary>
        /// 手动设置当前选择项
        /// </summary>
        /// <param name="group"></param>
        /// <param name="val"></param>
        public static void SetValue(this ToggleGroup group, int val)
        {
            var com = ToggleGroupExtension.Get(group.gameObject);
            if (com != null)
            {
                com.SetValue(val);
            }
        }

        /// <summary>
        /// 获取当前选择项
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static int GetValue(this ToggleGroup group)
        {
            var com = ToggleGroupExtension.Get(group.gameObject);
            if (com != null)
            {
                return com.GetValue();
            }
            return 0;
        }
        #endregion
    }
}
