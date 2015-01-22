using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace XSerializer.WebApi
{
    public class XSerializerXmlMediaTypeFormatter : XmlMediaTypeFormatter
    {
        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return ReadFromStreamAsync(type, readStream, content, formatterLogger, new CancellationToken(false));
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (readStream == null)
            {
                throw new ArgumentNullException("readStream");
            }

            var completion = new TaskCompletionSource<object>();

            if (cancellationToken.IsCancellationRequested)
            {
                completion.SetCanceled();
            }
            else
            {
                try
                {
                    var value = ReadValue(type, readStream, content, formatterLogger);
                    completion.SetResult(value);
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            }

            return completion.Task;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return WriteToStreamAsync(type, value, writeStream, content, transportContext, new CancellationToken(false));
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (writeStream == null)
            {
                throw new ArgumentNullException("writeStream");
            }

            CheckForIEnumerable(ref type, ref value);

            var completion = new TaskCompletionSource<bool>();

            if (cancellationToken.IsCancellationRequested)
            {
                completion.SetCanceled();
            }
            else
            {
                try
                {
                    WriteValue(type, value, writeStream, content);
                    completion.SetResult(true);
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            }

            return completion.Task;
        }

        private object ReadValue(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var contentHeaders = content == null ? null : content.Headers;

            if (contentHeaders != null && contentHeaders.ContentLength == 0)
            {
                return GetDefaultValueForType(type);
            }

            var encoding = SelectCharacterEncoding(contentHeaders);

            try
            {
                var serializer = XmlSerializer.Create(type, new XmlSerializationOptions(encoding: encoding, shouldIndent: Indent));
                return serializer.Deserialize(readStream);
            }
            catch (Exception ex)
            {
                if (formatterLogger == null)
                {
                    throw;
                }

                formatterLogger.LogError(string.Empty, ex);

                return GetDefaultValueForType(type);
            }
        }

        private static void CheckForIEnumerable(ref Type type, ref object value)
        {
            // TODO: optimize this
            if (IsIEnumerableOfT(type))
            {
                var argType = type.GetGenericArguments()[0];
                type = argType.MakeArrayType();
                value = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(argType).Invoke(null, new[] { value });
            }
        }

        private static bool IsIEnumerableOfT(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        private void WriteValue(Type type, object value, Stream writeStream, HttpContent content)
        {
            var encoding = SelectCharacterEncoding(content != null ? content.Headers : null);

            using (var writer = new StreamWriter(writeStream, encoding, 1024, true))
            {
                var serializer = XmlSerializer.Create(type, new XmlSerializationOptions(encoding: encoding, shouldIndent: Indent));
                serializer.Serialize(writer, value);
            }
        }
    }
}
