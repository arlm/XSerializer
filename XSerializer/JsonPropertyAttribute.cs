using System;

namespace XSerializer
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer{T}"/> to serialize the decorated property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonPropertyAttribute : Attribute
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyAttribute"/> class.
        /// </summary>
        public JsonPropertyAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of json property.</param>
        public JsonPropertyAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets the name of the json property.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
    }
}