namespace XSerializer
{
    using System;
    using System.Text.RegularExpressions;

    public class RedactAttribute : Attribute
    {
        private static readonly Regex Numbers = new Regex(@"\d", RegexOptions.Compiled);
        private static readonly Regex AllNumbers = new Regex(@"[-.0-9]", RegexOptions.Compiled);
        private static readonly Regex Letters = new Regex(@"[a-zA-Z]", RegexOptions.Compiled);

        /// <summary>
        /// Redacts the clear-text.
        /// </summary>
        /// <param name="clearText">Some clear-text.</param>
        /// <param name="redactEnabled">Whether redaction is currently enabled.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(string clearText, bool redactEnabled)
        {
            if (clearText == null)
            {
                return null;
            }

            return redactEnabled
                ? Letters.Replace(Numbers.Replace(clearText, "1"), "X")
                : clearText;
        }

        /// <summary>
        /// Redacts the string representation of <paramref name="booleanValue"/>.
        /// </summary>
        /// <param name="booleanValue">A <see cref="bool"/>.</param>
        /// <param name="redactEnabled">Whether redaction is currently enabled.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(bool? booleanValue, bool redactEnabled)
        {
            if (booleanValue == null)
            {
                return null;
            }

            return redactEnabled
                ? "XXXXXX"
                : booleanValue.Value.ToString().ToLower();
        }

        /// <summary>
        /// Redacts the string representation of <paramref name="enumValue"/>.
        /// </summary>
        /// <param name="enumValue">An <see cref="Enum"/>.</param>
        /// <param name="redactEnabled">Whether redaction is currently enabled.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(Enum enumValue, bool redactEnabled)
        {
            if (enumValue == null)
            {
                return null;
            }

            return redactEnabled
                ? "XXXXXX"
                : enumValue.ToString();
        }

        /// <summary>
        /// Redacts the string representation of <paramref name="dateTimeValue"/>.
        /// </summary>
        /// <param name="dateTimeValue">A <see cref="DateTime"/>.</param>
        /// <param name="redactEnabled">Whether redaction is currently enabled.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(DateTime? dateTimeValue, bool redactEnabled)
        {
            if (dateTimeValue == null)
            {
                return null;
            }

            return redactEnabled
                ? Numbers.Replace(dateTimeValue.Value.ToString("O"), "1")
                : dateTimeValue.Value.ToString("O");
        }

        /// <summary>
        /// Redacts the string representation of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">An object.</param>
        /// <param name="redactEnabled">Whether redaction is currently enabled.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(object value, bool redactEnabled)
        {
            if (value == null)
            {
                return null;
            }

            return redactEnabled
                ? Letters.Replace(AllNumbers.Replace(value.ToString(), "1"), "X")
                : value.ToString();
        }
    }
}