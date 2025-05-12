using Microsoft.AspNetCore.Components;
using Mimeo.DynamicUI.Extensions;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI.Blazor.FormFields;

/// <summary>
/// Base class for a UI control that can edit a form field.
/// </summary>
/// <typeparam name="TValue">The type of data the form field expects to edit.</typeparam>
public abstract class FormFieldBase<TValue> : ComponentBase, IDisposable
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (Definition != null)
        {
            Definition.PropertyChanged -= OnPropertyChange; // Ensure we only have one handler (blazor can set parameters as side effects of the event handler)
            Definition.PropertyChanged += OnPropertyChange;
        }
        if (ViewModel != null && ViewModel is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged -= OnPropertyChange;
            notifyPropertyChanged.PropertyChanged += OnPropertyChange;
        }
    }

#pragma warning disable BL0007 // Component parameters should be auto properties

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public FormFieldDefinition? Definition { get; set; }

    [Parameter]
    public Expression<Func<object?>>? For { get; set; }

    [Parameter]
    public object? ViewModel { get; set; }

    [Parameter]
    public virtual string? Class { get; set; }

    [Parameter]
    public bool ReadOnly { get => _readOnly ?? Definition?.ReadOnly ?? false; set => _readOnly = value; }
    private bool? _readOnly;

    [Parameter]
    public bool Disabled { get => _disabled ?? Definition?.Disabled ?? false; set => _disabled = value; }
    private bool? _disabled;

    /// <summary>
    /// Gets or sets the current value of the form field.
    /// </summary>
    public TValue? Value
    {
        get
        {
            object? value;
            if (For != null && ViewModel != null)
            {
                value = For.GetSelectedProperty().GetValue(ViewModel);
            }
            else if (Definition != null && ViewModel is ViewModel dynamicUiViewModel)
            {
                value = dynamicUiViewModel.GetValue(Definition);
            }
            else
            {
                throw new InvalidOperationException("Either For and ViewModel need to be set, or Definition needs to be set and ViewModel be a Dynamic UI view model");
            }

            if (value == null)
            {
                var targetType = typeof(TValue);
                if (!targetType.IsClass && (!targetType.IsGenericType || targetType.GetGenericTypeDefinition() != typeof(Nullable<>)))
                {
                    // We're going to run into some weirdness with the return type of "TValue?"
                    // "?" on a class means it could return null, which all classes are capable of doing as reference types
                    // "?" on a struct is shorthand for Nullable<TValue>
                    // "?" on a generic type argument with no class/struct constraint is treated like a class
                    // If TValue is a struct and value is null, we'll get an error in the (TValue?)value conversion when we unbox value
                    value = Activator.CreateInstance<TValue>();
                }
            }

            return (TValue?)value;
        }
        set
        {
            if (For != null && ViewModel != null)
            {
                var property = For.GetSelectedProperty();
                property.SetValue(ViewModel, value);
                return;
            }
            else if (Definition != null && ViewModel is ViewModel dynamicUiViewModel)
            {
                dynamicUiViewModel.SetValue(Definition, value);
            }
            else
            {
                throw new InvalidOperationException("Either For and ViewModel need to be set, or Definition needs to be set and ViewModel be a Dynamic UI view model");
            }
        }
    }
#pragma warning restore BL0007

    protected void OnPropertyChange(object? sender, PropertyChangedEventArgs e)
    {
        StateHasChanged();
    }

    public virtual void Dispose()
    {
        if (Definition != null)
        {
            Definition.PropertyChanged -= OnPropertyChange;
        }
        if (ViewModel != null && ViewModel is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged -= OnPropertyChange;
        }
    }
}