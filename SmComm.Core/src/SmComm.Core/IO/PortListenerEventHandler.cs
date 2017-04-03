using SmSimple.Core.Util;
using System;
using SmComm.Core.Parser;

namespace SmComm.Core.IO
{
    public sealed class PortListenerEventArgs : EventArgs
    {
        private readonly string _text;
        private readonly MeasMsg measMsg;
        private readonly INamedPort virtualPort;


        internal PortListenerEventArgs(INamedPort port, MeasMsg msg)
        {
            measMsg = msg;
            virtualPort = port;
        }

        internal PortListenerEventArgs(INamedPort port, string text)
        {
            _text = text;
            virtualPort = port;
        }

        internal PortListenerEventArgs(INamedPort port, MeasMsg measMsg, string text)
        {
            _text = text;
            virtualPort = port;
            this.measMsg = measMsg;
        }
        public string Text { get { return _text; } }
        public MeasMsg MeasMsg { get { return measMsg; } }
        public INamedPort VirtualPort { get { return virtualPort; } }
    }



    /// <summary>
    ///  Events that should be handled by classes external to smComm module
    /// </summary>
    public sealed class PortListenerEventHandler
    {

        public const string Connected = "Connected.";
        public const string Disconnected = "Disconnected.";

        public static event EventHandler<PortListenerEventArgs> PortListenerPlainTextEvent;
        public static event EventHandler<PortListenerEventArgs> PortListenerLogFileEvent;
        public static event EventHandler<PortListenerEventArgs> PortListenerMeasMsgEvent;

        public static event EventHandler<PortListenerEventArgs> PortStatusEvent;
        public static event EventHandler<PortListenerEventArgs> SerialModemEvent;
        public static event EventHandler<PortListenerEventArgs> UnparsedMsgEvent;
        public static event EventHandler<PortListenerEventArgs> OttParsivelEvent;
        public static event EventHandler<PortListenerEventArgs> EndOfLogFileData;

        public static event EventHandler<PortListenerEventArgs> ParsingErrorEvent;

        //public static event EventHandler<PortListenerEventArgs> PortListenerErrorEvent;
        public static event EventHandler<PortListenerEventArgs> PortListenerCouldNotConnectEvent;


        // used by mmoc and modem task, smsmodem task
        internal void ProcessPlainText(INamedPort port, string messageText)
        {
            var e = new PortListenerEventArgs(port, messageText);
            if (PortListenerPlainTextEvent != null)
                PortListenerPlainTextEvent(this, e);
        }

        internal void ProcessObservationData(INamedPort port, MeasMsg measMsg, string text)
        {
            var e = new PortListenerEventArgs(port, measMsg, text);
            if (PortListenerMeasMsgEvent != null)
                PortListenerMeasMsgEvent(this, e);
        }


        internal void ProcessUnparsedData(INamedPort port, string text)
        {
            var e = new PortListenerEventArgs(port, text);
            if (UnparsedMsgEvent != null)
                UnparsedMsgEvent(this, e);
        }

        internal void ProcessOttParsivelData(INamedPort port, MeasMsg measMsg, string text)
        {
            var e = new PortListenerEventArgs(port, measMsg, text);
            if (OttParsivelEvent != null)
                OttParsivelEvent(this, e);
        }

        internal void ProcessParsingError(ParserEventArgs parserEventArgs)
        {
            var e = new PortListenerEventArgs(parserEventArgs.VirtualPort, parserEventArgs.ErrorDescription);
            if (ParsingErrorEvent != null)
                ParsingErrorEvent(this, e);
        }

        internal void ProcessCouldNotConnectError(INamedPort port, string error)
        {
            var e = new PortListenerEventArgs(port, error);
            if (PortListenerCouldNotConnectEvent != null)
                PortListenerCouldNotConnectEvent(this, e);
        }


        public void ProcessLogFileData(INamedPort port, MeasMsg measMsg)
        {
            var e = new PortListenerEventArgs(port, measMsg);
            if (PortListenerLogFileEvent != null)
                PortListenerLogFileEvent(this, e);
        }

        public void OnEndOfLogFileData(INamedPort port, MeasMsg measMsg)
        {
            var e = new PortListenerEventArgs(port, measMsg);
            if (EndOfLogFileData != null)
                EndOfLogFileData(this, e);
        }

        internal void ProcessModemEventData(INamedPort port, string msg)
        {
            var e = new PortListenerEventArgs(port, msg);
            if (SerialModemEvent != null)
                SerialModemEvent(this, e);
        }

        internal void ProcessPortStatusData(INamedPort port, string msg)
        {
            var e = new PortListenerEventArgs(port, msg);
            if (PortStatusEvent != null)
                PortStatusEvent(this, e);
        }

    }
}
