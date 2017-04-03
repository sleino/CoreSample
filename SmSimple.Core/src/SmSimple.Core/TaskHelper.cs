using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmSimple.Core
{
    public sealed class TaskHelper : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private Action action;
        public bool IsRunning { get; private set; }
        private TimeSpan interval;

        public TaskHelper() { }

        public void StartTask(Action action)
        {
            this.action = action;
            if (IsRunning)
                StopTask();

            Task.Run(() => {
                DoSomeWork(cancellationTokenSource.Token, action);
            },
                cancellationTokenSource.Token
           );

            IsRunning = true;
        }
        /// <summary>
        /// Run action at every interval. 
        /// Duration of action is not taken into account so the exection time will drift.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="interval"></param>
        public void StartRecurringTask(Action action, TimeSpan interval)
        {
            this.action = action;
            this.interval = interval;
            StartRecurringTask();
        }


        private void StartRecurringTask()
        {
            if (IsRunning)
                StopTask();

            Task.Run(() => {
                    DoSomeRecurringWork(cancellationTokenSource.Token, action);
                },
                cancellationTokenSource.Token
           );

            IsRunning = true;
        }

        public void StopTask()
        {
            if (!IsRunning)
                return;

            if (!cancellationTokenSource.Token.IsCancellationRequested)
                cancellationTokenSource.Cancel();
            IsRunning = false;
        }

        private void DoSomeWork(CancellationToken ct, Action action)
        {
            try
            {
                if (ct.IsCancellationRequested == true)
                    ct.ThrowIfCancellationRequested();

                action();

            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "action");
            }
        }

        private void DoSomeRecurringWork(CancellationToken ct, Action action)
        {
            try
            {
                if (ct.IsCancellationRequested == true)
                    ct.ThrowIfCancellationRequested();

                while (true)
                {
                    Task.Delay(this.interval).Wait();
                    action();
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "action");
            }
        }
        
        #region Dispose methods

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (disposed)
                return;

            if (isDisposing)
            {
                StopTask();
             
                cancellationTokenSource?.Dispose();
                disposed = true;
            }
        }

        public void Cancel()
        {
            cancellationTokenSource?.Cancel();
        }

        #endregion
    }

}
