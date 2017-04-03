using SmSimple.Core;
using System;
using System.Diagnostics;
using System.Globalization;
using Xunit;

namespace SmSimple.Core.ModuleTest
{
    public class TFDateTimeEx
    {
        [Fact]
        public void TimeTest()
        {
            DateTime dt1 = DateTimeEx.Now;
            DateTime dt2 = DateTimeEx.Now;
            DateTime dt3 = DateTimeEx.Now;
            Assert.True(dt1.CompareTo(dt2) <= 1, "dt1.CompareTo(dt2)<= 1");
            Assert.True(dt2.CompareTo(dt3) <= 1, "dt2.CompareTo(d3)<= 1");

            TimeSpan oneHour = new TimeSpan(0, 1, 0, 0);

            DateTimeEx.Now = dt1.Subtract(oneHour);
            DateTime dt4 = DateTimeEx.Now;
            Assert.True(dt4.CompareTo(dt1) <= 1, "dt4.CompareTo(dt1)<= 1");
            Assert.True(DateTimeEx.Now.CompareTo(dt4) == 0, "DateTimeEx.Now.CompareTo(dt4) == 0");
            System.Threading.Thread.Sleep(10);
            Assert.True(DateTimeEx.Now.CompareTo(dt4) == 0, "DateTimeEx.Now.CompareTo(dt4) == 0");
        }

        [Fact]
        public void GetCompilationDateTest()
        {
            string compilationDateString = DateTimeEx.GetCompilationDateAsString;
           Assert.True(compilationDateString.Length > 0);
            Console.WriteLine("Compilation Time is " + compilationDateString);

            var compilationDate = DateTime.ParseExact(compilationDateString, "yyyy-MM-dd HH:mm:ss",   CultureInfo.InvariantCulture);

            // take into account summer time savings & time zone
           Assert.True(compilationDate < DateTime.Now.AddDays(1));
        }


        [Theory]
        [InlineData(1,2)]
        [InlineData(1, 5)]
        public void GetTimeTest(int minutes, int seconds)
        {
           DateTimeEx.Now = new DateTime(1999, 12, 3, 2, minutes, seconds);
           Assert.True(DateTimeEx.Now.Minute == 1);
        }


        [Fact]
        public void GetNextFullTimeTest()
        {
            DateTimeEx.Now = new DateTime(2009, 1, 13, 9, 34, 14);
           Assert.True(DateTimeEx.GetNextFullTime(30) == new DateTime(2009, 1, 13, 9, 34, 30));
           Assert.True(DateTimeEx.GetNextFullTime(60) == new DateTime(2009, 1, 13, 9, 35, 0));
           Assert.True(DateTimeEx.GetNextFullTime(300) == new DateTime(2009, 1, 13, 9, 35, 0));
           Assert.True(DateTimeEx.GetNextFullTime(600) == new DateTime(2009, 1, 13, 9, 40, 0));
        }

        [Fact]
        public void IsScheduledTimeTest()
        {
            int timerIntevalMilliseconds = 150;
            int intervalSeconds = 300;
            TimeSpan syncTime = new TimeSpan(0, 0, 0);

            TestWithAllCurTimeValues(intervalSeconds, timerIntevalMilliseconds, syncTime);

            timerIntevalMilliseconds = 100;
            intervalSeconds = 250;
            TestWithAllCurTimeValues(intervalSeconds, timerIntevalMilliseconds, syncTime);

            timerIntevalMilliseconds = 100;
            intervalSeconds = 250;
            syncTime = new TimeSpan(0, 0, 30);
            TestWithAllCurTimeValues(intervalSeconds, timerIntevalMilliseconds, syncTime);

            timerIntevalMilliseconds = 100;
            intervalSeconds = 1200;
            syncTime = new TimeSpan(0, 0, 0);
            TestWithAllCurTimeValues(intervalSeconds, timerIntevalMilliseconds, syncTime);
        }


