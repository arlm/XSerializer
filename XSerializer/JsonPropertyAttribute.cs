using System;

namespace XSerializer
{
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonPropertyAttribute : Attribute
    {
        private readonly string _name;

        public JsonPropertyAttribute()
        {
        }

        public JsonPropertyAttribute(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }

    internal static class JsonPropertyAttributeExtensions
    {
        public static string GetNameOrDefaultTo(this JsonPropertyAttribute jsonPropertyAttribute, string fallbackName)
        {
            return
                jsonPropertyAttribute == null
                    ? fallbackName
                    : string.IsNullOrEmpty(jsonPropertyAttribute.Name)
                        ? fallbackName
                        : jsonPropertyAttribute.Name;
        }
    }
}