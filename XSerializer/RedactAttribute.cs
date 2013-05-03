using System;
using System.Text.RegularExpressions;

namespace XSerializer
{
    public class RedactAttribute : Attribute
    {
        private static readonly Regex Numbers = new Regex(@"\d", RegexOptions.Compiled);
        private static readonly Regex Letters = new Regex(@"[a-zA-Z]", RegexOptions.Compiled);

        private readonly Regex _redactRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactAttribute"/> class.
        /// </summary>
        public RedactAttribute()
            : this(".*")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactAttribute"/> class. 
        /// </summary>
        /// <param name="redactPattern">
        /// </param>
        public RedactAttribute(string redactPattern)
        {
            _redactRegex = new Regex(redactPattern, RegexOptions.Singleline | RegexOptions.Compiled);
        }

        /// <summary>
        /// Redacts the clear-text.
        /// </summary>
        /// <param name="clearText">Some clear-text.</param>
        /// <returns>The redacted text.</returns>
        public string Redact(string clearText)
        {
            return _redactRegex.Replace(clearText, match => Letters.Replace(Numbers.Replace(match.Value, "#"), "X"));
        }
    }
}