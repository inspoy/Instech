// == Inspoy Technology ==
// Assembly: Instech.Framework.Tests
// FileName: MonoSingletonTest.cs
// Created on 2020/08/17 by inspoy
// All rights reserved.

using System;
using System.Collections;
using Instech.Framework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Instech.FrameworkTest.Core
{
    public class MonoSingletonForTest : MonoSingleton<MonoSingletonForTest>
    {
        public int SomeProperty { get; set; }

        protected override void Init()
        {
            SomeProperty = 789;
        }
    }

    [TestFixture]
    [Description("MonoBehaviour单例")]
    [Category("Framework")]
    public class MonoSingletonTest
    {
        [TearDown]
        public void TearDown()
        {
            MonoSingletonForTest.DestroySingleton();
        }
        
        [UnityTest]
        [Description("创建和销毁MonoBehaviour单例")]
        public IEnumerator CreateDestroyMonoSingleton()
        {
            Assert.IsFalse(MonoSingletonForTest.HasSingleton());
            MonoSingletonForTest.CreateSingleton();
            Assert.IsTrue(MonoSingletonForTest.HasSingleton());
            Assert.AreEqual(789, MonoSingletonForTest.Instance.SomeProperty);
            MonoSingletonForTest.DestroySingleton();
            Assert.IsFalse(MonoSingletonForTest.HasSingleton());
            yield return null;
        }

        [Test]
        [Description("不创建MonoBehaviour单例就访问")]
        public void AccessMonoSingletonWithoutCreating()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var instance = MonoSingletonForTest.Instance;
            });
        }

        [UnityTest]
        [Description("多次创建普通单例")]
        public IEnumerator CreateMonoSingletonMultiTimes()
        {
            MonoSingletonForTest.CreateSingleton();
            Assert.Throws<MethodAccessException>(MonoSingletonForTest.CreateSingleton);
            MonoSingletonForTest.DestroySingleton();
            yield return null;
        }

        [Test]
        [Description("手动在场景中添加单例脚本")]
        public void CreateMonoSingletonManually()
        {
            var go = new GameObject("MonoSingletonTest");
            go.AddComponent<MonoSingletonForTest>().SomeProperty = 233;
            try
            {
                Assert.DoesNotThrow(MonoSingletonForTest.CreateSingleton);
                Assert.AreEqual(233, MonoSingletonForTest.Instance.SomeProperty);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        [Description("手动在场景中添加多个单例脚本")]
        public void CreateMultipleMonoSingletonManually()
        {
            var go1 = new GameObject("MonoSingletonTest1");
            go1.AddComponent<MonoSingletonForTest>();
            var go2 = new GameObject("MonoSingletonTest2");
            go2.AddComponent<MonoSingletonForTest>();
            Assert.Throws<MethodAccessException>(MonoSingletonForTest.CreateSingleton);
            Object.DestroyImmediate(go1);
            Object.DestroyImmediate(go2);
        }
    }
}
