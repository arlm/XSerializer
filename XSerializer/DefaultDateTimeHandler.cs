using System;

namespace XSerializer
{
    public class DefaultDateTimeHandler : IDateTimeHandler
    {
        private const string _dateFormat = "yyyy-MM-ddTHH:mm:ss.fffffffK";

        public override bool Equals(object obj)
        {
            return obj != null && obj.GetType() == typeof(DefaultDateTimeHandler);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_dateFormat.GetHashCode() * 397) ^ typeof(DefaultDateTimeHandler).GetHashCode();
            }
        }

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

        public DateTimeOffset ParseDateTimeOffset(string value)
        {
            int year, month, day, hour, minute, second, offsetHours, offsetMinutes;
            long ticks;
            DateTimeKind dateTimeKind;

            GetDateTimeComponents(value, out year, out month, out day, out hour, out minute, out second, out ticks, out offsetHours, out offsetMinutes, out dateTimeKind);

            var offset = new TimeSpan(offsetHours, offsetMinutes, 0);
            var dateTimeOffset = new DateTimeOffset(year, month, day, hour, minute, second, offset);
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
            year = 0;
            month = 0;
            day = 0;
            hour = 0;
            minute = 0;
            second = 0;
            offsetHours = 0;
            offsetMinutes = 0;
            dateTimeKind = DateTimeKind.Unspecified;

            double fraction = 0;

            for (int i = 0; i < _dateFormat.Length && i < value.Length; i++)
            {
                char c = value[i];

                switch (_dateFormat[i])
                {
                    case 'y':
                        ThrowIfNotDigit(c);
                        year = year * 10 + (c - '0');
                        break;
                    case 'M':
                        ThrowIfNotDigit(c);
                        month = month * 10 + (c - '0');
                        break;
                    case 'd':
                        ThrowIfNotDigit(c);
                        day = day * 10 + (c - '0');
                        break;
                    case 'H':
                        ThrowIfNotDigit(c);
                        hour = hour * 10 + (c - '0');
                        break;
                    case 'm':
                        ThrowIfNotDigit(c);
                        minute = minute * 10 + (c - '0');
                        break;
                    case 's':
                        ThrowIfNotDigit(c);
                        second = second * 10 + (c - '0');
                        break;
                    case '.':
                        fraction = ParseFraction(value, ref i);
                        break;
                    case 'K':
                        ParseOffset(value, i, out offsetHours, out offsetMinutes, out dateTimeKind);
                        break;
                    case '-':
                    case 'T':
                    case ':':
                        if (value[i] != c)
                        {
                            throw new Exception();
                        }
                        break;
                }
            }

            ticks = (long)(fraction * TimeSpan.TicksPerSecond);
        }

        private static double ParseFraction(string sourceString, ref int i)
        {
            int numerator = 0;
            int denominator = 1;

            for (i = i + 1; i < _dateFormat.Length && i < sourceString.Length; i++)
            {
                char c = sourceString[i];

                if (_dateFormat[i] == 'f')
                {
                    ThrowIfNotDigit(c);
                    numerator = numerator * 10 + (c - '0');
                    denominator *= 10;
                }
                else
                {
                    i--;
                    break;
                }
            }

            var fraction = numerator / (double)denominator;
            return fraction;
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
                throw new Exception();
            }

            dateTimeKind = DateTimeKind.Local;

            ThrowIfNotDigit(sourceString[i + 1]);
            ThrowIfNotDigit(sourceString[i + 2]);
            hours = hours * 10 + (sourceString[i + 1] - '0');
            hours = hours * 10 + (sourceString[i + 2] - '0');

            if (sourceString[i + 3] != ':')
            {
                throw new Exception();
            }

            ThrowIfNotDigit(sourceString[i + 4]);
            ThrowIfNotDigit(sourceString[i + 5]);
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
        private static void ThrowIfNotDigit(char c)
        {
            if (!char.IsDigit(c))
            {
                throw new Exception();
            }
        }
    }
}