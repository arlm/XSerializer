using System;
using System.Text;
using NUnit.Framework;
using XSerializer.Encryption;

namespace XSerializer.Tests.Encryption
{
    public class SerializationStateTests
    {
        public class TheGetMethod
        {
            [Test]
            public void HasAnInitialValueOfNull()
            {
                var state = new SerializationState();

                Assert.That(state.GetRawValue(), Is.Null);
            }

            [Test]
            public void ThrowsAnExceptionIfGetValueIsNull()
            {
                var state = new SerializationState();

                Assert.That(() => state.Get<Foo>(null), Throws.InstanceOf<ArgumentNullException>());
            }

            [Test]
            public void ThrowsAnExceptionIfGetValueReturnsNull()
            {
                var state = new SerializationState();

                Assert.That(() => state.Get<Foo>(() => null), Throws.ArgumentException);
            }

            [Test]
            public void ReturnsTheObjectFromTheGetMethod()
            {
                var state = new SerializationState();

                var foo = state.Get(() => new Foo(1));

                Assert.That(foo.Bar, Is.EqualTo(1));
            }

            [Test]
            public void ReturnsTheSameObjectFromEachCallToTheGetMethod()
            {
                var state = new SerializationState();

                var foo1 = state.Get(() => new Foo(2));
                var foo2 = state.Get(() => new Foo(2));
                var foo3 = state.Get(() => new Foo(2));

                Assert.That(foo1, Is.SameAs(foo2));
                Assert.That(foo2, Is.SameAs(foo3));
            }

            private class Foo
            {
                private readonly int _bar;

                public Foo(int bar)
                {
                    _bar = bar;
                }

                public int Bar
                {
                    get { return _bar; }
                }
            }
        }

        public class WhenUsedInASerializationOperation
        {
            private IEncryptionMechanism _current;
            private MyEncryptionMechanism _myEncryptionMechanism;

            [SetUp]
            public void Setup()
            {
                _current = EncryptionMechanism.Current;
                _myEncryptionMechanism = new MyEncryptionMechanism();
                EncryptionMechanism.Current = _myEncryptionMechanism;
            }

            [TearDown]
            public void TearDown()
            {
                EncryptionMechanism.Current = _current;
            }

            [Test]
            public void TheSameInstanceIsUsedForEachEncryptionOperationWithinASingleSerializationOperation()
            {
                Assert.That(MyEncryptionMechanism.LastSerializationState, Is.Null);

                var foo = new Foo
                {
                    Bar = new Bar
                    {
                        Grault = "abc",
                        Qux = "xyz"
                    },
                    Baz = new Baz
                    {
                        Fred = "a1",
                        Waldo = "b2"
                    },
                    Garply = "Hello, world!"
                };

                var serializer = new XmlSerializer<Foo>(x => x.Indent());

                var xml = serializer.Serialize(foo);

                Assert.That(MyEncryptionMechanism.LastSerializationState, Is.Not.Null);

                Assert.That(MyEncryptionMechanism.LastSerializationState.GetRawValue(), Is.Not.Null);
                Assert.That(MyEncryptionMechanism.LastSerializationState.GetRawValue(), Is.InstanceOf<MyEncryptionMechanism.Counts>());

                var counts = (MyEncryptionMechanism.Counts)MyEncryptionMechanism.LastSerializationState.GetRawValue();

                Assert.That(counts.DecryptInvocationCount, Is.EqualTo(0));
                Assert.That(counts.EncryptInvocationCount, Is.EqualTo(3));

                MyEncryptionMechanism.LastSerializationState = null;

                serializer.Deserialize(xml);

                Assert.That(MyEncryptionMechanism.LastSerializationState, Is.Not.Null);

                Assert.That(MyEncryptionMechanism.LastSerializationState.GetRawValue(), Is.Not.Null);
                Assert.That(MyEncryptionMechanism.LastSerializationState.GetRawValue(), Is.InstanceOf<MyEncryptionMechanism.Counts>());

                counts = (MyEncryptionMechanism.Counts)MyEncryptionMechanism.LastSerializationState.GetRawValue();

                Assert.That(counts.DecryptInvocationCount, Is.EqualTo(3));
                Assert.That(counts.EncryptInvocationCount, Is.EqualTo(0));

                MyEncryptionMechanism.LastSerializationState = null;
            }

            [Test]
            public void ADifferentInstanceIsUsedForEachSerializationOperation()
            {
                Assert.That(MyEncryptionMechanism.LastSerializationState, Is.Null);

                var foo = new Foo
                {
                    Bar = new Bar
                    {
                        Grault = "abc",
                        Qux = "xyz"
                    },
                    Baz = new Baz
                    {
                        Fred = "a1",
                        Waldo = "b2"
                    },
                    Garply = "Hello, world!"
                };

                var serializer = new XmlSerializer<Foo>(x => x.Indent());

                var xml = serializer.Serialize(foo);

                var serializationState1 = MyEncryptionMechanism.LastSerializationState;

                foo = serializer.Deserialize(xml);

                var serializationState2 = MyEncryptionMechanism.LastSerializationState;

                xml = serializer.Serialize(foo);

                var serializationState3 = MyEncryptionMechanism.LastSerializationState;

                serializer.Deserialize(xml);

                var serializationState4 = MyEncryptionMechanism.LastSerializationState;

                Assert.That(serializationState1, Is.Not.SameAs(serializationState2));
                Assert.That(serializationState1, Is.Not.SameAs(serializationState3));
                Assert.That(serializationState1, Is.Not.SameAs(serializationState4));
                Assert.That(serializationState2, Is.Not.SameAs(serializationState3));
                Assert.That(serializationState2, Is.Not.SameAs(serializationState4));
                Assert.That(serializationState3, Is.Not.SameAs(serializationState4));

                MyEncryptionMechanism.LastSerializationState = null;
            }

            private class MyEncryptionMechanism : IEncryptionMechanism
            {
                public static SerializationState LastSerializationState;

                public string Encrypt(string plainText, object encryptKey, SerializationState serializationState)
                {
                    LastSerializationState = serializationState;

                    var counts = serializationState.Get(() => new Counts());

                    counts.EncryptInvocationCount++;

                    return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
                }

                public string Decrypt(string cipherText, object encryptKey, SerializationState serializationState)
                {
                    LastSerializationState = serializationState;

                    var counts = serializationState.Get(() => new Counts());

                    counts.DecryptInvocationCount++;

                    return Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));
                }

                public class Counts
                {
                    public int EncryptInvocationCount;
                    public int DecryptInvocationCount;
                }
            }

            private class Foo
            {
                public Bar Bar { get; set; }

                public Baz Baz { get; set; }

                [Encrypt]
                public string Garply { get; set; }
            }

            private class Bar
            {
                public string Qux { get; set; }

                [Encrypt]
                public string Grault { get; set; }
            }

            [Encrypt]
            private class Baz
            {
                public string Fred { get; set; }
                public string Waldo { get; set; }
            }
        }
    }
}
