// == Inspoy Technology ==
// Assembly: Instech.Framework.Utils
// FileName: Assert.cs
// Created on 2021/03/10 by inspoy
// All rights reserved.

#define INSTECH_ASSERT_ENABLE

using System.Collections.Generic;
using System.Diagnostics;
using Instech.Framework.Logging;
using JetBrains.Annotations;

namespace Instech.Framework.Utils
{
    /// <summary>
    /// 断言短语集合，接口类似<see cref="UnityEngine.Assertions.Assert"/>，集成了日志模块和<c>Instech.Framework.Logging</c>
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// 断言指定条件成立
        /// </summary>
        [Conditional("INSTECH_ASSERT_ENABLE")]
        [AssertionMethod]
        public static void IsTrue(string module, [AssertionCondition(AssertionConditionType.IS_TRUE)]bool condition, string message)
        {
            Logger.Assert(module, condition, message);
        }

        /// <summary>
        /// 断言指定条件<b>不</b>成立
        /// </summary>
        [Conditional("INSTECH_ASSERT_ENABLE")]
        [AssertionMethod]
        public static void IsFalse(string module, [AssertionCondition(AssertionConditionType.IS_FALSE)]bool condition, string message)
        {
            Logger.Assert(module, !condition, message);
        }

        /// <summary>
        /// 断言指定引用对象为<c>null</c>
        /// </summary>
        [Conditional("INSTECH_ASSERT_ENABLE")]
        [AssertionMethod]
        public static void IsNull(string module, [AssertionCondition(AssertionConditionType.IS_NULL)]object target, string message)
        {
            Logger.Assert(module, target == null, message);
        }

        /// <summary>
        /// 断言指定引用对象<b>不</b>为<c>null</c>
        /// </summary>
        [Conditional("INSTECH_ASSERT_ENABLE")]
        [AssertionMethod]
        public static void IsNotNull(string module, [AssertionCondition(AssertionConditionType.IS_NOT_NULL)]object target, string message)
        {
            Logger.Assert(module, target != null, message);
        }

        /// <summary>
        /// 断言给定的值和期望值<b>相等</b>
        /// </summary>
        /// <remarks>即使两个对象不相同(不引用同一个对象)，它们也可能逻辑上是相等的</remarks>
        [Conditional("INSTECH_ASSERT_ENABLE")]
        [AssertionMethod]
        public static void AreEqual<T>(string module, T expected, T actual, string message)
        {
            var comparer = EqualityComparer<T>.Default;
            var equal = comparer.Equals(expected, actual);
            Logger.Assert(module, equal, message);
        }

        /// <summary>
        /// 断言给定的值和期望值<b>不相等</b>
        /// </summary>
        [Conditional("INSTECH_ASSERT_ENABLE")]
        [AssertionMethod]
        public static void AreNotEqual<T>(string module, T expected, T actual, string message)
        {
            var comparer = EqualityComparer<T>.Default;
            var equal = comparer.Equals(expected, actual);
            Logger.Assert(module, equal, message);
        }

        /// <summary>
        /// 断言给定的值和期望值<b>相同</b>
        /// </summary>
        /// <remarks>即使两个对象在逻辑上是相等的，它们也可能不相同(不引用同一个对象)</remarks>
        [Conditional("INSTECH_ASSERT_ENABLE")]
        [AssertionMethod]
        public static void AreSame(string module, object expected, object actual, string message)
        {
            Logger.Assert(module, ReferenceEquals(expected, actual), message);
        }

        /// <summary>
        /// 断言给定的值和期望值<b>不相同</b>
        /// </summary>
        [Conditional("INSTECH_ASSERT_ENABLE")]
        [AssertionMethod]
        public static void AreNotSame(string module, object expected, object actual, string message)
        {
            Logger.Assert(module, !ReferenceEquals(expected, actual), message);
        }

        /// <summary>
        /// 断言这里的代码不应当被执行到
        /// </summary>
        [Conditional("INSTECH_ASSERT_ENABLE")]
        [AssertionMethod]
        [ContractAnnotation("=> halt")]
        public static void ShouldNotHappen(string module, string message)
        {
            Logger.Assert(module, false, message);
        }
    }
}
