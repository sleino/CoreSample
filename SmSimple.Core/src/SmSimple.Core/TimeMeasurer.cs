using System;
using System.Threading.Tasks;

namespace SmSimple.Core
{
    public sealed class TimeMeasurer
    {
        private string sourceName = string.Empty;
        private DateTime lastCheckTime = DateTime.MinValue;
        private TimeSpan limitTime;
        private TimeSpan currentLimitTime;

        public TimeMeasurer(string source, DateTime initialCheckTime, TimeSpan limit)
        {
            if (source != null)
                sourceName = source;
            limitTime = limit;
            currentLimitTime = limit;
            lastCheckTime = initialCheckTime;
        }

        public TimeMeasurer(string source, TimeSpan limit)
        {
            if (source != null)
                sourceName = source;
            limitTime = limit;
            currentLimitTime = limit;
            lastCheckTime = DateTime.MinValue;
        }

        public bool IsTime()
        {
            var curTime = DateTime.Now;
            TimeSpan difference = curTime.Subtract(lastCheckTime);
            bool result = (difference > limitTime);
            lastCheckTime = curTime;
            return result;
        }

        public void Reset()
        {
            currentLimitTime = limitTime;
        }

        public bool IsTime(TimeSpan timeSinceLastCheck)
        {
            currentLimitTime = currentLimitTime.Subtract(timeSinceLastCheck);
            return (currentLimitTime.Ticks < 0);
        }

        public bool WaitAndCheck(int waitMs)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(waitMs);
            Task.Delay(timeSpan).Wait();
            return IsTime(timeSpan);
        }

        public TimeSpan ElapsedTime
        {
            get { return DateTime.Now.Subtract(lastCheckTime); }
        }

        public string Source { get { return sourceName; } }
    }

}
