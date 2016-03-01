namespace XSerializer
{
    public enum MalformedDocumentError
    {
        ObjectMissingOpenCurlyBrace,
        ObjectMissingCloseCurlyBrace,
        PropertyNameMissingOpenQuote,
        PropertyNameMissingCloseQuote,
        PropertyInvalidName,
        PropertyMissingNameValueSeparator,
        PropertyMissingItemSeparator,
        StringMissingOpenQuote,
        StringMissingCloseQuote,
        StringInvalidValue,
        LiteralInvalidValue,
        BooleanMissingValue,
        BooleanInvalidValue,
    }
}