using System;
using System.Collections.Generic;
using System.Diagnostics;
using SmSimple.Core;
using SmSimple.Core.Util;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;

namespace SmValidation.Core
{
    public static class Validation
    {
        public static bool IsValidInt(string itemValue)
        {
            int tmp = 0;
            return int.TryParse(itemValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out tmp);
        }

        public static bool IsValidFloat(string itemValue)
        {
            if (itemValue == null) return false;
            if (itemValue.Length == 0) return false;
            itemValue = itemValue.Trim();

            // Don't use Validation.TryParseDouble as it indicates -.123 as valid float
            //double d = 0;
            //if (!Validation.TryParseDouble(itemValue,  out d))
            //{
            //    return false;
            //}
            //return true;

            char[] chars = itemValue.ToCharArray();
            for (var i = 0; i < chars.GetLength(0); i++)
            {
                if (!(char.IsDigit(chars[i])) && (chars[i] != '.') && (chars[i] != '-'))
                    return false;
            }

            int dotIndex = itemValue.IndexOf(".", StringComparison.Ordinal);
            int hyphenIndex = itemValue.IndexOf("-", StringComparison.Ordinal);

            if (itemValue.LastIndexOf(".", StringComparison.Ordinal) != dotIndex) return false;
            if (itemValue.LastIndexOf("-", StringComparison.Ordinal) != hyphenIndex) return false;

            if (dotIndex == 0) return false;
            if (dotIndex == itemValue.Length) return false;

            if (hyphenIndex > 0) return false;
            if (dotIndex == hyphenIndex + 1) return false; // -.nnn    .nnn

            return true;
        }

        //public static bool IsIntegerOrSlash(string itemValue) 
        //{
        //    if (itemValue == null) return false;
        //    if (itemValue.Length==0)return false;

        //    char[] chars = itemValue.ToCharArray();
        //    for (var  i = 0; i< chars.GetLength(0);i++) 
        //    {
        //        if (!(char.IsDigit(chars[i]) ) && (chars[i] != '/')&& (chars[i] != '-')) 
        //        {
        //            errorHandler.ProcessMessage( itemValue + " " + StringManage.GetString("should contain either digits or slashes (/) ") );
        //            return false;
        //        }
        //    }
        //    return true;
        //}


        public static bool TryParseDecimal(string data, out decimal dValue)
        {
            return StringUtil.TryParseDecimal(data, out dValue);
        }

        public static bool TryParseDouble(string data, out double dValue)
        {
            return StringUtil.TryParseDouble(data, out dValue);
        }

        public static bool TryParseInt(string data, out int iValue)
        {
            return StringUtil.TryParseInt(data, out iValue);
        }

        public static bool ContainsDecimalOrSlashes(string itemValue)
        {

            if (string.IsNullOrEmpty(itemValue))
                return false;
            if (itemValue[0] == '.') return false;
            if (itemValue.Length > 1)
                if ((itemValue[0] == '-') && (itemValue[1] == '.'))
                    return false;

            double result;
            if (Validation.TryParseDouble(itemValue, out result))
                return true;

            if (!IsFilledWithSlashes(itemValue))
                return false;

            return true;
        }

        public static bool IsFilledWithSlashes(string itemValue)
        {
            if (string.IsNullOrEmpty(itemValue))
                return false;

            foreach (var c in itemValue)
            {
                if (c != '/')
                    return false;
            }
            return true;
        }

        public static bool IsInRangeOrSlash(string itemValue, string variableName, double lowerBound, double upperBound)
        {
            if (string.IsNullOrEmpty(itemValue))
                return false;

            if (string.IsNullOrEmpty(variableName))
                return false;

            Debug.Assert(lowerBound <= upperBound, "IsInRangeOrSlash, lowerBound > upperBound");
            try
            {
                if (!ContainsDecimalOrSlashes(itemValue))
                    return false;

                if (IsFilledWithSlashes(itemValue))
                    return true;

                if (itemValue.LastIndexOf("/", StringComparison.Ordinal) > 0)
                    return false;

                double d = 0;
                if (!Validation.TryParseDouble(itemValue, out d))
                    return false;

                return Validation.CheckBounds(d, variableName, lowerBound, upperBound);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, "IsInRangeOrSlash " + ex.Message);
                throw;
            }
        }

        public static bool CheckBounds(double i, string variableName, double lowerBound, double upperBound)
        {
            if ((i < lowerBound))
                return false;

            if ((i > upperBound))
                return false;

            return true;
        }

        // return true if _value is in _enum
        public static bool IsValidEnum<EnumeratedType>(string _value)
        {
            if (_value == null)
                return false;

            foreach (EnumeratedType eType in Enum.GetValues(typeof(EnumeratedType)))
                if (string.Compare(_value, eType.ToString(),StringComparison.Ordinal) == 0)
                    return true;
            return false;
        }

