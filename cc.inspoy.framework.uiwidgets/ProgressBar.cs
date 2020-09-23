/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.UiWidgets
 * FileName: ProgressBar.cs
 * Created on 2020/08/04 by inspoy
 * All rights reserved.
 */

using UnityEngine;
using UnityEngine.UI;

#if !UNITY_EDITOR
using Instech.Framework.Logging;
using Logger = Instech.Framework.Logging.Logger;
#endif

namespace Instech.Framework.UiWidgets
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
        public Vector4 Offset;

        [SerializeField]
        [Range(0, 1)]
        private float _progress;

        private void Awake()
        {
            if (Fill == null)
            {
#if UNITY_EDITOR
                var fillTrans = new GameObject("fill", typeof(RectTransform)).transform;
                fillTrans.SetParent(transform);
                fillTrans.gameObject.AddComponent<Image>();
                Fill = fillTrans.GetComponent<RectTransform>();
                _delayResetProgress = true;
#else
                Logger.LogWarning(LogModule.Ui, $"进度条{gameObject.name}没有设置Fill");
#endif
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
            Fill.anchoredPosition = new Vector2(Offset.x, Offset.y);
            Fill.sizeDelta = new Vector2(Offset.z, Offset.w);
        }

        private void Update()
        {
#if UNITY_EDITOR
            UpdateInEditor();
#endif
        }

#if UNITY_EDITOR
        private bool _delayResetProgress;

        private void OnValidate()
        {
            _delayResetProgress = true;
        }

        private void UpdateInEditor()
        {
            if (_delayResetProgress)
            {
                _delayResetProgress = false;
                ResetProgress();
            }
        }
#endif
    }
}
