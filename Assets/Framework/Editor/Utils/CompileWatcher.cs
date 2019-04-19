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
        private const string CompilingTimeMiliKey = "Instech_EditorPrefs_CompilingTimeMili";
        private static bool _isCompiling = EditorPrefs.GetBool(CompilingKey, false);
        private static int _beginTime = EditorPrefs.GetInt(CompilingTimeKey, 0);
        private static int _beginTimeMili = EditorPrefs.GetInt(CompilingTimeMiliKey, 0);

        static CompileWatcher()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (_isCompiling && !EditorApplication.isCompiling)
            {
                var curTime = Utility.GetMiliTimeStampNow();
                var cost = curTime - (ulong)_beginTime * 1000 - (ulong)_beginTimeMili;
                Logger.LogInfo(LogModule.Editor, $"Compiling cost: {cost / 1000f:F2}s");
                _isCompiling = false;
                EditorPrefs.SetBool(CompilingKey, false);
            }
            else if (!_isCompiling && EditorApplication.isCompiling)
            {
                _isCompiling = true;
                var curTime = Utility.GetMiliTimeStampNow();
                _beginTime = (int)(curTime / 1000);
                _beginTimeMili = (int)(curTime % 1000);
                EditorPrefs.SetBool(CompilingKey, true);
                EditorPrefs.SetInt(CompilingTimeKey, _beginTime);
                EditorPrefs.SetInt(CompilingTimeMiliKey, _beginTimeMili);
            }
        }
    }
}
