/**
 * == Instech ==
 * Assembly: Gameplay
 * FileName: TestPresenter.cs
 * Created on 2018/05/04 by inspoy
 * All rights reserved.
 */

using System;
using Instech.Framework;

namespace Instech.Game
{
    public class TestPresenter : IBasePresenter
    {
        private TestView _view;

        /// <inheritdoc />
        public void InitWithView(BaseView view)
        {
            _view = view as TestView;
            if (_view == null)
            {
                throw new Exception("Init Ui View Failed: " + view);
            }

            _view.AddEventListener(_view.BtnGo, EventEnum.UiPointerClick, OnGoClicked);

            // Called when view is initialized
        }

        /// <inheritdoc />
        public void OnViewShow()
        {
            // Called when view will be shown
        }

        /// <inheritdoc />
        public void OnViewHide()
        {
            // Called when view will be hidden
        }

        /// <inheritdoc />
        public void OnViewRemoved()
        {
            // Called when view will be destroyed
        }

        /// <summary>
        /// 获取对应的View
        /// </summary>
        /// <returns></returns>
        public TestView GetView()
        {
            return _view;
        }

        private void OnGoClicked(Event e)
        {
            Logger.LogInfo(null, "btnGo clicked");
            _view.LblInfo.text = $"CurTime: {DateTime.Now.ToLongTimeString()}";
        }
    }
}
