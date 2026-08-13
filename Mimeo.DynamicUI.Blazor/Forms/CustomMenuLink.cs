namespace Mimeo.DynamicUI.Blazor.Forms
{
    public class CustomMenuLink : CustomMenuItemBase
    {
        public required Func<ViewModel, string> Path { get; set; }
        public Func<ViewModel, string>? Target { get; set; }
    }
}
