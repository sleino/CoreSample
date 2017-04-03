using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Diagnostics;
using SmValidation.Core;
using System.Globalization;
using System.Threading;

namespace SmValidation.Core.ModuleTest
{
    public class TFConversion
    {
            
        [Fact]
        public void ConversionTests5()
        {
            string[] s1 = { "-99.09", "-100.1", "-1.1", "-1", "-0.2", "0", "0.454", "1", "1.7" };
            Decimal[] s2 = { 0.09M, 0.1M, 0.1M, 0.0M, 0.2M, 0.0M, 0.454M, 0.0M, 0.7M };
            int[] s3 = { -99, -100, -1, -1, 0, 0, 0, 1, 1 };

            for (var i = 0; i < s1.GetUpperBound(0); i++)
            {
                Decimal result = Decimal.MinusOne;
                Assert.True(Conversion.GetDecimalPart(s1[i], ref result));
                Assert.True(result == s2[i]);

                int result2 = 87;
                Assert.True(Conversion.GetIntegerPart(s1[i], ref result2));
                Assert.True(result2 == s3[i]);
            }
        }


        [Fact]
        public void ConversionTests()
        {
            // valid values
            string[] s1 = { "-99.09", "-100.1", "-1", "-0.2", "0", "0.454", "1", "1.3" };
            for (var i = 0; i < s1.GetUpperBound(0); i++)
            {
                double convertedValue = -119;
                Assert.True(Conversion.IsNumericDataValue(s1[i]));
                Assert.True(Conversion.IsNumericDataValue(s1[i], ref convertedValue));

                CultureInfo cuEN = new CultureInfo("en-US");    // us
                string sresult = convertedValue.ToString(cuEN);
                Assert.True(sresult == s1[i]);
            }

            // invalid values
            string[] s2 = { "a", "-1,1", "//", "", "0,454", "@", ".3" };
            for (var i = 0; i < s2.GetUpperBound(0); i++)
            {
                Assert.False(Conversion.IsNumericDataValue(s2[i]));
                double convertedValue = 1.999;
                Assert.False(Conversion.IsNumericDataValue(s2[i], ref convertedValue));
            }

        }

        [Fact]
        public void ConversionTest2()
        {

            DateTime dt = new DateTime(2000, 1, 1, 0, 0, 0, 33);
            DateTime dt00 = new DateTime(2000, 1, 1, 0, 0, 0, 00);
            Assert.True(Conversion.RoundMilliSecondsToZero(dt) == dt00);
        }

        [Fact]
        public void ConversionTest3()
        {
            string rawvalue = "15.21";

            Assert.True(Conversion.RoundNumericalValue(rawvalue, 2) == "15.21");
            Assert.True(Conversion.RoundNumericalValue(rawvalue, 1) == "15.2");
            Assert.True(Conversion.RoundNumericalValue(rawvalue, 0) == "15");

            Assert.True(Conversion.RoundNumericalValue(rawvalue, -1) == "20");
            Assert.True(Conversion.RoundNumericalValue(rawvalue, -2) == "0");
            Assert.True(Conversion.RoundNumericalValue(rawvalue, -3) == "0");

            Assert.True(Conversion.RoundNumericalValue("-1.17", 1) == "-1.2");
            Assert.True(Conversion.RoundNumericalValue("-1.1", 0) == "-1");
            Assert.True(Conversion.RoundNumericalValue("-0.1", 0) == "0");
            Assert.True(Conversion.RoundNumericalValue("TEST", 0) == String.Empty);

        }

        [Fact]
        public void ConversionTests4()
        {
            // valid values
            string[] s1 = { "-99.09", "-100.9", "-1", "-0.2", "0", "0.454", "1", "1.3", "NaN" };
            string[] s3 = { "-99", "-101", "-1", "0", "0", "0", "1", "1" };
            for (var i = 0; i < s1.GetUpperBound(0); i++)
            {
                int convertedValue = -119;
                Assert.True(Conversion.IsNumericDataValue(s1[i]));
                Assert.True(Conversion.IsNumericDataValue(s1[i], ref convertedValue));
                Assert.True((s3[i] == convertedValue.ToString(CultureInfo.InvariantCulture)));
            }

            // invalid values
            string[] s2 = { "a", "-1,1", "//", "", "0,454", "@", ".3" };
            for (var i = 0; i < s2.GetUpperBound(0); i++)
            {
                Assert.False(Conversion.IsNumericDataValue(s2[i]));
                int convertedValue = 23;
                Assert.False(Conversion.IsNumericDataValue(s2[i], ref convertedValue));
            }
        }


        [Fact]
        public void ConversionTests6()
        {
            // valid values
            string[] s1 = { "111.111.111.111", "64.6.6.1" };
            for (var i = 0; i < s1.GetUpperBound(0); i++)
            {
                Assert.True(Conversion.IsValidIpAddress(s1[i]));
            }

            // invalid values
            string[] s2 = { "1121.111.111.111", "-64.6.6.1", "" };
            for (var i = 0; i < s2.GetUpperBound(0); i++)
            {
                Assert.False(Conversion.IsValidIpAddress(s2[i]));
            }
        }

/*
        [Fact]
        public void CultureTest()
        {
            try
            {
             
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                ConversionTests();
                CultureInfo cuEN = new CultureInfo("en-US");    // us
                Thread.CurrentThread.CurrentCulture = cuEN;
                ConversionTests();
                CultureInfo cuFI = new CultureInfo("fi-FI");    // finland
                Thread.CurrentThread.CurrentCulture = cuFI;
                ConversionTests();
                CultureInfo cuGR = new CultureInfo("el-GR");    // greece
                Thread.CurrentThread.CurrentCulture = cuGR;
                ConversionTests();
                CultureInfo cuPTBR = new CultureInfo("pt-BR");  // portuguese-brazil
                Thread.CurrentThread.CurrentCulture = cuPTBR;
                ConversionTests();
                CultureInfo cuPTPT = new CultureInfo("pt-PT");  // portuguese-portugal
                Thread.CurrentThread.CurrentCulture = cuPTPT;
                ConversionTests();
                CultureInfo cuMSMY = new CultureInfo("ms-MY");  // Malay - Malaysia
                Thread.CurrentThread.CurrentCulture = cuMSMY;
                ConversionTests();
                CultureInfo cuESMX = new CultureInfo("es-MX");  //Spanish - Mexico
                Thread.CurrentThread.CurrentCulture = cuESMX;
                ConversionTests();
                CultureInfo cuFR = new CultureInfo("fr-FR");    // france
                Thread.CurrentThread.CurrentCulture = cuFR;
                ConversionTests();
                CultureInfo cuSA = new CultureInfo("ar-SA");    // saudi-arabia
                Thread.CurrentThread.CurrentCulture = cuSA;
                ConversionTests();
                CultureInfo cuTH = new CultureInfo("th-TH");    // thaibuddhist
                Thread.CurrentThread.CurrentCulture = cuTH;
                ConversionTests();

            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        } // 
        */
    }
}
