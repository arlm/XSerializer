using System;

namespace XSerializer
{
    public interface IXmlSerializerOptions
    {
        string DefaultNamespace { get; }
        Type[] ExtraTypes { get; }
        string RootElementName { get; }
        RedactAttribute RedactAttribute { get; }
        bool TreatEmptyElementAsString { get; }
        bool ShouldAlwaysEmitNil { get; }
        bool ShouldUseAttributeDefinedInInterface { get; }
        string XmlChoiceElement { get; }
    }
}