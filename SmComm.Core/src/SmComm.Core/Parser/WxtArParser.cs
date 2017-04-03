using SmSimple.Core;
using SmSimple.Core.Util;
using System;
using System.Collections.Generic;

namespace SmComm.Core.Parser
{
    /// <summary>
    /// Summary description for WxtArParser.
    /// </summary>
    /// 
    /*
    0R1,Dn=031D,Dm=062D,Dx=092D,Sn=0.0M,Sm=0.1M,Sx=0.2M
    0R5,Th=23.2C,Vh=12.6N,Vs=12.8V,Vr=3.501V
    0R1,Dn=094D,Dm=111D,Dx=142D,Sn=0.0M,Sm=0.1M,Sx=0.1M
    0R1,Dn=090D,Dm=136D,Dx=226D,Sn=0.0M,Sm=0.0M,Sx=0.1M
    0R1,Dn=326D,Dm=080D,Dx=168D,Sn=0.0M,Sm=0.1M,Sx=0.1M
    0R5,Th=23.4C,Vh=12.6N,Vs=12.8V,Vr=3.505V
    0R2,Ta=24.6C,Ua=36.9P,Pa=1027.6H
    0R1,Dn=041D,Dm=078D,Dx=113D,Sn=0.0M,Sm=0.1M,Sx=0.2M
    0R1,Dn=092D,Dm=098D,Dx=103D,Sn=0.0M,Sm=0.1M,Sx=0.2M
    0R1,Dn=133D,Dm=141D,Dx=157D,Sn=0.0M,Sm=0.1M,Sx=0.2M
    0R5,Th=23.4C,Vh=12.6N,Vs=12.9V,Vr=3.501V
    0R1,Dn=114D,Dm=122D,Dx=127D,Sn=0.1M,Sm=0.2M,Sx=0.4M
    0R1,Dn=094D,Dm=095D,Dx=098D,Sn=0.1M,Sm=0.1M,Sx=0.2M
    0R1,Dn=064D,Dm=079D,Dx=094D,Sn=0.0M,Sm=0.1M,Sx=0.2M
    0R5,Th=23.4C,Vh=12.6N,Vs=12.8V,Vr=3.501V
    0R1,Dn=100D,Dm=116D,Dx=141D,Sn=0.0M,Sm=0.1M,Sx=0.2M
    0R1,Dn=082D,Dm=099D,Dx=116D,Sn=0.1M,Sm=0.1M,Sx=0.2M
    0R4,Tr=-224.2#,Ra=0.0M,Sl=0.000247V,Rt=109.4R,Sr=1.173100V
     * 
     * 
     // Tr : aux temperature
     *  Ra: aux rain
     *  SI: snow level
     *  Sr: solar radiation
     *  Rt: pt1000 resistance
    */
    // 
    internal sealed class WxtArParser
    {

        private DateTime dateTime;
        const string DefaultStation = "";

        internal WxtArParser()
        {

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
            measMsg.Station = ParseStation(data[0]);

            //if (CheckForWD30(data, ref measMsg))
            //    return measMsg;

            foreach (var item in data)
                ParseItem(item, measMsg);

            if (measMsg.Count == 0)
                return null;

            return measMsg;
        }

        internal bool RoundSecondsToClosestZero { get; set; }

        // station will be the character(s) before command string 
        // command string = R0, R1,R2, or r0, r1,r2 ,etc
        private string ParseStation(string dataItem1)
        {
            if (string.IsNullOrWhiteSpace(dataItem1))
                return DefaultStation;

            if (dataItem1.Contains("="))
                return DefaultStation;

            // find command response character. 
            int i = dataItem1.LastIndexOf("R");
            if (i < 1)
                i = dataItem1.LastIndexOf("r");
            if (i < 1)
                return DefaultStation;

            return dataItem1.Substring(0, i);
        }

        private readonly Dictionary<string, string> engineeringUnits = new Dictionary<string, string>()
        {
            {"Dn","D"}, // min wd
            {"Dm","D"}, // avg wd
            {"Dx","D"}, // max wd
            {"Sn","M"}, // min ws
            {"Sm","M"}, // avg ws
            {"Sx","M"}, // max ws
            {"Ta","C"},
            {"Tp","C"}, // Internal temperature
            {"Ua","P"},
            {"Pa","H"},
            {"Rc","M"},
            {"Rd","s"},
            {"Ri","M"},
            {"Hc","M"},
            {"Hd","s"},
            {"Hi","M"},
            {"Rp","M"},  // Rain peak intensity
            {"Hp","M"},  // Hail peak intensity
            {"Th","T"},  // Heating temperature
            {"Vs","V"},  // Supply voltage
            {"Vh","V"},  // Heating voltage

             {"Tr","C"},
             {"Sr","V"},
             {"Rt","R"},
             {"Ra","M"},
             {"Sl","V"},
        };

        private readonly Dictionary<string, string> wxtNameToOcName = new Dictionary<string, string>()
        {
            {"Dm","WD"},
            {"Sm","WS"},
            {"Ta","TA"},
            //{"Tp",""},
            {"Pa","PA"},
            {"Ua","RH"},
            {"Rc","WXT_Rc"},
            {"Rd","WXT_Rd"},
            {"Ri","WXT_Ri"},
            {"Hc","WXT_Hc"},
            {"Hd","WXT_Hd"},
            {"Hi","WXT_Hi"},
            //{"Rp","M"},
            //{"Hp","M"},
             //{"Th",""},
             {"Vs","VSUPP"},

             {"Tr","TA2"},
             {"Sr","SR"},
             {"Rt","PTRES"},
             {"Ra","PRAUX"},
             {"Sl","SNOW"},
        };

        private void ParseItem(string dataItem, MeasMsg measMsg)
        {

            //    Debug.WriteLine("Parsing: " + dataItem);
            if (string.IsNullOrWhiteSpace(dataItem))
                return;

            const int minVarNameLen = 2;
            int index = dataItem.IndexOf("=");
            if (index < minVarNameLen)
                return;

            string start = dataItem.Substring(0, index);
            if (string.IsNullOrWhiteSpace(start))
                return;

            // dataItem ends with '='
            if (dataItem.Length < index + 1)
                return;

            string dataValue = dataItem.Substring(index + 1);
            if (string.IsNullOrWhiteSpace(dataValue))
                return;

            ParseDataItem(start, dataValue, measMsg);

        }

        private void ParseDataItem(string start, string dataValue, MeasMsg measMsg)
        {
            if (!engineeringUnits.ContainsKey(start))
                return;
            if (!wxtNameToOcName.ContainsKey(start))
                return;
            int index = dataValue.IndexOf(engineeringUnits[start]);
            if (index == -1)
                return;
            var finalDataValue = dataValue.Substring(0, index);

            var meas = new Meas(wxtNameToOcName[start], dateTime, finalDataValue, MeasStatus.cOK);
            measMsg.AddMeas(meas);
        }



        private static string[] PreProcess(string data)
        {
            char[] trimChars = { '\r', '\n' };
            data = data.TrimStart(trimChars);

            const int iMaxElements = 1000;
            char[] separator = { ',' };
            return data.Split(separator, iMaxElements, StringSplitOptions.RemoveEmptyEntries);

        }
    } // WxtArParser
}
