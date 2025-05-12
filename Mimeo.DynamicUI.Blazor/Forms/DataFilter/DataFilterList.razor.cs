using Microsoft.AspNetCore.Components;
using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Data.OData;

namespace Mimeo.DynamicUI.Blazor.Forms.DataFilter
{
    public partial class DataFilterList
    {
        [Parameter]
        public FilterViewModel? FilterViewModel { get; set; }

        [Parameter]
        public EventCallback OnChange { get; set; }

        [Parameter]
        public bool Visible { get; set; } = true;

        [Parameter]
        public ODataExpressionGenerator? ODataExpressionGenerator { get; set; }

        public bool HasFilters => FilterViewModel?.Filters.Count > 0;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
        }

        public void AddFilter(DataFieldDefinition property)
        {
            FilterViewModel?.Filters.Add(new DataQueryFilter(property));
            this.StateHasChanged();
        }

        private async Task OnFilterChange(DataQueryFilter filter)
        {
            if (OnChange.HasDelegate)
            {
                await OnChange.InvokeAsync();
            }
        }

        private async Task OnFilterDelete(DataQueryFilter filter)
        {
            if (FilterViewModel != null && FilterViewModel.Filters.Remove(filter))
            {
                this.StateHasChanged();

                if (OnChange.HasDelegate)
                {
                    await OnChange.InvokeAsync();
                }
            }
        }
    }
}
