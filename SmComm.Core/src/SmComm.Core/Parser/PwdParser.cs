using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using SmSimple.Core;
using SmSimple.Core.Util;
using SmValidation.Core;
using System.IO;

namespace SmComm.Core.Parser
{

    // 
    public sealed class PwdMsg7Parser : ParserBase
    {
        //private readonly Dictionary<string, string> ceiloVariableValues = new Dictionary<string, string>();

        private string visibilityAlarm;
        private string alarmsAndWarningsInformation;
        private string visibility1MinAvg;
        private string visibility10MinAvg;
        private string instantPresentWeatherNwsCode;
        private string instantPresentWeather;
        private string presentWeather15m;
        private string presentWeather1h;
        private string precipitationIntensity;
        private string cumulativeWaterSum;
        private string cumulativeSnowSum;
        private string temperature;
        private string backgroundLuminance;


        private readonly char[] invalidChars = Path.GetInvalidPathChars();
        public const string Id = "PW  1";

        private readonly Dictionary<string, string> variableNames = new Dictionary<string, string>
        {
            {"PWD_VISALARM","PWD_VISALARM"},
            {"PWD_WARNING","PWD_WARNING"},
            {"VISAVG1M","VISAVG1M"},
            {"VISAVG10M","VISAVG10M"},
            {"NWSCODE","NWSCODE"},
            {"PW","PW"},
            {"PW15M","PW15M"},
            {"PW1H","PW1H"},
            {"PRFAVG1M","PRFAVG1M"},
            {"PRSUM24H","PRSUM24H"},
            {"SNOWSUM24H","SNOWSUM24H"},
            {"PWD_TA","PWD_TA"},
            {"LMAVG1M","LMAVG1M"},
            {"METARPWCODE","METARPWCODE"},
            {"METARRWCODE","METARRWCODE"},
        };

        public string GetVariableName(string key)
        {
            if (variableNames.ContainsKey(key))
                return variableNames[key];
            return string.Empty;
        }

        public void SetVariableName(string key, string name)
        {
            if (variableNames.ContainsKey(key))
                variableNames[key] = name;
            else
                Debug.Assert(false, "You cannot add new keys into PwdMsg7Parser dictionary." + key);
        }


        public override List<string> ParseMessage(string data)
        {
            var result = new List<string>();
            if (data == null)
            {
                Debug.Assert(data != null, "null data in Parse Message");
                return result;
            }
            errorText = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(data))
                    return result;

                MeasMsg measMsg;
                bool ok = Parse(data, out measMsg);
                if (ok)
                {
                    string tmp = measMsg.ToString();
                    if (!string.IsNullOrEmpty(tmp))
                        result.Add(tmp);
                }
                else
                    HandleParsingError("Error in parsing message Pwd parser", data.ReplaceSohStxEtx());

