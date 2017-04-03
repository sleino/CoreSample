
namespace SmComm.Core.Parser.NMEA
{
    public sealed class NMEAProprietarySentence : NMEASentence
    {     
        public ManufacturerCodes Manufacturer { get; set; }
        public string SentenceIDString { get; set; }
    }
}
