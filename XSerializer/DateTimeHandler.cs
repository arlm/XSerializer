namespace XSerializer
{
    /// <summary>
    /// Provides access to a default instance of <see cref="IDateTimeHandler"/>.
    /// </summary>
    public static class DateTimeHandler
    {
        private static readonly IDateTimeHandler _default = new DefaultDateTimeHandler();

        /// <summary>
        /// Gets the default implementation of <see cref="IDateTimeHandler"/>.
        /// </summary>
        public static IDateTimeHandler Default
        {
            get { return _default; }
        }
    }
}