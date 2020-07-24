/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Common
 * FileName: GameState.cs
 * Created on 2018/05/11 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using Instech.Framework.Core;
using Instech.Framework.Logging;

namespace Instech.Framework.Common
{
    /// <summary>
    /// 所有游戏状态需要实现此接口
    /// </summary>
    public interface IGameState
    {
        void OnStateEnter(string lastState);
        void OnStateLeave(string nextState);
        void UpdateFrame(float dt);
        void UpdateLogic(float dt);
    }

    public class GameStateMachine : Singleton<GameStateMachine>
    {
        private readonly Dictionary<Type, IGameState> _registeredGameStates = new Dictionary<Type, IGameState>();
        private IGameState _curState;
        private string _curStateName;

        /// <summary>
        /// 获取当前状态
        /// </summary>
        /// <returns></returns>
        public IGameState GetCurrentState()
        {
            return _curState;
        }

        /// <summary>
        /// 更新帧
        /// </summary>
        /// <param name="dt"></param>
        public void UpdateFrame(float dt)
        {
            _curState?.UpdateFrame(dt);
        }

        /// <summary>
        /// 更新逻辑
        /// </summary>
        /// <param name="dt"></param>
        public void UpdateLogic(float dt)
        {
            _curState?.UpdateLogic(dt);
        }

        /// <summary>
        /// 注册状态，应当在游戏初始化时调用
        /// </summary>
        /// <param name="state"></param>
        /// <exception cref="ArgumentNullException">参数无效</exception>
        /// <exception cref="ArgumentException">重复注册</exception>
        public void RegisterGameState(IGameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state), "状态为空");
            }
            if (_registeredGameStates.ContainsKey(state.GetType()))
            {
                throw new ArgumentException("重复的状态", nameof(state));
            }
            _registeredGameStates.Add(state.GetType(), state);
            Logger.LogInfo(LogModule.GameFlow, "状态已注册:" + state.GetType().FullName);
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateType"></param>
        public void ChangeState(Type stateType)
        {
            if (stateType == null)
            {
                throw new ArgumentNullException(nameof(stateType), "状态类型无效");
            }

            if (!_registeredGameStates.TryGetValue(stateType, out var newState))
            {
                throw new ArgumentException("状态未注册", nameof(stateType));
            }
            Logger.LogInfo(LogModule.GameFlow, $"准备切换状态[{_curStateName}]=>[{stateType}]");
            _curState?.OnStateLeave(stateType.FullName);
            newState.OnStateEnter(_curStateName);
            _curState = newState;
            _curStateName = stateType.FullName;
            Logger.LogInfo(LogModule.GameFlow, "状态切换完成");
        }

        protected override void Init()
        {
            _curState = null;
            _curStateName = string.Empty;
        }

        protected override void Deinit()
        {
            _curState?.OnStateLeave(string.Empty);
            _curState = null;
            _curStateName = string.Empty;
        }
    }
}
