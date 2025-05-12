using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Mimeo.DynamicUI.Blazor.Extensions;
using Mimeo.DynamicUI.Blazor.Forms.DataFilter;
using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Data.OData;
using Radzen;
using Radzen.Blazor;

namespace Mimeo.DynamicUI.Blazor.Forms;

public partial class ODataGrid
{

    [Inject]
    public IDateTimeConverter? DateTimeConverter { get; set; }

    [Inject]
    public NavigationManager? NavigationManager { get; set; }

    [Parameter]
    public IDataService? Service { get; set; }

    [Parameter]
    public IImportExportDataService? ImportExportDataService { get; set; }        

    [Parameter]
    public bool ConfirmDeletes { get; set; } = true;

    [Parameter]
    public string? CreateDialogTitle { get; set; }

    [Parameter]
    public string? UpdateDialogTitle { get; set; }

    [Parameter]
    public string? DeleteDialogTitle { get; set; }

    [Parameter]
    public string? CopyDialogTitle { get; set; }

    [Parameter]
    public string? ViewDialogTitle { get; set; }

    [Parameter]
    public string? DeleteDialogMessage { get; set; }

    [Parameter]
    public string? ImportExportRole { get; set; } = "can_mass_import_and_export";

    [Parameter]
    public bool DisableImportExportRoleCheck { get; set; }

    [Parameter]
    public bool UseInlineSearch { get; set; } = true;

    [Parameter]
    public ODataExpressionGenerator? ODataExpressionGenerator { get; set; }

    /// <summary>
    /// A query string that stores data about filters and sorts, so that a specific query can be saved
    /// </summary>
    [Parameter]
    public string? FiltersQueryString { get; set; }

    [Parameter]
    public Density Density { get; set; } = Density.Compact;
    private ButtonSize MenuButtonSize => Density == Density.Compact ? ButtonSize.Small : ButtonSize.Medium;

    [Parameter]
    public bool AllowCreate { get; set; } = true;

    [Parameter]
    public bool AllowUpdate { get; set; } = true;

    [Parameter]
    public bool AllowCopy { get; set; } = true;

    [Parameter]
    public bool AllowDelete { get; set; } = true;

    [Parameter]
    public bool AllowView { get; set; } = true;

    [Parameter]
    public List<CustomMenuItem> CustomRowActions { get; set; } = [];

    /// <summary>
    /// For form fields of type <see cref="FormFieldType.Custom"/>, a dictionary matching form field property language keys to controls that can display them.
    /// Controls must inherit <see cref="FormFieldBase{TValue}"/>.
    /// </summary>
    [Parameter]
    public Dictionary<string, Type> CustomFormFieldTypes { get; set; } = [];

    private bool CanCreate => (Service?.SupportsCreate ?? false) && AllowCreate;
    private bool CanUpdate => (Service?.SupportsUpdate ?? false) && AllowUpdate;
    private bool CanCopy => (Service?.SupportsCopy ?? false) && AllowCopy;
    private bool CanDelete => (Service?.SupportsDelete ?? false) && AllowDelete;
    private bool CanView => (Service?.SupportsView ?? false) && AllowView;
    private bool SupportsSearchText => Service?.SupportsSearchText ?? false;

    private RadzenDataGrid<GridItem>? gridRef;
    private bool IsSmall;
    private bool IsLarge;

    private IEnumerable<FormFieldDefinition>? formFields;
    private ViewModel? searchModel;
    private FilterViewModel? filterViewModel;
    private IEnumerable<GridItem> items = Enumerable.Empty<GridItem>();
    private int count;
    private DataFilterList _dataFilter = default!;
    private RadzenDropDown<DataFieldDefinition> addFilterDropdown = default!;
    private IEnumerable<string>? searchSuggestions;

    private bool isFirstDataLoad = true;
    private DataQuery? previousQuery;

    /// <summary>
    /// Forces a data reload using the previously used filters
    /// </summary>
    public async Task Reload()
    {
        if (gridRef == null)
        {
            throw new InvalidOperationException("gridRef has not yet been bound");
        }

        await gridRef.Reload();
    }

