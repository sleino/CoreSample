using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using SmSimple.Core;

namespace SmComm.Core.IO
{

    internal sealed class OutQueueManager<T>
    {
        private const int capacity = 2000;
        private readonly TimeMeasurer capacityInfoTimeMeasurer;

        private readonly Queue<T> outQueue = new Queue<T>();
        private readonly Lock outQueueLock = new Lock();
        private readonly Stopwatch outQueueStopWatch = new Stopwatch();

        internal OutQueueManager()
        {
            capacityInfoTimeMeasurer = new TimeMeasurer("QueueManager", new TimeSpan(0, 1, 0, 0));
        }
        internal OutQueueManager(string source)
        {
            capacityInfoTimeMeasurer = new TimeMeasurer(source, new TimeSpan(0, 1, 0, 0));
        }

        internal bool OutQueueLocked { get; set; }


        // wait until OutQueueManager.OutQueue becomes free. 
        // used only when sending messages 
        internal void WaitForQueueToOpen()
        {
            const int deltaWaitInMs = 0; // 25;

            outQueueStopWatch.Reset();
            outQueueStopWatch.Start();
            const int maxElapsedMilliseconds = 30 * 1000;
 
            while (OutQueueLocked && outQueueStopWatch.ElapsedMilliseconds < maxElapsedMilliseconds)
            {
                Thread.Sleep(deltaWaitInMs);
            }
        }

        internal Queue<T> OutQueue
        {
            get { return outQueue; }
        }

        internal Lock OutQueueLock
        {
            get { return outQueueLock; }
        }

        internal void Clear()
        {
            outQueue.Clear();
        }

        internal int Count
        {
            get {return outQueue.Count;}
        }


        internal void Enqueue(T msg)
        {
            if (msg == null)
                return;

            try
            {
                if (OutQueueLocked)
                    WaitForQueueToOpen();

                if (OutQueueLocked)
                {
                    ExceptionRecorder.RecordException("Output queue lock could not be opened 2. Count = " + OutQueue.Count + ". Failing to send command: " + msg);
                    return;
                }

                OutQueueLocked = true;
                lock (OutQueueLock)
                {
                    OutQueue.Enqueue(msg);
                    if (outQueue.Count > capacity)
                    {
                        InformCapacityLimitReached();
                        outQueue.Dequeue();
                    }
                }
            }
            catch (Exception ex)
            {
                var s = string.Format(CultureInfo.InvariantCulture, "{0}{1}", "Exception while sending message: ", ex.Message);
                ExceptionRecorder.RecordException(s);
            }
            finally
            {
                OutQueueLocked = false;
            }
        }

        private void InformCapacityLimitReached()
        {
            if (capacityInfoTimeMeasurer.IsTime())
            {
                string msg = "Output queue capacity (" + capacity + ") limit reached.";
                if (!string.IsNullOrEmpty(capacityInfoTimeMeasurer.Source))
                    msg = msg + capacityInfoTimeMeasurer.Source;
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, msg);
            }
        }

        internal T Dequeue()
        {
            return OutQueue.Dequeue();
        }
    }
}
