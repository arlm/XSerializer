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
        public void ObjectMissingOpenCurlyBrace()
        {
            var ex = DeserializeFail(typeof(int), @"""Bar"":123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingOpenCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ObjectMissingCloseCurlyBrace()
        {
            var ex = DeserializeFail(typeof(int), @"{""Bar"":123");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingCloseCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(10));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyNameMissingOpenQuote()
        {
            var ex = DeserializeFail(typeof(int), @"{Bar"":123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissingOpenQuote));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(1));
            Assert.That(ex.Value, Is.EqualTo('B'));
        }

        [Test]
        public void PropertyNameMissingCloseQuote()
        {
            var ex = DeserializeFail(typeof(int), @"{""Bar:123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissingCloseQuote));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(1));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyInvalidName()
        {
            var ex = DeserializeFail(typeof(int), @"{123:456}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyInvalidName));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(1));
            Assert.That(ex.Value, Is.EqualTo("123"));
        }

        [Test]
        public void PropertyMissingSeparator1()
        {
            var ex = DeserializeFail(typeof(int), @"{""Bar""123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(6));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyMissingSeparator2()
        {
            var ex = DeserializeFail(typeof(int), @"{""Bar""wtf}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(6));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyMissingNameValueSeparator()
        {
            var ex = DeserializeFail(typeof(int), @"{""Bar"":123""Baz"":456}", bazPropertyType:typeof(int));

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingItemSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(10));
            Assert.That(ex.Value, Is.Null);
        }

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
            Type barPropertyType, string json, bool encrypt = false, Type bazPropertyType = null)
        {
            var type = GetFooType(barPropertyType, encrypt, bazPropertyType);

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
                Console.WriteLine(ex);
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

        private static Type GetFooType(Type barPropertyType, bool encrypted, Type bazPropertyType)
        {
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("FooAssembly"), AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule("FooModule");

            const TypeAttributes classTypeAttributes =
                TypeAttributes.NotPublic | TypeAttributes.Class | TypeAttributes.AutoClass
                | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout;

            var typeBuilder = moduleBuilder.DefineType("Foo", classTypeAttributes);

            var barFieldBuilder = typeBuilder.DefineField("_bar", barPropertyType, FieldAttributes.Private);
            CreateProperty(barPropertyType, typeBuilder, barFieldBuilder, encrypted, "Bar");

            if (bazPropertyType != null)
            {
                var bazFieldBuilder = typeBuilder.DefineField("_baz", barPropertyType, FieldAttributes.Private);
                CreateProperty(bazPropertyType, typeBuilder, bazFieldBuilder, encrypted, "Baz");
            }

            CreateConstructor(typeBuilder);

            return typeBuilder.CreateType();
        }

        private static void CreateProperty(Type propertyType, TypeBuilder typeBuilder, FieldBuilder fieldBuilder, bool encrypt, string name)
        {
            var propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, propertyType, null);

            propertyBuilder.SetGetMethod(GetGetMethodBuilder(propertyType, typeBuilder, fieldBuilder, name));
            propertyBuilder.SetSetMethod(GetSetMethodBuilder(propertyType, typeBuilder, fieldBuilder, name));

            if (encrypt)
            {
                var ctor = typeof(EncryptAttribute).GetConstructor(Type.EmptyTypes);

                var attributeBuilder = new CustomAttributeBuilder(ctor, new object[0]);

                propertyBuilder.SetCustomAttribute(attributeBuilder);
            }
        }

        private static MethodBuilder GetGetMethodBuilder(Type propertyType, TypeBuilder typeBuilder, FieldBuilder fieldBuilder, string name)
        {
            var getMethodBuilder = typeBuilder.DefineMethod(
                "get_" + name, _methodAttributes, _callingConventions, propertyType, Type.EmptyTypes);
            
            var il = getMethodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldBuilder);
            il.Emit(OpCodes.Ret);

            return getMethodBuilder;
        }

        private static MethodBuilder GetSetMethodBuilder(Type propertyType, TypeBuilder typeBuilder, FieldBuilder fieldBuilder, string name)
        {
            var setMethodBuilder = typeBuilder.DefineMethod(
                "set_" + name, _methodAttributes, _callingConventions, typeof(void), new[] { propertyType });

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