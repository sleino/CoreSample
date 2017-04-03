using SmSimple.Core;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace SmComm.Core.IO
{
    public static class SocketServerHelper
    {
        /// <summary>
        /// Start socket server with active incoming connections
        /// </summary>
        public static TcpListener Start( int tcpPort, string address= "127.0.0.1")
        {
            IPAddress localAddr = IPAddress.Parse(address);
            var socketServer = new TcpListener(localAddr, tcpPort);
            socketServer.Start();
            Debug.WriteLine("tcpServer.Start()." + address + ":" + tcpPort);
            return socketServer;
        }

        /// <summary>
        /// Stop socket server with active incoming connections
        /// </summary>
        public static void Stop( CancellationTokenSource cancellation, TcpListener socketServer)
        {
            try
            {
                cancellation?.Cancel();
                Debug.WriteLine("cancellation.Cancel().");
                socketServer?.Stop();
                Debug.WriteLine("tcpServer.Stop().");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "Exception while stopping socket server");
            }
        }
    }
}
