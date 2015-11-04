using System;

namespace XSerializer
{
    public interface IDateTimeHandler
    {
        DateTime ParseDateTime(string value);
        DateTimeOffset ParseDateTimeOffset(string value);
    }
}