/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Ui
 * FileName: MoveTo.cs
 * Created on 2020/10/21 by inspoy
 * All rights reserved.
 */

using UnityEngine;

namespace Instech.Framework.Ui.Tweening
{
    public class MoveTo : PersistentAction
    {
        private readonly Vector2 _target;
        private Vector2 _origin;

        public MoveTo(Vector2 target, float interval) : base(interval)
        {
            _target = target;
        }

        internal override void OnRun()
        {
            base.OnRun();
            _origin = Owner.anchoredPosition;
        }

        internal override bool Update(float dt)
        {
            var finished = base.Update(dt);
            Owner.anchoredPosition = Vector2.Lerp(_origin, _target, Progress);
            return finished;
        }
    }
}
