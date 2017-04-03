using SmSimple.Core.Attributes;

namespace SmSimple.Core
{
    [ImmutableAttribute]
    public sealed class Lock
    {
        public bool IsActive { get; set; }
    }
}