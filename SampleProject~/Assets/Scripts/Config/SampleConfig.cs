// == SampleProject~ ==
// Assembly: Game
// FileName: SampleConfig.cs
// Created on 2021/10/25 by inspoy
// All rights reserved.

using Instech.Framework.Data;
using Instech.Framework.Logging;
using Instech.Framework.Utils;
using JetBrains.Annotations;

namespace Game.Config
{
    /// <summary>
    /// 自动生成于:2021-10-25 14:57:09
    /// Hash:2853108716
    /// </summary>
    [GameConfig("Sample")]
    [UsedImplicitly]
    public sealed partial class SampleConfig : BaseConfig
    {
        /// <summary>
        /// 文本字段
        /// </summary>
        public string TextValue { get; private set; } = string.Empty;

        /// <summary>
        /// 整数字段
        /// </summary>
        public int IntValue { get; private set; }

        /// <summary>
        /// 数组字段
        /// </summary>
        public int[] ArrayValue { get; private set; } = EmptyIntArray;

        public static bool Has(int id)
        {
            return ConfigManager.Instance.HasId<SampleConfig>(id);
        }

        public static SampleConfig Get(int id)
        {
            return ConfigManager.Instance.GetSingle<SampleConfig>(id);
        }

        public override void InitWithData(IConfigData data)
        {
            if (Id != 0)
            {
                Logger.LogError(LogModule.Data, "已经初始化过了！");
                return;
            }
            Id = data.GetInt("Id");
            TextValue = data.GetString("TextValue");
            IntValue = data.GetInt("IntValue");
            var rawArrayValue = data.GetString("ArrayValue");
            ArrayValue = Utility.SplitToInt(rawArrayValue);
        }
    }
}
