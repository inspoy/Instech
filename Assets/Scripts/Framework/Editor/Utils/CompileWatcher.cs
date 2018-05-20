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
        private static bool _isCompiling;
        private static int _beginTime;

        static CompileWatcher()
        {
            _isCompiling = EditorPrefs.GetBool(CompilingKey, false);
            _beginTime = EditorPrefs.GetInt(CompilingTimeKey, 0);
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (_isCompiling && !EditorApplication.isCompiling)
            {
                var cost = Utility.GetTimeStampNow() - _beginTime;
                Logger.LogInfo("Editor", $"Compiling cost: {cost}s");
                EditorUtility.DisplayDialog("编译完成", $"编译耗时：{cost}s", "OK");
                _isCompiling = false;
                EditorPrefs.SetBool(CompilingKey, false);
            }
            else if (!_isCompiling && EditorApplication.isCompiling)
            {
                _isCompiling = true;
                _beginTime = Utility.GetTimeStampNow();
                EditorPrefs.SetBool(CompilingKey, true);
                EditorPrefs.SetInt(CompilingTimeKey, _beginTime);
            }
        }
    }
}
