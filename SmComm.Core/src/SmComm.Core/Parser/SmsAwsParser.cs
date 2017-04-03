using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using SmSimple.Core;
using SmSimple.Core.Util;
using SmValidation.Core;


namespace SmComm.Core.Parser
{
    public sealed class SmsAwsParser : ParserBase2
    {
        private const char EndChar = ')';

        public SmsAwsParser()
        {
            mainSeparatorChar = ';';
            indexOfFirstValue = 3;
        }

        public MultiMeasMsg ParseMessageIntoMultipleMeasMsg(string sRawData)
        {
            var measMsgList = new List<MeasMsg>();
            var remainder = string.Empty;

            Debug.Assert(sRawData != null, "null input in ParseMessageIntoMultipleMeasMsg");

            char[] trimChars = { '\r', '\n' };
            sRawData = sRawData.TrimEnd(trimChars);

            var lastCharOk = sRawData.EndsWith(")");

            const int iMaxElements = 1000;
            char[] separator = { EndChar };
            var values = sRawData.Split(separator, iMaxElements);

            int iMsgCount;
            for (iMsgCount = 0; iMsgCount < values.Length; iMsgCount++)
            {
                var data = values[iMsgCount];

                var doParse = (data.Length > 20); // min message = (S:A;D:160101;T:120000;B:1  => min len == 25

                if (doParse)
                    remainder = data;

                if (iMsgCount + 1 == values.Length)
                    if (!lastCharOk)
                        doParse = false;

                if (doParse)
                    ParseValue(data, ref remainder, ref measMsgList);
            }

            if (lastCharOk && measMsgList.Count < iMsgCount)
                if (remainder.Length > 0)
                    remainder = string.Concat(remainder, ")");

            var result = new MultiMeasMsg(measMsgList, remainder);
            return result;
        }

        private void ParseValue(string data, ref string remainder, ref List<MeasMsg> measMsgList)
        {
            var msgPart = string.Concat(data, ")");
            var measMsg = ParseMessageIntoMeasMsg(msgPart, virtualPort);

            if (measMsg ==null)
                return;

           measMsgList.Add(measMsg);
           remainder = string.Empty;
          
        }


        public static bool IsSmsAwsData(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            int pos1 = text.IndexOf('(');
            if (pos1 == -1)
                return false;

            text = text.Trim().Replace("\r", "").Replace("\n", "");

            int pos2 = text.IndexOf(')');
            int len = text.Length;
            return ((len > 2) && (pos2 > pos1));
        }

        public static string Code(MeasMsg measMsg, string replaceSemicolonInData = ",")
        {
            try
            {
                if (measMsg == null)
                    return string.Empty;

                var sb = new StringBuilder();
                sb.Append("(S:");
                sb.Append(measMsg.Station);
                sb.Append(";D:");
                sb.Append(measMsg.Time.ToString("yyMMdd", CultureInfo.InvariantCulture));
                sb.Append(";T:");
                sb.Append(measMsg.Time.ToString("HHmmss", CultureInfo.InvariantCulture));
                foreach (var meas in measMsg.MeasValues)
                {
                    //		measMsg.MeasList.ForEach(delegate(Meas meas)
                    //		{
                    if ((meas.Name != null) && (meas.ObsValue != null))
                    {
                        sb.Append(";");
                        sb.Append(meas.Name);
                        sb.Append(":");
                        sb.Append(meas.ObsValue.Replace(";", replaceSemicolonInData));
                    }
                    //	});
                }
                //   sb.Append(";");
                sb.Append(")");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception in Code(MeasMsg) " + ex.Message);
                return string.Empty;
            }
        }

        protected override bool ParseStationName(string[] values, ref string stationName)
        {
            if (indexOfFirstValue == 0)
            {
                stationName = DefaultStationName;
                return true;
            }

            if (values.GetUpperBound(0) < 0)
            {
                errorText = "Station name not found 0 ";
                return false;
            }
            char[] separator = { ':' };
            var s = values[0];
            string[] sFields = s.Split(separator, 2);
            if (sFields.GetUpperBound(0) < 1)
            {
                errorText = "Station name not found 1 ";
                if (s != null)
                    errorText += " Message start:" + s.SafeSubstring(0, 20);
                return false;
            }

            string lastChar = sFields[0].Substring(sFields[0].Length - 1);
            if (string.IsNullOrEmpty(lastChar))
            {
                errorText = "Station name not found 2 ";
                return false;
            }

            if (!string.Equals(lastChar, "S", StringComparison.Ordinal))
            {
                errorText = "Station indicator 'S' not found 3 ";
                return false;
            }

            if (sFields[1].Length == 0 || (sFields[1] == "/"))
            {
                errorText = "Station name not found";
                return false;
            }

            string tmp = sFields[1].Trim();
            int iSemicolon = tmp.IndexOf(';');
            if (iSemicolon > -1)
                tmp = tmp.Substring(0, iSemicolon - 1);

            stationName = tmp.Trim();
            var ok = Validation.IsValidStationName(stationName);
            if (ok)
                return ok;

            errorText = "Error in validating station name:";
            if (stationName == null)
                errorText += " name is null";
            else if (stationName == string.Empty)
                errorText += " name is empty";
            else
                errorText += stationName + "_XXX_" + sFields[0] + "_XXX_" + sFields[1];
            return ok;


        }

