// == Inspoy Technology ==
// Assembly: Instech.Framework.Gameplay
// FileName: GameStart.cs
// Created on 2021/07/12 by inspoy
// All rights reserved.

using System;
using UnityEngine;

namespace Instech.Framework.Gameplay
{
    public class GameStart : MonoBehaviour
    {
        private void Start()
        {
            GameMain.CreateSingleton();
            Destroy(gameObject);
        }
    }
}
