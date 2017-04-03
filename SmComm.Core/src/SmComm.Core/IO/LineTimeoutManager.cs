using SmSimple.Core;
using SmSimple.Core.Util;
using System;
using System.Globalization;
using System.Threading;

namespace SmComm.Core.IO
{
    /// <summary>
    /// Monitor idle time in port.
    /// If LineIdleTimeout time passes without any incoming data, raise an event for timeout.
    /// </summary>
    public sealed class LineTimeoutManager : IDisposable
    {
        const int defaultTimeoutMs = 300 * 1000;
        private TimeSpan lineIdleTimeout = TimeSpan.FromMilliseconds(defaultTimeoutMs);
        private DateTime lastDataReceived = DateTime.MinValue;
        private readonly TimerOnce timer;
       
        private VaiListener Listener { get; set; }

        public LineTimeoutManager(VaiListener listener)
        {
            this.timer = new TimerOnce(defaultTimeoutMs, OnTimerOnceEvent);
            this.Listener = listener;
        }

        /// <summary>
        /// Start waiting for timeout
        /// </summary>
        internal void StartTimer()
        {            
            if (Listener==null || Listener.NamedPort == null)
                return;
           
            LastDataReceived = DateTimeEx.Now;    
            if (LineIdleTimeout.TotalMilliseconds >= 1000)
                this.timer.Change((int)LineIdleTimeout.TotalMilliseconds);
        }
        /// <summary>
        /// Stop waiting for timeout
        /// </summary>
        internal void StopTimer()
        {
            if (timer != null)
                this.timer.Change(Timeout.Infinite);
        }

        private void OnTimerOnceEvent(Object state)
        {
            Listener.TimeoutHasOccurred();
        }

        public TimeSpan LineIdleTimeout
        {
            get { return lineIdleTimeout; }
            set
            {
                StopTimer();
                lineIdleTimeout = value;
                StartTimer();
            }
        }

        public DateTime LastDataReceived
        {
            get { return lastDataReceived; }
            set
            {
                lastDataReceived = value;
                this.timer.Change((int)lineIdleTimeout.TotalMilliseconds);
            }
        }


        public string LineIdleTimeoutToString
        {
            get
            {
                return lineIdleTimeout.TotalHours.ToString("00", CultureInfo.InvariantCulture) + ":" +
                    lineIdleTimeout.Minutes.ToString("00", CultureInfo.InvariantCulture) + ":" +
                    lineIdleTimeout.Seconds.ToString("00", CultureInfo.InvariantCulture);
            }
        }


        #region IDisposable
        ~LineTimeoutManager()
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
                StopTimer();
                if (timer != null)
                    timer.Dispose();
            }
            disposed = true;
        }

        #endregion
    }
}
