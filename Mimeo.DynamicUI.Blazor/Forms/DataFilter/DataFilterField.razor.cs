using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Mimeo.DynamicUI.Blazor.Extensions;
using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Data.OData;
using Radzen.Blazor.Rendering;

namespace Mimeo.DynamicUI.Blazor.Forms.DataFilter
{
    public partial class DataFilterField
    {
        private ElementReference _customSplitButton;
        private Popup? _popup;
        private IEnumerable<FilterOperatorItem>? _filterOperators;

        [Parameter]
        public required DataQueryFilter Filter { get; set; }

        [Parameter]
        public EventCallback OnChange { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> OnDelete { get; set; }

        [Parameter]
        public ODataExpressionGenerator? ODataExpressionGenerator { get; set; }

        private FilterOperatorItem? SelectedFilterOperator
        {
            get => _selectedFilterOperator;
            set
            {
                _selectedFilterOperator = value;
                Filter.Operator = value?.FilterOperator ?? default;
            }
        }
        private FilterOperatorItem? _selectedFilterOperator;

        protected override void OnParametersSet()
        {
            if (ODataExpressionGenerator == null)
            {
                return;
            }

            _filterOperators = ODataExpressionGenerator.GetSupportedFilterOperators(Filter.FilterDefinition).Select(o => new FilterOperatorItem
            {
                FilterOperator = o,
                Label = lang.GetValueOrDefault("filteroperator_" + o.ToString("f").ToLower())
            }).ToList();
            SelectedFilterOperator = _filterOperators.SingleOrDefault(o => o.FilterOperator == Filter.Operator) ?? _filterOperators.FirstOrDefault();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && _popup != null && _customSplitButton.Id != null 
                && Filter.Value == null /* Show if a user added the filter, don't if it was loaded from the URL */)
            {
                await _popup.ToggleAsync(_customSplitButton);
            }
        }

        private async Task OnSave()
        {
            if (_popup != null)
            {
                await _popup.CloseAsync();
            }
            
            await OnChange.InvokeAsync();
        }

        protected class FilterOperatorItem
        {
            public DataFilterOperator FilterOperator { get; set; }
            public required string Label { get; set; }
        }
    }
}