                return result;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, "exception " + ex.Message);
                HandleParsingError("Error in parsing PWDParser. " + ex.Message + " ", data);
                return result;
            }
        }


        new internal bool Parse(string data, out MeasMsg measMsg)
        {
            var progress = 0;
            measMsg = new MeasMsg();
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "PwdMsg7Parser: null or empty input data.");
                    return false;
                }
                progress++;


                int indexOfSOH = data.IndexOf('\x01');
                if (indexOfSOH > -1)
                    data = data.Substring(indexOfSOH);

                char[] charsToTrim = { '\n', '\r' };
                data = data.TrimStart(charsToTrim);
                char[] separator = { '\n' };
                var lines = data.Split(separator);
                if (!CheckLines(lines))
                    return false;


                if (lines.GetLength(0) < 3)
                {
                    var error = GetErrorData(data);
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "PwdMsg7Parser: incomplete message. Lines:" + lines.GetLength(0) + data + " " + error);
                    return false;
                }

                progress++;

                string pw1 = lines[0].Substring(0, 5).Trim(invalidChars).Replace("\r", "").Replace("\n", "").Replace('\x01', ' ').Replace('\x02', ' ').Replace(" ", string.Empty);
                if (!Validation.IsValidFileName(pw1) || !Validation.IsValidDirectoryName(pw1))
                {
                    var error = GetErrorData(data);
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "PwdMsg7Parser: invalid station name. Lines:" + lines.GetLength(0) + data + " " + error);
                    return false;
                }
                measMsg.Station = pw1;
                measMsg.Time = DateTimeEx.Now;

                ResetVariables();
                progress++;

                const char STX = '\x02';
                var stxIndex = lines[0].IndexOf(STX);
                if (stxIndex < 0)
                    return false;
                var tmp = lines[0].Substring(stxIndex + 1).Trim(charsToTrim);
                if (!ParseDataItems(tmp))
                    return false;
                progress++;

                measMsg.AddMeas(new Meas(GetVariableName("PWD_VISALARM"), measMsg.Time, visibilityAlarm, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("PWD_WARNING"), measMsg.Time, alarmsAndWarningsInformation, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("VISAVG1M"), measMsg.Time, visibility1MinAvg, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("VISAVG10M"), measMsg.Time, visibility10MinAvg, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("NWSCODE"), measMsg.Time, instantPresentWeatherNwsCode, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("PW"), measMsg.Time, instantPresentWeather, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("PW15M"), measMsg.Time, presentWeather15m, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("PW1H"), measMsg.Time, presentWeather1h, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("PRFAVG1M"), measMsg.Time, precipitationIntensity, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("PRSUM24H"), measMsg.Time, cumulativeWaterSum, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("SNOWSUM24H"), measMsg.Time, cumulativeSnowSum, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("PWD_TA"), measMsg.Time, temperature, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas(GetVariableName("LMAVG1M"), measMsg.Time, backgroundLuminance, MeasStatus.cOK, measMsg.Station));


                progress++;

                if (lines.GetLength(0) > 1)
                {
                    progress++;
                    string line = (lines[1].Trim(charsToTrim));
                    progress++;
                    measMsg.AddMeas(new Meas(GetVariableName("METARPWCODE"), measMsg.Time, line, MeasStatus.cOK, measMsg.Station));
                }
                if (lines.GetLength(0) > 2)
                {
                    progress++;
                    string line = (lines[2].Trim(charsToTrim));
                    progress++;
                    measMsg.AddMeas(new Meas(GetVariableName("METARRWCODE"), measMsg.Time, line, MeasStatus.cOK, measMsg.Station));
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Error in parsing pwd message. Progress:" + progress + " data:" + data, ex);
                return false;
            }
        }

        private static string GetErrorData(string data)
        {
            var sb = new StringBuilder();
            foreach (var c in data)
            {
                var i = Convert.ToInt32(c);
                sb.Append(i);
                sb.Append(";");
            }
            return sb.ToString();
        }

        private static bool CheckLines(string[] lines)
        {
            if (lines == null)
            {
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "PwdMsg7Parser: message does not contain newline.");
                return false;
            }

            if (lines.GetLength(0) < 1)
            {
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "PwdMsg7Parser: message contains only header line: " + lines[0]);
                return false;
            }

            if (lines[0].Length < 4)
            {
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "PwdMsg7Parser: unable to parse sensor id from header line:" + lines[0]);
                return false;
            }
            return true;
        }

        private bool ParseDataItems(string line)
        {
            try
            {
                if (line.Length < 2)
                {
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "PwdMsg7Parser: line is empty or contains only 1 character:" + line);
                    return false;
                }

                //int iDetectionStatus;
                visibilityAlarm = line[0].ToString();
                //               bool detectionStatusIsInteger = Validation.TryParseInt(visibilityAlarm, out iDetectionStatus);

                alarmsAndWarningsInformation = line[1].ToString();

                var remainingLine = line.Substring(2);
                while (remainingLine.Contains("  "))
                    remainingLine = remainingLine.Replace("  ", " ").Trim();

                char[] spaceSeparator = { ' ' };
                var dataItems = remainingLine.Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);

                var itemCount = dataItems.GetLength(0);
                if (itemCount > 0)
                    visibility1MinAvg = dataItems[0];
                if (itemCount > 1)
                    visibility10MinAvg = dataItems[1];
                if (itemCount > 2)
                    instantPresentWeatherNwsCode = dataItems[2];
                if (itemCount > 3)
                    instantPresentWeather = dataItems[3];
                if (itemCount > 4)
                    presentWeather15m = dataItems[4];
                if (itemCount > 5)
                    presentWeather1h = dataItems[5];
                if (itemCount > 6)
                    precipitationIntensity = dataItems[6];
                if (itemCount > 7)
                    cumulativeWaterSum = dataItems[7];
                if (itemCount > 8)
                    cumulativeSnowSum = dataItems[8];
                if (itemCount > 9)
                    temperature = dataItems[9];
                if (itemCount > 10)
                    backgroundLuminance = dataItems[10];


                return true;
            }
            catch (Exception ex)
            {
                var msg = "Exception parsing data items on first line";
                if (line != null)
                {
                    //var sb = new StringBuilder();
                    //for (var  i = 0; i< secondLine.Length; i++)
                    //   msg.
                    msg = msg + line;
                }

                ExceptionHandler.HandleException(ex, msg);
                return false;
            }
        }



        private void ResetVariables()
        {
            visibilityAlarm = string.Empty;
            alarmsAndWarningsInformation = string.Empty;
            visibility1MinAvg = "///";
            visibility10MinAvg = "///";
            instantPresentWeatherNwsCode = "///";
            instantPresentWeather = "///";
            presentWeather15m = "///";



            presentWeather1h = "///";
            precipitationIntensity = "///";
            cumulativeWaterSum = "///";
            cumulativeSnowSum = "///";
            temperature = "///";
            backgroundLuminance = "///";
        }




    } // class
}
