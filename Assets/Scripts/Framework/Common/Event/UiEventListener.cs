/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: UiEventListener.cs
 * Created on 2018/05/03 by inspoy
 * All rights reserved.
 */

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
        [HideInInspector] public EventDispatcher Dispatcher;

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
            Dispatcher?.DispatchEvent(EventEnum.UiPointerDown);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiPointerUp);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiPointerEnter);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiPointerExit);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiSelect);
        }

        public override void OnUpdateSelected(BaseEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiUpdateSelected);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiSubmit);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiBeginDrag);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiDrag);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiEndDrag);
        }
        
        private void Start()
        {
            // 处理特殊事件，EventTrigger不提供的事件
            var toggleGroupCom = gameObject.GetComponent<ToggleGroup>();
            if (toggleGroupCom != null)
            {
                // 这是个ToggleGroup，单选框组
                // TODO: 等待ToggleGroup扩展完成
                // toggleGroupCom.AddValueChangeListener(OnValueChange);
            }

            var toggleCom = gameObject.GetComponent<Toggle>();
            if (toggleCom != null)
            {
                // 这是个Toggle，复选框
                toggleCom.onValueChanged.AddListener(OnToggleChange);
            }

            var inputCom = gameObject.GetComponent<InputField>();
            if (inputCom != null)
            {
                // 这是个InputField，输入框
                inputCom.onValueChanged.AddListener(OnInputChange);
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
            Dispatcher?.DispatchEvent(EventEnum.UiToggleChange, new SimpleEventData(selected));
        }

        private void OnValueChange(int selectId)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiValueChange, new SimpleEventData(selectId));
        }

        private void OnInputChange(string val)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiValueChange, new SimpleEventData(val));
        }

        private void OnSliderChange(float val)
        {
            Dispatcher?.DispatchEvent(EventEnum.UiValueChange, new SimpleEventData(val));
        }

        private void OnDestroy()
        {
            Dispatcher?.RemoveAllEventListeners();
            Dispatcher = null;
        }
    }
}
