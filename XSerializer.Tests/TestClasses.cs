using System.Xml.Serialization;

namespace XSerializer.Tests
{
    [XmlInclude(typeof(OneWithInterface))]
    public interface IOne
    {
        string Id { get; set; }
        ITwo Two { get; set; }
    }

    [XmlInclude(typeof(OneWithAbstract))]
    public abstract class OneBase
    {
        public string Id { get; set; }
        public abstract TwoBase Two { get; set; }
    }

    public class OneWithInterface : IOne
    {
        public string Id { get; set; }
        public ITwo Two { get; set; }
    }

    public class OneWithAbstract : OneBase
    {
        public override TwoBase Two { get; set; }
    }

    [XmlInclude(typeof(TwoWithInterface))]
    public interface ITwo
    {
        string Id { get; set; }
        string Value { get; set; }
    }

    [XmlInclude(typeof(TwoWithAbstract))]
    public abstract class TwoBase
    {
        public string Id { get; set; }
        public abstract string Value { get; set; }
    }

    public class TwoWithInterface : ITwo
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

    public class TwoWithAbstract : TwoBase
    {
        public override string Value { get; set; }
    }

    [XmlRoot("Container")]
    public class ContainerWithInterface
    {
        public string Id { get; set; }
        public IOne One { get; set; }
    }

    [XmlRoot("Container")]
    public class ContainerWithAbstract
    {
        public string Id { get; set; }
        public OneBase One { get; set; }
    }

    public class Foot
    {
        public Bark Bark { get; set; }
    }

    [XmlInclude(typeof(Barnicle))]
    public interface IBar
    {
    }

    [XmlInclude(typeof(Barnicle))]
    public abstract class BarBase : IBar
    {
    }

    public class Bark : BarBase
    {
        public int Bite { get; set; }
    }

    public class IntClass
    {
        public int Value { get; set; }
    }

    public class Fool
    {
        public Barn Barn { get; set; }
    }

    public class Barn : BarBase
    {
        [XmlAttribute]
        public int CowCount { get; set; }

        public string Color { get; set; }
    }

    public class Food
    {
        public Barnicle Barnicle { get; set; }
    }

    public class Barnicle : BarBase
    {
        [XmlAttribute]
        public bool IsAttached { get; set; }

        [XmlText]
        public string Chantey { get; set; }
    }

    [XmlRoot("Foo")]
    public class FooWithAbstract
    {
        public BarBase Bar { get; set; }
    }

    [XmlRoot("Foo")]
    public class FooWithInterface
    {
        public IBar Bar { get; set; }
    }

    public class JitPreparation
    {
    }
}