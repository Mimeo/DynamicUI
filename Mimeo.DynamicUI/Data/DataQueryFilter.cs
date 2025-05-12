using Mimeo.DynamicUI.Extensions;
using System.Linq.Expressions;
using System.Text.Json;

namespace Mimeo.DynamicUI.Data
{
    public class DataQueryFilter : DataQueryFilterBase
    {
        public DataQueryFilter(DataFieldDefinition filterDefinition)
        {
            FilterDefinition = filterDefinition;
        }

        public DataQueryFilter(DataFieldDefinition filterDefinition, DataFilterOperator? @operator, object? value)
        {
            FilterDefinition = filterDefinition;
            Operator = @operator;
            Value = value;
        }

        public DataQueryFilter(string className, Type propertyType, string propertyName, DataFilterOperator? @operator, object? value)
            : this(new DataFieldDefinition(className, propertyType, propertyName), @operator, value)
        {
        }

        public DataQueryFilter(Type classType, Type propertyType, string propertyName, DataFilterOperator? @operator, object? value)
            : this(new DataFieldDefinition(classType, propertyType, propertyName), @operator, value)
        {
        }

        public DataQueryFilter(Expression<Func<object>> propertySelector, DataFilterOperator? @operator, object? value)
        {
            var selectedProperty = propertySelector.GetSelectedProperty();
            if (selectedProperty.DeclaringType == null)
            {
                throw new ArgumentException("Selected property must have a declaring type", nameof(propertySelector));
            }

            FilterDefinition = new DataFieldDefinition(selectedProperty.DeclaringType, selectedProperty.PropertyType, selectedProperty.Name);
            Operator = @operator;
            Value = value;
        }

        public DataFieldDefinition FilterDefinition { get; }

        public DataFilterOperator? Operator { get; set; }

        public object? Value { get; set; }

        public bool IgnoreCase { get; set; } = true;

        public override DataQueryFilterBase Clone()
        {
            return new DataQueryFilter(FilterDefinition, Operator, Value)
            {
                IgnoreCase = IgnoreCase
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not DataQueryFilter other)
            {
                return false;
            }

            return this.FilterDefinition.ODataPath == other.FilterDefinition.ODataPath
                && this.Operator == other.Operator
                && this.Value == other.Value
                && this.IgnoreCase == other.IgnoreCase;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FilterDefinition, Operator, Value, IgnoreCase);
        }
    }
}
