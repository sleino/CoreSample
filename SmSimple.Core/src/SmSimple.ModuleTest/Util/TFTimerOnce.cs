using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;
using System;
using SmSimple.Core.Util;

namespace SmSimple.Core.ModuleTest
{
    public class TFTimerOnce
    {
        private int onTimerEventTriggered;
        private void OnTimerOnceEvent(Object state)
        {
            onTimerEventTriggered++;
            Debug.WriteLine(DateTimeEx.NowToStringWithMs + " Timer event");
        }

        [Fact]
        public void TimerOnceTest01()
        {
            const int milliSecondsToWait = 500;

            Debug.WriteLine(DateTimeEx.NowToStringWithMs + "TimerOnceTest01");
            var startTime = DateTimeEx.Now;

            var duration = TimeSpan.FromSeconds(0);

            using (TimerOnce timerEx = new TimerOnce(milliSecondsToWait))
            {
               // while ((duration.TotalMilliseconds < 2000 + milliSecondsToWait) && !timerEx.WaitIsOver)
                while (!timerEx.WaitIsOver)
                {
                    Thread.Sleep(100);
                    duration = DateTime.Now.Subtract(startTime);
                }
                Assert.True(timerEx.WaitIsOver);
            }
        }


        [Fact]
        public void TimerOnceTest02()
        {
            const int milliSecondsToWait = 500;

            Debug.WriteLine(DateTimeEx.NowToStringWithMs + "TimerOnceTest02");
            var startTime = DateTimeEx.Now;

            var duration = TimeSpan.FromSeconds(0);
            onTimerEventTriggered = 0;

            using (TimerOnce timerEx = new TimerOnce(milliSecondsToWait, OnTimerOnceEvent))
            {
                while (duration.TotalMilliseconds < 1000 + milliSecondsToWait)
                {
                    Thread.Sleep(100);
                    duration = DateTime.Now.Subtract(startTime);
                }
                Assert.True(onTimerEventTriggered > 0);
            }
        }

    }
}
