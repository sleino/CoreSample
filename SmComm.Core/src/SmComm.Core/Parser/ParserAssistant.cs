using System;
using System.Collections.Generic;
using System.Text;
using SmComm.Core.IO;
using SmComm.Core.Parser.NMEA;
using SmSimple.Core.Util;
using SmSimple.Core;
using System.Diagnostics;

namespace SmComm.Core.Parser
{
    public class ParserAssistant
    {
        public ParserAssistant()
        {
            RemoveDuplicateMessages = true;
        }

        private static readonly PortListenerEventHandler _portListenerEventHandler = new PortListenerEventHandler();
        internal static PortListenerEventHandler portListenerEventHandler
        {
            get { return _portListenerEventHandler; }
        }

        public bool RemoveDuplicateMessages { get; set; }
        private static readonly DuplicateMessageAssistant duplicateMessageAssistant = new DuplicateMessageAssistant();
        private static readonly ChannelDataCounter channelDataCounter = new ChannelDataCounter();

        private static readonly SmsAwsParser smsAwsParser = new SmsAwsParser();

        /*
        private readonly Crc32Checker crc32Checker = new Crc32Checker();
        public bool UseCrc32WithSmsAWS { get; set; }
        public Crc32Parameters Crc32Parameters
        {
            get { return crc32Checker.Crc32Parameters; }
            set { crc32Checker.Crc32Parameters = value; }
        }
        */

        private static readonly WxtNmeaParser wxtNmeaParser = new WxtNmeaParser();
       
        private static readonly WxtArParser wxtArParser = new WxtArParser();

        private static readonly CeiloMsg61Parser ceiloMsg61Parser = new CeiloMsg61Parser();
        private YourViewParser yourViewParser = new YourViewParser();
        private AwacNMEAParser awacNmeaParser = new AwacNMEAParser();
        private static readonly PwdMsg7Parser pwdMsg7Parser = new PwdMsg7Parser();
        //  private static readonly RosaMesParser rosaMesParser = new RosaMesParser();
        /*
        private static readonly Dictionary<string, ParserBase> tacmetParsers = new Dictionary<string, ParserBase>{
            {TacmetParser.TacmetMsgId.INSTW.ToString(), new TacmetParser(TacmetParser.TacmetMsgId.INSTW)} ,
            {TacmetParser.TacmetMsgId.PTU.ToString(), new TacmetParser(TacmetParser.TacmetMsgId.PTU)},
            {TacmetParser.TacmetMsgId.CONF.ToString(), new TacmetConfParser()},
            {TacmetParser.TacmetMsgId.WIND.ToString(), new TacmetParser(TacmetParser.TacmetMsgId.WIND)},
            {TacmetParser.TacmetMsgId.SA20.ToString(), new TacmetParser(TacmetParser.TacmetMsgId.SA20)},
            {TacmetParser.TacmetMsgId.CT22061.ToString(),new CeiloMsg61Parser()},
            {TacmetParser.TacmetMsgId.PWD.ToString(), new PwdForTacmetParser()},
            {PwdMsg7Parser.Id, new PwdMsg7Parser()}
        };
        
        */
        public bool WriteAllInputIntoLog { get; set; }
        public bool WriteAllInputPortIntoLog { get; set; }
        public bool WriteParserErrorsIntoLog
        {
            get { return smsAwsParser.WriteParserErrorsIntoLog; }
            set { smsAwsParser.WriteParserErrorsIntoLog = value; }
        }

       
        public YourViewParser YourViewParser
        {
            get { return yourViewParser; }
            set { yourViewParser = value; }
        }

        public AwacNMEAParser AwacNMEAParser
        {
            get { return awacNmeaParser; }
            set { awacNmeaParser = value; }
        }
       
        private bool roundSecondsToClosestfullMinute;
        public bool RoundSecondsToClosestFullMinute
        {
            set
            {
                roundSecondsToClosestfullMinute = value;
                smsAwsParser.RoundSecondsToClosestFullMinute = value;
                YourViewParser.RoundSecondsToClosestFullMinute = value;
                AwacNMEAParser.RoundSecondsToClosestFullMinute = value;
                wxtArParser.RoundSecondsToClosestZero = value;
                wxtNmeaParser.RoundSecondsToClosestZero = value;
                ceiloMsg61Parser.RoundSecondsToClosestFullMinute = value;
             //   rosaMesParser.RoundSecondsToClosestFullMinute = value;
            }
            get { return roundSecondsToClosestfullMinute; }
        }

