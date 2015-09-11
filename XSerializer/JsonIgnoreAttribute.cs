using System;

namespace XSerializer
{
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonIgnoreAttribute : Attribute
    {
    }
}