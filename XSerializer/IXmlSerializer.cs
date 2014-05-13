using System.IO;

namespace XSerializer
{
    public interface IXmlSerializer
    {
        string Serialize(object instance);
        void Serialize(Stream stream, object instance);
        void Serialize(TextWriter writer, object instance);
        object Deserialize(string xml);
        object Deserialize(Stream stream);
        object Deserialize(TextReader reader);
    }
}