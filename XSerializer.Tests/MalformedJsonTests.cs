using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using XSerializer.Encryption;

namespace XSerializer.Tests
{
    public class MalformedJsonTests
    {
        public class Bar
        {
            public int Baz { get; set; }
        }
        
        public class Bar2
        {
            public int Baz { get; set; }
            public int Qux { get; set; }
        }

        [Test]
        public void PathForNestedObjectIsCorrect()
        {
            var ex = DeserializeFail(typeof(Bar), @"{""Bar"":{""Baz"":123.45}}");

            Assert.That(ex.Path, Is.EqualTo("Bar.Baz"));
        }

        [Test]
        public void PathForArrayIsCorrect()
        {
            var ex = DeserializeFail(typeof(Bar[]), @"{""Bar"":[{""Baz"":123},{""Baz"":123.45},{""Baz"":123}]}");

            Assert.That(ex.Path, Is.EqualTo("Bar[1].Baz"));
        }

        [Test]
        public void ObjectMissingOpenCurlyBrace1()
        {
            var ex = DeserializeFail(typeof(int), @"""Bar"":123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingOpenCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ObjectMissingOpenCurlyBrace2()
        {
            var ex = DeserializeFail(typeof(Bar), @"{""Bar"":""Baz"":123}}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingOpenCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ObjectMissingOpenCurlyBrace2_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"""Baz"":123}"));
            var ex = DeserializeFail(typeof(Bar), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingOpenCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ObjectMissingOpenCurlyBrace3()
        {
            var ex = DeserializeFail(typeof(string), @"wtf");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingOpenCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void ObjectMissingValue()
        {
            var ex = DeserializeFail(typeof(string), @"");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ObjectMissingValue_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(""));
            var ex = DeserializeFail(typeof(Bar), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ObjectMissingCloseCurlyBrace1()
        {
            var ex = DeserializeFail(typeof(int), @"{""Bar"":123");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingCloseCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(10));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ObjectMissingCloseCurlyBrace2()
        {
            var ex = DeserializeFail(typeof(Bar), @"{""Bar"":{""Baz"":123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingCloseCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(18));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ObjectMissingCloseCurlyBrace2_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{""Baz"":123"));
            var ex = DeserializeFail(typeof(Bar), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingCloseCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar.Baz"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ObjectExpectedEndOfString1()
        {
            var ex = DeserializeFail<Bar>(@"{""Baz"":123}true");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(11));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void ObjectExpectedEndOfDecryptedString1_Encrypted()
        {
            var ex = DeserializeFail<Bar>(Encrypt(@"{""Baz"":123}true"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void ObjectExpectedEndOfString2()
        {
            var ex = DeserializeFail<Bar>(@"{""Baz"":123}wtf");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(11));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void ObjectExpectedEndOfDecryptedString2_Encrypted()
        {
            var ex = DeserializeFail<Bar>(Encrypt(@"{""Baz"":123}wtf"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void DictionaryMissingOpenCurlyBrace1()
        {
            var ex = DeserializeFail(typeof(Dictionary<string, int>), @"{""Bar"":""Baz"":123}}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingOpenCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DictionaryMissingOpenCurlyBrace1_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"""Baz"":123}"));
            var ex = DeserializeFail(typeof(Dictionary<string, int>), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingOpenCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DictionaryMissingOpenCurlyBrace2()
        {
            var ex = DeserializeFail<Dictionary<string, int>>(@"wtf");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingOpenCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void DictionaryMissingValue()
        {
            var ex = DeserializeFail<Dictionary<string, int>>(@"");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DictionaryMissingValue_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(""));
            var ex = DeserializeFail(typeof(Dictionary<string, int>), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DictionaryMissingCloseCurlyBrace1()
        {
            var ex = DeserializeFail<Dictionary<string, int>>(@"{""Bar"":123");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingCloseCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(10));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DictionaryMissingCloseCurlyBrace2()
        {
            var ex = DeserializeFail(typeof(Dictionary<string, int>), @"{""Bar"":{""Baz"":123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingCloseCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(18));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DictionaryMissingCloseCurlyBrace2_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{""Baz"":123"));
            var ex = DeserializeFail(typeof(Dictionary<string, int>), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingCloseCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar.Baz"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DictionaryExpectedEndOfString1()
        {
            var ex = DeserializeFail<Dictionary<string, int>>(@"{""Baz"":123}true");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(11));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void DictionaryExpectedEndOfDecryptedString1_Encrypted()
        {
            var ex = DeserializeFail<Dictionary<string, int>>(Encrypt(@"{""Baz"":123}true"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void DictionaryExpectedEndOfString2()
        {
            var ex = DeserializeFail<Dictionary<string, int>>(@"{""Baz"":123}wtf");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(11));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void DictionaryExpectedEndOfDecryptedString2_Encrypted()
        {
            var ex = DeserializeFail<Dictionary<string, int>>(Encrypt(@"{""Baz"":123}wtf"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void PropertyNameMissing()
        {
            var ex = DeserializeFail(typeof(int), @"{");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissing));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(1));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyNameMissing_Encrypted1()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{"));
            var ex = DeserializeFail(typeof(Bar), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissing));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyNameMissing_Encrypted2()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{"));
            var ex = DeserializeFail(typeof(Dictionary<string, int>), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissing));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
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
        public void PropertyNameMissingOpenQuote_Encrypted1()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{Baz"":123}"));
            var ex = DeserializeFail(typeof(Bar), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissingOpenQuote));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('B'));
        }

        [Test]
        public void PropertyNameMissingOpenQuote_Encrypted2()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{Baz"":123}"));
            var ex = DeserializeFail(typeof(Dictionary<string, int>), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissingOpenQuote));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
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
        public void PropertyNameMissingCloseQuote_Encrypted1()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{""Baz:123}"));
            var ex = DeserializeFail(typeof(Bar), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissingCloseQuote));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyNameMissingCloseQuote_Encrypted2()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{""Baz:123}"));
            var ex = DeserializeFail(typeof(Dictionary<string, int>), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissingCloseQuote));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
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
        public void PropertyInvalidName_Encrypted1()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{123:456}"));
            var ex = DeserializeFail(typeof(Bar), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyInvalidName));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("123"));
        }

        [Test]
        public void PropertyInvalidName_Encrypted2()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{123:456}"));
            var ex = DeserializeFail(typeof(Dictionary<string, int>), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyInvalidName));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("123"));
        }

        [Test]
        public void PropertyMissingNameValueSeparator1()
        {
            var ex = DeserializeFail(typeof(int), @"{""Bar""123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(6));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyMissingNameValueSeparator1_Encrypted1()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{""Baz""123}"));
            var ex = DeserializeFail(typeof(Bar), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar.Baz"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyMissingNameValueSeparator1_Encrypted2()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{""Baz""123}"));
            var ex = DeserializeFail(typeof(Dictionary<string, int>), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar.Baz"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyMissingNameValueSeparator2()
        {
            var ex = DeserializeFail(typeof(int), @"{""Bar""wtf}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(6));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyMissingNameValueSeparator2_Encrypted1()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{""Baz""wtf}"));
            var ex = DeserializeFail(typeof(Bar), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar.Baz"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyMissingNameValueSeparator2_Encrypted2()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{""Baz""wtf}"));
            var ex = DeserializeFail(typeof(Dictionary<string, int>), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar.Baz"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyMissingItemSeparator()
        {
            var ex = DeserializeFail(typeof(int), @"{""Bar"":123""Baz"":456}", bazPropertyType:typeof(int));

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingItemSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(10));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyMissingItemSeparator_Encrypted1()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{""Baz"":123""Qux"":456}"));
            var ex = DeserializeFail(typeof(Bar2), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingItemSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar.Baz"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void PropertyMissingItemSeparator_Encrypted2()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"{""Baz"":123""Qux"":456}"));
            var ex = DeserializeFail(typeof(Dictionary<string, int>), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingItemSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar.Baz"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
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
            Assert.That(ex.Value, Is.EqualTo(@"""abc}"));
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
            Assert.That(ex.Value, Is.EqualTo(@"""abc"));
        }

        [Test]
        public void StringMissingValue()
        {
            var ex = DeserializeFail(typeof(string), @"{""Bar"":");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void StringMissingValue_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(""));
            var ex = DeserializeFail(typeof(string), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
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

        [Test]
        public void LiteralInvalidValue1()
        {
            var ex = DeserializeFail(typeof(bool), @"{""Bar"":tr");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("tr"));
        }

        [Test]
        public void LiteralInvalidValue1_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("tr"));
            var ex = DeserializeFail(typeof(bool), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("tr"));
        }

        [Test]
        public void LiteralInvalidValue2()
        {
            var ex = DeserializeFail(typeof(bool), @"{""Bar"":twoo}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("tw"));
        }

        [Test]
        public void LiteralInvalidValue2_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("twoo"));
            var ex = DeserializeFail(typeof(bool), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("tw"));
        }

        [Test]
        public void LiteralInvalidValue3()
        {
            var ex = DeserializeFail(typeof(bool), @"{""Bar"":fal");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("fal"));
        }

        [Test]
        public void LiteralInvalidValue3_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("fal"));
            var ex = DeserializeFail(typeof(bool), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("fal"));
        }

        [Test]
        public void LiteralInvalidValue4()
        {
            var ex = DeserializeFail(typeof(bool), @"{""Bar"":falce}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("falc"));
        }

        [Test]
        public void LiteralInvalidValue4_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("falce"));
            var ex = DeserializeFail(typeof(bool), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("falc"));
        }

        [Test]
        public void LiteralInvalidValue5()
        {
            var ex = DeserializeFail(typeof(object), @"{""Bar"":nul");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("nul"));
        }

        [Test]
        public void LiteralInvalidValue5_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("nul"));
            var ex = DeserializeFail(typeof(object), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("nul"));
        }

        [Test]
        public void LiteralInvalidValue6()
        {
            var ex = DeserializeFail(typeof(object), @"{""Bar"":nun}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("nun"));
        }

        [Test]
        public void LiteralInvalidValue6_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("nun"));
            var ex = DeserializeFail(typeof(object), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("nun"));
        }

