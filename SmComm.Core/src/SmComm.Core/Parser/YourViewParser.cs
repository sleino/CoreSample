using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SmSimple.Core;
using SmSimple.Core.Util;
using System.Diagnostics;

namespace SmComm.Core.Parser
{
    public sealed class YourViewParser
    {
        public static readonly List<string> SpecialFields = new List<string>()
        {
            "<TAB>",
            "<DATE yyyy-MM-dd>",
            "<DATE yyyyMMdd>",
            "<DATE ddMMyyyy>",
            "<DATE ddMMyy>",
            "<TIME HH:mm:ss>",
            "<TIME HHmm>",
            "<TIME HHmmss>",
            "<STATION NAME>",
            "<STATION ID>" };//, "<SOH>", "<STX>", "<ETX>", "<EOT>" };

        public static readonly string YV_SPACE = "<SPACE>";
        public static readonly string YV_SOH = "<SOH>";
        public static readonly string YV_STX = "<STX>";
        public static readonly string YV_TAB = "<TAB>";

        private readonly Dictionary<string, List<string>> variableNames = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, string> fieldSeparators = new Dictionary<string, string>();
        private readonly Dictionary<string, string> messageStarts = new Dictionary<string, string>();
        private readonly Dictionary<string, string> subParsers = new Dictionary<string, string>();
        private readonly Dictionary<string, bool> doNotUseSeparatorAtBeginningOfMessage = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> doNotUseSeparatorAtEndOfMessage = new Dictionary<string, bool>();
        private readonly Dictionary<string, string> stationMessageNames = new Dictionary<string, string>();

        private readonly CeiloMsg61Parser ceiloParser = new CeiloMsg61Parser();

        private const char SOH = '\x01';
        private const char STX = '\x02';
        private const char ETX = '\x03';
        private static readonly string sSOH = SOH.ToString();
        private static readonly string sSTX = STX.ToString();
        private static readonly string sETX = ETX.ToString();

        public bool UseStationName { get; set; }

        public string StationName { set; get; }
        public void DoNotUseSeparatorAtBeginningOfMessage(string messageName, bool dataValue)
        {
            if (messageName == null)
                return;

            doNotUseSeparatorAtBeginningOfMessage[messageName] = dataValue;
        }

        public void DoNotUseSeparatorAtEndOfMessage(string messageName, bool dataValue)
        {
            if (messageName == null)
                return;

            doNotUseSeparatorAtEndOfMessage[messageName] = dataValue;
        }

        public void SetVariableNames(string messageName, IList<string> names)
        {
            if (messageName == null)
                return;

            if (!variableNames.ContainsKey(messageName))
                variableNames[messageName] = new List<string>();
            else
                variableNames[messageName].Clear();

            variableNames[messageName].AddRange(names);

        }

        public void SetStationMessageNames(Dictionary<string, string> stationNames)
        {
            this.stationMessageNames.Clear();
            foreach (KeyValuePair<string, string> kvp in stationNames)
                if (!stationMessageNames.ContainsKey(kvp.Value))
                    this.stationMessageNames.Add(kvp.Value, kvp.Key);
        }

        public void SetFieldSeparator(string messageName, string fieldSeparator)
        {
            if (messageName == null)
                return;

            fieldSeparators[messageName] = fieldSeparator;

        }

        public void SetMessageStart(string messageName, char messageStart)
        {
            SetMessageStart(messageName, messageStart.ToString());

        }

        public void SetMessageStart(string messageName, string messageStart)
        {
            if (messageName == null)
                return;

            messageStarts[messageName] = messageStart;

        }

        public void SetMessageSubParser(string messageName, string subParserName)
        {
            subParsers[messageName] = subParserName;
        }

        internal bool RoundSecondsToClosestFullMinute { get; set; }

        private static string ConvertFieldSeparator(string savedForm)
        {
            if (!savedForm.Contains("<"))
                return savedForm;

            return savedForm.Replace(YV_TAB, "\t").Replace(YV_SPACE, " ").ReplaceLfCr();

        }


        private static string ConvertMsgStart(string savedForm)
        {
            if (!savedForm.Contains("<"))
                return savedForm;

            return savedForm.Replace(YV_SOH, sSOH).Replace(YV_STX, sSTX);
        }


