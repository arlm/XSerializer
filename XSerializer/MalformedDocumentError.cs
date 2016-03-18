namespace XSerializer
{
    public enum MalformedDocumentError
    {
        ObjectMissingOpenCurlyBrace,
        ObjectMissingCloseCurlyBrace,
        PropertyNameMissing,
        PropertyNameMissingOpenQuote,
        PropertyNameMissingCloseQuote,
        PropertyInvalidName,
        PropertyMissingNameValueSeparator,
        PropertyMissingItemSeparator,
        ArrayMissingOpenSquareBracket,
        ArrayMissingCommaOrCloseSquareBracket,
        StringMissingOpenQuote,
        StringMissingCloseQuote,
        StringInvalidValue,
        LiteralInvalidValue,
        BooleanInvalidValue,
        NumberInvalidValue,
        MissingValue,
        InvalidValue,
        ExpectedEndOfString,
        ExpectedEndOfDecryptedString,
    }
}