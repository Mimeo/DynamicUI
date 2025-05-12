using System.Linq.Expressions;

namespace Mimeo.DynamicUI
{
    /// <summary>
    /// A form field whose UI will be implemented by the client application
    /// </summary>
    public class CustomFormFieldDefinition : FormFieldDefinition
    {
        public CustomFormFieldDefinition(Expression<Func<object?>> @for) : base(FormFieldType.Custom, @for)
        {
        }
    }
}
