/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Ui
 * FileName: MoveBy.cs
 * Created on 2020/10/22 by inspoy
 * All rights reserved.
 */

using UnityEngine;

namespace Instech.Framework.Ui.Tweening
{
    public class MoveBy : PersistentAction
    {
        private readonly Vector2 _delta;
        private Vector2 _origin;
        private Vector2 _target;

        public MoveBy(Vector2 delta, float interval) : base(interval)
        {
            _delta = delta;
        }

        internal override void OnRun()
        {
            base.OnRun();
            _origin = Owner.anchoredPosition;
            _target = _origin + _delta;
        }

        internal override bool Update(float dt)
        {
            var ret = base.Update(dt);
            Owner.anchoredPosition = Vector2.Lerp(_origin, _target, Progress);
            return ret;
        }
    }
}