        internal void PlainTextReceived(string text, INamedPort virtualPort)
        {
            LogInputData(text, virtualPort);

            // To dialler task, to mmoc console terminal
            portListenerEventHandler.ProcessPlainText(virtualPort, text);
        }



        internal void DataReceived(string text, INamedPort virtualPort)
        {

            channelDataCounter.RegisterMessage(virtualPort);

            // LogInputData(text, virtualPort);

            // To dialler task, to mmoc console terminal
            //   portListenerEventHandler.ProcessStringData(virtualPort, text);

            ProcessSmsAwsObservationData(virtualPort, text);

            /*
            if (virtualPort.MessageFormat == AwsMessageFormat.SMSAWS)
            {
                if (RemoveDuplicateMessages)
                    if (duplicateMessageAssistant.DuplicateStringDetected(text, virtualPort))
                        return;

                ProcessSmsAwsObservationData(virtualPort, text);
                return;
            }

            if (virtualPort.MessageFormat == AwsMessageFormat.AWAC_NMEA)
            {
                if (RemoveDuplicateMessages)
                    if (duplicateMessageAssistant.DuplicateStringDetected(text, virtualPort))
                        return;

                ProcessAwacNMEAObservationData(virtualPort, text);
                return;
            }



            else if (virtualPort.MessageFormat == AwsMessageFormat.WXT_NMEA)
            {
                ProcessWxtNmeaObservationData(virtualPort, text);
                return;
            }
            else if (virtualPort.MessageFormat == AwsMessageFormat.WXT_AR)
            {
                ProcessWxtArObservationData(virtualPort, text);
                return;
            }
            else if (virtualPort.MessageFormat == AwsMessageFormat.PWD_MSG7)
            {
                ProcessPwdMsg7(virtualPort, text);
                return;
            }


            else if (virtualPort.MessageFormat == AwsMessageFormat.YOURVIEW)
            {
                ProcessYourViewObservationData(virtualPort, text);
                return;
            }

            else if (virtualPort.MessageFormat == AwsMessageFormat.OTT_PARSIVEL)
            {
                ProcessOttParsivelObservationData(virtualPort, text);
                return;
            }

            //else if (virtualPort.MessageFormat == AwsMessageFormat.ROSA)
            //{
            //    ProcessRosaObservationData(virtualPort, text);
            //    return;
            //}


            else if (virtualPort.MessageFormat == AwsMessageFormat.CT25KAM61)
            {
                bool ok = ProcessCeiloObservationData(virtualPort, text);
                if (ok)
                    return;
                var parser = smsAwsParser;
                ProcessObservationData(virtualPort, text, parser);
            }

            else if (virtualPort.MessageFormat == AwsMessageFormat.SIF)
            {
                if (RemoveDuplicateMessages)
                    if (duplicateMessageAssistant.DuplicateStringDetected(text, virtualPort))
                        return;

                portListenerEventHandler.ProcessUnparsedData(virtualPort, text);
                return;
            }
            */
            /*
            if (virtualPort.EndChar == SmsAwsParser.EndChar)
            {
                ProcessSmsAwsObservationData(virtualPort, text);
                return;
            }

            if (virtualPort.EndChar == TmoLogFileParser.EndChar)
            {
                if (!IsSmsAwsData(text))
                {
                    ProcessTmoLogFileData(virtualPort, text);
                }
                else
                {
                    ProcessSmsAwsLogFileData(virtualPort, text);
                }
            }
            */
        }

        private void LogInputData(string text, INamedPort virtualPort)
        {
            if (virtualPort.RemoteSocketName == null)
                return;

            if (WriteAllInputIntoLog)
            {
                if (WriteAllInputPortIntoLog)
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Input,
                        string.Concat(virtualPort.RemoteSocketName
                        , "_"
                        , virtualPort.LocalSocketName
                        ,
                        text.Replace("\r", "<CR>").Replace("\n", "<NL>")));
                else
                    SimpleFileWriter.WriteLineToEventFile(DirectoryName.Input,
                        text.Replace("\r", "<CR>").Replace("\n", "<NL>"));

            }
        }

       