        [Test]
        public void BooleanMissingValue()
        {
            var ex = DeserializeFail(typeof(bool), @"{""Bar"":");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void BooleanMissingValue_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(""));
            var ex = DeserializeFail(typeof(bool), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void BooleanInvalidValue1()
        {
            var ex = DeserializeFail(typeof(bool), @"{""Bar"":asdf");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.BooleanInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('a'));
        }

        [Test]
        public void BooleanInvalidValue1_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("asdf"));
            var ex = DeserializeFail(typeof(bool), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.BooleanInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('a'));
        }

        [Test]
        public void BooleanInvalidValue2()
        {
            var ex = DeserializeFail(typeof(bool), @"{""Bar"":""twoo""");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.BooleanInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("twoo"));
        }

        [Test]
        public void BooleanInvalidValue2_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(@"""twoo"""));
            var ex = DeserializeFail(typeof(bool), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.BooleanInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("twoo"));
        }

        [Test]
        public void BooleanInvalidValue3()
        {
            var ex = DeserializeFail(typeof(bool), @"{""Bar"":[true]");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.BooleanInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('['));
        }

        [Test]
        public void BooleanInvalidValue3_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("[true]"));
            var ex = DeserializeFail(typeof(bool), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.BooleanInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('['));
        }

        [Test]
        public void BooleanExpectedEndOfString1()
        {
            var ex = DeserializeFail<bool>(@"truetrue");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(4));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void BooleanExpectedEndOfDecryptedString1_Encrypted()
        {
            var ex = DeserializeFail<bool>(Encrypt(@"truetrue"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void BooleanExpectedEndOfString2()
        {
            var ex = DeserializeFail<bool>(@"truewtf");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(4));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void BooleanExpectedEndOfDecryptedString2_Encrypted()
        {
            var ex = DeserializeFail<bool>(Encrypt(@"truewtf"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void NumberMissingValue()
        {
            var ex = DeserializeFail(typeof(double), @"{""Bar"":");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void NumberMissingValue_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(""));
            var ex = DeserializeFail(typeof(double), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void NumberInvalidValue1()
        {
            var ex = DeserializeFail(typeof(double), @"{""Bar"":wtf}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.NumberInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void NumberInvalidValue1_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("wtf"));
            var ex = DeserializeFail(typeof(double), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.NumberInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void NumberInvalidValue2()
        {
            var ex = DeserializeFail(typeof(double), @"{""Bar"":1.2.3}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.NumberInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("1.2.3"));
        }

        [Test]
        public void NumberInvalidValue2_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("1.2.3"));
            var ex = DeserializeFail(typeof(double), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.NumberInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("1.2.3"));
        }

        [Test]
        public void NumberInvalidValue3()
        {
            var ex = DeserializeFail(typeof(double), @"{""Bar"":true}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.NumberInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("true"));
        }

        [Test]
        public void NumberInvalidValue3_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("true"));
            var ex = DeserializeFail(typeof(double), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.NumberInvalidValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("true"));
        }

        [Test]
        public void NumberExpectedEndOfString1()
        {
            var ex = DeserializeFail<double>(@"123.45true");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(6));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void NumberExpectedEndOfDecryptedString1_Encrypted()
        {
            var ex = DeserializeFail<double>(Encrypt(@"123.45true"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void NumberExpectedEndOfString2()
        {
            var ex = DeserializeFail<double>(@"123.45wtf");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(6));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void NumberExpectedEndOfDecryptedString2_Encrypted()
        {
            var ex = DeserializeFail<double>(Encrypt(@"123.45wtf"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void ArrayMissingValue()
        {
            var ex = DeserializeFail(typeof(double[]), @"{""Bar"":");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ArrayMissingValue_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt(""));
            var ex = DeserializeFail(typeof(double[]), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ArrayMissingOpenSquareBracket1()
        {
            var ex = DeserializeFail(typeof(double[]), @"{""Bar"":1,2,3]}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ArrayMissingOpenSquareBracket));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("1"));
        }

        [Test]
        public void ArrayMissingOpenSquareBracket1_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("1,2,3]"));
            var ex = DeserializeFail(typeof(double[]), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ArrayMissingOpenSquareBracket));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo("1"));
        }

        [Test]
        public void ArrayMissingOpenSquareBracket2()
        {
            var ex = DeserializeFail(typeof(double[]), @"{""Bar"":wtf}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ArrayMissingOpenSquareBracket));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void ArrayMissingOpenSquareBracket2_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("wtf"));
            var ex = DeserializeFail(typeof(double[]), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ArrayMissingOpenSquareBracket));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void ArrayMissingCommaOrCloseSquareBracket1()
        {
            var ex = DeserializeFail(typeof(double[]), @"{""Bar"":[1,2,3}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ArrayMissingCommaOrCloseSquareBracket));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(13));
            Assert.That(ex.Value, Is.EqualTo('}'));
        }

        [Test]
        public void ArrayMissingCommaOrCloseSquareBracket1_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}}}", Encrypt("[1,2,3"));
            var ex = DeserializeFail(typeof(double[]), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ArrayMissingCommaOrCloseSquareBracket));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ArrayMissingCommaOrCloseSquareBracket2()
        {
            var ex = DeserializeFail(typeof(double[]), @"{""Bar"":[1,2,3");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ArrayMissingCommaOrCloseSquareBracket));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(13));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ArrayMissingCommaOrCloseSquareBracket2_Encrypted()
        {
            var json = string.Format(@"{{""Bar"":{0}", Encrypt("[1,2,3"));
            var ex = DeserializeFail(typeof(double[]), json, true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ArrayMissingCommaOrCloseSquareBracket));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void ArrayExpectedEndOfString1()
        {
            var ex = DeserializeFail<double[]>(@"[1,2,3]true");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void ArrayExpectedEndOfDecryptedString1_Encrypted()
        {
            var ex = DeserializeFail<double[]>(Encrypt(@"[1,2,3]true"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void ArrayExpectedEndOfString2()
        {
            var ex = DeserializeFail<double[]>(@"[1,2,3]wtf");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void ArrayExpectedEndOfDecryptedString2_Encrypted()
        {
            var ex = DeserializeFail<double[]>(Encrypt(@"[1,2,3]wtf"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void DynamicMissingValue()
        {
            var ex = DeserializeFail<object>(@"");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicMissingValue_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@""), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.MissingValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicInvalidValue()
        {
            var ex = DeserializeFail<object>(@"wtf");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.InvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void DynamicInvalidValue_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"wtf"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.InvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void DynamicStringMissingCloseQuote()
        {
            var ex = DeserializeFail<object>(@"""abc");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.InvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo(@"""abc"));
        }

        [Test]
        public void DynamicStringMissingCloseQuote_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"""abc"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.InvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo(@"""abc"));
        }

        [Test]
        public void DynamicLiteralInvalidValue1()
        {
            var ex = DeserializeFail<object>("tr");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("tr"));
        }

        [Test]
        public void DynamicLiteralInvalidValue1_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt("tr"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("tr"));
        }

        [Test]
        public void DynamicLiteralInvalidValue2()
        {
            var ex = DeserializeFail<object>("twoo");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("tw"));
        }

        [Test]
        public void DynamicLiteralInvalidValue2_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt("twoo"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("tw"));
        }

        [Test]
        public void DynamicLiteralInvalidValue3()
        {
            var ex = DeserializeFail<object>("fal");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("fal"));
        }

        [Test]
        public void DynamicLiteralInvalidValue3_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt("fal"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("fal"));
        }

        [Test]
        public void DynamicLiteralInvalidValue4()
        {
            var ex = DeserializeFail<object>("falce");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("falc"));
        }

        [Test]
        public void DynamicLiteralInvalidValue4_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt("falce"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("falc"));
        }

        [Test]
        public void DynamicLiteralInvalidValue5()
        {
            var ex = DeserializeFail<object>("nul");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("nul"));
        }

        [Test]
        public void DynamicLiteralInvalidValue5_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt("nul"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("nul"));
        }

        [Test]
        public void DynamicLiteralInvalidValue6()
        {
            var ex = DeserializeFail<object>("nun");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("nun"));
        }

        [Test]
        public void DynamicLiteralInvalidValue6_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt("nun"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.LiteralInvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("nun"));
        }

        [Test]
        public void DynamicNumberInvalidValue()
        {
            var ex = DeserializeFail<object>("1.2.3");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.InvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("1.2.3"));
        }

        [Test]
        public void DynamicNumberInvalidValue_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt("1.2.3"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.InvalidValue));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("1.2.3"));
        }

        [Test]
        public void DynamicNumberArrayMissingOpenSquareBracket()
        {
            var ex = DeserializeFail<object>("1,2,3]");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(1));
            Assert.That(ex.Value, Is.EqualTo(','));
        }

        [Test]
        public void DynamicNumberArrayMissingOpenSquareBracket_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt("1,2,3]"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo(','));
        }

        [Test]
        public void DynamicArrayMissingCommaOrCloseSquareBracket()
        {
            var ex = DeserializeFail<object>("[1,2,3");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ArrayMissingCommaOrCloseSquareBracket));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(6));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicArrayMissingCommaOrCloseSquareBracket_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt("[1,2,3"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ArrayMissingCommaOrCloseSquareBracket));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicObjectMissingOpenCurlyBrace()
        {
            var ex = DeserializeFail<object>(@"""Bar"":123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(5));
            Assert.That(ex.Value, Is.EqualTo(':'));
        }

        [Test]
        public void DynamicObjectMissingOpenCurlyBrace_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"""Bar"":123}"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo(':'));
        }

        [Test]
        public void DynamicObjectMissingCloseCurlyBrace()
        {
            var ex = DeserializeFail<object>(@"{""Bar"":123");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingCloseCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(10));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicObjectMissingCloseCurlyBrace_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"{""Bar"":123"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ObjectMissingCloseCurlyBrace));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicPropertyNameMissing()
        {
            var ex = DeserializeFail<object>(@"{");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissing));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(1));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicPropertyNameMissing_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"{"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissing));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicPropertyNameMissingOpenQuote()
        {
            var ex = DeserializeFail<object>(@"{Bar"":123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissingOpenQuote));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(1));
            Assert.That(ex.Value, Is.EqualTo('B'));
        }

        [Test]
        public void DynamicPropertyNameMissingOpenQuote_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"{Bar"":123}"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissingOpenQuote));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('B'));
        }

        [Test]
        public void DynamicPropertyNameMissingCloseQuote()
        {
            var ex = DeserializeFail<object>(@"{""Bar:123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissingCloseQuote));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(1));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicPropertyNameMissingCloseQuote_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"{""Bar:123}"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyNameMissingCloseQuote));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicPropertyInvalidName()
        {
            var ex = DeserializeFail<object>(@"{123:456}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyInvalidName));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(1));
            Assert.That(ex.Value, Is.EqualTo("123"));
        }

        [Test]
        public void DynamicPropertyInvalidName_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"{123:456}"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyInvalidName));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo("123"));
        }

        [Test]
        public void DynamicPropertyMissingNameValueSeparator1()
        {
            var ex = DeserializeFail<object>(@"{""Bar""123}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(6));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicPropertyMissingNameValueSeparator1_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"{""Bar""123}"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicPropertyMissingNameValueSeparator2()
        {
            var ex = DeserializeFail<object>(@"{""Bar""wtf}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(6));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicPropertyMissingNameValueSeparator2_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"{""Bar""wtf}"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingNameValueSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicPropertyMissingItemSeparator()
        {
            var ex = DeserializeFail<object>(@"{""Bar"":123""Baz"":456}");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingItemSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(10));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicPropertyMissingItemSeparator_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"{""Bar"":123""Baz"":456}"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.PropertyMissingItemSeparator));
            Assert.That(ex.Path, Is.EqualTo("Bar"));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.Null);
        }

        [Test]
        public void DynamicExpectedEndOfString1()
        {
            var ex = DeserializeFail<object>(@"{""Bar"":123}true");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(11));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void DynamicExpectedEndOfDecryptedString1_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"{""Bar"":123}true"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.True);
        }

        [Test]
        public void DynamicExpectedEndOfString2()
        {
            var ex = DeserializeFail<object>(@"{""Bar"":123}wtf");

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(11));
            Assert.That(ex.Value, Is.EqualTo('w'));
        }

        [Test]
        public void DynamicExpectedEndOfDecryptedString2_Encrypted()
        {
            var ex = DeserializeFail<object>(Encrypt(@"{""Bar"":123}wtf"), true);

            Assert.That(ex.Error, Is.EqualTo(MalformedDocumentError.ExpectedEndOfDecryptedString));
            Assert.That(ex.Path, Is.EqualTo(""));
            Assert.That(ex.Line, Is.EqualTo(0));
            Assert.That(ex.Position, Is.EqualTo(0));
            Assert.That(ex.Value, Is.EqualTo('w'));
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

        private static MalformedDocumentException DeserializeFail<T>(string json, bool encrypt = false)
        {
            var serializer = JsonSerializer.Create(typeof(T), new JsonSerializerConfiguration
            {
                EncryptionMechanism = EncryptionMechanism.Current,
                EncryptRootObject = encrypt
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