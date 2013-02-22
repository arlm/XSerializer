using System;
using System.Collections.Generic;
using System.Linq;

namespace XSerializer
{
    public class XmlSerializerFactory
    {
        private readonly Dictionary<Type, Func<string, Type[], string, IXmlSerializer>> _getSerializerMap = new Dictionary<Type, Func<string, Type[], string, IXmlSerializer>>();
        private readonly Dictionary<int, IXmlSerializer> _serializerCache = new Dictionary<int, IXmlSerializer>();

        public static readonly XmlSerializerFactory Instance = new XmlSerializerFactory();

        public IXmlSerializer GetSerializer(Type type, string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            Func<string, Type[], string, IXmlSerializer> getSerializer;
            if (!_getSerializerMap.TryGetValue(type, out getSerializer))
            {
                var getSerializerMethod =
                    typeof(XmlSerializerFactory)
                        .GetMethod("GetSerializer", new[] { typeof(string), typeof(Type[]), typeof(string) })
                        .MakeGenericMethod(type);

                getSerializer =
                    (Func<string, Type[], string, IXmlSerializer>)Delegate.CreateDelegate(
                        typeof(Func<string, Type[], string, IXmlSerializer>), this, getSerializerMethod);
                _getSerializerMap[type] = getSerializer;
            }

            return getSerializer(defaultNamespace, extraTypes, rootElementName);
        }

        public IXmlSerializer<T> GetSerializer<T>(string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            IXmlSerializer<T> serializer;

            if (!TryGetCachedSerializer(defaultNamespace, extraTypes, rootElementName, out serializer))
            {
                if (!TryGetDefaultSerializer(defaultNamespace, extraTypes, rootElementName, out serializer))
                {
                    serializer = (IXmlSerializer<T>)CustomSerializer.GetSerializer(typeof(T), defaultNamespace, extraTypes, rootElementName);
                }

                CacheSerializer(defaultNamespace, extraTypes, rootElementName, serializer);
            }

            return serializer;
        }

        protected virtual bool TryGetCachedSerializer<T>(string defaultNamespace, Type[] extraTypes, string rootElementName, out IXmlSerializer<T> serializer)
        {
            IXmlSerializer serializerObject;
            if (_serializerCache.TryGetValue(CreateKey(typeof(T), defaultNamespace, extraTypes, rootElementName), out serializerObject))
            {
                serializer = (IXmlSerializer<T>)serializerObject;
                return true;
            }

            serializer = null;
            return false;
        }

        protected virtual bool TryGetDefaultSerializer<T>(string defaultNamespace, Type[] extraTypes, string rootElementName, out IXmlSerializer<T> serializer)
        {
            serializer = (IXmlSerializer<T>)DefaultSerializer.GetSerializer(typeof(T), defaultNamespace, extraTypes, rootElementName);
            return serializer != null;
        }

        protected virtual void CacheSerializer<T>(string defaultNamespace, Type[] extraTypes, string rootElementName, IXmlSerializer<T> serializer)
        {
            _serializerCache[CreateKey(typeof(T), defaultNamespace, extraTypes, rootElementName)] = serializer;
        }

        internal int CreateKey(Type type, string defaultNamespace, Type[] extraTypes, string rootElementName)
        {
            unchecked
            {
                var key = type.GetHashCode();

                key = (key * 397) ^ (string.IsNullOrWhiteSpace(defaultNamespace) ? "" : defaultNamespace).GetHashCode();

                if (extraTypes != null)
                {
                    key = extraTypes
                        .Where(extraType => extraType != null)
                        .Distinct(EqualityComparer<Type>.Default)
                        .OrderBy(extraType => extraType.FullName)
                        .Aggregate(key, (current, extraType) => (current * 397) ^ extraType.GetHashCode());
                }

                key = (key * 397) ^ (string.IsNullOrWhiteSpace(rootElementName) ? type.Name : rootElementName).GetHashCode();

                return key;
            }
        }
    }
}