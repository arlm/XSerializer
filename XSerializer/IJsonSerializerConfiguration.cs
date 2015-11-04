using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using XSerializer.Encryption;

namespace XSerializer
{
    /// <summary>
    /// 
    /// </summary>
    public interface IJsonSerializerConfiguration
    {
        Encoding Encoding { get; }
        IEncryptionMechanism EncryptionMechanism { get; }
        object EncryptKey { get; }
        bool EncryptRootObject { get; }
        IDateTimeHandler DateTimeHandler { get; }
        IDictionary<Type, Type> ConcreteImplementationsByType { get; }
        IDictionary<PropertyInfo, Type> ConcreteImplementationsByProperty { get; }
    }
}