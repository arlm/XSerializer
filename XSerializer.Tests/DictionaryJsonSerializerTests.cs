using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using XSerializer.Encryption;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class DictionaryJsonSerializerTests
    {
        [Test]
        public void CanSerializeIDictionaryOfStringToObject()
        {
            var serializer = new JsonSerializer<IDictionary<string, object>>();

            var json = serializer.Serialize(new Dictionary<string, object>{ { "foo", "abc" }, { "bar", 123.45 } });

            Assert.That(json, Is.EqualTo(@"{""foo"":""abc"",""bar"":123.45}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToObjectEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<IDictionary<string, object>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, object> { { "foo", "abc" }, { "bar", 123.45 } });

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"{""foo"":""abc"",""bar"":123.45}")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void ForIDictionaryOfStringToObjectAValueWhoseTypeIsDecoratedWithTheEncryptAttributeIsEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<IDictionary<string, object>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, object> { { "foo", new Foo { Bar = "abc", Baz = true } } });

            var expected =
                @"{""foo"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Bar"":""abc"",""Baz"":true}")
                    + @""""
                + "}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void ForIDictionaryOfStringToObjectAValueHasAPropertyDecoratedWithTheEncryptAttributeHasThatPropertyEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<IDictionary<string, object>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, object> { { "foo", new Qux { Bar = "abc", Baz = true } } });

            var expected =
                @"{""foo"":{""Bar"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"""abc""")
                    + @""""
                + @",""Baz"":true}}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToString()
        {
            var serializer = new JsonSerializer<IDictionary<string, string>>();

            var json = serializer.Serialize(new Dictionary<string, string> { { "foo", "abc" }, { "bar", null } });

            Assert.That(json, Is.EqualTo(@"{""foo"":""abc"",""bar"":null}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToStringEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<IDictionary<string, string>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, string> { { "foo", "abc" }, { "bar", null } });

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"{""foo"":""abc"",""bar"":null}")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToCustomType()
        {
            var serializer = new JsonSerializer<IDictionary<string, Corge>>();

            var json = serializer.Serialize(new Dictionary<string, Corge> { { "foo", new Corge { Bar = "abc", Baz = true } } });

            Assert.That(json, Is.EqualTo(@"{""foo"":{""Bar"":""abc"",""Baz"":true}}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfCustomTypeToCustomType()
        {
            var serializer = new JsonSerializer<IDictionary<Grault, Corge>>();

            var json = serializer.Serialize(new Dictionary<Grault, Corge> { { new Grault { Bar = "xyz", Baz = false }, new Corge { Bar = "abc", Baz = true } } });

            Assert.That(json, Is.EqualTo(@"{""{\""Bar\"":\""xyz\"",\""Baz\"":false}"":{""Bar"":""abc"",""Baz"":true}}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfValueTypeToCustomType()
        {
            var serializer = new JsonSerializer<IDictionary<int, Corge>>();

            var json = serializer.Serialize(new Dictionary<int, Corge> { { 1, new Corge { Bar = "abc", Baz = true } } });

            Assert.That(json, Is.EqualTo(@"{""1"":{""Bar"":""abc"",""Baz"":true}}"));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToCustomTypeEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<IDictionary<string, Corge>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, Corge> { { "foo", new Corge { Bar = "abc", Baz = true } }});

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"{""foo"":{""Bar"":""abc"",""Baz"":true}}")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToCustomTypeWithTypeDecoratedWithEncryptAttribute()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<IDictionary<string, Foo>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, Foo> { { "foo", new Foo { Bar = "abc", Baz = true } } });

            var expected =
                @"{""foo"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Bar"":""abc"",""Baz"":true}")
                    + @""""
                + "}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeIDictionaryOfStringToCustomTypeWithPropertyDecoratedWithEncryptAttribute()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<IDictionary<string, Qux>>(configuration);

            var json = serializer.Serialize(new Dictionary<string, Qux> { { "foo", new Qux { Bar = "abc", Baz = true } } });

            var expected =
                @"{""foo"":{""Bar"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"""abc""")
                    + @""""
                + @",""Baz"":true}}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeNonGenericIDictionary()
        {
            var serializer = new JsonSerializer<Hashtable>();

            var json = serializer.Serialize(new Hashtable { { "foo", new Qux { Bar = "abc", Baz = true } } });

            var expected = @"{""foo"":{""Bar"":""abc"",""Baz"":true}}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserializeDictionaryOfStringToType()
        {
            var serializer = new JsonSerializer<Dictionary<string, int>>();

            var json = @"{""foo"":1,""bar"":2}";

            var dictionary = serializer.Deserialize(json);

            Assert.That(dictionary["foo"], Is.EqualTo(1));
            Assert.That(dictionary["bar"], Is.EqualTo(2));
        }

        [Test]
        public void CanDeserializeIDictionaryOfStringToType()
        {
            var serializer = new JsonSerializer<IDictionary<string, int>>();

            var json = @"{""foo"":1,""bar"":2}";

            var dictionary = serializer.Deserialize(json);

            Assert.That(dictionary["foo"], Is.EqualTo(1));
            Assert.That(dictionary["bar"], Is.EqualTo(2));
        }

        [Test]
        public void CanDeserializeDictionaryOfStringToObject()
        {
            var serializer = new JsonSerializer<Dictionary<string, object>>();

            var json = @"{""foo"":1,""bar"":{""baz"":true}}";

            var dictionary = serializer.Deserialize(json);

            Assert.That(dictionary["foo"], Is.EqualTo(1));
            Assert.That(dictionary["bar"], Is.EqualTo(new JsonObject { { "baz", true } }));
        }

        [Test]
        public void CanDeserializeIDictionaryOfStringToObject()
        {
            var serializer = new JsonSerializer<IDictionary<string, object>>();

            var json = @"{""foo"":1,""bar"":{""baz"":true}}";

            var dictionary = serializer.Deserialize(json);

            Assert.That(dictionary["foo"], Is.EqualTo(1));
            Assert.That(dictionary["bar"], Is.EqualTo(new JsonObject { { "baz", true } }));
        }

        [Test]
        public void CanDeserializeDictionaryOfCustomTypeToType()
        {
            var serializer = new JsonSerializer<Dictionary<Grault, int>>();

            var json = @"{""{\""Bar\"":\""abc\"",\""Baz\"":true}"":1"
                     + @",""{\""Bar\"":\""xyz\"",\""Baz\"":false}"":2}";

            var dictionary = serializer.Deserialize(json);

            Assert.That(dictionary[new Grault { Bar = "abc", Baz = true }], Is.EqualTo(1));
            Assert.That(dictionary[new Grault { Bar = "xyz", Baz = false }], Is.EqualTo(2));
        }

        [Test]
        public void CanDeserializeIDictionaryOfCustomTypeToType()
        {
            var serializer = new JsonSerializer<IDictionary<Grault, int>>();

            var json = @"{""{\""Bar\"":\""abc\"",\""Baz\"":true}"":1"
                     + @",""{\""Bar\"":\""xyz\"",\""Baz\"":false}"":2}";

            var dictionary = serializer.Deserialize(json);

            Assert.That(dictionary[new Grault { Bar = "abc", Baz = true }], Is.EqualTo(1));
            Assert.That(dictionary[new Grault { Bar = "xyz", Baz = false }], Is.EqualTo(2));
        }

        [Test]
        public void CanDeserializeHashTable()
        {
            var serializer = new JsonSerializer<Hashtable>();

            var json = @"{""foo"":1,""bar"":{""baz"":true},""{\""qux\"":false}"":123.45}";

            var dictionary = serializer.Deserialize(json);

            Assert.That(dictionary["foo"], Is.EqualTo(1));
            Assert.That(dictionary["bar"], Is.EqualTo(new JsonObject { { "baz", true } }));
            Assert.That(dictionary[new JsonObject { { "qux", false } }], Is.EqualTo(123.45));
        }

        [Test]
        public void CanDeserializeNonGenericIDictionary()
        {
            var serializer = new JsonSerializer<IDictionary>();

            var json = @"{""foo"":1,""bar"":{""baz"":true},""{\""qux\"":false}"":123.45}";

            var dictionary = serializer.Deserialize(json);

            Assert.That(dictionary["foo"], Is.EqualTo(1));
            Assert.That(dictionary["bar"], Is.EqualTo(new JsonObject { { "baz", true } }));
            Assert.That(dictionary[new JsonObject { { "qux", false } }], Is.EqualTo(123.45));
        }

        [Test]
        public void CanDeserializeReadonlyGenericDictionaryProperty()
        {
            var serializer = new JsonSerializer<GarplyGenericDictionary>();

            var json = @"{""GraultMap"":{""Bar"":456,""Foo"":123}}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.GraultMap["Foo"], Is.EqualTo(123));
            Assert.That(garply.GraultMap["Bar"], Is.EqualTo(456));
        }

        [Test]
        public void CanDeserializeReadonlyNonGenericIDictionaryProperty()
        {
            var serializer = new JsonSerializer<GarplyNonGenericIDictionary>();

            var json = @"{""GraultMap"":{""Bar"":456,""Foo"":123}}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.GraultMap["Foo"], Is.EqualTo(123));
            Assert.That(garply.GraultMap["Bar"], Is.EqualTo(456));
        }

        [Test]
        public void CanDeserializeReadonlyNonGenericImplementationOfNonGenericIDictionaryProperty()
        {
            var serializer = new JsonSerializer<GarplyNonGenericImplementationOfNonGenericIDictionary>();

            var json = @"{""GraultMap"":{""Bar"":456,""Foo"":123}}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.GraultMap["Foo"], Is.EqualTo(123));
            Assert.That(garply.GraultMap["Bar"], Is.EqualTo(456));
        }

        [Test]
        public void CanDeserializeReadonlyGenericIDictionaryProperty()
        {
            var serializer = new JsonSerializer<GarplyGenericIDictionary>();

            var json = @"{""GraultMap"":{""Bar"":456,""Foo"":123}}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.GraultMap["Foo"], Is.EqualTo(123));
            Assert.That(garply.GraultMap["Bar"], Is.EqualTo(456));
        }

        [Test]
        public void CanDeserializeReadonlyGenericImplementationOfGenericIDictionaryProperty()
        {
            var serializer = new JsonSerializer<GarplyGenericImplementationOfGenericIDictionary>();

            var json = @"{""GraultMap"":{""Bar"":456,""Foo"":123}}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.GraultMap["Foo"], Is.EqualTo(123));
            Assert.That(garply.GraultMap["Bar"], Is.EqualTo(456));
        }

        [Encrypt]
        public class Foo
        {
            public string Bar { get; set; }
            public bool Baz { get; set; }
        }

        public class Qux
        {
            [Encrypt]
            public string Bar { get; set; }
            public bool Baz { get; set; }
        }

        public class Corge
        {
            public string Bar { get; set; }
            public bool Baz { get; set; }
        }

        public class Grault
        {
            public string Bar { get; set; }

            public bool Baz { get; set; }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Bar != null ? Bar.GetHashCode() : 0) * 397) ^ Baz.GetHashCode();
                }
            }

            public override bool Equals(object obj)
            {
                var other = obj as Grault;

                if (other == null)
                {
                    return false;
                }

                return Bar == other.Bar && Baz == other.Baz;
            }
        }

        public class GarplyNonGenericIDictionary
        {
            private readonly IDictionary _graults = new Hashtable();

            public IDictionary GraultMap { get { return _graults; } }
        }

        public class GarplyNonGenericImplementationOfNonGenericIDictionary
        {
            private readonly Hashtable _graults = new Hashtable();

            public Hashtable GraultMap { get { return _graults; } }
        }

        public class GarplyGenericDictionary
        {
            private readonly Dictionary<string, int> _graults = new Dictionary<string, int>();

            public Dictionary<string, int> GraultMap { get { return _graults; } }
        }

        public class GarplyGenericIDictionary
        {
            private readonly IDictionary<string, int> _graults = new Dictionary<string, int>();

            public IDictionary<string, int> GraultMap { get { return _graults; } }
        }

        public class GarplyGenericImplementationOfGenericIDictionary
        {
            private readonly CustomIDictionary<string, int> _graults = new CustomIDictionary<string, int>();

            public CustomIDictionary<string, int> GraultMap { get { return _graults; } }
        }

        public class CustomIDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            private readonly IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_dictionary).GetEnumerator();
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                _dictionary.Add(item);
            }

            public void Clear()
            {
                _dictionary.Clear();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                return _dictionary.Contains(item);
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                _dictionary.CopyTo(array, arrayIndex);
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                return _dictionary.Remove(item);
            }

            public int Count
            {
                get { return _dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return _dictionary.IsReadOnly; }
            }

            public bool ContainsKey(TKey key)
            {
                return _dictionary.ContainsKey(key);
            }

            public void Add(TKey key, TValue value)
            {
                _dictionary.Add(key, value);
            }

            public bool Remove(TKey key)
            {
                return _dictionary.Remove(key);
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return _dictionary.TryGetValue(key, out value);
            }

            public TValue this[TKey key]
            {
                get { return _dictionary[key]; }
                set { _dictionary[key] = value; }
            }

            public ICollection<TKey> Keys
            {
                get { return _dictionary.Keys; }
            }

            public ICollection<TValue> Values
            {
                get { return _dictionary.Values; }
            }
        }
    }
}