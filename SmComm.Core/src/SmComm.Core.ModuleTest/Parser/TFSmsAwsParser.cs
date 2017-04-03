using SmSimple.Core.Util;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SmComm.Core.Parser.ModuleTest
{
    public class TFSmsAwsParser 
    {

        [Fact]
        public void SmsAwsParserTest()
        {
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var smsAwsParser = new SmsAwsParser();
            Assert.True(smsAwsParser.indexOfFirstValue == 3);
            var s = string.Empty;
            var results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 0);

            s = "(PR:0.6;TA:1.1)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 0);

            s = "(S:;D:050914;T:170000;PR:0.6;TA:1.1)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 0);

            s = "(S:JABALALKAWR;T:170000;PR:0.6;TA:1.1)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 0);

            s = "(S:JABALALKAWR;D:050914;250000;PR:0.6;TA:1.1)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 0);

            s = "(S:JABALALKAWR;D:050914;T:170000;PR:0.6;TA:1.1)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 2);

            s = "(S:JABALALKAWR;D:050914;T:170000;PR:0.6;TA:1.1;)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 2);

            s = "(S:JABALALKAWR;D:050914;T:170000;PR:0.6;TIMEOFWS:12:00:00;)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 2);

            ValidateResults(results);

            var testTime = new DateTime(2005, 9, 14, 17, 0, 0);
            var msg = smsAwsParser.ParseMessageIntoMeasMsg(s);
            Assert.True(msg.Station == "JABALALKAWR");
            var meas = new Meas();
            Assert.True(msg.GetMeasByName("PR", ref meas));
            //Assert.True(((Meas)(msg.MeasList[0])).ObsTime ==  testTime);
            Assert.True(meas.ObsTime == testTime);
            Assert.True(msg.Count == 2);
        } // ParserTest1


        [Fact]
        public void SmsAwsParserTestWithNoStationOrDateTime()
        {
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var smsAwsParser = new SmsAwsParser()
            {
                indexOfFirstValue = 0
            };
            Assert.True(smsAwsParser.indexOfFirstValue == 0);

            var s = string.Empty;
            var results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 0);

            s = "(PR:0.6;TA:1.1)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 2);

            s = "(S:;D:140901;T:170000;PR:0.6;TA:1.1)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 5);

            s = "(S:JABALALKAWR;T:170000;PR:0.6;TA:1.1)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 4);

            s = "(S:JABALALKAWR;D:140901;250000;PR:0.6;TA:1.1)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 2);

            s = "(S:JABALALKAWR;D:140901;T:170000;PR:0.6;TA:1.1)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 5);

            s = "(S:JABALALKAWR;D:140901;T:170000;PR:0.6;TA:1.1;)";
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 5);

            ValidateResults(results);


            var msg = smsAwsParser.ParseMessageIntoMeasMsg(s);
            Assert.True(msg.Station == "S1");
            var meas = new Meas();
            Assert.True(msg.GetMeasByName("PR", ref meas));
            //Assert.True(((Meas)(msg.MeasList[0])).ObsTime ==  testTime);
            TimeSpan diff = meas.ObsTime.Subtract(DateTime.Now);
            Assert.True(diff.TotalSeconds < 2);
            Assert.True(msg.Count == 5);
        } // ParserTest1


        [Fact]
        public void CodeTest()
        {
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var smsAwsParser = new SmsAwsParser();
            const string s = "(S:JABALALKAWR;D:050914;T:170000;PR:0.6;TA:1.1)";
            var results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 2);

            ValidateResults(results);

            var testTime = new DateTime(2005, 9, 14, 17, 0, 0);
            var msg = smsAwsParser.ParseMessageIntoMeasMsg(s);
            Assert.True(msg.Station == "JABALALKAWR");
            var meas = new Meas();
            Assert.True(msg.GetMeasByName("PR", ref meas));
            //Assert.True(((Meas)(msg.MeasList[0])).ObsTime ==  testTime);
            Assert.True(meas.ObsTime == testTime);
            Assert.True(msg.Count == 2);

            // Test starts here
            var tmp = SmsAwsParser.Code(msg);
            Assert.True(tmp.Length > 0);
            results = smsAwsParser.ParseMessage(tmp);
            Assert.True(results.Count == 2);
            ValidateResults(results);
        } // ParserTest1

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

        [Fact]
        public void CodePansioTest()
        {
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var smsAwsParser = new SmsAwsParser();
            const string s = "(S:PANSIO;D:130109;T:095126;QFEAVG1M:1030.1;TAAVG1M:13.1;TWAVG1M:12.1; VIS:1234;WLAVG1M:57;WD:34;WS:5.1)";
            var results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 7);

            ValidateResults(results);

            var msg = smsAwsParser.ParseMessageIntoMeasMsg(s);
            Assert.True(msg.Station == "PANSIO");


            // Test starts here
            var tmp = SmsAwsParser.Code(msg);
            Assert.True(tmp.Length > 0);
            results = smsAwsParser.ParseMessage(tmp);
            Assert.True(results.Count == 7);
            ValidateResults(results);
        } // ParserTest1

 
        [Fact]
        public void ParseSwitzerland()
        {
            var smsAwsParser = new SmsAwsParser();
            const string msg = "(S:AWS1  ;D:070601;T:105802;TAAVG60S://////;TAMAX1H://////;TAMIN1H://////;TGAVG60S://////;TGMAX1H://////;TGMIN1H://////;RHAVG60S:////;RH_MAX:////;RH_MIN:////;DPAVG60S://////;DP_MAX://////;DP_MIN://////;QFEAVG60S:///////;QFE_MAX:///////;QFE_MIN:///////;QNHAVG60S:///////;QNH_MAX:///////;QNH_MIN:///////;ppp://////;SRAVG60S:0    ;SR_MAX://///;SR_MIN://///;SR_ONOFF://////;WXT_Rc:///////;PRSS1H:///////;PRSS24H:///////;DC:11.917;LATITUDE://////;LONGITUDE://////)";
            var result = smsAwsParser.ParseMessageIntoMeasMsg(msg);
            Assert.True(result.Count == 29);
        }

        [Fact]
        public void ParseLeadingSpaces()
        {
            var smsAwsParser = new SmsAwsParser();
            const string msg = "(S:AWS1  ;D:070601;T:105802;TAAVG60S ://////;TAMAX1H: //////;TAMIN1H://////;TGAVG60S://////;TGMAX1H://////;TGMIN1H://////;RHAVG60S:////;RH_MAX:////;RH_MIN:////;DPAVG60S://////;DP_MAX://////;DP_MIN://////;QFEAVG60S:///////;QFE_MAX:///////;QFE_MIN:///////;QNHAVG60S:///////;QNH_MAX:///////;QNH_MIN:///////;ppp://////;SRAVG60S:0    ;SR_MAX://///;SR_MIN://///;SR_ONOFF://////;WXT_Rc:///////;PRSS1H:///////;PRSS24H:///////;DC:11.917;LATITUDE://////;LONGITUDE://////)";
            var result = smsAwsParser.ParseMessageIntoMeasMsg(msg);
            Assert.True(result.Count == 29);
        }

        [Fact]
        public void ParseYearWith4Digits()
        {
            var smsAwsParser = new SmsAwsParser();
            const string msg = "(S:AWS1  ;D:20070601;T:105802;TAAVG60S ://////;TAMAX1H: //////;TAMIN1H://////;TGAVG60S://////;TGMAX1H://////;TGMIN1H://////;RHAVG60S:////;RH_MAX:////;RH_MIN:////;DPAVG60S://////;DP_MAX://////;DP_MIN://////;QFEAVG60S:///////;QFE_MAX:///////;QFE_MIN:///////;QNHAVG60S:///////;QNH_MAX:///////;QNH_MIN:///////;ppp://////;SRAVG60S:0    ;SR_MAX://///;SR_MIN://///;SR_ONOFF://////;WXT_Rc:///////;PRSS1H:///////;PRSS24H:///////;DC:11.917;LATITUDE://////;LONGITUDE://////)";
            var result = smsAwsParser.ParseMessageIntoMeasMsg(msg);
            Assert.True(result.Count == 29);
        }

        [Fact]
        public void ParseSemicolonInTheEnd()
        {
            var smsAwsParser = new SmsAwsParser();
            string msg = "(S:AWS1  ;D:070601;T:105802;TAAVG60S:12.1;)";
            MeasMsg result = smsAwsParser.ParseMessageIntoMeasMsg(msg);
            Assert.True(result.Count == 1);

            msg = "(S:AWS1  ;D:070601;T:105802;TAAVG60S:12.1;TA2:23.1;)";
            result = smsAwsParser.ParseMessageIntoMeasMsg(msg);
            Assert.True(result.Count == 2);
        }



        [Fact]
        public void DisdroSmsAwsMsg()
        {
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var smsAwsParser = new SmsAwsParser();
            var sb = new StringBuilder();

            var s = string.Empty;
            var results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 0);

            sb.Append("      DCP_MSG   repþÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿ   L  ø  Qô  ‡!  ¼N  ñ{ý   ÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿ     |  æ  P  º  $  ŽûOWZ1   î   L   Z(S:Station3;D:120307;T:125300;PR1H:/;LAT:60.000000;LON:25.000000;QUAL:/;R1MIN:/;Z1MIN:/)");
            sb.Append(@"ûOWZm  [      Z(S:Station3;D:120307;T:125400;PR1H:7;LAT:60.000000;LON:25.000000;QUAL:/;R1MIN:/;Z1MIN:/)");
            sb.Append(@"ûOWZ©  È   î   Z(S:Station3;D:120307;T:125500;PR1H:/;LAT:60.000000;LON:25.000000;QUAL:/;R1MIN:/;Z1MIN:/)");
            sb.Append(@"ûOWZå  5  [   Z(S:Station3;D:120307;T:125600;PR1H:/;LAT:60.000000;LON:25.000000;QUAL:/;R1MIN:/;Z1MIN:/)");
            sb.Append(@"ûOW[   ¢  È   Z(S:Station3;D:120307;T:125700;PR1H:/;LAT:60.000000;LON:25.000000;QUAL:/;R1MIN:/;Z1MIN:/)");
            sb.Append(@"ûOW[\    5   Z(S:Station3;D:120307;T:125800;PR1H:/;LAT:60.000000;LON:25.000000;QUAL:/;R1MIN:/;Z1MIN:/)");
            sb.Append(@"ûOW[™  |  ¢   Z(S:Station3;D:120307;T:125900;PR1H:/;LAT:60.000000;LON:25.000000;QUAL:/;R1MIN:/;Z1MIN:/)");
            sb.Append(@"ûOW[Ô  í     ^(S:Station3;D:120307;T:130000;PR1H:0.000;LAT:60.000000;LON:25.000000;QUAL:/;R1MIN:/;Z1MIN:/)");
            sb.Append(@"ûOW\  ^  |   ^(S:Station3;D:120307;T:130100;PR1H:0.000;LAT:60.000000;LON:25.000000;QUAL:/;R1MIN:/;Z1MIN:/)");
            s = sb.ToString();
            results = smsAwsParser.ParseMessage(s);
            Assert.True(results.Count == 70);


            var testTime = new DateTime(2012, 3, 7, 12, 54, 0);
            var multiMeasMsg = smsAwsParser.ParseMessageIntoMultipleMeasMsg(s);
           
            foreach (var msg in multiMeasMsg.MeasMsgList)
            {
                Assert.True(msg.Station == "Station3");
                var meas = new Meas();
                Assert.True(msg.GetMeasByName("PR1H", ref meas));
                //Assert.True(((Meas)(msg.MeasList[0])).ObsTime ==  testTime);
                // Assert.True(meas.ObsTime == testTime);
                if (meas.ObsTime == testTime)
                {
                    Assert.True(meas.ObsValue == "7");
                    Assert.True(msg.Count == 6);
                }
            }
        } // ParserTest1


        [Fact]
        public void CommonCodeSpace()
        {
            var sb = new StringBuilder();
            sb.Append("(S:MAWS;D:130628;T:080013; RH|AVG|PT1M|||%|:59; RH|MIN|P1D|||%|:50; RH|MAX|P1D|||%|:90;");
            sb.Append("TA|AVG|PT1M|||degC|:23.2; TA|MIN|P1D|||degC|:15.0; TA|MAX|P1D|||degC|:25.0; TD|AVG|PT1M|||degC|:14.7;");
            sb.Append("PA|AVG|PT1M|||degC|:1002.8; QFE|AVG|PT1M|||hPa|:1002.9; QFF|AVG|PT1M|||hPa|:1008.8; PATE|AVG|PT3H|||hPa|:0;");
            sb.Append("PATR|AVG|PT3H|||hPa|:0.1; PR|SUM|PT1M|||mm|:0.5; PR|SUM|PT1H|||mm|:0.5; PR|SUM|P1D|||mm|:2.0; PRF|AVG|PT1M|||mmph|:2.1;");
            sb.Append("SNH|AVG|PT1M|||cm|:0; ETO|SUM|P1D|||mm|:3.547; SDUR|SUM|PT1M|||min|:1; SDUR|SUM|P1D|||min|:339; SR|AVG|PT1M|||Wpm2|:310;");
            sb.Append("VIS|AVG|PT1M|||m|:11409; PW|AVG|PT15M|||WMO-306-4680|:61; STATUS|VALUE||||SCODE|:0; EXTDC|VALUE||||VDC|:12.0)3115D1C3");
            string msg = sb.ToString();


            var smsAwsParser = new SmsAwsParser();
            var result = smsAwsParser.ParseMessageIntoMeasMsg(msg);
            Assert.True(result.Count == 25);

            sb.Clear();
            sb.Append("(S:MAWS;D:130628;T:100013;WS|||||mps|:5.1;WS|MAX|PT2M|||mps|:6.0;WS|MIN|PT2M|||mps|:4.2;");
            sb.Append("WD|||||deg|:67;WD|MAX|PT2M|||deg|:67;WD|MIN|PT2M|||deg|:45;WD|AVG|PT2M|||deg|:51;WS|AVG|PT2M|||mps|:5.2;");
            sb.Append("WS|AVG|PT10M|||mps|:5.1;WS|MAX|PT10M|||mps|:6.0;WS|MIN|PT10M|||mps|:4.2;WD|AVG|PT10M|||deg|:63;");
            sb.Append("WD|MAX|PT10M|||deg|:80;WD|MIN|PT10M|||deg|:45)585AFA6C");
            result = smsAwsParser.ParseMessageIntoMeasMsg(msg);
            Assert.True(result.Count == 25);
        }

        [Fact]
        public void ArgTest()
        {
            var sb = new StringBuilder();
            sb.Append("(S:PTU;D:130827;T:103302;WSAVG2M:2.4;WDAVG2M:325;WSMAX2M:3.4;WDMAX2M:320;WGUSTTIME2M:10:32:25;WDGUST2M:316;WSAVG10M:2.6;WDAVG10M:300;WSMAX10M:3.6;WDMAX10M:313;WGUSTTIME10M:10:25:10;WDGUST10M:287;WSAVG1H:2.3;WDAVG1H:287;WSMAX1H:4.1;WDMAX1H:285;WGUSTTIME1H:10:10:14;WDGUST1H:276;WDMIN2M:290;WDMIN10M:276;WSMIN10M:2.1;WSMIN2M:2.2;EXTDC:13.4;VBATT:13.2)");
            var msg = sb.ToString();

            var smsAwsParser = new SmsAwsParser();
            var result = smsAwsParser.ParseMessageIntoMeasMsg(msg);
            var meas = new Meas();
            var ok = result.GetMeasByName("WGUSTTIME10M", ref meas);
            Assert.True(ok);

            Assert.True(result.Count == 24);
        }


        [Fact]
        public void KotaBharuTest()
        {
            var sb = new StringBuilder();
            sb.Append("(S:Kota Bharu;D;151015;T:000247;WS:   0.9;WD:  205;TAAVG1M:  25.4;RHAVG1M:  97;DPAVG1M:  24.9;PAAVG1M: 1012.4;QFEAVG1M: 1012.6;QFFAVG1M: 1012.6;QNHAVG1M: 1012.6;PTREND3H:    1.2;PTEND3H:     1;WSAVG2M:   0.9;WSMAX2M:   1.1;WSMIN2M:   0.6;WDAVG2M:  205;WDMAX2M:  215;WDMIN2M:  195;WSAVG10M:   0.8;WSMAX10M:   1.4;WSMIN10M:   0.5;WDAVG10M:  201;WDMIN10M:  183;WDMAX10M:  215;WSGUST10M://////;WDGUST10M:  206;BATTERY:  14.0;STATUS:     0;SENSORSTATUS:     0)");
            var msg = sb.ToString();

            var smsAwsParser = new SmsAwsParser();
            var result = smsAwsParser.ParseMessageIntoMeasMsg(msg);
            Assert.True(result == null);
        }

        [Fact]
        public void LatviaTest()
        {
            var sb = new StringBuilder();
            sb.Append(" (S:AWS;D:160114;T:133656;MAN.DW.SQUALL.DIRECTION:;MAN.DW.SPOUT.DIRECTION:;MAN.DW.SPOUT.PRECIPITATION:;MAN.DW.SNOW.CHARACTER:;MAN.DW.SQUALL.START:48960;MAN.DW.ICE.DIAMETER:;MAN.DW.ICE.TYPE:;MAN.DW.HAIL.OCCURRENCE:;MAN.DW.HAIL.MAX.DIAMETER:;MAN.DW.SNOW.EVOLUTION:;MAN.DW.SNOW.OCCURRENCE:;MAN.DW.SNOW.OBSCURATION:;MAN.DW.SPOUT.CHARACTER:;MAN.DW.SQUALL.WEATHER:8;MAN.DW.SQUALL.CHARACTER:;MAN.DW.SPOUT.WEATHER:;MAN.DW.SPOUT.OCCURRENCE:;MAN.DW.MIST.LOW.CLOUDTYPE:;MAN.DW.MIST.CLOUDLAYER:;MAN.DW.MIST.PHENOMENON:;MAN.DW.THUNDER.PRECIPITATION:;MAN.DW.THUNDER.DIRECTION:;MAN.DW.FOG.WITHPRECIPITATION:;MAN.DW.ICE.OCCURRENCE:;MAN.DW.SPOUT.INTENSITY:)਍");
            var msg = sb.ToString();

            var smsAwsParser = new SmsAwsParser();

            var result = smsAwsParser.ParseMessageIntoMeasMsg(msg);
            result.Channel = 0;

            var meas = new Meas();
            var ok = result.GetMeasByName("MAN.DW.SQUALL.DIRECTION", ref meas);
            Assert.True(ok);


        }
        // (S:AWS;D:160114;T:133656;MAN.DW.SQUALL.DIRECTION:;MAN.DW.SPOUT.DIRECTION:;MAN.DW.SPOUT.PRECIPITATION:;MAN.DW.SNOW.CHARACTER:;MAN.DW.SQUALL.START:48960;MAN.DW.ICE.DIAMETER:;MAN.DW.ICE.TYPE:;MAN.DW.HAIL.OCCURRENCE:;MAN.DW.HAIL.MAX.DIAMETER:;MAN.DW.SNOW.EVOLUTION:;MAN.DW.SNOW.OCCURRENCE:;MAN.DW.SNOW.OBSCURATION:;MAN.DW.SPOUT.CHARACTER:;MAN.DW.SQUALL.WEATHER:8;MAN.DW.SQUALL.CHARACTER:;MAN.DW.SPOUT.WEATHER:;MAN.DW.SPOUT.OCCURRENCE:;MAN.DW.MIST.LOW.CLOUDTYPE:;MAN.DW.MIST.CLOUDLAYER:;MAN.DW.MIST.PHENOMENON:;MAN.DW.THUNDER.PRECIPITATION:;MAN.DW.THUNDER.DIRECTION:;MAN.DW.FOG.WITHPRECIPITATION:;MAN.DW.ICE.OCCURRENCE:;MAN.DW.SPOUT.INTENSITY:)਍

    } // class
}
