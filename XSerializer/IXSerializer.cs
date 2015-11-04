using System;
using System.IO;

namespace XSerializer
{
    public interface IXSerializer
    {
        string Serialize(object instance);
        void Serialize(Stream stream, object instance);
        void Serialize(TextWriter textWriter, object instance);
        object Deserialize(string data);
        object Deserialize(Stream stream);
        object Deserialize(TextReader textReader);
    }

    [Obsolete]
    public interface IXmlSerializer : IXSerializer
    {
    }
}