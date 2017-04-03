using SmSimple.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmComm.Core.IO
{
    /// <summary>
    /// Helps counting number of messages received via port
    /// </summary>
    internal class ChannelDataCounter
    {
        private readonly Dictionary<string, int> channelDataCount = new Dictionary<string, int>();
        private DateTime lastChannelDataCountReset = DateTime.Now;
        private readonly TimeSpan OneHour = TimeSpan.FromHours(1);


        internal void RegisterMessage(INamedPort namedPort)
        {
            try
            {
                if (namedPort == null)
                    return;
                if (namedPort.RemoteSocketName == null)
                    return;
                var key = namedPort.RemoteSocketName;
                if (!channelDataCount.ContainsKey(key))
                    channelDataCount.Add(key, 0);
                channelDataCount[key]++;
                if (DateTime.Now.Subtract(lastChannelDataCountReset) > OneHour)
                    PrintChannelDataCount();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "RegisterMessage");
            }
        }

        private void PrintChannelDataCount()
        {
            StringBuilder sb = new StringBuilder("Cdc_:");
            foreach (var key in channelDataCount.Keys)
                sb.Append(key + ":" + channelDataCount[key] + ";");
            SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, sb.ToString());
            lastChannelDataCountReset = DateTime.Now;
            channelDataCount.Clear();
        }
    }
}
