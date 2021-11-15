using Instech.Framework.Common;
using Instech.Framework.Logging;

namespace Instech.Framework.Gameplay
{
    public interface IGameModule
    {
        string Name { get; }
        bool Enabled { get; set; }
        IModuleData CreateData();
        void OnCreate();
        void OnDestroy();
        void OnUpdateFrame(float dt);
        void OnUpdateLogic(float dt);
    }

    /// <summary>
    /// 游戏系统模块的基类，每个模块需要绑定一个"模块数据"
    /// </summary>
    /// <typeparam name="TDataType">绑定的数据类型</typeparam>
    public abstract class BaseGameModule<TDataType> : IGameModule where TDataType : class, IModuleData, new()
    {
        public bool Enabled { get; set; }

        /// <summary>
        /// 所有GameModule创建完毕，且ModuleData均已就绪
        /// </summary>
        public abstract void OnCreate();

        public abstract void OnDestroy();

        public IModuleData CreateData()
        {
            ModuleData = new TDataType();
            return ModuleData;
        }

        public virtual void OnUpdateFrame(float dt)
        {
        }

        public virtual void OnUpdateLogic(float dt)
        {
        }

        /// <summary>
        /// 共享的事件派发器
        /// </summary>
        protected abstract EventDispatcher Dispatcher { get; }

        protected TDataType ModuleData { get; private set; }

        public string Name => _name ??= GetType().Name;

        private string _name;

        protected void LogVerbose(string content)
        {
            Logger.LogVerbose(Name, content);
        }
    }
}
