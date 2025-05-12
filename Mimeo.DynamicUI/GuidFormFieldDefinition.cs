using Mimeo.DynamicUI.Extensions;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI
{
    public class GuidFormFieldDefinition : FormFieldDefinition
    {
        public GuidFormFieldDefinition(Expression<Func<Guid>> @for) : base(FormFieldType.Guid, LinqExtensions.Cast<Guid, object?>(@for))
        {
        }
    }
}
