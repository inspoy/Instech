/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: BasePresenter.cs
 * Created on 2018/05/01 by inspoy
 * All rights reserved.
 */

namespace Instech.Framework
{
    /// <summary>
    /// 所有UI Presenter需要实现的接口
    /// </summary>
    public interface IBasePresenter
    {
        /// <summary>
        /// 初始化UI
        /// 一般在这里创建和初始化所需对象
        /// </summary>
        /// <param name="view"></param>
        void InitWithView(BaseView view);

        /// <summary>
        /// 当UI显示时（第一次也会调用）
        /// 一般在这里处理数据相关的初始化
        /// 包括事件监听
        /// </summary>
        void OnViewShow();

        /// <summary>
        /// 当UI隐藏时（暂不关闭）
        /// 一般在这里清理数据
        /// </summary>
        void OnViewHide();

        /// <summary>
        /// 当UI移除
        /// 一般在这里清理对象
        /// </summary>
        void OnViewRemoved();
    }
}
