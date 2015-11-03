using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using XSerializer.Encryption;

namespace XSerializer
{
    public class JsonSerializerConfiguration : IJsonSerializerConfiguration
    {
        public JsonSerializerConfiguration()
        {
            Encoding = Encoding.UTF8;
            DateTimeHandler = XSerializer.DateTimeHandler.Default;
            ConcreteImplementationsByType = new Dictionary<Type, Type>();
            ConcreteImplementationsByProperty = new Dictionary<PropertyInfo, Type>();
        }

        public Encoding Encoding { get; set; }
        public bool RedactEnabled { get; set; }
        public IEncryptionMechanism EncryptionMechanism { get; set; }
        public object EncryptKey { get; set; }
        public bool EncryptRootObject { get; set; }
        public IDateTimeHandler DateTimeHandler { get; set; }
        public IDictionary<Type, Type> ConcreteImplementationsByType { get; set; }
        public IDictionary<PropertyInfo, Type> ConcreteImplementationsByProperty { get; set; }
    }
}