using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Radzen.Blazor;

namespace Mimeo.DynamicUI.Blazor.Controls
{
    /// <summary>
    /// A <see cref="RadzenButton"/> that renders as an anchor instead of a button, so that the browser's
    /// open-in-new-tab gestures (middle click, ctrl+click) work on it. Styling comes from the base class,
    /// so it looks the same as the equivalent <see cref="RadzenButton"/>.
    /// </summary>
    /// <remarks>
    /// Only the icon, image and text content of <see cref="RadzenButton"/> is supported; a link cannot be busy.
    /// </remarks>
    public class LinkButton : RadzenButton
    {
        /// <summary>
        /// The URL to navigate to.
        /// </summary>
        [Parameter]
        public string? Path { get; set; }

        /// <summary>
        /// The browsing context to open the URL in, such as "_blank".
        /// </summary>
        [Parameter]
        public string? Target { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (!Visible)
            {
                return;
            }

            builder.OpenElement(0, "a");
            builder.AddAttribute(1, "style", Style);
            builder.AddAttribute(2, "tabindex", Disabled ? -1 : TabIndex);
            builder.AddAttribute(3, "target", Target);
            builder.AddMultipleAttributes(4, Attributes);
            // An anchor has no disabled state, so drop the destination instead. GetCssClass() still greys it out.
            builder.AddAttribute(5, "href", Disabled ? null : Path);
            builder.AddAttribute(6, "class", GetCssClass());
            builder.AddAttribute(7, "id", GetId());
            builder.AddElementReferenceCapture(8, element => Element = element);

            builder.OpenElement(9, "span");
            builder.AddAttribute(10, "class", "rz-button-box");
            if (ChildContent != null)
            {
                builder.AddContent(11, ChildContent);
            }
            else
            {
                if (!string.IsNullOrEmpty(Icon))
                {
                    builder.OpenElement(12, "i");
                    builder.AddAttribute(13, "class", "notranslate rz-button-icon-left rzi");
                    builder.AddAttribute(14, "aria-hidden", "true");
                    builder.AddAttribute(15, "style", !string.IsNullOrEmpty(IconColor) ? $"color:{IconColor}" : null);
                    builder.AddContent(16, Icon);
                    builder.CloseElement();
                }
                if (!string.IsNullOrEmpty(Image))
                {
                    builder.OpenElement(17, "img");
                    builder.AddAttribute(18, "class", "notranslate rz-button-icon-left rzi");
                    builder.AddAttribute(19, "src", Image);
                    builder.AddAttribute(20, "alt", ImageAlternateText);
                    builder.CloseElement();
                }
                if (!string.IsNullOrEmpty(Text))
                {
                    builder.OpenElement(21, "span");
                    builder.AddAttribute(22, "class", "rz-button-text");
                    builder.AddContent(23, Text);
                    builder.CloseElement();
                }
            }
            builder.CloseElement();

            builder.CloseElement();
        }
    }
}
