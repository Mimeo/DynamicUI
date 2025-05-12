using Microsoft.AspNetCore.Components;

namespace Mimeo.DynamicUI.Blazor.Forms
{
    public class CustomMenuItem
    {
        /// <summary>
        /// Name of the action to be performed.
        /// Should either be a user-friendly string or a key to be given to the string localizer.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Callback when the menu item is clicked
        /// </summary>
        public required EventCallback<ViewModel> Callback { get; set; }

        /// <summary>
        /// Name of the material icon used to represent the action
        /// </summary>
        public string Icon { get; set; } = "menu";
    }
}