        internal bool Parse(string data, out MeasMsg measMsg)
        {

            //    Debug.WriteLine(DateTimeEx.NowToStringWithMs + "\tYourview Parser starts" + data.Substring(0, Math.Min(data.Length, 30)));


            measMsg = new MeasMsg();
            try
            {
                if (variableNames.Count == 0)
                {
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, "YourView parser has not been initialised. Variable names have not been set.");
                    return false;
                }

                CheckControlParameters();

                if (string.IsNullOrEmpty(data))
                {
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser: null or empty input data.");
                    return false;
                }

                string messageName = GetMessageName(data);

                if (string.IsNullOrEmpty(messageName))
                {
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser: unable to solve message name.");
                    return false;
                }

                if (!fieldSeparators.ContainsKey(messageName))
                {
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser: field separator has not been defined for message " + messageName);
                    return false;
                }


                string fieldSeparator = ConvertFieldSeparator(fieldSeparators[messageName]);
                string[] separator = { fieldSeparator };


                if (!messageStarts.ContainsKey(messageName))
                {
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser: field start character(s) have not been defined for message " + messageName);
                    return false;
                }

                string convertedStart = ConvertMsgStart(messageStarts[messageName]).Trim();

                string msgStart = convertedStart;
                if (messageStarts.Count > 1)
                    msgStart = messageName + convertedStart;

                string tmp = sSTX + " ";
                while (data.Contains(tmp))
                    data = data.Replace(tmp, sSTX);
                string tmp2 = " " + sSTX;
                while (data.Contains(tmp2))
                    data = data.Replace(tmp2, sSTX);


                int startOfDataValues = data.IndexOf(msgStart);
                if (startOfDataValues == -1)
                {
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser: cannot find start of message:" + messageName + ". Looking for:" + msgStart + ".Data to be parsed is:" + data);
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser: cannot find start of message:" + messageName + ". msgStart len" + msgStart.Length + ".Data len:" + data.Length);
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Index of STX:" + data.IndexOf(STX).ToIcString() + " ");
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Index of first 0:" + data.IndexOf("0").ToIcString() + " ");
                    foreach (var c in msgStart)
                        SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Index of first " + c + ":" + data.IndexOf(c).ToIcString() + " ");
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser: cannot find start of message:" + messageName + ". Looking for:" + msgStart.ReplaceSohStxEtx() + ".Data to be parsed is:" + data.ReplaceSohStxEtx());
                    return false;
                }

                int msgStartLen = msgStart.Length;
                string dataValueString = data.Substring(startOfDataValues + msgStartLen);
                if (fieldSeparators[messageName] == YV_SPACE)
                    while (dataValueString.Contains("  "))
                        dataValueString = dataValueString.Replace("  ", " ");

                Debug.Assert(data.Contains(msgStart), "data.Contains(msgStart)");
                /*
                Debug.WriteLine("message  in data:" + data.Contains(msgStart));
                Debug.WriteLine("data:" + data.ReplaceLfCr() + "\t\t" + "datavalue:" + dataValueString);
                Debug.WriteLine("messageStarts[messageName]:" + messageStarts[messageName] +"\t" +"startOfDataValues:" + startOfDataValues);
                */

                if (!doNotUseSeparatorAtBeginningOfMessage[messageName])
                    if (dataValueString.StartsWith(fieldSeparator))
                        dataValueString = dataValueString.Substring(fieldSeparator.Length);

                if (subParsers.ContainsKey(messageName))
                    if (subParsers[messageName] == "CTK25KAM61")
                    {
                        int ceiloStart = data.IndexOf("CT");
                        if (ceiloStart == -1)
                            return false;
                        return this.ceiloParser.Parse(data.Substring(ceiloStart), out measMsg);
                    }

                string[] lines = dataValueString.Split(separator, StringSplitOptions.None);

                /*
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser:dataValueString :" + dataValueString.ReplaceSohStxEtx());
                for(int i = 0; i< lines.GetLength(0);i++)
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser:line " + i + ":" + lines[i].ReplaceSohStxEtx());
                */
                if (UseStationName)
                    measMsg.Station = StationName;
                measMsg.Time = DateTimeEx.Now;


                // char[] charsToTrim = { '\n', 'r', SOH, STX, ETX };
                // string messageName = lines[0].Trim(charsToTrim);



                if (!variableNames.ContainsKey(messageName))
                {
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser: unknown message received. Message name =" + messageName);
                    return false;
                }

                var varList = variableNames[messageName];
                int lineLen = lines.GetLength(0);
                int lineIndex = 0;
                var max = varList.Count;
                for (var varCount = 0; varCount < max; varCount++)
                {
                    lineIndex = varCount;
                    if (lineIndex < lineLen)
                    {
                        string varname = varList[varCount];
                        if (string.IsNullOrEmpty(varname))
                            continue;

                        string lineValue = lines[lineIndex];
                        if (SpecialFields.Contains(varname) || varname.StartsWith("<DATE ") || varname.StartsWith("<TIME "))
                            measMsg = HandleSpecialField(varname, lineValue, measMsg);
                        else
                        {
                            if (string.IsNullOrWhiteSpace(lineValue))
                                continue;
                            measMsg.AddMeas(new Meas(varname, measMsg.Time, lineValue, MeasStatus.cOK, measMsg.Station));
                        }
                    }
                }
                /*
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser:message successfully parsed.Station name" + measMsg.Station);
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser:message date:" + measMsg.Time);
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser:message contents:" + measMsg.ToString());
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YourView parser:UseStation:" + UseStationName);
                */

                //                Debug.WriteLine(DateTimeEx.NowToStringWithMs + "\tYourview Parser ends" + data.Substring(0, Math.Min(data.Length, 30)));
                return true;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Error in parsing yourview message." + " data:" + data, ex);
                return false;
            }
        }

