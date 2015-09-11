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
                Writer.Write(value
                        .Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("/", "\\/")
                        .Replace("\b", "\\b")
                        .Replace("\f", "\\f")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                        .Replace("\t", "\\t"));
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
    }
}