using System;

namespace XSerializer
{
    /// <summary>
    /// Defines a mapping that instructs the <see cref="JsonSerializer{T}"/> to use the value of the
    /// <see cref="Type"/> property in place of type of the decorated member during deserialization
    /// operations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property )]
    public class JsonConcreteImplementationAttribute : Attribute
    {
        private readonly Type _type;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConcreteImplementationAttribute"/> class.
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> to use in place of the type of the decorated member.</param>
        public JsonConcreteImplementationAttribute(Type type)
        {
            _type = type;
        }

        /// <summary>
        /// Gets the <see cref="System.Type"/> to use in place of the type of the decorated member.
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }
    }
}