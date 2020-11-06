/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Ui
 * FileName: Tweener.cs
 * Created on 2020/10/21 by inspoy
 * All rights reserved.
 */

using System.Collections.Generic;
using Instech.Framework.Logging;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Ui.Tweening
{
    public class Tweener
    {
        public static int RunningActions => Instance?._actions.Count ?? -1;
        internal static Tweener Instance;

        private readonly List<Action> _actions = new List<Action>();
        private readonly List<Action> _delayAddActions = new List<Action>();
        private readonly List<Action> _delayRemoveActions = new List<Action>();
        private bool _updating;

        public Tweener()
        {
            if (Instance != null)
            {
                Logger.LogError(LogModule.Ui, "Tweener should have only one instance");
                return;
            }
            Instance = this;
        }

        public void UpdateFrame(float dt)
        {
            _updating = true;
            for (var i = _actions.Count - 1; i >= 0; --i)
            {
                var action = _actions[i];
                var finished = action.Owner == null || action.Update(dt);
                if (finished)
                {
                    _actions.RemoveAt(i);
                }
            }
            _updating = false;
            foreach (var action in _delayAddActions)
            {
                action.Running = true;
                _actions.Add(action);
            }
            _delayAddActions.Clear();
            foreach (var action in _delayRemoveActions)
            {
                action.Running = false;
                _actions.Remove(action);
            }
            _delayRemoveActions.Clear();
        }

        public void RunAction(Action action)
        {
            if (_updating)
            {
                _delayAddActions.Add(action);
                return;
            }
            action.Running = true;
            _actions.Add(action);
        }

        public void Stop(Action action)
        {
            if (_updating)
            {
                _delayRemoveActions.Add(action);
                return;
            }
            action.Running = false;
            _actions.Remove(action);
        }
    }
}
