/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Ui
 * FileName: RotateTo.cs
 * Created on 2020/10/22 by inspoy
 * All rights reserved.
 */

using UnityEngine;

namespace Instech.Framework.Ui.Tweening
{
    public class RotateTo : PersistentAction
    {
        private readonly float _target;
        private Vector3 _origin;
        public RotateTo(float target,float interval) : base(interval)
        {
            _target = target;
        }

        internal override void OnRun()
        {
            base.OnRun();
            _origin = Owner.localEulerAngles;
        }

        internal override bool Update(float dt)
        {
            var ret= base.Update(dt);
            var newAngles = _origin;
            newAngles.z = Mathf.Lerp(_origin.z, _target, Progress);
            Owner.localEulerAngles = newAngles;
            return ret;
        }
    }
}
