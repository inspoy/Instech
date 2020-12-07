// == Inspoy Technology ==
// Assembly: Instech.FrameworkTest.Core
// FileName: SingletonTest.cs
// Created on 2020/08/17 by inspoy
// All rights reserved.

using System;
using Instech.Framework.Core;
using NUnit.Framework;

namespace Instech.FrameworkTest.Core
{
    public class AutoCreateSingletonForTest : AutoCreateSingleton<AutoCreateSingletonForTest>
    {
        public int SomeProperty { get; set; }

        protected override void Init()
        {
            SomeProperty = 123;
        }
    }

    public class SingletonForTest : Singleton<SingletonForTest>
    {
        public int SomeProperty { get; set; }

        protected override void Init()
        {
            SomeProperty = 456;
        }
    }

    [TestFixture]
    [Description("单例")]
    [Category("Framework")]
    public class SingletonTest
    {
        [TearDown]
        public void TearDown()
        {
            SingletonForTest.DestroySingleton();
            AutoCreateSingletonForTest.DestroySingleton();
        }

        [Test]
        [Description("创建和销毁普通单例")]
        public void CreateDestroySingleton()
        {
            Assert.IsFalse(SingletonForTest.HasSingleton());
            SingletonForTest.CreateSingleton();
            Assert.IsTrue(SingletonForTest.HasSingleton());
            Assert.AreEqual(456, SingletonForTest.Instance.SomeProperty);
            SingletonForTest.DestroySingleton();
            Assert.IsFalse(SingletonForTest.HasSingleton());
        }

        [Test]
        [Description("不创建普通单例就访问")]
        public void AccessSingletonWithoutCreating()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var instance = SingletonForTest.Instance;
            });
            Assert.DoesNotThrow(() =>
            {
                var instance = AutoCreateSingletonForTest.Instance;
                Assert.AreEqual(123, instance.SomeProperty);
                AutoCreateSingletonForTest.DestroySingleton();
            });
        }

        [Test]
        [Description("多次创建普通单例")]
        public void CreateSingletonMultiTimes()
        {
            SingletonForTest.CreateSingleton();
            Assert.Throws<MethodAccessException>(SingletonForTest.CreateSingleton);
            SingletonForTest.DestroySingleton();
        }
    }
}
