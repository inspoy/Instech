/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: GenUtf8JsonCode.cs
 * Created on 2019/03/13 by inspoy
 * All rights reserved.
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Instech.Framework.Editor
{
    public static class GenUtf8JsonCode
    {
        public static void GenCode()
        {
            var exePath = Application.dataPath + ProjectSettings.Instance.Utf8JsonGeneratorPath;
            var inputPath = new Dictionary<Type, string>();
            CollectTypes(inputPath);
            Logger.LogInfo(LogModule.Editor, $"要为{inputPath.Count}个文件生成代码");
            var targetFolder = Application.dataPath + "/Scripts/GenCode/";
            try
            {
                var initCode = new StringBuilder("JsonUtils.InitJsonResolver(\n");
                foreach (var item in inputPath)
                {
                    initCode.Append($"    Generated{item.Key.Name}Resolver.Instance,\n");
                    var targetPath = targetFolder + item.Key.FullName + "Resolver.cs";
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                    var arg = $"-i {item.Value} -o {targetPath} -r Generated{item.Key.Name}Resolver -n Game.JsonResolver";
                    Logger.LogInfo(LogModule.Editor, $"生成{item.Key.FullName}Resolver.cs, 命令行\n{exePath} {arg}");
                    Process.Start(exePath, arg)?.WaitForExit();

                    if (!File.Exists(targetPath))
                    {
                        Logger.LogError(LogModule.Editor, "生成失败");
                    }
                }
                if (inputPath.Count > 0)
                {
                    initCode.Remove(initCode.Length - 2, 2);
                }
                initCode.Append("\n);");
                Logger.LogInfo(LogModule.Editor, $"生成成功，请复制以下代码到游戏初始化过程：\n{initCode}");
            }
            catch (Exception e)
            {
                Logger.LogError(LogModule.Editor, "执行生成工具发生异常");
                Logger.LogException(LogModule.Editor, e);
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
                    inputPath.Add(type, attr.GetFileFullPath());
                }
            }
        }
    }
}
