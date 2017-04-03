using SmComm.Core.Parser;

namespace SmComm.Core.IO
{
    public interface IVirtualPort
    {
        string LongName { get; }
        string ShortName();
        string LocalAddress { get; }

        string CurrentDestination { get; }
        string DestinationAndId { get; }
        AwsMessageFormat MessageFormat { get; }
        char EndChar { get; }
        bool Initialise(ConnectionData connectionData);
    }

    public interface INamedPort {
        string RemoteSocketName { get; }
        string LocalSocketName { get;  }

        string Read();
        void Dispose();
        bool HasDataAvailable();
        NamedPortState ConnectionState { get; }
    }
}
