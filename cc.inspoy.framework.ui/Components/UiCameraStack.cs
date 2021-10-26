// == Inspoy Technology ==
// Assembly: Instech.Framework.Ui
// FileName: UiCameraStack.cs
// Created on 2021/10/25 by inspoy
// All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Instech.Framework.Ui
{
    [RequireComponent(typeof(Camera))]
    public class UiCameraStack : MonoBehaviour
    {
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            if (UiManager.HasSingleton())
            {
                var data = _camera.GetUniversalAdditionalCameraData();
                data.cameraStack.Add(UiManager.Instance.UiCamera);
            }
        }
    }
}
