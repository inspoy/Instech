/**
 * == Inspoy Technology ==
 * Assembly: Framework.Tests
 * FileName: TestData.cs
 * Created on 2018/11/19 by inspoy
 * All rights reserved.
 */

using Instech.Framework;
using NUnit.Framework;

namespace FrameworkTests
{
    [TestFixture]
    [Category("Framework")]
    public class TestData
    {
        private class TestTableConfig : BaseConfig
        {
            /// <summary>
            /// 整数值
            /// </summary>
            public int IntVal { get; private set; }

            /// <summary>
            /// 字符串值
            /// </summary>
            public string StrVal { get; private set; } = string.Empty;

            public int SquaredIntVal { get; private set; }

            public override void InitWithData(IConfigData data)
            {
                if (Id != 0)
                {
                    Logger.LogError(LogModule.Data, "已经初始化过了！");
                    return;
                }
                Id = data.GetInt("Id");
                IntVal = data.GetInt("IntVal");
                StrVal = data.GetString("StrVal");
            }

            public override void CustomProcess(IConfigData data)
            {
                base.CustomProcess(data);
                SquaredIntVal = IntVal * IntVal;
            }
        }

        public void RunConfigTests()
        {
            ConfigManager.CreateSingleton();
            try
            {
                ConfigManager.Instance.RegisterConfigType<TestTableConfig>("TestTable");
                ConfigManager.Instance.FinishInit();
                var table = ConfigManager.Instance.GetAllConfig<TestTableConfig>();
                Assert.IsNotNull(table, "读取数据失败");
                Assert.AreEqual(3, table.Count, "数据行数不正确，期望=3，实际=" + table.Count);
                if (!table.TryGetValue(1, out var first))
                {
                    Assert.Fail("找不到id=1的记录");
                }
                Assert.AreEqual(1, first.Id);
                Assert.AreEqual(123, first.IntVal);
                Assert.AreEqual("a123", first.StrVal);
                Assert.AreEqual(123 * 123, first.SquaredIntVal);
                if (!table.TryGetValue(2, out var second))
                {
                    Assert.Fail("找不到id=2的记录");
                }
                Assert.AreEqual(2, second.Id);
                Assert.AreEqual(456, second.IntVal);
                Assert.AreEqual("b456", second.StrVal);
                Assert.AreEqual(456 * 456, second.SquaredIntVal);
                if (!table.TryGetValue(3, out var third))
                {
                    Assert.Fail("找不到id=3的记录");
                }
                Assert.AreEqual(3, third.Id);
                Assert.AreEqual(0, third.IntVal);
                Assert.AreEqual(string.Empty, third.StrVal);
            }
            finally
            {
                ConfigManager.DestroySingleton();
            }
        }

        [SetUp]
        public void SetUp()
        {
            ConfigManager.IsTesting = true;
        }

        [TearDown]
        public void TearDown()
        {
            ConfigManager.IsTesting = false;
            ConfigManager.ForceUseBinary = false;
        }

        [Test]
        [Description("二进制数据源")]
        public void BinaryDataSource()
        {
            ConfigManager.ForceUseBinary = true;
            RunConfigTests();
        }

        [Test]
        [Description("Excel数据源")]
        public void ExcelDataSource()
        {
            ConfigManager.ForceUseBinary = false;
            RunConfigTests();
        }
    }
}
