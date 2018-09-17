/**
 * == Instech ==
 * Assembly: Gameplay
 * FileName: GameCommonConfig.cs
 * Created on 2018/09/17 by inspoy
 * All rights reserved.
 */

using Instech.Framework;

namespace Game.Config
{
    public class GameCommonConfig : BaseConfig
    {
        /// <summary>
        /// 描述-程序不使用
        /// </summary>
        public string Desc = "";

        /// <summary>
        /// 整数值
        /// </summary>
        public int IntVal;

        /// <summary>
        /// 字符串值
        /// </summary>
        public string StrVal = "";

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

            CustomProcess(data);
        }

        #region ===== 自定义的代码
        /// <summary>
        /// 字符串值分割出的整数数组
        /// </summary>
        public int[] IntVals;

        protected override void CustomProcess(IConfigData data)
        {
            base.CustomProcess(data);
            IntVals = Utility.SplitToInt(StrVal);
        }

        public static int GetInt(int id)
        {
            return ConfigManager.Instance.GetSingle<GameCommonConfig>(id).IntVal;
        }

        public static string GetString(int id)
        {
            return ConfigManager.Instance.GetSingle<GameCommonConfig>(id).StrVal;
        }
        #endregion ===== 自定义的代码
    }
}
