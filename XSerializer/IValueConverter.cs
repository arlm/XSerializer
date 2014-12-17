namespace XSerializer
{
    internal interface IValueConverter
    {
        object ParseString(string value, ISerializeOptions options);
        string GetString(object value, ISerializeOptions options);
    }
}