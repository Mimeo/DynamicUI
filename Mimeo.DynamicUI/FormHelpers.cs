using System.Globalization;

namespace Mimeo.DynamicUI
{
    public static class FormHelpers
    {
        public static string TitleCaseConvert(string value)
        {
            return new CultureInfo("en").TextInfo.ToTitleCase(value.ToLower().Replace("_", " "));
        }
    }

    public enum FormType
    {
        Create,
        Update,
        Delete
    }
}
