using SmSimple.Core.Attributes;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SmSimple.Core.Util
{
    public enum MeasStatus
    {
        cOK,
        cNOT_OK
    }

    [Immutable]
    public struct Meas
    {
        private readonly string name;
        private readonly DateTime obsTime;
        private readonly string obsValue;
        private readonly double doubleObsValue;
        private readonly bool hasDoubleObsValue;

        private readonly MeasStatus status;
        private readonly string station;

        public Meas(Meas meas)
            : this(meas.Name, meas.obsTime, meas.obsValue, meas.doubleObsValue,meas.hasDoubleObsValue, meas.status, meas.station)
        {
        }

        public Meas(Meas meas, double newValue)
            : this(meas.Name, meas.obsTime, newValue.ToString(CultureInfo.InvariantCulture), meas.status, meas.station)
        {
        }

        public Meas(Meas meas, string newValue)
            : this(meas.Name, meas.obsTime, newValue, meas.status, meas.station)
        {
        }

        public Meas(string sName, DateTime dtObsTime, string sObsValue, MeasStatus msStatus, string station)
        {
            Debug.Assert(sObsValue != null, "Meas sObsValue is null");
            name = sName;
            obsTime = dtObsTime;
            obsValue = FilterDataValue(sObsValue);
            status = msStatus;
            this.station = station;
            hasDoubleObsValue =  StringUtil.TryParseDouble(sObsValue, out doubleObsValue);
        }

        public Meas(string sName, DateTime dtObsTime, string sObsValue, double dObsValue, bool hasDoubleObsValue, MeasStatus msStatus, string station) {
            name = sName;
            obsTime = dtObsTime;
            obsValue = FilterDataValue(sObsValue);
            status = msStatus;
            this.station = station;
            doubleObsValue = dObsValue;
            this.hasDoubleObsValue = hasDoubleObsValue;
        }

        public Meas(string sName, DateTime dtObsTime, string sObsValue, MeasStatus msStatus)
            : this(sName, dtObsTime, sObsValue, msStatus, string.Empty)
        {
        }

        public Meas(string sName, DateTime dtObsTime, double dObsValue, MeasStatus msStatus)
            : this(sName, dtObsTime, dObsValue.ToString(CultureInfo.InvariantCulture), msStatus, string.Empty)
        {
        }

        internal static bool InitialiseFromSmallString(DateTime dateTime, string station, string input, out Meas meas)
        {
            meas = new Meas();

            const char separator1 = '|';
            if (string.IsNullOrWhiteSpace(input))
                return false;
            if (input.IndexOf(separator1) == -1)
                return false;

            const int numberOfFields = 2;

            char[] delimiter = { separator1 };
            string[] fields = input.Split(delimiter);
            if (fields.GetLength(0) != numberOfFields)
                return false;

            string name = fields[0];
            string dataValue = fields[1];

            meas = new Meas(name,dateTime, dataValue, MeasStatus.cOK, station);
           
            return true;

        }

        internal Meas FilterObsValue()
        {
            return new Meas(name, obsTime, FilterDataValue(obsValue), status, station);
        }

        public static string FilterDataValue(string dataValue)
        {
            return dataValue.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty).Trim();
        }




        internal static bool Initialise(string sInput, out Meas meas)
        {
            meas = new Meas();
            try
            {
                if (null == sInput)
                    return false;

                const string cMeas = "MEAS";
                if (sInput.IndexOf(cMeas) != 0)
                    return false;

                return InitialiseFields(sInput, out meas);
            }
            catch (System.ArgumentNullException)
            {
                ExceptionRecorder.RecordException("Null argument in meas initialisation, string =" + sInput);
                return false;
            }
            catch (System.ArgumentException)
            {
                Debug.Assert(false, "Check date format in meas " + sInput);
                ExceptionRecorder.RecordException("Argument exception in meas initialisation, string =" + sInput);
                return false;
            }
            catch (System.FormatException)
            {
                ExceptionRecorder.RecordException("Format exception in meas initialisation, string =" + sInput);
                return false;
            }
            catch (System.OverflowException)
            {
                ExceptionRecorder.RecordException("Overflow exception in meas initialisation, string =" + sInput);
                return false;
            }
        }

        private static bool InitialiseFields(string sInput, out Meas meas)
        {
            meas = new Meas();

            const int numberOfTabs = 10;
            const string delimStr = "\t";
            char[] delimiter = delimStr.ToCharArray();
            string[] fields = sInput.Split(delimiter);
            if (fields.GetUpperBound(0) != numberOfTabs)
                return false;

            //	station = fields[1];
            //	name = fields[4];

            DateTime obsTime;
            if (!ParseDateTime(fields[7], out obsTime))
                return false;
            //	obsValue = fields[8];

            MeasStatus status = MeasStatus.cNOT_OK;
            if (string.Compare(fields[9], "0", false) == 0)
                status = MeasStatus.cOK;
            meas = new Meas(fields[4], obsTime, fields[8], status, fields[1]);
            return true;
        }

        private static bool ParseDateTime(string rawString, out DateTime dateTime)
        {
            dateTime = DateTime.MinValue;
            try
            {
                const string dateDelimStr = "- :";
                char[] dateDelimiter = dateDelimStr.ToCharArray();
                string[] dateFields = rawString.Split(dateDelimiter);
                if (dateFields.GetLength(0) != 6)
                    return false;

                int year;
                int month;
                int day;
                int hour;
                int minute;
                int seconds;
                if (!int.TryParse(dateFields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out year))
                    return false;
                if (!int.TryParse(dateFields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out month))
                    return false;
                if (!int.TryParse(dateFields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out day))
                    return false;
                if (!int.TryParse(dateFields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out hour))
                    return false;
                if (!int.TryParse(dateFields[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out minute))
                    return false;
                if (!int.TryParse(dateFields[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds))
                    return false;

                if (year < 1 || year > 2100)
                    return false;
                if (month < 1 || month > 12)
                    return false;
                if (day < 1 || day > 31)
                    return false;
                if (hour < 0 || hour > 23)
                    return false;
                if (minute < 0 || minute > 59)
                    return false;
                if (seconds < 0 || seconds > 59)
                    return false;
                dateTime = new DateTime(year, month, day, hour, minute, seconds);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException("Exception while parsing date " + rawString, ex);
                return false;
            }
        }

        public DateTime ObsTime
        {
            get { return obsTime; } /*set {obsTime = value;} */
        }

        public string ObsValue
        {
            get { return obsValue; } /*set {obsValue = value;}*/
        }

        public double DoubleObsValue {
            get { 
                Debug.Assert(hasDoubleObsValue, "hasDoubleObsValue");
                return doubleObsValue; 
            }
        }

        public bool HasDoubleObsValue {
            get {
                return hasDoubleObsValue;
            }
        }

        public string Station
        {
            get { return station; } /*set { station = value; } */
        }

        public string Name
        {
            get { return name; } /* set {name = value;} */
        }

        public MeasStatus Status
        {
            get { return status; } /*set {status = value;}*/
        }

        //private string StatusDesc {get {if (Status == MeasStatus.cOK) return "OK"; return "FAIL"; }}

        //public bool HasNumericObsValue(ref double dValue) {
        //    return (SimpleConversions.IsNumericDataValue(obsValue, ref dValue)); }

        public int Compare(Meas otherMeas)
        {
            int tmp = string.Compare(this.Name, otherMeas.Name, false);
            if (tmp != 0)
                return tmp;

            if (otherMeas.ObsTime < this.ObsTime)
                return 1;

            if (otherMeas.ObsTime > this.ObsTime)
                return -1;

            tmp = string.Compare(this.ObsValue, otherMeas.ObsValue, false);
            if (tmp != 0)
                return tmp;

            return (string.Compare(
                ToStdString(string.Empty),
                otherMeas.ToStdString(string.Empty), 
                false
                ));

        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            //if (object.ReferenceEquals(this, obj)) return true;
            if (GetType() != obj.GetType()) 
                return false;

            Meas otherMeas = (Meas)obj;

            if (string.Compare(this.Name, otherMeas.Name, false) != 0)
                return false;

            if (otherMeas.ObsTime != this.ObsTime)
                return false;

            if (otherMeas.ObsValue != this.ObsValue)
                return false;

            return (string.Compare(ToStdString(string.Empty),
                otherMeas.ToStdString(string.Empty), false) == 0);
        }

        public static bool operator ==(Meas left, Meas right)
        {
            return (left.name == right.name);
        }

        public static bool operator !=(Meas left, Meas right)
        {
            return (left.name != right.name);
        }

        public bool HasNumericDataValue(out double numericValue)
        {
            bool ok = (StringUtil.TryParseDouble(obsValue, out numericValue));
            return ok;
        }

        public bool HasNumericDataValue(out float numericValue)
        {
            //	double dValue = 0;
            bool result = float.TryParse(obsValue, out numericValue);
            //if (result)
            //    numericValue = dValue;
            return result;
        }

        public override int GetHashCode()
        {
            return ToStdString(string.Empty).GetHashCode();
        }

        public override string ToString()
        {
            return ToStdString(string.Empty);
        }

        public string ToStdString(string measMsgStation)
        {
            //			char cr =  '\u000d';
            //			char lf = '\u000a';
            char tab = '\u0009';
            var sb = new StringBuilder();
            sb.Append("MEAS");
            sb.Append(tab);
            if (string.IsNullOrEmpty(Station))
                sb.Append(measMsgStation);
            else
                sb.Append(Station);
            sb.Append(tab); // sitename would be after this line
            sb.Append(tab); // devicename would be after this line
            sb.Append(tab);
            sb.Append(Name);
            sb.Append(tab);
            sb.Append("NONE");
            sb.Append(tab);
            sb.Append(tab);
            sb.Append(ObsTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            sb.Append(tab);
            sb.Append(ObsValue);
            sb.Append(tab);
            sb.Append((int) Status);
            sb.Append(tab);
            sb.Append(Status);
            //sb.Append(tab);
            sb.Append("\r");
            sb.Append("\n");
            return sb.ToString();
        }

        public string ToSmallString() {
            char separator1 = '|';
            
            var sb = new StringBuilder();
            sb.Append(Name);
            sb.Append(separator1);
            sb.Append(ObsValue);
            sb.Append("\t");
            return sb.ToString();
        }
    } // public class Meas
}