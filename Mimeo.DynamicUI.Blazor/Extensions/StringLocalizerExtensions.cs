using Microsoft.Extensions.Localization;

namespace Mimeo.DynamicUI.Blazor.Extensions
{
    public static class StringLocalizerExtensions
    {
        /// <summary>
        /// Gets the value of the key from the localizer, or the default language defined if the key is not found.
        /// </summary>
        internal static string GetValueOrDefault(this IStringLocalizer localizer, string key)
        {
            var value = localizer[key];
            if (value.ResourceNotFound)
            {
                return DefaultLanguage.ResourceManager.GetString(key) ?? key;
            }

            return value.Value;
        }
    }
}
