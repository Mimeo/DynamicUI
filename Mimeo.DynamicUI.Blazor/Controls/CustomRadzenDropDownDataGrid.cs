using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mimeo.DynamicUI.Blazor.Controls
{
    public class CustomRadzenDropDownDataGrid<TValue> : RadzenDropDownDataGrid<TValue>
    {
        [Parameter]
        public EventCallback OnOpenPopup { get; set; }

        /// <summary>
        /// If specified, the handler for when an item is removed by clicking a Chip is overridden and handled by this callback instead
        /// </summary>
        /// <remarks>
        /// Because of how we're working around different issues, the default handler risks removing more than just the given item
        /// </remarks>
        [Parameter]
        public EventCallback<object> OnChipRemoveOverride { get; set; }

        protected override string GetComponentCssClass()
        {
            // The base class does this:
            // return GetClassList("rz-dropdown").Add("rz-dropdown-chips", Chips && selectedItems.Count > 0).Add("rz-clear", AllowClear).ToString();

            // Problem is that selectedItems is empty if we're viewing a page without any selected items,
            // resulting in an abrupt UI change when navigating between pages.
            // This isn't a Radzen problem on its own (meaning this isn't something we can report as an issue),
            // but is a side effect of us working around a different issue where selected item names don't always persist across pages
            // We're using SelectedValue as a hint to the names of selected items from different pages
            var hasItems = selectedItems.Count > 0 || (SelectedValue is IEnumerable enumerable && SelectedValue is not string && enumerable.Cast<object>().Any());
            return GetClassList("rz-dropdown").Add("rz-dropdown-chips", Chips && hasItems).Add("rz-clear", AllowClear).ToString();
        }

        protected override async Task OpenPopup(string key = "ArrowDown", bool isFilter = false, bool isFromClick = false)
        {
            await base.OpenPopup(key, isFilter, isFromClick);

            await OnOpenPopup.InvokeAsync();
        }

        protected override async Task OnChipRemove(object item)
        {
            if (OnChipRemoveOverride.HasDelegate)
            {
                await OnChipRemoveOverride.InvokeAsync(item);
            }
            else
            {
                await base.OnChipRemove(item);
            }
        }
    }
}
