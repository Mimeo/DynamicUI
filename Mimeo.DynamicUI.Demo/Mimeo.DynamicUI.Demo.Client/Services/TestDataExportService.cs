using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Data.OData;
using Mimeo.DynamicUI.Demo.Shared.Models;
using Mimeo.DynamicUI.Extensions;
using System.Net.Http.Json;

namespace Mimeo.DynamicUI.Demo.Client.Services
{
    public class TestDataExportService : IImportExportDataService<TestModel>
    {
        public TestDataExportService(HttpClient httpClient, ODataExpressionGenerator oDataExpressionGenerator)
        {
            this.httpClient = httpClient;
            this.apiUrl = "/api";
            this.oDataExpressionGenerator = oDataExpressionGenerator;
        }

        private readonly HttpClient httpClient;
        private readonly string apiUrl;
        private readonly ODataExpressionGenerator oDataExpressionGenerator;

        public async Task<DataResponse<TestModel>> GetExportModels(DataQuery args)
        {
            var apiUrl = $"{this.apiUrl}/Test";
            return await httpClient.Request(apiUrl).QueryOData<TestModel, PagedResultsModel<TestModel>>(pagedResultsModel => new DataResponse<TestModel>
            {
                Count = pagedResultsModel.TotalCount.GetValueOrDefault(),
                Value = pagedResultsModel.Data
            }, args, oDataExpressionGenerator);
        }

        public async Task ImportData(ImportEventArgs<TestModel> args)
        {
            // With the current design, the API must implement upsert
            var apiUrl = $"{this.apiUrl}/Test";

            var importedCount = 0;
            await foreach (var model in args.Data)
            {
                var response = await httpClient.PostAsync(apiUrl, JsonContent.Create(model));
                response.EnsureSuccessStatusCode();

                importedCount += 1;
                args.SetImportedCount(importedCount);
            }
        }
    }
}
