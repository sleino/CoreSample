using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;

namespace SmSimple.Core.Util
{
    public static class StringUtil
    {
        public static bool CompareIC(System.String a, System.String b, System.Boolean ignoreCase)
        {
            if (!ignoreCase)
                return (String.CompareOrdinal(a, b) == 0);

            return
                (string.Compare(a, b, ignoreCase) == 0);
        }

        public static string DateToStdString(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string FormatDouble(double d, int decimals)
        {
            string format = "{0:N" + decimals + "}";
            return string.Format(format, d);
        }

        public static bool StringCheck(string s)
        {
            if (s == null) return false;
            if (s.Length == 0) return false;
            return true;
        }

        // return -1 if strigns are similar
        // return integer that indicates first differing char
        public static int Diff(string s1, string s2)
        {
            if ((s1 == null) && (s2 == null)) return -1;
            if ((s1 != null) && (s2 == null)) return 0;
            if ((s1 == null) && (s2 != null)) return 0;

            if (String.CompareOrdinal(s1, s2) == 0)
                return -1;

            int len = s1.Length;
            if (s2.Length < len)
                len = s2.Length;

            int i = 0;
            while (i < len)
            {
                if (s1[i] != s2[i])
                    return i;
                i++;
            }

            return len;
        }

        public static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static string SubString(string s, int maxLen)
        {
            if (s == null)
                return string.Empty;

            if (s.Length < maxLen)
                return s;

            return s.Substring(0, maxLen);
        }

        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            return new string((value ?? string.Empty).Skip(startIndex).Take(length).ToArray());
        }

        //public static int CompareInvariantCulture(string strA, string strB)
        //{
        //    return string.Compare(strA, strB, CultureInfo.InvariantCulture, CompareOptions.None);
        //}

        public static int IndexOf(string s, string s2)
        {
            return s.IndexOf(s2,StringComparison.Ordinal);
        }

        public static string[] Split(string text, char separator, StringSplitOptions options = StringSplitOptions.None)
        {
            char[] sep = {separator};
            return text.Split(sep, options);
        }

        public static string[] Split(string text, string separator, StringSplitOptions options = StringSplitOptions.None)
        {
            string[] sep = {separator};
            return text.Split(sep, options);
        }

        private static readonly System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

        public static string ByteArrayToString(byte[] dBytes)
        {
            return encoding.GetString(dBytes);
        }

        public static byte[] StringToByteArray( string data)
        {
            return encoding.GetBytes(data);
        }

        public static string ByteArrayToString2(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append(b.ToString("X").PadLeft(2, '0'));
            return sb.ToString();
        }

        public static byte[] HexStringToByteArray2(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[] BinaryStringToByteArray2(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 8), 2))
                             .ToArray();
        }

        //public static byte[] GetBytes(string str)
        //{
        //    byte[] bytes System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length); = new byte[str.Length * sizeof(char)];
        //   
        //    return bytes;
        //}

        //public static string GetString(byte[] bytes)
        //{
        //    char[] chars = new char[bytes.Length / sizeof(char)];
        //    System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        //    return new string(chars);
        //}


        public static bool TryParseFloat(string data, out float fValue)
        {
            // Exponential format is allowed. 
            bool result = float.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out fValue);
            if (!result)
                return false;

            if (Double.IsNaN(fValue))
                return false;

            return true;
        }



        public static bool TryParseDoubleAllowExp(string data, out double dValue)
        {
            // Exponential format is allowed. 
            //bool result = double.TryParse(data, NumberStyles.Number, CultureInfo.InvariantCulture, out dValue);
            bool result = double.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out dValue);
            if (!result)
                return false;

            if (Double.IsNaN(dValue))
                return false;

            return true;
        }

        public static bool TryParseDecimal(string data, out decimal dValue)
        {
            // Exponential format is not allowed. 
            var style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingWhite;
            bool result = decimal.TryParse(data, style, CultureInfo.InvariantCulture, out dValue);
            //bool result = decimal.TryParse(data, NumberStyles.Number, CultureInfo.InvariantCulture, out dValue);
            if (!result)
                return false;

            return true;
        }

        public static bool TryParseDouble(string data, out double dValue)
        {
            // Exponential format is not allowed. 
            var style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingWhite;
            bool result = double.TryParse(data, style, CultureInfo.InvariantCulture, out dValue);
           // bool result = double.TryParse(data, NumberStyles.Number, CultureInfo.InvariantCulture, out dValue);
            //  bool result = double.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out dValue);
            if (!result)
                return false;

            if (Double.IsNaN(dValue))
                return false;

            return true;
        }


        public static bool TryParseInt(string data, out int iValue)
        {
            bool result = int.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out iValue);
            if (!result)
                return false;
            
            return true;
        }

        public static byte[] FromHex(string hex)
        {
            //byte[] data = FromHex("47-61-74-65-77-61-79-53-65-72-76-65-72");
            //string s = Encoding.ASCII.GetString(data);

            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        public static bool IsHex(IEnumerable<char> chars)
        {
            bool isHex;
            foreach (var c in chars)
            {
                isHex = ((c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F'));

                if (!isHex)
                    return false;
            }
            return true;
        }

    }


}