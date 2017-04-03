using System;

namespace SmComm.Core.IO
{
    public sealed class NamedPortEventArgs : EventArgs
    {
        public enum PortEventType
        {
            DataWithEndCharReceived = 0,
            LineIdleTimeout = 1,
            ModemEvent = 2,
            TextReceived = 3,
            ByteReceived = 4
        }


        internal NamedPortEventArgs(PortEventType peType, INamedPort port)
        {
            PortEvent = peType;
            Port = port;
        }

        internal NamedPortEventArgs(PortEventType peType, string text, INamedPort port)
        {
            PortEvent = peType;
            Port = port;
            Text = text;
        }

        internal NamedPortEventArgs(PortEventType peType, byte data, INamedPort port)
        {
            PortEvent = peType;
            Port = port;
            Byte = data;
        }


        public string Text { get; private set; }
        internal byte Byte { get; private set; }

        public PortEventType PortEvent { get; private set; }


        internal string PortLongName 
        {
            get { return Port.RemoteSocketName; }
        }

        public INamedPort Port { get; private set; }

    }
}