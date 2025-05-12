namespace Mimeo.DynamicUI.Extensions
{
    public static class AsyncLinqExtensions
    {
        public static async IAsyncEnumerable<TTarget> CastAsync<TSource, TTarget>(this IAsyncEnumerable<TSource> source)
        {
            await foreach (var item in source)
            {
                yield return (TTarget)(object)item!;
            }
        }

        public static async Task<int> CountAsync<TSource>(this IAsyncEnumerable<TSource> source) 
        {
            int count = 0;
            await foreach (var item in source)
            {
                count++;
            }
            return count;
        }

        // Based on https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/Batch.cs
        public static IAsyncEnumerable<TSource[]> BatchAsync<TSource>(this IAsyncEnumerable<TSource> source, int size)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            return _(); async IAsyncEnumerable<TSource[]> _()
            {
                TSource[]? bucket = null;
                var count = 0;

                await foreach (var item in source)
                {
                    bucket ??= new TSource[size];
                    bucket[count++] = item;

                    // The bucket is fully buffered before it's yielded
                    if (count != size)
                        continue;

                    yield return bucket;

                    bucket = null;
                    count = 0;
                }

                // Return the last bucket with all remaining elements
                if (count > 0)
                {
                    Array.Resize(ref bucket, count);
                    yield return bucket;
                }
            }
        }
    }
}
