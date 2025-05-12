using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Mimeo.DynamicUI.Blazor.Extensions;
using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Data.OData;
using Radzen;
using System.Reflection;

namespace Mimeo.DynamicUI.Blazor.Controls
{
    public partial class DataSourceDropDownGrid<TValue>
    {
        [Inject]
        public IStringLocalizer Lang { get; set; } = default!;

        [Inject]
        public ODataExpressionGenerator ODataExpressionGenerator { get; set; } = default!;

        [Parameter]
        public string? Id { get; set; }

        [Parameter]
        public TValue? Value { get; set; }

        [Parameter]
        public EventCallback<TValue> ValueChanged { get; set; }

        [Parameter]
        public List<TValue?>? Values { get; set; }

        [Parameter]
        public EventCallback<List<TValue?>> ValuesChanged { get; set; }

        [Parameter]
        public bool Multiple { get; set; }

        [Parameter]
        public IReadOnlyDataService? ItemSource { get; set; }

        [Parameter]
        public DataQuery? ItemSourceDataQuery { get; set; }

        [Parameter]
        public FormFieldDefinition? NameField { get; set; }

        [Parameter]
        public FormFieldDefinition? ValueField { get; set; }

        [Parameter]
        public Dictionary<string, Type>? CustomFormFieldTypes { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

        private IEnumerable<FormFieldDefinition>? formFields;
        private IEnumerable<FormFieldDefinition>? searchFields;
        private CustomRadzenDropDownDataGrid<TValue>? gridRef;
        private CustomRadzenDropDownDataGrid<IEnumerable<TValue>>? gridRefMulti;
        private List<GridItem> gridItems = []; // If null, grid will auto load. Set to [] to defer initial load.
        private bool firstGridItemLoad = false;
        private int gridCount;
        private GridItem? gridSelectedItem;
        private List<GridItem> gridSelectedItems = [];
        private List<GridItem>? defaultSelectedItems;
        private bool isLoading;
        private string Placeholder
        {
            get
            {
                if (Multiple)
                {
                    if (Values?.Count == 1)
                    {
                        return lang.GetValueOrDefault("multiselectdropdown_placeholder_single");
                    }
                    else
                    {
                        return string.Format(lang.GetValueOrDefault("multiselectdropdown_placeholder"), Values?.Count ?? 0);
                    }
                }
                else
                {
                    if (Value != null)
                    {
                        return lang.GetValueOrDefault("multiselectdropdown_placeholder_single");
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        private DataQuery? previousQuery;

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (ItemSource == null)
            {
                throw new InvalidOperationException("ItemSource must be set");
            }

            var sampleViewModel = gridItems.FirstOrDefault()?.ListModel ?? await ItemSource.GetNewListModel();
            formFields = sampleViewModel.GetDropDownListForm().Values;
            searchFields = sampleViewModel.GetSearchForm().Values;

            // Load selected items, if any, so the control knows what the display names are (we only have the IDs right now)
            await LoadSelectedItems(gridItems.Select(g => g.ListModel).Concat(defaultSelectedItems?.Select(i => i.ListModel) ?? []));
        }

        protected async Task OnOpenPopup()
        {
            // Defer loading the first page until the grid is opened
            // This way, a dropdown without any selected item doesn't make an API call
            // Useful for pages with multiple dropdowns that would otherwise each make their own API calls
            if (!firstGridItemLoad)
            {
                if (gridRefMulti != null && Multiple)
                {
                    await gridRefMulti.Reload();
                }
                else if (gridRef != null && !Multiple)
                {
                    await gridRef.Reload();
                }
            }
        }

        private async Task OnGridLoadData(LoadDataArgs args)
        {
            if (ItemSource == null)
            {
                throw new InvalidOperationException($"Parameter '{nameof(ItemSource)}' is required");
            }

            isLoading = true;
            StateHasChanged();

            try
            {
                // Work around Radzen bug where it's not thread safe
                args.Filters = args.Filters?.ToList();
                args.Sorts = args.Sorts?.ToList();

                var searchModel = await ItemSource.GetSearchModel();

                var query = args.ToODataQuery(searchModel);
                if (ItemSourceDataQuery != null)
                {
                    query.Sorts ??= ItemSourceDataQuery.Sorts;
                    query.Filters.AddRange(ItemSourceDataQuery.Filters);
                }

                // The Radzen dropdown data grid doesn't allow filtering on specific columns, and only shows a single search box
                if (ItemSource.SupportsSearchText)
                {
                    // If our item source actually supports that, then great
                    query.SearchText = args.Filter;
                }
                else
                {
                    // Otherwise we'll have to fake it by searching for things where any item contains the search text
                    var filterGroup = new DataQueryFilterGroup
                    {
                        FiltersConjunction = DataFilterConjunction.Or,
                        Filters = []
                    };
                    foreach (var filterableFormField in searchFields ?? formFields?.Where(f => f.Filterable) ?? [])
                    {
                        if ((filterableFormField.FilterType ?? filterableFormField.PropertyType) != typeof(string))
                        {
                            // We're faking it so we don't need to be thorough
                            // Comparing strings to non-strings only sometimes works
                            // If the client application needs something more specific, it can make the data service handle search text
                            continue;
                        }
                        var field = new DataFieldDefinition(filterableFormField);
                        var allowedOperators = ODataExpressionGenerator.GetSupportedFilterOperators(field);
                        var defaultOperator = allowedOperators.FirstOrDefault();
                        if (defaultOperator == default)
                        {
                            continue;
                        }
                        filterGroup.Filters.Add(new DataQueryFilter(field, defaultOperator, args.Filter));
                    }
                    query.Filters.Add(filterGroup);
                }

                if (previousQuery == query)
                {
                    return;
                }

                var result = await ItemSource.GetModels(query);

                var retrievedItems = (result.Value ?? Enumerable.Empty<ViewModel>())
                    .DistinctBy(li => GetViewModelValue(li)) // We will get an error if we have multiple items with the same value
                    .ToList();
                gridCount = result.Count;
                gridItems = retrievedItems.Select(i => new GridItem(i, GetViewModelValue)).ToList();

                previousQuery = query;
                firstGridItemLoad = true;
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task LoadSelectedItems(IEnumerable<ViewModel> retrievedItems)
        {
            if (ItemSource == null)
            {
                throw new InvalidOperationException($"Parameter '{nameof(ItemSource)}' is required");
            }

            if (Multiple)
            {
                if (Values != null)
                {
                    // The grid only knows the names of selected items if the current result set contains them
                    var valuesToLookUp = Values.Except(retrievedItems.Select(i => GetViewModelValue(i)));
                    if (valuesToLookUp.Any())
                    {
                        defaultSelectedItems = (await GetViewModelsByValues(valuesToLookUp))
                            .Concat(retrievedItems.Where(i => Values.Contains(GetViewModelValue(i))))
                            .Select(i => new GridItem(i, GetViewModelValue))
                            .ToList();
                    }
                    else
                    {
                        defaultSelectedItems = retrievedItems
                            .Where(i => Values.Contains(GetViewModelValue(i)))
                            .Select(i => new GridItem(i, GetViewModelValue))
                            .ToList();
                    }
                }
            }
            else
            {
                if (Value != null)
                {
                    if (gridSelectedItem == null || GetViewModelValue(gridSelectedItem.ListModel)?.Equals(Value) == true)
                    {
                        // We're telling it what the value is, but it doesn't know what the name is if the selected item isn't on the current page of data
                        // Let's help it out
                        var existingItem = retrievedItems.FirstOrDefault(i => GetViewModelValue(i)?.Equals(Value) == true);
                        if (existingItem != null)
                        {
                            gridSelectedItem = new GridItem(existingItem, GetViewModelValue);
                        }
                        else
                        {
                            var viewModel = (await GetViewModelsByValues([Value])).FirstOrDefault();
                            gridSelectedItem = viewModel != null ? new GridItem(viewModel, GetViewModelValue) : null;
                        }
                    }
                }
                else
                {
                    gridSelectedItem = null;
                }
            }

            if (defaultSelectedItems?.Any() == false)
            {
                defaultSelectedItems = null;
            }
        }

        private string GetViewModelNameByValue(object value)
        {
            var viewModel = defaultSelectedItems?.FirstOrDefault(i => GetViewModelValue(i.ListModel)?.Equals(value) == true)?.ListModel
                ?? gridSelectedItem?.ListModel
                ?? gridItems?.FirstOrDefault(i => GetViewModelValue(i.ListModel)?.Equals(value) == true)?.ListModel;
            if (viewModel == null)
            {
                return $"[{value}]";
            }

            return GetViewModelName(viewModel);
        }

        private async Task OnValueChanged(TValue value)
        {
            Value = value;
            await ValueChanged.InvokeAsync(value);
        }

        private async Task OnValuesChanged(IEnumerable<TValue?>? values)
        {
            Values = values?.ToList() ?? [];
            await ValuesChanged.InvokeAsync(Values);
        }

        private async Task OnChipRemove(object item)
        {
            // Because of our workarounds, the control's handling of OnChipRemove is a bit bugged and can remove too many things
            // We're going to do it ourselves instead
            var viewModel = ((GridItem)item).ListModel;
            await OnValuesChanged(Values?.Where(v => v?.Equals(GetViewModelValue(viewModel)) == true));
        }

        private string GetViewModelName(ViewModel viewModel)
        {
            if (NameField == null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (ItemSource is IListItemDataService listItemDataService)
                {
                    if (viewModel is ListItem viewModelListItem)
                    {
                        return viewModelListItem.Name;
                    }
                    else
                    {
                        var listItem = listItemDataService.ConvertViewModelToListItem(viewModel);
                        return listItem.Name;
                    }
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }

            return (NameField != null ? viewModel.GetValue(NameField)?.ToString() : null)
                ?? viewModel.ToString()
                ?? viewModel.GetType().Name;
        }

        private TValue? GetViewModelValue(ViewModel viewModel)
        {
            if (ValueField == null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (ItemSource is IListItemDataService listItemDataService)
                {
                    if (viewModel is ListItem viewModelListItem)
                    {
                        return (TValue?)(object)viewModelListItem.Value;
                    }
                    else
                    {
                        var listItem = listItemDataService.ConvertViewModelToListItem(viewModel);
                        return (TValue?)(object)listItem.Value;
                    }
                }
#pragma warning restore CS0618 // Type or member is obsolete

                if (typeof(TValue).IsAssignableFrom(viewModel.GetType()))
                {
                    // The value is the object itself
                    return (TValue)(object)viewModel;
                }

                throw new InvalidOperationException("ValueField must be set");
            }

            return (TValue?)viewModel.GetValue(ValueField);
        }

        private async Task<List<ViewModel>> GetViewModelsByValues(IEnumerable<TValue?> values)
        {
            var valuesSet = values.ToHashSet();
            if (!valuesSet.Any())
            {
                return [];
            }

            if (ItemSource == null)
            {
                throw new InvalidOperationException("ItemSource must be set");
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (ItemSource is IListItemDataService listItemDataService)
            {
                // Backwards compatibility for when we first only supported ListItem, whose values are strings
                return (await listItemDataService.GetListItemsByValues(values.Cast<string>())).Cast<ViewModel>().ToList();
            }
#pragma warning restore CS0618 // Type or member is obsolete

            if (ValueField == null)
            {
                if (typeof(TValue).IsAssignableFrom(valuesSet.First()?.GetType()))
                {
                    // The value is the object itself
                    return values.Cast<ViewModel>().ToList();
                }

                throw new InvalidOperationException("ValueField must be set");
            }

            return (await ItemSource.GetModels(new DataQuery
            {
                Filters = [
                    new DataQueryFilter(new DataFieldDefinition(ValueField), DataFilterOperator.In, valuesSet)
                ],
                Top = valuesSet.Count
            })).Value?.ToList() ?? [];
        }

        // Wrap view model in a class because Radzen will try doing reflection on ViewModel without knowing the real type,
        // then unsurprisingly be unable to find the requested properties in the base class
        private class GridItem : IEquatable<GridItem?>
        {
            public GridItem(ViewModel listModel, Func<ViewModel, TValue?> valueGetter)
            {
                this.ListModel = listModel;
                this.valueGetter = valueGetter;
            }

            private readonly Func<ViewModel, TValue?> valueGetter;

            public ViewModel ListModel { get; set; } = default!;

            public TValue? Value => valueGetter.Invoke(ListModel);

            public override bool Equals(object? obj)
            {
                return Equals(obj as GridItem);
            }

            public bool Equals(GridItem? other)
            {
                return other is not null && EqualityComparer<TValue?>.Default.Equals(Value, other.Value);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(valueGetter, ListModel, Value);
            }

            public static bool operator ==(GridItem? left, GridItem? right)
            {
                return EqualityComparer<GridItem>.Default.Equals(left, right);
            }

            public static bool operator !=(GridItem? left, GridItem? right)
            {
                return !(left == right);
            }
        }
    }
}
