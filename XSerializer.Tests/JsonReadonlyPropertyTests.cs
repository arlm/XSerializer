using System;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class JsonReadonlyPropertyTests
    {
        [TestCase(typeof(FooNoAttribute<string>))]
        [TestCase(typeof(FooNoAttribute<DateTime>))]
        [TestCase(typeof(FooNoAttribute<DateTime?>))]
        [TestCase(typeof(FooNoAttribute<DateTimeOffset>))]
        [TestCase(typeof(FooNoAttribute<DateTimeOffset?>))]
        [TestCase(typeof(FooNoAttribute<TimeSpan>))]
        [TestCase(typeof(FooNoAttribute<TimeSpan?>))]
        [TestCase(typeof(FooNoAttribute<Guid>))]
        [TestCase(typeof(FooNoAttribute<Guid?>))]
        [TestCase(typeof(FooNoAttribute<Baz>))]
        [TestCase(typeof(FooNoAttribute<Baz?>))]
        [TestCase(typeof(FooNoAttribute<Type>))]
        [TestCase(typeof(FooNoAttribute<Uri>))]
        [TestCase(typeof(FooNoAttribute<double>))]
        [TestCase(typeof(FooNoAttribute<double?>))]
        [TestCase(typeof(FooNoAttribute<int>))]
        [TestCase(typeof(FooNoAttribute<int?>))]
        [TestCase(typeof(FooNoAttribute<float>))]
        [TestCase(typeof(FooNoAttribute<float?>))]
        [TestCase(typeof(FooNoAttribute<long>))]
        [TestCase(typeof(FooNoAttribute<long?>))]
        [TestCase(typeof(FooNoAttribute<decimal>))]
        [TestCase(typeof(FooNoAttribute<decimal?>))]
        [TestCase(typeof(FooNoAttribute<byte>))]
        [TestCase(typeof(FooNoAttribute<byte?>))]
        [TestCase(typeof(FooNoAttribute<sbyte>))]
        [TestCase(typeof(FooNoAttribute<sbyte?>))]
        [TestCase(typeof(FooNoAttribute<short>))]
        [TestCase(typeof(FooNoAttribute<short?>))]
        [TestCase(typeof(FooNoAttribute<ushort>))]
        [TestCase(typeof(FooNoAttribute<ushort?>))]
        [TestCase(typeof(FooNoAttribute<uint>))]
        [TestCase(typeof(FooNoAttribute<uint?>))]
        [TestCase(typeof(FooNoAttribute<ulong>))]
        [TestCase(typeof(FooNoAttribute<ulong?>))]
        [TestCase(typeof(FooNoAttribute<bool>))]
        [TestCase(typeof(FooNoAttribute<bool?>))]
        public void DoesNotSerializePrimitivePropertyWhenNotDecoratedWithJsonPropertyAttribute(Type type)
        {
            var serializer = JsonSerializer.Create(type);

            var foo = Activator.CreateInstance(type);

            var json = serializer.Serialize(foo);

            Assert.That(json, Is.Not.StringContaining("\"Bar\":"));

            Assert.That(() => serializer.Deserialize(json), Throws.Nothing);
        }

        [TestCase(typeof(FooWithAttribute<string>))]
        [TestCase(typeof(FooWithAttribute<DateTime>))]
        [TestCase(typeof(FooWithAttribute<DateTime?>))]
        [TestCase(typeof(FooWithAttribute<DateTimeOffset>))]
        [TestCase(typeof(FooWithAttribute<DateTimeOffset?>))]
        [TestCase(typeof(FooWithAttribute<TimeSpan>))]
        [TestCase(typeof(FooWithAttribute<TimeSpan?>))]
        [TestCase(typeof(FooWithAttribute<Guid>))]
        [TestCase(typeof(FooWithAttribute<Guid?>))]
        [TestCase(typeof(FooWithAttribute<Baz>))]
        [TestCase(typeof(FooWithAttribute<Baz?>))]
        [TestCase(typeof(FooWithAttribute<Type>))]
        [TestCase(typeof(FooWithAttribute<Uri>))]
        [TestCase(typeof(FooWithAttribute<double>))]
        [TestCase(typeof(FooWithAttribute<double?>))]
        [TestCase(typeof(FooWithAttribute<int>))]
        [TestCase(typeof(FooWithAttribute<int?>))]
        [TestCase(typeof(FooWithAttribute<float>))]
        [TestCase(typeof(FooWithAttribute<float?>))]
        [TestCase(typeof(FooWithAttribute<long>))]
        [TestCase(typeof(FooWithAttribute<long?>))]
        [TestCase(typeof(FooWithAttribute<decimal>))]
        [TestCase(typeof(FooWithAttribute<decimal?>))]
        [TestCase(typeof(FooWithAttribute<byte>))]
        [TestCase(typeof(FooWithAttribute<byte?>))]
        [TestCase(typeof(FooWithAttribute<sbyte>))]
        [TestCase(typeof(FooWithAttribute<sbyte?>))]
        [TestCase(typeof(FooWithAttribute<short>))]
        [TestCase(typeof(FooWithAttribute<short?>))]
        [TestCase(typeof(FooWithAttribute<ushort>))]
        [TestCase(typeof(FooWithAttribute<ushort?>))]
        [TestCase(typeof(FooWithAttribute<uint>))]
        [TestCase(typeof(FooWithAttribute<uint?>))]
        [TestCase(typeof(FooWithAttribute<ulong>))]
        [TestCase(typeof(FooWithAttribute<ulong?>))]
        [TestCase(typeof(FooWithAttribute<bool>))]
        [TestCase(typeof(FooWithAttribute<bool?>))]
        public void SerializesPrimitivePropertyWhenDecoratedWithJsonPropertyAttribute(Type type)
        {
            var serializer = JsonSerializer.Create(type);

            var foo = Activator.CreateInstance(type);

            var json = serializer.Serialize(foo);

            Assert.That(json, Is.StringContaining("\"Bar\":"));

            Assert.That(() => serializer.Deserialize(json), Throws.Nothing);
        }

        [Test]
        public void ReadonlyPropertyInBaseClassIsOptInSerialized()
        {
            var serializer = new JsonSerializer<Foo>();

            var foo = new Foo();

            var json = serializer.Serialize(foo);

            Assert.That(json, Is.StringContaining("\"Bar\":"));

            Assert.That(() => serializer.Deserialize(json), Throws.Nothing);
        }

        public class FooNoAttribute<T>
        {
            public T Bar { get { return default(T); } }
        }

        public class FooWithAttribute<T>
        {
            [JsonProperty]
            public T Bar { get { return default(T); } }
        }

        public enum Baz
        {
            Waldo, Fred
        }

        public abstract class FooBase
        {
            private readonly string _bar;

            protected FooBase(string bar)
            {
                _bar = bar;
            }

            [JsonProperty]
            public string Bar { get { return _bar; } }
        }

        public class Foo : FooBase
        {
            public Foo()
                : base("baz")
            {
            }
        }
    }
}