using CsvHelper;
using System.Text.Json;

namespace Mimeo.DynamicUI.Demo.Shared.Models
{
    /// <summary>
    /// Stores a value that has no fixed number of CSV columns, such as a list, as JSON in a single field.
    /// An empty field reads as <c>null</c>.
    /// </summary>
    public sealed class JsonCsvConverter<T> : ICsvConverter<T>
    {
        public T Read(CsvReader reader, int index)
        {
            var field = reader.GetString(index);
            return string.IsNullOrEmpty(field) ? default! : JsonSerializer.Deserialize<T>(field)!;
        }

        public void Write(CsvWriter writer, T value)
        {
            writer.WriteField(value is null ? string.Empty : JsonSerializer.Serialize(value));
        }
    }
}
