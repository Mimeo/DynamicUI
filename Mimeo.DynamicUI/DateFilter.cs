namespace Mimeo.DynamicUI
{
    public record DateFilter(DateFilterOption Option, int DaysToAdd = 0, DateTime? Date = null)
    {
        public DateTime? ToDateTime()
        {
            var now = DateTime.UtcNow;
            return Option switch
            {
                DateFilterOption.Today => now,
                DateFilterOption.Yesterday => now.AddDays(-1),
                DateFilterOption.Tomorrow => now.AddDays(1),
                DateFilterOption.SevenDaysAgo => now.AddDays(-7),
                DateFilterOption.SevenDaysFromNow => now.AddDays(7),
                DateFilterOption.ThirtyDaysAgo => now.AddDays(-30),
                DateFilterOption.ThirtyDaysFromNow => now.AddDays(30),
                DateFilterOption.XDaysFromNow => now.AddDays(DaysToAdd),
                DateFilterOption.Exact when Date.HasValue => Date.Value,
                DateFilterOption.Exact => null,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