    /// <summary>
    /// Gets the query used to show the currently displayed data
    /// </summary>
    public DataQuery? GetQuery()
    {
        return previousQuery?.Clone();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (DateTimeConverter != null && ODataExpressionGenerator == null)
        {
            ODataExpressionGenerator = new ODataExpressionGenerator(DateTimeConverter);
        }

        if (Service != null && ODataExpressionGenerator != null)
        {
            var sampleViewModel = items?.FirstOrDefault()?.ListModel ?? await taskRunningService.Run(() => Service.GetNewListModel());
            formFields = sampleViewModel.GetListForm().Values;
            searchModel = await taskRunningService.Run(() => Service.GetSearchModel());
            filterViewModel = new FilterViewModel(searchModel.GetSearchForm().Values, lang, ODataExpressionGenerator);
        }

        if (!string.IsNullOrEmpty(FiltersQueryString) && filterViewModel != null && NavigationManager != null && previousQuery == null)
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var queryString = QueryHelpers.ParseQuery(uri.Query);
            if (queryString.TryGetValue(FiltersQueryString, out var filtersQueryString))
            {
                var queryModel = DataQuerySerializationModel.Deserialize(Uri.UnescapeDataString(filtersQueryString.ToString()));
                if (queryModel != null)
                {
                    var oDataQuery = queryModel.ToODataQuery(filterViewModel);
                    filterViewModel.Filters = oDataQuery.Filters;

                    if (gridRef != null)
                    {
                        await gridRef.Reload();
                    }
                }
            }
        }
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Don't base first render off of `firstRender` since the grid might not be fully rendered on the first render
        // (It's complicated, but likely a combination of us dynamically building the columns plus Radzen's specific internals)
        if (isFirstDataLoad && gridRef?.ColumnsCollection.Count > 0)
        {
            isFirstDataLoad = false;

            await Reload();
        }
    }

    private void UpdateQueryStrings(DataQuery query)
    {
        if (string.IsNullOrEmpty(FiltersQueryString))
        {
            return;
        }

        if (NavigationManager == null)
        {
            throw new InvalidOperationException("NavigationManager is required");
        }

        var serialized = DataQuerySerializationModel.Serialize(query);
        NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(FiltersQueryString, Uri.EscapeDataString(serialized)));
    }

    private async Task LoadData(LoadDataArgs args)
    {
        if (Service == null)
        {
            throw new InvalidOperationException($"Parameter '{nameof(Service)}' is required");
        }

        // Work around Radzen bug where it's not thread safe
        args.Filters = args.Filters.ToList();
        args.Sorts = args.Sorts.ToList();

        // The GridItem class is private to this control
        // Filters and sorts will be on a GridItem property,
        // but we need to feed the actual property to ListMethod()
        foreach (var filter in args.Filters)
        {
            filter.Property = filter.Property.Replace($"{nameof(GridItem.ListModel)}.", "").Replace($"{nameof(GridItem.ListModel)}/", "");
        }
        foreach (var orderBy in args.Sorts)
        {
            orderBy.Property = orderBy.Property.Replace($"{nameof(GridItem.ListModel)}.", "").Replace($"{nameof(GridItem.ListModel)}/", "");
        }
        searchModel ??= await taskRunningService.Run(() => Service.GetSearchModel());
        var query = args.ToODataQuery(searchModel);

        if (!UseInlineSearch)
        {
            query.Filters = filterViewModel?.Filters.ToList() ?? new();
            query.SearchText = filterViewModel?.SearchText;
        }

        if (previousQuery == query)
        {
            return;
        }

        UpdateQueryStrings(query);

        await taskRunningService.Run(async () =>
        {
            var result = await Service.GetModels(query);
            count = result.Count;
            items = (result.Value?.Select(v => CreateGridItem(v)) ?? Enumerable.Empty<GridItem>());
        });

        previousQuery = query.Clone();
    }

    private async Task LoadSearchSuggestions(LoadDataArgs args)
    {
        if (Service == null || filterViewModel == null)
        {
            throw new InvalidOperationException($"Parameter '{nameof(Service)}' is required");
        }

        searchSuggestions = await Service.GetSearchTextAutocompleteSuggestions(filterViewModel.SearchText);
    }

    private async Task OpenEditDialogAsync(ViewModel? model = null)
    {
        if (Service == null)
        {
            throw new InvalidOperationException("Service is required");
        }

        var formType = model == null ? FormType.Create : FormType.Update;
        var dialogTitle = model == null
            ? (CreateDialogTitle ?? lang.GetValueOrDefault("create"))
            : (UpdateDialogTitle ?? lang.GetValueOrDefault("edit"));

        ViewModel editModel;
        if (model != null)
        {
            editModel = await taskRunningService.Run(() => Service.GetEditModel(model)) ?? throw new Exception("Could not get edit model copy");
        }
        else
        {
            editModel = await taskRunningService.Run(() => Service.GetNewEditModel());
        }

        var result = await dialogService.OpenEditDialogAsync(dialogTitle, editModel!, customFormFieldTypes: CustomFormFieldTypes);

        if (result == true)
        {
            if (formType == FormType.Create)
            {
                if (!Service.SupportsCreate)
                {
                    throw new NotSupportedException("Service does not support Create");
                }

                await taskRunningService.Run(() => Service.Create(editModel));
            }
            else if (formType == FormType.Update)
            {
                if (!Service.SupportsUpdate)
                {
                    throw new NotSupportedException("Service does not support Update");
                }

                await taskRunningService.Run(() => Service.Update(editModel));
            }

            if (gridRef != null)
            {
                previousQuery = null;
                await gridRef.Reload();
            }
        }
    }

    private async Task OpenViewDialogAsync(ViewModel listModel)
    {
        if (Service == null)
        {
            throw new InvalidOperationException("Service is required");
        }

        var dialogTitle = ViewDialogTitle ?? lang.GetValueOrDefault("view");

        var viewModel = await taskRunningService.Run(() => Service.GetViewModel(listModel)) ?? throw new Exception("Could not get view model");

        await dialogService.OpenViewDialogAsync(dialogTitle, viewModel!, customFormFieldTypes: CustomFormFieldTypes);
    }

    private async Task OpenCopyDialogAsync(ViewModel listModel)
    {
        if (Service == null)
        {
            throw new InvalidOperationException("Service is required");
        }

        var dialogTitle = CopyDialogTitle ?? lang.GetValueOrDefault("copy");

        var copyModel = await taskRunningService.Run(() => Service.GetCopyModel(listModel)) ?? throw new Exception("Could not get edit model copy");

        var result = await dialogService.OpenEditDialogAsync(dialogTitle, copyModel!, customFormFieldTypes: CustomFormFieldTypes);
        if (result == true)
        {
            if (!Service.SupportsCopy)
            {
                throw new NotSupportedException("Service does not support Copy");
            }
            if (!Service.SupportsCreate)
            {
                throw new NotSupportedException("Service does not support Create");
            }

            await taskRunningService.Run(() => Service.Create(copyModel));

            if (gridRef != null)
            {
                await gridRef.Reload();
            }
        }
    }

    private async Task OpenDeleteDialogAsync(ViewModel listModel)
    {
        if (Service == null)
        {
            throw new InvalidOperationException("Service is required");
        }

        var title = DeleteDialogTitle ?? lang.GetValueOrDefault("dynamiceditform_deleteconfirm_defaulttitle");
        var message = DeleteDialogMessage ?? lang.GetValueOrDefault("dynamiceditform_deleteconfirm_defaulttext");
        if (ConfirmDeletes)
        {
            if (!await dialogService.OpenConfirmation(title, message))
            {
                return;
            }
        }
        await taskRunningService.Run(() => Service.Delete(listModel));

        if (gridRef != null)
        {
            await gridRef.Reload();
        }
    }

    private void AddFilter(object? oDataPath)
    {
        if (oDataPath is not string oDataPathString || string.IsNullOrEmpty(oDataPathString))
        {
            return;
        }

        var searchField = filterViewModel?.AvailableSearchFields.FirstOrDefault(f => f.GetODataPath() == oDataPathString) ?? throw new InvalidOperationException("Unable to determine search field");
        _dataFilter.AddFilter(searchField);
        addFilterDropdown.Reset();

    }

    private async Task OnDataFilterChange()
    {
        if (gridRef != null)
        {
            gridRef.Reset();
            await gridRef.Reload();
        }
    }

    private async Task OnSearchChanged()
    {
        if (gridRef != null)
        {
            gridRef.Reset();
            await gridRef.Reload();
        }
    }

    private async Task OnImportComplete()
    {
        if (gridRef != null)
        {
            await gridRef.Reload();
        }
    }

    private void ShowTooltip(ElementReference element, string languageKey)
    {
        tooltipService.Open(element, lang.GetValueOrDefault(languageKey), new TooltipOptions() { Position = TooltipPosition.Bottom });
    }
    private bool GetActionsMenuVisibility()
    {
        return CanCopy || CanUpdate || CanView || CanDelete;
    }

    private GridItem CreateGridItem(ViewModel listModel)
    {
        return new GridItem
        {
            ListModel = listModel,
            EditRow = OpenEditDialogAsync,
            DeleteRow = OpenDeleteDialogAsync,
            CopyRow = OpenCopyDialogAsync,
            ViewRow = OpenViewDialogAsync,
            ShowTooltip = ShowTooltip
        };
    }

    // Confine creation of delegates to this class and not in the UI
    // Otherwise blazor will have to recreate them on every render
    // Additional reading: https://github.com/dotnet/aspnetcore/issues/17886#issuecomment-565794337
    private class GridItem
    {
        public ViewModel ListModel { get; set; } = default!;

        public Func<ViewModel, Task> EditRow { get; set; } = default!;
        public Func<ViewModel, Task> DeleteRow { get; set; } = default!;
        public Func<ViewModel, Task> CopyRow { get; set; } = default!;
        public Func<ViewModel, Task> ViewRow { get; set; } = default!;
        public Action<ElementReference, string> ShowTooltip { get; set; } = default!;

        public async Task OnCustomAction(MouseEventArgs _, CustomMenuItem customMenuItem)
        {
            await customMenuItem.Callback.InvokeAsync(ListModel);
        }
        public void OnCustomActionMouseEnter(ElementReference element, CustomMenuItem customMenuItem) => ShowTooltip(element, customMenuItem.Name);

        public async Task OnEditClick(MouseEventArgs _) => await EditRow(ListModel);
        public void OnEditMouseEnter(ElementReference element) => ShowTooltip(element, "edit");

        public async Task OnDeleteClick(MouseEventArgs _) => await DeleteRow(ListModel);
        public void OnDeleteMouseEnter(ElementReference element) => ShowTooltip(element, "delete");

        public async Task OnCopyClick(MouseEventArgs _) => await CopyRow(ListModel);
        public void OnCopyMouseEnter(ElementReference element) => ShowTooltip(element, "copy");

        public async Task OnViewClick(MouseEventArgs _) => await ViewRow(ListModel);
        public void OnViewMouseEnter(ElementReference element) => ShowTooltip(element, "view");

    }
}