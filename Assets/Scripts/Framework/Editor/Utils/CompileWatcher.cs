/**
 * == Inspoy Technology ==
 * Assembly: Framework.Editor
 * FileName: CompileWatcher.cs
 * Created on 2018/05/07 by inspoy
 * All rights reserved.
 */

using UnityEditor;

namespace Instech.Framework.Editor
{
    /// <summary>
    /// 统计工程编译时间
    /// </summary>
    [InitializeOnLoad]
    public static class CompileWatcher
    {
        private const string CompilingKey = "Instech_EditorPrefs_Compling";
        private const string CompilingTimeKey = "Instech_EditorPrefs_ComplingTime";
        private static bool _isCompiling = EditorPrefs.GetBool(CompilingKey, false);
        private static int _beginTime = EditorPrefs.GetInt(CompilingTimeKey, 0);

        static CompileWatcher()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (_isCompiling && !EditorApplication.isCompiling)
            {
                var cost = Utility.GetTimeStampNow() - _beginTime;
                Logger.LogInfo(LogModule.Editor, $"Compiling cost: {cost}s");
                _isCompiling = false;
                EditorPrefs.SetBool(CompilingKey, false);
            }
            else if (!_isCompiling && EditorApplication.isCompiling)
            {
                _isCompiling = true;
                _beginTime = (int)Utility.GetTimeStampNow();
                EditorPrefs.SetBool(CompilingKey, true);
                EditorPrefs.SetInt(CompilingTimeKey, _beginTime);
            }
        }
    }
}
