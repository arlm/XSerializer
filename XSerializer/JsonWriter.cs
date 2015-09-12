using System;
using System.IO;

namespace XSerializer
{
    internal class JsonWriter : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly IJsonSerializeOperationInfo _info;

        private StringWriter _encryptingStringWriter;
        private bool _encryptWrites;

        public JsonWriter(TextWriter writer, IJsonSerializeOperationInfo info)
        {
            _writer = writer;
            _info = info;
        }

        private TextWriter Writer
        {
            get
            {
                return _encryptingStringWriter ?? _writer;
            }
        }

        public bool EncryptWrites
        {
            get { return _encryptWrites; }
            set
            {
                if (value == _encryptWrites)
                {
                    return;
                }

                Flush();
                _encryptWrites = value;

                if (_encryptWrites)
                {
                    // We're starting an encrypt session
                    _encryptingStringWriter = new StringWriter();
                }
                else
                {
                    var plainText = _encryptingStringWriter.GetStringBuilder().ToString();
                    _encryptingStringWriter = null;

                    // We're ending an encrypt session - encrypt if a mechanism exists.
                    if (_info.EncryptionMechanism != null)
                    {
                        var encryptedValue =
                            _info.EncryptionMechanism.Encrypt(
                                plainText,
                                _info.EncryptKey,
                                _info.SerializationState);

                        WriteValue(encryptedValue);
                    }
                    else
                    {
                        // No encryption mechanism exists - just write the plain text.
                        Writer.Write(plainText);
                    }
                }
            }
        }

        public void WriteValue(string value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                Writer.Write('"');
                Writer.Write(Escape(value));
                Writer.Write('"');
            }
        }

        public void WriteValue(double value)
        {
            Writer.Write(value);
        }

        public void WriteValue(bool value)
        {
            Writer.Write(value ? "true" : "false");
        }

        public void WriteNull()
        {
            Writer.Write("null");
        }

        public void WriteOpenObject()
        {
            Writer.Write('{');
        }

        public void WriteCloseObject()
        {
            Writer.Write('}');
        }

        public void WriteNameValueSeparator()
        {
            Writer.Write(':');
        }

        public void WriteItemSeparator()
        {
            Writer.Write(',');
        }

        public void WriteOpenArray()
        {
            Writer.Write('[');
        }

        public void WriteCloseArray()
        {
            Writer.Write(']');
        }

        public void Flush()
        {
            Writer.Flush();
        }

        public void Dispose()
        {
            // TODO: Something?
        }

        private static string Escape(string value)
        {
            int flags = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '\\':
                        flags |= 0x01;
                        break;
                    case '"':
                        flags |= 0x02;
                        break;
                    case '/':
                        flags |= 0x04;
                        break;
                    case '\b':
                        flags |= 0x08;
                        break;
                    case '\f':
                        flags |= 0x10;
                        break;
                    case '\n':
                        flags |= 0x20;
                        break;
                    case '\r':
                        flags |= 0x40;
                        break;
                    case '\t':
                        flags |= 0x80;
                        break;
                }
            }

            if (flags == 0)
            {
                return value;
            }

            if ((flags & 0x01) == 0x01)
            {
                value = value.Replace(@"\", @"\\");
            }

            if ((flags & 0x02) == 0x02)
            {
                value = value.Replace(@"""", @"\""");
            }

            if ((flags & 0x04) == 0x04)
            {
                value = value.Replace("/", @"\/");
            }

            if ((flags & 0x08) == 0x08)
            {
                value = value.Replace("\b", @"\b");
            }

            if ((flags & 0x10) == 0x10)
            {
                value = value.Replace("\f", @"\f");
            }

            if ((flags & 0x20) == 0x20)
            {
                value = value.Replace("\n", @"\n");
            }

            if ((flags & 0x40) == 0x40)
            {
                value = value.Replace("\r", @"\r");
            }

            if ((flags & 0x80) == 0x80)
            {
                value = value.Replace("\t", @"\t");
            }

            return value;
        }
    }
}