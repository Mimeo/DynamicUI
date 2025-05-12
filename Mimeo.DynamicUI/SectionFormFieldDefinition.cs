using Mimeo.DynamicUI.Extensions;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI
{
    public class SectionFormFieldDefinition : FormFieldDefinition
    {
        public SectionFormFieldDefinition(Expression<Func<ViewModel>> @for) : base(FormFieldType.Section, LinqExtensions.Cast<ViewModel, object?>(@for))
        {
            var value = @for.Compile().Invoke();
            if (value is ViewModel vm)
            {
                SectionViewModel = vm;
            }
            else
            {
                throw new ArgumentException("@for must resolve to a ViewModel", nameof(@for));
            }
        }

        public ViewModel SectionViewModel { get; }
    }
}
