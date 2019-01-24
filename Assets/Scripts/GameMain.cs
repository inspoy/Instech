/**
 * == Instech ==
 * Assembly: Gameplay
 * FileName: GameMain.cs
 * Created on 2018/05/01 by inspoy
 * All rights reserved.
 */

using Instech.Framework;
using UnityEngine;
using Game.Config;

namespace Game
{
    public class GameMain : MonoSingleton<GameMain>
    {
        protected override void Init()
        {

            // Framework
            FastYield.CreateSingleton();
            LogToFile.CreateSingleton();
            GameStateMachine.CreateSingleton();
            ConfigManager.CreateSingleton();
            RegisterAllConfig();
            ConfigManager.Instance.FinishInit();
            AssetBundleManagerInitOption.CreateSingleton();
            // 需要在编辑器使用AssetBundle的把下面改成true
            AssetBundleManagerInitOption.Instance.UseAssetBundle = false;
            AssetBundleManager.CreateSingleton();
            UiManager.CreateSingleton();
            RegisterAllCanvases();
            Scheduler.CreateSingleton();

            GameStateMachine.Instance.RegisterGameState(new TestState());
            GameStateMachine.Instance.ChangeState(typeof(TestState));
        }

        private void RegisterAllConfig()
        {
            ConfigManager.Instance.RegisterConfigType<GameCommonConfig>("GameCommon");
        }

        private void RegisterAllCanvases()
        {
            UiManager.Instance.AddCanvas("Normal");
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
