/**
 * == Inspoy Technology ==
 * Assembly: Framework.Tests
 * FileName: TestLocalization.cs
 * Created on 2019/03/07 by inspoy
 * All rights reserved.
 */

using Instech.Framework;
using NUnit.Framework;

namespace FrameworkTests
{
    [TestFixture]
    [Category("Framework")]
    public class TestLocalization
    {
        [Test]
        [Description("读取文件")]
        public void LoadFile()
        {
            ObjectPoolManager.DestroySingleton();
            LocalizationManager.DestroySingleton();

            ObjectPoolManager.CreateSingleton();
            LocalizationManager.CreateSingleton();
            try
            {
                LocalizationManager.Instance.InitSetting("zh-CN");

                // 正常文本
                var normalString = LocalizationManager.Instance.GetString("test_normal");
                Assert.AreEqual("普通文本", normalString);

                // 不存在的文本
                var defaultString = LocalizationManager.Instance.GetString("NotExistRecord");
                Assert.AreEqual("{L10N:NotExistRecord}", defaultString);

                // 术语
                var termString = LocalizationManager.Instance.GetString("test_with_term1");
                Assert.AreEqual("我是术语", termString);

                // 不存在的术语
                var defaultTermString = LocalizationManager.Instance.GetString("test_with_term2");
                Assert.AreEqual("我是{L10N:不存在的术语}", defaultTermString);

                // 转义测试
                var escapeString = LocalizationManager.Instance.GetString("test_escape");
                Assert.AreEqual("我是#术语#术语##", escapeString);

                // 递归测试
                var recursion1String = LocalizationManager.Instance.GetString("test_recursion");
                Assert.AreEqual("自递归测试自递归测试自递归测试自递归测试自递归测试自递归测试自递归测试自递归测试自递归测试自递归测试自递归测试#test_recursion#", recursion1String);
                var recursion2String = LocalizationManager.Instance.GetString("test_recursion1");
                Assert.AreEqual("递归测试1递归测试2递归测试1递归测试2递归测试1递归测试2递归测试1递归测试2递归测试1递归测试2递归测试1#test_recursion2#", recursion2String);

                // 参数测试
                var paramString1 = LocalizationManager.Instance.GetString("test_parameter", 123, 456);
                Assert.AreEqual("参数测试123,456", paramString1);
                var paramString2 = LocalizationManager.Instance.GetString("test_parameter", 123, 456, 789);
                Assert.AreEqual("参数测试123,456", paramString2);
                var paramString3 = LocalizationManager.Instance.GetString("test_parameter", 123);
                Assert.AreEqual("参数测试{0},{1}", paramString3);
            }
            finally
            {
                LocalizationManager.DestroySingleton();
                ObjectPoolManager.DestroySingleton();
            }
        }
    }

}