        private static void ProcessPwdMsg7(INamedPort virtualPort, string text)
        {
            var parser = pwdMsg7Parser;
            ProcessObservationData(virtualPort, text, parser);
        }

        private static bool ProcessCeiloObservationData(INamedPort virtualPort, string text)
        {
            try
            {
                MeasMsg measMsg;
                var ok = ceiloMsg61Parser.Parse(text, out measMsg);
                if (ok)
                    portListenerEventHandler.ProcessObservationData(virtualPort, measMsg, text);
                return ok;

            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException("ProcessCeiloObservationData", ex);
                return false;
            }
        }

        private static void ProcessObservationData(INamedPort virtualPort, string text, ParserBase parser)
        {
            MeasMsg msg = parser.ParseMessageIntoMeasMsg(text, virtualPort);
            // To AwsDataReceiver && Observation Console
            if (msg != null) // msg will be null if garbage was received 
                portListenerEventHandler.ProcessObservationData(virtualPort, msg, text);
        }

        private void ProcessYourViewObservationData(INamedPort virtualPort, string text)
        {
            MeasMsg msg;
            yourViewParser.Parse(text, out msg);
            // To AwsDataReceiver && Observation Console
            if (msg != null) // msg will be null if garbage was received 
                portListenerEventHandler.ProcessObservationData(virtualPort, msg, text);
        }

        private void ProcessAwacNMEAObservationData(INamedPort virtualPort, string text)
        {
            MeasMsg msg = this.awacNmeaParser.Parse(text);
            // To AwsDataReceiver && Observation Console
            if (msg != null) // msg will be null if garbage was received 
                portListenerEventHandler.ProcessObservationData(virtualPort, msg, text);
        }

        //private void ProcessRosaObservationData(INamedPort virtualPort, string text)
        //{
        //    MeasMsg msg;
        //    rosaMesParser.Parse(text, out msg);
        //    // To AwsDataReceiver && Observation Console
        //    if (msg != null) // msg will be null if garbage was received 
        //        portListenerEventHandler.ProcessObservationData(virtualPort, msg, text);
        //}

