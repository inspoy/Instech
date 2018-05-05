/**
 * == Instech ==
 * Assembly: Gameplay
 * FileName: TestView.cs
 * Created on 2018/05/04 by inspoy
 * All rights reserved.
 */

using System;
using Instech.Framework;
using UnityEngine;
using UnityEngine.UI;
using Logger = Instech.Framework.Logger;

namespace Game
{
    public class TestView : BaseView
    {
        /// <summary>
        /// [Text] Info
        /// </summary>
        public Text TxtInfo;

        /// <summary>
        /// [Button] Go
        /// </summary>
        public Button BtnGo;

        /// <summary>
        /// [GameObject] Over
        /// </summary>
        public GameObject LayOver;

        /// <summary>
        /// 获取对应的Presenter
        /// </summary>
        /// <returns></returns>
        public TestPresenter GetPresenter()
        {
            return Presenter as TestPresenter;
        }

        protected override void Awake()
        {
#if UNITY_EDITOR
            var time1 = DateTime.Now;
#endif

            if (TxtInfo == null)
            {
                Logger.LogError("UI", "找不到控件txtInfo");
            }

            if (BtnGo == null)
            {
                Logger.LogError("UI", "找不到控件btnGo");
            }

            if (LayOver == null)
            {
                Logger.LogError("UI", "找不到控件layOver");
            }

            Presenter = new TestPresenter();
            OnAwakeFinish();

#if UNITY_EDITOR
            var time2 = DateTime.Now;
            var diff = time2 - time1;
            Logger.LogInfo("UI", $"View Created: vwTest, cost {diff.TotalMilliseconds}ms");
#else
            Logger.LogInfo("UI", "View Created: vwTest");
#endif
        }
    }
}
