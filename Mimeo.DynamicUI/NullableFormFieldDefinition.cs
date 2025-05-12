using Mimeo.DynamicUI.Extensions;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI
{
    public class NullableFormFieldDefinition : FormFieldDefinition
    {
        public NullableFormFieldDefinition(Expression<Func<bool>> @for, string? label, FormFieldDefinition forFormField)
            : base(FormFieldType.Nullable, LinqExtensions.Cast<bool, object?>(@for))
        {
            this.NullCheckboxLabel = label;
            this.FormFieldDefinition = forFormField;
        }

        public NullableFormFieldDefinition(Expression<Func<bool>> @for, FormFieldDefinition forFormField)
            : base(FormFieldType.Nullable, LinqExtensions.Cast<bool, object?>(@for))
        {
            this.NullCheckboxLabel = null;
            this.FormFieldDefinition = forFormField;
        }

        public string? NullCheckboxLabel { get; set; }

        public FormFieldDefinition FormFieldDefinition { get; set; }
    }
}
