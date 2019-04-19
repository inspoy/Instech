/**
 * == Inspoy Technology ==
 * Assembly: Tests
 * FileName: TestCommon.cs
 * Created on 2018/11/17 by inspoy
 * All rights reserved.
 */

using System;
using Instech.Framework;
using NUnit.Framework;

namespace FrameworkTests
{
    [TestFixture]
    [Category("Framework")]
    public class TestCommon
    {
        private class TestForSingleton : Singleton<TestForSingleton>
        {
            public int SomeProperty { get; set; }
            protected override void Init()
            {
                SomeProperty = 123;
            }
        }

        private class TestForObjectPool : IPoolable
        {
            public int SomeProperty { get; set; }
            public void OnActivate()
            {
                SomeProperty = 123;
            }

            public void OnRecycle()
            {
                // do nothing
            }

            #region IDisposable Support
            private bool _disposedValue; // 要检测冗余调用

            private void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    _disposedValue = true;
                }
            }
            public void Dispose()
            {
                Dispose(true);
            }
            #endregion
        }

        private class TestForGameState : IGameState
        {
            public bool Entered { get; private set; }
            public int Counter { get; private set; }
            public void OnStateEnter(string lastState)
            {
                Entered = true;
            }

            public void OnStateLeave(string nextState)
            {
                Entered = false;
            }

            public void UpdateFrame(float dt)
            {
                Counter += 2;
            }

            public void UpdateLogic(float dt)
            {
                Counter += 1;
            }
        }

        [Test]
        [Description("游戏状态切换")]
        public void GameState()
        {
            GameStateMachine.CreateSingleton();
            var state = new TestForGameState();
            GameStateMachine.Instance.RegisterGameState(state);
            Assert.AreEqual(false, state.Entered);
            GameStateMachine.Instance.ChangeState(typeof(TestForGameState));
            Assert.AreEqual(true, state.Entered, "状态没有正确进入");
            Assert.AreEqual(0, state.Counter);
            GameStateMachine.Instance.UpdateLogic(0);
            Assert.AreEqual(1, state.Counter, "没有正确执行UpdateLogic");
            GameStateMachine.Instance.UpdateFrame(0);
            Assert.AreEqual(3, state.Counter, "没有正确执行UpdateFrame");
            GameStateMachine.DestroySingleton();
            Assert.AreEqual(false, state.Entered, "状态没有正确退出");
        }

        [Test]
        [Description("对象池")]
        public void ObjectPool()
        {
            ObjectPoolManager.DestroySingleton();

            ObjectPoolManager.CreateSingleton();
            try
            {
                ObjectPool<TestForObjectPool>.CreateSingleton();
                Assert.AreEqual(0, ObjectPool<TestForObjectPool>.Instance.PooledCount);

                var obj1 = ObjectPool<TestForObjectPool>.Instance.Get();
                obj1.SomeProperty = 456;
                ObjectPool<TestForObjectPool>.Instance.Recycle(obj1);
                Assert.AreEqual(1, ObjectPool<TestForObjectPool>.Instance.PooledCount);

                var obj2 = ObjectPool<TestForObjectPool>.Instance.Get();
                Assert.AreEqual(0, ObjectPool<TestForObjectPool>.Instance.PooledCount);
                Assert.AreEqual(123, obj2.SomeProperty);
                obj2.Dispose();

                ObjectPool<TestForObjectPool>.Instance.Alloc(5);
                Assert.AreEqual(5, ObjectPool<TestForObjectPool>.Instance.PooledCount);

                ObjectPool<TestForObjectPool>.Instance.Clear();
                Assert.AreEqual(0, ObjectPool<TestForObjectPool>.Instance.PooledCount);
                ObjectPool<TestForObjectPool>.DestroySingleton();
            }
            finally
            {
                ObjectPoolManager.DestroySingleton();
            }
        }

        [Test]
        [Description("单例")]
        public void Singleton()
        {
            TestForSingleton.CreateSingleton();
            Assert.AreEqual(123, TestForSingleton.Instance.SomeProperty);
            TestForSingleton.Instance.SomeProperty = 456;
            Assert.AreEqual(456, TestForSingleton.Instance.SomeProperty);
            var ok = false;
            try
            {
                TestForSingleton.CreateSingleton();
            }
            catch (MethodAccessException)
            {
                ok = true;
            }
            catch (Exception e)
            {
                Assert.Fail("单例重复创建出现其他异常" + e);
            }
            Assert.AreEqual(true, ok, "单例可重复创建");
            TestForSingleton.DestroySingleton();
            Assert.AreEqual(false, TestForSingleton.HasSingleton(), "单例没有被销毁");
            TestForSingleton.DestroySingleton();
        }
    }

    [TestFixture]
    [Category("Framework")]
    public class TestCommonEvent
    {
        [Test]
        [Description("事件派发")]
        public void DispatchEvent()
        {
            ObjectPoolManager.DestroySingleton();


            ObjectPoolManager.CreateSingleton();
            try
            {
                var dispatcher = new EventDispatcher(this);
                var triggered = false;
                dispatcher.AddEventListener("TestEvent", e => triggered = true);
                dispatcher.DispatchEvent("TestEvent");
                Assert.AreEqual(true, triggered, "事件回调没有被触发");
                triggered = false;
                dispatcher.DispatchEvent("AnotherEvent");
                Assert.AreEqual(false, triggered, "回调被其他事件触发了");
                triggered = false;
                dispatcher.RemoveAllEventListeners("TestEvent");
                dispatcher.DispatchEvent("TestEvent");
                Assert.AreEqual(false, triggered, "回调被注销后还能被触发");
            }
            finally
            {
                ObjectPoolManager.DestroySingleton();
            }
        }

        [Test]
        [Description("响应式属性")]
        public void ReactiveProperty()
        {
            ObjectPoolManager.DestroySingleton();


            ObjectPoolManager.CreateSingleton();
            try
            {
                var rp = new ReactiveProperty<int> { Value = 123 };
                var triggered = false;
                rp.AddEventListener(val => triggered = true);
                rp.Value = 456;
                Assert.AreEqual(true, triggered, "值改为不同值没有触发回调");
            }
            finally
            {
                ObjectPoolManager.DestroySingleton();
            }
        }

        [Test]
        [Description("响应式属性改为相同值")]
        public void ReactivePropertySameValue()
        {
            var rp = new ReactiveProperty<int> { Value = 123 };
            var triggered = false;
            rp.AddEventListener(val => triggered = true);
            rp.Value = 123;
            Assert.AreEqual(true, triggered, "值改为相同值没有触发回调");
        }

        [Test]
        [Description("响应式属性(过滤相同值)")]
        public void ReactivePropertyAlwaysTrigger()
        {
            var rp = new ReactiveProperty<int> { Value = 123, AlwaysTrigger = false };
            var triggered = false;
            rp.AddEventListener(val => triggered = true);
            rp.Value = 123;
            Assert.AreEqual(false, triggered, "值改为相同值触发了回调");
        }
    }
}
