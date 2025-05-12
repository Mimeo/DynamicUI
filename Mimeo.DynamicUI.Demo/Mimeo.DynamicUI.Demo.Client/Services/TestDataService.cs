using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Data.OData;
using Mimeo.DynamicUI.Demo.Shared.Models;
using Mimeo.DynamicUI.Demo.Shared.ViewModels;
using Mimeo.DynamicUI.Extensions;
using System.Net.Http.Json;

namespace Mimeo.DynamicUI.Demo.Client.Services
{
    public class TestDataService : IDataService<TestViewModel>
    {
        public TestDataService(HttpClient httpClient, ODataExpressionGenerator oDataExpressionGenerator)
        {
            this.httpClient = httpClient;
            this.apiUrl = "/api";
            this.oDataExpressionGenerator = oDataExpressionGenerator;
        }

        private readonly HttpClient httpClient;
        private readonly string apiUrl;
        private readonly ODataExpressionGenerator oDataExpressionGenerator;

        public bool SupportsCreate => true;

        public bool SupportsUpdate => true;

        public bool SupportsCopy => true;

        public bool SupportsDelete => true;

        public bool SupportsView => true;

        public bool SupportsSearchText => true;

        public async Task<DataResponse<TestViewModel>> GetModels(DataQuery args)
        {
            var apiUrl = $"{this.apiUrl}/Test";
            var results = await httpClient.Request(apiUrl).QueryOData<TestModel, PagedResultsModel<TestModel>>(pagedResultsModel => new DataResponse<TestModel>
            {
                Count = pagedResultsModel.TotalCount.GetValueOrDefault(),
                Value = pagedResultsModel.Data
            }, args, oDataExpressionGenerator, uri => !string.IsNullOrEmpty(args.SearchText) ? new Uri(uri.OriginalString + "&search=" + Uri.EscapeDataString(args.SearchText)) : uri);
            return new DataResponse<TestViewModel>
            {
                Value = results.Value?.Select(m => new TestViewModel(m, this))?.ToList() ?? new(),
                Count = results.Count
            };
        }

        public async Task<TestViewModel?> GetEditModel(TestViewModel listModel)
        {
            var apiUrl = $"{this.apiUrl}/Test/{listModel.Id}";
            var model = await httpClient.Request(apiUrl).GetJsonAsync<TestModel>();
            if (model == null)
            {
                return null;
            }

            return new TestViewModel(model, this);
        }

        public async Task<TestViewModel?> GetViewModel(TestViewModel listModel)
        {
            return await GetEditModel(listModel);
        }

        public async Task Create(TestViewModel editModel)
        {
            var apiUrl = $"{this.apiUrl}/Test";
            var response = await httpClient.PostAsync(apiUrl, JsonContent.Create(editModel.ToModel()));
            response.EnsureSuccessStatusCode();
        }

        public async Task Update(TestViewModel editModel)
        {
            var apiUrl = $"{this.apiUrl}/Test";
            var response = await httpClient.PutAsync(apiUrl, JsonContent.Create(editModel.ToModel()));
            response.EnsureSuccessStatusCode();
        }

        public async Task Delete(TestViewModel listModel)
        {
            var apiUrl = $"{this.apiUrl}/Test/{listModel.Id}";
            var response = await httpClient.DeleteAsync(apiUrl);
            response.EnsureSuccessStatusCode();
        }

        public async Task<TestViewModel?> GetCopyModel(TestViewModel listModel)
        {
            var model = await GetEditModel(listModel);
            if (model == null)
            {
                return null;
            }

            model.Id = Guid.Empty.ToString();
            return model;
        }

        public Task<TestViewModel> GetNewModel()
        {
            return Task.FromResult(new TestViewModel(this));
        }

        public Task<IEnumerable<string>> GetSearchTextAutocompleteSuggestions(string? text)
        {
            return Task.FromResult(new[]
            {
                "Suggestion 1",
                "Suggestion 2"
            }.AsEnumerable());
        }
    }
}
