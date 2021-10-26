using Instech.Framework.Common;
using Instech.Framework.Logging;
using UnityEngine.SceneManagement;

namespace Game.GameState
{
    public class TitleState : IGameState
    {
        public void OnStateEnter(string lastState)
        {
            Logger.LogInfo(LogModule.GameFlow, "已顺利进入TitleState！");
            SceneManager.LoadScene("SampleScene");
        }

        public void OnStateLeave(string nextState)
        {
        }

        public void UpdateFrame(float dt)
        {
        }

        public void UpdateLogic(float dt)
        {
        }
    }
}
