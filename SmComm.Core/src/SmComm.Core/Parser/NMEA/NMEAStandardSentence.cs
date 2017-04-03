
namespace SmComm.Core.Parser.NMEA
{
    public sealed class NMEAStandardSentence : NMEASentence
    {
        public TalkerIdentifiers TalkerID { get; set; }
        public SentenceIdentifiers SentenceID { get; set; }        
    }
}
