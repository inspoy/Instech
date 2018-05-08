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
            // AssetBundleManager.CreateSingleton();
            UiManager.CreateSingleton();

            UiManager.Instance.AddView<TestView>();
        }

        #region Test
        public bool DoTest;
        private void Update()
        {
            if (DoTest)
            {
                DoTest = false;
                TestFunc();
            }
        }

        private static void TestFunc()
        {
            UiManager.Instance.AddView<TestView>();
        }
        #endregion
    }
}
