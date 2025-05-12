namespace Mimeo.DynamicUI.Extensions
{
    internal static class FormFieldDefinitionExtensions
    {
        public static DateDisplayMode GetDateDisplayMode(this FormFieldDefinition definition)
        {
            return definition switch
            {
                DateTimeFieldDefinition dtfd => dtfd.DisplayMode,
                DateSearchFieldDefinition dsfd => dsfd.DisplayMode,
                _ => DateDisplayMode.Raw,
            };
        }
    }
}
