namespace Mimeo.DynamicUI.Blazor.Forms
{
    public class CustomMenuLink : CustomMenuItemBase
    {
        public required string Path { get; set; }
        public string? Target { get; set; }
    }
}
