using System;

namespace XSerializer
{
    public class MalformedDocumentException : XSerializerException
    {
        private readonly string _path;
        private readonly int _line;
        private readonly int _position;
        private readonly object _value;

        internal MalformedDocumentException(string message, string path, int line, int position, Exception innerException = null)
            : base(FormatMessage(message, path, line, position, null), innerException)
        {
            _path = path;
            _line = line;
            _position = position;
        }

        internal MalformedDocumentException(string message, string path, object value, int line, int position, Exception innerException = null)
            : base(FormatMessage(message, path, line, position, value ?? "null"), innerException)
        {
            _path = path;
            _line = line;
            _position = position;
            _value = value;
        }

        private static string FormatMessage(string message, string path, int line, int position, object value)
        {
            if (value != null)
            {
                return message + " Path: " + path + ", Value: " + value + ", Line: " + line + ", Position: " + position;
            }

            return message + " Path: " + path + ", Line: " + line + ", Position: " + position;
        }

        public string Path
        {
            get { return _path; }
        }

        public int Line
        {
            get { return _line; }
        }

        public int Position
        {
            get { return _position; }
        }

        public object Value
        {
            get { return _value; }
        }
    }
}