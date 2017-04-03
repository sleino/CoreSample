using System;
using System.Diagnostics;
using System.Globalization;
using SmSimple.Core.Util;
using SmSimple.Core.Attributes;

namespace SmSimple.Core
{
    [ImmutableAttribute]
    public struct DateTimeEx
    {
        private static bool testOngoing;
        private static DateTime swTestTime; // used ONLY for internal system testing
        private static DateTime swTestTimeUtc; // used ONLY for internal system testing

        // use this to set an artificial, non-changing Now value
        // remember to use DateTimeEx.Now when getting current time in code
        public static DateTime Now
        {
            get
            {
                if (!testOngoing)
                    return DateTime.Now;
                return swTestTime;
            }

            set
            {
                testOngoing = true;
                swTestTime = value;
            }
        }

        public static DateTime NowInUtc 
        {
            get
            {
                if (!testOngoing)
                    return DateTime.UtcNow;
                if (swTestTimeUtc>DateTime.MinValue)
                   return swTestTimeUtc;
                return DateTimeEx.Now.AddHours(-2);
            }
            set
            {
                testOngoing = true;
                swTestTimeUtc = value;
            }

        }

        public static DateTime NowNoMs
        {
            get { return ForceMillisecondsToZero(Now); }
        }

        public static void StopTesting()
        {
            testOngoing = false;
        }

        public static bool IsTesting
        {
            get { return testOngoing; }
        }

        public static DateTime TodayEx
        {
            get
            {
                if (!testOngoing)
                    return DateTime.Today;
                return swTestTime;
            }
        }


        public static DateTime Today
        {
            get
            {
                if (!testOngoing)
                    return DateTime.Today;
                return swTestTime.Date;
            }
            //set
            //{
            //    testOngoing = true;
            //    swTestTime = value;
            //}
        }

        public static bool TryFormat(DateTime dateTime, string format, out string formattedDate)
        {
            formattedDate = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(format))
                    return false;

                char[] validChars = { 'y', 'M', 'd', 'h', 'H', 'm', 's', 't' };
                int index = format.IndexOfAny(validChars);
                if (index == -1)
                    return false;

