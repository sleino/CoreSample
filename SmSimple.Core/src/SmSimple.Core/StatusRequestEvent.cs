using System;
using SmSimple.Core.Attributes;

namespace SmSimple
{
    public sealed class StatusRequestArgs : EventArgs
    {
    }

    public delegate void StatusRequestEvent(object sender, StatusRequestArgs e);

    [ImmutableAttribute]
    public sealed class StatusRequestEventHandler
    {
        public static event StatusRequestEvent ApplicationStatusRequest;

        internal void MakeStatusRequest()
        {
            var e = new StatusRequestArgs();
            if (ApplicationStatusRequest != null)
                ApplicationStatusRequest(this, e);
        }
    }
}