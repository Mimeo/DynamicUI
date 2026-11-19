using AwesomeAssertions;
using Bunit;
using CsvHelper;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Mimeo.DynamicUI.Blazor.Forms;
using Mimeo.DynamicUI.Blazor.Services;
using Mimeo.DynamicUI.Data;
using System.Globalization;

namespace Mimeo.DynamicUI.Blazor.Tests.Forms
{
    /// <summary>
    /// ImportForm reads CSV records through reflection (a generic helper made generic over
    /// <see cref="IImportExportDataService.GetImportModelType"/>), so these tests cover the model shapes it must handle.
    /// </summary>
    public class ImportFormCsvTests
    {
        private const string CsvContentType = "text/csv";

        [Fact]
        public async Task ImportsCsvRecordsAsInstancesOfTheImportModelType()
        {
            // Arrange
            using var context = CreateContext();
            var service = new RecordingImportService<CsvImportModel>();
            var imported = new List<object?>();

            var form = RenderImportForm(context, service, async args =>
            {
                await foreach (var record in args.Data)
                {
                    imported.Add(record);
                }
            });

            // Act
            await UploadAsync(form, "Id,Name,Price\n1,Alpha,1.5\n2,\"Smith, Jr\",20\n");

            // Assert
            form.WaitForAssertion(() => imported.Should().HaveCount(2));
            imported.Should().AllBeOfType<CsvImportModel>();
            imported.Cast<CsvImportModel>().Should().BeEquivalentTo(
            [
                new CsvImportModel { Id = 1, Name = "Alpha", Price = 1.5m },
                new CsvImportModel { Id = 2, Name = "Smith, Jr", Price = 20m },
            ]);
            ErrorText(form).Should().BeEmpty();
        }

        [Fact]
        public async Task ImportedRecordsFlowThroughTheTypedImportExportDataServiceAdapter()
        {
            // Arrange: the same wiring ImportExportButtons uses
            using var context = CreateContext();
            var service = new RecordingImportService<CsvImportModel>();
            IImportExportDataService untypedService = service;

            var form = RenderImportForm(context, service, untypedService.ImportData);

            // Act
            await UploadAsync(form, "Id,Name,Price\n1,Alpha,1.5\n");

            // Assert
            form.WaitForAssertion(() => service.Imported.Should().ContainSingle());
            service.Imported[0].Should().BeEquivalentTo(new CsvImportModel { Id = 1, Name = "Alpha", Price = 1.5m });
        }

        [Fact]
        public async Task ImportsStructModels()
        {
            // Arrange: CsvHelper supports [CsvRecord] structs, but IAsyncEnumerable<T> is only covariant for reference types
            using var context = CreateContext();
            var service = new RecordingImportService<CsvImportStruct>();
            IImportExportDataService untypedService = service;

            var form = RenderImportForm(context, service, untypedService.ImportData);

            // Act
            await UploadAsync(form, "Id,Name\n1,Alpha\n2,Beta\n");

            // Assert
            form.WaitForAssertion(() => ErrorText(form).Should().BeEmpty());
            form.WaitForAssertion(() => service.Imported.Should().Equal(
                new CsvImportStruct { Id = 1, Name = "Alpha" },
                new CsvImportStruct { Id = 2, Name = "Beta" }));
        }

        [Fact]
        public async Task ImportHonorsCsvRecordMapOfAModelThatCannotBeAnnotated()
        {
            // Arrange: replacement for the removed CsvImportClassMapType
            using var context = CreateContext();
            var service = new RecordingImportService<MappedImportModel>();
            IImportExportDataService untypedService = service;

            var form = RenderImportForm(context, service, untypedService.ImportData);

            // Act
            await UploadAsync(form, "Identifier,Name\n7,Alpha\n");

            // Assert
            form.WaitForAssertion(() => service.Imported.Should().ContainSingle());
            service.Imported[0].Should().BeEquivalentTo(new MappedImportModel { Id = 7, Name = "Alpha", Secret = null });
        }