                formattedDate = dateTime.ToString(format);
                return true;
            }
            catch (System.FormatException)
            {
               
                return false;
            }
        }


        public const string FormatStringDefault = "yyyy-MM-dd HH:mm:ss";
        public const string FormatStringFile = "yyyyMMdd_HHmmss";
        public const string FormatStringFileMs = "yyyyMMdd_HHmmss_fff";
        public const string FormatStringWithMs = "yyyy-MM-dd HH:mm:ss.fff";
        public const string FormatStringDayFile = "yyyyMMdd";
        
        public const string FormatStringDate = "yyyy-MM-dd";
        public const string FormatStringTime = "HH:mm:ss";
        public const string FormatStringTimeWithMs = "HH:mm:ss.fff";

        public static string NowToString
        {
            get { return Now.ToString(FormatStringDefault, CultureInfo.InvariantCulture); }
        }

        public static string NowToStringWithMs
        {
            get { return Now.ToString(FormatStringWithMs, CultureInfo.InvariantCulture); }
        }

        public static string NowToTimeStringWithMs
        {
            get { return Now.ToString(FormatStringTimeWithMs, CultureInfo.InvariantCulture); }
        }

        public static string NowToFileString
        {
            get { return Now.ToString(FormatStringFile, CultureInfo.InvariantCulture); }
        }

        public static string NowToFileMsString
        {
            get { return Now.ToString(FormatStringFileMs, CultureInfo.InvariantCulture); }
        }

        public static string NowToDayFileString
        {
            get { return Now.ToString(FormatStringDayFile, CultureInfo.InvariantCulture); }
        }

        public static string NowToUniversalTimeString
        {
            get { return DateTimeEx.Now.ToUniversalTime().ToString(FormatStringTime, CultureInfo.InvariantCulture); }
        }

        public static string NowToCurrentCultureDateTimeString
        {
            get {
                string format1 = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern;
                string tmp  = Now.ToString(format1);
                string format2 = CultureInfo.CurrentUICulture.DateTimeFormat.LongTimePattern;
                string tmp2 = Now.ToString(format2);
                return tmp + " " + tmp2;
            }
        }



        public static string ToStdDateTimeFormat(DateTime dt)
        {
            return dt.ToString(FormatStringDefault, CultureInfo.InvariantCulture);
        }

        public static string ToStdTimeFormat(DateTime dt)
        {
            return dt.ToString(FormatStringDefault, CultureInfo.InvariantCulture);
        }

        public static string ToStdTimeOnlyFormat(DateTime dt)
        {
            return dt.ToString(FormatStringTime, CultureInfo.InvariantCulture);
        }


        public static string ToStdDateFormat(DateTime dt)
        {
            return dt.ToString(FormatStringDate, CultureInfo.InvariantCulture);
        }

        public static string ToStdDateTimeFormatWithMs(DateTime dt) {
            return dt.ToString(FormatStringTimeWithMs, CultureInfo.InvariantCulture);
        }

        public static TimeSpan ToSyncTimeSpan(DateTime dateTime)
        { 
            TimeSpan ts = TimeSpan.FromHours(dateTime.Hour).
                Add(TimeSpan.FromMinutes(dateTime.Minute).
                Add(TimeSpan.FromSeconds(dateTime.Second)));
            return ts;
        }

        // return true if data has format FormatStringDefault ("yyyy-MM-dd HH:mm:ss")
        public static bool TryParseStandardFormat(string data, out DateTime dateTime)
        {
            if (data != null)
                return DateTime.TryParseExact(data, FormatStringDefault, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces, out dateTime);
            dateTime = Now;
            return false;
        }


        // return true if data has format yyyymmdd.HHMMss
        public static bool TryParseDotFormat(string data, out DateTime dateTime)
        {
            dateTime = DateTime.MinValue;
            if (string.IsNullOrEmpty(data))
                return false;

            double dateValue;
            if (!double.TryParse(data, out dateValue))
                return false;

            int dateIntValue = Convert.ToInt32(dateValue);
            string integerPart = dateIntValue.ToString();
            double timeValue = Math.Round((dateValue - dateIntValue), 6);
            string decimalPart = timeValue.ToString();

            if (integerPart.Length != 8)
                return false;

            if (
                !DateTime.TryParseExact(integerPart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out dateTime))
                return false;

            int len = decimalPart.Length;
            if (len == 0)
                return true;

            // remove "0." from the start
            if (len > 1)
            {
                Debug.Assert(decimalPart.StartsWith("0"), "Decimal part starts with " + decimalPart);
                if (len < 3)
                    return false;
                decimalPart = decimalPart.Substring(2);
                len = decimalPart.Length;
            }
            int hours;
            if (!int.TryParse(decimalPart.Substring(0, Math.Min(2, len)), out hours))
                return false;

            dateTime = dateTime.AddHours(hours);
            if (len < 3)
                return true;

            int minutes;
            if (!int.TryParse(decimalPart.Substring(2, Math.Min(2, len - 2)), out minutes))
                return false;
            dateTime = dateTime.AddMinutes(minutes);
            if (len < 5)
                return true;

            int seconds;
            if (!int.TryParse(decimalPart.Substring(4, Math.Min(2, len - 4)), out seconds))
                return false;
            dateTime = dateTime.AddSeconds(seconds);
            return true;
        }

        public static double ToDouble(DateTime dateTime)
        {
            double result = dateTime.DayOfYear;
            result += ((double)(dateTime.Hour)/ 24) ;
            result += ((double)(dateTime.Minute) / 3600);
            result += ((double)(dateTime.Second) / 86400);
            return result;
        }

        public static string ToFileStringFormat(DateTime dt)
        {
            return dt.ToString(FormatStringFile, CultureInfo.InvariantCulture);
        }

        public static string FormatInvariantCulture(DateTime dateTime, string format)
        {
            return dateTime.ToString(format, CultureInfo.InvariantCulture);
        }

        //----------
        public override string ToString()
        {
            return string.Concat(testOngoing.ToString(), NowToString, swTestTime.ToString());
        }

        //----

        public static DateTime ForceMillisecondsToZero(DateTime dt)
        {
            DateTime result = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            return result;
        }

        public static DateTime ForceSecondsToZero(DateTime dt)
        {
            DateTime result = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
            return result;
        }

        // return the closest future datetime with seconds from last midnight divisible by the given argument
        // e.g. if called at 00:10:11 with seconds == 30, returns 00:10:30
        public static DateTime GetNextFullTime(int seconds)
        {
            if (seconds < 1)
                return DateTimeEx.Now;

            DateTime now = DateTimeEx.Now;
            int secondsFromLastMidnight = now.Hour*3600 + now.Minute*60 + now.Second + seconds;
            double tmp = secondsFromLastMidnight/seconds;
            int nextFullTimeSecs = ((int) tmp)*seconds;
            DateTime result = now.Date + TimeSpan.FromSeconds(nextFullTimeSecs);
            return result;
        }


        // Assumes that in AssemblyInfo.cs,
        // the version is specified as 1.0.* or the like,
        // with only 2 numbers specified;
        // the next two are generated from the date.
        // This routine decodes them.
        public static string GetCompilationDateAsString
        {
            get { return GetCompilationDate.ToString(FormatStringDefault, CultureInfo.InvariantCulture); }
        }

        public static DateTime GetCompilationDate
        {
            get
            {
                //System.Version v = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

                //var y = typeof().GetType().
                //var x = typeof(DateTimeEx).AssemblyQualifiedName;

                //DateTime baseDate = new DateTime(2000, 1, 1, 0, 0, 0);
                //DateTime compilationDate = baseDate.AddDays(v.Build).AddSeconds(v.Revision*2);                
                //return compilationDate;
                return new DateTime(2000, 1, 1, 0, 0, 0);
            }
        }

        public static readonly DateTime UnixBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

        public static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
        {
            double tmp;
            if (StringUtil.TryParseDouble(unixTimeStamp, out tmp))
                return UnixTimeStampToDateTime(tmp);

            return UnixBaseTime;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = UnixBaseTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }


        public static double DateTimeToUnixTimeStamp(DateTime dtDateTime)
        {
            double tmp =dtDateTime.Subtract(UnixBaseTime).TotalSeconds;
            return tmp;
        }

        // Following function calculates if a repeating scheduled event should be triggered. 
        // The function has been written so that program does not have to keep track of the previous event time.
        // It allows easy scheduling of events at even intervals, e.g. 00:00:00; 00:05:00; etc.
        // Restrictions apply to all parameters, see asserts & code

        // ScheduledTimePoint STP(i) = syncTime + i * intervalSeconds
        // Parameter i ranges from 0 to maxI, where maxI = syncTime.Seconds - secondsInDay / intervalSeconds
        // IsScheduledTime returns true if 
        //     ScheduledTimePoint(i) <= curTime < ScheduledTimePoint(i) + timerIntevalMilliseconds, for some i.	


        public static bool IsScheduledTime(int timerIntervalMilliseconds, int eventIntervalSeconds, TimeSpan syncTime)
        {
            return IsScheduledTime(DateTimeEx.Now, timerIntervalMilliseconds, eventIntervalSeconds, syncTime);
        }


        public static bool IsScheduledTime(DateTime curTime, int timerIntervalMilliseconds, int eventIntervalSeconds, TimeSpan syncTime)
        {
            const int secondsInDay = 24*3600;
            const int minTimerIntervalMilliSecs = 99;


            Debug.Assert(timerIntervalMilliseconds > 0, "0");
            Debug.Assert(timerIntervalMilliseconds > minTimerIntervalMilliSecs, "1");

            Debug.Assert(eventIntervalSeconds < secondsInDay + 1, "2");
            Debug.Assert(eventIntervalSeconds > 1, "3");
            Debug.Assert(eventIntervalSeconds*1000 > timerIntervalMilliseconds, "4");

            Debug.Assert(syncTime.TotalSeconds < eventIntervalSeconds + 1, "5");
            Debug.Assert(syncTime.TotalSeconds < secondsInDay + 1, "6");

            if (!(timerIntervalMilliseconds > 0)) return false;
            if (!(timerIntervalMilliseconds > minTimerIntervalMilliSecs)) return false;

            if (!(eventIntervalSeconds > 1)) return false;
            if (!(eventIntervalSeconds < secondsInDay + 1)) return false;
            if (!(eventIntervalSeconds*1000 > timerIntervalMilliseconds)) return false;

            if (!(syncTime.TotalSeconds < eventIntervalSeconds)) return false;
            if (!(syncTime.TotalSeconds < secondsInDay + 1)) return false;

            return IsSchedTime(curTime, timerIntervalMilliseconds, eventIntervalSeconds, syncTime);
        }

        private static bool IsSchedTime(DateTime curTime, int timerIntervalMilliseconds, int intervalSeconds, TimeSpan syncTime, bool debug=false)
        {
            DateTime curDate = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0);
            TimeSpan curDaysTime = curTime.Subtract(curDate);
            int previousSyncSeconds = ((int) (curDaysTime.TotalSeconds/intervalSeconds))*intervalSeconds;
            //Debug.Assert(previousSyncSeconds == curDaysTime.TotalSeconds - (curDaysTime.TotalSeconds % intervalSeconds));
            DateTime previousSyncTime = curDate.AddSeconds(previousSyncSeconds).AddSeconds(syncTime.TotalSeconds);
