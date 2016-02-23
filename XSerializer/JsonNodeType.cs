namespace XSerializer
{
    internal enum JsonNodeType
    {
        None,
        String,
        Number,
        Boolean,
        Null,
        OpenObject,
        CloseObject,
        NameValueSeparator,
        ItemSeparator,
        OpenArray,
        CloseArray,
        Whitespace,
        Invalid
    }
}