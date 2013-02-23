using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public static class SerializationExtensions
    {
        public static string Serialize<T>(
            this IXmlSerializer<T> serializer,
            T instance,
            Encoding encoding,
            Formatting formatting,
            XmlSerializerNamespaces namespaces)
        {
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriterWithEncoding(sb, encoding ?? Encoding.UTF8))
            {
                using (var xmlWriter = new SerializationXmlTextWriter(stringWriter))
                {
                    xmlWriter.Formatting = formatting;
                    serializer.Serialize(instance, xmlWriter, namespaces);
                }
            }
            return sb.ToString();
        }

        public static string SerializeObject(
            this IXmlSerializer serializer,
            object instance,
            Encoding encoding,
            Formatting formatting,
            XmlSerializerNamespaces namespaces)
        {
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriterWithEncoding(sb, encoding ?? Encoding.UTF8))
            {
                using (var xmlWriter = new SerializationXmlTextWriter(stringWriter))
                {
                    xmlWriter.Formatting = formatting;
                    serializer.SerializeObject(instance, xmlWriter, namespaces);
                }
            }
            return sb.ToString();
        }

        public static void Serialize<T>(
            this IXmlSerializer<T> serializer,
            T instance,
            Stream stream,
            Encoding encoding,
            Formatting formatting,
            XmlSerializerNamespaces namespaces)
        {
            var xmlWriter = new SerializationXmlTextWriter(stream, encoding ?? Encoding.UTF8)
            {
                Formatting = formatting
            };
            serializer.Serialize(instance, xmlWriter, namespaces);
        }

        public static void SerializeObject(
            this IXmlSerializer serializer,
            object instance,
            Stream stream,
            Encoding encoding,
            Formatting formatting,
            XmlSerializerNamespaces namespaces)
        {
            var xmlWriter = new SerializationXmlTextWriter(stream, encoding ?? Encoding.UTF8)
            {
                Formatting = formatting
            };
            serializer.SerializeObject(instance, xmlWriter, namespaces);
        }

        public static void Serialize<T>(
            this IXmlSerializer<T> serializer,
            T instance,
            TextWriter writer,
            Formatting formatting,
            XmlSerializerNamespaces namespaces)
        {
            var xmlWriter = new SerializationXmlTextWriter(writer)
            {
                Formatting = formatting
            };
            serializer.Serialize(instance, xmlWriter, namespaces);
        }

        public static void SerializeObject(
            this IXmlSerializer serializer,
            object instance,
            TextWriter writer,
            Formatting formatting,
            XmlSerializerNamespaces namespaces)
        {
            var xmlWriter = new SerializationXmlTextWriter(writer)
            {
                Formatting = formatting
            };
            serializer.SerializeObject(instance, xmlWriter, namespaces);
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

        public static bool HasDefaultConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsSerializable(this PropertyInfo property)
        {
            var isSerializable = property.GetIndexParameters().Length == 0 && (property.IsReadWriteProperty() || property.IsSerializableReadOnlyProperty());
            return isSerializable;
        }

        public static bool IsReadWriteProperty(this PropertyInfo property)
        {
            var isReadWriteProperty = property.CanCallGetter() && property.CanCallSetter();
            return isReadWriteProperty;
        }

        public static bool IsSerializableReadOnlyProperty(this PropertyInfo property)
        {
            var canCallGetter = property.CanCallGetter();
            var canCallSetter = property.CanCallSetter();

            return
                (canCallGetter && !canCallSetter)
                && (property.PropertyType.IsAssignableToNonGenericIDictionary()
                    || property.PropertyType.IsAssignableToGenericIDictionary()); // TODO: add additional serializable types?
        }

        public static bool IsAssignableToNonGenericIDictionary(this Type type)
        {
            var isAssignableFromIDictionary = typeof(IDictionary).IsAssignableFrom(type);
            return isAssignableFromIDictionary;
        }

        public static bool IsAssignableToGenericIDictionary(this Type type)
        {
            var isAssignableFromGenericIDictionary =
                (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            return isAssignableFromGenericIDictionary;
        }

        public static Type GetGenericIDictionaryType(this Type type)
        {
            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                return type;
            }

            return type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public static bool CanCallGetter(this PropertyInfo property)
        {
            var canCallGetter = property.CanRead && property.GetGetMethod() != null;
            return canCallGetter;
        }

        public static bool CanCallSetter(this PropertyInfo property)
        {
            var canCallSetter = property.CanWrite && property.GetSetMethod() != null;
            return canCallSetter;
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