#if DEBUG
            if (debug)
            {
                int nextSyncSeconds1 = previousSyncSeconds + intervalSeconds;
                DateTime nextSyncTime1 = curDate.AddSeconds(nextSyncSeconds1).AddSeconds(syncTime.TotalSeconds);
                Debug.WriteLine(DateTime.Now.TimeOfDay + "\t" + previousSyncTime.TimeOfDay + "\t" + curTime.TimeOfDay + "\t" + nextSyncTime1.TimeOfDay);
            }
#endif
            if ((previousSyncTime <= curTime) && (curTime < previousSyncTime.AddMilliseconds(timerIntervalMilliseconds)))
                return true;

            int nextSyncSeconds = previousSyncSeconds + intervalSeconds;
            DateTime nextSyncTime = curDate.AddSeconds(nextSyncSeconds).AddSeconds(syncTime.TotalSeconds);
            if ((nextSyncTime <= curTime) && (curTime < nextSyncTime.AddMilliseconds(timerIntervalMilliseconds)))
                return true;

            return false;
        }


        public static bool IsWithinSamplingPeriod(DateTime curTime, int sampleLengthSeconds, int sampleIntervalSeconds, int sampleSyncTimeSeconds)
        {
            DateTime curDate = curTime.Date;
            TimeSpan curDaysTime = curTime.Subtract(curDate);
            int previousSampleStartSeconds = ((int)(curDaysTime.TotalSeconds / sampleIntervalSeconds)) * sampleIntervalSeconds;
            //Debug.Assert(previousSyncSeconds == curDaysTime.TotalSeconds - (curDaysTime.TotalSeconds % intervalSeconds));
            DateTime sampleStartTime = curDate.AddSeconds(previousSampleStartSeconds).AddSeconds(sampleSyncTimeSeconds);
  
            int sampleEndSeconds = previousSampleStartSeconds + sampleLengthSeconds;
            DateTime sampleEndTime = curDate.AddSeconds(sampleEndSeconds).AddSeconds(sampleSyncTimeSeconds);

            int periodEndSeconds =  previousSampleStartSeconds + sampleIntervalSeconds;
            DateTime periodEndTime = curDate.AddSeconds(periodEndSeconds).AddSeconds(sampleSyncTimeSeconds);

            //Debug.WriteLine(DateTime.Now.TimeOfDay
            //    + "\tsampleStartTime:" + sampleStartTime.TimeOfDay 
            //    + "\tcurTime:" + curTime.TimeOfDay 
            //    + "\tsampleEndTime:" + sampleEndTime.TimeOfDay
            //    + "\tperiodEndTime:" + periodEndTime.TimeOfDay);


            Debug.Assert(curTime >= sampleStartTime, "curTime>= sampleStartTime");
            Debug.Assert(curTime <= periodEndTime, "curTime <= sampleEndTime");


            return curTime < sampleEndTime;
        }

