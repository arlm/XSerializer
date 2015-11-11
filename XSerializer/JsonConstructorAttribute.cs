using System;

namespace XSerializer
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer{T}"/> to use the decorated constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class JsonConstructorAttribute : Attribute
    {
    }
}