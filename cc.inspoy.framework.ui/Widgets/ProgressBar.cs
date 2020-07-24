/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: ProgressBar.cs
 * Created on 2018/05/06 by inspoy
 * All rights reserved.
 */

using Instech.Framework.Logging;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Ui
{
    [ExecuteInEditMode]
    public class ProgressBar : MonoBehaviour
    {
        /// <summary>
        /// 当前进度值，范围0-1
        /// </summary>
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = Mathf.Clamp01(value);
                ResetProgress();
            }
        }

        public RectTransform Fill;
        public Vector2 Boarder;

        [SerializeField]
        [Range(0, 1)]
        private float _progress;

        private void Awake()
        {
            if (Fill == null)
            {
                Logger.LogWarning(LogModule.Ui, $"进度条{gameObject.name}没有设置Fill");
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
            Fill.sizeDelta = Boarder;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResetProgress();
        }
#endif
    }
}
