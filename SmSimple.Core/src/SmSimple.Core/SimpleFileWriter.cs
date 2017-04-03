using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace SmSimple.Core
{
    public enum DirectoryName
    {
        History,
        EventLog,
        FileTransfer,
        ModemLog,
        Diag,
        QualityControlLog,
        AlarmLog,
        Input,
        MessageGeneration
    }

    public class SimpleFileWriter
    {
        private static readonly Lock logLock = new Lock();
        private static readonly Dictionary<DirectoryName, Lock> locks = new Dictionary<DirectoryName, Lock>();

        private static volatile Dictionary<DirectoryName, bool> enableLogging = new Dictionary<DirectoryName, bool>();

        internal static bool UseMonthlyRotatingSystem { get; set; } = true;

        public static bool UseFixedDirectory { get; set; }
        public static string FixedDirectory { get; set; } = @"C:\temp";

        private static readonly int logTypeCount = 1 + DirectoryName.MessageGeneration - DirectoryName.History;


        public static void EnableLogging(DirectoryName directoryName, bool enable)
        {
            if (enableLogging.Count == 0)
                InitEnableLogging();

            enableLogging[directoryName] = enable;
        }


        public static void WriteLineToEventFile(DirectoryName directoryName, string strMessage)
        {
            try
            {
                InitialiseLoggingAndLocks();

                if (!enableLogging[directoryName])
                    return;

                lock (locks[directoryName])
                {
                    string dir = MaintainDirectory(directoryName);
                    string fileName = GetEventFileName(directoryName);
                    if (UseMonthlyRotatingSystem)
                        MaintainFile(fileName);
                    if (!File.Exists(fileName))
                        WriteFirstLine(fileName);
                    WriteLine(fileName, strMessage);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.Assert(false,
                    "Debug code exception while writing data into " + directoryName + "  file." + ex.Message);
#else
                Debug.Assert(false, "Release code Exception while writing data into " + directoryName + "  file." + ex.Message);
#endif
            }
        }

        private static void WriteFirstLine(string fileName)
        {
            string firstLine = "Compilation date: " + DateTimeEx.GetCompilationDateAsString;
            WriteLine(fileName, firstLine);
        }

        private static void WriteLine(string fileName, string strMessage)
        {
            File.AppendAllText(fileName, DateTimeEx.NowToStringWithMs + "\t" + strMessage + Environment.NewLine);
        }

        public static void WriteStackToDiagnosticsLog(string data)
        {
        }

        public static string GetStack
        {
            get { return " GetStack is Unimplemented"; }
        }


        private static string GetEventFileName(DirectoryName directoryName)
        {
            string fullDir = GetDirectory(directoryName);
            DateTime dtNow = DateTimeEx.Now;
            var sb = new StringBuilder();
            sb.Append(fullDir);
            sb.Append("\\");
            sb.Append(Environment.MachineName);
            sb.Append("_");
            sb.Append(directoryName);
            sb.Append("_");
            if (UseMonthlyRotatingSystem)
            {
                if (dtNow.Day < 10)
                    sb.Append("0");
                sb.Append(dtNow.Day.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                sb.Append(DateTimeEx.NowToDayFileString);
            }
            sb.Append(".txt");
            return sb.ToString();
        }

        private static void InitialiseLoggingAndLocks()
        {
            if (enableLogging.Count < logTypeCount)
            {
                lock (logLock)
                {
                    InitEnableLogging();
                }
            }

            if (locks.Count == 0)
                InitLocks();
        }

        private static void InitEnableLogging()
        {
            foreach (DirectoryName eType in Enum.GetValues(typeof(DirectoryName)))
            {
                if (!enableLogging.ContainsKey(eType))
                    enableLogging.Add(eType, true);
            }
        }

        private static void InitLocks()
        {
            foreach (DirectoryName eType in Enum.GetValues(typeof(DirectoryName)))
            {
                locks.Add(eType, new Lock());
            }
        }

        private static string MaintainDirectory(DirectoryName directoryName)
        {
            string dir = GetDirectory(directoryName);
            FileSystem.TryCreateDirectory(dir);
            return dir;
        }

        private static void MaintainFile(string fileName)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileName));

            if (File.Exists(fileName))
            {
                var curTime = DateTime.Now;
                DateTime lastWriteTime = File.GetLastWriteTime(fileName);
                if (lastWriteTime.Year != curTime.Year || lastWriteTime.Month != curTime.Month)
                    File.Delete(fileName);
            }
        }


        public static string GetDirectory(DirectoryName directoryName)
        {
            string location = AppContext.BaseDirectory;

            if (!UseFixedDirectory)
                return location + "\\logs\\" + directoryName;
            else
                return FixedDirectory + "\\logs\\" + directoryName;
        }
    }
}