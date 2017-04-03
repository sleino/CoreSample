using System;
using SmComm.Core.Parser;
using System.Diagnostics;

namespace SmComm.Core.IO
{
    public enum ConnectionType { TcpIpClient, ComPort, SerialModemPort, TcpModemPort }

    /// <summary>
    /// Holds various data related to a single NamedPort
    /// </summary>
    public sealed class ConnectionData
    {      
        public ConnectionData(string station)
        {
            StationName = station;
        }

        public string Name
        {
            get
            {
                if ((ConnectionDataType == ConnectionType.TcpIpClient)
                    || (ConnectionDataType == ConnectionType.TcpModemPort))
                {
                    if ((!string.IsNullOrEmpty(IpAddress)))
                        return IpAddress + ":" + TcpPort;
                }
                else if ((ConnectionDataType == ConnectionType.ComPort)
                    || (ConnectionDataType == ConnectionType.SerialModemPort))
                {
                    throw new Exception("Serial port support disabled");
                    //if ((!string.IsNullOrEmpty(serialPortData.PortName)))
                    //    return serialPortData.PortName;
                }
                Debug.Assert(false, "null name in connection data " + ConnectionDataType);
                return string.Empty;
            }
        }

        public string StationName { get; private set; } = string.Empty;
        public string TelephoneNumber { get; private set; } = string.Empty;

        public ConnectionType ConnectionDataType { get; private set; }

        public string PollCommand { get; private set; } = "poll";

        public string ModemSimCardReply { get; private set; } = string.Empty;

        public bool HangupModemBeforeUse    { get; set; }
        public int ConnectionTimeoutMilliseconds { get; set; }
        public int LineIdleTimeoutSeconds { get; set; }

        public string IpAddress { get; set; }

        public string TcpPort { get; set; }

        public char EndChar { get; set; } = ')';

        public string SecondaryEndString { get; set; } = string.Empty;

        public AwsMessageFormat ParserName { get; set; } = AwsMessageFormat.SMSAWS;

    }
}
