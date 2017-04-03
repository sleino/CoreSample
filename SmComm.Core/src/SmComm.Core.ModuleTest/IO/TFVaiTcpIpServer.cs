using SmComm.Core.IO;
using SmSimple.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SmComm.Core.ModuleTest.IO
{
   
    public class TFVaiTcpIpServer
    {
 
        const int numSentMessages = 30;
        private readonly Dictionary<int, int> receivedMsgCount = new Dictionary<int, int>();
        private readonly Dictionary<int, bool> keepServerRunning = new Dictionary<int, bool>();
        private readonly Dictionary<int, CancellationTokenSource> cancellation = new Dictionary<int, CancellationTokenSource>();

        /// <summary>
        /// Connect to a tcp ip server
        /// Send messages to the server
        /// </summary>
        [Fact]
        public void WriteToSocketServer()
        {
            const int tcpPort = 10097;
            TcpListener socketServer = null;
            try
            {
                socketServer = SocketServerHelper.Start(tcpPort);
                var ipAddress = "127.0.0.1";
               
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Testing.");
                for (var testRun = 0; testRun < 2; testRun++)
                    using (var tcpIpClient = new VaiTcpIpClient())
                    {
                        Assert.True(tcpIpClient.ConnectAsync(IPAddress.Parse(ipAddress), tcpPort).Result);
                        for (var i = 0; i < numSentMessages; i++)
                        {
                            var message = "Sample data " + i;
                            Assert.True(tcpIpClient.Write(message));
                            Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                        }
                        Assert.True(tcpIpClient.Disconnect());
                    }
            }
            finally
            {
                SocketServerHelper.Stop(null, socketServer);
            }
        }

        
        /// <summary>
        /// Start TcpListener. 
        /// Send and receive data using VaiTcpIpClient
        /// </summary>
        [Fact]
        public void TestListener()
        {
            const int tcpPort = 10099;
            NamedPortEventRaiser.VirtualPortEvent += VirtualPortEventRaiser_VirtualPortEvent;
            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "\tTest Started...");
            TcpListener socketServer = null;
            cancellation.Add(tcpPort, new CancellationTokenSource());
            try
            {
                receivedMsgCount.Add(tcpPort, 0);
                keepServerRunning.Add(tcpPort, true);
                socketServer = SocketServerHelper.Start(tcpPort);

                Task.Run(() => CheckForClientsAsync(socketServer, tcpPort));              
                Task.Run(() => SendTestDataWithNewTcpClientAsync(tcpPort));

                while (receivedMsgCount[tcpPort] < numSentMessages)
                    Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();

                Assert.True(receivedMsgCount[tcpPort] == numSentMessages);
           }
            finally
            {
                receivedMsgCount.Remove(tcpPort);
                keepServerRunning[tcpPort] = false;
                SocketServerHelper.Stop(cancellation[tcpPort], socketServer);
                cancellation.Remove(tcpPort);
                Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "\tTest Finished.");
            }
        }
        
        private async Task<bool> CheckForClientsAsync(TcpListener socketServer, int tcpPort)
        {
            TcpClient tcpClient = null;
            VaiListener listener = null;
            try
            {
                while (keepServerRunning[tcpPort] && !cancellation[tcpPort].IsCancellationRequested)
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
                    tcpClient = await socketServer.AcceptTcpClientAsync().WithWaitCancellation(cancellation[tcpPort].Token);

                    var tcpIpClient = new VaiTcpIpClient(tcpClient);
                    
                    var portDataParser = new PortDataParser(tcpIpClient);
                    listener = new VaiListener(tcpIpClient, portDataParser);

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
                listener?.Dispose(); // will also dispose tcpIpClient
                if (!keepServerRunning[tcpPort])
                    Debug.WriteLine("CheckForClient finally. keepServerRunning:" + keepServerRunning[tcpPort]);
            }
        }
        
        /// <summary>
        /// Send some messages at even intervals using new tcpClient
        /// </summary>
        private async void SendTestDataWithNewTcpClientAsync(int tcpPort)
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1"); // DnsWrapper.GetPrimaryIp;
            using (var tcpIpClient = new VaiTcpIpClient())
            {
                bool ok = await tcpIpClient.ConnectAsync(ipAddress, tcpPort);
                Debug.Assert(ok, "Failed to connect to server");

                for (var i = 0; i < numSentMessages; i++)
                {
                    var msg = i + "ABC";
                    ok = tcpIpClient.Write(msg);
                    Debug.Assert(ok, "Failed to send message:" + msg);
                    Debug.WriteLine("Sending:" + msg);
                    Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                }
                Debug.WriteLine("End send data.");
            }
        }

        private async void SendTestDataAsync(int tcpPort)
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1"); // DnsWrapper.GetPrimaryIp;
            for (var i = 0; i < numSentMessages; i++)
            {
                using (var tcpIpClient = new VaiTcpIpClient())
                {
                    bool ok = await tcpIpClient.ConnectAsync(ipAddress, tcpPort);

                    Debug.Assert(ok, "Failed to connect to server");
               

                        var msg = i + "ABC";
                        tcpIpClient.Write(msg);
                        Debug.WriteLine("Sending:" + msg);
                        Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
             
                    Debug.WriteLine("End send data.");
                }
            }
        }

        /// <summary>
        /// Receive sent messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VirtualPortEventRaiser_VirtualPortEvent(object sender, NamedPortEventArgs e)
        {
            try
            {
                Debug.WriteLine("Tf Socket Server DataReceived:" + e.Text);
                Debug.Assert(true);
                if (e.PortEvent == NamedPortEventArgs.PortEventType.TextReceived)
                {                   
                    int portIndex = e.Port.LocalSocketName.IndexOf(":");
                    if (portIndex == -1)
                        return;

                    bool ok = int.TryParse(e.Port.LocalSocketName.Substring(portIndex + 1), out int port);
                    if (receivedMsgCount.ContainsKey(port))
                    {
                        receivedMsgCount[port] = receivedMsgCount[port] + 1;
                        Debug.WriteLine("HandleReceivedPlainText from port:" + port + ":" + receivedMsgCount[port] + "  " + e.Text);
                    }
                    else
                        SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Key for receivedMsgCount not found: " + port);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "VirtualPortEventRaiser_VirtualPortEvent");
            }
        }


        /// <summary>
        /// Receive sent messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VirtualPortEventRaiser_VirtualPortEvent2(object sender, NamedPortEventArgs e)
        {
            try
            {
                Debug.WriteLine("Tf Socket Server DataReceived:" + e.Text);
                Debug.Assert(true);
                if (e.PortEvent == NamedPortEventArgs.PortEventType.TextReceived)
                {
                    int portIndex = e.Port.LocalSocketName.IndexOf(":");
                    if (portIndex == -1)
                        return;

                    bool ok = int.TryParse(e.Port.LocalSocketName.Substring(portIndex + 1), out int port);
                    receivedMsgCount[port] = receivedMsgCount[port] + 1;
                    Debug.WriteLine("HandleReceivedPlainText from port:" + port + ":" + receivedMsgCount[port] + "  " + e.Text);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "VirtualPortEventRaiser_VirtualPortEvent");
            }
        }


        /// <summary>
        /// Send and receive data using VaiSockerServer
        /// </summary>
        [Fact]
        public void TestVaiSockerServer()
        {
            var stopWatch = new Stopwatch();
            
            const int tcpPort = 10095;
            receivedMsgCount.Add(tcpPort, 0);
            NamedPortEventRaiser.VirtualPortEvent += VirtualPortEventRaiser_VirtualPortEvent;

            using (var vaiSocketServer = new VaiSocketServer(true))
            {
                try
                {
                    vaiSocketServer.Start("127.0.0.1", tcpPort);
                    stopWatch.Start();
                    Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                    Task.Run(() => SendTestDataWithNewTcpClientAsync(tcpPort));
                   
                    while (receivedMsgCount[tcpPort] == 0 && stopWatch.Elapsed.TotalSeconds < 2)
                        Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();

                    Assert.True(receivedMsgCount[tcpPort] > 0, "No messages received in TestVaiSockerServer");
                }
                finally
                {
                    receivedMsgCount.Remove(tcpPort);
                    Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "\tTest Finished.");
                }
            }
        }
    }
}
