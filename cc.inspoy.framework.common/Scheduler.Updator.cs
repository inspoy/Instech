/**
 * == CountryRailway ==
 * Assembly: 
 * FileName: Scheduler.Updator.cs
 * Created on 2020/07/13 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using Instech.Framework.Logging;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Common
{
    /// <summary>
    /// 试图为一个listener添加同种类Update超过一次时产生此异常
    /// </summary>
    public sealed class DuplicateListenerException : Exception
    {
        public DuplicateListenerException(string type) : base("Each listener can only register once for: " + type)
        {
        }
    }

    public partial class Scheduler
    {
        #region Static Interfaces

        public static void RegisterFrameUpdate(object listener, UpdateHandler handler)
        {
            Instance.AddHandler(Instance._updateLock,
                Instance._frameUpdateHandlers, Instance._frameUpdateHandlersLateAdd,
                listener, handler);
        }

        public static void UnregisterFrameUpdate(object listener)
        {
            Instance.RemoveHandler(Instance._updateLock,
                Instance._frameUpdateHandlers, Instance._frameUpdateHandlersLateRemove,
                listener);
        }

        public static void RegisterLogicUpdate(object listener, UpdateHandler handler)
        {
            Instance.AddHandler(Instance._fixedUpdateLock,
                Instance._logicUpdateHandlers, Instance._logicUpdateHandlersLateAdd,
                listener, handler);
        }

        public static void UnregisterLogicUpdate(object listener)
        {
            Instance.RemoveHandler(Instance._fixedUpdateLock,
                Instance._logicUpdateHandlers, Instance._logicUpdateHandlersLateRemove,
                listener);
        }

        public static void RegisterLateUpdate(object listener, UpdateHandler handler)
        {
            Instance.AddHandler(Instance._fixedUpdateLock,
                Instance._lateUpdateHandlers, Instance._lateUpdateHandlersLateAdd,
                listener, handler);
        }

        public static void UnregisterLateUpdate(object listener)
        {
            Instance.RemoveHandler(Instance._fixedUpdateLock,
                Instance._lateUpdateHandlers, Instance._lateUpdateHandlersLateRemove,
                listener);
        }

        /// <summary>
        /// LateUpdate中每累计1秒调用一次，注意：如果出现卡顿超过1秒，则卡顿恢复后只会调用一次
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="handler"></param>
        public static void RegisterSecondUpdate(object listener, UpdateHandler handler)
        {
            Instance.AddHandler(Instance._lateUpdateLock,
                Instance._secondUpdateHandlers, Instance._secondUpdateHandlersLateAdd,
                listener, handler);
        }

        public static void UnregisterSecondUpdate(object listener)
        {
            Instance.RemoveHandler(Instance._lateUpdateLock,
                Instance._secondUpdateHandlers, Instance._secondUpdateHandlersLateRemove,
                listener);
        }

        #endregion

        public delegate void UpdateHandler(float dt);

        #region Fields

        private Dictionary<object, UpdateHandler> _frameUpdateHandlers;
        private Dictionary<object, UpdateHandler> _logicUpdateHandlers;
        private Dictionary<object, UpdateHandler> _lateUpdateHandlers;
        private Dictionary<object, UpdateHandler> _secondUpdateHandlers;

        private Dictionary<object, UpdateHandler> _frameUpdateHandlersLateAdd;
        private List<object> _frameUpdateHandlersLateRemove;
        private Dictionary<object, UpdateHandler> _logicUpdateHandlersLateAdd;
        private List<object> _logicUpdateHandlersLateRemove;
        private Dictionary<object, UpdateHandler> _lateUpdateHandlersLateAdd;
        private List<object> _lateUpdateHandlersLateRemove;
        private Dictionary<object, UpdateHandler> _secondUpdateHandlersLateAdd;
        private List<object> _secondUpdateHandlersLateRemove;

        private float _secondTimer;

        #endregion

        private void InitUpdator()
        {
            _frameUpdateHandlers = new Dictionary<object, UpdateHandler>();
            _logicUpdateHandlers = new Dictionary<object, UpdateHandler>();
            _lateUpdateHandlers = new Dictionary<object, UpdateHandler>();
            _secondUpdateHandlers = new Dictionary<object, UpdateHandler>();

            _frameUpdateHandlersLateAdd = new Dictionary<object, UpdateHandler>();
            _logicUpdateHandlersLateAdd = new Dictionary<object, UpdateHandler>();
            _lateUpdateHandlersLateAdd = new Dictionary<object, UpdateHandler>();
            _secondUpdateHandlersLateAdd = new Dictionary<object, UpdateHandler>();
            _frameUpdateHandlersLateRemove = new List<object>();
            _logicUpdateHandlersLateRemove = new List<object>();
            _lateUpdateHandlersLateRemove = new List<object>();
            _secondUpdateHandlersLateRemove = new List<object>();

            _secondTimer = 0;
        }

        private string GetUpdateTypeNameWithContainer(Dictionary<object, UpdateHandler> container)
        {
            if (container == _frameUpdateHandlers)
            {
                return "FrameUpdate";
            }

            if (container == _frameUpdateHandlersLateAdd)
            {
                return "FrameUpdate_LateAdd";
            }

            if (container == _logicUpdateHandlers)
            {
                return "LogicUpdate";
            }

            if (container == _logicUpdateHandlersLateAdd)
            {
                return "LogicUpdate_LateAdd";
            }

            if (container == _lateUpdateHandlers)
            {
                return "LateUpdate";
            }

            if (container == _lateUpdateHandlersLateAdd)
            {
                return "LateUpdate_LateAdd";
            }

            if (container == _secondUpdateHandlers)
            {
                return "SecondUpdate";
            }

            if (container == _secondUpdateHandlersLateAdd)
            {
                return "SecondUpdate_LateAdd";
            }

            return "Unknown";
        }

        private bool ValidateUpdateLock(bool isLocked, Dictionary<object, UpdateHandler> container, Dictionary<object, UpdateHandler> lateAdd,
            object listener, UpdateHandler handler)
        {
            if (isLocked)
            {
                if (lateAdd.ContainsKey(listener))
                {
                    throw new DuplicateListenerException(GetUpdateTypeNameWithContainer(lateAdd));
                }

                lateAdd.Add(listener, handler);
            }

            return !isLocked;
        }

        private bool ValidateUpdateLock(bool isLocked, List<object> lateRemove, object listener)
        {
            if (isLocked && !lateRemove.Contains(listener))
            {
                lateRemove.Add(listener);
            }

            return !isLocked;
        }

        private void AddHandler(
            bool isLocked,
            Dictionary<object, UpdateHandler> container,
            Dictionary<object, UpdateHandler> lateAdd,
            object listener, UpdateHandler handler)
        {
            if (listener == null || handler == null)
            {
                throw new ArgumentNullException();
            }

            if (container.ContainsKey(listener))
            {
                throw new DuplicateListenerException(GetUpdateTypeNameWithContainer(container));
            }

            if (isLocked)
            {
                if (lateAdd.ContainsKey(listener))
                {
                    throw new DuplicateListenerException(GetUpdateTypeNameWithContainer(lateAdd));
                }

                lateAdd.Add(listener, handler);
            }
            else
            {
                container.Add(listener, handler);
            }
        }

        private void RemoveHandler(
            bool isLocked,
            Dictionary<object, UpdateHandler> container,
            List<object> lateRemove,
            object listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException();
            }

            if (isLocked)
            {
                lateRemove.Add(listener);
            }
            else
            {
                container.Remove(listener);
            }
        }

        private void UpdateFrame()
        {
            var dt = Time.deltaTime;
            foreach (var handler in _frameUpdateHandlers)
            {
                try
                {
                    handler.Value?.Invoke(dt);
                }
                catch (Exception exc)
                {
                    Logger.LogException(LogModule.Framework, exc);
                }
            }

            foreach (var item in _frameUpdateHandlersLateAdd)
            {
                AddHandler(false, _frameUpdateHandlers, null, item.Key, item.Value);
            }

            foreach (var item in _frameUpdateHandlersLateRemove)
            {
                RemoveHandler(false, _frameUpdateHandlers, null, item);
            }
        }

        private void UpdateLogic()
        {
            var dt = Time.fixedDeltaTime;
            foreach (var handler in _logicUpdateHandlers)
            {
                try
                {
                    handler.Value?.Invoke(dt);
                }
                catch (Exception exc)
                {
                    Logger.LogException(LogModule.Framework, exc);
                }
            }

            foreach (var item in _logicUpdateHandlersLateAdd)
            {
                AddHandler(false, _logicUpdateHandlers, null, item.Key, item.Value);
            }

            foreach (var item in _logicUpdateHandlersLateRemove)
            {
                RemoveHandler(false, _logicUpdateHandlers, null, item);
            }
        }

        private void UpdateLateFrame()
        {
            var dt = Time.deltaTime;
            foreach (var handler in _lateUpdateHandlers)
            {
                try
                {
                    handler.Value?.Invoke(dt);
                }
                catch (Exception exc)
                {
                    Logger.LogException(LogModule.Framework, exc);
                }
            }

            foreach (var item in _lateUpdateHandlersLateAdd)
            {
                AddHandler(false, _lateUpdateHandlers, null, item.Key, item.Value);
            }

            foreach (var item in _lateUpdateHandlersLateRemove)
            {
                RemoveHandler(false, _lateUpdateHandlers, null, item);
            }

            _secondTimer += dt;
            if (_secondTimer > 1)
            {
                var delta = Mathf.Floor(_secondTimer);
                foreach (var handler in _secondUpdateHandlers)
                {
                    try
                    {
                        handler.Value?.Invoke(delta);
                    }
                    catch (Exception exc)
                    {
                        Logger.LogException(LogModule.Framework, exc);
                    }
                }

                foreach (var item in _secondUpdateHandlersLateAdd)
                {
                    AddHandler(false, _secondUpdateHandlers, null, item.Key, item.Value);
                }

                foreach (var item in _secondUpdateHandlersLateRemove)
                {
                    RemoveHandler(false, _secondUpdateHandlers, null, item);
                }

                _secondTimer %= 1;
            }
        }
    }
}
