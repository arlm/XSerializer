using System;
using System.IO;
using System.Text;

namespace XSerializer
{
    internal class JsonReader : IDisposable
    {
        private readonly TextReader _reader;
        private readonly IJsonSerializeOperationInfo _info;

        private StringReader _decryptedReader;
        private bool _decryptReads;

        public JsonReader(TextReader reader, IJsonSerializeOperationInfo info)
        {
            _reader = reader;
            _info = info;
        }

        public JsonNodeType NodeType { get; private set; }
        public object Value { get; private set; }

        private TextReader Reader
        {
            get
            {
                if (_decryptedReader == null)
                {
                    return _reader;
                }

                if (_decryptedReader.Peek() == -1)
                {
                    _decryptedReader = null;
                    return _reader;
                }

                return _decryptedReader;
            }
        }

        public bool DecryptReads
        {
            get { return _decryptReads; }
            set
            {
                if (value == _decryptReads)
                {
                    return;
                }

                _decryptReads = value;

                if (_decryptReads)
                {
                    if (NodeType != JsonNodeType.String)
                    {
                        throw new XSerializerException("Cannot decrypt non-string value.");
                    }

                    _decryptedReader = new StringReader(_info.EncryptionMechanism.Decrypt((string)Value, _info.EncryptKey, _info.SerializationState));
                    Read();
                }
                else
                {
                    if (_decryptedReader.Peek() != -1)
                    {
                        throw new InvalidOperationException("Attempted to set DecryptReads to false before the encrypted stream has been consumed.");
                    }

                    _decryptedReader = null;
                }
            }
        }

        public void Dispose()
        {
            // TODO: Something?
        }

        /// <summary>
        /// Reads the next non-whitespace node from the stream.
        /// </summary>
        /// <returns>true if the next node was read successfully; false if there are no more nodes to read.</returns>
        public bool ReadContent()
        {
            while (true)
            {
                if (!Read())
                {
                    return false;
                }

                if (NodeType != JsonNodeType.Whitespace)
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Reads the next node from the stream.
        /// </summary>
        /// <returns>true if the next node was read successfully; false if there are no more nodes to read.</returns>
        public bool Read()
        {
            var next = Reader.Peek();

            if (next == -1)
            {
                Value = null;
                NodeType = JsonNodeType.None;
                return false;
            }

            var c = (char)Reader.Read();

            switch (c)
            {
                case '"':
                    Value = ReadString();
                    NodeType = JsonNodeType.String;
                    break;
                case '-':
                case '.':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    Value = ReadNumber(c);
                    NodeType = JsonNodeType.Number;
                    break;
                case 't':
                    ReadLiteral("true", 'r', 'u', 'e');
                    Value = true;
                    NodeType = JsonNodeType.Boolean;
                    break;
                case 'f':
                    ReadLiteral("false", 'a', 'l', 's', 'e');
                    Value = false;
                    NodeType = JsonNodeType.Boolean;
                    break;
                case 'n':
                    ReadLiteral("null", 'u', 'l', 'l');
                    Value = null;
                    NodeType = JsonNodeType.Null;
                    break;
                case '{':
                    Value = '{';
                    NodeType = JsonNodeType.OpenObject;
                    break;
                case '}':
                    Value = '}';
                    NodeType = JsonNodeType.CloseObject;
                    break;
                case ':':
                    Value = ':';
                    NodeType = JsonNodeType.NameValueSeparator;
                    break;
                case ',':
                    Value = ',';
                    NodeType = JsonNodeType.ItemSeparator;
                    break;
                case '[':
                    Value = '[';
                    NodeType = JsonNodeType.OpenArray;
                    break;
                case ']':
                    Value = ']';
                    NodeType = JsonNodeType.CloseArray;
                    break;
                case ' ':
                case '\r':
                case '\n':
                case '\t':
                    Value = ReadWhitespace(c);
                    NodeType = JsonNodeType.Whitespace;
                    break;
            }

            return true;
        }

        private void ReadLiteral(string value, params char[] literalMinusFirstChar)
        {
            foreach (var literalChar in literalMinusFirstChar)
            {
                var peek = Reader.Peek();

                if (peek == -1)
                {
                    throw new XSerializerException(string.Format("Reached end of input before literal '{0}' was parsed.", value));
                }

                if (Reader.Read() != literalChar)
                {
                    throw new XSerializerException(string.Format("Invalid literal character '{0}' in literal '{1}.", (char)peek, value));
                }
            }
        }

        private string ReadString()
        {
            var sb = new StringBuilder();

            while (true)
            {
                var read = Reader.Read();

                if (read == -1)
                {
                    throw new XSerializerException("Reached end of input before closing quote was found for string.");
                }

                var c = (char)read;

                switch (c)
                {
                    case '"':
                        return sb.ToString();
                    case '\\':
                        sb.Append(ReadEscapedChar());
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
        }

        private char ReadEscapedChar()
        {
            var read = Reader.Read();

            if (read == -1)
            {
                throw new XSerializerException("Reached end of input before reading escaped character.");
            }

            var c = (char)read;

            switch (c)
            {
                case '"':
                case '\\':
                case '/':
                    return c;
                case 'b':
                    return '\b';
                case 'f':
                    return '\f';
                case 'n':
                    return '\n';
                case 'r':
                    return '\r';
                case 't':
                    return '\t';
                case 'u':
                    throw new NotImplementedException("Hexadecimal unicode escape characters have not yet been implemented.");
                default:
                    throw new XSerializerException("Unknown escaped character: \\" + c);
            }
        }

        private double ReadNumber(char c)
        {
            var sb = new StringBuilder();

            sb.Append(c);

            while (true)
            {
                var i = Reader.Peek();

                switch (i)
                {
                    case '+':
                    case '-':
                    case '.':
                    case 'e':
                    case 'E':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        break;
                    default:
                        var number = sb.ToString();

                        try
                        {
                            return double.Parse(number);
                        }
                        catch (FormatException ex)
                        {
                            throw new XSerializerException(string.Format("Invalid number: '{0}'.", number), ex);
                        }
                }

                sb.Append((char)Reader.Read());
            }
        }

        private string ReadWhitespace(char first)
        {
            var sb = new StringBuilder();
            sb.Append(first);

            while (IsWhitespace(Reader.Peek()))
            {
                sb.Append((char)Reader.Read());
            }

            return sb.ToString();
        }

        private static bool IsWhitespace(int c)
        {
            switch (c)
            {
                case ' ':
                case '\r':
                case '\n':
                case '\t':
                    return true;
                default:
                    return false;
            }
        }
    }
}