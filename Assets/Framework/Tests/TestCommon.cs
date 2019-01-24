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
            private bool _disposedValue; // Ҫ����������

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
        [Description("��Ϸ״̬�л�")]
        public void GameState()
        {
            GameStateMachine.CreateSingleton();
            var state = new TestForGameState();
            GameStateMachine.Instance.RegisterGameState(state);
            Assert.AreEqual(false, state.Entered);
            GameStateMachine.Instance.ChangeState(typeof(TestForGameState));
            Assert.AreEqual(true, state.Entered, "״̬û����ȷ����");
            Assert.AreEqual(0, state.Counter);
            GameStateMachine.Instance.UpdateLogic(0);
            Assert.AreEqual(1, state.Counter, "û����ȷִ��UpdateLogic");
            GameStateMachine.Instance.UpdateFrame(0);
            Assert.AreEqual(3, state.Counter, "û����ȷִ��UpdateFrame");
            GameStateMachine.DestroySingleton();
            Assert.AreEqual(false, state.Entered, "״̬û����ȷ�˳�");
        }

        [Test]
        [Description("�����")]
        public void ObjectPool()
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

        [Test]
        [Description("����")]
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
                Assert.Fail("�����ظ��������������쳣" + e);
            }
            Assert.AreEqual(true, ok, "�������ظ�����");
            TestForSingleton.DestroySingleton();
            Assert.AreEqual(123, TestForSingleton.Instance.SomeProperty, "����û�б�����");
            TestForSingleton.DestroySingleton();
        }
    }

    [TestFixture]
    [Category("Framework")]
    public class TestCommonEvent
    {
        [Test]
        [Description("�¼��ɷ�")]
        public void DispatchEvent()
        {
            var dispatcher = new EventDispatcher(this);
            var triggered = false;
            dispatcher.AddEventListener("TestEvent", e => triggered = true);
            dispatcher.DispatchEvent("TestEvent");
            Assert.AreEqual(true, triggered, "�¼��ص�û�б�����");
            triggered = false;
            dispatcher.DispatchEvent("AnotherEvent");
            Assert.AreEqual(false, triggered, "�ص��������¼�������");
            triggered = false;
            dispatcher.RemoveAllEventListeners("TestEvent");
            dispatcher.DispatchEvent("TestEvent");
            Assert.AreEqual(false, triggered, "�ص���ע�����ܱ�����");
        }

        [Test]
        [Description("��Ӧʽ����")]
        public void ReactiveProperty()
        {
            var rp = new ReactiveProperty<int> { Value = 123 };
            var triggered = false;
            rp.AddEventListener(val => triggered = true);
            rp.Value = 456;
            Assert.AreEqual(true, triggered, "ֵ��Ϊ��ֵͬû�д����ص�");
        }

        [Test]
        [Description("��Ӧʽ���Ը�Ϊ��ֵͬ")]
        public void ReactivePropertySameValue()
        {
            var rp = new ReactiveProperty<int> { Value = 123 };
            var triggered = false;
            rp.AddEventListener(val => triggered = true);
            rp.Value = 123;
            Assert.AreEqual(true, triggered, "ֵ��Ϊ��ֵͬû�д����ص�");
        }

        [Test]
        [Description("��Ӧʽ����(������ֵͬ)")]
        public void ReactivePropertyAlwaysTrigger()
        {
            var rp = new ReactiveProperty<int> { Value = 123, AlwaysTrigger = false };
            var triggered = false;
            rp.AddEventListener(val => triggered = true);
            rp.Value = 123;
            Assert.AreEqual(false, triggered, "ֵ��Ϊ��ֵͬ�����˻ص�");
        }
    }
}
