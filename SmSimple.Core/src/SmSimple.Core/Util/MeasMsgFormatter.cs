using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace SmSimple.Core.Util
{
    public static class MeasMsgFormatter
    {
        private const int DynColumnMinLen = 6;
        private static string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static string DateTimeFormat 
        {
            get { return dateTimeFormat; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                dateTimeFormat = value;
            }
        }

#if DEBUG
        // returns string containting MeasValues from measMsgToBeFormatted as defined by the sample measTemplate
        public static string FormatMeasMsg(MeasMsg measMsgToBeFormatted, MeasMsg measTemplate, bool header,
            char dataDelimiter)
        {
            if ((measMsgToBeFormatted == null) || (measTemplate == null))
                return string.Empty;

            var sb = new StringBuilder();
            // header
            if (header)
                sb.Append(GetHeaderByMeasNames(measMsgToBeFormatted, dataDelimiter));

            // body
            foreach (var meas in measTemplate.MeasList)
            {
                //	measTemplate.MeasList.ForEach(delegate(Meas meas) { 				
                Meas measToBeFormatted = new Meas();
                if (measMsgToBeFormatted.GetMeasByName(meas.Name, ref measToBeFormatted))
                    sb.Append(meas.ObsValue);
                sb.Append(dataDelimiter);
                //});
            }
            return sb.ToString();
        }

        public static string FormatMeasMsg(MeasMsg measMsg)
        {
            return MeasMsg.WriteIntoString(measMsg);
        }

        public static string FormatMeasMsgInReverse(MeasMsg measMsg)
        {
            var sb = new StringBuilder();
            for (var i = measMsg.Count - 1; i > -1; i--)
            {
                Meas m = measMsg.MeasList[i];
                sb.Append(m.ToStdString(measMsg.Station));
            }
            return sb.ToString();
        }
#endif

        public static string FormatMeasMsgWithDynCol(MeasMsg measMsg, bool header, char dataDelimiter)
        {
            if (measMsg.Count == 0)
                return string.Empty;


            var sb = new StringBuilder();
            if (header)
                sb.Append(GetVaisalaHeader(measMsg, dataDelimiter, true));

            sb.Append((measMsg.MeasList[0]).ObsTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
            sb.Append(' ');



            var count = measMsg.MeasList.Count;
            for (var i = 0; i < count; i++)
            {
                Meas meas = measMsg.MeasList[i];
                sb.Append(meas.ObsValue);
                if (i < measMsg.MeasList.Count - 1)
                {
                    int numSpaces = GetNumberOfSpacesToAdd(meas);
                    sb.Append(' ', numSpaces);
                }
            }
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        private static int GetNumberOfSpacesToAdd(Meas meas)
        {
            int lenObsValue = meas.ObsValue.Length;
            int lenName = Math.Max(meas.Name.Length, DynColumnMinLen);
            if (lenName > lenObsValue)
                return lenName - lenObsValue + 1;
            return 0;
        }

        public static string FormatMeasMsg(MeasMsg measMsg, bool header, char dataDelimiter)
        {
            if (measMsg.Count == 0)
                return string.Empty;
            var sb = new StringBuilder();
            if (header)
                sb.Append(GetVaisalaHeader(measMsg, dataDelimiter, false));
            sb.Append((measMsg.MeasList[0]).ObsTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
            foreach (var m in measMsg.MeasList)
            {
                sb.Append(dataDelimiter);
                sb.Append(m.ObsValue);
            }
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }


        public static string GetVaisalaHeader(MeasMsg measMsg, char dataDelimiter, bool useDynamicColumns)
        {
            var sb = new StringBuilder();
            sb.Append("TIME");

            // 16 = LEN ("yyyy-MM-dd HH:mm:ss") - LEN("TIME") + 2
            if (useDynamicColumns)
            {
                sb.Append(' ', 16);
                sb.Append(GetHeaderByMeasNamesWithDynCol(measMsg));
            }
            else
                sb.Append(GetHeaderByMeasNames(measMsg, dataDelimiter));
            return sb.ToString();
        }

        private static string GetHeaderByMeasNames(MeasMsg measMsg, char dataDelimiter)
        {
            var sb = new StringBuilder();
            foreach (var m in measMsg.MeasList)
            {
                sb.Append(dataDelimiter);
                sb.Append(m.Name);
            }
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }


        private static string GetHeaderByMeasNamesWithDynCol(MeasMsg measMsg)
        {
            var sb = new StringBuilder();
            foreach (var m in measMsg.MeasList)
            {
                sb.Append(m.Name);
                int spacesToAdd = Math.Max(0, DynColumnMinLen - m.Name.Length) + 1;
                sb.Append(' ', spacesToAdd);
            }
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        /// <summary>
        /// Converts MeasMsg into XML format
        /// </summary>
        /// <param name="measMsg"></param>
        /// <returns></returns>
        public static string ToXml(MeasMsg measMsg)
        {
            var sb = new StringBuilder();
            var xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Encoding = Encoding.UTF8;

            var utf8noBOM = new UTF8Encoding(false);
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = utf8noBOM;

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Measurements");
                writer.WriteElementString("Station", measMsg.Station);
                writer.WriteElementString("Time", DateTimeEx.ToFileStringFormat(measMsg.Time));

                var query = measMsg.MeasList.OrderBy(m => m.Name, StringComparer.Ordinal);
                var list = query.ToList();
                foreach (Meas meas in list)
                {
                    writer.WriteStartElement("Meas");
                    writer.WriteElementString("ID", meas.Name);
                    writer.WriteElementString("Data", meas.ObsValue);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return sb.ToString();
        }



        /// <summary>
        /// Converts MeasMsg into XML format
        /// </summary>
        /// <param name="measMsg"></param>
        /// <returns></returns>
        public static string ToXml(MeasMsg measMsg, Dictionary<string, string> replacementNames)
        {
            StringBuilder sb = new StringBuilder();


            Encoding utf8noBOM = new UTF8Encoding(false);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = utf8noBOM;

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Measurements");
                writer.WriteElementString("Station", measMsg.Station);
                writer.WriteElementString("Time", DateTimeEx.ToFileStringFormat(measMsg.Time));

                var query = measMsg.MeasList.OrderBy(m => m.Name, StringComparer.Ordinal);
                var list = query.ToList();
                foreach (Meas meas in list)
                {
                    string newName = string.Empty;
                    if (replacementNames.ContainsKey(meas.Name))
                        newName = replacementNames[meas.Name];
                    else
                        continue;

                    writer.WriteStartElement("Meas");
                    writer.WriteElementString("ID", newName);
                    writer.WriteElementString("VaisalaID", meas.Name);
                    writer.WriteElementString("ObsTime", DateTimeEx.ToFileStringFormat(meas.ObsTime));
                    writer.WriteElementString("Data", meas.ObsValue);

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return sb.ToString();
        }


        private static readonly List<string> timeSeriesWhichUseReplacementTable = new List<string>() {
            "ST1","ST2", "ST3","ST4"
        };

        private static readonly Dictionary<string, string> replacementTable = new Dictionary<string, string>() {
            { "A","1" },
            { "B","2" },
            { "C","3" },
            { "D","4" },
            { "E","5" },
            { "F","6" },
        };

        public static string ToFortumXml(MeasMsg measMsg, Dictionary<string, string> replacementNames)
        {


            StringBuilder sb = new StringBuilder();
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Encoding = Encoding.UTF8;

            Encoding utf8noBOM = new UTF8Encoding(false);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = utf8noBOM;

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Measurements");
                writer.WriteElementString("Station", measMsg.Station);
                writer.WriteElementString("Time", DateTimeEx.ToFileStringFormat(measMsg.Time));

                var query = measMsg.MeasList.OrderBy(m => m.Name, StringComparer.Ordinal);
                var list = query.ToList();

                foreach (Meas meas in list)
                {
                    string newName = string.Empty;
                    if (replacementNames.ContainsKey(meas.Name))
                        newName = replacementNames[meas.Name];
                    else
                        continue;

                    string dataValue = meas.ObsValue;
                    if (timeSeriesWhichUseReplacementTable.Contains(meas.Name))
                        if (replacementTable.ContainsKey(dataValue))
                            dataValue = replacementTable[dataValue];


                    writer.WriteStartElement("Meas");
                    writer.WriteElementString("ID", newName);
                    writer.WriteElementString("VaisalaID", meas.Name);
                    writer.WriteElementString("ObsTime", DateTimeEx.ToFileStringFormat(meas.ObsTime));
                    writer.WriteElementString("Data", dataValue);

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return sb.ToString();
        }


        public static MeasMsg RenameMeasurements(MeasMsg measMsg, Dictionary<string, string> replacementNames)
        {
            MeasMsg result = new MeasMsg();
            result.Station = measMsg.Station;
            result.Time = measMsg.Time;
            foreach (var meas in measMsg.MeasList)
                if (replacementNames.ContainsKey(meas.Name))
                {
                    string newName = replacementNames[meas.Name];
                    if (newName == "?")
                        newName = meas.Name + "?";

                    Meas meas2 = new Meas(newName, meas.ObsTime, meas.ObsValue, meas.Status);
                    result.AddMeas(meas2);
                }

                else
                {
                    Meas meas2 = new Meas("****" + meas.Name, meas.ObsTime, meas.ObsValue, meas.Status);
                    result.AddMeas(meas2);
                }
            return result;
        }
    } // class
}
