using System;
using System.Diagnostics;
using System.Globalization;

namespace SmSimple.Core
{
    public static class ExceptionHandler
    {
        public static void HandleException(Exception ex)
        {
            ExceptionRecorder.RecordException(ex);
        }

        public static void HandleException(string message, Exception ex)
        {
            ExceptionRecorder.RecordException(message, ex);
        }

        public static void HandleException(Exception ex, string message)
        {
            ExceptionRecorder.RecordException(message, ex);
        }

        public static void HandleException(string message)
        {
            ExceptionRecorder.RecordException(message);
        }

        public static void RecordException(string message)
        {
            ExceptionRecorder.RecordException(message);
        }
    }

    /// <summary>
    /// Handles all exceptions which are not handled in the throwing module
    /// </summary>
    public sealed class ExceptionRecorder
    {
        private readonly static StatusRequestEventHandler statusRequestEventHandler = new StatusRequestEventHandler();

        public static void RecordException(Exception ex)
        {
            RecordException(string.Empty, ex);
        }

        public static void RecordException(string s, Exception ex)
        {
            if (ex == null && s != null)
                RecordException(s);
            string s2 = s;
            if (ex.Message != null)
                s2 += ex.GetType() + ".";
            if (!s2.Contains(ex.Message))
                s2 += ex.Message;
            RecordException(s2);
            if (ex != null && ex.InnerException != null)
                if (ex.InnerException.Message != null)
                    ExceptionRecorder.RecordException("Inner exception:" + ex.InnerException.Message);
        }

        private static DateTime lastExceptionTime = DateTime.MinValue;
        private static string lastExceptionMsg = string.Empty;

        private static bool SameExceptionHappenedLessThanMillisecondAgo(string exceptionMessage)
        {
            if ((lastExceptionTime.ToString("yyyy-MM-dd HH:mm:ss:fff") ==
                 DateTimeEx.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"))
                && (exceptionMessage == lastExceptionMsg))
                return true;

            lastExceptionTime = DateTimeEx.Now;
            lastExceptionMsg = exceptionMessage;
            return false;
        }

        private static bool SameExceptionHappenedEarlier(string exceptionMessage, TimeSpan silentTime)
        {
            TimeSpan timeSinceLastException = DateTimeEx.Now.Subtract(lastExceptionTime);
            if ((timeSinceLastException < silentTime)
                && (exceptionMessage == lastExceptionMsg))
                return true;

            lastExceptionTime = DateTimeEx.Now;
            lastExceptionMsg = exceptionMessage;
            return false;
        }

        public static void RecordException(string s)
        {
            if (SameExceptionHappenedLessThanMillisecondAgo(s))
                return;

            ProcessException(s);
        }

        public static void RecordException(string s, bool assert)
        {
            if (SameExceptionHappenedLessThanMillisecondAgo(s))
                return;

            SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, s);
            Debug.Assert(assert, s);
        }

        public static void RecordIoException(string s)
        {

            RecordException(s, TimeSpan.FromMilliseconds(2000));

        }

        public static void RecordException(string s, TimeSpan silentTime)
        {
            if (SameExceptionHappenedEarlier(s, silentTime))
                return;

            ProcessException(s);
        }

        private static void ProcessException(string s)
        {
            SimpleFileWriter.WriteStackToDiagnosticsLog(s);
            SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, s);
#if DEBUG
            //  Debugger.Break();
            string s1 = string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}\r\n", DateTimeEx.NowToString, s,
                SimpleFileWriter.GetStack);
            Debug.Assert(false, s + Environment.NewLine + s1);
#endif
        }

        //  DoNothingWith(x);// This suppress the 'x' not used warning
        [Conditional("Debug")]
        public static void DoNothingWith(object obj)
        {

        }
    }

    public sealed class SmException : Exception
    {
        public SmException()
        {
        }

        public SmException(string message)
            : base(message)
        {
        }

        public SmException(string message, Exception inner)
            : base(message, inner)
        {
        }
    };
}