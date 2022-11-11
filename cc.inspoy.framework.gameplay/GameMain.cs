// == Inspoy Technology ==
// Assembly: Instech.Framework.Gameplay
// FileName: GameMain.cs
// Created on 2021/07/12 by inspoy
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Instech.Framework.AssetHelper;
using Instech.Framework.Common;
using Instech.Framework.Core;
using Instech.Framework.Data;
using Instech.Framework.Logging;
using Instech.Framework.Ui;
using Instech.Framework.Utils;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.Gameplay
{
    public class GameMainConfig
    {
        /// <summary>
        /// 需要创建的所有Canvas列表
        /// </summary>
        public IEnumerable<string> CanvasList;

        /// <summary>
        /// 如果需要从配置里加载，也可以通过这个接口来实现延时获取，调用时会保证ConfigManager已创建
        /// </summary>
        public ICanvasListProvider CanvasListProvider;

        /// <summary>
        /// 是否在启动时打印环境信息的日志
        /// </summary>
        public bool PrintLaunchLog;

        /// <summary>
        /// 数据表加密Key
        /// </summary>
        public byte[] ConfigDbKey;

        /// <summary>
        /// 本地存档加密Key
        /// </summary>
        public byte[] LocalStorageKey;

        /// <summary>
        /// 初始化游戏逻辑，框架初始化完成后执行
        /// </summary>
        public Action InitGameLogicHandler;

        /// <summary>
        /// 游戏逻辑的清理和收尾工作，退出游戏时销毁框架前执行
        /// </summary>
        public Action QuitGameHandler;
#if UNITY_EDITOR
        /// <summary>
        /// Excel配置表的路径
        /// </summary>
        public string ExcelFolder;

        /// <summary>
        /// 在编辑器环境中使用AssetBundle
        /// </summary>
        public bool UseBundleInEditor;

        /// <summary>
        /// 在编辑器环境中使用二进制配置档
        /// </summary>
        public bool ForceUseBinary;
#endif
    }

    public interface IGameMain
    {
        GameMainConfig Setup();
    }

    public interface ICanvasListProvider
    {
        IEnumerable<string> GetCanvasList();
    }

    internal class GameMain : MonoSingleton<GameMain>
    {
        private GameMainConfig _gameConfig;
        private Assembly _gameAssembly;

        protected override void Init()
        {
            var sw = Stopwatch.StartNew();
            Type bootstrapType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IGameMain).IsAssignableFrom(type) && type != typeof(IGameMain))
                    {
                        _gameAssembly = assembly;
                        bootstrapType = type;
                        break;
                    }
                }

                if (bootstrapType != null) break;
            }

            if (bootstrapType == null)
            {
                Logger.LogError(LogModule.GameFlow, "You must add a class which implements IGameMain");
                return;
            }

            var bootstrap = Activator.CreateInstance(bootstrapType) as IGameMain;
            _gameConfig = bootstrap?.Setup();
            if (_gameConfig == null)
            {
                Logger.LogError(LogModule.GameFlow, "Bad config from IGameMain");
                return;
            }

            var t1 = sw.ElapsedMilliseconds;
            sw.Restart();
#if UNITY_EDITOR
            // 一些静态设置
            ConfigManager.ExcelDocumentPath = _gameConfig.ExcelFolder;
            AssetManager.UseBundleInEditor = _gameConfig.UseBundleInEditor;
            ConfigManager.ForceUseBinary = _gameConfig.ForceUseBinary;
