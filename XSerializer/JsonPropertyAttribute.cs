using System;

namespace XSerializer
{
    public class JsonPropertyAttribute : Attribute
    {
        private readonly string _name;

        public JsonPropertyAttribute(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}