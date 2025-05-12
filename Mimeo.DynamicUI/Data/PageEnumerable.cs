namespace Mimeo.DynamicUI.Data
{
    public delegate Task<Page<T>?> PageQuery<T>(object? continuationToken);

    public class PageEnumerable<TEntity> : IAsyncEnumerable<TEntity>
    {
        public PageEnumerable(PageQuery<TEntity> query)
        {
            this.query = query ?? throw new ArgumentNullException(nameof(query));
        }

        private readonly PageQuery<TEntity> query;

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new PageEnumerator<TEntity>(query);
        }
    }

    public class PageEnumerator<TEntity> : IAsyncEnumerator<TEntity>
    {
        public PageEnumerator(PageQuery<TEntity> query)
        {
            this.query = query ?? throw new ArgumentNullException(nameof(query));
        }

        private readonly PageQuery<TEntity> query;

        public TEntity Current => _current ?? throw new InvalidOperationException($"{nameof(MoveNextAsync)} must be called before {nameof(Current)}");
        private TEntity? _current;

        private Page<TEntity>? currentSegment;
        private IEnumerator<TEntity>? currentSegmentEnumerator;
        private bool isLastSegment;

        public ValueTask DisposeAsync()
        {
            currentSegmentEnumerator?.Dispose();
            return default;
        }

        private async ValueTask LoadNextSegment()
        {
            if (isLastSegment)
            {
                throw new InvalidOperationException("Cannot move past the last segment");
            }

            currentSegment = await query(currentSegment?.ContinuationToken);
            currentSegmentEnumerator = currentSegment?.Data.GetEnumerator();
            isLastSegment = currentSegment?.ContinuationToken == null;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (currentSegmentEnumerator == null)
            {
                await LoadNextSegment();
                if (currentSegmentEnumerator == null)
                {
                    return false;
                }
            }

            if (!currentSegmentEnumerator.MoveNext())
            {
                if (isLastSegment)
                {
                    return false;
                }

                await LoadNextSegment();
                if (currentSegmentEnumerator == null || !currentSegmentEnumerator.MoveNext())
                {
                    return false;
                }
            }

            _current = currentSegmentEnumerator.Current;
            return true;
        }
    }

    public struct Page<T>
    {
        public Page(IEnumerable<T> data, object? continuationToken)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            ContinuationToken = continuationToken;
        }

        public IEnumerable<T> Data { get; }
        public object? ContinuationToken { get; }
    }
}
