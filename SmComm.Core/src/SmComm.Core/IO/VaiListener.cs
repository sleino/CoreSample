using SmSimple.Core;
using System;
using System.Diagnostics;
using System.Threading;


namespace SmComm.Core.IO
{
    /// <summary>
    /// Runs listening thread / task for NamedPort
    /// </summary>
    public sealed class VaiListener : IDisposable
    {
        private readonly LineTimeoutManager lineTimeoutManager;
        public LineTimeoutManager LineTimeoutManager { get; }

        public INamedPort NamedPort { get; }
        public PortDataParser PortDataParser { get; private set; }
        public bool IsListening { get; private set; }

        public VaiListener(INamedPort namedPort, PortDataParser portDataParser, int lineTimeoutSeconds=300)
        {
            NamedPort = namedPort;
            PortDataParser = portDataParser;
            lineTimeoutManager = new LineTimeoutManager(this);
            lineTimeoutManager.LineIdleTimeout = TimeSpan.FromSeconds(lineTimeoutSeconds);
        }


        public void StartListening()
        {
            IsListening = true;
          
            while (IsListening)
            {
                lineTimeoutManager.StartTimer();
                try
                {
                    Listen();
                }
                catch (Exception ex)
                {
                    ExceptionRecorder.RecordException("Exception in TcpServer ListenNoThrow :" + ex.Message);
                    Thread.Sleep(50);
                    IsListening = false;
                }
                lineTimeoutManager.StopTimer();
            }
        }


        private void Listen()
        {
            while (IsListening)
            {
                // use Monitor to prevent possible exceptions when Disposing an active Listener
                if (Monitor.TryEnter(NamedPort)) 
                {
                    try
                    {
                        if (NamedPort.HasDataAvailable())
                        {
                            
                            string msg = NamedPort.Read();                            
                            lineTimeoutManager.LastDataReceived = DateTimeEx.Now;
                                                     
                            PortDataParser.AppendAndParse(msg);
                            
                        }
                    }
                    finally {
                        Monitor.Exit(NamedPort);
                    }
                }
            }
        }

        internal void StopListening() {
            IsListening = false;
        }

        internal void TimeoutHasOccurred() {
            NamedPortEventRaiser.RaiseLineTimeoutEvent(NamedPort);
            Debug.WriteLine(DateTimeEx.NowToStringWithMs + " TimeoutHasOccurred local:" + NamedPort.LocalSocketName + " remote" +NamedPort.RemoteSocketName) ;
        }

        #region IDisposable
        ~VaiListener()
        {
            Dispose(false);
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {     
                lineTimeoutManager?.Dispose();
                IsListening = false;

                NamedPort?.Dispose();
            }
            disposed = true;
        }


        #endregion
    } // class
}
