using SmSimple.Core;
using System;

namespace SmComm.Core.IO
{
    public class NamedPortEventRaiser
    {
        public static event EventHandler<NamedPortEventArgs> VirtualPortEvent;

        internal static void RaiseTextWithEndCharReceived(INamedPort ivp, string messageText)
        {
            var e = new NamedPortEventArgs(NamedPortEventArgs.PortEventType.DataWithEndCharReceived, messageText, ivp);
            VirtualPortEvent?.Invoke(ivp, e);
        }

        internal static void RaiseTextReceived(INamedPort ivp, string messageText)
        {
            int test = 0;
            try
            {
                test = 1;
                 var e = new NamedPortEventArgs(NamedPortEventArgs.PortEventType.TextReceived, messageText, ivp);
                test++;
                if (VirtualPortEvent != null)
                {
                    test = 10;
                    VirtualPortEvent(ivp, e);
                    test = 11;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException("RaiseTextReceived:" + test+ ">", ex);
            }
        }

        internal static void RaiseLineTimeoutEvent(INamedPort ivp)
        {
            var e = new NamedPortEventArgs(NamedPortEventArgs.PortEventType.LineIdleTimeout, ivp);
            VirtualPortEvent?.Invoke(ivp, e);
        }

        internal static void RaiseDialUpModemEvent(INamedPort port, string text)
        {
            var e = new NamedPortEventArgs(NamedPortEventArgs.PortEventType.ModemEvent, text, port);
            VirtualPortEvent?.Invoke(port, e);
        }

        internal static void RaiseByteReceivedEvent(INamedPort ivp, byte messageBytes)
        {
            var e = new NamedPortEventArgs(NamedPortEventArgs.PortEventType.ByteReceived, messageBytes, ivp);
            VirtualPortEvent?.Invoke(ivp, e);
        }
    }
}
