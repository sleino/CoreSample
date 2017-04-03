#if DEBUG
using SmSimple.Core.Util;
using System;
using System.Globalization;
using System.Text;
using Xunit;

namespace SmComm.Core.Parser.ModuleTest
{

    public class TFCeiloParser 
    {

		[Fact]
		public void CeiloParserTest1()
		{
            MeasMsg measMsg ;
			// SmsAwsPreParser parser = new SmsAwsPreParser();
            var parser = new CeiloMsg61Parser();
			var s = String.Empty;
            var ok = parser.Parse(s, out measMsg);
			Assert.True(!ok);

            const char SOH = '\x01';
            const char STX = '\x02';
            const char ETX = '\x03';

            const char LF = '\n';
            const char CR = '\r';
            
            var line1 = SOH + "CT02060" +STX + CR + LF;
            var line2 = "30 01230 12340 23450 FEDCBA98" + CR + LF;
            var line3 = ETX.ToString() + CR + LF;

            var sb = new StringBuilder();
            sb.Append(line1);
            sb.Append(line2);
            sb.Append(line3);

            
			ok = parser.Parse(sb.ToString(), out measMsg);
			Assert.True(ok);

            string measValue;
            Assert.True(measMsg.GetMeasObsValueByName("CB1", out measValue ));
            Assert.True(measValue == "01230");

            Assert.True(measMsg.GetMeasObsValueByName("CB2", out measValue));
            Assert.True(measValue == "12340");

            Assert.True(measMsg.GetMeasObsValueByName("CB3", out measValue));
            Assert.True(measValue == "23450");

            Assert.True(measMsg.GetMeasObsValueByName("VV", out measValue));
            Assert.True(measValue == "///");

            Assert.True(measMsg.GetMeasObsValueByName("CL_STATUSCODE", out measValue));
            Assert.True(measValue == "FEDCBA98");

            Assert.False(measMsg.GetMeasObsValueByName("CL1", out measValue));


            line1 = SOH + "CT02060" + STX + CR + LF;
            line2 = "30 01230 12340 23450 FEDCBA98" + CR + LF;
            line3 = "  3 055  5 170  0 ///  0 ///  0 ///" + CR + LF;
            string line4 = ETX.ToString() + CR + LF;

            sb = new StringBuilder();
            sb.Append(line1);
            sb.Append(line2);
            sb.Append(line3);
            sb.Append(line4);

            ok = parser.Parse(sb.ToString(), out measMsg);
            Assert.True(ok);
            Assert.True(measMsg.Station == "CT0");

            Assert.True(measMsg.GetMeasObsValueByName("CB1", out measValue));
            Assert.True(measValue == "01230");

            Assert.True(measMsg.GetMeasObsValueByName("CB2", out measValue));
            Assert.True(measValue == "12340");

            Assert.True(measMsg.GetMeasObsValueByName("CB3", out measValue));
            Assert.True(measValue == "23450");

            Assert.True(measMsg.GetMeasObsValueByName("VV", out measValue));
            Assert.True(measValue == "///");

            Assert.True(measMsg.GetMeasObsValueByName("CL_STATUSCODE", out measValue));
            Assert.True(measValue == "FEDCBA98");

            Assert.True(measMsg.GetMeasObsValueByName("CL1", out measValue));
            Assert.True(measValue == "055");

            Assert.True(measMsg.GetMeasObsValueByName("SC1", out measValue));
            Assert.True(measValue == "3");

            Assert.True(measMsg.GetMeasObsValueByName("CL2", out measValue));
            Assert.True(measValue == "170");

            Assert.True(measMsg.GetMeasObsValueByName("SC2", out measValue));
            Assert.True(measValue == "5");

            Assert.True(measMsg.GetMeasObsValueByName("CL3", out measValue));
            Assert.True(measValue == "///");

            Assert.True(measMsg.GetMeasObsValueByName("SC3", out measValue));
            Assert.True(measValue == "0");
		} // 

