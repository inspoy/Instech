/**
 * == Instech ==
 * Assembly: Gameplay
 * FileName: TestState.cs
 * Created on 2018/05/11 by inspoy
 * All rights reserved.
 */

using Instech.Framework;

namespace Game
{
    /// <summary>
    /// 测试状态，功能演示用
    /// </summary>
    public class TestState : IGameState
    {
        public void UpdateFrame(float dt)
        {
        }

        public void UpdateLogic(float dt)
        {
        }

        public void OnStateEnter(string lastState)
        {
            Logger.LogInfo(null, "进入状态TestState");
            UiManager.Instance.AddView<TestView>();
        }

        public void OnStateLeave(string nextState)
        {
            Logger.LogInfo(null, "离开状态TestState");
        }
    }
}
