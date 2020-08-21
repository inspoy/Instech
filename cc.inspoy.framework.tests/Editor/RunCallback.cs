/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Tests
 * FileName: RunCallback.cs
 * Created on 2020/08/17 by inspoy
 * All rights reserved.
 */

using System.IO;
using System.Linq;
using Instech.FrameworkTest;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(RunCallback))]

namespace Instech.FrameworkTest
{
    public class RunCallback : ITestRunCallback
    {
        public void RunStarted(ITest testsToRun)
        {
            // do nothing
        }

        public void RunFinished(ITestResult testResults)
        {
            var mode = testResults.Children.First().Test.Properties.Get("platform").ToString();
            var resultPath = Path.Combine(Application.persistentDataPath, "TestResults.xml");
            var targetPath = Path.GetFullPath(Application.dataPath + $"/../TestResults_{mode}.xml");
            File.Copy(resultPath, targetPath, true);
            Debug.Log("[FrameworkTest]Tests completed, result copied to:\n"+targetPath);
        }

        public void TestStarted(ITest test)
        {
            // do nothing
        }

        public void TestFinished(ITestResult result)
        {
            // do nothing
        }
    }
}
