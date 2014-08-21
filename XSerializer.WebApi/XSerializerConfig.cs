using System.Web.Http;

namespace XSerializer.WebApi
{
    public static class XSerializerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.Add(new XSerializerXmlMediaTypeFormatter());
        }
    }
}