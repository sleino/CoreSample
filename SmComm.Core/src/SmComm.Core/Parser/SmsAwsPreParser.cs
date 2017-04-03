using SmSimple.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace SmComm.Core.Parser
{
    public static class SmsAwsPreParser
    {
        /// <summary>
        ///  Splits a SMSAWS message containing several submessages into parts.
        ///  E.g. following message contains two submessages
        ///  (S:sname;D:YYYYMMDD;T:HHMMSS;TA1:12.1\r\nS:sname;D:YYYYMMDD;T:HH+1MMSS;TA1:13.1\r\n)
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static List<string> FindSmsAwsSubMessages(string inputData)
        {
            if (inputData == null)
                return new List<string>();

            try
            {
                char[] trimChars = { '\r', '\n' };
                inputData = inputData.Trim(trimChars);
                var start = inputData.IndexOf("(S:", StringComparison.Ordinal);
                if (start < 0)
                {
                    var msg = string.Format(CultureInfo.InvariantCulture, "{0}\r\nCould not find first characters: (S:", inputData);
                    return new List<string>();
                }
                inputData = inputData.Substring(start);
                return ParseData(inputData);

            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex);
                return new List<string>();
            }
        } // ParseSmsAwsMessage

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        private static List<string> ParseData(string inputData)
        {
            var result = new List<string>();
            var sNext = FindNextSubMessage(ref inputData);
            while (inputData.Length > 0)
            {
                if (sNext.Length > 0)
                {
                    if (sNext.Contains("(")) // '(' not allowed except at the beginning
                        return new List<string>();

                    result.Add(string.Format(CultureInfo.InvariantCulture, "({0})", sNext)); 
                }
                sNext = FindNextSubMessage(ref inputData);
            }
            if (sNext.Length > 0)
                result.Add(string.Format(CultureInfo.InvariantCulture, "({0})", sNext));    //result.Add("(S:MAWS110;D:050912;T:010000;PR:0.0)");		

            return result;
        }

        private static string FindNextSubMessage(ref string s)
        {
            try
            {
                if (s.Length == 0)
                    return String.Empty;

                var start = s.IndexOf("S:", StringComparison.Ordinal);
                if (start.Equals(-1))
                {
                    s = string.Empty;
                    return String.Empty;
                }
                var endpos = s.IndexOf(")", start + 1, StringComparison.Ordinal);

                var end2 = s.IndexOf("\nS:", start + 1, StringComparison.Ordinal);
                if (end2 > start)
                    if (end2 < endpos)
                        endpos = end2;

                var length = endpos - start;
                var result = string.Empty;
                if (length > 0)
                    result = s.Substring(start, length);

                if (endpos > 0)
                    s = s.Substring(endpos);
                else
                    s = string.Empty;

                if (result.Length > 0)
                {
                    Debug.Assert(!result[0].Equals('('), "( at beginning");
                    Debug.Assert(!result[result.Length - 1].Equals(')'), ") at end");
                }
                while (result.EndsWith("\n") || result.EndsWith("\r"))
                    result = result.Substring(0, result.Length - 1);
                return result;
            }

            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex);
             //   errorHandler.ProcessError(string.Format(CultureInfo.InvariantCulture, "{0}\r\n{1}", ex.Message, s));
                return String.Empty;
            }
        }
    }
}
