namespace XSerializer.Tests
{
    public static class StripXsiXsdExtension
    {
        public static string StripXsiXsdDeclarations(this string xml)
        {
            return
                xml.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "")
                   .Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
        }
    }
}