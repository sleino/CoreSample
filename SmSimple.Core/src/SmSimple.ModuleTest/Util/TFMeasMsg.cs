using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;
using System;
using SmSimple.Core.Util;

namespace SmSimple.Core.ModuleTest
{
    public class TFMeasMsg
    {
        [Fact]
        public void SubString()
        {
            string str = "mytest";
            Assert.True(str.SafeSubstring(0, 3) == "myt");
            Assert.True(str.SafeSubstring(0, 0) == "");
            Assert.True(str.SafeSubstring(0, 100) == str);

            Assert.True(str.SafeSubstring(3, 3) == "est");
            Assert.True(str.SafeSubstring(3, 0) == "");
            Assert.True(str.SafeSubstring(3, 100) == "est");

            var tmp = str.SafeSubstring(3, -2);
            Assert.True( tmp== "");
        }

        [Fact]
        public void CharCount() {
            string str = "mytest";
            Assert.True(str.CharCount('t') == 2);
            Assert.True(str.CharCount('y') == 1);
            Assert.True(str.CharCount('x') == 0);
        }

        [Fact]
        public void StringUtil()
        {
            string[] s1 = { "abc", "abcd", "abcde", "abcdef" };
            string[] s2 = { "abc", "abc", "Abcde", "abcdEf" };
            int i = SmSimple.Core.Util.StringUtil.Diff("", "");
            Assert.True(i == -1, "1");

            i = SmSimple.Core.Util.StringUtil.Diff("x", "x");
            Assert.True(i == -1, "x");

            i = SmSimple.Core.Util.StringUtil.Diff(null, null);
            Assert.True(i == -1, "x");

            i = SmSimple.Core.Util.StringUtil.Diff("xy", "xy");
            Assert.True(i == -1, "xy");
            // ----------------------------------------------------
            i = SmSimple.Core.Util.StringUtil.Diff(null, "xy");
            Assert.True(i == 0, "1");
            i = SmSimple.Core.Util.StringUtil.Diff("xy", null);
            Assert.True(i == 0, "2");

            i = SmSimple.Core.Util.StringUtil.Diff("", "xy");
            Assert.True(i == 0, "3");
            i = SmSimple.Core.Util.StringUtil.Diff("xy", "");
            Assert.True(i == 0, "4");

            i = SmSimple.Core.Util.StringUtil.Diff("x", "xy");
            Assert.True(i == 1, "5");
            i = SmSimple.Core.Util.StringUtil.Diff("xy", "x");
            Assert.True(i == 1, "6");

            i = SmSimple.Core.Util.StringUtil.Diff("Xy", "xy");
            Assert.True(i == 0, "7");
            i = SmSimple.Core.Util.StringUtil.Diff("xy", "Xy");
            Assert.True(i == 0, "8");
        }
    }
}
