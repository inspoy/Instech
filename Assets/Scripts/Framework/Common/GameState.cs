/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: GameState.cs
 * Created on 2018/05/11 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;

namespace Instech.Framework
{
    /// <summary>
    /// 所有游戏状态需要实现此接口
    /// </summary>
    public interface IGameState
    {
        void UpdateFrame(float dt);
        void UpdateLogic(float dt);
        void OnStateEnter(string lastState);
        void OnStateLeave(string nextState);
    }

    public class GameStateMachine : Singleton<GameStateMachine>
    {
        private readonly Dictionary<string, IGameState> _registeredGameStates = new Dictionary<string, IGameState>();
        private IGameState _curState;
        private string _curStateName;

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
        /// <param name="stateName"></param>
        /// <param name="state"></param>
        /// <exception cref="ArgumentNullException">参数无效</exception>
        /// <exception cref="ArgumentException">重复注册</exception>
        public void RegisterGameState(string stateName, IGameState state)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                throw new ArgumentNullException(nameof(stateName), "名称无效");
            }
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state), "状态为空");
            }
            if (_registeredGameStates.ContainsKey(stateName))
            {
                throw new ArgumentException("重复的状态", nameof(stateName));
            }
            _registeredGameStates.Add(stateName, state);
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateName"></param>
        public void ChangeState(string stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                throw new ArgumentNullException(nameof(stateName), "名称无效");
            }
            IGameState newState;
            if (!_registeredGameStates.TryGetValue(stateName, out newState))
            {
                throw new ArgumentException("状态未注册", nameof(stateName));
            }
            _curState?.OnStateLeave(stateName);
            newState.OnStateEnter(_curStateName);
            _curState = newState;
        }

        protected override void Init()
        {
            _curState = null;
            _curStateName = string.Empty;
        }

        protected override void Uninit()
        {
            _curState?.OnStateLeave(string.Empty);
            _curState = null;
            _curStateName = string.Empty;
        }
    }
}
