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
        ArrayMissingValue,
        ArrayMissingOpenSquareBracket,
        ArrayMissingCommaOrCloseSquareBracket,
        StringMissingOpenQuote,
        StringMissingCloseQuote,
        StringInvalidValue,
        LiteralInvalidValue,
        BooleanMissingValue,
        BooleanInvalidValue,
        NumberMissingValue,
        NumberInvalidValue,
    }
}