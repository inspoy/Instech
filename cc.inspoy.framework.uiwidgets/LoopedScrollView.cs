/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.UiWidgets
 * FileName: LoopedScrollView.cs
 * Created on 2020/09/18 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using Instech.Framework.Common;
using Instech.Framework.Logging;
using Instech.Framework.Ui;
using Instech.Framework.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Event = Instech.Framework.Common.Event;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.UiWidgets
{
    /// <summary>
    /// 循环列表
    /// </summary>
    public class LoopedScrollView : MonoBehaviour
    {
        #region Serialized Fields

        public enum ScrollDirection
        {
            Horizontal,
            Vertical
        }

        /// <summary>
        /// 滚动方向
        /// </summary>
        public ScrollDirection Direction;

        public BaseView BaseItem;
        public RectTransform Touch;

        #endregion

        #region Private Fields

        private readonly List<BaseView> _cachedViews = new List<BaseView>();
        private readonly List<int> _cachedViewsIdx = new List<int>();

        /// <summary>
        /// Item尺寸，横向滚动时为宽度，纵向滚动时为高度
        /// </summary>
        private float _itemSize;

        private float _touchSize;
        private int _capacity;
        private float _curOffset;
        private bool _isDragging;
        private float _draggingSpeed;
        private float _targetOffset;
        private bool _isSeeking;
        private float _seekingSpeed;

        #endregion

        #region Interfaces & Properties

        public BaseView OwnerView { get; set; }
        public Action<int, BaseView> OnItemShowHandler { private get; set; }
        public uint ItemCount { get; set; }
        public bool DragEnabled { get; set; } = true;

        public void Refresh()
        {
            var maxOffset = Mathf.Max(0, ItemCount * _itemSize - _touchSize);
            _curOffset = Mathf.Clamp(_curOffset, 0, maxOffset);
            RefreshItemPositions(true);
        }

        public void SeekToIndex(int idx, bool smooth = true)
        {
            _targetOffset = _itemSize * idx;
            if (smooth)
            {
                _isSeeking = true;
                _seekingSpeed = 1000;
            }
            else
            {
                _isSeeking = false;
                _curOffset = _targetOffset;
            }
        }

        #endregion

        #region Unity Events

#if UNITY_EDITOR
        private void Awake()
        {
            if (transform.childCount == 0)
            {
                // 新添加的组件
                var touchGo = new GameObject("Touch");
                touchGo.transform.SetParent(transform);
                Touch = touchGo.AddComponent<RectTransform>();
                Touch.anchoredPosition = Vector2.zero;
                Touch.sizeDelta = Vector2.zero;
                Touch.anchorMin = Vector2.zero;
                Touch.anchorMax = Vector2.one;
                touchGo.AddComponent<RectMask2D>();
                touchGo.AddComponent<CanvasRenderer>();
                touchGo.AddComponent<ClickMask>();
            }
        }
#endif

        private void Start()
        {
            // 检查BaseItem的有效性
            if (!CheckBaseItem())
            {
                return;
            }
            BaseItem.gameObject.SetActive(false);

            // 计算滚动框可以容纳多少Item
            var sizeDelta = BaseItem.RectTransform.sizeDelta;
            _itemSize = Direction == ScrollDirection.Horizontal ? sizeDelta.x : sizeDelta.y;
            var rect = Touch.rect;
            _touchSize = Direction == ScrollDirection.Horizontal ? rect.width : rect.height;
            _capacity = Mathf.CeilToInt(_touchSize / _itemSize) + 1;

            // 分配缓存用
            AssignWithCapacity();

            // 注册拖动事件
            var dispatcher = Touch.gameObject.GetDispatcher();
            dispatcher.AddEventListener(EventEnum.UiBeginDrag, OnTouchDragBegin);
            dispatcher.AddEventListener(EventEnum.UiDrag, OnTouchDrag);
            dispatcher.AddEventListener(EventEnum.UiEndDrag, OnTouchDragEnd);
        }

        private void Update()
        {
            if (_isSeeking)
            {
                if (Mathf.Abs(_targetOffset - _curOffset) < 0.1f)
                {
                    _curOffset = _targetOffset;
                    _isSeeking = false;
                }
                else if (_targetOffset > _curOffset)
                {
                    _curOffset += _seekingSpeed * Time.deltaTime;
                    if (_curOffset > _targetOffset)
                    {
                        _curOffset = _targetOffset;
                        _isSeeking = false;
                    }
                }
                else
                {
                    _curOffset -= _seekingSpeed * Time.deltaTime;
                    if (_curOffset < _targetOffset)
                    {
                        _curOffset = _targetOffset;
                        _isSeeking = false;
                    }
                }
                RefreshItemPositions();
            }
            if (!_isDragging)
            {
                var maxOffset = Mathf.Max(0, ItemCount * _itemSize - _touchSize);
                if (_curOffset < -0.1f)
                {
                    _curOffset = Mathf.Lerp(_curOffset, 0, 0.1f);
                    _draggingSpeed = 0;
                    RefreshItemPositions();
                }
                else if (_curOffset > maxOffset + 0.1f)
                {
                    _curOffset = Mathf.Lerp(_curOffset, maxOffset, 0.1f);
                    _draggingSpeed = 0;
                    RefreshItemPositions();
                }
                else if (Mathf.Abs(_draggingSpeed) > 0.1f)
                {
                    _curOffset += _draggingSpeed;
                    _draggingSpeed *= 0.9f;
                    RefreshItemPositions();
                }
                else
                {
                    _draggingSpeed = 0;
                }
            }
        }

        #endregion

        #region Methods

        private void OnTouchDrag(Event e)
        {
            var data = e.GetData<UnityEventData>().Content as PointerEventData;
            if (data == null || !DragEnabled)
            {
                return;
            }
            var delta = Direction == ScrollDirection.Horizontal ? data.delta.x : data.delta.y;
            _draggingSpeed = -delta;
            var maxOffset = Mathf.Max(0, ItemCount * _itemSize - _touchSize);
            if (_curOffset < 0)
            {
                var dx = (_curOffset + _itemSize) / _itemSize;
                _curOffset -= delta * Interpolation.Calc(dx, 0, 1, EaseType.CircleIn);
            }
            else if (_curOffset < maxOffset)
            {
                _curOffset -= delta;
            }
            else
            {
                var dx = (maxOffset + _itemSize - _curOffset) / _itemSize;
                _curOffset -= delta * Interpolation.Calc(dx, 0, 1, EaseType.CircleIn);
            }
            _curOffset = Mathf.Clamp(_curOffset, -_itemSize + 0.1f, maxOffset + _itemSize - 0.1f);
            RefreshItemPositions();
        }

        private void OnTouchDragBegin(Event e)
        {
            if (DragEnabled)
            {
                _isDragging = true;
                if (_isSeeking)
                {
                    _isSeeking = false;
                }
            }
        }

        private void OnTouchDragEnd(Event e)
        {
            _isDragging = false;
        }

        private bool CheckBaseItem()
        {
            if (BaseItem == null)
            {
                Logger.LogWarning(LogModule.Ui, "BaseItem was not set", gameObject);
                return false;
            }
            var rt = BaseItem.RectTransform;
            if (rt != null && Direction == ScrollDirection.Horizontal)
            {
                if (Math.Abs(rt.anchorMin.x - rt.anchorMax.x) < Mathf.Epsilon)
                {
                    return true;
                }
                Logger.LogWarning(LogModule.Ui, "BaseItem's anchorMin.x must equal to anchorMax.x", BaseItem);
                return false;
            }
            if (rt != null && Direction == ScrollDirection.Vertical)
            {
                if (Math.Abs(rt.anchorMin.y - rt.anchorMax.y) < Mathf.Epsilon)
                {
                    return true;
                }
                Logger.LogWarning(LogModule.Ui, "BaseItem's anchorMin.y must equal to anchorMax.y", BaseItem);
                return false;
            }
            return false;
        }

        private void AssignWithCapacity()
        {
            if (_capacity < _cachedViews.Count)
            {
                var rmCnt = _cachedViews.Count - _capacity;
                for (var i = 0; i < rmCnt; ++i)
                {
                    var last = _cachedViews[_cachedViews.Count - 1];
                    last.Close();
                    _cachedViews.RemoveAt(_cachedViews.Count - 1);
                }
            }
            if (_capacity > _cachedViews.Count)
            {
                for (var i = _cachedViews.Count; i < _capacity; ++i)
                {
                    var newOne = UiManager.Instance.CloneView(BaseItem);
                    newOne.gameObject.SetActive(false);
                    _cachedViews.Add(newOne);
                    _cachedViewsIdx.Add(0);
                }
            }
        }

        private void RefreshItemPositions(bool forceUpdate = false)
        {
            var dir = Direction == ScrollDirection.Horizontal ? Vector2.right : Vector2.down;
            var logicIndex = Mathf.FloorToInt(Mathf.Clamp(_curOffset / _itemSize, 0, ItemCount - 1));
            var firstIndex = logicIndex % _capacity;
//            print($"refresh, idx={logicIndex}, offset={_curOffset}, spd={_draggingSpeed}, dragging={_isDragging}");
            for (var i = 0; i < _capacity; ++i)
            {
                var itemIndex = (i + firstIndex) % _capacity;
                var dataIndex = i + logicIndex;
                if (dataIndex < 0 || dataIndex >= ItemCount)
                {
                    _cachedViews[itemIndex].gameObject.SetActive(false);
                    continue;
                }
                _cachedViews[itemIndex].RectTransform.anchoredPosition = (i * _itemSize - _curOffset % _itemSize) * dir;
                _cachedViews[itemIndex].gameObject.SetActive(true);
                var needRefresh = _cachedViewsIdx[itemIndex] != dataIndex;
                if (needRefresh || forceUpdate)
                {
                    _cachedViewsIdx[itemIndex] = dataIndex;
                    OnItemShowHandler?.Invoke(dataIndex, _cachedViews[itemIndex]);
                }
            }
        }

        #endregion
    }
}
