using System;

namespace XSerializer
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property )]
    public class JsonConcreteImplementationAttribute : Attribute
    {
        private readonly Type _type;

        public JsonConcreteImplementationAttribute(Type type)
        {
            _type = type;
        }

        public Type Type
        {
            get { return _type; }
        }
    }
}