/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ProgressBar.cs
 * Created on 2018/05/06 by inspoy
 * All rights reserved.
 */

using UnityEngine;

namespace Instech.Framework
{
    [ExecuteInEditMode]
    public class ProgressBar : MonoBehaviour
    {
        /// <summary>
        /// 当前进度值
        /// </summary>
        public float Progress
        {
            get { return _progress; }
            set
            {
                _progress = Mathf.Clamp01(value);
                ResetProgress();
            }
        }

        public RectTransform Fill;

        [SerializeField]
        [Range(0, 1)]
        private float _progress;

        private void Awake()
        {
            if (Fill == null)
            {
                Logger.LogWarning("UI", $"进度条{gameObject.name}没有设置Fill");
            }
        }

        private void ResetProgress()
        {
            if (Fill == null)
            {
                return;
            }
            Fill.anchorMin = Vector2.zero;
            Fill.anchorMax = new Vector2(_progress, 1);
            Fill.anchoredPosition = Vector2.zero;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResetProgress();
        }
#endif
    }
}
