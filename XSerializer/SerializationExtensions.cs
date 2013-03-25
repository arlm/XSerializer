using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace XSerializer
{
    public static class SerializationExtensions
    {
        private static readonly Dictionary<Type, string> TypeToXsdTypeMap = new Dictionary<Type, string>();
        private static readonly Dictionary<string, Type> XsdTypeToTypeMap = new Dictionary<string, Type>();
        private static readonly Dictionary<int, Type> XsdTypeToTypeCache = new Dictionary<int, Type>();

        static SerializationExtensions()
        {
            TypeToXsdTypeMap.Add(typeof(bool), "xsd:boolean");
            TypeToXsdTypeMap.Add(typeof(byte), "xsd:unsignedByte");
            TypeToXsdTypeMap.Add(typeof(sbyte), "xsd:byte");
            TypeToXsdTypeMap.Add(typeof(short), "xsd:short");
            TypeToXsdTypeMap.Add(typeof(ushort), "xsd:unsignedShort");
            TypeToXsdTypeMap.Add(typeof(int), "xsd:int");
            TypeToXsdTypeMap.Add(typeof(uint), "xsd:unsignedInt");
            TypeToXsdTypeMap.Add(typeof(long), "xsd:long");
            TypeToXsdTypeMap.Add(typeof(ulong), "xsd:unsignedLong");
            TypeToXsdTypeMap.Add(typeof(float), "xsd:float");
            TypeToXsdTypeMap.Add(typeof(double), "xsd:double");
            TypeToXsdTypeMap.Add(typeof(decimal), "xsd:decimal");
            TypeToXsdTypeMap.Add(typeof(DateTime), "xsd:dateTime");
            TypeToXsdTypeMap.Add(typeof(string), "xsd:string");

            foreach (var typeToXsdType in TypeToXsdTypeMap)
            {
                XsdTypeToTypeMap.Add(typeToXsdType.Value, typeToXsdType.Key);
            }
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
            var isAssignableToIDictionary = typeof(IDictionary).IsAssignableFrom(type);
            return isAssignableToIDictionary;
        }

        internal static bool IsAssignableToGenericIDictionary(this Type type)
        {
            var isAssignableToGenericIDictionary =
                (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            return isAssignableToGenericIDictionary;
        }

        internal static bool IsAssignableToGenericIDictionaryWithKeyOrValueOfTypeObject(this Type type)
        {
            var isAssignableToGenericIDictionary = type.IsAssignableToGenericIDictionary();
            if (!isAssignableToGenericIDictionary)
            {
                return false;
            }

            var iEnumerableType = type.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            return iEnumerableType.GetGenericArguments()[0] == typeof(object) || iEnumerableType.GetGenericArguments()[1] == typeof(object);
        }

        internal static bool IsAssignableToNonGenericIEnumerable(this Type type)
        {
            var isAssignableToIEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
            return isAssignableToIEnumerable;
        }

        internal static bool IsAssignableToGenericIEnumerable(this Type type)
        {
            var isAssignableToGenericIEnumerable =
                (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return isAssignableToGenericIEnumerable;
        }

        internal static bool IsAssignableToGenericIEnumerableOfTypeObject(this Type type)
        {
            var isAssignableToGenericIEnumerable = type.IsAssignableToGenericIEnumerable();
            if (!isAssignableToGenericIEnumerable)
            {
                return false;
            }

            var iEnumerableType = type.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return iEnumerableType.GetGenericArguments()[0] == typeof(object);
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

        internal static bool IsPrimitiveLike(this Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
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
            if (TypeToXsdTypeMap.TryGetValue(type, out xsdType))
            {
                return xsdType;
            }

            return type.Name;
        }

        public static Type GetXsdType<T>(this XmlReader reader, Type[] extraTypes)
        {
            string typeName = null;

            while (reader.MoveToNextAttribute())
            {
                if (reader.LocalName == "type" && reader.LookupNamespace(reader.Prefix) == "http://www.w3.org/2001/XMLSchema-instance")
                {
                    typeName = reader.Value;
                    break;
                }
            }

            reader.MoveToElement();

            if (typeName == null)
            {
                return null;
            }

            Type type;

            if (XsdTypeToTypeMap.TryGetValue(typeName, out type))
            {
                return type;
            }

            var key = CreateTypeCacheKey<T>(typeName);
            if (!XsdTypeToTypeCache.TryGetValue(key, out type))
            {
                //// try REAL hard to get the type. (holy crap, this is UUUUUGLY!!!!)

                var matchingExtraTypes = extraTypes.Where(t => t.Name == typeName && typeof(T).IsAssignableFrom(t)).ToList();
                if (matchingExtraTypes.Count == 1)
                {
                    type = matchingExtraTypes[0];
                }

                if (type == null)
                {
                    var typeNameWithPossibleNamespace = typeName;

                    if (!typeName.Contains('.'))
                    {
                        typeNameWithPossibleNamespace = typeof(T).Namespace + "." + typeName;
                    }

                    var checkPossibleNamespace = typeName != typeNameWithPossibleNamespace;

                    type = Type.GetType(typeName);
                    type = typeof(T).IsAssignableFrom(type) ? type : null;

                    if (type == null)
                    {
                        type = checkPossibleNamespace ? Type.GetType(typeNameWithPossibleNamespace) : null;
                        type = typeof(T).IsAssignableFrom(type) ? type : null;

                        if (type == null)
                        {
                            type = typeof(T).Assembly.GetType(typeName);
                            type = typeof(T).IsAssignableFrom(type) ? type : null;

                            if (type == null)
                            {
                                type = checkPossibleNamespace ? typeof(T).Assembly.GetType(typeNameWithPossibleNamespace) : null;
                                type = typeof(T).IsAssignableFrom(type) ? type : null;

                                if (type == null)
                                {
                                    var matches = typeof(T).Assembly.GetTypes().Where(t => t.Name == typeName && typeof(T).IsAssignableFrom(t)).ToList();
                                    if (matches.Count == 1)
                                    {
                                        type = matches.Single();
                                    }

                                    var entryAssembly = Assembly.GetEntryAssembly();
                                    if (entryAssembly != null)
                                    {
                                        type = entryAssembly.GetType(typeName);
                                        type = typeof(T).IsAssignableFrom(type) ? type : null;

                                        if (type == null)
                                        {
                                            type = checkPossibleNamespace ? entryAssembly.GetType(typeNameWithPossibleNamespace) : null;
                                            type = typeof(T).IsAssignableFrom(type) ? type : null;
                                        }

                                        if (type == null)
                                        {
                                            matches = entryAssembly.GetTypes().Where(t => t.Name == typeName && typeof(T).IsAssignableFrom(t)).ToList();
                                            if (matches.Count == 1)
                                            {
                                                type = matches.Single();
                                            }
                                        }
                                    }

                                    if (type == null)
                                    {
                                        matches = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                                        {
                                            try
                                            {
                                                return a.GetTypes();
                                            }
                                            catch
                                            {
                                                return Enumerable.Empty<Type>();
                                            }
                                        }).Where(t => t.Name == typeName && typeof(T).IsAssignableFrom(t)).ToList();

                                        if (matches.Count == 1)
                                        {
                                            type = matches.Single();
                                        }
                                        else if (matches.Count > 1)
                                        {
                                            throw new SerializationException(string.Format("More than one type matches '{0}'. Consider decorating your type with the XmlIncludeAttribute, or pass in the type into the serializer as an extra type.", typeName));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (type == null)
                {
                    throw new SerializationException(string.Format("No suitable type matches '{0}'. Consider decorating your type with the XmlIncludeAttribute, or pass in the type into the serializer as an extra type.", typeName));
                }

                XsdTypeToTypeCache[key] = type;
            }

            return type;
        }

        private static int CreateTypeCacheKey<T>(string typeName)
        {
            unchecked
            {
                var key = typeof(T).GetHashCode();
                key = (key * 397) ^ typeName.GetHashCode();
                return key;
            }
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