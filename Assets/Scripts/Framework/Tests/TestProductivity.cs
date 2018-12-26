/**
 * == Inspoy Technology ==
 * Assembly: Framework.Tests
 * FileName: TestProductivity.cs
 * Created on 2018/11/19 by inspoy
 * All rights reserved.
 */

using System;
using Instech.Framework;
using NUnit.Framework;
using UnityEngine;
using Logger = Instech.Framework.Logger;

namespace FrameworkTests
{
    [TestFixture]
    [Category("Framework")]
    public class TestProductivity
    {
        private class TestException : Exception
        {
        }

        public void EmitSomeLogs()
        {
            Logger.LogDebug(null, "TestDebug");
            Logger.LogInfo(null, "TestInfo");
            Logger.LogWarning(null, "TestWarning");
            Logger.LogError(null, "TestError");
            Logger.LogException(null, new TestException());
            Logger.Assert(null, false, "TestAssert");
        }

        [SetUp]
        public void SetUp()
        {
            Logger.DisableUnityLog = true;
        }

        [TearDown]
        public void TearDown()
        {
            Logger.DisableUnityLog = false;
        }

        [Test]
        [Description("日志系统")]
        public void TestLogger()
        {
            var gotDebug = false;
            var gotInfo = false;
            var gotWarning = false;
            var gotError = false;
            var gotExcepion = false;
            var gotAssert = false;
            Logger.OnLog += (module, content, level, trace) =>
            {
                switch (level)
                {
                    case LogLevel.Exception:
                        gotExcepion = true;
                        break;
                    case LogLevel.Assert:
                        gotAssert = true;
                        break;
                    case LogLevel.Error:
                        gotError = true;
                        break;
                    case LogLevel.Warning:
                        gotWarning = true;
                        break;
                    case LogLevel.Info:
                        gotInfo = true;
                        break;
                    case LogLevel.Debug:
                        gotDebug = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            };

            // 全关
            Logger.LogLevel = LogLevel.Off;
            EmitSomeLogs();
            Assert.IsFalse(gotDebug || gotInfo || gotWarning || gotError || gotExcepion || gotAssert);
            gotDebug = gotInfo = gotWarning = gotError = gotExcepion = gotAssert = false;

            // Error等级
            Logger.LogLevel = LogLevel.Error;
            EmitSomeLogs();
            Assert.IsTrue(gotError && gotExcepion && gotAssert);
            Assert.IsFalse(gotDebug || gotInfo || gotWarning);
            gotDebug = gotInfo = gotWarning = gotError = gotExcepion = gotAssert = false;

            // Info等级
            Logger.LogLevel = LogLevel.Info;
            EmitSomeLogs();
            Assert.IsTrue(gotInfo && gotWarning && gotError && gotExcepion && gotAssert);
            Assert.IsFalse(gotDebug);
            gotDebug = gotInfo = gotWarning = gotError = gotExcepion = gotAssert = false;

            // 全开
            Logger.LogLevel = LogLevel.All;
            EmitSomeLogs();
            Assert.IsTrue(gotDebug && gotInfo && gotWarning && gotError && gotExcepion && gotAssert);
        }
    }
}
