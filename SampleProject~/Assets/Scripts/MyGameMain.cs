// == SampleProject~ ==
// Assembly: Game
// FileName: MyGameMain.cs
// Created on 2021/07/12 by inspoy
// All rights reserved.

using Game.GameState;
using Instech.Framework.Common;
using Instech.Framework.Gameplay;
using JetBrains.Annotations;
using UnityEngine;

namespace Game
{
    [UsedImplicitly]
    public class MyGameMain : IGameMain
    {
        public GameMainConfig Setup()
        {
            return new GameMainConfig
            {
                CanvasList = null,
                PrintLaunchLog = true,
                ConfigDbKey = null,
                InitGameLogicHandler = InitGameLogic,
                QuitGameHandler = DeinitGameLogic,
#if UNITY_EDITOR
                ExcelFolder = GetExcelFolder(),
                UseBundleInEditor = false,
                ForceUseBinary = false,
#endif
                
            };
        }

#if UNITY_EDITOR
        public string GetExcelFolder()
        {
            return $"{Application.dataPath}/../GameConfig/";
        }
#endif

        private void InitGameLogic()
        {
            GameStateMachine.Instance.RegisterGameState(new TitleState());
            GameStateMachine.Instance.ChangeState(typeof(TitleState));
        }

        private void DeinitGameLogic()
        {
        }
    }
}
