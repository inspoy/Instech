// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui
// FileName: ScaleTo.cs
// Created on 2020/10/22 by inspoy
// All rights reserved.

using UnityEngine;

namespace Instech.Framework.Ui.Tweening
{
    public class ScaleTo : PersistentAction
    {
        private readonly Vector3 _target;
        private Vector3 _origin;

        public ScaleTo(float target, float interval) : base(interval)
        {
            _target = target * Vector3.one;
        }

        public ScaleTo(Vector3 target, float interval) : base(interval)
        {
            _target = target;
        }

        internal override void OnRun()
        {
            base.OnRun();
            _origin = Owner.localScale;
        }

        internal override bool Update(float dt)
        {
            var ret = base.Update(dt);
            Owner.localScale = Vector3.Lerp(_origin, _target, Progress);
            return ret;
        }
    }
}
