namespace Mimeo.DynamicUI
{
    public interface IDateTimeConverter
    {
        DateTime UtcToDisplay(DateTime utc, DateDisplayMode mode);
        DateTime DisplayToUtc(DateTime display, DateDisplayMode mode);
    }
}
