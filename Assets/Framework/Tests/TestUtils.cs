/**
 * == Inspoy Technology ==
 * Assembly: Framework.Tests
 * FileName: TestUtils.cs
 * Created on 2018/11/19 by inspoy
 * All rights reserved.
 */

using System.Text;
using Instech.Framework;
using NUnit.Framework;

namespace FrameworkTests
{
    [TestFixture]
    [Category("Framework")]
    public class TestUtils
    {
        [Test]
        [Description("GZ压缩解压测试")]
        public void Gzip()
        {
            var utf8 = new UTF8Encoding(false, true);
            const string rawString = "HelloWorld你好世界";
            var rawBytes = utf8.GetBytes(rawString);
            var zippedBytes = GzipHelper.Compress(rawBytes);
            var unzippedBytes = GzipHelper.UnCompressToBytes(zippedBytes);
            var result = utf8.GetString(unzippedBytes);
            Assert.AreEqual(rawString, result);
        }

        [Test]
        [Description("MD5散列")]
        public void Md5()
        {
            const string raw = "Hello";
            var ans = Utility.GetMd5(raw);
            Assert.AreEqual("8b1a9953c4611296a827abf8c47804d7", ans);
        }

        [Test]
        [Description("字符串分割为整数数组")]
        public void SplitToInt()
        {
            const string raw1 = "1,2,3";
            var ans1 = Utility.SplitToInt(raw1);
            Assert.AreEqual(new[] { 1, 2, 3 }, ans1);
            const string raw2 = "4;3;2";
            var ans2 = Utility.SplitToInt(raw2, ';');
            Assert.AreEqual(new[] { 4, 3, 2 }, ans2);
            const string raw3 = "abc,def";
            var ans3 = Utility.SplitToInt(raw3);
            Assert.AreEqual(new[] { 0, 0 }, ans3);
            var raw4 = string.Empty;
            var ans4 = Utility.SplitToInt(raw4);
            Assert.AreEqual(new int[] { }, ans4);
        }

        [Test]
        [Description("字符串分割为字符串数组")]
        public void SplitToString()
        {
            const string raw1 = "asd,qwe";
            var ans1 = Utility.SplitToString(raw1);
            Assert.AreEqual(new[] { "asd", "qwe" }, ans1);
            const string raw2 = "zxc;jkl";
            var ans2 = Utility.SplitToString(raw2, ';');
            Assert.AreEqual(new[] { "zxc", "jkl" }, ans2);
            var raw3 = string.Empty;
            var ans3 = Utility.SplitToString(raw3);
            Assert.AreEqual(new string[] { }, ans3);
        }

        [Test]
        [Description("四舍五入")]
        public void Round()
        {
            Assert.AreEqual(1, Utility.Round(1.499f));
            Assert.AreEqual(2, Utility.Round(1.500f));
            Assert.AreEqual(2, Utility.Round(1.999f));
            Assert.AreEqual(2, Utility.Round(2.000f));
            Assert.AreEqual(3, Utility.Round(2.5f));
        }

        [Test]
        [Description("浮点数相等")]
        public void FloatEqual()
        {
            Assert.IsFalse(Utility.FloatEqual(0f, float.Epsilon));
            Assert.IsTrue(Utility.FloatEqual(0d, double.Epsilon));
        }
    }
}
