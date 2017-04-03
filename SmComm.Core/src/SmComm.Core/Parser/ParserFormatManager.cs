using System;
using System.Diagnostics;

namespace SmComm.Core.Parser
{
    public static class ParserFormatManager
    {

        public static bool IsValidMessageFormatName(string formatName)
        {
            if (formatName == null)
                return false;

            var values = Enum.GetValues(typeof(AwsMessageFormat));
            foreach (AwsMessageFormat fName in values)
                if (string.Compare(fName.ToString(), formatName,StringComparison.Ordinal) == 0)
                    return true;

            return false;
        }

        // AwsMessageFormat check
        public static char GetEndChar(AwsMessageFormat format)
        {
            char etx = '\u0003';

            if (format == AwsMessageFormat.SMSAWS)
                return ')';
            else if (format == AwsMessageFormat.WXT_NMEA)
                return '\n';
            else if (format == AwsMessageFormat.WXT_AR)
                return '\n';
            else if (format == AwsMessageFormat.YOURVIEW)
                return etx;
            else if (format == AwsMessageFormat.SIF)
                return ')';
            else if (format == AwsMessageFormat.OTT_PARSIVEL)
                return '"';
            else if (format == AwsMessageFormat.CT25KAM61)
                return etx;
            else if (format == AwsMessageFormat.TACMET)
                return etx;
            else if (format == AwsMessageFormat.PWD_MSG7)
                return etx;
            else if (format == AwsMessageFormat.ROSA)
                return '=';
            else if (format == AwsMessageFormat.AWAC_NMEA)
                return '*';
            Debug.Assert(false, "Untested parser for " + format);

            return ')';
        }
    }
}
