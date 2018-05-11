/**
 * == Instech ==
 * Assembly: Gameplay
 * FileName: GameStart.cs
 * Created on 2018/05/11 by inspoy
 * All rights reserved.
 */

using UnityEngine;

namespace Game
{
    /// <summary>
    /// 游戏入口
    /// </summary>
    public class GameStart : MonoBehaviour
    {
        private void Awake()
        {
            GameMain.CreateSingleton();
        }
    }
}
