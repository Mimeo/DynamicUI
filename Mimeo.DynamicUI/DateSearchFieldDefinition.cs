using System.Linq.Expressions;

namespace Mimeo.DynamicUI
{
    /// <summary>
    /// A special form field representing just a date, relatively or absolutely. The same general rules as <see cref="DateTimeFieldDefinition"/> apply.
    /// </summary>
    /// <remarks>
    /// Only ever use this when implementing <see cref="ViewModel.GetSearchFormFields"/>, or suffer the consequences.
    /// </remarks>
    public class DateSearchFieldDefinition(Expression<Func<object?>> @for, DateDisplayMode dateDisplayMode = DateDisplayMode.Raw)
        : FormFieldDefinition(FormFieldType.Date, @for, typeof(DateFilter))
    {
        public DateDisplayMode DisplayMode { get; set; } = dateDisplayMode;
    }
}
