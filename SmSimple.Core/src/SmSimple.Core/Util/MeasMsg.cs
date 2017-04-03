using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace SmSimple.Core.Util
{

    public class MultiMeasMsg
    {
        private readonly List<MeasMsg> measMsgList;
        private readonly string remainder;

        //public MetVector() {}
        public MultiMeasMsg(List<MeasMsg> mMsgList, string rem)
        {
            measMsgList = mMsgList;
            remainder = rem;
        }

        public IList<MeasMsg> MeasMsgList
        {
            get { return measMsgList; }
            //set {measMsgList=value;}
        }

        public string Remainder
        {
            get { return remainder; }
            //	set {remainder=value;}
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;
            MultiMeasMsg tmp = (MultiMeasMsg)obj;
            if (!remainder.Equals(tmp.Remainder))
                return false;
            return MeasMsgList.Equals(tmp.MeasMsgList);

            //			return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (remainder != null)
                return remainder.GetHashCode();
            return base.GetHashCode();
        }

        public static bool operator ==(MultiMeasMsg left, MultiMeasMsg right)
        {
            if (left.Remainder == right.Remainder)
                return (left.MeasMsgList == right.MeasMsgList);
            return false;
        }

        public static bool operator !=(MultiMeasMsg left, MultiMeasMsg right)
        {
            if (left.Remainder != right.Remainder)
                return true;
            return (left.MeasMsgList != right.MeasMsgList);
        }
    }


    public interface IMeasMsgBase
    {
        string Station { get; set; }
        DateTime Time { get; set; }
        int Channel { get; set; }
    }


    public sealed class MeasMsg : IMeasMsgBase
    {
        private readonly List<Meas> measList = new List<Meas>(128);

        private string station = string.Empty;
        private DateTime obsTime = DateTime.MinValue;
        private const char tab = '\u0009';
        private int channel = int.MinValue;

        public MeasMsg()
        {
            TimeStampFormat = "yyyy-MM-dd HH:mm:ss";
            FileTimeStampFormat = "yyyyMMdd_HHmmss";
        }

        public MeasMsg(MeasMsg measMsg) : this()
        {
            Station = measMsg.Station;
            Time = measMsg.Time;
            foreach (var m in measMsg.MeasList)
                AddMeas(m);
        }

        public int Channel
        {
            get
            {
                if (channel == int.MinValue)
                {
                    ExceptionHandler.HandleException("Invalid channel in measMsg");
                    return 0;
                }
                return channel;
            }
            set { channel = value; }
        }

        public bool Initialise(string sInput)
        {
            if (null == sInput)
                return false;

            string sMeas = string.Empty;
            while (GetNextMeasString(ref sInput, ref sMeas))
            {
                Meas meas = new Meas();
                if (Meas.Initialise(sMeas, out meas))
                    AddMeas(meas);
                else
                {
                    Debug.Assert(true, "Received string was incomplete: " + sMeas);
                }
            }

            if (channel == int.MinValue)
                this.Channel = -1;
            if (Count == 0)
                return false;
            measList.Reverse();
            return true;

            //return (Count>0);	
        }

        public bool InitialiseFromSmallString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            if (input.IndexOf(tab) == -1)
                return false;

            char[] key = { tab };
            var items = input.Split(key);
            var len = items.GetLength(0);
            if (len < 2)
                return false;

            DateTime dateTime;
            if (!DateTimeEx.TryParseStandardFormat(items[1], out dateTime))
                return false;

            this.Station = items[0];
            this.obsTime = dateTime;
            if (channel == int.MinValue)
                this.Channel = -1;

            for (int i = 2; i < len; i++)
            {
                if (string.IsNullOrWhiteSpace(items[i]))
                    continue;
                Meas meas;
                if (Meas.InitialiseFromSmallString(dateTime, Station, items[i], out meas))
                {
                    this.AddMeas(meas);
                }
            }
            return true;
        }


        /// <summary>
        /// Return true if other measMsg has same timestamp, station and measurement names match
        /// </summary>
        /// <param name="measMsg"></param>
        /// <returns></returns>
        public bool CheckForDuplicateFilter(MeasMsg measMsg)
        {


            if (measMsg == null)
                return false;
            if (measMsg.obsTime != this.obsTime)
                return false;
            if (measMsg.Station != this.station)
                return false;
            if (measMsg.Count != this.Count)
                return false;
            for (var i = 0; i < Count; i++)
            {
                string measName = measList[i].Name;
                string dataValueInThisMeasMsg;


                bool ok = this.GetMeasObsValueByName(measName, out dataValueInThisMeasMsg);
                if (!ok)
                {
                    return false;
                }

                string dataValueInTheOtherMeasMsg;
                ok = measMsg.GetMeasObsValueByName(measName, out dataValueInTheOtherMeasMsg);
                if (!ok)
                {
                    return false;
                }

                //if (measName == "Vx3s" && measMsg.Time.Second < 4)
                //    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Vx3s comparison this:" + dataValueInThisMeasMsg + "\t:other:" + dataValueInTheOtherMeasMsg);


                bool thisContainsSlashesButTheOtherOneDoesnt =
                    dataValueInThisMeasMsg.Contains("/") && !dataValueInTheOtherMeasMsg.Contains("/");

                if (thisContainsSlashesButTheOtherOneDoesnt)
                {
                    // if (measName == "Vx3s" && measMsg.Time.Second < 4)
                    //     SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Vx3s thisContainsSlashesButTheOtherOneDoesnt this:" + dataValueInThisMeasMsg + "\t:other:" + dataValueInTheOtherMeasMsg);
                    return false;
                }

                //bool theOtherOneContainsSlashesButThisDoesnt =  !dataValueInThisMeasMsg.Contains("/") && dataValueInTheOtherMeasMsg.Contains("/");

                //if (theOtherOneContainsSlashesButThisDoesnt)
                //{
                //     if (measName == "Vx3s" && measMsg.Time.Second < 4)
                //        SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "Vx3s theOtherOneContainsSlashesButThisDoesnt this:" + dataValueInThisMeasMsg + "\t:other:" + dataValueInTheOtherMeasMsg);
                ////    return true;
                //}

            }
            return true;
        }


        /// <summary>
        /// If this has slashes on some meas, copy value from the other one
        /// If this is missing some meas, copy valuee from the other one
        /// Return negative value if station, obstime, count are different
        /// Return 0 if measMsg contains same data values as this
        /// Return number of modified measValues
        /// </summary>
        /// <param name="measMsg"></param>
        /// <returns></returns>
        public int Merge(MeasMsg measMsg)
        {
            List<Meas> itemsToAdd = new List<Meas>();
            List<Meas> itemsToChange = new List<Meas>();
            int returnValue = 0;
            if (measMsg == null)
                return -1;
            if (measMsg.obsTime != this.obsTime)
                return -2;
            if (measMsg.Station != this.station)
                return -3;
            for (var i = 0; i < Count; i++)
            {
                string measName = measList[i].Name;
                string dataValueInThisMeasMsg;


                bool ok = this.GetMeasObsValueByName(measName, out dataValueInThisMeasMsg);
                if (!ok)
                {
                    return -10 - i;
                }

                string dataValueInTheOtherMeasMsg;
                ok = measMsg.GetMeasObsValueByName(measName, out dataValueInTheOtherMeasMsg);
                if (!ok)
                {
                    continue;
                }

                bool thisContainsSlashesButTheOtherOneDoesnt =
                    dataValueInThisMeasMsg.Contains("/") && !dataValueInTheOtherMeasMsg.Contains("/");

                if (thisContainsSlashesButTheOtherOneDoesnt)
                {
                    Meas tmp = new Meas();
                    bool ok2 = measMsg.GetMeasByName(measName, ref tmp);
                    Debug.Assert(ok2, "Merge 1");
                    //if (tmp.Name == "PA1")
                    //    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Diag, "öåöå Merging PA1. Old value:" + dataValueInThisMeasMsg + " . New value: " + dataValueInTheOtherMeasMsg);
                    itemsToChange.Add(tmp);
                    returnValue++;
                }
            }

            foreach (var meas in itemsToChange)
                this.UpdateMeas(meas);

            for (var i = 0; i < measMsg.Count; i++)
            {
                Meas meas = measMsg.MeasList[i];
                if (!this.ContainsMeas(meas.Name))
                {
                    this.AddMeas(meas);
                    returnValue++;
                }
            }
            return returnValue;
        }

        public void SetStationInEachMeas(string stationName)
        {
            Station = stationName;

            var count = Count;
            for (var i = 0; i < count; i++)
            {
                Meas meas = measList[i];
                MeasList[i] = new Meas(meas.Name, meas.ObsTime, meas.ObsValue, meas.Status, stationName);
            }
        }

        public string Station
        {
            get { return station; }
            set
            {
                if (value != null)
                    station = value;
            }
        }

        public DateTime Time
        {
            get { return obsTime; }
            set
            {
                obsTime = value;
                for (var i = 0; i < Count; i++)
                {
                    //Meas m = measList[i];
                    //m.ObsTime = value;
                    measList[i] = new Meas(measList[i].Name, value, measList[i].ObsValue, measList[i].Status,
                        measList[i].Station);
                }
            }
        }

        public Meas GetMeas(int i)
        {
            return measList[i];
        }

        public bool GetMeasByName(string name, ref Meas meas)
        {
            try
            {
                Debug.Assert(!string.IsNullOrEmpty(name), "GetMeasByName, invalid argument");
                if (string.IsNullOrEmpty(name))
                    return false;

                var m1 = measList.Find(item => item.Name == name);
                if (m1.Name != null && m1.Name == name)
                {
                    meas = m1;
                    return true;
                }

                //foreach (var m1 in measList)
                //    if (m1.Name == name)
                //    {
                //        meas = m1;
                //        return true;
                //    }

                return false;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("GetMeasByName " + ex.Message);
                return false;
            }
        }

        public bool GetMeasObsValueByName(string name, out string obsValue)
        {
            obsValue = string.Empty;
            Meas m = new Meas();
            if (GetMeasByName(name, ref m))
            {
                obsValue = m.ObsValue;
                return true;
            }
            return false;
        }

        public bool GetNumericDoubleObsValueByName(string name, out double obsValue)
        {
            obsValue = 0;

            Meas meas = new Meas();
            if (!GetMeasByName(name, ref meas))
                return false;
            if (!meas.HasDoubleObsValue)
                return false;
            obsValue = meas.DoubleObsValue;
            return true;

            //string stringValue;
            //if (!GetMeasObsValueByName(name, out stringValue))
            //    return false;

            //double dValue;
            //if (!StringUtil.TryParseDouble(stringValue, out dValue))
            //    return false;

            //obsValue = dValue;
            //return true;
        }


        public bool GetNumericObsValueByName(string name, ref double obsValue)
        {
            Meas meas = new Meas();
            if (!GetMeasByName(name, ref meas))
                return false;
            if (!meas.HasDoubleObsValue)
                return false;
            obsValue = meas.DoubleObsValue;
            return true;

            //string stringValue;
            //if (!GetMeasObsValueByName(name, out stringValue))
            //    return false;

            //double dValue;
            //if (!StringUtil.TryParseDouble(stringValue, out dValue))
            //    return false;

            //obsValue = dValue;
            //return true;
        }

        public bool GetNumericObsValueByName(string name, out int obsValue)
        {
            obsValue = 0;
            Meas meas = new Meas();
            if (!GetMeasByName(name, ref meas))
                return false;
            if (!meas.HasDoubleObsValue)
                return false;
            obsValue = (int)meas.DoubleObsValue;
            return true;
            //obsValue = 0;
            //string stringValue = string.Empty;
            //if (!GetMeasObsValueByName(name, out stringValue))
            //    return false;

            //double dValue = 0;
            //if (!StringUtil.TryParseDouble(stringValue, out dValue))
            //    return false;

            //obsValue = (int) dValue;
            //return true;
        }

        public bool GetNumericObsValueByName(string name, out float obsValue)
        {
            obsValue = 0;
            Meas meas = new Meas();
            if (!GetMeasByName(name, ref meas))
                return false;
            if (!meas.HasDoubleObsValue)
                return false;
            obsValue = (float)meas.DoubleObsValue;
            return true;
            //obsValue = 0;
            //string stringValue = string.Empty;
            //if (!GetMeasObsValueByName(name, out stringValue))
            //    return false;

            //float dValue = 0;
            //if (!StringUtil.TryParseFloat(stringValue, out dValue))
            //    return false;

            //obsValue = dValue;
            //return true;
        }

        public int Count
        {
            get { return measList.Count; }
        }

        internal List<Meas> MeasList
        {
            get { return measList; }
        }


        public List<string> MeasNames()
        {
            var measNameList = new List<string>();
            measList.ForEach(m => measNameList.Add(m.Name));
            return measNameList;
        }

        public IEnumerable<Meas> MeasValues
        {
            get { return measList; }
        }


        public void AddMeasMsg(MeasMsg otherMeasMsg)
        {
            foreach (var meas in otherMeasMsg.MeasValues)
            {
                if (!measList.Contains(meas))
                    measList.Add(meas);
            }
        }

        public void AddMeas(string measName, string measValue, DateTime obsTime)
        {
            var meas = new Meas(measName, obsTime, measValue, MeasStatus.cOK, this.Station);
            measList.Add(meas);
        }

        public void AddMeas(string measName, string measValue)
        {
            Debug.Assert(measName != null, "measName was null ");
            Debug.Assert(measValue != null, "measValue was null , name = " + measName);
            if (measValue == null)
                measValue = string.Empty;
            var meas = new Meas(measName, Time, measValue, MeasStatus.cOK, station);

            AddMeas(meas);
        }

        public void AddMeas(Meas meas)
        {
            if (obsTime == DateTime.MinValue)
                obsTime = meas.ObsTime;
            if (string.IsNullOrEmpty(Station) && !string.IsNullOrEmpty(meas.Station))
                Station = meas.Station;
            else if (string.IsNullOrEmpty(meas.Station) && !string.IsNullOrEmpty(Station))
                meas = new Meas(meas.Name, meas.ObsTime, meas.ObsValue, meas.Status, Station);
            measList.Add(meas);
        }


        public void UpdateMeas(string measName, string dataValue)
        {
            if (!ContainsMeas(measName))
                return;

            Meas meas = measList.Find(m => m.Name == measName);
            meas = new Meas(meas, dataValue);
            UpdateMeas(meas);
        }

        public void UpdateMeas(Meas meas)
        {
            for (var i = 0; i < Count; i++)
            {
                if (UpdateMeasByIndex(i, meas))
                    return;
            }
            Debug.Assert(false, "UpdateMeas did not find the meas name in the meas list, name = " + meas.Name);
        }


        public void UpdateMeas(int index, Meas meas)
        {
            if (Count > index)
            {
                UpdateMeasByIndex(index, meas);
                return;
            }

            UpdateMeas(meas);
        }

        private bool UpdateMeasByIndex(int index, Meas meas)
        {
            Meas currentMeas = measList[index];
            if (StringUtil.CompareIC(currentMeas.Name, meas.Name, false))
            {
                currentMeas = new Meas(meas);
                //currentMeas.ObsValue = meas.ObsValue;
                //currentMeas.ObsTime = meas.ObsTime;
                //currentMeas.Station = meas.Station;
                //currentMeas.Status = meas.Status;
                measList[index] = currentMeas;
                return true;
            }
            return false;
        }


        public bool ContainsMeas(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            for (var i = 0; i < Count; i++)
            {
                Meas curMeas = measList[i];
                if (StringUtil.CompareIC(curMeas.Name, name, false))
                    return true;
            }
            return false;
        }

        public bool ContainsMeas(Meas meas)
        {
            Debug.Assert(meas.Name != null, "meas.Name was null ");
            for (var i = 0; i < Count; i++)
            {
                Meas curMeas = measList[i];
                if (StringUtil.CompareIC(curMeas.Name, meas.Name, false))
                    return true;
            }
            return false;
        }

        public bool ContainsMeasWithGivenValue(string measName, string data)
        {
            string tmp = string.Empty;
            if (!GetMeasObsValueByName(measName, out tmp))
                return false;

            return (string.Compare(tmp, data, false) == 0);
        }

        public void UpsertMeasMsg(MeasMsg mm)
        {
            Debug.Assert(mm != null, "mm was null");
            if (mm == null)
                return;
            obsTime = mm.Time;
            station = mm.Station;
            foreach (var m in mm.MeasList)
                UpsertMeas(m);
        }

        public void UpsertMeas(Meas meas)
        {
            if (!ContainsMeas(meas))
                AddMeas(meas);
            else
                UpdateMeas(meas);
        }

        public string CreateFileName
        {
            get
            {
                if (string.IsNullOrEmpty(station))
                    return Path.GetRandomFileName();
                return Station + "_" + Time.ToString(FileTimeStampFormat) + ".txt";
            }
        }

        public string CreateUniqueFileName(string directory)
        {
            string result = CreateFileName;
            string fullName = directory + "\\" + result;
            while (File.Exists(fullName))
            {
                result = Station + "_" + Path.GetRandomFileName() + ".txt";
                fullName = directory + "\\" + result;
            }
            return result;
        }

        private static bool GetNextMeasString(ref string s, ref string sMeas)
        {
            if (s == null || sMeas == null)
                return false;
            int i = s.LastIndexOf("MEAS", StringComparison.Ordinal);
            if (i > -1)
            {
                sMeas = s.Substring(i);
                s = s.Substring(0, i);
                return true;
            }
            return false;
        }

        public string ToSmallString()
        {

            var sb = new StringBuilder();
            sb.Append(station);
            sb.Append(tab);
            sb.Append(obsTime.ToString(TimeStampFormat, CultureInfo.InvariantCulture));
            sb.Append(tab);
            foreach (var m in MeasList)
                sb.Append(m.ToSmallString());
            return sb.ToString();
        }


        public static string TimeStampFormat { get; set; }
        public static string FileTimeStampFormat { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(station);
            sb.Append(obsTime.ToString(TimeStampFormat, CultureInfo.InvariantCulture));
            sb.Append(WriteIntoString(this));
            return sb.ToString();
        }

        internal static string WriteIntoString(MeasMsg measMsg)
        {
            var sb = new StringBuilder();
            foreach (var m in measMsg.MeasList)
                sb.Append(m.ToStdString(measMsg.Station));
            return sb.ToString();
        }


    } // public class MeasMsg

}
