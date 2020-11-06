/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Ui
 * FileName: PersistentAction.cs
 * Created on 2020/10/22 by inspoy
 * All rights reserved.
 */

namespace Instech.Framework.Ui.Tweening
{
    public abstract class PersistentAction : Action
    {
        protected readonly float Interval;
        protected float Timer;
        protected float Progress => Timer / Interval;

        protected PersistentAction(float interval)
        {
            Interval = interval;
        }

        internal override void OnRun()
        {
            Timer = 0;
        }

        internal override bool Update(float dt)
        {
            Timer += dt;
            return Timer > Interval;
        }
    }
}
