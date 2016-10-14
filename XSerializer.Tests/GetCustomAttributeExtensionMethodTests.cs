using System;
using NUnit.Framework;
using Haz=NUnit.Framework.Has;

namespace XSerializer.Tests
{
    public class GetCustomAttributeExtensionMethodTests
    {
        public class PropertyAttributes
        {
            [Test]
            public void WhenAnAttributeDecoratesAnInterfacePropertyAndItIsImplementedImplicitlyAndShouldUseAttributeDefinedInInterfaceIsTrueThenTheInterfacePropertyAttributeIsReturned()
            {
                var property = typeof(InterfaceBehavior).GetProperty("Bar");

                var attributes = property.GetCustomAttributes<ExampleAttribute>(true);

                Assert.That(attributes, Haz.Length.EqualTo(1));
                Assert.That(attributes[0].Type, Is.EqualTo(typeof(IFoo)));
            }

            [Test]
            public void WhenAnAttributeDecoratesAnInterfacePropertyAndItIsImplementedImplicitlyAndShouldUseAttributeDefinedInInterfaceIsFalseThenNoAttributesAreReturned()
            {
                var property = typeof(InterfaceBehavior).GetProperty("Bar");

                var attributes = property.GetCustomAttributes<ExampleAttribute>(false);

                Assert.That(attributes, Is.Empty);
            }

            [Test]
            public void WhenAnAttributeDecoratesAnInterfacePropertyAndItIsImplementedExplicitlyAndShouldUseAttributeDefinedInInterfaceIsTrueThenNoAttributesAreReturned()
            {
                var property = typeof(RemovingBehavior).GetProperty("Bar");

                var attributes = property.GetCustomAttributes<ExampleAttribute>(true);

                Assert.That(attributes, Is.Empty);
            }

            [Test]
            public void WhenAnAttributeDecoratesAnInterfacePropertyAndItIsImplementedImplicitlyAndTheClassPropertyIsAlsoDecoratedWithTheSameAttributeTypeAndShouldUseAttributeDefinedInInterfaceIsTrueThenTheClassPropertyAttributeThenTheInterfacePropertyAttributeAreReturned()
            {
                var property = typeof(OverridingBehavior).GetProperty("Bar");

                var attributes = property.GetCustomAttributes<ExampleAttribute>(true);

                Assert.That(attributes, Haz.Length.EqualTo(2));
                Assert.That(attributes[0].Type, Is.EqualTo(typeof(OverridingBehavior)));
                Assert.That(attributes[1].Type, Is.EqualTo(typeof(IFoo)));
            }
        }

        public class TypeAttributes
        {
            [Test]
            public void WhenSearchingForAttributesDecoratingATypeThenTheOnlyAttributesFromThatTypeAreReturned()
            {
                var type = typeof(TypeBehavior);

                var attributes = type.GetCustomAttributes<ExampleAttribute>();

                Assert.That(attributes, Haz.Length.EqualTo(1));
                Assert.That(attributes[0].Type, Is.EqualTo(typeof(TypeBehavior)));
            }
        }

        public class ExampleAttribute : Attribute
        {
            private readonly Type _type;

            public ExampleAttribute(Type type)
            {
                _type = type;
            }

            public Type Type
            {
                get { return _type; }
            }
        }

        [Example(typeof(IFoo))]
        public interface IFoo
        {
            [Example(typeof(IFoo))]
            string Bar { get; set; }
        }

        public class InterfaceBehavior : IFoo
        {
            public string Bar { get; set; }
        }

        public class RemovingBehavior : IFoo
        {
            public string Bar { get; set; }
            string IFoo.Bar { get { return Bar; } set { Bar = value; } }
        }

        public class OverridingBehavior : IFoo
        {
            [Example(typeof(OverridingBehavior))]
            public string Bar { get; set; }
        }

        [Example(typeof(ExampleBaseClass))]
        public abstract class ExampleBaseClass
        {
        }

        [Example(typeof(IExampleInterface))]
        public interface IExampleInterface
        {
        }

        [Example(typeof(TypeBehavior))]
        public class TypeBehavior : ExampleBaseClass, IExampleInterface
        {
        }
    }
}