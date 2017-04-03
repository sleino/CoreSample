using SmSimple.Core;
using SmSimple.Core.Util;
using SmValidation.Core;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmComm.Core.IO
{
    public sealed class VaiTcpIpClient:IDisposable, INamedPort
    {
        private TcpClient tcpClient ;
        private Encoding encoding = Encoding.UTF8;     
        private NetworkStream stream;

        public NamedPortState ConnectionState { get; set; } = NamedPortState.Disconnected;

        public IPAddress IpAddr     { get; private set; }
        public Int32 Port           { get; private set; }
        public int ReadBufferSize   { get; set; } = 4096;

        public VaiTcpIpClient()     { }

        /// <summary>
        /// Provide TcpClient in the constructor when handling connections from a TcpListener
        /// </summary>
        /// <param name="tcpClient"></param>
        public VaiTcpIpClient(TcpClient tcpClient)     {
            this.tcpClient = tcpClient;
        }
        /// <summary>
        /// Return address and port of local port
        /// </summary>
        /// <returns></returns>
        public string LocalSocketName
        {
            get
            {
                if ((tcpClient != null)
                    && (tcpClient.Client != null)
                    && (tcpClient.Client.LocalEndPoint != null))
                    return tcpClient.Client.LocalEndPoint.ToString();
              
                return string.Empty;
            }
        }

        /// <summary>
        /// Return address and port of remote port
        /// </summary>
        /// <returns></returns>
        public string RemoteSocketName
        {
            get
            {
                if ((tcpClient != null) && (tcpClient.Client != null) && tcpClient.Connected)
                    if (tcpClient.Client.RemoteEndPoint != null)
                        return tcpClient.Client.RemoteEndPoint.ToString();

                return string.Empty;
            }
        }

        private NetworkStream GetStream()
        {
            try
            {
                if ((tcpClient != null) && (tcpClient.Connected))
                    return tcpClient.GetStream();
                return null;
            }
            catch (ObjectDisposedException ex)
            {
                ExceptionRecorder.RecordException("Exception in GetStream " + ex.Message);
                return null;
            }
            catch (InvalidOperationException ex)
            {
                ExceptionRecorder.RecordException("Exception in GetStream " + ex.Message);
                return null;
            }
        }

        public bool HasDataAvailable()
        {
            try
            {
                bool? result = GetStream()?.DataAvailable;
                return result == true;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception in TcpIpClient HasDataAvailable :" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Create TcpClient for connecting to a server.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="tcpPort"></param>
        private void InitializeConnection(IPAddress ipAddress, int tcpPort) {
            tcpClient = new TcpClient();
            this.IpAddr = ipAddress;
            this.Port = tcpPort;
           
            /*                 
             When NoDelay is false, a TcpClient does not send a packet over the network 
             until it has collected a significant amount of outgoing data. Because of 
             the amount of overhead in a TCP segment, sending small amounts of data is 
             inefficient. However, situations do exist where you need to send very small 
             amounts of data or expect immediate responses from each packet you send. 
             Your decision should weigh the relative importance of network efficiency
             versus application requirements.
             */
            tcpClient.NoDelay = true;

            /*
             * Keepalives are only to detect if the computer on the other end of an
                idle connection (one on which you are not sending data) has become
                unreachable. It should only be used in cases where there is no other
                way to cleanup connections that would otherwise be "stuck" for days,
                weeks, or months. New protocols and programs should use more sensible
                application-level methods.
            */
            // tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);

            ConnectionState = NamedPortState.Connecting;
            tcpClient.ReceiveTimeout = 0;
        }


        /// <summary>
        /// Open a connection to the given socket
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="tcpPort"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(IPAddress ipAddress, int tcpPort)
        {
            try
            {
                InitializeConnection(ipAddress, tcpPort);
                await tcpClient.Client.ConnectAsync(IpAddr, Port);
                return true;
            }
            catch (SocketException ex)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Please check connection port parameters: {0} ", ex.Message);
                if (null != IpAddr)
                    msg += " Ip: " + IpAddr;
#if DEBUG
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, msg);
#endif
                Debug.Assert(true, msg); 

                // keep this delay above 100 ms. If the connection is broken, the system will try again
                Thread.Sleep(250);
                ConnectionState = NamedPortState.Disconnected;

                //     Debug.WriteLine(DateTimeEx.NowToString + msg);
                return false;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                //     port is not between System.Net.IPEndPoint.MinPort and System.Net.IPEndPoint.MaxPort.
                Debug.Assert(false, " port is not between System.Net.IPEndPoint.MinPort and System.Net.IPEndPoint.MaxPort." + ex.Message);
                ExceptionRecorder.RecordException(" port is not between System.Net.IPEndPoint.MinPort and System.Net.IPEndPoint.MaxPort." + ex.Message);
                ConnectionState = NamedPortState.Disconnected;
                Debug.WriteLine(DateTimeEx.NowToString + " port is not between System.Net.IPEndPoint.MinPort and System.Net.IPEndPoint.MaxPort.");
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "Connect. - ObjectDisposedException: {0}", ex.Message));
                Debug.Assert(false, " System.Net.Sockets.TcpClient is closed" + ex.Message);
                Debug.WriteLine(DateTimeEx.NowToString + " System.Net.Sockets.TcpClient is closed" + ex.Message);
                ConnectionState = NamedPortState.Disconnected;
                throw;
            }
            catch (ArgumentNullException ex)
            {
                ExceptionRecorder.RecordException("hostname is null " + ex.Message);
                ConnectionState = NamedPortState.Disconnected;
                throw;
            }
            catch (System.Security.SecurityException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "Connect. - SecurityException: {0}", ex.Message));
                Debug.WriteLine(DateTimeEx.NowToString + " Connect. - SecurityException:" + ex.Message);
                ConnectionState = NamedPortState.Disconnected;
                throw;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex);
                return false;
            }
        } // Connect

   
        public bool Disconnect()
        {
            bool result = false;
            try
            {
                ConnectionState = NamedPortState.Disconnecting;
                if (tcpClient == null)
                    return true;

                GetStream()?.Dispose();
                tcpClient.Client?.Dispose();
                tcpClient.Dispose();
                result = true;
            }
            catch (Exception ex)
            {
                var s = string.Format(CultureInfo.InvariantCulture, "Disconnect and stop listening. - Exception: {0}", ex.Message);
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, s);
                Debug.Assert(true, "OK." + ex.Message);
            }
            finally
            {
                Debug.Assert(!IsOpen(), "IsOpen in finally");
                ConnectionState = NamedPortState.Disconnected;
            }
            return result;
        }

        public bool IsOpen()
        {
            try
            {
                //       Debug.WriteLine("IsOpen tcpClient != null" + tcpClient != null);              
                if (tcpClient != null && tcpClient.Client != null)
                {
                    //           Debug.WriteLine("IsOpen tcpClient.Connected:" + tcpClient.Connected);
                    return tcpClient.Connected;

                }
                return false;
            }
            catch (NullReferenceException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "client null reference in IsOpen. - exception: {0}", ex.Message));
                //  throw;
                return false;
            }
        }


        public bool Write(string dataMsg)
        {
            try
            {
                if ((tcpClient == null) || !IsOpen() || dataMsg == null)
                    return false;

                return WriteNoCatch(dataMsg);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "Write Invalid data to write. - exception: {0}", ex.Message));
                throw;
                //return false;
            }
            catch (ObjectDisposedException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "Write Stream was disposed. - exception: {0}", ex.Message));
                throw;
                //return false;
            }
            catch (System.IO.IOException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "Write IO Error. - exception: {0}. \r\nError while writing {2} to {1}", ex.Message, IpAddr, dataMsg));
                throw;
                //return false;
            }
            catch (ArgumentNullException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "Write IO Error. - ArgumentNullException: {0}", ex.Message));
                throw;
            }
        }


        public bool WriteBytes(byte[] data)
        {
            try
            {
                if ((tcpClient == null) || !IsOpen())
                    return false;
                return WriteNoCatch(data);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "Write Invalid data to write. - exception: {0}", ex.Message));
                throw;
                //return false;
            }
            catch (ObjectDisposedException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "Write Stream was disposed. - exception: {0}", ex.Message));
                throw;
                //return false;
            }
            catch (System.IO.IOException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "Write IO Error. - exception: {0}. \r\nError while writing {2} to {1}", ex.Message, IpAddr, data));
                throw;
                //return false;
            }
            catch (ArgumentNullException ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "Write IO Error. - ArgumentNullException: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Read data from current NetworkStream.
        /// Return null if stream is not available.
        /// </summary>
        /// <returns></returns>
        public string Read()
        {
            try
            {
                var data = new Byte[PortDataParser.MaxReadBlock];
                var stream = GetStream();

                if (stream == null)
                    return null;
                var bytes = stream.Read(data, 0, data.Length);
                var responseData = encoding.GetString(data, 0, bytes);
                return responseData;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, @"tcp/ip client read - exception: {0}", ex));
                return string.Empty;
            }
        }

        /// <summary>
        /// Read bytes from current NetworkStream.
        /// Return null if stream is not available.
        /// </summary>
        /// <returns></returns>
        public byte[] ReadBytes()
        {
            try
            {
                if (tcpClient == null)
                    return null;
                if (!tcpClient.Connected)
                    return null;
                stream = GetStream();
                if (stream == null)
                    return null;

                var data = new Byte[ReadBufferSize];
                stream.ReadTimeout = 5000;
                var bytes = stream.Read(data, 0, data.Length);

                return data;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException(string.Format(CultureInfo.InvariantCulture, "read - exception: {0}", ex));
                throw;
            }
        }

        /// <summary>
        /// Write data to current network stream.
        /// Do not catch exceptions.</summary>
        /// <param name="dataMsg"></param>
        /// <returns></returns>
        private bool WriteNoCatch(string dataMsg)
        {
            stream = GetStream();
            if ((stream == null) || (!stream.CanWrite))
                return false;
            Byte[] data = encoding.GetBytes(dataMsg);
            if (data == null)
                return false;
            stream.Write(data, 0, data.Length);
            return true;
        }

        /// <summary>
        /// Write data to current network stream.
        /// Do not catch exceptions.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool WriteNoCatch(byte[] data)
        {
            stream = GetStream();
            if ((stream == null) || (!stream.CanWrite))
                return false;
            stream.Write(data, 0, data.Length);
            return true;
        }

        /// <summary>
        /// Return true if ipAddressOrHostName is valid ip address or host name.
        /// Ip address and host name will be returned in out parameters.
        /// </summary>
        /// <param name="ipAddressOrHostName"></param>
        /// <param name="ipAddress"></param>
        /// <param name="hostName"></param>
        /// <returns></returns>
        private static bool SolveIpAddressFromHostNameOrIpAddress(string ipAddressOrHostName, out string ipAddress, out string hostName)
        {
            ipAddress = string.Empty;
            hostName = string.Empty;
            var ok = Conversion.IsValidIpAddress(ipAddressOrHostName);
            // no host name
            if (ok)
            {
                ipAddress = ipAddressOrHostName;
                return true;
            }

            var list = DnsWrapper.GetHostIp4Addresses(ipAddressOrHostName);
            if (list == null || list.Count == 0)
                return false;

            hostName = ipAddressOrHostName;
            foreach (var address in list)
            {
                string addr = address.ToString();
                if (Conversion.IsValidIpAddress(addr))
                    ipAddress = addr;
            }
            return true;
        }

        #region IDisposable
        ~VaiTcpIpClient()
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
                Disconnect();
            }
            disposed = true;
        }

        #endregion
    } // class
}
