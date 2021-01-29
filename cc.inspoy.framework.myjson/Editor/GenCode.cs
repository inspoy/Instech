// == Inspoy Technology ==
// Assembly: Instech.Framework.MyJson.Editor
// FileName: GenUtf8JsonCode.cs
// Created on 2019/03/13 by inspoy
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Instech.Framework.Common.Editor;
using Instech.Framework.Logging;
using UnityEngine;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.Framework.MyJson.Editor
{
    public static class GenUtf8JsonCode
    {
        public const string GeneratorPath = ".generator/Utf8Json.UniversalCodeGenerator/Utf8Json.UniversalCodeGenerator.exe";

        public static void GenCode()
        {
            var packagesRoot = ProjectSettings.GetPackageFullPath("cc.inspoy.framework.myjson");
            var exePath = Path.Combine(packagesRoot, GeneratorPath);
            var inputPath = new Dictionary<Type, string>();
            CollectTypes(inputPath);
            Logger.LogInfo(LogModule.Data, $"要为{inputPath.Count}个文件生成代码");
            var targetFolder = Application.dataPath + "/Scripts/GenCode/";
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            try
            {
                var initCode = new StringBuilder("MyJson.InitJsonResolvers(\n");
                foreach (var item in inputPath)
                {
                    initCode.Append($"    Generated{item.Key.Name}Resolver.Instance,\n");
                    var targetPath = targetFolder + item.Key.FullName + "Resolver.cs";
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }

                    var arg = $"-i {item.Value} -o {targetPath} -r Generated{item.Key.Name}Resolver -n Game.JsonResolver";
                    Logger.LogInfo(LogModule.Data, $"生成{item.Key.FullName}Resolver.cs, 命令行\n{exePath} {arg}");
                    Process.Start(exePath, arg)?.WaitForExit();

                    if (!File.Exists(targetPath))
                    {
                        Logger.LogError(LogModule.Data, "生成失败");
                    }
                }

                if (inputPath.Count > 0)
                {
                    initCode.Remove(initCode.Length - 2, 2);
                }

                initCode.Append("\n);");
                Logger.LogInfo(LogModule.Data, $"生成成功，请复制以下代码到游戏初始化过程：\n{initCode}");
            }
            catch (Exception e)
            {
                Logger.LogError(LogModule.Data, "执行生成工具发生异常");
                Logger.LogException(LogModule.Data, e);
            }
        }

        private static void CollectTypes(Dictionary<Type, string> inputPath)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var allTypes = assembly.GetTypes();
                foreach (var type in allTypes)
                {
                    var attr = type.GetCustomAttribute<GenerateJsonResolverAttribute>(false);
                    if (attr == null)
                    {
                        continue;
                    }

                    if (inputPath.ContainsValue(attr.FilePath))
                    {
                        Logger.LogWarning(LogModule.Editor, $"文件 {attr.FilePath} 存在多个包含GenResolver属性的类");
                        continue;
                    }

                    if (!File.Exists(attr.FilePath))
                    {
                        Logger.LogWarning(LogModule.Editor, $"<{type.FullName}>配置了错误的文件路径: {attr.FilePath}");
                        continue;
                    }

                    inputPath.Add(type, attr.FilePath);
                }
            }
        }
    }
}