using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using XSerializer.Encryption;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class ListJsonSerializerTests
    {
        [Test]
        public void CanSerializeGenericIEnumerable()
        {
            var serializer = new JsonSerializer<List<string>>();

            var json = serializer.Serialize(new List<string> { "abc", "xyz" });

            Assert.That(json, Is.EqualTo(@"[""abc"",""xyz""]"));
        }

        [Test]
        public void CanSerializeGenericIEnumerableEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<List<string>>(configuration);

            var json = serializer.Serialize(new List<string> { "abc", "xyz" });

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"[""abc"",""xyz""]")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeNonGenericIEnumerable()
        {
            var serializer = new JsonSerializer<ArrayList>();

            var json = serializer.Serialize(new ArrayList { "abc", "xyz" });

            Assert.That(json, Is.EqualTo(@"[""abc"",""xyz""]"));
        }

        [Test]
        public void CanSerializeSerializeNonGenericIEnumerableEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<ArrayList>(configuration);

            var json = serializer.Serialize(new ArrayList { "abc", "xyz" });

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"[""abc"",""xyz""]")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeGenericIEnumerableOfObjectWhenAnItemTypeIsDecoratedWithEncrypteAttribute()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<List<object>>(configuration);

            var json = serializer.Serialize(new List<object> { new Foo { Bar = "abc", Baz = true }, new Foo { Bar = "xyz", Baz = false } });

            var expected =
                "["
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Bar"":""abc"",""Baz"":true}")
                    + @""""
                + ","
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Bar"":""xyz"",""Baz"":false}")
                    + @""""
                + "]";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeGenericIEnumerableOfObjectWhenAnItemTypePropertyIsDecoratedWithEncrypteAttribute()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<List<object>>(configuration);

            var json = serializer.Serialize(new List<object> { new Qux { Bar = "abc", Baz = true }, new Qux { Bar = "xyz", Baz = false } });

            var expected =
                @"[{""Bar"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"""abc""")
                    + @""""
                + @",""Baz"":true},{""Bar"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"""xyz""")
                    + @""""
                + @",""Baz"":false}]";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserializeGenericList()
        {
            var serializer = new JsonSerializer<List<string>>();

            var result = serializer.Deserialize(@"[""abc"",""xyz""]");

            Assert.That(result, Is.InstanceOf<List<string>>());
            Assert.That(result, Is.EqualTo(new List<string> { "abc", "xyz" }));
        }

        [Test]
        public void CanDeserializeGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable<string>>();

            var result = serializer.Deserialize(@"[""abc"",""xyz""]");

            Assert.That(result, Is.InstanceOf<List<string>>());
            Assert.That(result, Is.EqualTo(new List<string> { "abc", "xyz" }));
        }

        [Test]
        public void CanDeserializeNonGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable>();

            var result = serializer.Deserialize(@"[""abc"",""xyz""]");

            Assert.That(result, Is.InstanceOf<List<object>>());
            Assert.That(result, Is.EqualTo(new List<object> { "abc", "xyz" }));
        }

        [Test]
        public void CanDeserializeArrayList()
        {
            var serializer = new JsonSerializer<ArrayList>();

            var result = serializer.Deserialize(@"[""abc"",""xyz""]");

            Assert.That(result, Is.InstanceOf<ArrayList>());
            Assert.That(result, Is.EqualTo(new ArrayList { "abc", "xyz" }));
        }

        [Test]
        public void CanDeserializeCustomGenericList()
        {
            var serializer = new JsonSerializer<CustomList<string>>();

            var result = serializer.Deserialize(@"[""abc"",""xyz""]");

            Assert.That(result, Is.InstanceOf<CustomList<string>>());
            Assert.That(result, Is.EqualTo(new CustomList<string> { "abc", "xyz" }));
        }

        public class CustomList<T> : List<T>
        {
        }

        [Test]
        public void CanDeserializeTypedCustomGenericList()
        {
            var serializer = new JsonSerializer<CustomStringList>();

            var result = serializer.Deserialize(@"[""abc"",""xyz""]");

            Assert.That(result, Is.InstanceOf<CustomStringList>());
            Assert.That(result, Is.EqualTo(new CustomStringList { "abc", "xyz" }));
        }

        public class CustomStringList : List<string>
        {
        }

        [Test]
        public void CanDeserializeCustomNonGenericList()
        {
            var serializer = new JsonSerializer<CustomArrayList>();

            var result = serializer.Deserialize(@"[""abc"",""xyz""]");

            Assert.That(result, Is.InstanceOf<CustomArrayList>());
            Assert.That(result, Is.EqualTo(new CustomArrayList { "abc", "xyz" }));
        }

        public class CustomArrayList : ArrayList
        {
        }

        [Test]
        public void CanDeserializeEmptyGenericList()
        {
            var serializer = new JsonSerializer<List<string>>();

            var result = serializer.Deserialize(@"[]");

            Assert.That(result, Is.InstanceOf<List<string>>());
            Assert.That(result, Is.EqualTo(new List<string>()));
        }

        [Test]
        public void CanDeserializeEmptyGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable<string>>();

            var result = serializer.Deserialize(@"[]");

            Assert.That(result, Is.InstanceOf<List<string>>());
            Assert.That(result, Is.EqualTo(new List<string>()));
        }

        [Test]
        public void CanDeserializeEmptyNonGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable>();

            var result = serializer.Deserialize(@"[]");

            Assert.That(result, Is.InstanceOf<List<object>>());
            Assert.That(result, Is.EqualTo(new List<object>()));
        }

        [Test]
        public void CanDeserializeNullGenericList()
        {
            var serializer = new JsonSerializer<List<string>>();

            var result = serializer.Deserialize(@"null");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void CanDeserializeNullGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable<string>>();

            var result = serializer.Deserialize(@"null");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void CanDeserializeNullNonGenericIEnumerable()
        {
            var serializer = new JsonSerializer<IEnumerable>();

            var result = serializer.Deserialize(@"null");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void CanDeserializeReadonlyNonGenericIListProperty()
        {
            var serializer = new JsonSerializer<GarplyNonGenericIList>();

            var json = @"{""Graults"":[true,false,null]}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.Graults[0], Is.True);
            Assert.That(garply.Graults[1], Is.False);
            Assert.That(garply.Graults[2], Is.Null);
        }

        [Test]
        public void CanDeserializeReadonlyNonGenericImplementationOfNonGenericIListProperty()
        {
            var serializer = new JsonSerializer<GarplyNonGenericImplementationOfNonGenericIList>();

            var json = @"{""Graults"":[true,false,null]}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.Graults[0], Is.True);
            Assert.That(garply.Graults[1], Is.False);
            Assert.That(garply.Graults[2], Is.Null);
        }

        [Test]
        public void CanDeserializeReadonlyGenericListProperty()
        {
            var serializer = new JsonSerializer<GarplyGenericList>();

            var json = @"{""Graults"":[1,2,3]}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.Graults[0], Is.EqualTo(1));
            Assert.That(garply.Graults[1], Is.EqualTo(2));
            Assert.That(garply.Graults[2], Is.EqualTo(3));
        }

        [Test]
        public void CanDeserializeReadonlyNonGenericImplementationOfGenericListProperty()
        {
            var serializer = new JsonSerializer<GarplyNonGenericImplementationOfGenericIList>();

            var json = @"{""Graults"":[1,2,3]}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.Graults[0], Is.EqualTo(1));
            Assert.That(garply.Graults[1], Is.EqualTo(2));
            Assert.That(garply.Graults[2], Is.EqualTo(3));
        }

        [Test]
        public void CanDeserializeReadonlyGenericIListProperty()
        {
            var serializer = new JsonSerializer<GarplyGenericIList>();

            var json = @"{""Graults"":[1,2,3]}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.Graults[0], Is.EqualTo(1));
            Assert.That(garply.Graults[1], Is.EqualTo(2));
            Assert.That(garply.Graults[2], Is.EqualTo(3));
        }

        [Test]
        public void CanDeserializeReadonlyNonGenericInterfaceImplementationOfGenericListProperty()
        {
            var serializer = new JsonSerializer<GarplyNonGenericInterfaceImplementationOfGenericIList>();

            var json = @"{""Graults"":[1,2,3]}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.Graults[0], Is.EqualTo(1));
            Assert.That(garply.Graults[1], Is.EqualTo(2));
            Assert.That(garply.Graults[2], Is.EqualTo(3));
        }

        [Test]
        public void CanDeserializeReadonlyGenericImplementationOfGenericIListProperty()
        {
            var serializer = new JsonSerializer<GarplyGenericImplementationOfGenericIList>();

            var json = @"{""Graults"":[1,2,3]}";

            var garply = serializer.Deserialize(json);

            Assert.That(garply.Graults[0], Is.EqualTo(1));
            Assert.That(garply.Graults[1], Is.EqualTo(2));
            Assert.That(garply.Graults[2], Is.EqualTo(3));
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

        public class GarplyNonGenericIList
        {
            private readonly IList _graults = new ArrayList();

            public IList Graults { get { return _graults; } }
        }

        public class GarplyNonGenericImplementationOfNonGenericIList
        {
            private readonly ArrayList _graults = new ArrayList();

            public ArrayList Graults { get { return _graults; } }
        }

        public class GarplyGenericList
        {
            private readonly List<int> _graults = new List<int>();

            public List<int> Graults { get { return _graults; } }
        }

        public class GarplyNonGenericImplementationOfGenericIList
        {
            private readonly IntList _graults = new IntList();

            public IntList Graults { get { return _graults; } }
        }

        public class GarplyGenericIList
        {
            private readonly IList<int> _graults = new List<int>();

            public IList<int> Graults { get { return _graults; } }
        }

        public class GarplyNonGenericInterfaceImplementationOfGenericIList
        {
            private readonly IIntList _graults = new IntList();

            public IIntList Graults { get { return _graults; } }
        }

        public class GarplyGenericImplementationOfGenericIList
        {
            private readonly CustomIList<int> _graults = new CustomIList<int>();

            public CustomIList<int> Graults { get { return _graults; } }
        }
        
        public class CustomIList<T> : IList<T>
        {
            private readonly IList<T> _list = new List<T>();

            public IEnumerator<T> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_list).GetEnumerator();
            }

            public void Add(T item)
            {
                _list.Add(item);
            }

            public void Clear()
            {
                _list.Clear();
            }

            public bool Contains(T item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public bool Remove(T item)
            {
                return _list.Remove(item);
            }

            public int Count
            {
                get { return _list.Count; }
            }

            public bool IsReadOnly
            {
                get { return _list.IsReadOnly; }
            }

            public int IndexOf(T item)
            {
                return _list.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                _list.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _list.RemoveAt(index);
            }

            public T this[int index]
            {
                get { return _list[index]; }
                set { _list[index] = value; }
            }
        }

        public interface IIntList : IList<int>
        {
        }

        public class IntList : IIntList
        {
            private readonly IList<int> _list = new List<int>();
            public IEnumerator<int> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_list).GetEnumerator();
            }

            public void Add(int item)
            {
                _list.Add(item);
            }

            public void Clear()
            {
                _list.Clear();
            }

            public bool Contains(int item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(int[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public bool Remove(int item)
            {
                return _list.Remove(item);
            }

            public int Count
            {
                get { return _list.Count; }
            }

            public bool IsReadOnly
            {
                get { return _list.IsReadOnly; }
            }

            public int IndexOf(int item)
            {
                return _list.IndexOf(item);
            }

            public void Insert(int index, int item)
            {
                _list.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _list.RemoveAt(index);
            }

            public int this[int index]
            {
                get { return _list[index]; }
                set { _list[index] = value; }
            }
        }
    }
}