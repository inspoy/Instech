// == Inspoy Technology ==
// Assembly: Instech.Framework.UiWidgets
// FileName: EventPassThrough.cs
// Created on 2020/09/19 by inspoy
// All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Instech.Framework.UiWidgets
{
    public class EventPassThrough : MonoBehaviour, IPointerClickHandler
    {
        public List<Component> PassTo;

        public void OnPointerClick(PointerEventData eventData)
        {
            foreach (var component in PassTo)
            {
                if (component is IPointerClickHandler handler)
                {
                    handler.OnPointerClick(eventData);
                }
            }
        }
    }
}
