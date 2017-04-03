using SmSimple.Core;
using SmSimple.Core.Util;
using SmValidation.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SmComm.Core.Parser
{
    // CT25KAM Data message 61
    public sealed class CeiloMsg61Parser : ParserBase
    {

        private string cloudHeightDetectionStatus;
        private string alarmsAndWarningsInformation;
        private string lowestCloudBase;
        private string secondLowestCloudBase;
        private string highestCloudBase;
        private string verticalVisibility;
        private string highestSignal;
        private string alarmsAndWarnings;

        private string skyConditionDetectionStatus;
        private readonly List<string> cloudHeightsBySkycondition = new List<string>();
        private readonly List<string> cloudAmountBySkycondition = new List<string>();


        public const string Id = "CT22061";

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
                MeasMsg measMsg;
                bool ok = Parse(data, out measMsg);
                if (ok)
                {
                    string tmp = measMsg.ToString();
                    if (!string.IsNullOrEmpty(tmp))
                        result.Add(tmp);
                }
                else
                    HandleParsingError("Error in parsing message. CTK parser ", data);

                return result;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, "exception " + ex.Message);
                HandleParsingError("Error in parsing. " + ex.Message, data);
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
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "CeiloMsg61Parser: null or empty input data.");
                    return false;
                }
                progress++;


                char[] charsToTrim = { '\n', '\r', '\x01' };
                data = data.TrimStart(charsToTrim);
                char[] separator = { '\n' };
                var lines = data.Split(separator);
                if (!CheckLines(lines))
                    return false;

                var lineCount = lines.GetLength(0);
                if (lineCount < 2)
                {

                    string error = GetErrorData(data);
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "CeiloMsg61Parser: incomplete message. Lines:" + lines.GetLength(0) + data + " " + error);
                    return false;
                }

                progress++;
                lines[0] = lines[0].Replace("\x01", string.Empty);
                var stationName = lines[0].Trim().Substring(0, 3).Replace("\r", "").Replace("\n", "");
                if (!Validation.IsValidDirectoryName(stationName))
                {
                    var error = GetErrorData(data);
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "CeiloMsg61Parser: invalid station name. Lines:" + lines.GetLength(0) + data + " " + error);
                    return false;
                }
                measMsg.Station = stationName;
                measMsg.Time = DateTimeEx.Now;

                ResetVariables();
                progress++;


                string tmp = lines[1].Trim(charsToTrim);
                if (!ParseSecondLine(tmp))
                    return false;
                progress++;

                measMsg.AddMeas(new Meas("CL_HDSTATUS", measMsg.Time, cloudHeightDetectionStatus, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas("CL_WARNING", measMsg.Time, alarmsAndWarningsInformation, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas("CB1", measMsg.Time, lowestCloudBase, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas("CB2", measMsg.Time, secondLowestCloudBase, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas("CB3", measMsg.Time, highestCloudBase, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas("VV", measMsg.Time, verticalVisibility, MeasStatus.cOK, measMsg.Station));
                measMsg.AddMeas(new Meas("CL_STATUSCODE", measMsg.Time, alarmsAndWarnings, MeasStatus.cOK, measMsg.Station));

                progress++;
                if (lineCount > 2)
                {
                    progress++;
                    ParseThirdLine(lines[2].Trim(charsToTrim));
                    progress++;
                    measMsg.AddMeas(new Meas("CL_SCSTATUS", measMsg.Time, skyConditionDetectionStatus, MeasStatus.cOK, measMsg.Station));
                    if (cloudHeightsBySkycondition.Count > 0)
                        measMsg.AddMeas(new Meas("CL1", measMsg.Time, cloudHeightsBySkycondition[0], MeasStatus.cOK, measMsg.Station));
                    if (cloudHeightsBySkycondition.Count > 1)
                        measMsg.AddMeas(new Meas("CL2", measMsg.Time, cloudHeightsBySkycondition[1], MeasStatus.cOK, measMsg.Station));
                    if (cloudHeightsBySkycondition.Count > 2)
                        measMsg.AddMeas(new Meas("CL3", measMsg.Time, cloudHeightsBySkycondition[2], MeasStatus.cOK, measMsg.Station));
                    if (cloudHeightsBySkycondition.Count > 3)
                        measMsg.AddMeas(new Meas("CL4", measMsg.Time, cloudHeightsBySkycondition[3], MeasStatus.cOK, measMsg.Station));
                    if (cloudHeightsBySkycondition.Count > 4)
                        measMsg.AddMeas(new Meas("CL5", measMsg.Time, cloudHeightsBySkycondition[4], MeasStatus.cOK, measMsg.Station));

                    if (cloudAmountBySkycondition.Count > 0)
                        measMsg.AddMeas(new Meas("SC1", measMsg.Time, cloudAmountBySkycondition[0], MeasStatus.cOK, measMsg.Station));
                    if (cloudAmountBySkycondition.Count > 1)
                        measMsg.AddMeas(new Meas("SC2", measMsg.Time, cloudAmountBySkycondition[1], MeasStatus.cOK, measMsg.Station));
                    if (cloudAmountBySkycondition.Count > 2)
                        measMsg.AddMeas(new Meas("SC3", measMsg.Time, cloudAmountBySkycondition[2], MeasStatus.cOK, measMsg.Station));
                    if (cloudAmountBySkycondition.Count > 3)
                        measMsg.AddMeas(new Meas("SC4", measMsg.Time, cloudAmountBySkycondition[3], MeasStatus.cOK, measMsg.Station));
                    if (cloudAmountBySkycondition.Count > 4)
                        measMsg.AddMeas(new Meas("SC5", measMsg.Time, cloudAmountBySkycondition[4], MeasStatus.cOK, measMsg.Station));

                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Error in parsing ceilo message. Progress:" + progress + " data:" + data, ex);
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
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "CeiloMsg61Parser: message does not contain newline.");
                return false;
            }

            if (lines.GetLength(0) < 1)
            {
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "CeiloMsg61Parser: message contains only header line: " + lines[0]);
                return false;
            }

            if (lines[0].Length < 4)
            {
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "CeiloMsg61Parser: unable to parse ceilo id from header line:" + lines[0]);
                return false;
            }
            return true;
        }

        private bool ParseSecondLine(string secondLine)
        {
            try
            {
                if (secondLine.Length < 2)
                {
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "CeiloMsg61Parser: 2nd line is empty or contains only 1 character:" + secondLine);
                    return false;
                }

                int iDetectionStatus;
                cloudHeightDetectionStatus = secondLine[0].ToString();
                var detectionStatusIsInteger = Validation.TryParseInt(cloudHeightDetectionStatus, out iDetectionStatus);

                alarmsAndWarningsInformation = secondLine[1].ToString();

                char[] spaceSeparator = { ' ' };
                var groups = secondLine.Substring(2).Split(spaceSeparator);

                var groupCount = groups.GetLength(0);
                if (detectionStatusIsInteger)
                {
                    if (iDetectionStatus == 0 || iDetectionStatus == 5) { }
                    else if (iDetectionStatus == 1 && groupCount > 1)
                        lowestCloudBase = groups[1];
                    else if (iDetectionStatus == 2 && groupCount > 2)
                    {
                        lowestCloudBase = groups[1];
                        secondLowestCloudBase = groups[2];
                    }
                    else if (iDetectionStatus == 3 && groupCount > 3)
                    {
                        lowestCloudBase = groups[1];
                        secondLowestCloudBase = groups[2];
                        highestCloudBase = groups[3];
                    }
                    else if (iDetectionStatus == 4 && groupCount > 4)
                    {
                        verticalVisibility = groups[1];
                        highestSignal = groups[2];
                    }
                }

                if (groupCount > 4)
                    alarmsAndWarnings = groups[4];

                return true;
            }
            catch (Exception ex)
            {
                var msg = "Exception parsing second line";
                if (secondLine != null)
                {
                    //var sb = new StringBuilder();
                    //for (var  i = 0; i< secondLine.Length; i++)
                    //   msg.
                    msg = msg + secondLine;
                }

                ExceptionHandler.HandleException(ex, msg);
                return false;
            }
        }

        private void ParseThirdLine(string thirdLine)
        {

            try
            {
                //char[] spaceSeparator = { ' ' };
                if (thirdLine.Length < 3)
                    return;

                /*
                int iDetectionStatus;
                
                skyConditionDetectionStatus = thirdLine.Substring(0, 3).TrimStart();
                bool detectionStatusIsInteger = Validation.TryParseInt(skyConditionDetectionStatus, out iDetectionStatus);
                if (!detectionStatusIsInteger)
                    return;

                if (iDetectionStatus == -1 || iDetectionStatus == 99)
                {
                    return;
                }
                
                cloudAmountBySkycondition.Add(skyConditionDetectionStatus);
                */
                // KOrea

                int indexOfDoubleSpace = thirdLine.IndexOf("  ");
                while (indexOfDoubleSpace > -1)
                {
                    thirdLine = thirdLine.Replace("  ", " ");
                    indexOfDoubleSpace = thirdLine.IndexOf("  ");
                }
                thirdLine = thirdLine.Trim();
                char[] separator = { ' ' };
                var items = thirdLine.Split(separator);
                var len = items.GetLength(0);
                if (len > 10)
                    len = 10;
                int i = 0;
                while (i < len)
                {
                    cloudAmountBySkycondition.Add(items[i]);
                    i++;
                    if (i == len)
                        break;
                    cloudHeightsBySkycondition.Add(items[i]);
                    i++;
                }

            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "Exception in sc parsing. " + thirdLine);
            }
        }


        private void ResetVariables()
        {
            cloudHeightDetectionStatus = string.Empty;
            alarmsAndWarningsInformation = string.Empty;
            lowestCloudBase = "///";
            secondLowestCloudBase = "///";
            highestCloudBase = "///";
            verticalVisibility = "///";
            highestSignal = "///";
            alarmsAndWarnings = string.Empty;

            skyConditionDetectionStatus = string.Empty;
            cloudHeightsBySkycondition.Clear();
            cloudAmountBySkycondition.Clear();
        }


        /*
        private bool GetCell(string data, string cellId, int startIndex, out string cell) 
        {
            cell = string.Empty;    
            try {                
                var searchString = cellId + ":";
                if (startIndex > data.Length - 1)
                    return false;

                var cellIndex = data.IndexOf(searchString, startIndex, StringComparison.InvariantCulture);
                if (cellIndex == -1)
                    return false;
            
                cellIndex = cellIndex + 3;
                var nextIndex = data.IndexOf(":", cellIndex, StringComparison.InvariantCulture);
                if (nextIndex == -1)
                    return false;

                if (nextIndex < cellIndex)
                    return false;

                startIndex = nextIndex;
                int len  = nextIndex - cellIndex;
                if (cellIndex + len > data.Length-1)
                    return false;

                cell = data.Substring(cellIndex, len);
                if (cell.Length < 2)
                    return false;
                cell = cell.Substring(0,cell.Length - 2).Trim();

                return true;
            }
            catch (Exception ex) {
                ExceptionRecorder.RecordException("Exception while parsing data. cellId =" + cellId + "startIndex:" + startIndex + " Data=" + data, ex);
                return false;
            }
        }
        */





    } // class
}
