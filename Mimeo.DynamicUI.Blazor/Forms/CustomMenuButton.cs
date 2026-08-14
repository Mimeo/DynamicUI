using Microsoft.AspNetCore.Components;

namespace Mimeo.DynamicUI.Blazor.Forms
{
    public class CustomMenuButton : CustomMenuItemBase
    {
        /// <summary>
        /// Callback when the menu item is clicked
        /// </summary>
        public required EventCallback<ViewModel> Callback { get; set; }
    }

    [Obsolete("Use CustomMenuItemBase instead for lists, use CustomMenuButton instead for buttons.", error: true)]
    public class CustomMenuItem : CustomMenuButton
    {
    }
}
