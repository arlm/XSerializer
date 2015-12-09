using System;
using System.Xml.Serialization;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class DeserializeEmptyStringIntoValueTypeTests
    {
        [Test]
        public void Abc()
        {
            var xml = @"
<Foo>
  <Bar></Bar>
  <Baz></Baz>
  <Qux></Qux>
  <Corge></Corge>
  <Grault></Grault>
  <Garply></Garply>
</Foo>";

            var serializer = new XmlSerializer<Foo1>();

            var foo = serializer.Deserialize(xml);

            Assert.That(foo.Bar, Is.EqualTo(new Waldo()));
            Assert.That(foo.Baz, Is.Null);
            Assert.That(foo.Qux, Is.EqualTo(new DateTime()));
            Assert.That(foo.Corge, Is.Null);
            Assert.That(foo.Grault, Is.EqualTo(new DateTimeOffset()));
            Assert.That(foo.Garply, Is.Null);
        }

        [Test]
        public void Def()
        {
            var xml = @"
<Foo>
  <Bar></Bar>
  <Baz></Baz>
  <Qux></Qux>
  <Corge></Corge>
  <Grault></Grault>
  <Garply></Garply>
</Foo>";

            var serializer = new XmlSerializer<Foo2>();

            var foo = serializer.Deserialize(xml);

            Assert.That(foo.Bar, Is.EqualTo(new TimeSpan()));
            Assert.That(foo.Baz, Is.Null);
            Assert.That(foo.Qux, Is.EqualTo(new Guid()));
            Assert.That(foo.Corge, Is.Null);
            Assert.That(foo.Grault, Is.EqualTo(new int()));
            Assert.That(foo.Garply, Is.Null);
        }

        [Test]
        public void Ghi()
        {
            var xml = @"
<Foo>
  <Bar></Bar>
  <Baz></Baz>
  <Qux></Qux>
</Foo>";

            var serializer = new XmlSerializer<Foo3>();

            var foo = serializer.Deserialize(xml);

            Assert.That(foo.Bar, Is.Null);
            Assert.That(foo.Baz, Is.Null);
            Assert.That(foo.Qux, Is.Null);
        }

        [Test]
        public void Jkl()
        {
            var xml = @"
<Foo>
  <Bar></Bar>
  <Baz></Baz>
  <Qux></Qux>
  <Corge></Corge>
  <Grault></Grault>
  <Garply></Garply>
</Foo>";

            var serializer = new XmlSerializer<Foo4>();

            var foo = serializer.Deserialize(xml);

            Assert.That(foo.Bar, Is.EqualTo(new Waldo()));
            Assert.That(foo.Baz, Is.Null);
            Assert.That(foo.Qux, Is.EqualTo(new DateTime()));
            Assert.That(foo.Corge, Is.Null);
            Assert.That(foo.Grault, Is.EqualTo(new DateTimeOffset()));
            Assert.That(foo.Garply, Is.Null);
        }

        [Test]
        public void Mno()
        {
            var xml = @"
<Foo>
  <Bar></Bar>
  <Baz></Baz>
  <Qux></Qux>
  <Corge></Corge>
  <Grault></Grault>
  <Garply></Garply>
</Foo>";

            var serializer = new XmlSerializer<Foo5>();

            var foo = serializer.Deserialize(xml);

            Assert.That(foo.Bar, Is.EqualTo(new TimeSpan()));
            Assert.That(foo.Baz, Is.Null);
            Assert.That(foo.Qux, Is.EqualTo(new Guid()));
            Assert.That(foo.Corge, Is.Null);
            Assert.That(foo.Grault, Is.EqualTo(new int()));
            Assert.That(foo.Garply, Is.Null);
        }

        [Test]
        public void Pqr()
        {
            var xml = @"
<Foo>
  <Bar></Bar>
  <Baz></Baz>
  <Qux></Qux>
  <Corge></Corge>
  <Grault></Grault>
</Foo>";

            var serializer = new XmlSerializer<Foo6>();

            var foo = serializer.Deserialize(xml);

            Assert.That(foo.Bar, Is.Null);
            Assert.That(foo.Baz, Is.Null);
            Assert.That(foo.Qux, Is.Null);
            Assert.That(foo.Corge, Is.EqualTo(new bool()));
            Assert.That(foo.Grault, Is.Null);
        }

        [XmlRoot("Foo")]
        public class Foo1
        {
            public Waldo Bar { get; set; }
            public Waldo? Baz { get; set; }
            public DateTime Qux { get; set; }
            public DateTime? Corge { get; set; }
            public DateTimeOffset Grault { get; set; }
            public DateTimeOffset? Garply { get; set; }
        }

        [XmlRoot("Foo")]
        public class Foo2
        {
            public TimeSpan Bar { get; set; }
            public TimeSpan? Baz { get; set; }
            public Guid Qux { get; set; }
            public Guid? Corge { get; set; }
            public int Grault { get; set; }
            public int? Garply { get; set; }
        }

        [XmlRoot("Foo")]
        public class Foo3
        {
            public Type Bar { get; set; }
            public Uri Baz { get; set; }
            public Enum Qux { get; set; }
        }

        [XmlRoot("Foo")]
        public class Foo4
        {
            [Redact] public Waldo Bar { get; set; }
            [Redact] public Waldo? Baz { get; set; }
            [Redact] public DateTime Qux { get; set; }
            [Redact] public DateTime? Corge { get; set; }
            [Redact] public DateTimeOffset Grault { get; set; }
            [Redact] public DateTimeOffset? Garply { get; set; }
        }

        [XmlRoot("Foo")]
        public class Foo5
        {
            [Redact] public TimeSpan Bar { get; set; }
            [Redact] public TimeSpan? Baz { get; set; }
            [Redact] public Guid Qux { get; set; }
            [Redact] public Guid? Corge { get; set; }
            [Redact] public int Grault { get; set; }
            [Redact] public int? Garply { get; set; }
        }

        [XmlRoot("Foo")]
        public class Foo6
        {
            [Redact] public Type Bar { get; set; }
            [Redact] public Uri Baz { get; set; }
            [Redact] public Enum Qux { get; set; }
            [Redact] public bool Corge { get; set; }
            [Redact] public bool? Grault { get; set; }
        }

        public enum Waldo
        {
            Ham,
            Eggs
        }
    }

}