        [Fact]
        public void CeiloParserTest2()
        {
            MeasMsg measMsg;
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var parser = new CeiloMsg61Parser();
            var s = String.Empty;
            var ok = parser.Parse(s, out measMsg);
            Assert.True(!ok);

            const char SOH = '\x01';
            const char STX = '\x02';
            const char ETX = '\x03';

            const char LF = '\n';
            const char CR = '\r';

            var line1 = SOH + "CT02061" + STX + CR + LF;
            var line2 = "30 00200 01000 05000 00000000" + CR + LF;
            var line3 = " -1 ///  0 ///  0 ///  0 ///  0 ///" + CR + LF;
            var line4 = ETX.ToString() + CR + LF;

            var sb = new StringBuilder();
            sb.Append(line1);
            sb.Append(line2);
            sb.Append(line3);
            sb.Append(line4);
            string measValue;
            ok = parser.Parse(sb.ToString(), out measMsg);
            Assert.True(ok);

            Assert.True(measMsg.GetMeasObsValueByName("CB1", out measValue));
            Assert.True(measValue == "00200");

            Assert.True(measMsg.GetMeasObsValueByName("CB2", out measValue));
            Assert.True(measValue == "01000");

            Assert.True(measMsg.GetMeasObsValueByName("CB3", out measValue));
            Assert.True(measValue == "05000");

            Assert.True(measMsg.GetMeasObsValueByName("VV", out measValue));
            Assert.True(measValue == "///");

            Assert.True(measMsg.GetMeasObsValueByName("CL_STATUSCODE", out measValue));
            Assert.True(measValue == "00000000");

            //Assert.False(measMsg.GetMeasObsValueByName("CL1", out measValue));
            //Assert.False(measMsg.GetMeasObsValueByName("SC1", out measValue));
            //Assert.False(measMsg.GetMeasObsValueByName("CL2", out measValue));
            //Assert.False(measMsg.GetMeasObsValueByName("SC2", out measValue));
            //Assert.False(measMsg.GetMeasObsValueByName("CL3", out measValue));
            //Assert.False(measMsg.GetMeasObsValueByName("SC3", out measValue));

        } // 



        [Fact]
        public void CeiloParserTest3Korea()
        {
            MeasMsg measMsg;
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var parser = new CeiloMsg61Parser();
            var s = String.Empty;
            var ok = parser.Parse(s, out measMsg);
            Assert.True(!ok);

            const char SOH = '\x01';
            const char STX = '\x02';
            const char ETX = '\x03';

            const char LF = '\n';
            const char CR = '\r';

            var line1 = SOH + "CT02061" + STX + CR + LF;
            var line2 = "30 00200 01000 05000 00000000" + CR + LF;
            var line3 = "  1    27  3    40  0 ///  0 ///  0 ///" + CR + LF;
            var line4 = ETX.ToString() + CR + LF;

            var sb = new StringBuilder();
            sb.Append(line1);
            sb.Append(line2);
            sb.Append(line3);
            sb.Append(line4);
            string measValue;
            ok = parser.Parse(sb.ToString(), out measMsg);
            Assert.True(ok);

            Assert.True(measMsg.GetMeasObsValueByName("CB1", out measValue));
            Assert.True(measValue == "00200");

            Assert.True(measMsg.GetMeasObsValueByName("CB2", out measValue));
            Assert.True(measValue == "01000");

            Assert.True(measMsg.GetMeasObsValueByName("CB3", out measValue));
            Assert.True(measValue == "05000");

            Assert.True(measMsg.GetMeasObsValueByName("VV", out measValue));
            Assert.True(measValue == "///");

            Assert.True(measMsg.GetMeasObsValueByName("CL_STATUSCODE", out measValue));
            Assert.True(measValue == "00000000");

            Assert.True(measMsg.GetMeasObsValueByName("CL1", out measValue));
            Assert.True(measValue == "27");

            Assert.True(measMsg.GetMeasObsValueByName("SC1", out measValue));
            Assert.True(measValue == "1");

            Assert.True(measMsg.GetMeasObsValueByName("CL2", out measValue));
            Assert.True(measValue == "40");

            Assert.True(measMsg.GetMeasObsValueByName("SC2", out measValue));
            Assert.True(measValue == "3");

            Assert.True(measMsg.GetMeasObsValueByName("CL3", out measValue));
            Assert.True(measValue == "///");

            Assert.True(measMsg.GetMeasObsValueByName("SC3", out measValue));
            Assert.True(measValue == "0");

        } // 



