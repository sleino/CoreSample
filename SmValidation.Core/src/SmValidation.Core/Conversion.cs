using System;
using System.Globalization;
using System.Diagnostics;
using SmSimple.Core;
using SmSimple.Core.Util;
using System.Text;
using System.Net;

namespace SmValidation.Core
{
    public static class Conversion
    {
        // return decimal part
        public static bool GetDecimalPart(string valueIn, ref Decimal decimalPart)
        {
            try
            {
                if (valueIn == null)
                    return false;

                int i = valueIn.IndexOf(".", StringComparison.Ordinal);
                if (i < 1)
                {
                    decimalPart = 0M;
                    return true;
                }

                Decimal dValue = Decimal.Parse(valueIn, NumberStyles.Float, CultureInfo.InvariantCulture);
                decimalPart = Math.Abs(dValue - (int)dValue);

                return true;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message + " error getting decimal part of " + valueIn);
                ExceptionRecorder.RecordException(ex.Message + " Assert while converting: " + valueIn);
                return false;
            }
        }

        // return true if valueIn is valid double value (. as decimal separator)
        public static bool IsNumericDataValue(string valueIn)
        {
            try
            {
                double dValue = 0;
                return StringUtil.TryParseDouble(valueIn, out dValue);

            }
            catch (System.ArgumentNullException ex)
            {
                Debug.Assert(false, "Validation.TryParseDouble should never throw. " + valueIn);
                ExceptionRecorder.RecordException("Assert while converting: " + valueIn, ex);
                return false;
            }
        }

        // return true if valueIn is valid double value (. as decimal separator), return value also
        public static bool IsNumericDataValue(string valueIn, ref double dValue)
        {
            return StringUtil.TryParseDouble(valueIn, out dValue);
        }
        public static bool IsNumericDataValue(string valueIn, ref decimal dValue)
        {
            return Decimal.TryParse(valueIn, out dValue);
        }

        public static bool IsNumericDataValue(string valueIn, ref int iValue)
        {
            iValue = 0;
            double dValue = 0;
            if (!StringUtil.TryParseDouble(valueIn, out dValue))
                return false;
            iValue = (int)Math.Round(dValue, 0);
            return true;
        }// IsNumericDataValue()

        public static bool GetIntegerPart(string valueIn, ref int iValue)
        {
            iValue = 0;
            double dValue = 0;
            if (!StringUtil.TryParseDouble(valueIn, out dValue))
                return false;
            if (dValue >= 0)
                iValue = (int)Math.Floor(dValue);
            else
                iValue = (int)Math.Ceiling(dValue);
            return true;
        }// IsNumericDataValue()


