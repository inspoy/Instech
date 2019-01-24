/**
 * == Instech ==
 * Assembly: 
 * FileName: GameCommonConfig.cs
 * Created on 2019/01/24 by inspo
 * All rights reserved.
 */

using Instech.Framework;

namespace Game.Config
{
    /// <summary>
    /// 自动生成于:2019-01-24 23:55:20
    /// Hash:2128114878
    /// </summary>
    [GameConfig("GameCommon")]
    public sealed class GameCommonConfig : BaseConfig
    {
        /// <summary>
        /// 描述-程序不使用
        /// </summary>
        public string Desc { get; private set; } = string.Empty;

        /// <summary>
        /// 整数值
        /// </summary>
        public int IntVal { get; private set; }

        /// <summary>
        /// 字符串值
        /// </summary>
        public string StrVal { get; private set; } = string.Empty;

        public static GameCommonConfig Get(int id)
        {
            return ConfigManager.Instance.GetSingle<GameCommonConfig>(id);
        }

        public override void InitWithData(IConfigData data)
        {
            if (Id != 0)
            {
                Logger.LogError(LogModule.Data, "已经初始化过了！");
                return;
            }
            Id = data.GetInt("Id");
            Desc = data.GetString("Desc");
            IntVal = data.GetInt("IntVal");
            StrVal = data.GetString("StrVal");
        }

        #region ===== 自定义的代码

        #endregion ===== 自定义的代码
    }
}
