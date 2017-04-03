using SmSimple.Core;
using SmSimple.Core.Util;
using System.Collections.Generic;

namespace SmComm.Core.IO
{
    /// <summary>
    /// Maintains lists of n most recently received messages, both in string and MeasMsg formats
    /// Informs user if a given string / MeasMsg has already been received
    /// </summary>
    internal class DuplicateMessageAssistant
    {
        private volatile List<string> messages = new List<string>();
        private volatile List<MeasMsg> measMsgList = new List<MeasMsg>();

        private readonly Lock stringLock = new Lock();
        private readonly Lock measMsgLock = new Lock();

        internal int CacheSize { get; set; } = 100;

        /// <summary>
        /// Return true if a copy of the string is already in internal cache.
        /// If not, add the string into the cache.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        internal bool DuplicateStringDetected(string msg)
        {
            bool duplicateFound = false;
            lock (stringLock)
            {
                duplicateFound = messages.Contains(msg);
                if (!duplicateFound)
                {
                    messages.Add(msg);

                    if (messages.Count > CacheSize)
                        messages.RemoveAt(0);
                }
            }
            return duplicateFound;
        }


        /// <summary>
        /// Return true if a copy of measMsg is already in internal cache.
        /// If not, add measMsg into the cache.
        /// </summary>
        /// <param name="measMsg"></param>
        /// <returns></returns>
        internal bool DuplicateMeasMsgDetected(MeasMsg measMsg)
        {
            bool duplicateFound = false;
            lock (measMsgLock)
            {
                foreach (var listItem in measMsgList)
                {
                    if (listItem.CheckForDuplicateFilter(measMsg))
                    {
                        duplicateFound = true;
                        break;
                    }
                }

                if (!duplicateFound)
                {
                    measMsgList.Add(measMsg);

                    if (measMsgList.Count > CacheSize)
                        measMsgList.RemoveAt(0);
                }
            }

            return duplicateFound;
        }

    }
}
