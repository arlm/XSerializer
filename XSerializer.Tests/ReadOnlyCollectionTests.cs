using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;

namespace XSerializer.Tests
{
    public class ReadOnlyCollectionTests
    {
        [Test]
        public void Smoke()
        {
            var foo = new Foo1
            {
                Bars = GetReadOnlyCollection(),
                Bazes = GetReadOnlyCollection(),
                Quxes = GetReadOnlyDictionary(),
                Corges = GetReadOnlyDictionary(),
                Graults = GetReadOnlyCollection()
            };
            var serializer = new XmlSerializer<Foo1>(x => x.Indent());

            //var foo = new Foo2(
            //    GetReadOnlyCollection(),
            //    GetReadOnlyCollection(),
            //    GetReadOnlyDictionary(),
            //    GetReadOnlyDictionary(),
            //    GetReadOnlyCollection());
            //var serializer = new XmlSerializer<Foo2>(x => x.Indent());

            //var foo = new Foo3(
            //    GetReadOnlyCollection(),
            //    GetReadOnlyCollection(),
            //    GetReadOnlyDictionary(),
            //    GetReadOnlyDictionary(),
            //    GetReadOnlyCollection());
            //var serializer = new XmlSerializer<Foo3>(x => x.Indent());

            //var foo = new Foo4(
            //    GetReadOnlyCollection(),
            //    GetReadOnlyCollection(),
            //    GetReadOnlyCollection());
            //var serializer = new XmlSerializer<Foo4>(x => x.Indent());

            var xml = serializer.Serialize(foo);

            var roundTrip = serializer.Deserialize(xml);

            Assert.That(roundTrip, Has.PropertiesEqualTo(foo));
        }

        private ReadOnlyCollection<int> GetReadOnlyCollection()
        {
            return new ReadOnlyCollection<int>(new [] { 1, 2, 3 });
        }

        private ReadOnlyDictionary<int, int> GetReadOnlyDictionary()
        {
            return new ReadOnlyDictionary<int, int>(new Dictionary<int, int>{{1,1}, {2,2}, {3,3}});
        }

        public class Foo1
        {
            public ReadOnlyCollection<int> Bars { get; set; }
            public IReadOnlyCollection<int> Bazes { get; set; }
            public ReadOnlyDictionary<int, int> Quxes { get; set; }
            public IReadOnlyDictionary<int, int> Corges { get; set; }
            public IReadOnlyList<int> Graults { get; set; }
        }

        public class Foo2
        {
            private readonly ReadOnlyCollection<int> _bars;
            private readonly IReadOnlyCollection<int> _bazes;
            private readonly ReadOnlyDictionary<int, int> _quxes;
            private readonly IReadOnlyDictionary<int, int> _corges;
            private readonly IReadOnlyList<int> _graults;

            public Foo2(
                ReadOnlyCollection<int> bars,
                IReadOnlyCollection<int> bazes,
                ReadOnlyDictionary<int, int> quxes,
                IReadOnlyDictionary<int, int> corges,
                IReadOnlyList<int> graults)
            {
                _bars = bars;
                _bazes = bazes;
                _quxes = quxes;
                _corges = corges;
                _graults = graults;
            }

            public ReadOnlyCollection<int> Bars { get { return _bars; } }
            public IReadOnlyCollection<int> Bazes { get { return _bazes; } }
            public ReadOnlyDictionary<int, int> Quxes { get { return _quxes; } }
            public IReadOnlyDictionary<int, int> Corges { get { return _corges; } }
            public IReadOnlyList<int> Graults { get { return _graults; } }
        }

        public class Foo3
        {
            private readonly ReadOnlyCollection<int> _bars;
            private readonly IReadOnlyCollection<int> _bazes;
            private readonly ReadOnlyDictionary<int, int> _quxes;
            private readonly IReadOnlyDictionary<int, int> _corges;
            private readonly IReadOnlyList<int> _graults;

            public Foo3(
                IEnumerable<int> bars,
                IEnumerable<int> bazes,
                IDictionary<int, int> quxes,
                IDictionary<int, int> corges,
                IEnumerable<int> graults)
            {
                _bars = bars.ToList().AsReadOnly();
                _bazes = bazes.ToList().AsReadOnly();
                _quxes = new ReadOnlyDictionary<int, int>(quxes);
                _corges = new ReadOnlyDictionary<int, int>(corges);
                _graults = graults.ToList().AsReadOnly();
            }

            public ReadOnlyCollection<int> Bars { get { return _bars; } }
            public IReadOnlyCollection<int> Bazes { get { return _bazes; } }
            public ReadOnlyDictionary<int, int> Quxes { get { return _quxes; } }
            public IReadOnlyDictionary<int, int> Corges { get { return _corges; } }
            public IReadOnlyList<int> Graults { get { return _graults; } }
        }

        public class Foo4
        {
            private readonly ReadOnlyCollection<int> _bars;
            private readonly IReadOnlyCollection<int> _bazes;
            private readonly IReadOnlyList<int> _graults;

            public Foo4(
                IList<int> bars,
                IList<int> bazes,
                IList<int> graults)
            {
                _bars = new ReadOnlyCollection<int>(bars);
                _bazes = new ReadOnlyCollection<int>(bazes);
                _graults = new ReadOnlyCollection<int>(graults);
            }

            public ReadOnlyCollection<int> Bars { get { return _bars; } }
            public IReadOnlyCollection<int> Bazes { get { return _bazes; } }
            public IReadOnlyList<int> Graults { get { return _graults; } }
        }
    }
}