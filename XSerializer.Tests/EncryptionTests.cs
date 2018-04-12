using System;
using System.Linq.Expressions;
using NUnit.Framework;
using XSerializer.Encryption;

namespace XSerializer.Tests
{
    public class EncryptionTests
    {
        [Test]
        public void Foooooo()
        {
            var foo = new Foo
            {
                Bar = "abc",
                Baz = 123,
                Qux = 123.45,
                Enum = ExpressionType.LeftShiftAssign,
                Type = typeof(BinaryExpression),
                Uri = new Uri("https://www.google.com/search?q=weird+wild+stuff")
            };

            var serializer = new XmlSerializer<Foo>(x => x.Indent(), typeof(ExpressionType));

            var xml = serializer.Serialize(foo);

            var roundTripFoo = serializer.Deserialize(xml);
        }

        public class Foo
        {
            [Encrypt]
            public string Bar { get; set; }
            [Encrypt]
            public int Baz { get; set; }
            [Encrypt]
            public double? Qux { get; set; }
            [Encrypt]
            public Enum Enum { get; set; }
            [Encrypt]
            public Type Type { get; set; }
            [Encrypt]
            public Uri Uri { get; set; }
        }
    }
}