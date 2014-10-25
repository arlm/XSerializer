using System;
using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public void CanRoundTripClassWithReadOnlyPropertyOfTypeIEnumerableOfCustomType()
        {
            var qux = new Qux(new[] { new Corge { Value = "abc" }, new Corge { Value = "xyz" } });

            var serializer = new XmlSerializer<Qux>(o => o.Indent());

            var xml = serializer.Serialize(qux);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Corges.Count(), Is.EqualTo(qux.Corges.Count()));
            Assert.That(roundTrip.Corges.First().Value, Is.EqualTo(qux.Corges.First().Value));
            Assert.That(roundTrip.Corges.Last().Value, Is.EqualTo(qux.Corges.Last().Value));
        }

        [Test]
        public void CanRoundTripClassWithReadOnlyOfTypeArray()
        {
            var grault = new Grault(new[] { 1, 2, 3 });

            var serializer = new XmlSerializer<Grault>(o => o.Indent());

            var xml = serializer.Serialize(grault);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip.Garplies, Is.EquivalentTo(grault.Garplies));
        }

        [Test]
        public void VerifyThatNotProvidingAnXmlElementForANullableReadonlyPropertyResultsInNull()
        {
            var serializer = new XmlSerializer<Baz>(x => x.Indent());

            var xml = @"<Baz/>";

            var baz = serializer.Deserialize(xml);

            Assert.That(baz.Bar, Is.Null); // Should not be 0.
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

        public class Baz
        {
            private readonly int? _bar;

            public Baz(int? bar)
            {
                _bar = bar;
            }

            public int? Bar
            {
                get { return _bar; }
            }
        }

        public class Qux
        {
            private readonly IEnumerable<Corge> _corges;

            public Qux(IEnumerable<Corge> corges)
            {
                _corges = corges;
            }

            public IEnumerable<Corge> Corges
            {
                get { return _corges; }
            }
        }

        public class Corge
        {
            public string Value { get; set; }
        }

        public class Grault
        {
            private readonly int[] _garplies;

            public Grault(int[] garplies)
            {
                _garplies = garplies;
            }

            public int[] Garplies
            {
                get { return _garplies; }
            }
        }
    }
}