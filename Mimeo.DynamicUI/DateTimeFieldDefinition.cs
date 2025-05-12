using System.Linq.Expressions;

namespace Mimeo.DynamicUI
{
    /// <summary>
    /// A form field representing a date/time. Note that the raw value is expected to be in UTC, though it can be presented to the user differently
    /// </summary>
    public class DateTimeFieldDefinition : FormFieldDefinition
    {
        public DateTimeFieldDefinition(Expression<Func<object?>> @for) : base(FormFieldType.DateTime, @for)
        {
        }

        public bool ShowTime { get; set; }

        public DateDisplayMode DisplayMode { get; set; }
    }
}
