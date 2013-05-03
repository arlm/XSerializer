using System;

namespace XSerializer.Tests
{
    public class TestOptions : IOptions
    {
        public static TestOptions Empty = new TestOptions();

        public static TestOptions WithExtraTypes(params Type[] extraTypes)
        {
            return new TestOptions { ExtraTypes = extraTypes };
        }

        public string DefaultNamespace { get; set; }
        public Type[] ExtraTypes { get; set; }
        public string RootElementName { get; set; }
    }
}