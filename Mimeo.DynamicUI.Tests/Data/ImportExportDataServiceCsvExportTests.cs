using AwesomeAssertions;
using CsvHelper;
using Mimeo.DynamicUI.Data;
using System.Globalization;
using System.Text;

namespace Mimeo.DynamicUI.Tests.Data
{
    public class ImportExportDataServiceCsvExportTests
    {
        [Fact]
        public async Task ExportsCsvRecordModelWithHeaderAndRows()
        {
            // Arrange
            IImportExportDataService service = new TestExportService<CsvExportModel>(
            [
                new CsvExportModel { Id = 1, Name = "Alpha", Price = 1.5m },
                new CsvExportModel { Id = 2, Name = "Beta", Price = 20m },
            ]);

            // Act
            var csv = await ExportCsvAsync(service);

            // Assert
            ReadLines(csv).Should().Equal(
                "Id,Name,Price",
                "1,Alpha,1.5",
                "2,Beta,20");
        }

        [Fact]
        public async Task ExportsEveryPageOfResults()
        {
            // Arrange
            var models = Enumerable.Range(1, 7)
                .Select(i => new CsvExportModel { Id = i, Name = $"Item {i}", Price = i })
                .ToList();
            IImportExportDataService service = new TestExportService<CsvExportModel>(models);

            // Act: a page size of 3 means 3 pages, the last one partial
            var csv = await ExportCsvAsync(service, new DataQuery { Top = 3 });

            // Assert
            var lines = ReadLines(csv);
            lines.Should().HaveCount(1 + models.Count);
            lines.Skip(1).Select(l => l.Split(',')[0]).Should().Equal(models.Select(m => m.Id.ToString(CultureInfo.InvariantCulture)));
        }

        [Fact]
        public async Task QuotesFieldsThatContainSeparatorsQuotesAndNewLines()
        {
            // Arrange
            IImportExportDataService service = new TestExportService<CsvExportModel>(
            [
                new CsvExportModel { Id = 1, Name = "Smith, \"Jr\"\nSecond line", Price = 0m },
            ]);

            // Act
            var csv = await ExportCsvAsync(service);

            // Assert: reading it back gives the original value
            using var reader = CsvReader.FromText(csv);
            var records = reader.GetRecords<CsvExportModel>().ToList();
            records.Should().ContainSingle().Which.Name.Should().Be("Smith, \"Jr\"\nSecond line");
        }

        [Fact]
        public async Task ExportUsesInvariantCultureRegardlessOfCurrentCulture()
        {
            // Arrange: de-DE writes 1.5 as "1,5", which would break the CSV columns
            var originalCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            try
            {
                IImportExportDataService service = new TestExportService<CsvExportModel>(
                [
                    new CsvExportModel { Id = 1, Name = "Alpha", Price = 1234.5m },
                ]);

                // Act
                var csv = await ExportCsvAsync(service);

                // Assert
                ReadLines(csv)[1].Should().Be("1,Alpha,1234.5");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public async Task ExportOfNoRecordsProducesNoDataRows()
        {
            // Arrange
            IImportExportDataService service = new TestExportService<CsvExportModel>([]);

            // Act
            var csv = await ExportCsvAsync(service);

            // Assert
            ReadLines(csv).Where(l => l != "Id,Name,Price").Should().BeEmpty();
        }

        [Fact]
        public async Task ExportHonorsCsvRecordMapOfAModelThatCannotBeAnnotated()
        {
            // Arrange: replacement for the removed CsvExportClassMapType
            IImportExportDataService service = new TestExportService<MappedExportModel>(
            [
                new MappedExportModel { Id = 1, Name = "Alpha", Secret = "hidden" },
            ]);

            // Act
            var csv = await ExportCsvAsync(service);

            // Assert
            ReadLines(csv).Should().Equal(
                "Identifier,Name",
                "1,Alpha");
        }

        [Fact]
        public async Task ExportOfModelWithoutCsvMappingFailsWithInvalidOperationException()
        {
            // Arrange: CsvHelper 34 can only write [CsvRecord] or [CsvRecordMap] types
            IImportExportDataService service = new TestExportService<UnmappedExportModel>(
            [
                new UnmappedExportModel { Id = 1 },
            ]);

            // Act
            var act = () => ExportCsvAsync(service);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private static async Task<string> ExportCsvAsync(IImportExportDataService service, DataQuery? query = null)
        {
            var bytes = await service.ExportData(ImportExportDataType.CSV, query ?? new DataQuery());
            return Encoding.UTF8.GetString(bytes);
        }

        private static string[] ReadLines(string csv) =>
            csv.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        private sealed class TestExportService<TModel>(IReadOnlyList<TModel> models) : IImportExportDataService<TModel>
        {
            public Task<DataResponse<TModel>> GetExportModels(DataQuery args)
            {
                var page = models.Skip(args.Skip ?? 0).Take(args.Top ?? int.MaxValue).ToList();
                return Task.FromResult(new DataResponse<TModel> { Count = page.Count, Value = page });
            }

            public Task ImportData(ImportEventArgs<TModel> args) => Task.CompletedTask;
        }
    }

    [CsvRecord]
    public partial class CsvExportModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }

    public class MappedExportModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Secret { get; set; }
    }

    [CsvRecordMap(typeof(MappedExportModel))]
    public partial class MappedExportModelMap
    {
        [CsvColumn("Identifier")]
        public int Id { get; set; }

        [CsvIgnore]
        public string? Secret { get; set; }
    }

    public class UnmappedExportModel
    {
        public int Id { get; set; }
    }
}
