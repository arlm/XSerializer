using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    internal static class SerializationExtensions
    {
        internal const string ReadOnlyDictionary = "System.Collections.ObjectModel.ReadOnlyDictionary`2, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        internal const string IReadOnlyDictionary = "System.Collections.Generic.IReadOnlyDictionary`2, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        internal const string IReadOnlyCollection = "System.Collections.Generic.IReadOnlyCollection`1, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        internal const string IReadOnlyList = "System.Collections.Generic.IReadOnlyList`1, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

        private static readonly Dictionary<Type, string> _typeToXsdTypeMap = new Dictionary<Type, string>();
        private static readonly Dictionary<string, Type> _xsdTypeToTypeMap = new Dictionary<string, Type>();
        private static readonly ConcurrentDictionary<int, Type> _xsdTypeToTypeCache = new ConcurrentDictionary<int, Type>();

        static SerializationExtensions()
        {
            _typeToXsdTypeMap.Add(typeof(bool), "xsd:boolean");
            _typeToXsdTypeMap.Add(typeof(byte), "xsd:unsignedByte");
            _typeToXsdTypeMap.Add(typeof(sbyte), "xsd:byte");
            _typeToXsdTypeMap.Add(typeof(short), "xsd:short");
            _typeToXsdTypeMap.Add(typeof(ushort), "xsd:unsignedShort");
            _typeToXsdTypeMap.Add(typeof(int), "xsd:int");
            _typeToXsdTypeMap.Add(typeof(uint), "xsd:unsignedInt");
            _typeToXsdTypeMap.Add(typeof(long), "xsd:long");
            _typeToXsdTypeMap.Add(typeof(ulong), "xsd:unsignedLong");
            _typeToXsdTypeMap.Add(typeof(float), "xsd:float");
            _typeToXsdTypeMap.Add(typeof(double), "xsd:double");
            _typeToXsdTypeMap.Add(typeof(decimal), "xsd:decimal");
            _typeToXsdTypeMap.Add(typeof(DateTime), "xsd:dateTime");
            _typeToXsdTypeMap.Add(typeof(string), "xsd:string");

            foreach (var typeToXsdType in _typeToXsdTypeMap)
            {
                _xsdTypeToTypeMap.Add(typeToXsdType.Value, typeToXsdType.Key);
            }
        }

        public static string Serialize<T>(
            this IXmlSerializerInternal<T> serializer,
            T instance,
            Encoding encoding,
            Formatting formatting,
            ISerializeOptions options)
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriterWithEncoding(sb, encoding ?? Encoding.UTF8))
            {
                using (var xmlWriter = new SerializationXmlTextWriter(stringWriter))
                {
                    xmlWriter.Formatting = formatting;
                    serializer.Serialize(xmlWriter, instance, options);
                }
            }

            return sb.ToString();
        }

        public static string SerializeObject(
            this IXmlSerializerInternal serializer,
            object instance,
            Encoding encoding,
            Formatting formatting,
            ISerializeOptions options)
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriterWithEncoding(sb, encoding ?? Encoding.UTF8))
            {
                using (var xmlWriter = new SerializationXmlTextWriter(stringWriter))
                {
                    xmlWriter.Formatting = formatting;
                    serializer.SerializeObject(xmlWriter, instance, options);
                }
            }

            return sb.ToString();
        }

        public static void Serialize<T>(
            this IXmlSerializerInternal<T> serializer,
            Stream stream,
            T instance,
            Encoding encoding,
            Formatting formatting,
            ISerializeOptions options)
        {
            StreamWriter streamWriter = null;

            try
            {
                streamWriter = new StreamWriter(stream, encoding ?? Encoding.UTF8);

                var xmlWriter = new SerializationXmlTextWriter(streamWriter)
                {
                    Formatting = formatting
                };

                serializer.Serialize(xmlWriter, instance, options);
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Flush();
                }
            }
        }

        public static void Serialize<T>(
            this IXmlSerializerInternal<T> serializer,
            TextWriter writer,
            T instance,
            Formatting formatting,
            ISerializeOptions options)
        {
            var xmlWriter = new SerializationXmlTextWriter(writer)
            {
                Formatting = formatting
            };

            serializer.Serialize(xmlWriter, instance, options);
        }

        public static T Deserialize<T>(this IXmlSerializerInternal<T> serializer, string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    return serializer.Deserialize(xmlReader);
                }
            }
        }

        public static object DeserializeObject(this IXmlSerializerInternal serializer, string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    return serializer.DeserializeObject(xmlReader);
                }
            }
        }

        public static T Deserialize<T>(this IXmlSerializerInternal<T> serializer, Stream stream)
        {
            var xmlReader = new XmlTextReader(stream);
            return serializer.Deserialize(xmlReader);
        }

        public static object DeserializeObject(this IXmlSerializerInternal serializer, Stream stream)
        {
            var xmlReader = new XmlTextReader(stream);
            return serializer.DeserializeObject(xmlReader);
        }

        public static T Deserialize<T>(this IXmlSerializerInternal<T> serializer, TextReader reader)
        {
            var xmlReader = new XmlTextReader(reader);
            return serializer.Deserialize(xmlReader);
        }

        public static object DeserializeObject(this IXmlSerializerInternal serializer, TextReader reader)
        {
            var xmlReader = new XmlTextReader(reader);
            return serializer.DeserializeObject(xmlReader);
        }

        internal static bool HasDefaultConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsSerializable(this PropertyInfo property, IEnumerable<ParameterInfo> constructorParameters)
        {
            if (property.DeclaringType.IsAnonymous())
            {
                return true;
            }

            if (Attribute.IsDefined(property, typeof(XmlIgnoreAttribute)))
            {
                return false;
            }

            var isSerializable = property.GetIndexParameters().Length == 0 && (property.IsReadWriteProperty() || property.IsSerializableReadOnlyProperty(constructorParameters));
            return isSerializable;
        }

        internal static bool IsReadWriteProperty(this PropertyInfo property)
        {
            var isReadWriteProperty = property.HasPublicGetter() && property.HasPublicSetter();
            return isReadWriteProperty;
        }

        internal static bool IsSerializableReadOnlyProperty(this PropertyInfo property, IEnumerable<ParameterInfo> constructorParameters = null)
        {
            return
                property.IsReadOnlyProperty()
                &&
                !IsConditionalProperty(property, constructorParameters)
                &&
                (
                    ((constructorParameters ?? Enumerable.Empty<ParameterInfo>()).Any(p => p.Name.ToLower() == property.Name.ToLower() && p.ParameterType == property.PropertyType))
                    ||
                    (property.PropertyType.IsAnyKindOfDictionary() && property.PropertyType != typeof(ExpandoObject))
                    ||
                    (property.PropertyType.IsAssignableToGenericIEnumerable() && property.PropertyType.HasAddMethodOfType(property.PropertyType.GetGenericIEnumerableType().GetGenericArguments()[0]))
                    ||
                    (property.PropertyType.IsAssignableToNonGenericIEnumerable() && property.PropertyType.HasAddMethod())
                    ||
                    (property.PropertyType.IsReadOnlyCollection())
                    ||
                    (property.PropertyType.IsArray && property.PropertyType.GetArrayRank() == 1)
                    ||
                    (property.PropertyType.IsReadOnlyDictionary())
                ); // TODO: add additional serializable types?
        }

        internal static bool IsReadOnlyCollection(this Type type)
        {
            var openGenerics = GetOpenGenerics(type);

            foreach (var openGeneric in openGenerics)
            {
                if (openGeneric == typeof(ReadOnlyCollection<>))
                {
                    return true;
                }

                switch (openGeneric.AssemblyQualifiedName)
                {
                    case IReadOnlyCollection:
                    case IReadOnlyList:
                        return true;
                }
            }

            return false;
        }

        internal static bool IsReadOnlyDictionary(this Type type)
        {
            var openGenerics = GetOpenGenerics(type);

            foreach (var openGeneric in openGenerics)
            {
                switch (openGeneric.AssemblyQualifiedName)
                {
                    case IReadOnlyDictionary:
                    case ReadOnlyDictionary:
                        return true;
                }
            }

            return false;
        }

        internal static Type GetIReadOnlyDictionaryInterface(this Type type)
        {
            var iReadOnlyDictionaryType = Type.GetType(IReadOnlyDictionary);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == iReadOnlyDictionaryType)
            {
                return type;
            }

            return
                type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == iReadOnlyDictionaryType);
        }

        private static IEnumerable<Type> GetOpenGenerics(Type type)
        {
            if (type.IsGenericType)
            {
                yield return type.GetGenericTypeDefinition();
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType)
                {
                    yield return interfaceType;
                }
            }

            while (true)
            {
                type = type.BaseType;

                if (type == null)
                {
                    yield break;
                }

                if (type.IsGenericType)
                {
                    yield return type.GetGenericTypeDefinition();
                }
            }
        }

        private static bool IsConditionalProperty(PropertyInfo property, IEnumerable<ParameterInfo> constructorParameters)
        {
            if (property.Name.EndsWith("Specified"))
            {
                var otherPropertyName = property.Name.Substring(0, property.Name.LastIndexOf("Specified"));
                return property.DeclaringType.GetProperties().Any(p => p.Name == otherPropertyName);
            }

            return false;
        }

        internal static bool HasAddMethodOfType(this Type type, Type addMethodType)
        {
            return type.GetMethod("Add", new[] { addMethodType }) != null;
        }

        internal static bool HasAddMethod(this Type type)
        {
            return type.GetMethods().Any(m => m.Name == "Add" && m.GetParameters().Length == 1);
        }

        internal static bool IsAnyKindOfDictionary(this Type type)
        {
            return type.IsAssignableToNonGenericIDictionary() || type.IsAssignableToGenericIDictionary();
        }

        internal static bool IsReadOnlyProperty(this PropertyInfo property)
        {
            var canCallGetter = property.HasPublicGetter();
            var canCallSetter = property.HasPublicSetter();

            return canCallGetter && !canCallSetter;
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

            Type iDictionaryType;
            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                iDictionaryType = type;
            }
            else
            {
                iDictionaryType = type.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            }
                
            return iDictionaryType.GetGenericArguments()[0] == typeof(object) || iDictionaryType.GetGenericArguments()[1] == typeof(object);
        }

        internal static bool IsAssignableToNonGenericIEnumerable(this Type type)
        {
            var isAssignableToIEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
            return isAssignableToIEnumerable;
        }

        internal static bool IsGenericIEnumerable(this Type type)
        {
            return type.IsInterface
                && type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        internal static bool IsAssignableToGenericIEnumerable(this Type type)
        {
            var isAssignableToGenericIEnumerable =
                type.IsGenericIEnumerable()
                || type.GetInterfaces().Any(i => i.IsGenericIEnumerable());
            return isAssignableToGenericIEnumerable;
        }

        internal static bool IsAssignableToGenericIEnumerableOfTypeObject(this Type type)
        {
            var isAssignableToGenericIEnumerable = type.IsAssignableToGenericIEnumerable();
            if (!isAssignableToGenericIEnumerable)
            {
                return false;
            }

            var iEnumerableType =
                type.IsGenericIEnumerable()
                    ? type
                    : type.GetInterfaces().Single(i => i.IsGenericIEnumerable());

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

        internal static Type GetGenericIEnumerableType(this Type type)
        {
            if (type.IsGenericIEnumerable())
            {
                return type;
            }

            return type.GetInterfaces().First(i => i.IsGenericIEnumerable());
        }

        private static bool HasPublicGetter(this PropertyInfo property)
        {
            if (!property.CanRead)
            {
                return false;
            }

            var getMethod = property.GetGetMethod();

            return getMethod != null && getMethod.IsPublic;
        }

        private static bool HasPublicSetter(this PropertyInfo property)
        {
            if (!property.CanWrite)
            {
                return false;
            }

            var setMethod = property.GetSetMethod();

            return setMethod != null && setMethod.IsPublic;
        }

        internal static bool ReadIfNeeded(this XmlReader reader, bool shouldRead)
        {
            if (shouldRead)
            {
                return reader.Read();
            }

            return true;
        }

        internal static bool IsNil(this XmlReader reader)
        {
            var nilFound = false;

            while (reader.MoveToNextAttribute())
            {
                if (reader.LocalName == "nil" && reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema-instance")
                {
                    nilFound = true;
                    break;
                }
            }

            reader.MoveToElement();

            return nilFound;
        }

        internal static void WriteNilAttribute(this XmlWriter writer)
        {
            writer.WriteAttributeString("xsi", "nil", null, "true");
        }

        internal static bool IsPrimitiveLike(this Type type)
        {
            return
                type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(Guid);
        }

        internal static bool IsNullablePrimitiveLike(this Type type)
        {
            return
                type.IsNullableType()
                && type.GetGenericArguments()[0].IsPrimitiveLike();
        }

        internal static bool IsNullableType(this Type type)
        {
            return type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static bool IsReferenceType(this Type type)
        {
            return !type.IsValueType;
        }

        internal static object GetUninitializedObject(this Type type)
        {
            return FormatterServices.GetUninitializedObject(type);
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

        public static string GetElementName(this Type type)
        {
            if (type.IsGenericType)
            {
                return type.Name.Substring(0, type.Name.IndexOf("`")) + "Of" + string.Join("_", type.GetGenericArguments().Select(x => x.GetElementName()));
            }

            if (type.IsArray)
            {
                return "ArrayOf" + type.GetElementType().Name;
            }
            
            return type.Name;
        }

        public static string GetXsdType(this Type type)
        {
            string xsdType;
            if (_typeToXsdTypeMap.TryGetValue(type, out xsdType))
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

            Type typeFromXsdType;

            if (_xsdTypeToTypeMap.TryGetValue(typeName, out typeFromXsdType))
            {
                return typeFromXsdType;
            }

            return _xsdTypeToTypeCache.GetOrAdd(
                CreateTypeCacheKey<T>(typeName),
                _ =>
                {
                    Type type = null;

                    //// try REAL hard to get the type. (holy crap, this is UUUUUGLY!!!!)

                    if (extraTypes != null)
                    {
                        var matchingExtraTypes = extraTypes.Where(t => t.Name == typeName && typeof(T).IsAssignableFrom(t)).ToList();
                        if (matchingExtraTypes.Count == 1)
                        {
                            type = matchingExtraTypes[0];
                        }
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

                    return type;
                });
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