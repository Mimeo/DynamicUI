namespace Mimeo.DynamicUI.Demo.Shared.Models
{
    public class PagedResultsModel<T>
    {
        public PagedResultsModel()
        {
        }

        public PagedResultsModel(int? totalCount, int skip, int take, IEnumerable<T> data)
        {
            this.Skip = skip;
            this.Take = take;
            this.Pages = totalCount.HasValue
                ? ((take > 0) ? (int)Math.Ceiling((double)totalCount / take) : 0)
                : null;
            this.PageNumber = (take > 0 && skip > 0) ? (int)Math.Ceiling((double)(skip + take) / take) : 1;
            this.Data = data ?? throw new ArgumentNullException(nameof(data));
            this.TotalCount = totalCount;
        }

        public int? TotalCount { get; set; }

        public int? Pages { get; set; }

        public int PageNumber { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }

        public IEnumerable<T> Data { get; set; } = default!;
    }
}
