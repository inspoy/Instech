// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui
// FileName: Action.cs
// Created on 2020/10/21 by inspoy
// All rights reserved.

using Instech.Framework.Logging;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Ui.Tweening
{
    public abstract class Action
    {
        internal bool Running;
        protected bool Nested;
        internal RectTransform Owner { get; private set; }

        public void Run(RectTransform owner)
        {
            if (Nested)
            {
                Logger.LogWarning(LogModule.Ui, "Cannot run a nested action");
                return;
            }
            if (Running)
            {
                Logger.LogWarning(LogModule.Ui, "Action is already running");
                return;
            }
            Owner = owner;
            OnRun();
            Tweener.Instance.RunAction(this);
        }

        public void Stop()
        {
            if (!Running)
            {
                Logger.LogWarning(LogModule.Ui, "Action is not running");
                return;
            }
            Tweener.Instance.Stop(this);
        }

        internal void SetAsNested(Action parent)
        {
            Nested = true;
            Owner = parent.Owner;
        }

        internal abstract void OnRun();
        internal abstract bool Update(float dt);
    }
}
