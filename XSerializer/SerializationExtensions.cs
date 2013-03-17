using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public static class SerializationExtensions
    {
        private static readonly Dictionary<Type, string> XsdTypeMap = new Dictionary<Type, string>();

        static SerializationExtensions()
        {
            XsdTypeMap.Add(typeof(bool), "xsd:boolean");
            XsdTypeMap.Add(typeof(byte), "xsd:unsignedByte");
            XsdTypeMap.Add(typeof(sbyte), "xsd:byte");
            XsdTypeMap.Add(typeof(short), "xsd:short");
            XsdTypeMap.Add(typeof(ushort), "xsd:unsignedShort");
            XsdTypeMap.Add(typeof(int), "xsd:int");
            XsdTypeMap.Add(typeof(uint), "xsd:unsignedInt");
            XsdTypeMap.Add(typeof(long), "xsd:long");
            XsdTypeMap.Add(typeof(ulong), "xsd:unsignedLong");
            XsdTypeMap.Add(typeof(float), "xsd:float");
            XsdTypeMap.Add(typeof(double), "xsd:double");
            XsdTypeMap.Add(typeof(decimal), "xsd:decimal");
            XsdTypeMap.Add(typeof(DateTime), "xsd:dateTime");
            XsdTypeMap.Add(typeof(string), "xsd:string");
        }

        public static string Serialize<T>(
            this IXmlSerializer<T> serializer,
            T instance,
            XmlSerializerNamespaces namespaces,
            Encoding encoding,
            Formatting formatting,
            bool alwaysEmitTypes)
        {
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriterWithEncoding(sb, encoding ?? Encoding.UTF8))
            {
                using (var xmlWriter = new SerializationXmlTextWriter(stringWriter))
                {
                    xmlWriter.Formatting = formatting;
                    serializer.Serialize(xmlWriter, instance, namespaces, alwaysEmitTypes);
                }
            }

            return sb.ToString();
        }

        public static string Serialize<T>(
            this IXmlSerializer<T> serializer,
            T instance,
            XmlSerializerNamespaces namespaces,
            Encoding encoding,
            Formatting formatting)
        {
            return serializer.Serialize(instance, namespaces, encoding, formatting, false);
        }

        public static string SerializeObject(
            this IXmlSerializer serializer,
            object instance,
            XmlSerializerNamespaces namespaces,
            Encoding encoding,
            Formatting formatting,
            bool alwaysEmitTypes)
        {
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriterWithEncoding(sb, encoding ?? Encoding.UTF8))
            {
                using (var xmlWriter = new SerializationXmlTextWriter(stringWriter))
                {
                    xmlWriter.Formatting = formatting;
                    serializer.SerializeObject(xmlWriter, instance, namespaces, alwaysEmitTypes);
                }
            }

            return sb.ToString();
        }

        public static string SerializeObject(
            this IXmlSerializer serializer,
            object instance,
            XmlSerializerNamespaces namespaces,
            Encoding encoding,
            Formatting formatting)
        {
            return serializer.SerializeObject(instance, namespaces, encoding, formatting, false);
        }

        public static void Serialize<T>(
            this IXmlSerializer<T> serializer,
            Stream stream,
            T instance,
            XmlSerializerNamespaces namespaces,
            Encoding encoding,
            Formatting formatting,
            bool alwaysEmitTypes)
        {
            var xmlWriter = new SerializationXmlTextWriter(stream, encoding ?? Encoding.UTF8)
            {
                Formatting = formatting
            };

            serializer.Serialize(xmlWriter, instance, namespaces, alwaysEmitTypes);
        }

        public static void Serialize<T>(
            this IXmlSerializer<T> serializer,
            Stream stream,
            T instance,
            XmlSerializerNamespaces namespaces,
            Encoding encoding,
            Formatting formatting)
        {
            serializer.Serialize(stream, instance, namespaces, encoding, formatting, false);
        }

        public static void SerializeObject(
            this IXmlSerializer serializer,
            Stream stream,
            object instance,
            XmlSerializerNamespaces namespaces,
            Encoding encoding,
            Formatting formatting,
            bool alwaysEmitTypes)
        {
            var xmlWriter = new SerializationXmlTextWriter(stream, encoding ?? Encoding.UTF8)
            {
                Formatting = formatting
            };

            serializer.SerializeObject(xmlWriter, instance, namespaces, alwaysEmitTypes);
        }

        public static void SerializeObject(
            this IXmlSerializer serializer,
            Stream stream,
            object instance,
            XmlSerializerNamespaces namespaces,
            Encoding encoding,
            Formatting formatting)
        {
            serializer.SerializeObject(stream, instance, namespaces, encoding, formatting, false);
        }

        public static void Serialize<T>(
            this IXmlSerializer<T> serializer,
            TextWriter writer,
            T instance,
            XmlSerializerNamespaces namespaces,
            Formatting formatting,
            bool alwaysEmitTypes)
        {
            var xmlWriter = new SerializationXmlTextWriter(writer)
            {
                Formatting = formatting
            };

            serializer.Serialize(xmlWriter, instance, namespaces, alwaysEmitTypes);
        }

        public static void Serialize<T>(
            this IXmlSerializer<T> serializer,
            TextWriter writer,
            T instance,
            XmlSerializerNamespaces namespaces,
            Formatting formatting)
        {
            serializer.Serialize(writer, instance, namespaces, formatting, false);
        }

        public static void SerializeObject(
            this IXmlSerializer serializer,
            TextWriter writer,
            object instance,
            XmlSerializerNamespaces namespaces,
            Formatting formatting,
            bool alwaysEmitTypes)
        {
            var xmlWriter = new SerializationXmlTextWriter(writer)
            {
                Formatting = formatting
            };

            serializer.SerializeObject(xmlWriter, instance, namespaces, alwaysEmitTypes);
        }

        public static void SerializeObject(
            this IXmlSerializer serializer,
            TextWriter writer,
            object instance,
            XmlSerializerNamespaces namespaces,
            Formatting formatting)
        {
            serializer.SerializeObject(writer, instance, namespaces, formatting, false);
        }

        public static T Deserialize<T>(this IXmlSerializer<T> serializer, string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    return serializer.Deserialize(xmlReader);
                }
            }
        }

        public static object DeserializeObject(this IXmlSerializer serializer, string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    return serializer.DeserializeObject(xmlReader);
                }
            }
        }

        public static T Deserialize<T>(this IXmlSerializer<T> serializer, Stream stream)
        {
            var xmlReader = new XmlTextReader(stream);
            return serializer.Deserialize(xmlReader);
        }

        public static object DeserializeObject(this IXmlSerializer serializer, Stream stream)
        {
            var xmlReader = new XmlTextReader(stream);
            return serializer.DeserializeObject(xmlReader);
        }

        public static T Deserialize<T>(this IXmlSerializer<T> serializer, TextReader reader)
        {
            var xmlReader = new XmlTextReader(reader);
            return serializer.Deserialize(xmlReader);
        }

        public static object DeserializeObject(this IXmlSerializer serializer, TextReader reader)
        {
            var xmlReader = new XmlTextReader(reader);
            return serializer.DeserializeObject(xmlReader);
        }

        internal static bool HasDefaultConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsSerializable(this PropertyInfo property)
        {
            if (property.DeclaringType.IsAnonymous())
            {
                return true;
            }

            var isSerializable = property.GetIndexParameters().Length == 0 && (property.IsReadWriteProperty() || property.IsSerializableReadOnlyProperty());
            return isSerializable;
        }

        internal static bool IsReadWriteProperty(this PropertyInfo property)
        {
            var isReadWriteProperty = property.CanCallGetter() && property.CanCallSetter();
            return isReadWriteProperty;
        }

        internal static bool IsSerializableReadOnlyProperty(this PropertyInfo property)
        {
            var canCallGetter = property.CanCallGetter();
            var canCallSetter = property.CanCallSetter();

            return
                (canCallGetter && !canCallSetter)
                && (property.PropertyType.IsAssignableToNonGenericIDictionary()
                    || property.PropertyType.IsAssignableToGenericIDictionary()) // TODO: add additional serializable types?
                && property.PropertyType != typeof(ExpandoObject);
        }

        internal static bool IsAssignableToNonGenericIDictionary(this Type type)
        {
            var isAssignableFromIDictionary = typeof(IDictionary).IsAssignableFrom(type);
            return isAssignableFromIDictionary;
        }

        internal static bool IsAssignableToGenericIDictionary(this Type type)
        {
            var isAssignableFromGenericIDictionary =
                (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            return isAssignableFromGenericIDictionary;
        }

        internal static Type GetGenericIDictionaryType(this Type type)
        {
            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                return type;
            }

            return type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        internal static bool CanCallGetter(this PropertyInfo property)
        {
            var canCallGetter = property.CanRead && property.GetGetMethod() != null;
            return canCallGetter;
        }

        internal static bool CanCallSetter(this PropertyInfo property)
        {
            var canCallSetter = property.CanWrite && property.GetSetMethod() != null;
            return canCallSetter;
        }

        internal static bool ReadIfNeeded(this XmlReader reader, bool shouldRead)
        {
            if (shouldRead)
            {
                return reader.Read();
            }

            return true;
        }

        public static bool IsAnonymous(this object instance)
        {
            if (instance == null)
            {
                return false;
            }

            return instance.GetType().IsAnonymous();
        }

        public static bool IsAnonymous(this Type type)
        {
            return
                type.Namespace == null
                && type.IsClass
                && type.IsNotPublic
                && type.IsSealed
                && type.DeclaringType == null
                && type.BaseType == typeof(object)
                && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                && type.Name.Contains("AnonymousType")
                && Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute));
        }

        public static string GetXsdType(this Type type)
        {
            string xsdType;
            if (XsdTypeMap.TryGetValue(type, out xsdType))
            {
                return xsdType;
            }

            return type.Name;
        }

        private class StringWriterWithEncoding : StringWriter
        {
            private readonly Encoding _encoding;

            public StringWriterWithEncoding(StringBuilder sb, Encoding encoding)
                : base(sb)
            {
                _encoding = encoding;
            }

            public override Encoding Encoding
            {
                get { return _encoding; }
            }
        }
    }
}