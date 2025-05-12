namespace Mimeo.DynamicUI.Data
{
    public class DataResponse<T>
    {
        public int Count { get; set; }
        public IEnumerable<T>? Value { get; set; }
    }
}
