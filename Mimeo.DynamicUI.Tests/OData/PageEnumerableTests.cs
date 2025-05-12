using FluentAssertions;
using Mimeo.DynamicUI.Data;

namespace Mimeo.DynamicUI.Tests.OData
{
    public class PageEnumerableTests
    {
        [Fact]
        public async Task CanEnumerateMultiplePages()
        {
            // Arrange
            var dataService = new DataService();
            IAsyncEnumerable<int> enumerable = new PageEnumerable<int>(continuationToken => dataService.Query(continuationToken as ContinuationToken));

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

            public Task<Page<int>?> Query(ContinuationToken? continuationToken)
            {
                continuationToken ??= new ContinuationToken();
                if (continuationToken.Skip >= Data.Count)
                {
                    return Task.FromResult<Page<int>?>(null);
                }

                return Task.FromResult<Page<int>?>(new Page<int>(Data.Skip(continuationToken.Skip).Take(10), new ContinuationToken
                {
                    Skip = continuationToken.Skip + 10
                }));
            }
        }

        private class DataResponse
        {
            public DataResponse(List<int> value, ContinuationToken continuationToken)
            {
                Value = value;
                ContinuationToken = continuationToken;
            }

            public List<int> Value { get; set; }
            public ContinuationToken ContinuationToken { get; set; }
        }

        private class ContinuationToken
        {
            public int Skip { get; set; }
        }
    }
}
