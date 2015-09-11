using System;

namespace XSerializer
{
    public class XSerializerException : Exception
    {
        public XSerializerException(string message)
            : base(message)
        {
        }

        public XSerializerException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}