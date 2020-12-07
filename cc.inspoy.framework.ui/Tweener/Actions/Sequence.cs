// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui
// FileName: Sequence.cs
// Created on 2020/10/22 by inspoy
// All rights reserved.

using System.Collections.Generic;

namespace Instech.Framework.Ui.Tweening
{
    public class Sequence : Action
    {
        private readonly List<Action> _actions;
        private int _idx;

        public Sequence(params Action[] actions)
        {
            _actions = new List<Action>(actions);
        }

        internal override void OnRun()
        {
            _idx = 0;
            foreach (var item in _actions)
            {
                item.SetAsNested(this);
            }
        }

        internal override bool Update(float dt)
        {
            if (_idx >= _actions.Count)
            {
                return true;
            }
            var action = _actions[_idx];
            if (!action.Running)
            {
                action.OnRun();
                action.Running = true;
            }
            if (action.Update(dt))
            {
                _idx += 1;
            }
            return false;
        }
    }
}
