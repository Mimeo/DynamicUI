using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI.Blazor.FormFields;

public class DynamicField : ComponentBase
{
    private static readonly Dictionary<FormFieldType, Type> formFieldTypeMap = new()
    {
        { FormFieldType.Text, typeof(TextField) },
        { FormFieldType.Checkbox, typeof(CheckboxField) },
        { FormFieldType.SingleSelect, typeof(SingleSelectField) },
        { FormFieldType.MultiSelect, typeof(MultiSelectField) },
        { FormFieldType.SingleSelectDropdown, typeof(SingleSelectDropDownField) },
        { FormFieldType.SingleSelectDataSourceDropdown, typeof(SingleSelectDropDownField) },
        { FormFieldType.MultiSelectDropdown, typeof(MultiSelectDropDownField) },
        { FormFieldType.MultiSelectDataSourceDropdown, typeof(MultiSelectDropDownField) },
        { FormFieldType.Color, typeof(ColorField) },
        { FormFieldType.Date, typeof(DateSearchField) },
        { FormFieldType.Time, typeof(TimeField) },
        { FormFieldType.DateTime, typeof(DateTimeField) },
        { FormFieldType.Integer, typeof(IntegerField) },
        { FormFieldType.Decimal, typeof(DecimalField) },
        { FormFieldType.List, typeof(ListField<>) },
        { FormFieldType.Nullable, typeof(NullableFormField) },
        { FormFieldType.Guid, typeof(GuidFormField) }
    };

    [Parameter]
    public FormFieldDefinition? FormField { get; set; }

    [Parameter]
    public object? ViewModel { get; set; }

    [Parameter]
    public Expression<Func<object?>>? For { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool? ReadOnly { get; set; }

    [Parameter]
    public bool? Disabled { get; set; }

    /// <summary>
    /// For form fields of type <see cref="FormFieldType.Custom"/>, a dictionary matching form field property language keys to controls that can display them.
    /// Controls must inherit <see cref="FormFieldBase{TValue}"/>.
    /// </summary>
    [Parameter]
    public Dictionary<string, Type> CustomFormFieldTypes { get; set; } = [];

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        if (FormField == null)
        {
            return;
        }

        Type? componentType;
        if (FormField.Type == FormFieldType.Custom)
        {
            componentType = CustomFormFieldTypes.GetValueOrDefault(FormField.LanguageKey);
            if (componentType == null)
            {
                builder.AddContent(0, $"Could not find UI implementation for '{FormField.LanguageKey}'. Please define one using {nameof(CustomFormFieldTypes)}.");
                return;
            }
            if (!InheritsFormFieldBase(componentType))
            {
                builder.AddContent(0, $"Component type '{componentType}' must inherit Mimeo.DynamicUI.Blazor.FormFields.FormFieldBase.");
                return;
            }
        }
        else
        {
            componentType = formFieldTypeMap.GetValueOrDefault(FormField.Type); 
            if (componentType == null)
            {
                builder.AddContent(0, $"Could not find UI implementation for form field type '{FormField.Type}'.");
                return;
            }
        }

        if (componentType.IsGenericType)
        {
            // Feed all generic type parameters from the underlying property to the control
            // So a HypotheticalControl<T1, T2, T3> can show a CustomClass<TA, TB, TC> 
            componentType = componentType.MakeGenericType(FormField.PropertyType.GetGenericArguments());
        }

        builder.OpenComponent(0, componentType);
        builder.AddAttribute(1, nameof(FormFieldBase<object>.Definition), FormField);
        if (ViewModel != null)
        {
            builder.AddAttribute(2, nameof(FormFieldBase<object>.ViewModel), ViewModel);
        }
        if (For != null)
        {
            builder.AddAttribute(3, nameof(FormFieldBase<object>.For), For);
        }
        if (ReadOnly.HasValue)
        {
            builder.AddAttribute(4, nameof(FormFieldBase<object>.ReadOnly), ReadOnly);
        }
        if (Disabled.HasValue)
        {
            builder.AddAttribute(5, nameof(FormFieldBase<object>.Disabled), Disabled);
        }
        if (!string.IsNullOrEmpty(Class))
        {
            builder.AddAttribute(6, "class", Class);
        }
        builder.CloseComponent();
    }

    private bool InheritsFormFieldBase(Type type)
    {
        // The built-in methods don't support unbound generics (or I haven't found the correct one yet)

        if (type.BaseType == null)
        {
            // Recursive base case
            return false;
        }

        if (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(FormFieldBase<>))
        {
            return true;
        }

        return InheritsFormFieldBase(type.BaseType);
    }
}