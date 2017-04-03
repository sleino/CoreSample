using SmSimple.Core;
using SmSimple.Core.Util;
using System;
using System.Diagnostics;
using System.Text;

namespace SmComm.Core.IO
{
    // Splits port data into chunks which can be parsed using the real parsers
    public class PortDataParser
    {
        internal const int MaxReadBlock = 4096;
        private volatile StringBuilder receivedData = new StringBuilder(MaxReadBlock);

        private readonly INamedPort iNamedPort;

        public PortDataParser(INamedPort vp, char endCharacter = ')')
        {
            iNamedPort = vp;
            EndChar = endCharacter;
            UseCrc32 = false;
        }

        internal char EndChar { get; set; } = ')';
  
        public bool UseCrc32 { get; set; }

#if DEBUG
        public void ClearDataReceptionBuffer()
        {
            receivedData.Clear();
        }
#endif
        /// <summary>
        /// Append text to the body of already received data.
        /// Call RaiseTextEvent to notify non-parsing modules
        /// If text contains EndCharacter, send text to parser
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool AppendAndParse(string text)
        {
            try
            {
                RaiseTextReceived(text);

                UpdateReceivedData(text);

                if (receivedData.Length >= MaxReadBlock)
                {
                    RaiseTextWithEndCharReceived(receivedData.ToString());
                    receivedData.Clear();
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "Exception in AppendAndParse ");
                return false;
            }
        }

        /// <summary>
        /// Append text to receivedData. 
        /// Look for end characters and send all data up to the end char to parser
        /// Put any leftover text to receivedData
        /// </summary>
        /// <param name="text"></param>
        private void UpdateReceivedData(string text) {
            receivedData.Append(text);

            var remainingMsg = receivedData.ToString();
            var startIndex = 0;
            var endIndex = remainingMsg.IndexOf(EndChar, startIndex);

            endIndex = IncreaseEndIndexIfCrcEnabled(endIndex, remainingMsg);

            while (endIndex > -1)
            {
                Debug.Assert(endIndex < remainingMsg.Length, "invalid end index " + endIndex);
                RaiseTextWithEndCharReceived(remainingMsg.Substring(startIndex, endIndex + 1));

                if (endIndex == remainingMsg.Length - 1)
                    remainingMsg = string.Empty;
                else
                    remainingMsg = remainingMsg.Substring(endIndex + 1);

                startIndex = 0;
                if (startIndex >= remainingMsg.Length)
                    break;

                endIndex = remainingMsg.IndexOf(EndChar);
                endIndex = IncreaseEndIndexIfCrcEnabled(endIndex, remainingMsg);
            }

            receivedData = new StringBuilder(remainingMsg, MaxReadBlock);
        }


        /// <summary>
        ///  When CRC is used, CRC bytes are located after the end character.
        /// </summary>
        /// <param name="endIndex"></param>
        /// <param name="remainingMsg"></param>
        /// <returns></returns>
        private int IncreaseEndIndexIfCrcEnabled(int endIndex, string remainingMsg)
        {
            if (UseCrc32 && endIndex > -1 && endIndex + 8 < remainingMsg.Length)
            {
                var crc = remainingMsg.Substring(endIndex + 1, 8);
                if (StringUtil.IsHex(crc))
                    endIndex = endIndex + 8;
            }
            return endIndex;
        }


        internal void RaiseTextWithEndCharReceived(string messageText)
        {
            try
            {
                if (iNamedPort != null)
                    NamedPortEventRaiser.RaiseTextWithEndCharReceived(iNamedPort, messageText);
            }
            catch (NullReferenceException)
            {
                Debug.Assert(true);
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception in RaiseTextWithEndCharReceived " + ":" + ex.Message);
            }
        }

        internal void RaiseTextReceived(string messageText)
        {
            try
            {
                if (iNamedPort != null)
                    NamedPortEventRaiser.RaiseTextReceived(iNamedPort, messageText);
            }
            catch (NullReferenceException)
            {
                Debug.Assert(true);
            }
            catch (Exception ex)
            {
                string message = "Exception in RaiseTextReceived " + (messageText ?? "messageText is null");
                ExceptionRecorder.RecordException(message, ex);             
            }
        }
    }
}
