using System;
using System.IO;

namespace XSerializer
{
    public static class JsonSerializer
    {
        public static IXSerializer Create(Type type)
        {
            return null;
        }
    }

    public class JsonSerializer<T> : IXSerializer
    {
        string IXSerializer.Serialize(object instance)
        {
            throw new NotImplementedException();
        }

        public string Serialize(T instance)
        {
            throw new NotImplementedException();
        }

        void IXSerializer.Serialize(Stream stream, object instance)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Stream stream, T instance)
        {
            throw new NotImplementedException();
        }

        void IXSerializer.Serialize(TextWriter writer, object instance)
        {
            throw new NotImplementedException();
        }

        public void Serialize(TextWriter writer, T instance)
        {
            throw new NotImplementedException();
        }

        object IXSerializer.Deserialize(string json)
        {
            throw new NotImplementedException();
        }

        public T Deserialize(string xml)
        {
            throw new NotImplementedException();
        }

        object IXSerializer.Deserialize(Stream stream)
        {
            throw new NotImplementedException();
        }

        public T Deserialize(Stream stream)
        {
            throw new NotImplementedException();
        }

        object IXSerializer.Deserialize(TextReader reader)
        {
            throw new NotImplementedException();
        }

        public T Deserialize(TextReader reader)
        {
            throw new NotImplementedException();
        }
    }
}