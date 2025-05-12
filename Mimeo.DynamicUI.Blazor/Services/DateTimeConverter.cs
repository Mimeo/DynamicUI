using NodaTime;

namespace Mimeo.DynamicUI.Blazor.Services
{   
    public class DateTimeConverter : IDateTimeConverter
    {
        public DateTime UtcToDisplay(DateTime utc, DateDisplayMode mode)
        {
            var timeZone = GetTimeZone(mode);
            if (timeZone == null)
            {
                return utc;
            }

            var utcTime = new LocalDateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second, utc.Millisecond);
            return utcTime.InUtc().ToInstant().InZone(timeZone).ToDateTimeUnspecified();
        }

        public DateTime DisplayToUtc(DateTime display, DateDisplayMode mode)
        {
            var timeZone = GetTimeZone(mode);
            if (timeZone == null)
            {
                return display;
            }

            var localTime = new LocalDateTime(display.Year, display.Month, display.Day, display.Hour, display.Minute, display.Second, display.Millisecond);
            return localTime.InZoneLeniently(timeZone).ToDateTimeUtc();
        }

        protected DateTimeZone? GetTimeZone(DateDisplayMode mode)
        {
            switch (mode)
            {
                case DateDisplayMode.Raw:
                case DateDisplayMode.Utc:
                    return null;
                case DateDisplayMode.UserLocal:
                    return GetUserTimeZone();
                case DateDisplayMode.ServerLocal:
                    return GetServerTimeZone();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        protected virtual DateTimeZone? GetUserTimeZone()
        {
            return DateTimeZoneProviders.Tzdb.GetSystemDefault();
        }

        protected virtual DateTimeZone? GetServerTimeZone()
        {
            // Being designed as a client-side application, we don't know the server's time zone.
            return null;
        }
    }
}
