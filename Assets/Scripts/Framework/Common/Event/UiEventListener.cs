/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: UiEventListener.cs
 * Created on 2018/05/03 by inspoy
 * All rights reserved.
 */

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Instech.Framework
{
    public static class UiEventListenerExtension
    {
        /// <summary>
        /// 根据UI控件获取它的事件派发器，没有的话会自动创建
        /// </summary>
        /// <param name="go">指定的UI控件</param>
        /// <returns>它的事件派发器</returns>
        public static EventDispatcher GetDispatcher(this GameObject go)
        {
            var listener = go.GetComponent<UiEventListener>();
            if (listener == null)
            {
                listener = go.AddComponent<UiEventListener>();
            }
            return listener.Dispatcher ?? (listener.Dispatcher = new EventDispatcher(go));
        }
    }

    public class UiEventListener : EventTrigger
    {
        /// <summary>
        /// 事件派发器
        /// </summary>
        public EventDispatcher Dispatcher { get; internal set; }

        public override void OnPointerClick(PointerEventData eventData)
        {
            var interactable = true;
            var btn = gameObject.GetComponent<Button>();
            if (btn)
            {
                interactable = btn.interactable;
            }
            if (interactable)
            {
                Dispatcher?.DispatchEvent(EventEnum.UiPointerClick);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiPointerDown, UnityEventData.GetNewOne(eventData));
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiPointerUp, UnityEventData.GetNewOne(eventData));
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiPointerEnter, UnityEventData.GetNewOne(eventData));
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiPointerExit, UnityEventData.GetNewOne(eventData));
        }

        public override void OnSelect(BaseEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiSelect, UnityEventData.GetNewOne(eventData));
        }

        public override void OnUpdateSelected(BaseEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiUpdateSelected, UnityEventData.GetNewOne(eventData));
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiSubmit, UnityEventData.GetNewOne(eventData));
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiBeginDrag, UnityEventData.GetNewOne(eventData));
        }

        public override void OnDrag(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiDrag, UnityEventData.GetNewOne(eventData));
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiEndDrag, UnityEventData.GetNewOne(eventData));
        }

        private void Start()
        {
            // 处理特殊事件，EventTrigger不提供的事件
            var toggleGroupCom = gameObject.GetComponent<ToggleGroup>();
            if (toggleGroupCom != null)
            {
                // 这是个ToggleGroup，单选框组
                toggleGroupCom.AddValueChangeListener(OnToggleValueChange);
            }

            var toggleCom = gameObject.GetComponent<Toggle>();
            if (toggleCom != null)
            {
                // 这是个Toggle，复选框
                toggleCom.onValueChanged.AddListener(OnToggleChange);
            }

            var inputCom = gameObject.GetComponent<TMP_InputField>();
            if (inputCom != null)
            {
                // 这是个InputField，输入框
                inputCom.onValueChanged.AddListener(OnInputChange);
                inputCom.onSubmit.AddListener(OnInputSubmit);
            }

            var sliderCom = gameObject.GetComponent<Slider>();
            if (sliderCom != null)
            {
                // 这是个Slider，滑动条
                sliderCom.onValueChanged.AddListener(OnSliderChange);
            }
        }

        private void OnToggleChange(bool selected)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiToggleChange, SimpleEventData.GetNewOne(selected));
        }

        private void OnToggleValueChange(int selectId)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiValueChange, SimpleEventData.GetNewOne(selectId));
        }

        private void OnInputChange(string val)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiValueChange, SimpleEventData.GetNewOne(val));
        }

        private void OnSliderChange(float val)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiValueChange, SimpleEventData.GetNewOne(val));
        }

        private void OnInputSubmit(string val)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiSubmit, SimpleEventData.GetNewOne(val));
        }

        private void OnDestroy()
        {
            Dispatcher?.RemoveAllEventListeners();
            Dispatcher = null;
        }
    }
}
