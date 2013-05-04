namespace XSerializer
{
    using System;
    using System.Text.RegularExpressions;

    public class RedactAttribute : Attribute
    {
        private static readonly Regex Numbers = new Regex(@"\d", RegexOptions.Compiled);
        private static readonly Regex AllNumbers = new Regex(@"[-0-9]", RegexOptions.Compiled);
        private static readonly Regex Letters = new Regex(@"[a-zA-Z]", RegexOptions.Compiled);

        /// <summary>
        /// Redacts the clear-text.
        /// </summary>
        /// <param name="clearText">Some clear-text.</param>
        /// <param name="redactEnabled">Whether redaction is currently enabled.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(string clearText, bool redactEnabled)
        {
            return redactEnabled
                ? Letters.Replace(Numbers.Replace(clearText, "#"), "X")
                : clearText;
        }

        /// <summary>
        /// Redacts the string representation of <paramref name="booleanValue"/>.
        /// </summary>
        /// <param name="booleanValue">A <see cref="bool"/>.</param>
        /// <param name="redactEnabled">Whether redaction is currently enabled.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(bool booleanValue, bool redactEnabled)
        {
            return redactEnabled
                ? "XXXXXX"
                : booleanValue.ToString().ToLower();
        }

        /// <summary>
        /// Redacts the string representation of <paramref name="enumValue"/>.
        /// </summary>
        /// <param name="enumValue">An <see cref="Enum"/>.</param>
        /// <param name="redactEnabled">Whether redaction is currently enabled.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(Enum enumValue, bool redactEnabled)
        {
            return redactEnabled
                ? "XXXXXX"
                : enumValue.ToString();
        }

        /// <summary>
        /// Redacts the string representation of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">An object.</param>
        /// <param name="redactEnabled">Whether redaction is currently enabled.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(object value, bool redactEnabled)
        {
            return redactEnabled
                ? Letters.Replace(AllNumbers.Replace(value.ToString(), "#"), "X")
                : value.ToString();
        }
    }
}