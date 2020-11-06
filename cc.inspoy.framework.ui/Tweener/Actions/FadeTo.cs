/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Ui
 * FileName: FadeTo.cs
 * Created on 2020/10/22 by inspoy
 * All rights reserved.
 */

using Instech.Framework.Logging;
using UnityEngine;
using UnityEngine.UI;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Ui.Tweening
{
    public class FadeTo : PersistentAction
    {
        private readonly Color _target;
        private Graphic _graphic;
        private Color _origin;
        private bool _noGraphic;

        public FadeTo(Color target, float interval) : base(interval)
        {
            _target = target;
        }

        public FadeTo(float alpha, float interval) : base(interval)
        {
            _target = _graphic.color;
            _target.a = Mathf.Clamp01(alpha);
        }

        internal override void OnRun()
        {
            base.OnRun();
            _graphic = Owner.GetComponent<Graphic>();
            if (_graphic == null)
            {
                Logger.LogError(LogModule.Ui, "No Image component on target GameObject");
                _noGraphic = true;
                return;
            }
            _origin = _graphic.color;
        }

        internal override bool Update(float dt)
        {
            if (_noGraphic)
            {
                return true;
            }
            var ret = base.Update(dt);
            _graphic.color = Color.Lerp(_origin, _target, Progress);
            return ret;
        }
    }
}
