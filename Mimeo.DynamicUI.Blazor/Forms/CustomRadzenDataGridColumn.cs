using Radzen.Blazor;
using Radzen;

namespace Mimeo.DynamicUI.Blazor.Forms;

public class CustomRadzenDataGridColumn<TItem> : RadzenDataGridColumn<TItem>
{
    private static readonly IReadOnlyDictionary<FilterOperator, string> DataFilterOperators = new Dictionary<FilterOperator, string>
    {
        {FilterOperator.Equals, "eq"},
        {FilterOperator.NotEquals, "ne"},
        {FilterOperator.LessThan, "lt"},
        {FilterOperator.LessThanOrEquals, "le"},
        {FilterOperator.GreaterThan, "gt"},
        {FilterOperator.GreaterThanOrEquals, "ge"},
        {FilterOperator.StartsWith, "startswith"},
        {FilterOperator.EndsWith, "endswith"},
        {FilterOperator.Contains, "contains"},
        {FilterOperator.DoesNotContain, "DoesNotContain"},
        {FilterOperator.IsNull, "eq"},
        {FilterOperator.IsEmpty, "eq"},
        {FilterOperator.IsNotNull, "ne"},
        {FilterOperator.IsNotEmpty, "ne"}
    };

    protected override string GetColumnODataFilter(object filterValue, FilterOperator filterOperator)
    {
        var odataOperator = DataFilterOperators[filterOperator];
        var property = GetFilterProperty().Replace('.', '/');

        if (FilterPropertyType != typeof(List<string>) || filterValue is not string) return base.GetColumnODataFilter(filterValue, filterOperator);

        return filterOperator switch
        {
            FilterOperator.Contains or FilterOperator.StartsWith or FilterOperator.EndsWith => 
                $"{property}/any(x: {odataOperator}(x,'{filterValue}'))",

            FilterOperator.DoesNotContain => 
                $"{property}/any(x: indexof(x,'{filterValue}') eq -1)",

            _ => $"{property}/any(x: x {odataOperator} '{filterValue}')"
        };
    }
}