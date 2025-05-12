using Mimeo.DynamicUI.Extensions;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI
{
    public class DecimalFieldDefinition : FormFieldDefinition
    {
        public DecimalFieldDefinition(Expression<Func<decimal?>> @for)
            : base(FormFieldType.Decimal, LinqExtensions.Cast<decimal?, object?>(@for))
        {
        }

        public DecimalFieldDefinition(Expression<Func<decimal>> @for)
            : base(FormFieldType.Decimal, LinqExtensions.Cast<decimal, object?>(@for))
        {
        }

        public int? DecimalPlaces { get; set; }
    }
}
