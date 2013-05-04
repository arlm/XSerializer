using System;

namespace XSerializer
{
    public interface IXmlSerializerOptions
    {
        string DefaultNamespace { get; }
        Type[] ExtraTypes { get; }
        string RootElementName { get; }
    }
}