        protected override bool ParseDate(string[] values, out string date)
        {
            date = string.Empty;
            if (indexOfFirstValue == 0)
            {
                date = DateTimeEx.FormatInvariantCulture(DateTime.Now, DateTimeEx.FormatStringDate);
                return true;
            }

            if (values.GetUpperBound(0) < 1)
            {
                errorText = "Date token 'D' was not found.";
                return false;
            }

            char[] separator = { ':' };
            var s = values[1];
            string[] sFields = s.Split(separator, 2);
            if (sFields.GetUpperBound(0) < 1)
            {
                errorText = "Date token 'D'  not found.";
                return false;
            }

            sFields[0] = sFields[0].Trim();

            if (!string.Equals(sFields[0], "D", StringComparison.Ordinal))
            {
                errorText = "Date token 'D' not found. Following character was detected:" + sFields[0]; ;
                return false;
            }

            var sb = new StringBuilder(19);

            var dateString = sFields[1].Trim();

            // support YYYY year format
            if (dateString.Length == 8)
            {
                sb.Append(dateString.Substring(0, 2));
                dateString = dateString.Substring(2);
            }
            else if (dateString.Length != 6)
            {
                errorText = "Date after token 'D' has invalid length:" + dateString.Length;
                return false;
            }
            else
            {
                Debug.Assert(dateString.Length == 6, "dateString.Length == 6");
                sb.Append("20");
            }
            sb.Append(dateString[0]);
            sb.Append(dateString[1]);
            sb.Append("-");
            sb.Append(dateString[2]);
            sb.Append(dateString[3]);
            sb.Append("-");
            sb.Append(dateString[4]);
            sb.Append(dateString[5]);
            date = sb.ToString();

            DateTime dtParse;
            if (!DateTime.TryParseExact(date, DateTimeEx.FormatStringDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtParse))
            {
                errorText = "Date has invalid format";
                return false;
            }

            return true;
        }

        protected override bool ParseTime(string[] values, out string time)
        {
            time = string.Empty;
            if (indexOfFirstValue == 0)
            {
                time = DateTimeEx.FormatInvariantCulture(DateTime.Now, DateTimeEx.FormatStringTime);
                return true;
            }

            if (values.GetUpperBound(0) < 2)
            {
                errorText = "Time token (T:) was not found.";
                return false;
            }

            char[] separator = { ':' };
            var s = values[2];
            var sFields = s.Split(separator, 2);
            if (sFields.GetUpperBound(0) < 1)
            {
                errorText = "Time token (T:) not found.";
                return false;
            }

            sFields[0] = sFields[0].Trim();
            sFields[1] = sFields[1].Trim();

            if (!string.Equals(sFields[0], "T", StringComparison.Ordinal))
            {
                errorText = "Time token 'T' not found. Following character was detected:" + sFields[0];
                return false;
            }


            if (sFields[1].Length != 6 || (sFields[1] == "/"))
            {
                errorText = "Time token 'T' not found or has invalid length, or contains '/'";
                return false;
            }
            var sb = new StringBuilder(10);
            sb.Append(sFields[1][0]);
            sb.Append(sFields[1][1]);
            sb.Append(":");
            sb.Append(sFields[1][2]);
            sb.Append(sFields[1][3]);
            sb.Append(":");
            sb.Append(sFields[1][4]);
            sb.Append(sFields[1][5]);
            time = sb.ToString();

            //indexOfFirstValue = 3;
            return true;
        }

        // todo status parsing is missing in smsawsparser
        protected override bool ParseValue(string curValue, int count, ref string variable, ref string varValue)
        {
            char[] separator = { ':' };
            var sFields = curValue.Split(separator, 2);
            if (sFields.GetUpperBound(0) < 1)
            {
                errorText = "Data value is missing in after token: " + curValue + ":";
                return false;
            }
            variable = sFields[0].Trim();

            var index = variable.IndexOf('|');
            if (index > 0)
                variable = ParseCommonCodeVarName(variable);


            varValue = sFields[1].Trim();

            if (sFields.GetLength(0) > 2)
            {
                var sb = new StringBuilder();
                for (var i = 1; i < sFields.GetLength(0); i++)
                    sb.Append(sFields[i]);
                varValue = sb.ToString();
            }

            if (Validation.IsValidVariableName(variable))
                return true;


            errorText = "Token contains invalid characters: " + curValue + ". Only letters and numbers are permitted.";
            return false;

        }


        private static string ParseCommonCodeVarName(string variable)
        {
            const int cFields = 6;
            const char sep = '|';
            var count = variable.Count(ch => ch == sep);
            if (count != cFields)
                return variable;


            char[] separator = { sep };
            var sFields = variable.Split(separator, cFields);
            if (sFields.GetLength(0) < 3)
                return variable;

            return string.Concat(sFields[0], "_", sFields[1], "_", sFields[2]);

        }
    }
}
