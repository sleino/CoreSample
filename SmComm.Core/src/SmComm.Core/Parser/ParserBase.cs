using SmComm.Core.IO;
using SmSimple.Core;
using SmSimple.Core.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SmComm.Core.Parser
{
    public sealed class ParserEventArgs : EventArgs
    {

        private readonly INamedPort virtualPort;
        private readonly string errorDescription;


        public ParserEventArgs(string errorDescription, INamedPort virtualPort)
        {
            this.virtualPort = virtualPort;
            this.errorDescription = errorDescription;
        }

        public INamedPort VirtualPort { get { return virtualPort; } }
        public string ErrorDescription { get { return errorDescription; } }
    }


    public interface IMultiMeasMsgParser
    {
        MeasMsg ParseMessageIntoMeasMsg(string data);
    }


    public abstract class ParserBase
    {

        protected static string errorText { get; set; }
        protected char mainSeparatorChar { get; set; }
        public int indexOfFirstValue { get; set; }

        private string defaultStationName = "S1";
        public string DefaultStationName { get { return defaultStationName; } set { defaultStationName = value; } }

        private bool writeParserErrorsIntoLog = true;
        internal bool WriteParserErrorsIntoLog { get { return writeParserErrorsIntoLog; } set { writeParserErrorsIntoLog = value; } }


        //public bool UseCrc32 { get; set; }
        //public int CrcPolynomial { get; set; }
        //public int CrcSeed { get; set; }

        public abstract List<string> ParseMessage(string data);

        protected INamedPort virtualPort;
        public MeasMsg ParseMessageIntoMeasMsg(string data, INamedPort virtualPort = null)
        {
            this.virtualPort = virtualPort;

            IList<string> sc = ParseMessage(data);
            var measMsg = new MeasMsg();
            var sb = new StringBuilder();
            foreach (var s in sc)
                sb.Append(s);

            if (!measMsg.Initialise(sb.ToString()))
                return null;

            if (RoundSecondsToClosestFullMinute)
            {
                var timeSpan = measMsg.Time.TimeOfDay;
                if (timeSpan.Seconds < 30)
                    timeSpan = timeSpan.Add(-TimeSpan.FromSeconds(timeSpan.Seconds));
                else
                    timeSpan = timeSpan.Add(TimeSpan.FromSeconds(60 - timeSpan.Seconds));
                measMsg.Time = measMsg.Time.Date.Add(timeSpan);
            }
            return measMsg;
        }

        public bool Parse(string data, out MeasMsg measMsg)
        {
            measMsg = new MeasMsg();

            try
            {
                if (string.IsNullOrWhiteSpace(data))
                    return false;

                measMsg = ParseMessageIntoMeasMsg(data, null);
                return true;
            }

            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, StringManager.DoNotTranslate("Exception while parsing message ") + data);
                return false;
            }
        }

        protected void HandleParsingError(string error, string messageBeingParsed = "")
        {

            var e = new ParserEventArgs(error, this.virtualPort);
            ParserAssistant.portListenerEventHandler.ProcessParsingError(e);

            if (!WriteParserErrorsIntoLog)
                return;

            SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, error);
        }

        public bool RoundSecondsToClosestFullMinute { get; set; }

    }

    public abstract class ParserBase2 : ParserBase
    {

        private string[] PreProcess(string data)
        {
            char[] trimChars = { '\r', '\n' };
            Debug.Assert(data != null, "null data in Parse Message");
            var openC = data.IndexOf('(');
            if (openC > 0)
                data = data.Substring(openC);

            data = data.Replace(
                "(", string.Empty).Replace(
                ")", string.Empty).Replace(
                ":;", ":/;").Replace(
                ":,;", ":/;").TrimStart(trimChars);
            while (StringUtil.IndexOf(data, "  ") > -1)
                data = data.Replace("  ", " ");
            //data = data.Replace(")", string.Empty);
            //data = data.Replace(":;", ":/;");
            //data = data.Replace(":,;", ":/;");

            //while (data.IndexOf("  ") > -1)
            //    data = data.Replace("  ", " ");

            const int iMaxElements = 1000;
            //  mainSeparatorChar = ';';
            char[] separator = { mainSeparatorChar };
            var result = data.Split(separator, iMaxElements);

            // SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, "Preprosessing." + result.GetLength(0) + mainSeparatorChar + "." + data);

            return result;
        }


        public override List<string> ParseMessage(string data)
        {
            if (data == null)
            {
                Debug.Assert(data != null, "null data in Parse Message");
                return new List<string>();
            }
            errorText = string.Empty;

            try
            {
                var values = PreProcess(data);
                var result = new List<string>();
                var ok = ParseValues(values, ref result);
                if (!ok)
                    HandleParsingError("Error in parsing message ", data);

                return result;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, "exception " + ex.Message);
                HandleParsingError("Error in parsing. " + ex.Message, data);
                return new List<string>();
            }
        }
        private bool ParseValues(string[] values, ref List<string> result)
        {

            string stationName = string.Empty;
            if (!ParseStationName(values, ref stationName))
            {
                HandleParsingError("Error in parsing station name " + errorText /*+ data */);
                return false;
            }

            string date;
            if (!ParseDate(values, out date))
            {
                HandleParsingError("Error in parsing date " + errorText /*+ data*/);
                return false;
            }

            string time;
            if (!ParseTime(values, out time))
            {
                HandleParsingError("Error in parsing time " + errorText /*+ data*/);
                return false;
            }

            // "MEAS\tStation01\t\t\tTA\tNONE\t\t2005-12-30 18:30:44\t-0.2\t1\t0\r\n";
            string sStation = string.Format(CultureInfo.InvariantCulture, "MEAS\t{0}\t\t\t", stationName);
            string sDate = string.Format(CultureInfo.InvariantCulture, "{0} {1}", date, time);

            ParseBody(sStation, sDate, values, ref result);
            return true;
        }

        private void ParseBody(string sStation, string sDate, string[] values, ref List<string> result)
        {

            for (var i = indexOfFirstValue; i < values.GetUpperBound(0) + 1; i++)
            {
                var curValue = values[i];
                var variable = string.Empty;
                var varValue = string.Empty;

                var semicolonAfterLastValue = ((curValue.Length == 0) && (i == values.GetUpperBound(0)));
                if (!semicolonAfterLastValue)
                    if (ParseValue(curValue, i, ref variable, ref varValue))
                    {
                        var sb = new StringBuilder();
                        sb.Append(sStation);
                        sb.Append(variable);
                        sb.Append("\tNONE\t\t");
                        sb.Append(sDate);
                        sb.Append("\t");
                        sb.Append(varValue);
                        sb.Append("\t0\t0\r\n");
                        result.Add(sb.ToString());
                    }
                    else
                    {
                        HandleParsingError("Error in parsing " + curValue + " " + errorText);
                        return;
                    }
            }
        }


        protected abstract bool ParseStationName(string[] values, ref string stationName);
        protected abstract bool ParseDate(string[] values, out string date);
        protected abstract bool ParseTime(string[] values, out string time);
        protected abstract bool ParseValue(string curValue, int count, ref string variable, ref string varValue);

    }
}
