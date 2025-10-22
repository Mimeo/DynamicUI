using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Extensions;
using System.ComponentModel;
using System.Linq.Expressions;
using StringLocalizerExtensions = Mimeo.DynamicUI.Extensions.StringLocalizerExtensions;

namespace Mimeo.DynamicUI
{
    public class FormFieldDefinition : INotifyPropertyChanged
    {
        private static readonly Dictionary<Type, FormFieldType> valueFormFieldTypeMap = new()
        {
            { typeof(string), FormFieldType.Text },
            { typeof(bool), FormFieldType.Checkbox },
            { typeof(TimeSpan), FormFieldType.Time },
            { typeof(DateTime), FormFieldType.DateTime },
            { typeof(DateTime?), FormFieldType.DateTime },
            { typeof(DateTimeOffset), FormFieldType.DateTime },
            { typeof(DateTimeOffset?), FormFieldType.DateTime },
            { typeof(DateFilter), FormFieldType.DateTime },
            { typeof(decimal), FormFieldType.Decimal },
            { typeof(int), FormFieldType.Integer },
            { typeof(int?), FormFieldType.Integer },
            { typeof(Guid), FormFieldType.Guid },
            { typeof(object), FormFieldType.Section },
        };

        public static FormFieldType DetermineFormFieldType(Type type)
        {
            return valueFormFieldTypeMap.GetValueOrDefault(type);
        }

        public FormFieldDefinition(Type classType, Type propertyType, string propertyName)
        {
            Type = DetermineFormFieldType(propertyType);
            PropertyType = propertyType;
            PropertyName = propertyName;
            LanguageKey = StringLocalizerExtensions.GetPropertyNameKey(classType, propertyName);
        }

        public FormFieldDefinition(string className, Type propertyType, string propertyName)
        {
            Type = DetermineFormFieldType(propertyType);
            PropertyType = propertyType;
            PropertyName = propertyName;
            LanguageKey = $"{className}_{propertyName}".ToLower();
        }

        public FormFieldDefinition(Type propertyType, string propertyName, string languageKey)
        {
            Type = DetermineFormFieldType(propertyType);
            PropertyType = propertyType;
            PropertyName = propertyName;
            LanguageKey = languageKey;
        }

        public FormFieldDefinition(string className, FormFieldType type, Type propertyType, string propertyName)
        {
            Type = type;
            PropertyType = propertyType;
            PropertyName = propertyName;
            LanguageKey = $"{className}_{propertyName}".ToLower();
        }

        public FormFieldDefinition(Type classType, FormFieldType type, Type propertyType, string propertyName)
        {
            Type = type;
            PropertyType = propertyType;
            PropertyName = propertyName;
            LanguageKey = StringLocalizerExtensions.GetPropertyNameKey(classType, propertyName);
        }

        public FormFieldDefinition(FormFieldType type, Type propertyType, string propertyName, string languageKey)
        {
            Type = type;
            PropertyType = propertyType;
            PropertyName = propertyName;
            LanguageKey = languageKey;
        }

        public FormFieldDefinition(FormFieldType type, Expression<Func<object?>> @for)
        {
            ArgumentNullException.ThrowIfNull(@for);

            Type = type;

            PropertyName = @for.GetSelectedProperty().Name;
            PropertyType = @for.GetSelectedProperty().PropertyType;
            LanguageKey = StringLocalizerExtensions.GetPropertyNameKey(@for);
            ValidationProperty = @for;
        }

        public FormFieldDefinition(FormFieldType type, Expression<Func<object?>> @for, Type searchFieldType)
        {
            ArgumentNullException.ThrowIfNull(@for);

            Type = type;
            SearchFieldType = searchFieldType;

            PropertyName = @for.GetSelectedProperty().Name;
            PropertyType = @for.GetSelectedProperty().PropertyType;
            LanguageKey = StringLocalizerExtensions.GetPropertyNameKey(@for);
            ValidationProperty = @for;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public FormFieldType Type { get; set; }

        public Expression<Func<object?>>? ValidationProperty { get; set; }

        /// <summary>
        /// The name of the property on the view model
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The name of the property to use when filtering by the current form field. Defaults to <see cref="PropertyName"/> if not set.
        /// </summary>
        public string FilterPropertyName
        {
            get
            {
                return _filterPropertyName ?? PropertyName;
            }
            set
            {
                _filterPropertyName = value;
            }
        }
        private string? _filterPropertyName;

        public string LanguageKey { get; set; }

        public string DescriptionLanguageKey => LanguageKey + "_desc";

        public Type PropertyType { get; set; }

        public Type? FilterType { get; set; }

        /// <summary>
        /// Refers to the type used internally when searching/filtering, for more complex implementations.
        /// </summary>
        /// <remarks>
        /// This usually isn't needed, and <see cref="PropertyType"/> is used if this is unset.
        /// </remarks>
        public Type? SearchFieldType { get; set; }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            }
        }
        private bool _isVisible = true;

        public bool ReadOnly { get; set; }

        public bool Disabled { get; set; }

        /// <summary>
        /// Whether the form field starts collapsed. The user may collapse or uncollapse afterward regardless.
        /// </summary>
        /// <remarks>
        /// If the form field is expected to take up a large amount of space, it may be a good idea to set this to True, such as if a list has dozens of items.
        /// </remarks>
        public bool Collapsed { get; set; }

        public bool Sortable { get; set; } = true;

        public SortDirection DefaultSortDirection { get; set; }

        public bool Filterable { get; set; } = true;

        public Action<object?>? OnValueChanged { get; set; }
    }

    public static class FormFieldDefinitionExtensions
    {
        public static T WithCustomLanguageKey<T>(this T field, string? customLanguageKey) where T : FormFieldDefinition
        {
            field.LanguageKey = customLanguageKey ?? field.LanguageKey;
            return field;
        }
    }
}
