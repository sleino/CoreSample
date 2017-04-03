using System;

namespace SmSimple.Core.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class ImmutableAttribute : System.Attribute
    {
    }
}
