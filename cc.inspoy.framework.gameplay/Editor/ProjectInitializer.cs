// == Inspoy Technology ==
// Assembly: Instech.Framework.Gameplay.Editor
// FileName: ProjectInitializer.cs
// Created on 2021/07/12 by inspoy
// All rights reserved.

using System.IO;
using System.Linq;
using Instech.Framework.Gameplay;
using Instech.Framework.Logging;
using Instech.Framework.Utils.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Common.Editor
{
    public static class ProjectInitializer
    {
        [MenuItem("Instech/Common/初始化新的工程", false, 1201)]
        public static void InitForEmptyProject()
        {
            var sceneBootPath = Application.dataPath + "/Scenes/SceneBoot.unity";
            if (File.Exists(sceneBootPath))
            {
                Logger.LogWarning(LogModule.Editor, "应该已经初始化过了");
                return;
            }

            // Define Symbols
            var defines = DefineSymbolManager.GetAllDefines().ToList();
            var customDefines = new[]
            {
                "INSTECH_LOGGER_ENABLE_NORMAL",
                "INSTECH_LOGGER_ENABLE_ERROR",
                "INSTECH_LOGGER_ENABLE_EXCEPTION"
            };
            foreach (var symbol in customDefines)
            {
                if (!defines.Contains(symbol))
                {
                    DefineSymbolManager.AddDefine(symbol);
                }
            }

            // Folders
            var folders = new[]
            {
                "/Scripts/UI",
                "/Scripts/Config",
                "/Scripts/ConfigExtension",
                "/Artwork/Prefabs/UI",
                "/ArtBlob",
                "/StaticResource"
            };
            foreach (var path in folders)
            {
                Directory.CreateDirectory(Application.dataPath + path);
            }

            // Boot Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            var bootGo = new GameObject("Bootstrap");
            bootGo.AddComponent<GameStart>();
            EditorSceneManager.SaveScene(scene, sceneBootPath);

            // Game.dll
            var asmdefPath = Application.dataPath + "/Scripts/Game.asmdef";
            File.WriteAllText(asmdefPath, "{\n\"name\": \"Game\"\n}");

            AssetDatabase.Refresh();
        }
    }
}