        [Fact]
        public void CeiloParserTest4()
        {
            MeasMsg measMsg;
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var parser = new CeiloMsg61Parser();
            var s = String.Empty;
            var ok = parser.Parse(s, out measMsg);
            Assert.True(!ok);

            const char SOH = '\x01';
            const char STX = '\x02';
            const char ETX = '\x03';

            const char LF = '\n';
            const char CR = '\r';

            var line1 = SOH + "CT02060" + STX + CR + LF;
            var line2 = "30 01230 12340 23450 FEDCBA98" + CR + LF;
            var line3 = ETX.ToString() + CR + LF;

            var sb = new StringBuilder();
            sb.Append(line1);
            sb.Append(line2);
            sb.Append(line3);


            ok = parser.Parse(sb.ToString(), out measMsg);
            Assert.True(ok);

            string measValue;
            Assert.True(measMsg.GetMeasObsValueByName("CB1", out measValue));
            Assert.True(measValue == "01230");

            Assert.True(measMsg.GetMeasObsValueByName("CB2", out measValue));
            Assert.True(measValue == "12340");

            Assert.True(measMsg.GetMeasObsValueByName("CB3", out measValue));
            Assert.True(measValue == "23450");

            Assert.True(measMsg.GetMeasObsValueByName("VV", out measValue));
            Assert.True(measValue == "///");

            Assert.True(measMsg.GetMeasObsValueByName("CL_STATUSCODE", out measValue));
            Assert.True(measValue == "FEDCBA98");

            Assert.False(measMsg.GetMeasObsValueByName("CL1", out measValue));


            line1 = SOH + "CT02060" + STX + CR + LF;
            line2 = "30 01230 12340 23450 FEDCBA98" + CR + LF;
            line3 = "  7 125  5 170  0 ///  0 ///  0 ///" + CR + LF;
            string line4 = ETX.ToString() + CR + LF;


            sb = new StringBuilder();
            sb.Append(line1);
            sb.Append(line2);
            sb.Append(line3);
            sb.Append(line4);

            ok = parser.Parse(sb.ToString(), out measMsg);
            Assert.True(ok);
            Assert.True(measMsg.Station == "CT0");

            Assert.True(measMsg.GetMeasObsValueByName("CB1", out measValue));
            Assert.True(measValue == "01230");

            Assert.True(measMsg.GetMeasObsValueByName("CB2", out measValue));
            Assert.True(measValue == "12340");

            Assert.True(measMsg.GetMeasObsValueByName("CB3", out measValue));
            Assert.True(measValue == "23450");

            Assert.True(measMsg.GetMeasObsValueByName("VV", out measValue));
            Assert.True(measValue == "///");

            Assert.True(measMsg.GetMeasObsValueByName("CL_STATUSCODE", out measValue));
            Assert.True(measValue == "FEDCBA98");

            Assert.True(measMsg.GetMeasObsValueByName("CL1", out measValue));
            Assert.True(measValue == "125");

         


            Assert.True(measMsg.GetMeasObsValueByName("SC1", out measValue));
            Assert.True(measValue == "7");

          

            Assert.True(measMsg.GetMeasObsValueByName("CL2", out measValue));
            Assert.True(measValue == "170");

            Assert.True(measMsg.GetMeasObsValueByName("SC2", out measValue));
            Assert.True(measValue == "5");

            Assert.True(measMsg.GetMeasObsValueByName("CL3", out measValue));
            Assert.True(measValue == "///");

            Assert.True(measMsg.GetMeasObsValueByName("SC3", out measValue));
            Assert.True(measValue == "0");
        } // 
// 
        /*
        private static void ValidateResults(IEnumerable<string> results)
		{
			foreach (var s1 in results)
			{
				Assert.True(s1.Length > 0);
				Assert.True(s1.StartsWith("MEAS\t"), "Does not start with MEAS\t");
				Assert.True(s1.EndsWith(Environment.NewLine), "Does not end properly");
				var m = new MeasMsg();
				Assert.True(m.Initialise(s1), "Could not initialise measmsg");
			}
			
		}
        */

	} // class
}
#endif