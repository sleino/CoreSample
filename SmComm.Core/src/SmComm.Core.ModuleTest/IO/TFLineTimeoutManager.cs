using SmComm.Core.IO;
using SmSimple.Core;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SmComm.Core.ModuleTest.IO
{
    public class TFLineTimeoutManager
    {
        private TcpListener socketServer = null;
        private const int tcpPort = 10100;
        private bool keepServerRunning;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        private bool timeoutHasTakenPlace;

        /// <summary>
        /// Start listening to incoming connections with timeout = 'timeoutLenSec' 
        /// Connect with a single client.
        /// Timeout event should take place after 'timeoutLenSec' seconds
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void TFBasicLineTimeout()
        {     
            NamedPortEventRaiser.VirtualPortEvent += VirtualPortEventRaiser_VirtualPortEvent;

            try
            {
                keepServerRunning = true;
                socketServer = SocketServerHelper.Start(tcpPort);
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                using (var tcpIpClient = new VaiTcpIpClient())
                {
                    var ipAddress = "127.0.0.1";
                    Assert.True(tcpIpClient.ConnectAsync(IPAddress.Parse(ipAddress), tcpPort).Result);
               
                    int timeoutLenSec = 2;
                    Task.Run(() => CheckForClientsAsync(timeoutLenSec));     

                    // wait 1 extra second to be sure       
                    Task.Delay(TimeSpan.FromSeconds((timeoutLenSec+1) )).Wait();

                    Assert.True(timeoutHasTakenPlace);
                }
            }
            finally
            {
                keepServerRunning = false;
                SocketServerHelper.Stop(cancellation, socketServer);
                Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "\tTest Finished.");
            }
        }

        private void VirtualPortEventRaiser_VirtualPortEvent(object sender, NamedPortEventArgs e)
        {
            if (e.PortEvent == NamedPortEventArgs.PortEventType.LineIdleTimeout)
                timeoutHasTakenPlace = true;
        }

        /// <summary>
        /// Start listening to incoming connections with timeout = 'timeoutLenSec' 
        /// Timeout event should take place after 'timeoutLenSec' seconds
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckForClientsAsync(int timeoutLenSec)
        {
            TcpClient tcpClient = null;
            VaiListener listener = null;
            try
            {
                while (keepServerRunning && !cancellation.IsCancellationRequested)
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
                    var portDataParser = new PortDataParser(tcpIpClient);

                    listener = new VaiListener(tcpIpClient, portDataParser, timeoutLenSec);

                    var t1 = new Task(() => listener.StartListening());
                    t1.Start();
                }
                return true;
            }
            // this exception may occur if there have been open connections
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine("Exception in CheckForClient." + ex.Message);
                Debug.Assert(true, ex.Message);
                return false;
            }
            // this exception may occur if there have been open connections
            catch (OperationCanceledException ex)
            {
                Debug.Assert(true, ex.Message);
                Debug.WriteLine("Exception in CheckForClient." + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in CheckForClient." + ex.Message);
                Debug.Assert(false, ex.Message);
                return false;
            }
            finally
            {
                listener?.Dispose();
                if (!keepServerRunning)
                    Debug.WriteLine("CheckForClient finally. keepServerRunning:" + keepServerRunning);
            }
        }

        /// <summary>
        /// Send some messages at even intervals using new tcpClient
        /// </summary>
        private async void ConnectWithNewTcpClientAsync()
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            using (var tcpIpClient = new VaiTcpIpClient())
            {
                bool ok = await tcpIpClient.ConnectAsync(ipAddress, tcpPort);
                Debug.WriteLine("Client connected.");
            }
        }
    }
}
