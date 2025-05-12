using FluentAssertions;
using Mimeo.DynamicUI.Data;

namespace Mimeo.DynamicUI.Tests.OData
{
    public class ODataPageEnumerable
    {
        [Fact]
        public async Task CanEnumerateMultiplePages()
        {
            // Arrange
            var dataService = new DataService();
            var loadDataArgs = new DataQuery();
            IAsyncEnumerable<int> enumerable = new DataPageEnumerable<int>(loadDataArgs, args => dataService.Query(args));

            // Act
            var results = await enumerable.ToListAsync();

            // Assert
            results.Should().BeEquivalentTo(DataService.Data);
        }

        private class DataService
        {
            static DataService()
            {
                Data = new List<int>();
                for (int i = 0; i < 100; i++)
                {
                    Data.Add(i);
                }
            }

            public static readonly List<int> Data;

            public Task<DataResponse<int>> Query(DataQuery args)
            {
                var skip = args.Skip ?? 0;
                var data = Data.Skip(skip).Take(10);
                return Task.FromResult(new DataResponse<int>
                {
                    Value = data,
                    Count = data.Count()
                });
            }
        }
    }
}
