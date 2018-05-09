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
    public class GameMain : MonoBehaviour
    {
        private void Awake()
        {
            FastYield.CreateSingleton();
            LogToFile.CreateSingleton();

            // AssetBundleManager.CreateSingleton();
            UiManager.CreateSingleton();
        }

        private void Update()
        {
            // TODO: currentState.Update(dt);

            // Test
            if (DoTest)
            {
                DoTest = false;
                TestFunc();
            }
        }

        private void OnApplicationQuit()
        {
            FastYield.DestroySingleton();
            LogToFile.DestroySingleton();
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
