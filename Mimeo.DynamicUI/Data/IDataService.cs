namespace Mimeo.DynamicUI.Data;

public interface IReadOnlyDataService
{
    bool SupportsView { get; }
    bool SupportsSearchText { get; }
    Task<DataResponse<ViewModel>> GetModels(DataQuery args);
    Task<ViewModel> GetNewListModel();

    /// <summary>
    /// Gets a new view model specifically used for storing search filters
    /// </summary>
    Task<ViewModel> GetSearchModel() => GetNewListModel();

    public Task<IEnumerable<string>> GetSearchTextAutocompleteSuggestions(string? text) => Task.FromResult(Enumerable.Empty<string>());
}

public interface IReadOnlyDataService<TListModel> : IReadOnlyDataService
    where TListModel : ViewModel
{
    new Task<TListModel> GetNewListModel();
    new Task<DataResponse<TListModel>> GetModels(DataQuery args);
    async Task<ViewModel> IReadOnlyDataService.GetNewListModel() => await GetNewListModel();
    async Task<DataResponse<ViewModel>> IReadOnlyDataService.GetModels(DataQuery args)
    {
        var result = await GetModels(args);
        return new DataResponse<ViewModel>
        {
            Value = result.Value,
            Count = result.Count
        };
    }
}

public interface IDataService : IReadOnlyDataService
{
    bool SupportsCreate { get; }
    bool SupportsUpdate { get; }
    bool SupportsCopy { get; }
    bool SupportsDelete { get; }

    Task<ViewModel> GetNewEditModel();
    Task<ViewModel?> GetEditModel(ViewModel listModel);
    Task<ViewModel?> GetCopyModel(ViewModel listModel);
    Task<ViewModel?> GetViewModel(ViewModel listModel);

    Task Create(ViewModel editModel);
    Task Update(ViewModel editModel);
    Task Delete(ViewModel listModel);
}

public interface IDataService<TListModel, TEditModel> : IDataService, IReadOnlyDataService<TListModel>
    where TListModel : ViewModel
    where TEditModel : ViewModel
{
    new Task<TEditModel> GetNewEditModel();
    Task<TEditModel?> GetEditModel(TListModel listModel);
    Task<TEditModel?> GetCopyModel(TListModel listModel);

    Task Create(TEditModel editModel);
    Task Update(TEditModel editModel);
    Task Delete(TListModel listModel);

    async Task<ViewModel> IDataService.GetNewEditModel() => await GetNewEditModel();
    async Task<ViewModel?> IDataService.GetEditModel(ViewModel listModel) => await GetEditModel((TListModel)listModel);
    async Task<ViewModel?> IDataService.GetCopyModel(ViewModel listModel) => await GetCopyModel((TListModel)listModel);
    async Task<ViewModel?> IDataService.GetViewModel(ViewModel listModel) => await GetEditModel((TListModel)listModel);

    async Task IDataService.Create(ViewModel editModel) => await Create((TEditModel)editModel);
    async Task IDataService.Update(ViewModel editModel) => await Update((TEditModel)editModel);
    async Task IDataService.Delete(ViewModel listModel) => await Delete((TListModel)listModel);
}

public interface IDataService<TModel> : IDataService<TModel, TModel>
    where TModel : ViewModel
{
    Task<TModel> GetNewModel();
    async Task<TModel> IReadOnlyDataService<TModel>.GetNewListModel() => await GetNewModel();
    async Task<TModel> IDataService<TModel, TModel>.GetNewEditModel() => await GetNewModel();
}