        private static bool IsNonNegativeDecimalOrSlash(string itemValue)
        {
            if (itemValue == null) return false;
            if (itemValue.Length == 0) return false;

            char[] chars = itemValue.ToCharArray();
            for (var i = 0; i < chars.GetLength(0); i++)
            {
                if (!(char.IsDigit(chars[i])) && (chars[i] != '/') && (chars[i] != '.'))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDateValue(string valueIn, string format, ref DateTime dtValue)
        {
            try
            {
                if (string.IsNullOrEmpty(valueIn))
                    return false;
                if (string.IsNullOrEmpty(format))
                    return false;

                DateTime result = new DateTime();
                bool validFormat = DateTime.TryParseExact(valueIn, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
                if (validFormat)
                    dtValue = result;

                return validFormat;
            }

            catch (System.FormatException ex)
            {
                Debug.Assert(false, "exception " + ex.Message);
                ExceptionRecorder.RecordException(" Error while converting date value (expecting " + format + "): " + valueIn, ex);
                return false;
            }
        }

        // return true if date is in Format yyyy-MM-dd HH:mm:ss
        public static bool IsDateValue(string valueIn, ref DateTime dtValue)
        {
            try
            {
                if (valueIn == null)
                    return false;
                if (valueIn.IndexOf(@"/") > -1) // this handles most common cases
                    return false;
                if (valueIn.Length == 0) // this handles most common cases
                    return false;

                String Format = "yyyy-MM-dd HH:mm:ss";
                //				CultureInfo en = new CultureInfo("en-US");
                //				DateTime parsedBack =DateTime.ParseExact(valueIn,Format,en.DateTimeFormat);	
                CultureInfo MyCultureInfo = new CultureInfo("fi-FI");
                DateTime result = new DateTime();
                bool validFormat = DateTime.TryParseExact(valueIn, Format, MyCultureInfo, DateTimeStyles.None, out result);
                if (validFormat)
                    dtValue = result;
                MyCultureInfo = null;
                return validFormat;
            }
            catch (System.FormatException ex)
            {
                ExceptionRecorder.RecordException(" Assert while converting date value (expecting YYYY-MM-DD HH:mm:ss): " + valueIn, ex);
                return false;
            }
        }


        public static string RoundNumericalValue(string s, int decimals)
        {
            return RoundNumericalValue(s, decimals, CultureInfo.InvariantCulture);
        }

        // if decimals > 0, round to given number of decimals
        // if decimals ==-1 round to closest 10
        // if decimals ==-2 round to closest 100, etc
        public static string RoundNumericalValue(string s, int decimals, System.IFormatProvider cultureInfo)
        {
            string result = s;
            double dValue = 0;
            if (Conversion.IsNumericDataValue(s, ref dValue))
            {
                var sb = new StringBuilder();
                sb.Append("#########0.");
                if (decimals > 0)
                    sb.Append('0', decimals);
                if ((-9 < decimals) && (decimals < 0))
                {
                    float rounding = (float)Math.Pow(10, -decimals);
                    dValue = Math.Round(dValue / rounding, 0) * rounding;
                }
                result = dValue.ToString(sb.ToString(), cultureInfo);
            }
            else
                result = string.Empty;
            return result;
        }

        public static Meas RoundObsValueIfNumeric(Meas meas, int decimals)
        {

            string newValue = Conversion.RoundNumericalValue(meas.ObsValue, decimals);
            if (newValue == string.Empty)
                newValue = "///";
            return new Meas(meas.Name, meas.ObsTime, newValue, meas.Status, meas.Station);
        }

        public static DateTime RoundMilliSecondsToZero(DateTime dt)
        {
            DateTime newTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            return newTime;
        }

        // return true if address is of format xxx.xxx.xxx.xxx:yyy
        public static bool IsValidTcpIpAddress(string address, ref string ip, ref int port)
        {
            if (String.IsNullOrEmpty(address))
                return false;

            address = address.Trim();

            int index = address.IndexOf(":");
            if (index < 8 || index > 15)
                return false;

            ip = address.Substring(0, index);
            if (!IsValidIpAddress(ip))
                return false;

            if (index > address.Length - 1)
                return false;

            string sPort = address.Substring(index + 1);
            if (!Int32.TryParse(sPort, out port))
                return false;

            return true;

        }

        public static bool IsValidIpAddress(string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address))
                    return false;

                address = address.Trim();
                if (!CheckAddress(address))
                    return false;

                IPAddress test;
                bool ok = IPAddress.TryParse(address, out test);  
                return ok;
            }
            catch (FormatException ex)
            {
                ExceptionRecorder.RecordException("FormatException: " + ex.Message);
                return false;
            }
        }

        private static bool CheckAddress(string address)
        {
            if (address.Length < 7)// 1.1.1.1
                return false;
            else if (address.Length > 15)// 111.111.111.111
                return false;
            else if (address.IndexOf('.') == -1)
                return false;
            else if (address.LastIndexOf('.') == address.IndexOf('.'))
                return false;
            else if (!IsNonNegativeDecimalOrSlash(address))
                return false;

            string delimStr = ".";
            char[] delimiter = delimStr.ToCharArray();
            string[] numbers = address.Split(delimiter, 4);
            if (numbers.GetLength(0) != 4)
                return false;
            return true;
        }

        public static bool LatLonToComponents(string latLon, out int degrees, out int minutes, out decimal seconds)
        {
            degrees = 0;
            minutes = 0;
            seconds = 0;
            decimal dLatLon;
            if (!decimal.TryParse(latLon, out dLatLon))
                return false;
            return LatLonToComponents(dLatLon, out degrees, out minutes, out seconds);
        }

        public static bool LatLonToDecimalDegrees(string latLon, out decimal dDegrees)
        {
            dDegrees = 0;
            int degrees = 0;
            int minutes = 0;
            decimal seconds = 0;
            decimal dLatLon;
            if (!decimal.TryParse(latLon, out dLatLon))
                return false;
            if (!LatLonToComponents(dLatLon, out degrees, out minutes, out seconds))
                return false;

            decimal dMinutes = minutes;
            dDegrees = degrees + dMinutes / 60m + seconds / (3600m);
            return true;
        }


        /// <summary>
        /// Convert from DDDMM.SSSS to degree, minute, second
        /// </summary>
        /// <param name="latLon"></param>
        /// <returns></returns>
        public static bool LatLonToComponents(decimal latLon, out int degrees, out int minutes, out decimal seconds)
        {
            degrees = 0;
            minutes = 0;
            seconds = 0;
            try
            {
                degrees = (int)(latLon / 100);
                decimal positiveDLon = Math.Abs(latLon);
                int positiveLon = (int)positiveDLon;
                minutes = positiveLon - Math.Abs(degrees * 100);
                seconds = ((positiveDLon - (int)positiveDLon) * 60);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex);
                return false;
            }
        }

    }
}
