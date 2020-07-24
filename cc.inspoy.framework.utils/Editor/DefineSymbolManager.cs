/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Utils.Editor
 * FileName: DefineSymbolManager.cs
 * Created on 2018/05/23 by inspoy
 * All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Instech.Framework.Utils.Editor
{
    /// <summary>
    /// 管理自定义宏
    /// </summary>
    public static class DefineSymbolManager
    {
        /// <summary>
        /// 添加一个自定义宏
        /// </summary>
        /// <param name="symbolName">宏名</param>
        /// <param name="buildTarget">目标构建平台，默认为Standalone</param>
        public static void AddDefine(string symbolName, BuildTargetGroup buildTarget = BuildTargetGroup.Standalone)
        {
            var cur = GetAllDefines(buildTarget).ToList();
            if (cur.Contains(symbolName))
            {
                return;
            }
            cur.Add(symbolName);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, string.Join(";", cur));
        }

        /// <summary>
        /// 移除一个自定义宏，不存在的话返回false
        /// </summary>
        /// <param name="symbolName">宏名</param>
        /// <param name="buildTarget">目标构建平台，默认为Standalone</param>
        /// <returns></returns>
        public static bool RemoveDefine(string symbolName, BuildTargetGroup buildTarget = BuildTargetGroup.Standalone)
        {
            var cur = GetAllDefines(buildTarget).ToList();
            if (cur.IndexOf(symbolName) == -1)
            {
                // 不存在这个宏
                return false;
            }
            cur.Remove(symbolName);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, string.Join(";", cur));
            return true;
        }

        /// <summary>
        /// 查询是否已经定义了某个宏
        /// </summary>
        /// <param name="symbolName">宏名</param>
        /// <param name="buildTarget">目标构建平台，默认为Standalone</param>
        /// <returns></returns>
        public static bool HasDefine(string symbolName, BuildTargetGroup buildTarget = BuildTargetGroup.Standalone)
        {
            var cur = GetAllDefines(buildTarget).ToList();
            return cur.Contains(symbolName);
        }

        /// <summary>
        /// 获取当前所有自定义宏
        /// </summary>
        /// <param name="buildTarget">目标构建平台，默认为Standalone</param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllDefines(BuildTargetGroup buildTarget = BuildTargetGroup.Standalone)
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget).Split(';');
        }
    }
}
