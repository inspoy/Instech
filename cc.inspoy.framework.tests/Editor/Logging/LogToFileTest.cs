/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Editor.Tests
 * FileName: LogToFileTest.cs
 * Created on 2020/09/14 by inspoy
 * All rights reserved.
 */

using System.Threading;
using Instech.Framework.Core;
using Instech.Framework.Logging;
using NUnit.Framework;

namespace Instech.FrameworkTest.Logging
{
    [TestFixture]
    [Description("日志输出到文件")]
    [Category("Framework")]
    public class LogToFileTest
    {
        [Test]
        public void DoTests()
        {
            ObjectPoolManager.DestroySingleton();
            ObjectPoolManager.CreateSingleton();
            LogToFile.CreateSingleton();
            
            Logger.LogInfo(null, "TestLog");
            Thread.Sleep(500); // 等待写文件的线程写入完成
            Assert.Greater(ObjectPool<LogToFileItem>.Instance.CreatedCount, 0);
            ObjectPool<LogToFileItem>.Instance.Clear();
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            Logger.DisableErrorStackTrace = true;
            Logger.LogError(null, "TestError without stacktrace");
            Logger.DisableErrorStackTrace = false;
            Logger.LogError(null, "TestError1 with stacktrace");
            Thread.Sleep(500); // 等待写文件的线程写入完成
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            
            LogToFile.DestroySingleton();
            ObjectPoolManager.DestroySingleton();
        }
    }
}
