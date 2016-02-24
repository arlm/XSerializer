using System;

namespace XSerializer
{
    /// <summary>
    /// The default implementation of <see cref="IDateTimeHandler"/>. Expects values to be
    /// in the "yyyy-MM-ddTHH:mm:ss.fffffffK" format.
    /// </summary>
    public class DefaultDateTimeHandler : IDateTimeHandler
    {
        private const string _dateFormat = "yyyy-MM-ddTHH:mm:ss.fffffffK";

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the
        /// current <see cref="object"/>.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="object"/> to compare with the current <see cref="object"/>.
        /// </param>
        /// <returns>
        /// true if the specified <see cref="object"/> is equal to the current
        /// <see cref="object"/>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj.GetType() == typeof(DefaultDateTimeHandler);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (_dateFormat.GetHashCode() * 397) ^ typeof(DefaultDateTimeHandler).GetHashCode();
            }
        }

        /// <summary>
        /// Parse the given string representation into a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">A string representation of a date time.</param>
        /// <returns>A <see cref="DateTime"/> value.</returns>
        public DateTime ParseDateTime(string value)
        {
            int year, month, day, hour, minute, second, offsetHours, offsetMinutes;
            long ticks;
            DateTimeKind dateTimeKind;

            GetDateTimeComponents(value, out year, out month, out day, out hour, out minute, out second, out ticks, out offsetHours, out offsetMinutes, out dateTimeKind);

            var toLocal = false;

            if (dateTimeKind == DateTimeKind.Local)
            {
                hour -= offsetHours;
                minute -= offsetMinutes;

                Adjust(ref year, ref month, ref day, ref hour, ref minute);

                dateTimeKind = DateTimeKind.Utc;
                toLocal = true;
            }

            var dateTime = new DateTime(year, month, day, hour, minute, second, dateTimeKind);
            dateTime = dateTime.AddTicks(ticks);

            if (toLocal)
            {
                dateTime = dateTime.ToLocalTime();
            }

            return dateTime;
        }

        /// <summary>
        /// Parse the given string representation into a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">A string representation of a date time.</param>
        /// <returns>A <see cref="DateTimeOffset"/> value.</returns>
        public DateTimeOffset ParseDateTimeOffset(string value)
        {
            int year, month, day, hour, minute, second, offsetHours, offsetMinutes;
            long ticks;
            DateTimeKind dateTimeKind;

            GetDateTimeComponents(value, out year, out month, out day, out hour, out minute, out second, out ticks, out offsetHours, out offsetMinutes, out dateTimeKind);

            TimeSpan utcOffset;

            switch (dateTimeKind)
            {
                case DateTimeKind.Unspecified:
                    utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                    break;
                case DateTimeKind.Utc:
                case DateTimeKind.Local:
                    utcOffset = new TimeSpan(offsetHours, offsetMinutes, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            
            var dateTimeOffset = new DateTimeOffset(year, month, day, hour, minute, second, utcOffset);
            dateTimeOffset = dateTimeOffset.AddTicks(ticks);

            return dateTimeOffset;
        }

        private static void GetDateTimeComponents(
            string value,
            out int year,
            out int month,
            out int day,
            out int hour,
            out int minute,
            out int second,
            out long ticks,
            out int offsetHours,
            out int offsetMinutes,
            out DateTimeKind dateTimeKind)
        {
            year = int.MinValue;
            month = int.MinValue;
            day = int.MinValue;
            hour = int.MinValue;
            minute = int.MinValue;
            second = int.MinValue;
            offsetHours = 0;
            offsetMinutes = 0;
            dateTimeKind = DateTimeKind.Unspecified;

            double? fraction = null;

            for (int valueIndex = 0, formatIndex = 0; formatIndex < _dateFormat.Length && valueIndex < value.Length; valueIndex++, formatIndex++)
            {
                char c = value[valueIndex];

                switch (_dateFormat[formatIndex])
                {
                    case 'y':
                        ThrowIfNotDigit(c, valueIndex, value, "year");
                        year = year * 10 + (c - '0');
                        break;
                    case 'M':
                        ThrowIfNotDigit(c, valueIndex, value, "month");
                        month = month * 10 + (c - '0');
                        break;
                    case 'd':
                        ThrowIfNotDigit(c, valueIndex, value, "day");
                        day = day * 10 + (c - '0');
                        break;
                    case 'H':
                        ThrowIfNotDigit(c, valueIndex, value, "hours");
                        hour = hour * 10 + (c - '0');
                        break;
                    case 'm':
                        ThrowIfNotDigit(c, valueIndex, value, "minutes");
                        minute = minute * 10 + (c - '0');
                        break;
                    case 's':
                        ThrowIfNotDigit(c, valueIndex, value, "seconds");
                        second = second * 10 + (c - '0');
                        break;
                    case '.':
                        if (value[valueIndex] == '.')
                        {
                            fraction = ParseFraction(value, ref valueIndex, ref formatIndex);
                        }
                        else
                        {
                            valueIndex--;
                            formatIndex++;
                            AdvancePastFractionalPart(ref formatIndex);
                        }
                        break;
                    case 'K':
                        ParseOffset(value, valueIndex, out offsetHours, out offsetMinutes, out dateTimeKind);
                        break;
                    case '-':
                    case 'T':
                    case ':':
                        if (_dateFormat[valueIndex] != c)
                        {
                            throw new FormatException(
                                string.Format("Expected '{0}' but was '{1}' at index {2} in '{3}'.",
                                _dateFormat[valueIndex], c, valueIndex, value));
                        }
                        break;
                }
            }

            if (year == int.MinValue)
            {
                throw new FormatException(string.Format("Missing year in date time '{0}'.", value));
            }

            if (month == int.MinValue)
            {
                throw new FormatException(string.Format("Missing month in date time '{0}'.", value));
            }

            if (day == int.MinValue)
            {
                throw new FormatException(string.Format("Missing day in date time '{0}'.", value));
            }

            if (hour == int.MinValue)
            {
                throw new FormatException(string.Format("Missing hour in date time '{0}'.", value));
            }

            if (minute == int.MinValue)
            {
                throw new FormatException(string.Format("Missing minute in date time '{0}'.", value));
            }

            if (second == int.MinValue)
            {
                throw new FormatException(string.Format("Missing second in date time '{0}'.", value));
            }

            ticks = fraction == null ? 0 : (long)(fraction * TimeSpan.TicksPerSecond);
        }

        private static double ParseFraction(string sourceString, ref int valueIndex, ref int formatIndex)
        {
            int numerator = 0;
            int denominator = 1;

            for (valueIndex = valueIndex + 1, formatIndex = formatIndex + 1; formatIndex < _dateFormat.Length && valueIndex < sourceString.Length; valueIndex++, formatIndex++)
            {
                char c = sourceString[valueIndex];

                if (_dateFormat[formatIndex] == 'f')
                {
                    if (!char.IsDigit(c))
                    {
                        valueIndex--;
                        break;
                    }

                    numerator = numerator * 10 + (c - '0');
                    denominator *= 10;
                }
                else
                {
                    valueIndex--;
                    break;
                }
            }

            if (denominator == 1)
            {
                throw new FormatException(string.Format(
                    "Missing decimal digits after '.' in {0}.", sourceString));
            }

            AdvancePastFractionalPart(ref formatIndex);

            var fraction = numerator / (double)denominator;
            return fraction;
        }

        private static void AdvancePastFractionalPart(ref int formatIndex)
        {
            while (_dateFormat[formatIndex] == 'f')
            {
                formatIndex++;
            }

            formatIndex--;
        }

        private static void ParseOffset(
            string sourceString,
            int i,
            out int offsetHours,
            out int offsetMinutes,
            out DateTimeKind dateTimeKind)
        {
            if (i >= sourceString.Length)
            {
                offsetHours = 0;
                offsetMinutes = 0;
                dateTimeKind = DateTimeKind.Unspecified;
                return;
            }

            int negate;
            int hours = 0;
            int minutes = 0;

            if (sourceString[i] == '+')
            {
                negate = 1;
            }
            else if (sourceString[i] == '-')
            {
                negate = -1;
            }
            else if (sourceString[i] == 'Z')
            {
                offsetHours = 0;
                offsetMinutes = 0;
                dateTimeKind = DateTimeKind.Utc;
                return;
            }
            else
            {
                throw new FormatException(
                    string.Format("Expected start of timezone but was '{0}' at index {1} in '{2}'.",
                    sourceString[i], i, sourceString));
            }

            dateTimeKind = DateTimeKind.Local;

            if (sourceString.Length < i + 5
                || !char.IsDigit(sourceString[i + 1])
                || !char.IsDigit(sourceString[i + 2])
                || sourceString[i + 3] != ':'
                || !char.IsDigit(sourceString[i + 4])
                || !char.IsDigit(sourceString[i + 5]))
            {
                throw new FormatException(
                    string.Format("Expected timezone offset but was '{0}' at index {1} in '{2}'.",
                    sourceString.Substring(i), i, sourceString));
            }

            hours = hours * 10 + (sourceString[i + 1] - '0');
            hours = hours * 10 + (sourceString[i + 2] - '0');
            minutes = minutes * 10 + (sourceString[i + 4] - '0');
            minutes = minutes * 10 + (sourceString[i + 5] - '0');

            offsetHours = negate * hours;
            offsetMinutes = negate * minutes;
        }

        private static void Adjust(ref int year, ref int month, ref int day, ref int hour, ref int minute)
        {
            if (minute < 0)
            {
                minute += 60;
                hour--;
            }
            else if (minute > 60)
            {
                minute -= 60;
                hour++;
            }

            if (hour < 0)
            {
                hour += 24;
                day--;
            }
            else if (hour > 23)
            {
                hour -= 24;
                day++;
            }

            if (day < 1)
            {
                month--;
            }
            else if (OverflowsMonth(day, month, year))
            {
                day = 32;
                month++;
            }

            if (month < 1)
            {
                month += 12;
                year--;
            }
            else if (month > 12)
            {
                month -= 12;
                year++;
            }

            if (day < 1)
            {
                day = DateTime.DaysInMonth(year, month);
            }
            else if (day > 31)
            {
                day = 1;
            }
        }

        private static bool OverflowsMonth(int day, int month, int year)
        {
            switch (month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    return day > 31;
                case 2:
                    if (day < 29)
                    {
                        return false;
                    }

                    if (day > 29)
                    {
                        return true;
                    }

                    return !DateTime.IsLeapYear(year);
                case 4:
                case 6:
                case 9:
                case 11:
                    return day > 30;
                default:
                    throw new ArgumentOutOfRangeException("month");
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private static void ThrowIfNotDigit(char c, int i, string sourceString, string dateTimePart)
        {
            if (!char.IsDigit(c))
            {
                throw new FormatException(
                    string.Format("Expected numeric {0} but was '{1}' at index {2} in '{3}'.",
                    dateTimePart, c, i, sourceString));
            }
        }
    }
}