        [Fact]
        public async Task ImportUsesInvariantCultureRegardlessOfCurrentCulture()
        {
            // Arrange: de-DE would read "1.5" as 15
            var originalCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            try
            {
                using var context = CreateContext();
                var service = new RecordingImportService<CsvImportModel>();
                IImportExportDataService untypedService = service;

                var form = RenderImportForm(context, service, untypedService.ImportData);

                // Act
                await UploadAsync(form, "Id,Name,Price\n1,Alpha,1.5\n");

                // Assert
                form.WaitForAssertion(() => service.Imported.Should().ContainSingle());
                service.Imported[0].Price.Should().Be(1.5m);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public async Task ShowsAnErrorWhenAColumnIsMissing()
        {
            // Arrange
            using var context = CreateContext();
            var service = new RecordingImportService<CsvImportModel>();
            IImportExportDataService untypedService = service;

            var form = RenderImportForm(context, service, untypedService.ImportData);

            // Act
            await UploadAsync(form, "Id,Name\n1,Alpha\n");

            // Assert
            form.WaitForAssertion(() => ErrorText(form).Should().Contain("Price"));
            service.Imported.Should().BeEmpty();
        }

        [Fact]
        public async Task ShowsAnErrorWhenTheModelHasNoCsvMapping()
        {
            // Arrange: CsvHelper 34 can only read [CsvRecord] or [CsvRecordMap] types
            using var context = CreateContext();
            var service = new RecordingImportService<UnmappedImportModel>();
            IImportExportDataService untypedService = service;

            var form = RenderImportForm(context, service, untypedService.ImportData);

            // Act
            await UploadAsync(form, "Id\n1\n");

            // Assert
            form.WaitForAssertion(() => ErrorText(form).Should().Contain(nameof(UnmappedImportModel)));
            service.Imported.Should().BeEmpty();
        }

        private static BunitContext CreateContext()
        {
            var context = new BunitContext();
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            context.Services.AddSingleton<TaskRunningService>();
            context.Services.AddSingleton<IStringLocalizer, KeyStringLocalizer>();
            return context;
        }

        private static IRenderedComponent<ImportForm> RenderImportForm(
            BunitContext context,
            IImportExportDataService service,
            Func<ImportEventArgs<object?>, Task> onImport)
        {
            return context.Render<ImportForm>(parameters => parameters
                .Add(p => p.ImportExportDataService, service)
                .Add(p => p.OnImport, onImport));
        }

        private static async Task UploadAsync(IRenderedComponent<ImportForm> form, string csv)
        {
            var inputFile = form.FindComponent<InputFile>();
            await form.InvokeAsync(() => inputFile.UploadFiles(InputFileContent.CreateFromText(csv, "import.csv", contentType: CsvContentType)));
        }

        private static string ErrorText(IRenderedComponent<ImportForm> form) =>
            form.Find(".alert-danger").TextContent.Trim();

        private sealed class RecordingImportService<TModel> : IImportExportDataService<TModel>
        {
            public List<TModel> Imported { get; } = [];

            public async Task ImportData(ImportEventArgs<TModel> args)
            {
                await foreach (var record in args.Data)
                {
                    Imported.Add(record);
                }
            }

            public Task<DataResponse<TModel>> GetExportModels(DataQuery args) =>
                Task.FromResult(new DataResponse<TModel> { Count = 0, Value = [] });
        }

        private sealed class KeyStringLocalizer : IStringLocalizer
        {
            public LocalizedString this[string name] => new(name, name);

            public LocalizedString this[string name, params object[] arguments] => new(name, name);

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
        }
    }

    [CsvRecord]
    public partial class CsvImportModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }

    [CsvRecord]
    public partial struct CsvImportStruct
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class MappedImportModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Secret { get; set; }
    }

    [CsvRecordMap(typeof(MappedImportModel))]
    public partial class MappedImportModelMap
    {
        [CsvColumn("Identifier")]
        public int Id { get; set; }

        [CsvIgnore]
        public string? Secret { get; set; }
    }

    public class UnmappedImportModel
    {
        public int Id { get; set; }
    }
}
