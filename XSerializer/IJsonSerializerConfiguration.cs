using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using XSerializer.Encryption;

namespace XSerializer
{
    public interface IJsonSerializerConfiguration
    {
        Encoding Encoding { get; }
        bool RedactEnabled { get; }
        IEncryptionMechanism EncryptionMechanism { get; }
        object EncryptKey { get; }
        bool EncryptRootObject { get; }
        IDateTimeHandler DateTimeHandler { get; }
        IDictionary<Type, Type> ConcreteImplementationsByType { get; }
        IDictionary<PropertyInfo, Type> ConcreteImplementationsByProperty { get; }
    }
}