        private void TestWithAllCurTimeValues(int intervalSeconds, int timerIntevalMilliseconds, TimeSpan syncTime)
        {
            WriteToConsole("Start stopwatch for IsScheduledTime.");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int iTestCount = 0;

            var curTime = new DateTime(2002, 1, 1, 0, 0, 0, 0);
            curTime = curTime.AddSeconds(syncTime.TotalSeconds);
           Assert.True(
                DateTimeEx.IsScheduledTime(curTime, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());

            curTime = new DateTime(2002, 1, 1, 0, 0, 0, timerIntevalMilliseconds / 2);
            curTime = curTime.AddSeconds(syncTime.TotalSeconds);
           Assert.True(
                DateTimeEx.IsScheduledTime(curTime, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());

            curTime = new DateTime(2002, 1, 1, 0, 0, 0, timerIntevalMilliseconds);
            curTime = curTime.AddSeconds(syncTime.TotalSeconds);
            Assert.False(
                DateTimeEx.IsScheduledTime(curTime, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());

            curTime = new DateTime(2002, 1, 1, 0, 0, 0, timerIntevalMilliseconds + 1);
            curTime = curTime.AddSeconds(syncTime.TotalSeconds);
            Assert.False(
                DateTimeEx.IsScheduledTime(curTime, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());

            curTime = new DateTime(2002, 1, 1, 0, 0, 0, 0);
            curTime = curTime.AddSeconds(intervalSeconds);
            curTime = curTime.AddSeconds(syncTime.TotalSeconds);
           Assert.True(
                DateTimeEx.IsScheduledTime(curTime, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());

            curTime = new DateTime(2002, 1, 1, 0, 0, 0, timerIntevalMilliseconds / 2);
            curTime = curTime.AddSeconds(intervalSeconds);
            curTime = curTime.AddSeconds(syncTime.TotalSeconds);
           Assert.True(
                DateTimeEx.IsScheduledTime(curTime, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());

            curTime = new DateTime(2002, 1, 1, 0, 0, 0, timerIntevalMilliseconds);
            curTime = curTime.AddSeconds(intervalSeconds);
            curTime = curTime.AddSeconds(syncTime.TotalSeconds);
            Assert.False(
                DateTimeEx.IsScheduledTime(curTime, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());

            curTime = new DateTime(2002, 1, 1, 0, 0, 0, timerIntevalMilliseconds / 2);
            curTime = curTime.AddSeconds(intervalSeconds / 2);
            Assert.False(
                DateTimeEx.IsScheduledTime(curTime, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());


            stopWatch.Stop();
            // 
            var s = string.Format("Calls made {0}. Duration per call (ticks): {1} ", iTestCount,
                (stopWatch.ElapsedTicks / iTestCount));
            WriteToConsole(s);
        }

        private static void WriteToConsole(string s)
        {
            if (s == null)
                s = "Received a null string.";
            DateTime dtNow = DateTimeEx.Now;
            s = DateTimeEx.ToStdDateTimeFormat(dtNow) + "." +
                dtNow.Millisecond.ToString(CultureInfo.InvariantCulture).PadRight(3, '0') + "\t" + s;
            Console.WriteLine(s, CultureInfo.InvariantCulture);
        }

        [Fact]
        public void IsWithinSampleInterval()
        {
            DateTime curTime = new DateTime(2015, 3, 19, 0, 0, 0);
            int samplePeriodLen = 10 * 60;
            int sampleStartInterval = 3600; ;
            int sampleSyncTime = 0;

            var ok = DateTimeEx.IsWithinSamplingPeriod(curTime, samplePeriodLen, sampleStartInterval, sampleSyncTime);
           Assert.True(ok);

            curTime = new DateTime(2015, 3, 19, 0, 9, 59);
            ok = DateTimeEx.IsWithinSamplingPeriod(curTime, samplePeriodLen, sampleStartInterval, sampleSyncTime);
           Assert.True(ok);

            curTime = new DateTime(2015, 3, 19, 0, 10, 00);
            ok = DateTimeEx.IsWithinSamplingPeriod(curTime, samplePeriodLen, sampleStartInterval, sampleSyncTime);
           Assert.True(!ok);

            curTime = new DateTime(2015, 3, 19, 0, 19, 00);
            ok = DateTimeEx.IsWithinSamplingPeriod(curTime, samplePeriodLen, sampleStartInterval, sampleSyncTime);
           Assert.True(!ok);

            curTime = new DateTime(2015, 3, 19, 0, 19, 00);
            ok = DateTimeEx.IsWithinSamplingPeriod(curTime, samplePeriodLen, sampleStartInterval, sampleSyncTime);
           Assert.True(!ok);

            curTime = new DateTime(2015, 3, 19, 1, 2, 00);
            ok = DateTimeEx.IsWithinSamplingPeriod(curTime, samplePeriodLen, sampleStartInterval, sampleSyncTime);
           Assert.True(ok);

            curTime = new DateTime(2015, 3, 19, 1, 3, 30);
            ok = DateTimeEx.IsWithinSamplingPeriod(curTime, samplePeriodLen, sampleStartInterval, sampleSyncTime);
           Assert.True(ok);
        }

        [Fact]
        public void DotFormatTest()
        {
            string[] testDataFail = { "", "dfga", "20120100" };
            string[] testDataOk =
            {
                "20120101", "20120101.1", "20120101.12", "20120101.112", "20120101.0909",
                "20120101.231100", "20120101.231155", "20120122.0652"
            };

            DateTime date;
           Assert.True(DateTimeEx.TryParseDotFormat("20120122.0652", out date));


            foreach (var data in testDataFail)
                Assert.False(DateTimeEx.TryParseDotFormat(data, out date));

            foreach (var data in testDataOk)
               Assert.True(DateTimeEx.TryParseDotFormat(data, out date));
        }

        /*
        [Fact]
        public void PcClockChangeTest()
        {
            // this event should take place every 5 minutes, synchronized at full hour
            int timerIntevalMilliseconds = 150;
            int intervalSeconds = 300;
            TimeSpan syncTime = new TimeSpan(0, 0, 0);


            // change PC clock by one hour forward

            int iTestCount = 0;

            var testTime = new DateTime(2014, 4, 1, 0, 0, 0, 0);
            testTime = testTime.AddSeconds(syncTime.TotalSeconds);


           Assert.True(
                DateTimeEx.IsScheduledTime(testTime, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());

            testTime = testTime.AddHours(1);
           Assert.True(
                DateTimeEx.IsScheduledTime(testTime, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());

            DateTimeEx.StopTesting();
            DateTime curTime = DateTimeEx.Now;

            SystemTimeManager.SetSystemUtcTime1(testTime);
           Assert.True(
                DateTimeEx.IsScheduledTime(DateTimeEx.Now, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());


            SystemTimeManager.SetSystemUtcTime1(testTime.AddMilliseconds(timerIntevalMilliseconds + 50));
            Assert.False(
                DateTimeEx.IsScheduledTime(DateTimeEx.Now, timerIntevalMilliseconds, intervalSeconds, syncTime),
                (iTestCount++).ToString());

            SystemTimeManager.SetSystemUtcTime1(curTime);
        }

        [Fact]
        public void DaylightSavingsTest()
        {
            var testTime = new DateTime(2015, 4, 8, 0, 0, 0, 0);
           Assert.True(TimeZoneInfo.Local.IsDaylightSavingTime(testTime));

            var testTime2 = new DateTime(2015, 1, 1, 0, 0, 0, 0);
            Assert.False(TimeZoneInfo.Local.IsDaylightSavingTime(testTime2));

            TimeSpan ts = TimeZoneInfo.Local.BaseUtcOffset;
           Assert.True(ts.TotalHours > 0);


            Debug.WriteLine("TimeZoneInfo.Local.DisplayName:" + TimeZoneInfo.Local.DisplayName);
            Debug.WriteLine("TimeZoneInfo.Local.StandardName:" + TimeZoneInfo.Local.StandardName);
            Debug.WriteLine("TimeZone.CurrentTimeZone.StandardName:" + TimeZone.CurrentTimeZone.StandardName);
            Debug.WriteLine("TimeZone.CurrentTimeZone.DaylightName:" + TimeZone.CurrentTimeZone.DaylightName);
        }
        */
        [Fact]
        public void UnixTimeTest()
        {
            DateTime now = new DateTime(2016, 2, 18, 10, 15, 0);
            double tmp = DateTimeEx.DateTimeToUnixTimeStamp(now);

            DateTime now2 = DateTimeEx.UnixTimeStampToDateTime(tmp);

           Assert.True(now2.Subtract(now).TotalSeconds < 1);
        }
    }
}
