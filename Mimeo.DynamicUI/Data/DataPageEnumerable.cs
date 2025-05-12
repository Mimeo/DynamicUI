namespace Mimeo.DynamicUI.Data
{
    public class DataPageEnumerable<T> : PageEnumerable<T>
    {
        public DataPageEnumerable(DataQuery args, Func<DataQuery, Task<DataResponse<T>>> query) : base(CreateQuery(args, query))
        {
        }

        private static PageQuery<T> CreateQuery(DataQuery args, Func<DataQuery, Task<DataResponse<T>>> query)
        {
            return async continuationToken =>
            {
                var skip = continuationToken as int? ?? 0;
                var newArgs = new DataQuery
                {
                    Filters = args.Filters,
                    Sorts = args.Sorts,
                    Skip = skip,
                    Top = args.Top ?? 250
                };

                var results = await query(newArgs);
                if (results?.Count == 0 || results?.Value == null || !results.Value.Any())
                {
                    return null;
                }

                return new Page<T>(results.Value, skip + results.Value.Count());
            };
        }
    }
}
