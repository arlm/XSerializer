using System;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using XSerializer.Encryption;

namespace XSerializer.Tests
{
    public class MalformedJsonTests
    {
        [Test]
        public void StringMissingOpenQuote()
        {
            var ex = DeserializeFail(typeof(string), @"{""Bar"":abc""}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.StringMissingOpenQuote));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('a'));
        }

        [Test]
        public void StringMissingCloseQuote()
        {
            var ex = DeserializeFail(typeof(string), @"{""Bar"":""abc}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.StringMissingCloseQuote));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void StringUnexpectedNode()
        {
            var ex = DeserializeFail(typeof(string), @"{""Bar"":[abc}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.StringMissingOpenQuote));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('['));
        }

        [Test]
        public void StringInvalidValue()
        {
            var ex = DeserializeFail(typeof(DateTime), @"{""Bar"":""abc""}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.StringInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("abc"));
        }

        [Test]
        public void StringMissingOpenQuote_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"abc"""));
            var ex = DeserializeFail(typeof(string), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.StringMissingOpenQuote));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('a'));
        }

        [Test]
        public void StringMissingCloseQuote_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"""abc"));
            var ex = DeserializeFail(typeof(string), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.StringMissingCloseQuote));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void StringUnexpectedNode_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"[abc"));
            var ex = DeserializeFail(typeof(string), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.StringMissingOpenQuote));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('['));
        }

        [Test]
        public void StringInvalidValue_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"""abc"""));
            var ex = DeserializeFail(typeof(DateTime), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.StringInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("abc"));
        }

        private static string Encrypt(string s)
        {
            return "\"" + EncryptionMechanism.Current.Encrypt(s, null, null) + "\"";
        }

        private static MalformedDocumentException DeserializeFail(
            Type propertyType, string json, bool encrypt = false)
        {
            var type = GetFooType(propertyType, encrypt);

            var serializer = JsonSerializer.Create(type, new JsonSerializerConfiguration
            {
                EncryptionMechanism = EncryptionMechanism.Current
            });

            try
            {
                serializer.Deserialize(json);
            }
            catch (MalformedDocumentException ex)
            {
                Console.WriteLine(ex);
                return ex;
            }
            catch (Exception ex)
            {
                throw new AssertionException("Expected MalformedDocumentException, but was: " + ex.GetType(), ex);
            }

            throw new AssertionException("Expected MalformedDocumentException, but no exception thrown");
        }

        private const MethodAttributes _methodAttributes = MethodAttributes.PrivateScope
                                                           | MethodAttributes.Public
                                                           | MethodAttributes.HideBySig
                                                           | MethodAttributes.SpecialName;

        private const CallingConventions _callingConventions = CallingConventions.Standard
                                                               | CallingConventions.HasThis;

        private static readonly ConstructorInfo _objectConstructor = typeof(object).GetConstructor(Type.EmptyTypes);

        private static Type GetFooType(Type propertyType, bool encrypted)
        {
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("FooAssembly"), AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule("FooModule");

            const TypeAttributes classTypeAttributes =
                TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.AutoClass
                | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout;

            var typeBuilder = moduleBuilder.DefineType("Foo", classTypeAttributes);

            var fieldBuilder = typeBuilder.DefineField("_bar", propertyType, FieldAttributes.Private);

            CreateProperty(propertyType, typeBuilder, fieldBuilder, encrypted);
            CreateConstructor(typeBuilder);

            return typeBuilder.CreateType();
        }

        private static void CreateProperty(Type propertyType, TypeBuilder typeBuilder, FieldBuilder fieldBuilder, bool encrypt)
        {
            var propertyBuilder = typeBuilder.DefineProperty("Bar", PropertyAttributes.HasDefault, propertyType, null);

            propertyBuilder.SetGetMethod(GetGetMethodBuilder(propertyType, typeBuilder, fieldBuilder));
            propertyBuilder.SetSetMethod(GetSetMethodBuilder(propertyType, typeBuilder, fieldBuilder));

            if (encrypt)
            {
                var ctor = typeof(EncryptAttribute).GetConstructor(Type.EmptyTypes);

                var attributeBuilder = new CustomAttributeBuilder(ctor, new object[0]);

                propertyBuilder.SetCustomAttribute(attributeBuilder);
            }
        }

        private static MethodBuilder GetGetMethodBuilder(Type propertyType, TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            var getMethodBuilder = typeBuilder.DefineMethod(
                "get_Bar", _methodAttributes, _callingConventions, propertyType, Type.EmptyTypes);
            
            var il = getMethodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldBuilder);
            il.Emit(OpCodes.Ret);

            return getMethodBuilder;
        }

        private static MethodBuilder GetSetMethodBuilder(Type propertyType, TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            var setMethodBuilder = typeBuilder.DefineMethod(
                "set_Bar", _methodAttributes, _callingConventions, typeof(void), new[] { propertyType });

            var il = setMethodBuilder.GetILGenerator();
            
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fieldBuilder);
            il.Emit(OpCodes.Ret);

            return setMethodBuilder;
        }

        private static void CreateConstructor(TypeBuilder typeBuilder)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

            var il = constructorBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _objectConstructor);
            il.Emit(OpCodes.Ret);
        }
    }
}