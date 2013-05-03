using System;
using System.Text.RegularExpressions;

namespace XSerializer
{
    public class RedactAttribute : Attribute
    {
        private static readonly Regex Numbers = new Regex(@"\d", RegexOptions.Compiled);
        private static readonly Regex Letters = new Regex(@"[a-zA-Z]", RegexOptions.Compiled);

        /// <summary>
        /// Redacts the clear-text.
        /// </summary>
        /// <param name="clearText">Some clear-text.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(string clearText)
        {
            return Letters.Replace(Numbers.Replace(clearText, "#"), "X");
        }
    }
}