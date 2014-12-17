using System;
using XSerializer.Encryption;

namespace XSerializer
{
    public interface IXmlSerializerOptions
    {
        string DefaultNamespace { get; }
        Type[] ExtraTypes { get; }
        string RootElementName { get; }
        RedactAttribute RedactAttribute { get; }
        EncryptAttribute EncryptAttribute { get; }
        bool TreatEmptyElementAsString { get; }
        bool ShouldAlwaysEmitNil { get; }
    }
}