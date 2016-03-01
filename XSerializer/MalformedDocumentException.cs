using System;
using System.Diagnostics;

namespace XSerializer
{
    public class MalformedDocumentException : XSerializerException
    {
        private readonly MalformedDocumentError _error;
        private readonly string _path;
        private readonly int _line;
        private readonly int _position;
        private readonly object _value;

        internal MalformedDocumentException(MalformedDocumentError error, string path, int line, int position, Exception innerException = null, params object[] additionalArgs)
            : base(FormatMessage(error, path, line, position, null, additionalArgs), innerException)
        {
            _error = error;
            _path = path;
            _line = line;
            _position = position;
        }

        internal MalformedDocumentException(MalformedDocumentError error, string path, object value, int line, int position, Exception innerException = null, params object[] additionalArgs)
            : base(FormatMessage(error, path, line, position, value ?? "null", additionalArgs), innerException)
        {
            _error = error;
            _path = path;
            _line = line;
            _position = position;
            _value = value;
        }

        public MalformedDocumentError Error
        {
            get { return _error; }
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

        private static string FormatMessage(MalformedDocumentError error, string path, int line, int position, object value, object[] additionalArgs)
        {
            string message;

            switch (error)
            {
                case MalformedDocumentError.ObjectMissingOpenCurlyBrace:
                    message = "Missing object open curly brace.";
                    break;
                case MalformedDocumentError.ObjectMissingCloseCurlyBrace:
                    message = "Missing object close curly brace.";
                    break;
                case MalformedDocumentError.PropertyNameMissingOpenQuote:
                    message = "Missing open quote for property name.";
                    break;
                case MalformedDocumentError.PropertyNameMissingCloseQuote:
                    message = "Missing close quote for property name.";
                    break;
                case MalformedDocumentError.PropertyInvalidName:
                    message = "Invalid property name.";
                    break;
                case MalformedDocumentError.PropertyMissingNameValueSeparator:
                    message = "Missing property name/value separator.";
                    break;
                case MalformedDocumentError.PropertyMissingItemSeparator:
                    message = "Missing property item separator.";
                    break;
                case MalformedDocumentError.StringMissingOpenQuote:
                    message = "Missing open quote for string value.";
                    break;
                case MalformedDocumentError.StringMissingCloseQuote:
                    message = "Missing close quote for string value.";
                    break;
                case MalformedDocumentError.StringInvalidValue:
                    Debug.Assert(additionalArgs.Length == 1);
                    message = string.Format("Invalid value for '{0}'.", additionalArgs[0]);
                    break;
                case MalformedDocumentError.LiteralInvalidValue:
                    Debug.Assert(additionalArgs.Length == 1);
                    message = string.Format("Invalid literal value, expected '{0}'.", additionalArgs[0]);
                    break;
                case MalformedDocumentError.BooleanInvalidValue:
                    message = "Invalid boolean value.";
                    break;
                case MalformedDocumentError.BooleanMissingValue:
                    message = "Missing boolean value.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("error");
            }

            if (value != null)
            {
                if (value is bool)
                {
                    value = value.ToString().ToLower();
                }

                return message + " Path: " + path + ", Value: " + value + ", Line: " + line + ", Position: " + position;
            }

            return message + " Path: " + path + ", Line: " + line + ", Position: " + position;
        }
    }
}