/*		static public void GetTime() {
			// Call the native GetSystemTime method
			// with the defined structure.
			SYSTEMTIME stime = new SYSTEMTIME();
			GetSystemTime(ref stime);
    
			// Show the current time.           
			MessageBox.Show("Current Time: "  + 
				stime.wHour.ToString() + ":"
				+ stime.wMinute.ToString());
		}

		static public void SetTime() {
			// Call the native GetSystemTime method
			// with the defined structure.
			SYSTEMTIME systime = new SYSTEMTIME();
			GetSystemTime(ref systime);
    
			// Set the system clock ahead one hour.
			systime.wHour = (ushort)(systime.wHour + 1 % 24);
			SetSystemTime(ref systime);
			MessageBox.Show("New time: " + systime.wHour.ToString() + ":"
				+ systime.wMinute.ToString());
		}
*/
    }

    // pair of startTime, endTime
    [ImmutableAttribute]
    public struct Gap
    {
        private readonly DateTime date;
        private readonly DateTime startTime;
        private DateTime endTime;

        public Gap(DateTime day)
        {
            date = day;
            startTime = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);
            endTime = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);
        }

        public Gap(DateTime day, DateTime start, DateTime end)
        {
            date = day;
            startTime = start;
            endTime = end;
        }

        public Gap(DateTime start, DateTime end)
        {
            date = start;
            startTime = start;
            endTime = end;
        }

        public DateTime Date
        {
            get { return date; }
        }

        public DateTime StartTime
        {
            get { return startTime; }
        }

        public DateTime EndTime
        {
            get { return endTime; }
        }

        public TimeSpan Length
        {
            get { return endTime.Subtract(startTime); }
        }

        public bool ValueIsWithin(DateTime data, bool allowEqualToStart = true, bool allowEqualToEnd=true) {
            if (data < StartTime)
                return false;
            if (data > EndTime)
                return false;
            if (!allowEqualToStart && data == StartTime)
                return false;
            if (!allowEqualToEnd && data == EndTime)
                return false;
            return true;
        }

        public override string ToString()
        {
            return DateTimeEx.ToStdDateTimeFormat(startTime) + " - " + DateTimeEx.ToStdDateTimeFormat(endTime);
        }

        public static bool InitialiseFromString(string gapString, out Gap gap)
        {
            gap = new Gap();
            try
            {
                if (gapString == null)
                    return false;
                if (gapString.Length != gap.ToString().Length)
                    return false;

                string start = gapString.Substring(0, DateTimeEx.FormatStringDefault.Length);
                DateTime dateStart;
                if (!DateTimeEx.TryParseStandardFormat(start, out dateStart))
                    return false;

                string end = gapString.Substring(DateTimeEx.FormatStringDefault.Length + 3);
                DateTime dateEnd;
                if (!DateTimeEx.TryParseStandardFormat(end, out dateEnd))
                    return false;
                gap = new Gap(dateStart, dateEnd);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception in Gap::InitialiseFromString ", ex);
                return false;
            }
        }


    }

    [ImmutableAttribute]
    public struct TimeSpanEx
    {
        public static string Format(TimeSpan timeSpan)
        {
            int days = timeSpan.Days;
            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;
            int seconds = timeSpan.Seconds;
            return string.Format("{0}d {1}h {2}min {3}s", days, hours, minutes, seconds);
        }
    }

}