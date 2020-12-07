// == Inspoy Technology ==
// Assembly: Instech.Framework.UiWidgets
// FileName: ClickMask.cs
// Created on 2020/09/19 by inspoy
// All rights reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Instech.Framework.UiWidgets
{
    public class ClickMask : Graphic, ICanvasRaycastFilter
    {
        public PolygonCollider2D Collider;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (Collider == null)
            {
                return true;
            }
            Vector2 local;
            local.x = sp.x - Screen.width / 2.0f;
            local.y = sp.y - Screen.height / 2.0f;
            var crossCount = 0;
            var points = Collider.points;
            var i = 0;
            var j = points.Length - 1;
            while (i < points.Length)
            {
                if (points[i].y > local.y != points[j].y > local.y &&
                    local.x < (points[j].x - points[i].x) / (points[j].y - points[i].y) * (local.y - points[i].y) + points[i].x)
                {
                    crossCount += 1;
                }
                j = i++;
            }
            return (crossCount & 1) == 1;
        }
    }
}
