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

namespace Instech.Game
{
    public class TestView : BaseView
    {
        /// <summary>
        /// [Text] Info
        /// </summary>
        public Text LblInfo { get; private set; }

        /// <summary>
        /// [Button] Go
        /// </summary>
        public Button BtnGo { get; private set; }

        /// <summary>
        /// 获取对应的Presenter
        /// </summary>
        /// <returns></returns>
        public TestPresenter GetPresenter()
        {
            return Presenter as TestPresenter;
            ;
        }

        protected override void Awake()
        {
#if UNITY_EDITOR
            var time1 = DateTime.Now;
#endif

            var lblInfoTrans = transform.Find("lblInfo");
            if (lblInfoTrans != null)
            {
                LblInfo = lblInfoTrans.GetComponent<Text>();
            }
            if (LblInfo == null)
            {
                Logger.LogWarning("UI","找不到控件: lblInfo");
            }

            var btnGoTrans = transform.Find("btnGo");
            if (btnGoTrans != null)
            {
                BtnGo = btnGoTrans.GetComponent<Button>();
            }
            if (BtnGo == null)
            {
                Logger.LogWarning("UI","找不到控件: btnGo");
            }

#if UNITY_EDITOR
            var time2 = DateTime.Now;
            var diff = time2 - time1;
            Logger.LogInfo("UI", $"View Created: vwTest, cost {diff.TotalMilliseconds}ms");
#else
            Logger.LogInfo("UI", "View Created: vwTest");
#endif
            Presenter = new TestPresenter();
            OnAwakeFinish();
        }
    }
}
