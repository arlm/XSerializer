using System;

namespace XSerializer
{
    internal interface IValueConverterConfiguration
    {
        string ElementName { get; }
        Type[] ExtraTypes { get; }
        RedactAttribute RedactAttribute { get; }
        bool ShouldEncrypt { get; }
    }
}