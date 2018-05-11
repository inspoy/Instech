/**
 * == Instech ==
 * Assembly: Gameplay
 * FileName: GameMain.cs
 * Created on 2018/05/01 by inspoy
 * All rights reserved.
 */

using Instech.Framework;
using UnityEngine;

namespace Game
{
    public class GameMain : MonoSingleton<GameMain>
    {
        protected override void Init()
        {
            FastYield.CreateSingleton();
            LogToFile.CreateSingleton();
            GameStateMachine.CreateSingleton();

            // AssetBundleManager.CreateSingleton();
            UiManager.CreateSingleton();

            GameStateMachine.Instance.RegisterGameState("TestState", new TestState());
            GameStateMachine.Instance.ChangeState("TestState");
        }

        private void Update()
        {
            GameStateMachine.Instance.UpdateFrame(Time.deltaTime);

            // Test
            if (DoTest)
            {
                DoTest = false;
                TestFunc();
            }
        }

        private void FixedUpdate()
        {
            GameStateMachine.Instance.UpdateLogic(Time.fixedDeltaTime);
        }

        private void OnApplicationQuit()
        {
            GameStateMachine.DestroySingleton();
            LogToFile.DestroySingleton();
            FastYield.DestroySingleton();
        }

        #region Test
        public bool DoTest;

        private static void TestFunc()
        {
            UiManager.Instance.AddView<TestView>();
        }
        #endregion
    }
}