#endif

            // Framework
            ObjectPoolManager.CreateSingleton();
            LogToFile.CreateSingleton();
            if (_gameConfig.PrintLaunchLog)
            {
                PrintLaunchLog(); // Log功能依赖上面两个类
            }

            FastYield.CreateSingleton();
            GameStateMachine.CreateSingleton();
            ConfigManager.CreateSingleton();
            RegisterAllConfig();
            ConfigManager.Instance.FinishInit();
            LocalizationManager.CreateSingleton();
            AssetManager.CreateSingleton();
            UiManager.CreateSingleton();
            RegisterAllCanvases();
            Scheduler.CreateSingleton();
            LS.Init(_gameConfig.LocalStorageKey);
            var t2 = sw.ElapsedMilliseconds;
            sw.Restart();

            // Game
            _gameConfig.InitGameLogicHandler?.Invoke();
            var t3 = sw.ElapsedMilliseconds;
            Logger.LogInfo(LogModule.GameFlow,
                $"GameMain.Init finished, fetching cost {t1}ms, framework init cost {t2}ms, gameplay init cost {t3}ms");
        }

        private void PrintLaunchLog()
        {
            Logger.LogInfo(LogModule.GameFlow, $"Game Launched, current platform: {Application.platform}");
            Logger.LogInfo(LogModule.GameFlow,
                "DataPaths:\n" +
                $"Data:{Application.dataPath}\n" +
                $"Streaming:{Application.streamingAssetsPath}\n" +
                $"Local:{PathHelper.LocalCacheFolder}\n" +
                $"Temp:{Application.temporaryCachePath}\n" +
                $"SaveData:{PathHelper.SaveDataPath}");
            Logger.LogInfo(LogModule.GameFlow, "HardwareInfo:");
            Logger.LogInfo(LogModule.GameFlow, JsonUtility.ToJson(DeviceUtils.GetHardwareInfo()));
            Logger.LogInfo(LogModule.GameFlow, "SoftwareInfo:");
            Logger.LogInfo(LogModule.GameFlow, JsonUtility.ToJson(DeviceUtils.GetSoftwareInfo()));
            Logger.LogInfo(LogModule.GameFlow, $"SessionID: {DeviceUtils.GetSessionId()}");
        }

        private void RegisterAllConfig()
        {
            ConfigManager.Instance.LoadDataSource(_gameConfig.ConfigDbKey);
            var allTypes = _gameAssembly.GetTypes();
            var regMethod = typeof(ConfigManager).GetMethod("RegisterConfigType");
            if (regMethod == null)
            {
                return;
            }

            foreach (var item in allTypes)
            {
                var attr = item.GetCustomAttribute<GameConfigAttribute>();
                if (attr == null)
                {
                    continue;
                }

                try
                {
                    regMethod.MakeGenericMethod(item).Invoke(ConfigManager.Instance, new object[] { attr.TableName });
                }
                catch (Exception e)
                {
                    Logger.LogError(LogModule.Data, "配置项注册失败: " + item.Name);
                    Logger.LogException(LogModule.Data, e);
                }
            }
        }

        private void RegisterAllCanvases()
        {
            var canvasList = new List<string>();
            if (_gameConfig.CanvasList != null)
            {
                canvasList.AddRange(_gameConfig.CanvasList);
            }

            if (_gameConfig.CanvasListProvider != null)
            {
                canvasList.AddRange(_gameConfig.CanvasListProvider.GetCanvasList());
            }

            foreach (var item in canvasList)
            {
                UiManager.Instance.AddCanvas(item);
            }

            UiManager.Instance.AddCanvas("Normal"); // Normal Canvas必须存在
        }

        private void Update()
        {
            GameStateMachine.Instance.UpdateFrame(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            GameStateMachine.Instance.UpdateLogic(Time.fixedDeltaTime);
        }

        private void OnApplicationQuit()
        {
            _gameConfig.QuitGameHandler?.Invoke();
            GameStateMachine.DestroySingleton();
            LocalStorage.DestroySingleton();
            Scheduler.DestroySingleton();
            UiManager.DestroySingleton();
            AssetManager.DestroySingleton();
            LocalizationManager.DestroySingleton();
            ConfigManager.DestroySingleton();
            FastYield.DestroySingleton();
            LogToFile.DestroySingleton();
            ObjectPoolManager.DestroySingleton();
        }
    }
}
