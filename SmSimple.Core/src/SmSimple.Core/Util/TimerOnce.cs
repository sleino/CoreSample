using System;
using System.Threading;

namespace SmSimple.Core.Util
{
    public class TimerOnce : IDisposable
    {
        private System.Threading.Timer timer;
        private int dueTimeMillisecs;

        const int minPeriod = -1;
        const int maxPeriod = 24 * 3600 * 1000;

        public TimerOnce(int dueTimeMs)
        {
            DueTimeMs = dueTimeMs;
            var timerDelegate = new TimerCallback(OnTimerEvent);
            timer = new System.Threading.Timer(timerDelegate, null, DueTimeMs, Timeout.Infinite);
        }

        public TimerOnce(TimeSpan defaultTimeout, TimerCallback timerDelegate)
        {
            DueTimeMs = (int)defaultTimeout.TotalMilliseconds;
            timer = new System.Threading.Timer(timerDelegate, this, DueTimeMs, Timeout.Infinite);
        }

        public TimerOnce(int dueTimeMs, TimerCallback timerDelegate)
        {
            DueTimeMs = dueTimeMs;
            timer = new System.Threading.Timer(timerDelegate, this, DueTimeMs, Timeout.Infinite);
        }

        public void Change(int dueTimeMs)
        {
            if (timer == null || disposed)
                return;
            DueTimeMs = dueTimeMs;
            timer.Change(dueTimeMs, Timeout.Infinite);
        }


        public int DueTimeMs
        {
            get { return this.dueTimeMillisecs; }
            private set
            {
                if (value < minPeriod)
                    value = minPeriod;
                else if (value > maxPeriod)
                    value = maxPeriod;
                this.dueTimeMillisecs = value;
            }
        }


        private void InitDueTime(int dueTimeMs)
        {
            this.dueTimeMillisecs = dueTimeMs;
        }

        public bool WaitIsOver { get; set; }


        private void OnTimerEvent(Object state)
        {
            WaitIsOver = true;
        }


        #region IDisposable
        ~TimerOnce()
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
