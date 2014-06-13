using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class NonDefaultConstructorTests
    {
        [Test]
        public void CanRoundTripClassWithOnlyOneNonDefaultConstructor()
        {
            var foo = new Foo(Tuple.Create(1))
            {
                OtherThings = new List<Tuple<int>>
                {
                    Tuple.Create(2),
                    Tuple.Create(3)
                }
            };

            var serialzier = new XmlSerializer<Foo>(o => o.Indent());

            var xml = serialzier.Serialize(foo);

            var roundTrip = serialzier.Deserialize(xml);

            Assert.That(roundTrip.Thing.Item1, Is.EqualTo(foo.Thing.Item1));
            Assert.That(roundTrip.OtherThings.Count, Is.EqualTo(foo.OtherThings.Count));
            Assert.That(roundTrip.OtherThings[0].Item1, Is.EqualTo(foo.OtherThings[0].Item1));
            Assert.That(roundTrip.OtherThings[1].Item1, Is.EqualTo(foo.OtherThings[1].Item1));
        }

        [Test]
        public void CanRoundTripClassWithDefaultAndNonDefaultConstructorUsingTheNonDefault()
        {
            var bar = new Bar("abc") { Qux = new Dictionary<string, bool> { { "a", true } } };

            var serializer = new XmlSerializer<Bar>(o => o.Indent());

            var xml = serializer.Serialize(bar);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Baz, Is.EqualTo(bar.Baz));
        }

        [Test]
        public void CanRoundTripClassWithDefaultAndNonDefaultConstructorUsingTheDefault()
        {
            var bar = new Bar { Qux = new Dictionary<string, bool> { { "a", true } } };

            var serializer = new XmlSerializer<Bar>(o => o.Indent());

            var xml = serializer.Serialize(bar);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Baz, Is.EqualTo(bar.Baz));
        }

        public class Foo
        {
            public Foo(Tuple<int> thing)
            {
                Thing = thing;
            }

            public Tuple<int> Thing { get; private set; }
            public List<Tuple<int>> OtherThings { get; set; }
        }

        public class Bar
        {
            private readonly string _baz;

            public Bar()
            {
            }

            public Bar(string baz)
            {
                _baz = baz;
            }

            public string Baz { get { return _baz; } }
            public Dictionary<string, bool> Qux { get; set; } 
        }
    }
}