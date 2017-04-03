using System;
using System.Threading;
using System.Text;
using System.Diagnostics;

namespace SmSimple.Core.Util
{
    /// <summary>
    ///  Non-drifting timer which triggers at same the same time points every day. 
    ///  Specify either time of first call after midnight  (firstTime) or first triggering time after constructor (milliseconds)
    /// </summary>
    public class TimerDaily : IDisposable
    {
        private System.Threading.Timer timer;
        private int period;
        //  private readonly TimerCallback timerCallback;


        private TimeSpan firstTime = TimeSpan.FromSeconds(0); // time of first call after midnight      
        private int dueTime;  // first triggering time after constructor (ms)

        const int minPeriod = -1;
        const int maxPeriod = 24 * 3600 * 1000;

        //public TimerDaily(TimeSpan firstTime, TimeSpan period)
        //{
        //    this.PeriodMs = (int)Math.Round(period.TotalMilliseconds, 0);
        //    this.firstTime = firstTime;
        //    double milliSecondsToNextStep = GetMillisecondsToNextFiring;
        //    DueTimeMs = (int)Math.Round(milliSecondsToNextStep, 0);
        // //   timerCallback = null;

        //    timer = new System.Threading.Timer(OnTimerEvent, this, DueTimeMs, Timeout.Infinite);
        //}

        public TimerDaily(TimeSpan firstTime, TimeSpan period, TimerCallback timerDelegate, string keyData = "")
        {
            this.KeyData = keyData;
            this.PeriodMs = (int)Math.Round(period.TotalMilliseconds, 0);
            this.firstTime = firstTime;
            double milliSecondsToNextStep = GetMillisecondsToNextFiring;
            if (milliSecondsToNextStep < double.MaxValue)
                DueTimeMs = (int)Math.Round(milliSecondsToNextStep, 0);
            else
                DueTimeMs = System.Threading.Timeout.Infinite;

            if (DueTimeMs < 0)
            {
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Tried to set timer with negative due time:" + DueTimeMs + " keydata:" + keyData);
                return;
            }
            timer = new System.Threading.Timer(timerDelegate, this, DueTimeMs, Timeout.Infinite);
            //  UpdateTimerDueDate();
        }

        //public TimerDaily(int dueTimeMs, int periodMs)
        //{
        //    PeriodMs = period;
        //    DueTimeMs = dueTimeMs;
        //    var timerDelegate = new TimerCallback(OnTimerEvent);
        //    timer = new System.Threading.Timer(timerDelegate, null, DueTimeMs, Timeout.Infinite);
        //}

        public TimerDaily(int dueTimeMs, int periodMs, TimerCallback timerDelegate)
        {
            PeriodMs = periodMs;
            DueTimeMs = dueTimeMs;

            timer = new System.Threading.Timer(timerDelegate, this, DueTimeMs, Timeout.Infinite);
            // UpdateTimerDueDate();
        }

        public string KeyData { get; set; }


        public int PeriodMs
        {
            get { return this.period; }
            private set
            {
                if (value < minPeriod)
                    value = minPeriod;
                else if (value > maxPeriod)
                    value = maxPeriod;
                this.period = value;
            }
        }

        public int DueTimeMs
        {
            get { return this.dueTime; }
            set
            {
                if (value > System.Threading.Timeout.Infinite)
                {
                    var nextDueDate = DateTimeEx.Now.AddMilliseconds(value);
                    this.firstTime = nextDueDate.TimeOfDay;
                    this.dueTime = value;
                }
                else if (value == -2)
                {
                    this.firstTime = TimeSpan.FromMilliseconds(0);
                    this.dueTime = GetRoundedMsToNextFiring;

                }
                else
                {
                    this.firstTime = TimeSpan.MinValue;
                    this.dueTime = value;
                }
            }
        }


        private bool WaitIsOver { get; set; }

        public bool Change(int dueTimeMs, int periodMs)
        {
            //   Debug.WriteLine(DateTimeEx.NowToStringWithMs + " Timer Change. dueTimeMs:" + dueTimeMs + "\ttimer period:" + periodMs);
            this.DueTimeMs = dueTimeMs;
            this.PeriodMs = periodMs;

            WaitIsOver = false;
            return timer.Change(dueTimeMs, periodMs);
        }

        private void OnTimerEvent(Object state)
        {
            WaitIsOver = true;
        }

        private double GetMillisecondsToNextFiring
        {
            get
            {
                double milliSecondsToNextStep = GetMillisecondsToNext(firstTime);

                return milliSecondsToNextStep;
            }
        }

        private int GetRoundedMsToNextFiring
        {
            get { return (int)Math.Ceiling(GetMillisecondsToNextFiring); }
        }

        public double GetMillisecondsToNext(TimeSpan firstFiringOfDay)
        {
            if (firstFiringOfDay == TimeSpan.MinValue)
                return double.MaxValue;


            var curTime = DateTimeEx.Now;
            // x: how many times event should have triggered
            int x = (int)Math.Ceiling(((curTime.TimeOfDay.Subtract(firstFiringOfDay)).TotalMilliseconds / PeriodMs));
            DateTime expectedTime = DateTimeEx.Today.AddMilliseconds(firstFiringOfDay.TotalMilliseconds).Add(TimeSpan.FromMilliseconds(x * PeriodMs));

            double currentDrift = curTime.Subtract(expectedTime).TotalMilliseconds;
            double milliSecondsToNextStep = Math.Ceiling(PeriodMs - currentDrift);
            while (milliSecondsToNextStep > PeriodMs)
                milliSecondsToNextStep -= PeriodMs;

#if DEBUG
            var sb = new StringBuilder();
            sb.Append("\tExpected:" + expectedTime.ToString(DateTimeEx.FormatStringWithMs));
            sb.Append("\tCurrentDrift (ms):" + currentDrift + "\t milliSecondsToNextStep:" + milliSecondsToNextStep);
            sb.Append("\tcurTime:" + curTime.ToString(DateTimeEx.FormatStringWithMs));
            Debug.WriteLine(curTime.ToString(DateTimeEx.FormatStringWithMs) + sb);

            Debug.Assert(milliSecondsToNextStep <= PeriodMs, "GetMillisecondsToNext " + milliSecondsToNextStep + ":" + PeriodMs);
#endif

            return milliSecondsToNextStep;
        }

        public void UpdateTimerDueDate()
        {
            int milliSecondsToNextStep = GetRoundedMsToNextFiring;
            //   Debug.WriteLine(DateTimeEx.NowToStringWithMs + " UpdateTimerDueDate.milliSecondsToNextStep:" + milliSecondsToNextStep + "\tperiod:" + PeriodMs);
            if (milliSecondsToNextStep < double.MaxValue)
                Change(milliSecondsToNextStep, this.PeriodMs);
        }

        #region IDisposable
        ~TimerDaily()
        {
            Dispose(false);
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (timer != null)
                    timer.Dispose();

            }
            disposed = true;
        }

        #endregion
    }
}
