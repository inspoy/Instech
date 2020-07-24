/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Common
 * FileName: Scheduler.cs
 * Created on 2019/01/06 by inspoy
 * All rights reserved.
 */

using System;
using Instech.Framework.Core;
using Instech.Framework.Logging;

namespace Instech.Framework.Common
{
    /// <summary>
    /// 常用异步操作，以及Update驱动
    /// </summary>
    public partial class Scheduler : MonoSingleton<Scheduler>
    {
        public static event Action OnQuit;
        private bool _updateLock;
        private bool _fixedUpdateLock;
        private bool _lateUpdateLock;

        protected override void Init()
        {
            InitTimer();
            InitUpdator();
        }

        private void Update()
        {
            _updateLock = true;
            UpdateFrame();
            _updateLock = false;
        }

        private void FixedUpdate()
        {
            _fixedUpdateLock = true;
            UpdateLogic();
            _fixedUpdateLock = false;
        }

        private void LateUpdate()
        {
            _lateUpdateLock = true;
            UpdateTimer();
            UpdateLateFrame();
            _lateUpdateLock = false;
        }

        private void OnApplicationQuit()
        {
            OnQuit?.Invoke();
        }
    }
}