        public static bool StrToEnum<EnumeratedType>(string candidate, ref EnumeratedType enumType)
        {

            foreach (EnumeratedType eType in Enum.GetValues(typeof(EnumeratedType)))
            {
                if (string.Compare(candidate, eType.ToString(), StringComparison.Ordinal) == 0)
                {
                    enumType = eType;
                    return true;
                }
            }
            return false;
        }

        public static bool StrToEnum<EnumeratedType>(string candidate, EnumeratedType defaultValue, out EnumeratedType enumType)
        {
            enumType = defaultValue;
            return StrToEnum<EnumeratedType>(candidate, ref enumType);
        }

        public static IList<string> EnumStrings<EnumeratedType>()
        {
            var result = new List<string>();
            foreach (EnumeratedType eType in Enum.GetValues(typeof(EnumeratedType)))
            {
                result.Add(eType.ToString());
            }
            result.Sort();
            return result;
        }



        public static bool IsValidEmailAddress(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            // based on the code sample from the .NET SDK documentation
            const string REGEX_VALID_EMAIL =
                @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|" +
                @"(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            bool valid = Regex.IsMatch(email, REGEX_VALID_EMAIL);
            return valid;
        }

        public static bool IsValidStationName(string candidateName)
        {
            if (string.IsNullOrEmpty(candidateName))
                return false;

            char[] invalidChars = { '<', '>', '|', ',', ';', '~', '^' };
            int indexOf = candidateName.IndexOfAny(invalidChars);
            return indexOf == -1;
        }

        public static bool IsValidVariableName(string candidateName)
        {
            if (string.IsNullOrEmpty(candidateName))
                return false;

            /*
             ^ : start of string
            [ : beginning of character group
            a-z : any lowercase letter
            A-Z : any uppercase letter
            0-9 : any digit
            _ : underscore
            ] : end of character group
            * : zero or more of the given characters
            $ : end of string
             */
            const string REGEX_VALID_NAME =
               @"^[a-zA-Z0-9_.]*$";
            bool valid = Regex.IsMatch(candidateName, REGEX_VALID_NAME);
            return valid;
        }

        public static bool IsValidInternalVariableName(string candidateName)
        {
            if (string.IsNullOrEmpty(candidateName))
                return false;

            /*
             ^ : start of string
            [ : beginning of character group
            a-z : any lowercase letter
            A-Z : any uppercase letter
            0-9 : any digit
            _ : underscore
            ] : end of character group
            * : zero or more of the given characters
            $ : end of string
             */
            const string REGEX_VALID_NAME =
               @"^[a-zA-Z0-9_.]*$";
            bool valid = Regex.IsMatch(candidateName, REGEX_VALID_NAME);
            return valid;
        }

        public static bool IsValidFullFileName(string fileNameWithPath)
        {
            try
            {
                if (string.IsNullOrEmpty(fileNameWithPath))
                    return false;
                char[] invChars = Path.GetInvalidPathChars();
                if (fileNameWithPath.IndexOfAny(invChars) > -1)
                    return false;
                string fileName = Path.GetFileName(fileNameWithPath);
                return IsValidFileName(fileName);
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception in isValidFullFileName " + fileNameWithPath, ex);
                return false;
            }
        }


        private static readonly char[] invFileNameChars = Path.GetInvalidFileNameChars();
        public static bool IsValidFileName(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return false;

                if (fileName.IndexOfAny(invFileNameChars) > -1)
                    return false;

                new System.IO.FileInfo(fileName);

                return true;
            }
            catch (System.Security.SecurityException ex)
            {
                ExceptionRecorder.RecordException("SecurityException in IsValidFileName " + fileName, ex);
                return false;
            }
            catch (System.ArgumentException ex)
            {
                ExceptionRecorder.RecordException("ArgumentException in IsValidFileName " + fileName, ex);
                return false;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                ExceptionRecorder.RecordException("UnauthorizedAccessException in IsValidFileName " + fileName, ex);
                return false;
            }
            catch (System.IO.PathTooLongException ex)
            {
                ExceptionRecorder.RecordException("PathTooLongException in IsValidFileName " + fileName, ex);
                return false;
            }
        }


        private static readonly char[] invPathChars = Path.GetInvalidPathChars();
        public static bool IsValidDirectoryName(string directoryName)
        {
            try
            {
                if (string.IsNullOrEmpty(directoryName))
                    return false;

                if (directoryName.IndexOfAny(invPathChars) > -1)
                    return false;


                new System.IO.DirectoryInfo(directoryName);
                return true;
            }
            catch (ArgumentException ex)
            {
                ExceptionRecorder.RecordException("ArgumentException in IsValidDirectoryName ", ex);
                return false;
            }
            catch (System.IO.PathTooLongException ex)
            {
                ExceptionRecorder.RecordException("PathTooLongException in IsValidDirectoryName ", ex);
                return false;
            }
            catch (System.Security.SecurityException ex)
            {
                ExceptionRecorder.RecordException("SecurityException in IsValidDirectoryName ", ex);
                return false;
            }
            catch (Exception ex)
            {
                ExceptionRecorder.RecordException("Exception in IsValidDirectoryName ", ex);
                return false;
            }
        }

    } // class
}
