using System;

namespace SmSimple.Core
{
    public sealed class DailyTimeMeasurer
    {
        private string sourceName = string.Empty;
        private DateTime lastTriggerTime = DateTime.MinValue;
        private TimeSpan triggerTime;
        private const int marginSeconds = 5;

        public DailyTimeMeasurer(string source, TimeSpan triggerTimeSpan)
        {
            if (source != null)
                sourceName = source;
            triggerTime = triggerTimeSpan;
        }

        public string Source { get { return sourceName; } }

        public bool IsTime(DateTime currentDateTime)
        {
            if (currentDateTime.TimeOfDay.TotalSeconds < triggerTime.TotalSeconds)
                return false; // it is too early for today's event
            if (lastTriggerTime.Date >= currentDateTime.Date)
                return false;  //  today's event has already occurred
            if (currentDateTime.TimeOfDay.TotalSeconds - triggerTime.TotalSeconds > marginSeconds)
                return false;

            lastTriggerTime = currentDateTime.Date;
            return true;
        }

    }
}