        private MeasMsg HandleSpecialField(string varname, string lineValue, MeasMsg measMsg)
        {

            if (varname.StartsWith("<DATE"))
            {
                const int dateStart = 6;
                int dateLen = varname.Length - dateStart - 1;
                return ParseDate(lineValue, varname.Substring(dateStart, dateLen), measMsg);
            }

            if (varname.StartsWith("<TIME"))
            {
                const int timeStart = 6;
                int timeLen = varname.Length - timeStart - 1;
                string format = varname.Substring(timeStart, timeLen).ToLower();
                if (!format.Contains("ss"))
                {
                    format = format + "ss";
                    lineValue = lineValue + "00";
                }
                return ParseTime(lineValue, format, measMsg);
            }
            if (varname == "<STATION NAME>")
                return ParseStatioName(lineValue, false, measMsg);
            if (varname == "<STATION ID>")
                return ParseStatioName(lineValue, true, measMsg);
            return measMsg;
        }

        private static MeasMsg ParseDate(string lineValue, string format, MeasMsg measMsg)
        {
            DateTime dateTime;
            bool ok = DateTime.TryParseExact(lineValue, format, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime);
            if (ok)
            {
                measMsg.Time = dateTime;
                return measMsg;
            }
            ExceptionHandler.RecordException("Yourview parser is unable to parse date " + lineValue + " format " + format);
            return measMsg;
        }

        private MeasMsg ParseTime(string lineValue, string format, MeasMsg measMsg)
        {
            TimeSpan timeSpan;
            bool ok = TimeSpan.TryParseExact(lineValue, format, CultureInfo.InvariantCulture, TimeSpanStyles.None, out timeSpan);
            if (ok)
            {

                if (RoundSecondsToClosestFullMinute)
                {
                    if (timeSpan.Seconds < 30)
                        timeSpan = timeSpan.Add(-TimeSpan.FromSeconds(timeSpan.Seconds));
                    else
                        timeSpan = timeSpan.Add(TimeSpan.FromSeconds(60 - timeSpan.Seconds));
                }
                measMsg.Time = measMsg.Time.Date.Add(timeSpan);

                return measMsg;
            }
            ExceptionHandler.RecordException("Yourview parser is unable to parse time " + lineValue + " format " + format);
            return measMsg;
        }


        private MeasMsg ParseStatioName(string lineValue, bool useStationId, MeasMsg measMsg)
        {
            string sName = lineValue;

            SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "ParseStatioName:" + useStationId + "|" + sName);

            if (useStationId)
            {
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "ParseStatioName 2:" + this.stationMessageNames.ContainsKey(sName));

                if (this.stationMessageNames.ContainsKey(sName))
                    sName = stationMessageNames[sName];
                else
                {
                    var sb = new StringBuilder();
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "YV Station list 2:");
                    foreach (var key in stationMessageNames.Keys)
                        sb.AppendLine(key + ":" + stationMessageNames[key]);
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, sb.ToString());
                }
            }
            if (sName != null)
                measMsg.Station = sName;

            return measMsg;
        }

        private void CheckControlParameters()
        {
            if (StationName == null)
                StationName = string.Empty;
        }

        private static string GetMessageName(string dataLine)
        {
            const string defaultName = "0";
            if (string.IsNullOrWhiteSpace(dataLine))
                return defaultName;

            int start = dataLine.IndexOf(SOH);
            if (start == -1)
                return defaultName;

            if (start == dataLine.Length - 1)
                return defaultName;

            string tmp = dataLine.Substring(start + 1);

            if (string.IsNullOrWhiteSpace(tmp))
                return defaultName;

            int end = tmp.IndexOf(STX);
            if (end < 1)
                return defaultName;

            string result = tmp.Substring(0, end).Trim();

            return result;
        }

    } // class
}
