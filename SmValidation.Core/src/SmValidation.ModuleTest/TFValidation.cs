using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Diagnostics;
using SmValidation.Core;

namespace SmValidation.Core.ModuleTest
{
    public class TFValidation
    {
        [Fact]
        public void TestValidation()
        {
            Assert.True(Validation.IsValidFloat("0"));
            Assert.True(Validation.IsValidFloat("0.0"));
            Assert.True(!Validation.IsValidFloat("0.0F"));
            Assert.True(!Validation.IsValidFloat("-1.0F"));
            Assert.True(Validation.IsValidFloat("-1"));
            Assert.True(Validation.IsValidFloat("-1.9"));
            Assert.True(Validation.IsValidFloat("1.9"));

            Assert.True(!Validation.IsValidFloat(null));
            Assert.True(!Validation.IsValidFloat(""));
            Assert.True(!Validation.IsValidFloat("-1,0"));
            Assert.True(!Validation.IsValidFloat("-1,0F"));

            Assert.True(!Validation.IsValidFloat("1-0"));
            Assert.True(!Validation.IsValidFloat("11-"));
            Assert.True(!Validation.IsValidFloat("11.0-"));
            Assert.True(!Validation.IsValidFloat("11-0."));
            Assert.True(!Validation.IsValidFloat(".110-"));
            Assert.True(!Validation.IsValidFloat(".11-0"));
            Assert.True(!Validation.IsValidFloat("11.-0"));
            Assert.True(!Validation.IsValidFloat("11-.0"));
            Assert.True(!Validation.IsValidFloat(".-110"));
            Assert.True(!Validation.IsValidFloat("-.110"));
            Assert.True(!Validation.IsValidFloat("3434.-"));
            Assert.True(!Validation.IsValidFloat("3434-."));

            Assert.True(!Validation.IsValidFloat("-11.-0"));
            Assert.True(!Validation.IsValidFloat("-11-.0"));
            Assert.True(!Validation.IsValidFloat("-.-110"));
            Assert.True(!Validation.IsValidFloat("--.110"));
            Assert.True(!Validation.IsValidFloat("-3434.-"));
            Assert.True(!Validation.IsValidFloat("-3434-."));


            Assert.True(!Validation.IsValidFloat("0.11.-0"));
            Assert.True(!Validation.IsValidFloat("0-11-.0"));
            Assert.True(!Validation.IsValidFloat("0.-110"));
            Assert.True(!Validation.IsValidFloat("0-.110"));

            Assert.True(Validation.IsInRangeOrSlash("/", "x", 1, 2));
            Assert.True(Validation.IsInRangeOrSlash("1", "x", 1, 2));
            Assert.True(Validation.IsInRangeOrSlash("2", "x", 1, 2));
           Assert.False(Validation.IsInRangeOrSlash("", "x", 1, 2));
           Assert.False(Validation.IsInRangeOrSlash(null, "x", 1, 2));
           Assert.False(Validation.IsInRangeOrSlash("3", "x", 1, 2));
        }

        [Fact]
        public void TestContainsDecimalOrSlashes()
        {
            Assert.True(Validation.ContainsDecimalOrSlashes("///"));
            Assert.True(Validation.ContainsDecimalOrSlashes("/"));
            Assert.True(Validation.ContainsDecimalOrSlashes("1"));
            Assert.True(Validation.ContainsDecimalOrSlashes("-11"));
            Assert.True(Validation.ContainsDecimalOrSlashes("1.0"));
            Assert.True(Validation.ContainsDecimalOrSlashes("-11.0"));
            Assert.True(Validation.ContainsDecimalOrSlashes("0.3"));
            Assert.True(Validation.ContainsDecimalOrSlashes("-0.04"));

            Assert.True(!Validation.ContainsDecimalOrSlashes(".04"));
            Assert.True(!Validation.ContainsDecimalOrSlashes("-.04"));
            Assert.True(!Validation.ContainsDecimalOrSlashes("/1"));
            Assert.True(!Validation.ContainsDecimalOrSlashes("1/"));
            Assert.True(!Validation.ContainsDecimalOrSlashes(null));
            Assert.True(!Validation.ContainsDecimalOrSlashes(""));
            Assert.True(!Validation.ContainsDecimalOrSlashes("h"));
        }
    }
}
