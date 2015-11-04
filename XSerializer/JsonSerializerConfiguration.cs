using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using XSerializer.Encryption;

namespace XSerializer
{
    /// <summary>
    /// An implementation of <see cref="IJsonSerializerConfiguration"/> with read-write
    /// properties.
    /// </summary>
    public class JsonSerializerConfiguration : IJsonSerializerConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerConfiguration"/> class.
        /// </summary>
        public JsonSerializerConfiguration()
        {
            Encoding = Encoding.UTF8;
            DateTimeHandler = XSerializer.DateTimeHandler.Default;
            ConcreteImplementationsByType = new Dictionary<Type, Type>();
            ConcreteImplementationsByProperty = new Dictionary<PropertyInfo, Type>();
        }

        /// <summary>
        /// Gets the <see cref="IJsonSerializerConfiguration.Encoding"/> to be used for serialization operations.
        /// </summary>
        /// <remarks>The default value is <see cref="System.Text.Encoding.UTF8"/></remarks>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets an instance of <see cref="IEncryptionMechanism"/> to be used to encrypted properties
        /// decorated with the <see cref="EncryptAttribute"/> attribute. If null, those properties
        /// will not be encryped.
        /// </summary>
        /// <remarks>The default value is null.</remarks>
        public IEncryptionMechanism EncryptionMechanism { get; set; }

        /// <summary>
        /// Gets an object to be used to retrieve encryption settings for an encryption operation.
        /// </summary>
        /// <remarks>The default value is null.</remarks>
        public object EncryptKey { get; set; }

        /// <summary>
        /// Gets a value indicating whether the root object should be encrypted in its entirety.
        /// </summary>
        /// <remarks>The default value is false.</remarks>
        public bool EncryptRootObject { get; set; }

        /// <summary>
        /// Gets an object used to create <see cref="DateTime"/> values from a string representation.
        /// </summary>
        /// <remarks>The default value is <see cref="XSerializer.DateTimeHandler.Default"/>.</remarks>
        public IDateTimeHandler DateTimeHandler { get; set; }

        /// <summary>
        /// Gets a dictionary for type-to-type mappings, where the value type is assignable to the key
        /// type. This is typically a interface-to-implementation or astract-class-to-inheritor
        /// relationship, although a concrete-class-to-inheritor relationship is also possible.
        /// </summary>
        /// <remarks>The default value is a new <see cref="Dictionary{TKey,TValue}"/>.</remarks>
        public IDictionary<Type, Type> ConcreteImplementationsByType { get; set; }

        /// <summary>
        /// Gets a dictionary for property-to-type mappings, where the value type is assignable to
        /// the key property's type. This is typically a interface-to-implementation or
        /// astract-class-to-inheritor relationship, although a concrete-class-to-inheritor relationship
        /// is also possible.
        /// </summary>
        /// <remarks>The default value is a new <see cref="Dictionary{TKey,TValue}"/>.</remarks>
        public IDictionary<PropertyInfo, Type> ConcreteImplementationsByProperty { get; set; }
    }
}