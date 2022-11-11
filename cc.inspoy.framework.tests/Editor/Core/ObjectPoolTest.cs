// == Inspoy Technology ==
// Assembly: Instech.Framework.Tests
// FileName: ObjectPoolTest.cs
// Created on 2020/08/17 by inspoy
// All rights reserved.

using Instech.Framework.Core;
using NUnit.Framework;
using UnityEngine;

namespace Instech.FrameworkTest.Core
{
    public class OrdinaryPoolableObjectForTest
    {
        public int SomeProperty { get; set; }
    }

    public class ObjectPoolForTest : IPoolable
    {
        public int SomeProperty { get; set; }

        public void OnActivate()
        {
            SomeProperty = 123;
        }

        public void OnDestroy()
        {
            // do nothing
        }

        public void OnRecycle()
        {
            // do nothing
        }
    }

    [TestFixture]
    [Description("对象池")]
    [Category("Framework")]
    public class ObjectPoolTest
    {
        [TearDown]
        public void TearDown()
        {
            ObjectPool<ObjectPoolForTest>.DestroySingleton();
        }

        [Test]
        [Description("新建空对象池")]
        public void NewPool()
        {
            ObjectPool<ObjectPoolForTest>.CreateSingleton();
            Assert.AreEqual(0, ObjectPool<ObjectPoolForTest>.Instance.PooledCount);
            ObjectPool<ObjectPoolForTest>.DestroySingleton();
        }

        [Test]
        [Description("池化回收")]
        public void Pooling()
        {
            ObjectPool<ObjectPoolForTest>.CreateSingleton();

            var obj = ObjectPool<ObjectPoolForTest>.Instance.Get();
            obj.SomeProperty = 233;
            ObjectPool<ObjectPoolForTest>.Instance.Recycle(obj);
            Assert.AreEqual(1, ObjectPool<ObjectPoolForTest>.Instance.PooledCount);

            ObjectPool<ObjectPoolForTest>.DestroySingleton();
        }

        [Test]
        [Description("再利用")]
        public void ReUse()
        {
            ObjectPool<ObjectPoolForTest>.CreateSingleton();
            var obj1 = ObjectPool<ObjectPoolForTest>.GetNew();
            obj1.SomeProperty = 321;
            obj1.Recycle();
            var obj2 = ObjectPool<ObjectPoolForTest>.GetNew();
            Assert.AreEqual(0, ObjectPool<ObjectPoolForTest>.Instance.PooledCount);
            Assert.AreEqual(123, obj2.SomeProperty);
            ObjectPool<ObjectPoolForTest>.DestroySingleton();
        }

        [Test]
        [Description("批量初始化")]
        public void BatchAlloc()
        {
            ObjectPool<ObjectPoolForTest>.CreateSingleton();
            ObjectPool<ObjectPoolForTest>.Instance.Alloc();
            Assert.AreEqual(ObjectPool<ObjectPoolForTest>.Instance.MaxCount, ObjectPool<ObjectPoolForTest>.Instance.PooledCount);
            ObjectPool<ObjectPoolForTest>.DestroySingleton();
        }

        [Test]
        [Description("对象池清空")]
        public void ClearPool()
        {
            ObjectPool<ObjectPoolForTest>.CreateSingleton();
            ObjectPool<ObjectPoolForTest>.Instance.Alloc(5);
            ObjectPool<ObjectPoolForTest>.Instance.Clear();
            Assert.AreEqual(0, ObjectPool<ObjectPoolForTest>.Instance.PooledCount);
            ObjectPool<ObjectPoolForTest>.DestroySingleton();
        }

