namespace XSerializer
{
    internal interface IValueConverter
    {
        object ParseString(string value);
        string GetString(object value, ISerializeOptions options);
    }
}