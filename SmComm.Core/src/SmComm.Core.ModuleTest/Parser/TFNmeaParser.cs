#if DEBUG
using SmComm.Core.Parser.NMEA;
using SmSimple.Core;
using SmSimple.Core.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace SmComm.Core.ModuleTest.Parser
{

    public class TFNmeaParser 
    {

        // $WIXDR,A,204,D,0,A,219,D,1,A,248,D,2,S,0.9,M,0,S,1.2,M,1,S,1.6,M,2*5A
        // $WIXDR,C,22.7,C,0,H,41.5,P,0,P,1017.7,H,0*79
        // $WIXDR,V,0.02,M,0,Z,10,s,0,R,0.7,M,0,V,0.0,M,1,Z,0,s,1,R,0.0,M,1,R,25.6,M,2,R,0.0,M,3*65

        [Fact]
        public void WXTMsgTest()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            string message = "$WIXDR,A,204,D,0,A,219,D,1,A,248,D,2,S,0.9,M,0,S,1.2,M,1,S,1.6,M,2*5A" + Environment.NewLine;
            var nmeaSentence = NMEAParser.Parse(message);
            stopWatch.Stop();
            Debug.WriteLine("Time 1:" + stopWatch.ElapsedTicks);

            var tmp = (NMEAStandardSentence)nmeaSentence;
            var talkerID = tmp.TalkerID;
            var sentenceIdentifiers = tmp.SentenceID;


            stopWatch.Start();
            message = "$WIXDR,C,22.7,C,0,H,41.5,P,0,P,1017.7,H,0*79" + Environment.NewLine;
            nmeaSentence = NMEAParser.Parse(message);
            stopWatch.Stop();
            Debug.WriteLine("Time 2:" + stopWatch.ElapsedTicks);

            stopWatch.Start();
            message = "$WIXDR,A,204,D,0,A,219,D,1,A,248,D,2,S,0.9,M,0,S,1.2,M,1,S,1.6,M,2*5A" + Environment.NewLine;
            nmeaSentence = NMEAParser.Parse(message);
            stopWatch.Stop();
            Debug.WriteLine("Time 3:" + stopWatch.ElapsedTicks);

        }

        [Fact]
        public void MNEAFormatTest()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            string message = "$PNORI,3,WAV6103,3,20,0.51,2.00,0*16" + Environment.NewLine;
            var nmeaSentence = NMEAParser.Parse(message);
            stopWatch.Stop();
            Debug.WriteLine("Time 1:" + stopWatch.ElapsedTicks);

            var tmp = (NMEAProprietarySentence)nmeaSentence;
            var talkerID = tmp.Manufacturer;
            var sentenceIdentifiers = tmp.SentenceIDString;


            stopWatch.Start();
            message = "$PNORS,073010,050000,00,B0,13.4,1520.6,114.9,-0.5,1.6,22.314,18.92,1039,0*0B" + Environment.NewLine;
            nmeaSentence = NMEAParser.Parse(message);
            stopWatch.Stop();
            Debug.WriteLine("Time 2:" + stopWatch.ElapsedTicks);

            stopWatch.Start();
            message = "$PNORC,073010,050000,1,0.10,-0.11,-0.01,0.15,137.2,C,88,83,87,,,*37" + Environment.NewLine;
            nmeaSentence = NMEAParser.Parse(message);
            stopWatch.Stop();
            Debug.WriteLine("Time 3:" + stopWatch.ElapsedTicks);

            stopWatch.Start();
            message = "$PNORC,073010,050000,2,0.15,-0.16,-0.02,0.22,138.1,C,76,71,74,,,*3D" + Environment.NewLine;
            nmeaSentence = NMEAParser.Parse(message);
            stopWatch.Stop();
            Debug.WriteLine("Time 3:" + stopWatch.ElapsedTicks);

            stopWatch.Start();
            message = "$PNORW,073010,051001,3,4,0.55,0.51,0.63,0.82,2.76,3.33,2.97,55.06,78.91,337.62,0.48,22.35,0,1,0.27,129.11,0000*4E" + Environment.NewLine;
            nmeaSentence = NMEAParser.Parse(message);
            stopWatch.Stop();
            Debug.WriteLine("Time 3:" + stopWatch.ElapsedTicks);
           
            stopWatch.Start();
            message = "$PNORE,073010,051001,3,0.02,0.01,98,0.000,0.000,0.000,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.002,0.002,0.002,0.002,0.002,0.002,0.003,0.003,0.004,0.006,0.010,0.023,0.049,0.091,0.162,0.176,0.213,0.179,0.160,0.104,0.097,0.072,0.056,0.036,0.032,0.034,0.040,0.032,0.028,0.021,0.017,0.017,0.014,0.012,0.009,0.011,0.010,0.012,0.009,0.010,0.009,0.007,0.006,0.007,0.007,0.008,0.007,0.006,0.005,0.004,0.004,0.003,0.003,0.003,0.003,0.002,0.003,0.003,0.002,0.002,0.002,0.002,0.002,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001,0.001*7E" + Environment.NewLine;
            nmeaSentence = NMEAParser.Parse(message);
            stopWatch.Stop();
            Debug.WriteLine("Time 3:" + stopWatch.ElapsedTicks);
           
           stopWatch.Start();
           message = "$PNORB,073010,051001,3,4,0.02,0.20,0.06,7.06,5.00,262.39,80.27,23.39,0000*62" + Environment.NewLine;
           nmeaSentence = NMEAParser.Parse(message);
           stopWatch.Stop();
           Debug.WriteLine("Time 3:" + stopWatch.ElapsedTicks);

           stopWatch.Start();
           message = "$PNORB,073010,051001,3,4,0.21,0.49,0.52,3.06,3.33,57.06,78.91,24.66,0000*50" + Environment.NewLine;
           nmeaSentence = NMEAParser.Parse(message);
           stopWatch.Stop();
           Debug.WriteLine("Time 3:" + stopWatch.ElapsedTicks);

           stopWatch.Start();
           message = "$PNORF,A1,073010,051001,3,0.02,0.01,48,-0.0216,-0.0521,-0.0563,-0.0565,-0.0287,-0.0149,-0.0099,-0.0531,-0.0445,-0.0431,-0.0204,-0.0141,0.0697,0.0833,0.0540,0.0190,-0.0195,-0.0367,-0.0025,-0.0143,0.0318,-0.0307,-0.0051,0.0041,0.0440,0.0114,0.0831,0.0527,0.0284,0.0104,0.0040,0.0030,0.0049,-0.0005,0.0001,-0.0007,0.0018,0.0011,0.0012,0.0008,0.0029,0.0035,0.0021,-9.0000,-9.0000,-9.0000,-9.0000,-9.0000*0B" + Environment.NewLine;
           nmeaSentence = NMEAParser.Parse(message);
           stopWatch.Stop();
           Debug.WriteLine("Time 3:" + stopWatch.ElapsedTicks);
          
        }


        [Fact]
        public void TestAwacNMEAParser_PNORI()
        {
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var awacNmeaParser = new AwacNMEAParser();
            awacNmeaParser.CellDirectionVariables = new List<string>() { "CELL_DIR_1", "CELL_DIR_2", "CELL_DIR_3", "CELL_DIR_4" };
            awacNmeaParser.CellSpeedVariables = new List<string>() { "CELL_SPEED_1", "CELL_SPEED_2", "CELL_SPEED_3", "CELL_SPEED_4" };
            awacNmeaParser.CellDepthVariables = new List<string>() { "CELL_DEPTH_1", "CELL_DEPTH_2", "CELL_DEPTH_3", "CELL_DEPTH_4" };

            var s = String.Empty;
            var measMsg = awacNmeaParser.Parse(s);
            Assert.True(measMsg == null);

            var testTime = new DateTime(2010, 1, 1, 13, 0, 0);
            DateTimeEx.Now = testTime;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            const string msg = "$PNORI,3,WAV6103,3,20,0.51,2.00,0*16";
            measMsg = awacNmeaParser.Parse(msg);
            stopWatch.Stop();
            Debug.WriteLine("Time :" + stopWatch.ElapsedTicks);

            int count = Enum.GetValues(typeof(AwacNMEAParser.VarNameInfoMessage)).Length + awacNmeaParser.CellDepthVariables.Count; 
            Assert.True(measMsg.Count == count, "result count");
            Assert.True(measMsg.Time == DateTimeEx.Now);


            Assert.True(measMsg.Station == String.Empty);
            var meas2 = new Meas();
            Assert.True(measMsg.GetMeasByName(AwacNMEAParser.VarNameInfoMessage.AWAC_INFO_INSTRUMENT.ToString(), ref meas2));
            Assert.True(meas2.ObsTime == testTime);
           

            string measValue;

            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarNameInfoMessage.AWAC_INFO_INSTRUMENT.ToString(), out measValue)));
            Assert.True(measValue == "AWAC");

            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarNameInfoMessage.AWAC_INFO_HEADID.ToString(), out measValue)));
            Assert.True(measValue == "WAV6103");

            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarNameInfoMessage.AWAC_INFO_NUMBEAMS.ToString(), out measValue)));
            Assert.True(measValue == "3");

            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarNameInfoMessage.AWAC_INFO_NUMCELLS.ToString(), out measValue)));
            Assert.True(measValue == "20");

            string blanking;
            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarNameInfoMessage.AWAC_INFO_BLANKING.ToString(), out blanking)));
            Assert.True(blanking == "0.51");
            double blankingD = Double.Parse(blanking);

            string cellSize;
            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarNameInfoMessage.AWAC_INFO_CELLSIZE.ToString(), out cellSize)));
            Assert.True(cellSize == "2");
            double cellSizeD = Double.Parse(cellSize);

            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarNameInfoMessage.AWAC_INFO_COORD.ToString(), out measValue)));
            Assert.True(measValue == "ENU");

            for (int i = 0; i < awacNmeaParser.CellDepthVariables.Count; i++)
            {
                 double dMeas ;
                Assert.True(measMsg.GetNumericDoubleObsValueByName(awacNmeaParser.CellDepthVariables[i], out dMeas));
                Assert.True(meas2.ObsTime == testTime);
                double tmp = (blankingD + (i+1) * cellSizeD);
                Assert.True(Math.Abs(dMeas - tmp)<0.1);
            }
        } // Test


        [Fact]
        public void TestAwacNMEAParser_PNORS()
        {
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var awacNmeaParser = new AwacNMEAParser();          

            var s = String.Empty;
            var measMsg = awacNmeaParser.Parse(s);
            Assert.True(measMsg == null);



            var testTime = new DateTime(2010, 1, 1, 13, 0, 0);
            DateTimeEx.Now = testTime;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            const string msg = "$PNORS,073010,050000,00,B0,13.4,1520.6,114.9,-0.5,1.6,22.314,18.92,1039,0*0B";
            measMsg = awacNmeaParser.Parse(msg);
            stopWatch.Stop();
            Debug.WriteLine("Time :" + stopWatch.ElapsedTicks);

            int count = Enum.GetValues(typeof(AwacNMEAParser.VarNameSensorMessage)).Length;
            Assert.True(measMsg.Count == count, "result count");
            Assert.True(measMsg.Time == DateTimeEx.Now);


            Assert.True(measMsg.Station == String.Empty);
            var meas2 = new Meas();
            Assert.True(measMsg.GetMeasByName(AwacNMEAParser.VarNameSensorMessage.AWAC_SENSOR_DATE.ToString(), ref meas2));
            Assert.True(meas2.ObsTime == testTime);


            string measValue;

            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarNameSensorMessage.AWAC_SENSOR_ANALOG2.ToString(), out measValue)));
            Assert.True(measValue == "0");

            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarNameSensorMessage.AWAC_SENSOR_ANALOG1.ToString(), out measValue)));
            Assert.True(measValue == "1039");



        } // Test


        [Fact]
        public void TestAwacNMEAParser_PNORC()
        {
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var awacNmeaParser = new AwacNMEAParser();

            awacNmeaParser.CellDirectionVariables = new List<string>() { "CELL_DIR_1", "CELL_DIR_2", "CELL_DIR_3", "CELL_DIR_4" };
            awacNmeaParser.CellSpeedVariables = new List<string>() { "CELL_SPEED_1", "CELL_SPEED_2", "CELL_SPEED_3", "CELL_SPEED_4" };
            awacNmeaParser.CellDepthVariables = new List<string>() { "CELL_DEPTH_1", "CELL_DEPTH_2", "CELL_DEPTH_3", "CELL_DEPTH_4" };

            var s = String.Empty;
            var measMsg = awacNmeaParser.Parse(s);
            Assert.True(measMsg == null);

            var testTime = new DateTime(2010, 1, 1, 13, 0, 0);
            DateTimeEx.Now = testTime;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            const string msg = "$PNORC,073010,050000,1,0.10,-0.11,-0.01,0.15,137.2,C,88,83,87,,,*37";
            measMsg = awacNmeaParser.Parse(msg);
            stopWatch.Stop();
            Debug.WriteLine("Time :" + stopWatch.ElapsedTicks);

            int count = 2;
            Assert.True(measMsg.Count == count, "result count");
            Assert.True(measMsg.Time == DateTimeEx.Now);

            // data from cell 1 => awacNmeaParser.CellSpeedVariables[0] / .CellDirectionVariables[0] used 
            Assert.True(measMsg.Station == String.Empty);
            var meas2 = new Meas();
            Assert.True(measMsg.GetMeasByName(awacNmeaParser.CellSpeedVariables[0], ref meas2));
            Assert.True(meas2.ObsTime == testTime);
            Assert.True(meas2.ObsValue == "0.15");

            Assert.True(measMsg.GetMeasByName(awacNmeaParser.CellDirectionVariables[0], ref meas2));
            Assert.True(meas2.ObsTime == testTime);
            Assert.True(meas2.ObsValue == "137.2");

            //string measValue;

            //Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_AMPLITUDE_3.ToString(), out measValue)));
            //Assert.True(measValue == "87");

            //Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_AMPLITUDE_2.ToString(), out measValue)));
            //Assert.True(measValue == "83");

        } // Test


        [Fact]
        public void TestAwacNMEAParser_PNORW()
        {
            // SmsAwsPreParser parser = new SmsAwsPreParser();
            var awacNmeaParser = new AwacNMEAParser();

            var s = String.Empty;
            var measMsg = awacNmeaParser.Parse(s);
            Assert.True(measMsg == null);

            var testTime = new DateTime(2010, 1, 1, 13, 0, 0);
            DateTimeEx.Now = testTime;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            const string msg = "$PNORW,073010,051001,3,4,0.55,0.51,0.63,0.82,2.76,3.33,2.97,55.06,78.91,337.62,0.48,22.35,0,1,0.27,129.11,0000*4E";
            measMsg = awacNmeaParser.Parse(msg);
            stopWatch.Stop();
            Debug.WriteLine("Time :" + stopWatch.ElapsedTicks);

            int count = Enum.GetValues(typeof(AwacNMEAParser.VarWaveMessage)).Length;
            Assert.True(measMsg.Count == count, "result count");
            Assert.True(measMsg.Time == DateTimeEx.Now);


            Assert.True(measMsg.Station == String.Empty);
            var meas2 = new Meas();
            Assert.True(measMsg.GetMeasByName(AwacNMEAParser.VarWaveMessage.AWAC_WAVE_NEAR_SURFACE_DIR.ToString(), ref meas2));
            Assert.True(meas2.ObsTime == testTime);


            string measValue;

            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarWaveMessage.AWAC_WAVE_NEAR_SURFACE_DIR.ToString(), out measValue)));
            Assert.True(measValue == "129.11");

            Assert.True((measMsg.GetMeasObsValueByName(AwacNMEAParser.VarWaveMessage.AWAC_WAVE_NEAR_SURFACE_SPEED.ToString(), out measValue)));
            Assert.True(measValue == "0.27");



        } // Test
    }
}

#endif