        [Test]
        [Description("对象池溢出")]
        public void Overflow()
        {
            ObjectPool<ObjectPoolForTest>.CreateSingleton();
            ObjectPool<ObjectPoolForTest>.Instance.MaxCount = 1;
            var obj1 = ObjectPool<ObjectPoolForTest>.GetNew();
            var obj2 = ObjectPool<ObjectPoolForTest>.GetNew();
            Assert.AreEqual(2, ObjectPool<ObjectPoolForTest>.Instance.CreatedCount);
            Assert.AreEqual(2, ObjectPool<ObjectPoolForTest>.Instance.ActiveCount);
            obj1.Recycle();
            obj2.Recycle();
            Assert.AreEqual(1, ObjectPool<ObjectPoolForTest>.Instance.PooledCount);
            Assert.AreEqual(0, ObjectPool<ObjectPoolForTest>.Instance.ActiveCount);
            ObjectPool<ObjectPoolForTest>.DestroySingleton();
        }

        [Test]
        [Description("获取调试用字符串")]
        public void DebugInfo()
        {
            if (!ObjectPoolManager.HasSingleton())
            {
                ObjectPoolManager.CreateSingleton();
            }
            Assert.DoesNotThrow(() =>
            {
                var info = ObjectPoolManager.Instance.GetDebugInformation();
                Debug.Log(info);
            });
        }

        [Test]
        [Description("非IPoolable对象的支持")]
        public void OridinaryObject()
        {
            var destroyCounter = 0;
            ObjectPool<OrdinaryPoolableObjectForTest>.CreateSingleton();

            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.RecycleCallback = obj => { obj.SomeProperty = 123; };
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.ActivateCallback = ObjectPool<OrdinaryPoolableObjectForTest>.Instance.EmptyCallback;
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.DestroyCallback = _ => { destroyCounter += 1; };

            var obj1 = ObjectPool<OrdinaryPoolableObjectForTest>.GetNew();
            obj1.SomeProperty = 321;
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.Recycle(obj1);
            Assert.AreEqual(123, obj1.SomeProperty);
            Assert.AreEqual(1, ObjectPool<OrdinaryPoolableObjectForTest>.Instance.PooledCount);
            
            Assert.DoesNotThrow(() =>
            {
                var obj2 = ObjectPool<OrdinaryPoolableObjectForTest>.GetNew();
                ObjectPool<OrdinaryPoolableObjectForTest>.Instance.Recycle(obj2);
            });
            
            Assert.AreEqual(0, destroyCounter);
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.MaxCount = 1;
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.Recycle(new OrdinaryPoolableObjectForTest());
            Assert.AreEqual(1, destroyCounter);
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.Clear();
            Assert.AreEqual(2, destroyCounter);

            ObjectPool<OrdinaryPoolableObjectForTest>.DestroySingleton();
        }

        [Test]
        [Description("非IPoolable对象没有设置池化回调的情况")]
        public void OridinaryObjectCallbackNotSet()
        {
            ObjectPool<OrdinaryPoolableObjectForTest>.CreateSingleton();

            // new
            Assert.Throws<PoolingCallbackNotSetException>(() => { ObjectPool<OrdinaryPoolableObjectForTest>.GetNew(); });
            // recycle
            Assert.Throws<PoolingCallbackNotSetException>(() => { ObjectPool<OrdinaryPoolableObjectForTest>.Instance.Recycle(null); });
            // reuse
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.ActivateCallback = ObjectPool<OrdinaryPoolableObjectForTest>.Instance.EmptyCallback;
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.RecycleCallback = ObjectPool<OrdinaryPoolableObjectForTest>.Instance.EmptyCallback;
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.DestroyCallback = ObjectPool<OrdinaryPoolableObjectForTest>.Instance.EmptyCallback;
            var obj = ObjectPool<OrdinaryPoolableObjectForTest>.Instance.Get();
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.Recycle(obj);
            ObjectPool<OrdinaryPoolableObjectForTest>.Instance.ActivateCallback = null;
            Assert.Throws<PoolingCallbackNotSetException>(() => { ObjectPool<OrdinaryPoolableObjectForTest>.GetNew(); });
            // clear
            Assert.Throws<PoolingCallbackNotSetException>(ObjectPool<OrdinaryPoolableObjectForTest>.Instance.Clear);

            ObjectPool<OrdinaryPoolableObjectForTest>.DestroySingleton();
        }
    }
}
