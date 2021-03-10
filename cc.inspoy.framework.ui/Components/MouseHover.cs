// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui
// FileName: MouseHover.cs
// Created on 2021/02/02 by inspoy
// All rights reserved.

using Instech.Framework.Common;
using UnityEngine;
using EventDispatcher = Instech.Framework.Common.EventDispatcher;

namespace Instech.Framework.Ui
{
    /// <summary>
    /// 鼠标悬停检测，无需挂接Raycaster
    /// </summary>
    public class MouseHover : MonoBehaviour
    {
        [Header("悬停触发时间")]
        [Range(0, 2)]
        public float HoverTime = 0.5f;

        private RectTransform _rt;
        private EventDispatcher _dispatcher;
        private Vector3[] _worldCorners;
        private Rect _rectInScreen;
        private bool _prevOver; // 上一帧是否在范围内
        private float _enterTick;

        private void Awake()
        {
            _rt = transform as RectTransform;
            _worldCorners = new Vector3[4];
            _dispatcher = gameObject.GetDispatcher();
        }

        private void OnEnable()
        {
            CalcRect();
        }

        private void OnRectTransformDimensionsChange()
        {
            CalcRect();
        }

        private void Update()
        {
            var isOver = IsOver();
            var now = Time.realtimeSinceStartup;
            if (isOver && !_prevOver)
            {
                // enter
                _enterTick = now;
            }
            if (isOver && now - _enterTick > HoverTime)
            {
                _dispatcher.DispatchEvent(EventEnum.UiPointerHover);
            }
            if (!isOver && _prevOver)
            {
                _dispatcher.DispatchEvent(EventEnum.UiPointerExit);
            }

            _prevOver = isOver;
        }

        private bool IsOver()
        {
            var mousePos = Input.mousePosition;
            var x = Mathf.Clamp(mousePos.x, 0, Screen.width);
            var y = Mathf.Clamp(mousePos.y, 0, Screen.height);
            return _rectInScreen.Contains(new Vector2(x, y));
        }

        private void CalcRect()
        {
            _rt.GetWorldCorners(_worldCorners);
            _rectInScreen = new Rect(
                _worldCorners[0].x,
                _worldCorners[0].y,
                _worldCorners[2].x - _worldCorners[0].x,
                _worldCorners[2].y - _worldCorners[0].y);
        }
    }
}