        private static void ProcessWxtNmeaObservationData(INamedPort virtualPort, string text)
        {
            try
            {
                var msg = wxtNmeaParser.Parse(text);
                // To AwsDataReceiver && Observation Console
                if (msg != null) // msg will be null if garbage was received 
                    portListenerEventHandler.ProcessObservationData(virtualPort, msg, text);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "Wxt NMEA message");
            }
        }

        private static void ProcessWxtArObservationData(INamedPort virtualPort, string text)
        {
            try
            {
                var msg = wxtArParser.Parse(text);
                // To AwsDataReceiver && Observation Console
                if (msg != null) // msg will be null if garbage was received 
                    portListenerEventHandler.ProcessObservationData(virtualPort, msg, text);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "Wxt AR message");
            }
        }

        private List<string> smsAwsMmessages = new List<string>();
        private void ProcessSmsAwsObservationData(INamedPort virtualPort, string text, bool qmlLogFileData = false)
        {

            //if (UseCrc32WithSmsAWS)
            //{
            //    var ok = crc32Checker.CheckSmsAwsMessage(text);
            //    if (!ok)
            //    {
            //        var parserStatus = StringManager.GetString("Crc32 check failure ");
            //        SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, parserStatus + text);
            //        portListenerEventHandler.ProcessPortStatusData(virtualPort, parserStatus);
            //        return;
            //    }
            //}


            smsAwsMmessages = SmsAwsPreParser.FindSmsAwsSubMessages(text);
            if (smsAwsMmessages.Count == 0)
                return;

            smsAwsMmessages.ForEach(curMessage =>
               ProcessSmsAwsMsg(virtualPort, curMessage, qmlLogFileData)
            );


            // trigger event informing the last message has been processed.
            if (qmlLogFileData)
            {
                int index = smsAwsMmessages.Count - 1;
                var measMsg = smsAwsParser.ParseMessageIntoMeasMsg(smsAwsMmessages[index], virtualPort);
                portListenerEventHandler.OnEndOfLogFileData(virtualPort, measMsg);
            }
        }

        void ParserBase_ParsingErrorEvent(object sender, ParserEventArgs e)
        {
            Debug.WriteLine(DateTimeEx.NowToStringWithMs + "ParserBase_ParsingErrorEvent" + e.ErrorDescription);
            portListenerEventHandler.ProcessParsingError(e);
        }



        private void ProcessSmsAwsMsg(INamedPort virtualPort, string curMessage, bool qmlLogFileData)
        {


            var measMsg = smsAwsParser.ParseMessageIntoMeasMsg(curMessage, virtualPort);

            if (measMsg != null)
            {
                var parserStatus = GetParsingStatus(measMsg);
                if (RemoveDuplicateMessages)
                    if (duplicateMessageAssistant.DuplicateMeasMsgDetected(measMsg))
                    {
                        portListenerEventHandler.ProcessPortStatusData(virtualPort, parserStatus);
                        return;
                    }


                if (qmlLogFileData)
                {
                    portListenerEventHandler.ProcessLogFileData(virtualPort, measMsg);
                }
                else
                {
                    portListenerEventHandler.ProcessObservationData(virtualPort, measMsg, curMessage);
                    portListenerEventHandler.ProcessPortStatusData(virtualPort, parserStatus);
                }

            }
        }


        private static readonly string cVariables = StringManager.GetString("Variables");
        private static readonly string cTimeDiff = StringManager.GetString("Time difference");
        private static string GetParsingStatus(MeasMsg measMsg)
        {
            var sb = new StringBuilder(cVariables);
            sb.Append(":");
            sb.Append(measMsg.Count);
            sb.Append(Environment.NewLine);
            sb.Append(cTimeDiff);
            sb.Append(":");
            TimeSpan ts = DateTimeEx.Now.Subtract(measMsg.Time);
            sb.Append(String.Format("{0} h  {1} min {2} sec",
              (int)(Math.Floor(ts.TotalHours)),
              ts.Minutes,
              ts.Seconds));
            return sb.ToString();
        }



        //private bool IsSmsAwsData(string text)
        //{
        //    if (string.IsNullOrEmpty(text))
        //        return false;
        //    int pos1 = text.IndexOf('(');
        //    if (pos1 == -1)
        //        return false;

        //    int pos2 = text.IndexOf("S:", pos1, StringComparison.InvariantCultureIgnoreCase);
        //    int pos3 = text.IndexOf("D:", pos2, StringComparison.InvariantCultureIgnoreCase);
        //    int pos4 = text.IndexOf(')', pos3);
        //    int len = text.Length;
        //    return ((len > 2) && (pos1 < pos2) && (pos2 < pos3) && (pos3 < pos4));
        //}

        //private void ProcessTmoLogFileData(INamedPort virtualPort, string text)
        //{
        //    var tmoLogFileParser = new TmoLogFileParser();
        //    var messages = tmoLogFileParser.ParseStringIntoMeasMsg(text, virtualPort.CurrentDestination);
        //    messages.ForEach(measMsg => portListenerEventHandler.ProcessLogFileData(virtualPort, measMsg));

        //    //messages.ForEach(delegate(MeasMsg msg)
        //    //{
        //    //    portListenerEventHandler.ProcessLogFileData(virtualPort, msg);
        //    //});
        //}

        //// this handles also regular smsaws data if end char is \r
        //private void ProcessSmsAwsLogFileData(INamedPort virtualPort, string text)
        //{
        //    List<string> messages = SmsAwsPreParser.ParseSmsAwsMessage(text);
        //    messages.ForEach(delegate(string curMessage)
        //    {
        //        MeasMsg msg = smsAwsParser.ParseMessageIntoMeasMsg(curMessage);
        //        if (msg != null) // msg will be null if garbage was received
        //            portListenerEventHandler.ProcessLogFileData(virtualPort, msg);
        //    });
        //}
        ////*********'
    }
}
