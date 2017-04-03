using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;
using System;
using SmSimple.Core.Util;

namespace SmSimple.Core.ModuleTest
{
    public class TFTimerDaily
    {
        [Fact]
        public void TimerDailyTest01()
        {
            Debug.WriteLine(DateTimeEx.NowToStringWithMs + "TimerDailyTest01");
            var startTime = DateTimeEx.Now;

            var duration = TimeSpan.FromSeconds(0);
            int firstRunTimeMs = 250;

            const int periodMs = 300;
            onTimerDailyEventTriggered = 0;
            onTimerDailyEventTriggered++;

            using (TimerDaily timerEx = new TimerDaily(firstRunTimeMs, periodMs, OnTimerDailyEvent))
            {
                Debug.WriteLine(DateTimeEx.NowToStringWithMs + " Timer created :"
                    + DateTimeEx.ToStdDateTimeFormatWithMs(DateTime.Now.AddMilliseconds(timerEx.DueTimeMs)));
                timerEx.KeyData = "XYZ";
                int thirdCall = firstRunTimeMs + 3 * periodMs;
                while (duration.TotalMilliseconds < thirdCall)
                {
                    Thread.Sleep(100);
                    duration = DateTime.Now.Subtract(startTime);
                }
                Assert.True(onTimerDailyEventTriggered > 2);
            }
        }


        private int onTimerDailyEventTriggered;
        private void OnTimerDailyEvent(Object state)
        {
            var timer = (TimerDaily)state;
            onTimerDailyEventTriggered++;
            Debug.WriteLine(DateTimeEx.NowToStringWithMs + " Timer event . Key:" + timer.KeyData);
            timer.UpdateTimerDueDate();
        }
    }
}
