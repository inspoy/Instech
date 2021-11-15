using System;
using System.IO;
using JetBrains.Annotations;

namespace Instech.Framework.Gameplay
{
    /// <summary>
    /// 实现模块数据的接口
    /// </summary>
    /// <remarks>
    /// 只有需要序列化（保存）的数据需要写在这里<br/>
    /// 其他的模块自用运行时变量就不用放到这里了<br/>
    /// 其他模块不能直接访问到这个类，如果需要给其他模块访问，需要在本模块中添加接口
    /// </remarks>
    public interface IModuleData
    {
        /// <summary>
        /// 保存数据，将数据序列化到<see cref="bw"/>中
        /// </summary>
        void Save([NotNull] BinaryWriter bw);

        /// <summary>
        /// 加载数据，从<see cref="br"/>中读取
        /// </summary>
        /// <param name="version">存档版本</param>
        /// <param name="br">存档数据源</param>
        void Load(int version, [NotNull] BinaryReader br);
    }
}
