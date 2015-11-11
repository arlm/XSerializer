using System;

namespace XSerializer
{
    /// <summary>
    /// Represents an object that can parse string representations of date time values.
    /// </summary>
    public interface IDateTimeHandler
    {
        /// <summary>
        /// Parse the given string representation into a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">A string representation of a date time.</param>
        /// <returns>A <see cref="DateTime"/> value.</returns>
        DateTime ParseDateTime(string value);

        /// <summary>
        /// Parse the given string representation into a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">A string representation of a date time.</param>
        /// <returns>A <see cref="DateTimeOffset"/> value.</returns>
        DateTimeOffset ParseDateTimeOffset(string value);
    }
}