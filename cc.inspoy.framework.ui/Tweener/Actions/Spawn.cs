/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Ui
 * FileName: Spawn.cs
 * Created on 2020/10/22 by inspoy
 * All rights reserved.
 */

using System.Collections.Generic;

namespace Instech.Framework.Ui.Tweening
{
    public class Spawn : Action
    {
        private readonly List<Action> _actions;

        public Spawn(params Action[] actions)
        {
            _actions = new List<Action>(actions);
        }

        internal override void OnRun()
        {
            foreach (var item in _actions)
            {
                item.SetAsNested(this);
                item.OnRun();
                item.Running = true;
            }
        }

        internal override bool Update(float dt)
        {
            var running = false;
            foreach (var action in _actions)
            {
                if (action.Running)
                {
                    running = true;
                    var finished = action.Update(dt);
                    if (finished)
                    {
                        action.Running = false;
                    }
                }
            }
            return !running;
        }
    }
}
