using SmComm.Core.Parser;
using SmSimple.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SmComm.Core.IO
{
    public abstract class VaiSocketServerBase : IDisposable
    {
 
        protected TcpListener socketServer = null;

        public bool KeepServerRunning { get; protected set;  }
        protected readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        protected IPAddress localIpAddr { get; set; }
        protected Int32 localPort  { get; set; }

        protected bool enableClientListening { get; set; }
        internal ParserAssistant parserAssistant = new ParserAssistant();

        internal VaiSocketServerBase(bool listenToClients = false)
        {
            enableClientListening = listenToClients;
            NamedPortEventRaiser.VirtualPortEvent += (PortEventReceived);
        }

 
        private const string cDefaultIpAndPort = "0:0";

        public string IpAndPort
        {
            get
            {
                if (localIpAddr != null)
                    return localIpAddr + ":" + localPort;
                return cDefaultIpAndPort;
            }
        }
        public string IPAddr
        {
            get { return localIpAddr.ToString(); }
            protected set { localIpAddr = IPAddress.Parse(value); }
        }
        public Int32 Port
        {
            get { return localPort; }
            protected set { localPort = value; }
        }

        internal protected void PortEventReceived(object sender, NamedPortEventArgs e)
        {
            try
            {
                if (e == null || IpAndPort == cDefaultIpAndPort)
                    return;

                string eventLocalPort = (e.Port != null ? e.Port.RemoteSocketName : "Null port");
 
                if (eventLocalPort != IpAndPort)
                    return;

                var eventType = e.PortEvent;
                if (eventType == NamedPortEventArgs.PortEventType.TextReceived)
                    parserAssistant.PlainTextReceived(e.Text, e.Port);
                else if (eventType == NamedPortEventArgs.PortEventType.DataWithEndCharReceived)
                    parserAssistant.DataReceived(e.Text, e.Port);
                else if (eventType == NamedPortEventArgs.PortEventType.LineIdleTimeout)             
                    LineTimeoutReceived(sender, e);

                //else if (e.GetPortEventType == VirtualPortEventArgs.PortEventType.ModemEvent)
                //    ModemEventReceived(e);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "PortEventReceived");
            }
        }

        internal abstract void LineTimeoutReceived(object sender, NamedPortEventArgs e);

        public ConnectionData ConnectionData { get; set; }



        public void SetParserAssistant(ParserAssistant parserAssistant)
        {
            this.parserAssistant = parserAssistant;
        }


        public bool WriteDetailedLog { get; set; }

        internal void WriteToLog(string text)
        {
            if (!WriteDetailedLog  )
                return;
            SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, text);
        }

        protected static void HandleError(string msg)
        {
            SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, msg);
        }

        #region IDisposable
        ~VaiSocketServerBase()
        {
            Dispose(false);
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (disposed)
                return;

            if (disposing)
            {
                NamedPortEventRaiser.VirtualPortEvent -= (PortEventReceived);

            }
            disposed = true;
        }
        #endregion
    }


    public sealed class VaiSocketServer : VaiSocketServerBase, IDisposable
    {

        private readonly Lock clientLock = new Lock();
        private readonly List<VaiTcpIpClient> clients = new List<VaiTcpIpClient>();
        private readonly List<VaiListener> listeners = new List<VaiListener>();

        private readonly Lock clientsToBeRemovedLock = new Lock();
        private volatile List<string> clientsToBeRemoved = new List<string>();
        private readonly List<string> clientsToBeRemovedTmp = new List<string>();

      //  private TaskHelper transmissionTask;
        private TaskHelper acceptClientsTask;
        private TaskHelper closeClientsTask;

        private Task acceptClients;
        private Task removeIdleClients;

     //   private bool runDataTransmission;

        const int transmissionInterval = 100;

        public VaiSocketServer(bool enableClientListening) : base(enableClientListening) { }

        #region IDisposable
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

                foreach (var listener in listeners)
                    listener.Dispose();
                listeners.Clear();

                foreach (VaiTcpIpClient client in clients)
                    client?.Dispose();
                clients.Clear();

                Debug.WriteLine("Disposed socket server " + IpAndPort);
                base.Dispose(disposing);
            }
        }

        #endregion

        public bool Start(string ipAddress, int port)
        {
            try
            {
                this.IPAddr = ipAddress;
                this.Port = port;

                if (KeepServerRunning) // Server already running ?
                    Stop();

                KeepServerRunning = true;
                socketServer = SocketServerHelper.Start(this.Port, this.IPAddr);
                acceptClients = Task.Run(() => AcceptClientsAsync());
                removeIdleClients = Task.Run(() => RemoveClients());

                return true;

            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "Error while initialising socket server" );
                return false;
            }
        }

        private void StartStopping()
        {
            KeepServerRunning = false;
          //  runDataTransmission = false;
        }

        public bool Stop()
        {
            try
            {
                StartStopping();

                SocketServerHelper.Stop(cancellation, socketServer);

                Debug.WriteLine("Closing socket server ... " + IpAndPort);
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, "Stopping socket server at " + IpAndPort);
            
                DisposeTasks();
                return true;
            }
            catch (Exception ex)
            {
                HandleError(StringManager.GetString("Error while stopping socket server: ") + ex.Message);
                return false;
            }
        }

        private void DisposeTasks()
        {
            acceptClientsTask?.Dispose();
            acceptClientsTask = null;

            closeClientsTask?.Dispose();
            closeClientsTask = null;
        }

        private async void AcceptClientsAsync()
        {
            TcpClient tcpClient = null;
            VaiListener listener = null;
            try
            {
                while (KeepServerRunning && !cancellation.IsCancellationRequested)
                {
                    // If you don't check for pending messages, 
                    // and there have been at least one active connection,
                    // AcceptTcpClientAsync will throw a "cannot used disposed object exception"
                    // when tcpServer stops.
                    if (!socketServer.Pending())
                    {
                        Thread.Sleep(0);
                        continue;
                    }

                    //Accept the pending client connection and return a TcpClient object initialized for communication.
                    tcpClient = await socketServer.AcceptTcpClientAsync().WithWaitCancellation(cancellation.Token);

                    var tcpIpClient = new VaiTcpIpClient(tcpClient);
                    clients.Add(tcpIpClient);

                    if (enableClientListening)
                    {
                        var portDataParser = new PortDataParser(tcpIpClient);
                        listener = new VaiListener(tcpIpClient, portDataParser);
                        Debug.WriteLine("Adding listener " + tcpIpClient.RemoteSocketName);
                        listeners.Add(listener);
                        var t1 = new Task(() => listener.StartListening());
                        t1.Start();
                    }
                }
            }
            // this exception may occur if there have been open connections
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine("Exception in CheckForClient." + ex.Message);
                Debug.Assert(true, ex.Message);
            }
            // this exception may occur if there have been open connections
            catch (OperationCanceledException ex)
            {
                Debug.Assert(true, ex.Message);
                Debug.WriteLine("Exception in CheckForClient." + ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in CheckForClient." + ex.Message);
                Debug.Assert(false, ex.Message);
            }
            finally
            {
                listener?.Dispose();
                if (!KeepServerRunning)
                    Debug.WriteLine("CheckForClient finally. keepServerRunning:" + KeepServerRunning);
            }
        }

        private void RemoveClients()
        {
            if (clientsToBeRemoved.Count == 0)
                return;

            lock (clientsToBeRemovedLock)
            {
                clientsToBeRemovedTmp.AddRange(clientsToBeRemoved);
                clientsToBeRemoved.Clear();
            }

            foreach (var clientName in clientsToBeRemovedTmp)
                RemoveClient(clientName);

            clientsToBeRemovedTmp.Clear();
        }

        private void RemoveClient(string clientName)
        {
            try
            {
                VaiTcpIpClient clientToRemove = null;
                foreach (var client in clients)
                {
                    if (string.Compare(client.RemoteSocketName, clientName, StringComparison.Ordinal) == 0)
                    {
                        clientToRemove = client;
                        break;
                    }
                }

                if (clientToRemove == null)
                    return;

                Debug.WriteLine(DateTimeEx.NowToStringWithMs + "\tRemoving client " + clientName);

                WriteToLog("Removing client:" + clientName);

                lock (clientLock)
                {
                    clients.Remove(clientToRemove);
                    Task.Delay(10).Wait();
                    clientToRemove.Dispose();
                }
                Debug.WriteLine(DateTimeEx.NowToStringWithMs + "\tClient removed: " + clientName);
                WriteToLog( clients.Count + " Client removed:" + clientName);
            }
            catch (Exception ex)
            {
                Debug.Assert(true, ex.Message);
                ExceptionHandler.HandleException(ex, "RemoveClient");
            }
        }

        private const int cRemoveClientsLoopMs = 300;
        private void CloseAllClients()
        {
            try
            {
                //    SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, "Closing socket server CloseAllClients " + IpAndPort + " clients:" + clients.Count);
                lock (clientsToBeRemovedLock)
                {
                    foreach (VaiTcpIpClient tcpClient in clients)
                        MarkClientToBeRemoved(tcpClient);
                }
                Thread.Sleep(cRemoveClientsLoopMs);
             //   RemoveClients();
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception in CloseAllClients " + ex.Message);
            }
        }

         internal override void LineTimeoutReceived(object sender, NamedPortEventArgs e)
        {
            foreach (VaiTcpIpClient client in clients)
            {
                if (string.Compare(client.RemoteSocketName, e.PortLongName, false) == 0)
                    lock (clientsToBeRemovedLock)
                    {
                        SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Client in timeout:" + e.PortLongName);
                        MarkClientToBeRemoved(client);
                        break;
                    }
            }
        }

        private void MarkClientToBeRemoved(INamedPort client)
        {
            clientsToBeRemoved.Add(client.RemoteSocketName);
            SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "clientsToBeRemoved.Count:" + clientsToBeRemoved.Count + "\tMarkClientToBeRemoved:" + client.RemoteSocketName);
        }


    } // internal class VaiSocketServer




}
