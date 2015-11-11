using System;

namespace XSerializer
{
    /// <summary>
    /// The exception that is thrown by the classes in XSerializer.
    /// </summary>
    public class XSerializerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XSerializerException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public XSerializerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XSerializerException"/> class with a
        /// specified error message and a reference to the inner exception that is the cause 
        /// of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="exception">
        /// The exception that is the cause of the current exception, or a null reference if 
        /// no inner exception is specified.
        /// </param>
        public XSerializerException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}