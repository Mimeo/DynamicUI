using CsvHelper;
using Mimeo.DynamicUI.Extensions;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Mimeo.DynamicUI.Data
{
    // The barest bones interface needed by the UI
    // Should offload as MUCH as possible to the implementer
    public interface IImportExportDataService
    {
        IEnumerable<ImportExportDataType> SupportedDataTypes { get; }

        public Type? CsvImportClassMapType => null;
        public Type? CsvExportClassMapType => null;
        public JsonSerializerOptions? JsonSerializerOptions => null;

        Type GetImportModelType(ImportExportDataType dataType);
        Task ImportData(ImportEventArgs<object?> args);

        Task<byte[]> ExportData(ImportExportDataType dataType, DataQuery searchParameters);
    }

    // A simple adapter to make it as easy as possible to implement in most (but not necessarily all) cases
    // Should offload as LITTLE as possible to the implementer
    public interface IImportExportDataService<TModel> : IImportExportDataService
    {
        IEnumerable<ImportExportDataType> IImportExportDataService.SupportedDataTypes => [ImportExportDataType.CSV, ImportExportDataType.JSON];

        Type IImportExportDataService.GetImportModelType(ImportExportDataType dataType) => typeof(TModel);
        Task ImportData(ImportEventArgs<TModel> args);

        async Task IImportExportDataService.ImportData(ImportEventArgs<object?> args)
        {
            var newArgs = new ImportEventArgs<TModel>(data: args.Data.CastAsync<object?, TModel>());
            void onImportedCountChanged(int importedCount)
            {
                args.SetImportedCount(importedCount);
            }
            newArgs.OnImportedCountChanged += onImportedCountChanged;
            await ImportData(newArgs);
            newArgs.OnImportedCountChanged -= onImportedCountChanged;
        }

        Task<DataResponse<TModel>> GetExportModels(DataQuery args);

        async Task<byte[]> IImportExportDataService.ExportData(ImportExportDataType dataType, DataQuery searchParameters)
        {
            var resultsEnumerable = new DataPageEnumerable<TModel>(searchParameters, GetExportModels);
            switch (dataType)
            {
                case ImportExportDataType.CSV:
                    {
                        await using var writer = new StringWriter();
                        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                        if (CsvExportClassMapType != null)
                        {
                            csv.Context.RegisterClassMap(CsvExportClassMapType);
                        }

                        await csv.WriteRecordsAsync(resultsEnumerable);

                        var file = Encoding.UTF8.GetBytes(writer.ToString());
                        return file;
                    }
                case ImportExportDataType.JSON:
                    {
                        var data = await resultsEnumerable.ToListAsync();
                        var json = JsonSerializer.Serialize(data, options: JsonSerializerOptions);

                        var file = Encoding.UTF8.GetBytes(json);
                        return file;
                    }
                default:
                    throw new ArgumentNullException(nameof(dataType), $"Unsupported data type: {dataType}");
            }
        }
    }
}
