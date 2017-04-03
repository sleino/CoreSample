using SmSimple.Core;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SmComm.Core.IO
{
    /// <summary>
    /// Generate random SMSAWS messages with pseudorealistic data.
    /// </summary>
    public static class AwsSimulator
    {
        private readonly static Random random = new Random(113);
        
        private static int hmsCounter = 0;

        public static string GetNewHmsMessage(string station)
        {

            var curTime = DateTimeEx.Now.AddHours(0);
            var sb = new StringBuilder();

            sb.Append("(S:" + station + ";D:");
            sb.Append(curTime.ToString("yyMMdd", CultureInfo.InvariantCulture));
            sb.Append(";T:");
            sb.Append(curTime.ToString("HHmmss", CultureInfo.InvariantCulture));

            sb.Append(GetTemperature(curTime));
            sb.Append(GetPressure("PAAVG1M", curTime));
            //s.Append(Getppp(curTime));
            sb.Append(GetWindDirData(curTime));
            sb.Append(GetWindSpeedData(curTime));
            sb.Append(GetShipPosition());
            sb.Append(GetShipMotion());
            sb.Append(";STATUS:0");
            sb.Append(";SENSORSTATUS:");
            //s.Append(0);          
            if (hmsCounter == 2)
                sb.Append("///");
            else
                sb.Append(hmsCounter % 4);
            sb.Append(";wawa:4");
            sb.Append(";UPTIME1H:60");
            sb.Append(";BATT1H:12.7");
            //s.Append(";QNHAVG1M:1010.");
            sb.Append(4 + hmsCounter % 2);
            sb.Append(";TAAVG1M:12.");
            sb.Append(hmsCounter % 5);
            sb.Append(";RHAVG1M:45");
            sb.Append(";DPAVG1M:10.");
            sb.Append(3 + hmsCounter % 5);
            if (hmsCounter == 2)
                sb.Append(";VIS:///");
            else
                sb.Append(";VIS:1600");
            sb.Append(";VV:5000");
            sb.Append(";CL1:900;SC1:1");
            sb.Append(";CL2:1600;SC2:3");
            sb.Append(")\r\n");

            hmsCounter++;
            if (hmsCounter == 4)
                hmsCounter = 0;

            return sb.ToString();
        }

        private static string GetTemperature(DateTime curTime)
        {
            var s = new StringBuilder();
            s.Append(";TAAVG1M:");
            double dTa = 10 + (curTime.Second / 10);
            s.Append(dTa.ToString(CultureInfo.InvariantCulture));
            s.Append(".");
            string sDecimal = (curTime.Minute / 6).ToString(CultureInfo.InvariantCulture);
            s.Append(sDecimal.Substring(0, 1));
            return s.ToString();
        }

        private static string GetPressure(string variableName, DateTime curTime)
        {
            var s = new StringBuilder();
            s.Append(";" + variableName + ":");
            double dPa = 975 + curTime.Second;
            if (variableName.StartsWith("QNH"))
                dPa = dPa + 1;
            var tmp = curTime.Second.ToString(CultureInfo.InvariantCulture);
            tmp = tmp.PadLeft(2, '0');
            var sPA = dPa.ToString(CultureInfo.InvariantCulture) + "." + tmp.Substring(1, 1);
            Debug.Assert(!sPA.EndsWith("."), "sPA value:" + sPA);
            s.Append(sPA);
            return s.ToString();
        }

        static int positionCounter = 0;
        private static string GetShipPosition()
        {
            var s = new StringBuilder();
            var dMinute1 = DateTimeEx.Now.Second.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
            var dMinute2 = DateTimeEx.Now.AddSeconds(23).Second.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
            //s.Append(string.Format(CultureInfo.InvariantCulture, ";LATITUDE:39{0}34.12;LAT_NS:N;LONGITUDE:123{1}45.78;LON_EW:W", dMinute1, dMinute2));
            positionCounter++;
            if (positionCounter % 2 == 0)
                s.Append(string.Format(CultureInfo.InvariantCulture, ";LAT:39{0}.3412;LAT_NS:N;LONG:60{0}.0034;LONG_EW:W", dMinute1, dMinute2));
            else
                s.Append(string.Format(CultureInfo.InvariantCulture, ";LAT:39{0}.3412;LAT_NS:N;LONG:60{0}.1224;LONG_EW:W", dMinute1, dMinute2));

            return s.ToString();
        }

        private static string GetShipMotion()
        {
            var s = new StringBuilder();
            var heading = DateTimeEx.Now.AddSeconds(12).Second * 6;
            s.Append(string.Format(CultureInfo.InvariantCulture, ";HEADING:{0}", heading));
            var pitch = Math.Sin(-2.5 + (double)(DateTimeEx.Now.AddSeconds(12).Second) / 12);
            s.Append(string.Format(CultureInfo.InvariantCulture, ";PITCH:{0:0.0}", pitch));
            var roll = 1 + 1.5 * Math.Sin((double)(DateTimeEx.Now.Minute) / 60);
            s.Append(string.Format(CultureInfo.InvariantCulture, ";ROLL:{0:0.0}", roll));
            var heave = 0.2 + Math.Abs(Math.Sin(Math.PI / (1 - ((double)(DateTimeEx.Now.Second) / 20))));
            s.Append(string.Format(CultureInfo.InvariantCulture, ";HEAVE:{0:0.0}", heave));
            var heaveRate = 0.5 + ((random.NextDouble() - 0.5) / 5);
            s.Append(string.Format(CultureInfo.InvariantCulture, ";HEAVE_RATE:{0:0.0}", heaveRate));
            var heavePeriod = 12 + (3 * (random.NextDouble()));
            s.Append(string.Format(CultureInfo.InvariantCulture, ";HEAVE_PERIOD:{0:0.0}", heavePeriod));
            return s.ToString();
        }

        private static string GetWindSpeedData(DateTime curTime)
        {
            var s = new StringBuilder();

            s.Append(";WSAVG10M:");
            const double dWSavg = 16;// 5 + (int)(curTime.Minute / 10);
            s.Append(dWSavg.ToString(CultureInfo.InvariantCulture));
            s.Append(".");
            var sDecimal = random.Next(9).ToString(CultureInfo.InvariantCulture);
            s.Append(sDecimal.Substring(0, 1));

            s.Append(";WSMAX10M:");
            const double dWSmax = 6 + dWSavg;
            s.Append(dWSmax.ToString(CultureInfo.InvariantCulture));
            s.Append(".");
            s.Append(sDecimal.Substring(0, 1));

            s.Append(";WSMIN10M:");
            const double dWSmin = dWSavg - 5;
            s.Append(dWSmin.ToString(CultureInfo.InvariantCulture));
            s.Append(".");
            s.Append(sDecimal.Substring(0, 1));

            s.Append(";WSAVG2M:");
            const double d2WSavg = 18;// 5 + (int)(curTime.Minute / 10);
            s.Append(d2WSavg.ToString(CultureInfo.InvariantCulture));
            s.Append(".");
            var s2Decimal = random.Next(9).ToString(CultureInfo.InvariantCulture);
            s.Append(s2Decimal.Substring(0, 1));

            s.Append(";WSMAX2M:");
            const double d2WSmax = 6 + d2WSavg;
            s.Append(d2WSmax.ToString(CultureInfo.InvariantCulture));
            s.Append(".");
            s.Append(sDecimal.Substring(0, 1));

            s.Append(";WSMIN2M:");
            const double d2WSmin = d2WSavg - 5;
            s.Append(d2WSmin.ToString(CultureInfo.InvariantCulture));
            s.Append(".");
            s.Append(sDecimal.Substring(0, 1));

            s.Append(";WS:");
            var dWs = d2WSavg + 1.3 * random.Next(9);
            s.Append(dWs.ToString(CultureInfo.InvariantCulture));

            s.Append(";WGUST10M:");
            // double d10Gust = d2WSmax  +1.4 * random.Next(9);
            s.Append(d2WSmax.ToString(CultureInfo.InvariantCulture));
            s.Append(".");
            s.Append(sDecimal.Substring(0, 1));

            s.Append(";WGUSTTIME10M:");
            var time = curTime.AddMinutes(-30).ToString("HH:mm");
            s.Append(time);

            //// ship
            //s.Append(";WSR:");
            //double dRWS = dWSavg - 2;
            //s.Append(dRWS.ToString(CultureInfo.InvariantCulture));
            //s.Append(".");
            //sDecimal = random.Next(9).ToString(CultureInfo.InvariantCulture);
            //s.Append(sDecimal.Substring(0, 1));

            return s.ToString();
        }

        private static string GetWindDirData(DateTime curTime)
        {
            var s = new StringBuilder();
            s.Append(";WDAVG10M:");
            double dWDavg = 140 - curTime.Second / 2 + random.Next(9);
            s.Append(dWDavg.ToString(CultureInfo.InvariantCulture));

            s.Append(";WDMAX10M:");
            var dWDmax = dWDavg + 20;
            s.Append(dWDmax.ToString(CultureInfo.InvariantCulture));

            s.Append(";WDMIN10M:");
            var dWDmin = dWDavg - 30;
            s.Append(dWDmin.ToString(CultureInfo.InvariantCulture));


            s.Append(";WDAVG2M:");
            double d2WDavg = 140 - curTime.Second / 2 + random.Next(9);
            s.Append(d2WDavg.ToString(CultureInfo.InvariantCulture));

            s.Append(";WDMAX2M:");
            var d2WDmax = d2WDavg + 25;
            s.Append(d2WDmax.ToString(CultureInfo.InvariantCulture));

            s.Append(";WDMIN2M:");
            var d2WDmin = d2WDavg - 35;
            s.Append(d2WDmin.ToString(CultureInfo.InvariantCulture));

            s.Append(";WD:");
            var dWd = d2WDavg + 2;
            s.Append(dWd.ToString(CultureInfo.InvariantCulture));

            s.Append(";WDGUST10M:");
            const string sWDGust = "141";
            s.Append(sWDGust);

            return s.ToString();
        }
    }
}
