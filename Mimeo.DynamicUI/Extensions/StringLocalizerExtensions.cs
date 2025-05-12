using Microsoft.Extensions.Localization;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI.Extensions
{
    public static class StringLocalizerExtensions
    {
        public static string GetPropertyNameKey(Type? classType, string propertyName)
        {
            var className = classType?.IsGenericType == true
                ? classType.GetGenericTypeDefinition().Name.Split('`')[0]
                : classType?.Name;
            var langKey = (!string.IsNullOrEmpty(className) ? $"{className}_{propertyName}" : $"{propertyName}").ToLower();
            return langKey;
        }

        public static string GetPropertyNameKey(Expression<Func<object?>> expression)
        {
            var property = expression.GetSelectedProperty();
            return GetPropertyNameKey(property.ReflectedType, property.Name);
        }

        public static string GetPropertyNameKey<TObject>(Expression<Func<TObject, object?>> expression)
        {
            var property = expression.GetSelectedProperty();
            var propertyName = property.Name;
            var className = typeof(TObject).Name;
            var langKey = $"{className}_{propertyName}".ToLower();
            return langKey;
        }

        public static string GetPropertyName<TObject>(this IStringLocalizer stringLocalizer, Expression<Func<TObject, object?>> expression)
        {
            var langKey = GetPropertyNameKey(expression);
            return stringLocalizer[langKey];
        }

        public static string GetPropertyName(this IStringLocalizer stringLocalizer, Expression<Func<object?>> expression)
        {
            var langKey = GetPropertyNameKey(expression);
            return stringLocalizer[langKey];
        }

        public static string GetPropertyName(this IStringLocalizer stringLocalizer, FormFieldDefinition formFieldDefinition)
        {
            return stringLocalizer[formFieldDefinition.LanguageKey];
        }

        public static string GetPropertyDescription(this IStringLocalizer stringLocalizer, FormFieldDefinition formFieldDefinition)
        {
            var langKey = formFieldDefinition.LanguageKey + "_desc";
            var translated = stringLocalizer[langKey];
            if (langKey == translated)
            {
                // Unlike the name, we only want to display the description if one's defined
                return "";
            }
            return translated;
        }

        public static string GetPropertyDescription(this IStringLocalizer stringLocalizer, Expression<Func<object?>> expression)
        {
            var langKey = GetPropertyNameKey(expression) + "_desc";
            var translated = stringLocalizer[langKey];
            if (langKey == translated)
            {
                // Unlike the name, we only want to display the description if one's defined
                return "";
            }
            return translated;
        }
    }
}
