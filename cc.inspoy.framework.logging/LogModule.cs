/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Logging
 * FileName: LogModule.cs
 * Created on 2018/07/02 by inspoy
 * All rights reserved.
 */

namespace Instech.Framework.Logging
{
    /// <summary>
    /// 通用的日志模块类型
    /// </summary>
    public static class LogModule
    {
        /// <summary>
        /// 默认未设置模块，仅用于临时调试，不应长期存在
        /// </summary>
        public const string Default = "Default";

        /// <summary>
        /// 非Instech.Framework.Logger发出的日志，一般是引擎或插件给出的日志信息，不在业务逻辑中使用
        /// </summary>
        internal const string Unhandled = "Unhandled";

        /// <summary>
        /// 编辑器杂项功能
        /// </summary>
        public const string Editor = "Editor";

        /// <summary>
        /// 构建相关
        /// </summary>
        public const string Build = "Build";

        /// <summary>
        /// 杂项框架相关
        /// </summary>
        public const string Framework = "Framework";

        /// <summary>
        /// UI相关（包含框架和业务）
        /// </summary>
        public const string Ui = "UI";

        /// <summary>
        /// 数据处理相关（如配置档）
        /// </summary>
        public const string Data = "Data";

        /// <summary>
        /// 游戏流程相关（如加载，状态切换等）
        /// </summary>
        public const string GameFlow = "GameFlow";

        /// <summary>
        /// ECS系统，非业务逻辑
        /// </summary>
        public const string Ecs = "ECS";

        /// <summary>
        /// 资源加载相关
        /// </summary>
        public const string Resource = "Resource";

        /// <summary>
        /// 渲染相关
        /// </summary>
        public const string Render = "Render";

        /// <summary>
        /// 本地化相关
        /// </summary>
        public const string Localization = "L10N";

        /// <summary>
        /// 物理相关
        /// </summary>
        public const string Physics = "Physics";
    }
}
