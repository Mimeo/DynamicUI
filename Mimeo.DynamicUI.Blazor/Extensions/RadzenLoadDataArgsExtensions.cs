using Mimeo.DynamicUI.Data;
using Radzen;

namespace Mimeo.DynamicUI.Blazor.Extensions
{
    public static class RadzenLoadDataArgsExtensions
    {
        public static DataQuery ToODataQuery(this LoadDataArgs args, ViewModel viewModel)
        {
            var query = new DataQuery
            {
                Skip = args.Skip,
                Top = args.Top,
            };
            foreach (var filter in args.Filters ?? [])
            {
                var formFieldDefinition = viewModel.GetListForm().Values.SingleOrDefault(f => f.PropertyName == filter.Property);
                if (formFieldDefinition == null)
                {
                    // Attempt to guess if we can't find it
                    formFieldDefinition = new FormFieldDefinition(className: "", filter.FilterValue?.GetType() ?? typeof(string), filter.Property);
                }

                if (filter.FilterValue != null)
                {
                    query.Filters.Add(new DataQueryFilter(new DataFieldDefinition(formFieldDefinition), ToDataFilterOperator(filter.FilterOperator), filter.FilterValue));
                }
                if (filter.SecondFilterValue != null)
                {
                    query.Filters.Add(new DataQueryFilter(new DataFieldDefinition(formFieldDefinition), ToDataFilterOperator(filter.SecondFilterOperator), filter.SecondFilterValue));
                }
            }
            foreach (var sort in args.Sorts ?? [])
            {
                var formFieldDefinition = viewModel.GetListForm().Values.SingleOrDefault(f => f.PropertyName == sort.Property);
                if (formFieldDefinition == null)
                {
                    // Attempt to guess if we can't find it
                    formFieldDefinition = new FormFieldDefinition(className: "", typeof(string), sort.Property);
                }

                query.Sorts.Add(new DataQuerySort(new DataFieldDefinition(formFieldDefinition), sort.SortOrder == SortOrder.Descending));
            }
            return query;
        }


        public static DataQuery ToODataQuery(this LoadDataArgs args, Type viewModelType)
        {
            FormFieldDefinition getFormField(string propertyName, object? filterValue)
            {
                var property = viewModelType.GetProperty(propertyName);

                if (property != null)
                {
                    return new FormFieldDefinition(viewModelType.Name, property.PropertyType, propertyName);
                }
                else
                {
                    // Attempt to guess the type if we can't find the property
                    return new FormFieldDefinition(viewModelType.Name, filterValue?.GetType() ?? typeof(string), propertyName);
                }
            }

            var query = new DataQuery
            {
                Skip = args.Skip,
                Top = args.Top,
            };
            foreach (var filter in args.Filters ?? [])
            {
                if (filter.FilterValue != null)
                {
                    query.Filters.Add(new DataQueryFilter(new DataFieldDefinition(getFormField(filter.Property, filter.FilterValue)), ToDataFilterOperator(filter.FilterOperator), filter.FilterValue));
                }
                if (filter.SecondFilterValue != null)
                {
                    query.Filters.Add(new DataQueryFilter(new DataFieldDefinition(getFormField(filter.Property, filter.SecondFilterValue)), ToDataFilterOperator(filter.SecondFilterOperator), filter.SecondFilterValue));
                }
            }

            foreach (var sort in args.Sorts ?? [])
            {
                query.Sorts.Add(new DataQuerySort(new DataFieldDefinition(getFormField(sort.Property, null)), sort.SortOrder == SortOrder.Descending));
            }
            return query;
        }

        public static DataQuery ToODataQuery<TViewModel>(this LoadDataArgs args) => ToODataQuery(args, typeof(TViewModel));

        private static DataFilterOperator ToDataFilterOperator(FilterOperator filterOperator)
        {
            return filterOperator switch
            {
                FilterOperator.Equals => DataFilterOperator.Equals,
                FilterOperator.NotEquals => DataFilterOperator.NotEquals,
                FilterOperator.LessThan => DataFilterOperator.LessThan,
                FilterOperator.LessThanOrEquals => DataFilterOperator.LessThanOrEquals,
                FilterOperator.GreaterThan => DataFilterOperator.GreaterThan,
                FilterOperator.GreaterThanOrEquals => DataFilterOperator.GreaterThanOrEquals,
                FilterOperator.Contains => DataFilterOperator.Contains,
                FilterOperator.StartsWith => DataFilterOperator.StartsWith,
                FilterOperator.EndsWith => DataFilterOperator.EndsWith,
                FilterOperator.DoesNotContain => DataFilterOperator.DoesNotContain,
                FilterOperator.In => DataFilterOperator.In,
                FilterOperator.NotIn => DataFilterOperator.NotIn,
                FilterOperator.IsNull => DataFilterOperator.IsNull,
                FilterOperator.IsEmpty => DataFilterOperator.IsEmpty,
                FilterOperator.IsNotNull => DataFilterOperator.IsNotNull,
                FilterOperator.IsNotEmpty => DataFilterOperator.IsNotEmpty,
                FilterOperator.Custom => DataFilterOperator.Custom,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
