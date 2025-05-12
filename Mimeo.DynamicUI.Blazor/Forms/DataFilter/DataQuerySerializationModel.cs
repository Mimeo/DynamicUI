using Mimeo.DynamicUI.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Mimeo.DynamicUI.Blazor.Forms.DataFilter
{
    /// <summary>
    /// A model of <see cref="DataQuery"/> that is used for serialization and deserialization
    /// </summary>
    public class DataQuerySerializationModel
    {
        public static string Serialize(DataQuery query)
        {
            var model = new DataQuerySerializationModel(query);
            var json = JsonSerializer.Serialize(model);

            // Compress data to save space
            // JSON is rather large for a URL, which has a maximum length that can vary by client and server
            using var memoryStream = new MemoryStream();
            using (var zlibStream = new ZLibStream(memoryStream, CompressionLevel.Fastest))
            {
                zlibStream.Write(Encoding.UTF8.GetBytes(json).AsSpan());
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static DataQuerySerializationModel? Deserialize(string serialized)
        {
            var compressedBytes = Convert.FromBase64String(serialized);
            using var memoryStream = new MemoryStream(compressedBytes);
            using var zlibStream = new ZLibStream(memoryStream, CompressionMode.Decompress);
            using var reader = new StreamReader(zlibStream, Encoding.UTF8);
            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<DataQuerySerializationModel>(json);
        }

        public DataQuerySerializationModel()
        {
        }

        public DataQuerySerializationModel(DataQuery query)
        {
            Skip = query.Skip;
            Top = query.Top;
            SearchText = query.SearchText;
            Filter = DataQueryFilterGroupSerializationModel.FromFilterGroup(query.Filter);
            Sorts = query.Sorts.Select(s => new DataQuerySortSerializationModel
            {
                ODataPath = s.Field.ODataPath,
                Descending = s.Descending
            }).ToList();
        }

        public DataQuery ToODataQuery(FilterViewModel filterViewModel)
        {
            return new DataQuery
            {
                Skip = Skip,
                Top = Top,
                SearchText = SearchText,
                Filter = Filter.ToFilterGroup(filterViewModel),
                Sorts = Sorts.Select(s =>
                {
                    var field = filterViewModel.AvailableSearchFields.First(a => a.ODataPath == s.ODataPath);
                    return new DataQuerySort(field, s.Descending);
                }).ToList()
            };
        }

        public int? Skip { get; set; }
        public int? Top { get; set; }

        /// <summary>
        /// General search text that could be for any supported property
        /// </summary>
        public string? SearchText { get; set; }

        [Obsolete]
        public List<DataQueryFilterSerializationModel> Filters
        {
            get => Filter.Filters;
            set => Filter.Filters = value;
        }

        public DataQueryFilterGroupSerializationModel Filter { get; set; } = new();

        public List<DataQuerySortSerializationModel> Sorts { get; set; } = [];
    }
}
