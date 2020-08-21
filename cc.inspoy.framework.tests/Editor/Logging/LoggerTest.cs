/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Editor.Tests
 * FileName: LoggerTest.cs
 * Created on 2020/08/19 by inspoy
 * All rights reserved.
 */

using System;
using System.Diagnostics;
using Instech.Framework.Logging;
using NUnit.Framework;
using UnityEngine;
using AssertionException = Instech.Framework.Logging.AssertionException;
using Logger = Instech.Framework.Logging.Logger;

namespace Instech.FrameworkTest.Logging
{
    [TestFixture]
    [Description("日志输出")]
    [Category("Framework")]
    public class LoggerTest
    {
        private class TestException : Exception
        {
        }

        private bool _gotDebug;
        private bool _gotVerbose;
        private bool _gotInfo;
        private bool _gotWarning;
        private bool _gotError;
        private bool _gotExcepion;
        private bool _gotAssert;

        [SetUp]
        public void SetUp()
        {
            Logger.CurDebugFlags = 1;
            Logger.OnLog += OnLog;
        }

        [TearDown]
        public void TearDown()
        {
            Logger.OnLog -= OnLog;
        }

        [Test]
        [Description("全关")]
        public void AllOff()
        {
            Reset();
            Logger.LogLevels = LogLevels.None;
            EmitSomeLogs();
            Assert.IsFalse(_gotDebug || _gotVerbose || _gotInfo || _gotWarning || _gotError || _gotExcepion || _gotAssert);
        }

        [Test]
        [Description("开启部分等级（非连续）")]
        public void PartiallyOn()
        {
            Reset();
            Logger.LogLevels = LogLevels.Assert | LogLevels.Error | LogLevels.Info;
            EmitSomeLogs();
            Assert.IsTrue(_gotAssert && _gotError && _gotInfo);
            Assert.IsFalse(_gotExcepion || _gotWarning || _gotVerbose || _gotDebug);
        }

        [Test]
        [Description("全开")]
        public void AllOn()
        {
            Reset();
            Logger.LogLevels = LogLevels.All;
            EmitSomeLogs();
            Assert.IsTrue(_gotDebug && _gotVerbose && _gotInfo && _gotWarning && _gotError && _gotExcepion && _gotAssert);
        }

        [Test]
        [Description("杂项覆盖")]
        public void Misc()
        {
            Logger.LogLevels = LogLevels.All;
            Logger.Print("TestPrint");
            Logger.LogException(null, null);
            Logger.LogInfo(null, null);
        }

        private void EmitSomeLogs()
        {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            Logger.LogDebug(null, 1, "TestDebug");
            Logger.LogVerbose(null, "TestVerbose");
            Logger.LogInfo(null, "TestInfo");
            Logger.LogWarning(null, "TestWarning");
            Logger.LogError(null, "TestError");
            Logger.LogException(null, new TestException());
            Assert.Throws<AssertionException>(()=>
            {
                Logger.Assert(null, false, "TestAssert");
            });
            Assert.DoesNotThrow(() =>
            {
                Logger.Assert(null, true, "TestAssert");
            });
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
        }

        private void OnLog(string module, string content, LogLevels level, StackTrace trace)
        {
            switch (level)
            {
                case LogLevels.Exception:
                    _gotExcepion = true;
                    break;
                case LogLevels.Assert:
                    _gotAssert = true;
                    break;
                case LogLevels.Error:
                    _gotError = true;
                    break;
                case LogLevels.Warning:
                    _gotWarning = true;
                    break;
                case LogLevels.Info:
                    _gotInfo = true;
                    break;
                case LogLevels.Verbose:
                    _gotVerbose = true;
                    break;
                case LogLevels.Debug:
                    _gotDebug = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        private void Reset()
        {
            _gotDebug = false;
            _gotVerbose = false;
            _gotInfo = false;
            _gotWarning = false;
            _gotError = false;
            _gotExcepion = false;
            _gotAssert = false;
        }
    }
}
