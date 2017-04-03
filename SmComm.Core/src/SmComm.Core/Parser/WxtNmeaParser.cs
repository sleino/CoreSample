using System;
using System.Collections.Generic;
using SmSimple.Core;
using SmSimple.Core.Util;

namespace SmComm.Core.Parser
{
    internal sealed class WxtNmeaParser
    {

        //2010-01-15 15:27:56	DataReceived $WIXDR,C,23.4,C,  0,  H,  15.8,P,   0,P,1029.6,H,0*7B
        private readonly string[] PTUVars = { "", "", "TA", "", "", "", "RH", "", "", "", "PA", "", "" };
        private readonly string[] WindVars = { "", "", "WDMIN1M", "", "", "", "WDAVG1M", "", "", "", "WDMAX1M", "", "", "", "WSMIN1M", "", "", "", "WSAVG1M", "", "", "", "WSMAX1M", "", "" };
        private readonly string[] RainVars = { "", "", "WXT_Rc", "", "", "", "WXT_Rd", "", "", "", "WXT_Ri", "", "", "", "WXT_Hc", "", "", "", "WXT_Hd", "", "", "", "WXT_Hi", "", "" };
        private readonly string[] DiagVars = { "", "", "VSUPP", "", "" };
        private readonly Dictionary<char, string[]> keyToVariableList = new Dictionary<char, string[]>();

        private readonly string[] WD30WindVars = { "", "WD", "", "WS" };

        // $WIXDR,A,204,D,0,A,219,D,1,A,248,D,2,S,0.9,M,0,S,1.2,M,1,S,1.6,M,2*5A
        // $WIXDR,C,22.7,C,0,H,41.5,P,0,P,1017.7,H,0*79
        // $WIXDR,V,0.02,M,0,Z,10,s,0,R,0.7,M,0,V,0.0,M,1,Z,0,s,1,R,0.0,M,1,R,25.6,M,2,R,0.0,M,3*65

        // A C V
        // $WIXDR,A,204,D,0,A,219,D,1,A,248,D,2,S,0.9,M,0,S,1.2,M,1,S,1.6,M,2,   C,22.7,C,0,C,22.9,C,1,H,41.5,P,0,P,1017.7,H,0,V,0.00,M,0,Z,0,s,0,R,0.7,M,0,V,0.0,M,1,Z,0,s,1,R,0.0,M,1,R,25.6,M,2,R,0.0,M,3,C,22.6,C,2,U,0.0,#,0,U,15.3,V,1*3A
        // $WIXDR,A,204,D,0,A,219,D,1,A,248,D,2,S,0.9,M,0,S,1.2,M,1,S,1.6,M,2*5A C,22.7,C,0,H,41.5,P,0,P,1017.7,H,0*79         V,0.02,M,0,Z,10,s,0,R,0.7,M,0,V,0.0,M,1,Z,0,s,1,R,0.0,M,1,R,25.6,M,2,R,0.0,M,3*65
        private DateTime dateTime;
        const string DefaultStation = "";

        internal WxtNmeaParser()
        {
            keyToVariableList.Add('A', WindVars);
            keyToVariableList.Add('C', PTUVars);
            keyToVariableList.Add('V', RainVars);
            keyToVariableList.Add('U', DiagVars);
            // keyToVariableList.Add('R', WD30WindVars);
        }

        public MeasMsg Parse(string text)
        {

            string[] data = PreProcess(text);
            if (data == null)
                return null;

            if (data.GetLength(0) == 0)
                return null;

            var measMsg = new MeasMsg();
            dateTime = DateTimeEx.Now;
            measMsg.Time = dateTime;
            measMsg.Station = DefaultStation;

            if (CheckForWMT700AndWD30(data, ref measMsg))
                return measMsg;

            char header;
            if (!ParseHeader(data, out header))
                return null;

            if (ParseBody(header, data, ref measMsg))
                return measMsg;

            return null;
        }

        internal bool RoundSecondsToClosestZero { get; set; }

        private bool CheckForWD30(string[] data, ref MeasMsg measMsg)
        {

            const string WD30id = "$WIMWV";
            if (!data[0].StartsWith(WD30id))
                return false;

            return ParseVariables(WD30WindVars, data, ref measMsg);
        }

        private bool CheckForWMT700AndWD30(string[] data, ref MeasMsg measMsg)
        {

            const string WMT700id = "MWV";
            if (string.IsNullOrWhiteSpace(data[0]))
                return false;

            if (data[0][0] != '$')
                return false;

            if (!data[0].EndsWith(WMT700id))
                return false;

            return ParseVariables(WD30WindVars, data, ref measMsg);
        }


        private static string[] PreProcess(string data)
        {
            char[] trimChars = { '\r', '\n' };
            data = data.TrimStart(trimChars);

            const int iMaxElements = 1000;
            char[] separator = { ',' };
            return data.Split(separator, iMaxElements);

        }
        private bool ParseHeader(string[] data, out char header)
        {
            header = ' ';
            const int minLen = 2;
            const int headerPosition = 1;
            if (data.GetLength(0) < minLen)
                return false;

            string tmp = data[headerPosition];
            if (!string.IsNullOrEmpty(tmp))
            {
                header = tmp[0];
                if (keyToVariableList.ContainsKey(header))
                    return true;
                //if ((header == 'A') || (header == 'C') || header == 'V')
                //    return true;
            }

            // other characters may be received when WXT is restarted
            //string error = "Invalid message header character in WXT message :";
            //if (tmp != null)
            //    error += tmp;

            //SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog,error);
            return false;
        }

        private bool ParseBody(char header, string[] data, ref MeasMsg measMsg)
        {
            if (keyToVariableList.ContainsKey(header))
                return ParseVariables(keyToVariableList[header], data, ref measMsg);
            //if (header == 'A')
            //    return ParseVariables(WindVars, data, ref measMsg);
            //if (header == 'C')
            //    return ParseVariables(PTUVars,data, ref measMsg);
            //if (header == 'V')
            //    return ParseVariables(RainVars, data, ref measMsg);
            //if (header == 'U')
            //    return ParseVariables(DiagVars, data, ref measMsg);
            return false;
        }

        private bool ParseVariables(string[] variables, string[] data, ref MeasMsg measMsg)
        {
            try
            {
                int dataLen = data.GetLength(0);
                int varLen = variables.GetLength(0);
                int loopMaxIndex = Math.Min(dataLen, varLen);
                //if (dataLen > varLen)
                //    return false;
                for (var i = 0; i < loopMaxIndex; i++)
                {
                    if (variables[i].Length > 0)
                    {
                        var meas = new Meas(variables[i], dateTime, data[i], MeasStatus.cOK);
                        measMsg.AddMeas(meas);
                        if (variables[i] == "WSAVG1M")
                        {
                            meas = new Meas("WS", dateTime, data[i], MeasStatus.cOK);
                            measMsg.AddMeas(meas);
                        }
                        else if (variables[i] == "WDAVG1M")
                        {
                            meas = new Meas("WD", dateTime, data[i], MeasStatus.cOK);
                            measMsg.AddMeas(meas);
                        }

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException("WXT.ParseVariables", ex);
                return false;
            }
        }
    } // WxtNmeaParser
}
