using System;

namespace XSerializer
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer{T}"/> not to serialize the decorated property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonIgnoreAttribute : Attribute
    {
    }
}