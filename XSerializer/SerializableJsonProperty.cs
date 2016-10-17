using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace XSerializer
{
    internal class SerializableJsonProperty
    {
        private readonly JsonMappings _mappings;
        private readonly string _name;
        private readonly Lazy<IJsonSerializerInternal> _serializer;
        private readonly Func<object, object> _getValue;
        private readonly Action<object, object> _setValue;

        public SerializableJsonProperty(PropertyInfo propertyInfo, bool encrypt, JsonMappings mappings, bool shouldUseAttributeDefinedInInterface)
        {
            if (propertyInfo.DeclaringType == null)
            {
                throw new ArgumentException("The DeclaringType of the PropertyInfo must not be null.", "propertyInfo");
            }

            _mappings = mappings;
            _name = propertyInfo.GetName(shouldUseAttributeDefinedInInterface);

            var propertyType = propertyInfo.PropertyType;

            if (_mappings.MappingsByProperty.ContainsKey(propertyInfo))
            {
                propertyType = _mappings.MappingsByProperty[propertyInfo];
            }
            else if (_mappings.MappingsByType.ContainsKey(propertyType))
            {
                propertyType = _mappings.MappingsByType[propertyType];
            }
            else
            {
                var mappingAttribute = propertyInfo.GetCustomAttribute<JsonMappingAttribute>(shouldUseAttributeDefinedInInterface);
                if (mappingAttribute != null)
                {
                    propertyType = mappingAttribute.Type;
                }
            }

            _serializer = new Lazy<IJsonSerializerInternal>(() => JsonSerializerFactory.GetSerializer(propertyType, encrypt, _mappings, shouldUseAttributeDefinedInInterface));

            _getValue = GetGetValueFunc(propertyInfo, propertyInfo.DeclaringType);

            if (propertyInfo.IsReadWriteProperty())
            {
                _setValue = GetSetValueAction(propertyInfo, propertyInfo.DeclaringType);
            }
            else if (propertyInfo.IsJsonSerializableReadOnlyProperty())
            {
                // TODO: Before any of these checks, see if there is a constructor that matches this property. If so, don't do the "addable" deserialization technique.

                if (typeof(IDictionary).IsAssignableFrom(propertyType))
                {
                    _setValue = (instance, value) =>
                    {
                        var dictionary = (IDictionary)value;
                        var destinationDictionary = (IDictionary)_getValue(instance);

                        foreach (DictionaryEntry item in dictionary)
                        {
                            destinationDictionary.Add(item.Key, item.Value);
                        }
                    };
                }
                else if (propertyType.IsAssignableToGenericIDictionary())
                {
                    var valueParameter = Expression.Parameter(typeof(object), "value");

                    var enumerableType = propertyType.GetGenericIEnumerableType();
                    var convertValue = Expression.Convert(valueParameter, enumerableType);

                    var getEnumeratorMethod = enumerableType.GetMethod("GetEnumerator");
                    var callGetEnumerator = Expression.Call(convertValue, getEnumeratorMethod);

                    var getEnumeratorLambda =
                        Expression.Lambda<Func<object, IEnumerator>>(callGetEnumerator, valueParameter);

                    var getEnumerator = getEnumeratorLambda.Compile();

                    var itemParameter = Expression.Parameter(typeof(object), "item");

                    var dictionaryType = propertyType.GetGenericIDictionaryType();
                    var dictionaryGenericArguments = dictionaryType.GetGenericArguments();
                    var keyValuePairType =
                        typeof(KeyValuePair<,>).MakeGenericType(
                            dictionaryGenericArguments[0], dictionaryGenericArguments[1]);

                    var convertItem = Expression.Convert(itemParameter, keyValuePairType);

                    var keyPropertyInfo = keyValuePairType.GetProperty("Key");
                    Expression keyProperty = Expression.Property(convertItem, keyPropertyInfo);

                    if (keyPropertyInfo.PropertyType.IsValueType) // Boxing is required
                    {
                        keyProperty = Expression.Convert(keyProperty, typeof(object));
                    }

                    var getItemKeyLambda = Expression.Lambda<Func<object, object>>(keyProperty, itemParameter);
                    var getItemKey = getItemKeyLambda.Compile();

                    var valuePropertyInfo = keyValuePairType.GetProperty("Value");
                    Expression valueProperty = Expression.Property(convertItem, valuePropertyInfo);

                    if (valuePropertyInfo.PropertyType.IsValueType) // Boxing is required
                    {
                        valueProperty = Expression.Convert(valueProperty, typeof(object));
                    }

                    var getItemValueLambda = Expression.Lambda<Func<object, object>>(valueProperty, itemParameter);
                    var getItemValue = getItemValueLambda.Compile();

                    var destinationDictionaryParameter = Expression.Parameter(typeof(object), "destinationDictionary");
                    var keyParameter = Expression.Parameter(typeof(object), "key");

                    var convertDestinationDictionary = Expression.Convert(destinationDictionaryParameter, dictionaryType);
                    var convertKey = Expression.Convert(keyParameter, keyPropertyInfo.PropertyType);
                    convertValue = Expression.Convert(valueParameter, valuePropertyInfo.PropertyType);

                    var addMethod = dictionaryType.GetMethod("Add");
                    var callAddMethod = Expression.Call(
                        convertDestinationDictionary, addMethod, convertKey, convertValue);

                    var addLambda = Expression.Lambda<Action<object, object, object>>(
                        callAddMethod, destinationDictionaryParameter, keyParameter, valueParameter);
                    var add = addLambda.Compile();

                    _setValue = (instance, value) =>
                    {
                        var dictionaryEnumerator = getEnumerator(value);
                        var destinationDictionary = _getValue(instance);

                        while (dictionaryEnumerator.MoveNext())
                        {
                            add(destinationDictionary, getItemKey(dictionaryEnumerator.Current), getItemValue(dictionaryEnumerator.Current));
                        }
                    };
                }
                else if (!propertyType.IsArray)
                {
                    if (typeof(IList).IsAssignableFrom(propertyType))
                    {
                        _setValue = (instance, value) =>
                        {
                            var list = (IList)value;
                            var destinationList = (IList)_getValue(instance);

                            foreach (var item in list)
                            {
                                destinationList.Add(item);
                            }
                        };
                    }
                    else if (propertyType.IsAssignableToGenericICollection())
                    {
                        var destinationListParameter = Expression.Parameter(typeof(object), "destinationList");
                        var itemParameter = Expression.Parameter(typeof(object), "item");

                        var collectionType = propertyType.GetGenericICollectionType();

                        var itemType = collectionType.GetGenericArguments()[0];
                        var convertItemParameter = Expression.Convert(itemParameter, itemType);

                        var convertDestinationList = Expression.Convert(destinationListParameter, collectionType);

                        var addMethod = collectionType.GetMethod("Add");
                        var callAddMethod = Expression.Call(
                            convertDestinationList,
                            addMethod,
                            new Expression[] { convertItemParameter });

                        var lambda = Expression.Lambda<Action<object, object>>(
                            callAddMethod,
                            destinationListParameter,
                            itemParameter);
                        var add = lambda.Compile();

                        _setValue = (instance, value) =>
                        {
                            var destinationList = _getValue(instance);

                            foreach (var item in (IEnumerable)value)
                            {
                                add(destinationList, item);
                            }
                        };
                    }
                }
            }
            else
            {
                _setValue = (instance, value) => {};
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public void WriteValue(JsonWriter writer, object instance, IJsonSerializeOperationInfo info)
        {
            var value = _getValue(instance);
            writer.WriteValue(Name);
            writer.WriteNameValueSeparator();
            _serializer.Value.SerializeObject(writer, value, info);
        }

        public object ReadValue(JsonReader reader, IJsonSerializeOperationInfo info, string path)
        {
            return _serializer.Value.DeserializeObject(reader, info, path);
        }

        public void SetValue(object instance, JsonReader reader, IJsonSerializeOperationInfo info, string path)
        {
            var value = ReadValue(reader, info, path);
            _setValue(instance, value);
        }

        public void SetValue(object instance, object value)
        {
            _setValue(instance, value);
        }

        private static Func<object, object> GetGetValueFunc(PropertyInfo propertyInfo, Type declaringType)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");

            Expression property = Expression.Property(Expression.Convert(instanceParameter, declaringType), propertyInfo);

            if (propertyInfo.PropertyType.IsValueType) // Needs boxing.
            {
                property = Expression.Convert(property, typeof(object));
            }

            var lambda = Expression.Lambda<Func<object, object>>(property, new[] { instanceParameter });
            return lambda.Compile();
        }

        private static Action<object, object> GetSetValueAction(PropertyInfo propertyInfo, Type declaringType)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            Expression property = Expression.Property(Expression.Convert(instanceParameter, declaringType), propertyInfo);
            var assign = Expression.Assign(property, Expression.Convert(valueParameter, propertyInfo.PropertyType));

            var lambda = Expression.Lambda<Action<object, object>>(assign, new[] { instanceParameter, valueParameter });
            return lambda.Compile();
        }
    }
}