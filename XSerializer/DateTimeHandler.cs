namespace XSerializer
{
    public static class DateTimeHandler
    {
        private static readonly IDateTimeHandler _default = new DefaultDateTimeHandler();

        public static IDateTimeHandler Default
        {
            get { return _default; }
        }
    }
}