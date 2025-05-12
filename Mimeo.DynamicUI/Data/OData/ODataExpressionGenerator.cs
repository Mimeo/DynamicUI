using Mimeo.DynamicUI.Extensions;
using System.Collections;

namespace Mimeo.DynamicUI.Data.OData
{
    public class ODataExpressionGenerator
    {
        public ODataExpressionGenerator(IDateTimeConverter dateTimeConverter)
        {
            this.dateTimeConverter = dateTimeConverter ?? throw new ArgumentNullException(nameof(dateTimeConverter));
        }

        protected readonly IDateTimeConverter dateTimeConverter;

        protected virtual string UtcDateTimeFormat => "yyyy-MM-ddTHH:mm:ss.ffffffZ";
        protected virtual string DateTimeOffsetFormat => "u";

        public string GenerateODataFilter(DataQuery dataQuery)
        {
            return GenerateODataFilter(dataQuery.Filter);
        }

        public virtual string GenerateODataFilter(DataQueryFilterGroup filterGroup)
        {
            return string.Join($" {GetConjunction(filterGroup.FiltersConjunction)} ", filterGroup.Filters.Select(GenerateODataFilterExpression).Where(f => !string.IsNullOrEmpty(f)));
        }

        protected virtual string GetConjunction(DataFilterConjunction filterConjunction)
        {
            return filterConjunction switch
            {
                DataFilterConjunction.And => "and",
                DataFilterConjunction.Or => "or",
                _ => throw new ArgumentException($"Unsupported conjunction '{filterConjunction}'", nameof(filterConjunction))
            };
        }

        public virtual string GenerateODataOrderBy(DataQuery query)
        {
            return string.Join(", ", query.Sorts.Select(GenerateODataOrderByExpression).Where(s => !string.IsNullOrEmpty(s)));
        }

        private static readonly Dictionary<DataFilterOperator, string> DataFilterOperators = new()
        {
            {DataFilterOperator.Equals, "eq"},
            {DataFilterOperator.NotEquals, "ne"},
            {DataFilterOperator.LessThan, "lt"},
            {DataFilterOperator.LessThanOrEquals, "le"},
            {DataFilterOperator.GreaterThan, "gt"},
            {DataFilterOperator.GreaterThanOrEquals, "ge"},
            {DataFilterOperator.StartsWith, "startswith"},
            {DataFilterOperator.EndsWith, "endswith"},
            {DataFilterOperator.Contains, "contains"},
            {DataFilterOperator.DoesNotContain, "DoesNotContain"},
            {DataFilterOperator.IsNull, "eq"},
            {DataFilterOperator.IsEmpty, "eq"},
            {DataFilterOperator.IsNotNull, "ne"},
            {DataFilterOperator.IsNotEmpty, "ne"},
            {DataFilterOperator.In, "in"},
            {DataFilterOperator.NotIn, "in"},
            {DataFilterOperator.Custom, ""}
        };

        /// <summary>
        /// Gets filter operators allowed for the given filter.
        /// The first item in the set should be the default
        /// </summary>
        public IEnumerable<DataFilterOperator> GetSupportedFilterOperators(DataFieldDefinition filter)
        {
            switch (filter.FormFieldDefinition.Type)
            {
                case FormFieldType.Text:
                    return new[]
                    {
                        DataFilterOperator.Contains,
                        DataFilterOperator.DoesNotContain,
                        DataFilterOperator.StartsWith,
                        DataFilterOperator.EndsWith,
                        DataFilterOperator.Equals,
                        DataFilterOperator.NotEquals
                    };
                case FormFieldType.SingleSelectDropdown:
                    return new[]
                    {
                        DataFilterOperator.Equals,
                        DataFilterOperator.NotEquals
                    };
                case FormFieldType.MultiSelectDropdown:
                    if (filter.IsCollection)
                    {
                        return new[]
                        {
                            DataFilterOperator.Contains,
                            DataFilterOperator.DoesNotContain,
                            DataFilterOperator.Equals,
                            DataFilterOperator.NotEquals
                        };
                    }
                    else
                    {
                        return new[]
                        {
                            DataFilterOperator.In,
                            DataFilterOperator.NotIn,
                            DataFilterOperator.Equals,
                            DataFilterOperator.NotEquals
                        }; 
                    }
                case FormFieldType.DateTime:
                case FormFieldType.Integer:
                case FormFieldType.Decimal:
                case FormFieldType.Date:
                case FormFieldType.Time:
                    return new[]
                    {
                        DataFilterOperator.Equals,
                        DataFilterOperator.NotEquals,
                        DataFilterOperator.LessThan,
                        DataFilterOperator.LessThanOrEquals,
                        DataFilterOperator.GreaterThan,
                        DataFilterOperator.GreaterThanOrEquals
                    };
                case FormFieldType.List:
                    var sampleItem = (filter.FormFieldDefinition as IListFieldDefinition)?.CreateNewItem();
                    if (sampleItem is ViewModel)
                    {
                        // UI will flatten view model properties and list them separately
                        return Array.Empty<DataFilterOperator>();
                    }

                    return new[]
                    {
                        DataFilterOperator.Contains,
                        DataFilterOperator.DoesNotContain,
                        DataFilterOperator.StartsWith,
                        DataFilterOperator.EndsWith,
                        DataFilterOperator.Equals,
                        DataFilterOperator.NotEquals,
                        DataFilterOperator.IsNull,
                        DataFilterOperator.IsEmpty
                    };
                case FormFieldType.Hidden:
                case FormFieldType.Section: // simply a logical grouping that has no value
                    return Array.Empty<DataFilterOperator>();
                case FormFieldType.Guid:
                case FormFieldType.Color:
                case FormFieldType.Nullable:
                case FormFieldType.Checkbox:
                default:
                    return new[]
                    {
                        DataFilterOperator.Equals,
                        DataFilterOperator.NotEquals
                    };
            }
        }

        public virtual string GenerateODataFilterExpression(DataQueryFilterBase filterBase)
        {
            if (filterBase is DataQueryFilter filter)
            {
                if (filter.FilterDefinition.IsInCollection)
                {
                    return GenerateCollectionItemExpression(filter);
                }

                return GenerateODataFilterExpression(
                    filter.FilterDefinition.Path.Last().FormFieldDefinition.FilterPropertyName,
                    filter.Operator.GetValueOrDefault(),
                    filter.FilterDefinition.FormFieldDefinition.FilterType ?? filter.FilterDefinition.FormFieldDefinition.PropertyType,
                    filter.Value,
                    filter.IgnoreCase,
                    filter.FilterDefinition.FormFieldDefinition.GetDateDisplayMode());
            }

            if (filterBase is DataQueryFilterGroup filterGroup)
            {
                return GenerateODataFilter(filterGroup);
            }

            throw new ArgumentOutOfRangeException("Unsupported filterBase type", nameof(filterBase));
        }

        public virtual string GenerateODataFilterExpression(string property, DataFilterOperator @operator, Type propertyType, object? propertyValue, bool ignoreCase, DateDisplayMode dateDisplayMode)
        {
            if (propertyValue is null && (@operator != DataFilterOperator.IsNull || @operator != DataFilterOperator.IsNotNull))
            {
                return "";
            }

            if (propertyValue is string propertyString && string.IsNullOrEmpty(propertyString) && (@operator != DataFilterOperator.IsEmpty || @operator != DataFilterOperator.IsNotEmpty))
            {
                return "";
            }

            if (IsEnum(propertyType) || IsNullableEnum(propertyType))
            {
                return GenerateEnumODataFilterExpression(property, @operator, propertyValue);
            }

            if (propertyType == typeof(string))
            {
                return GenerateStringODataFilterExpression(property, @operator, propertyType, propertyValue, ignoreCase);
            }

            if (IsEnumerable(propertyType) && propertyType != typeof(string))
            {
                return GenerateEnumerableODataFilter(property, @operator, propertyType, propertyValue, ignoreCase);
            }

            if (IsNumeric(propertyType))
            {
                return GenerateNumericODataFilterExpression(property, @operator, propertyValue);
            }

            if (propertyType == typeof(bool) || propertyType == typeof(bool?))
            {
                return GenerateBoolODataFilterExpression(property, @operator, propertyValue);
            }

            if (propertyType == typeof(DateTime) ||
                propertyType == typeof(DateTime?) ||
                propertyType == typeof(DateTimeOffset) ||
                propertyType == typeof(DateTimeOffset?))
            {
                var dateTimeValue = propertyValue;
                if (dateTimeValue is DateFilter dateFilter)
                {
                    dateTimeValue = dateFilter.ToDateTime();
                }

                return GenerateDateODataFilterExpression(property, @operator, dateTimeValue, dateDisplayMode);
            }

            if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
            {
                return GenerateGuidODataFilterExpression(property, @operator, propertyValue);
            }

            return "";
        }

        protected virtual string GenerateCollectionItemExpression(DataQueryFilter filter)
        {
            var collectionOperators = (CollectionOperator: "any", ComparisonOperator: "eq", Negator: string.Empty);
            if (filter.Operator is DataFilterOperator.NotEquals or DataFilterOperator.DoesNotContain)
            {
                collectionOperators = (CollectionOperator: "all", ComparisonOperator: "ne", Negator: "not ");
            }

            // Start from the base: ChildProperty eq 'value'
            int xCount = 0;
            var expression = GenerateODataFilterExpression(
                $"x{xCount}/" + filter.FilterDefinition.Path.Last().FormFieldDefinition.FilterPropertyName,
                filter.Operator.GetValueOrDefault(),
                filter.FilterDefinition.FormFieldDefinition.FilterType ?? filter.FilterDefinition.FormFieldDefinition.PropertyType,
                filter.Value,
                filter.IgnoreCase,
                (filter.FilterDefinition.FormFieldDefinition as DateTimeFieldDefinition)?.DisplayMode ?? DateDisplayMode.Raw);

            if (string.IsNullOrEmpty(expression))
            {
                return "";
            }

            // Wrap the expression in the parent property
            // SampleProperty/any(x1: x2/ChildProperty eq 'value')
            // SampleProperty/any(x1: x1/ChildProperty2/any(x2: x2/ChildProperty3 eq 'value'))
            foreach (var parent in filter.FilterDefinition.Path.Skip(1).Reverse().Skip(1))
            {
                expression = $"x{xCount + 1}/{parent.FormFieldDefinition.FilterPropertyName}/{collectionOperators.CollectionOperator}(x{xCount}: {expression})";
                xCount++;
            }
            expression = $"{filter.FilterDefinition.Path[0].FormFieldDefinition.FilterPropertyName}/{collectionOperators.CollectionOperator}(x{xCount}: {expression})";

            return expression;
        }

        protected virtual string GenerateEnumODataFilterExpression(string property, DataFilterOperator @operator, object? propertyValue)
        {
            return $"{property} {DataFilterOperators[@operator]} '{GetValueAsString(propertyValue)}'";
        }

        protected virtual string GenerateStringODataFilterExpression(string property, DataFilterOperator @operator, Type propertyType, object? propertyValue, bool ignoreCase)
        {
            if (ignoreCase)
            {
                property = $"tolower({property})";
            }

            if (propertyValue is string propertyValueString)
            {
                if (ignoreCase)
                {
                    propertyValueString = $"'{propertyValueString.ToLower()}'";
                }
                else
                {
                    propertyValueString = $"'{propertyValueString}'";
                }

                if (!string.IsNullOrEmpty(propertyValueString) && @operator == DataFilterOperator.Contains)
                {
                    return $"contains({property}, {propertyValueString})";
                }

                if (!string.IsNullOrEmpty(propertyValueString) && @operator == DataFilterOperator.DoesNotContain)
                {
                    return $"indexof({property}, {propertyValueString}) eq -1";
                }

                if (!string.IsNullOrEmpty(propertyValueString) && @operator == DataFilterOperator.StartsWith)
                {
                    return $"startswith({property}, {propertyValueString})";
                }

                if (!string.IsNullOrEmpty(propertyValueString) && @operator == DataFilterOperator.EndsWith)
                {
                    return $"endswith({property}, {propertyValueString})";
                }

                if (!string.IsNullOrEmpty(propertyValueString) && @operator == DataFilterOperator.Equals)
                {
                    return $"{property} eq {propertyValueString}";
                }

                if (!string.IsNullOrEmpty(propertyValueString) && @operator == DataFilterOperator.NotEquals)
                {
                    return $"{property} ne {propertyValueString}";
                }

                if (@operator == DataFilterOperator.IsNull || @operator == DataFilterOperator.IsNotNull)
                {
                    return $"{property} {DataFilterOperators[@operator]} null";
                }

                if (@operator == DataFilterOperator.IsEmpty || @operator == DataFilterOperator.IsNotEmpty)
                {
                    return $"{property} {DataFilterOperators[@operator]} ''";
                }
            }
            else if (propertyValue is IEnumerable<string> propertyValueEnumerable)
            {
                string propertyValueExpression = ignoreCase
                    ? $"({string.Join(",", propertyValueEnumerable.Select(v => $"'{v.ToLower()}'"))})"
                    : $"({string.Join(",", propertyValueEnumerable.Select(v => $"'{v}'"))})";

                if (@operator == DataFilterOperator.In)
                {
                    return $"{property} in {propertyValueExpression}";
                }

                if (@operator == DataFilterOperator.NotIn)
                {
                    // "not in" seems to not be an official verb
                    // https://stackoverflow.com/a/63644880
                    return $"{property} in {propertyValueExpression} eq false";
                }
            }

            return "";
        }

        protected virtual string GenerateEnumerableODataFilter(string property, DataFilterOperator @operator, Type propertyType, object? propertyValue, bool ignoreCase)
        {
            if (propertyValue is string)
            {
                var containsExpression = GenerateStringODataFilterExpression("i", @operator, propertyType, propertyValue as string, ignoreCase);
                return $"{property}/any(i: {containsExpression})";
            }

            var enumerableValue = ((IEnumerable)(propertyValue != null ? propertyValue : Enumerable.Empty<object>())).AsQueryable();

            var enumerableValueAsString = "(" + string.Join(",",
                (enumerableValue.ElementType == typeof(string) ? enumerableValue.Cast<string>().Select(i => $@"'{i}'").Cast<object>() : enumerableValue.Cast<object>())) + ")";

            var enumerableValueAsStringOrForAny = string.Join(" or ",
                (enumerableValue.ElementType == typeof(string)
                    ? enumerableValue.Cast<string>().Select(i => $@"i eq '{i}'").Cast<object>()
                    : enumerableValue.Cast<object>().Select(i => $@"i eq {i}").Cast<object>()));

            if (enumerableValue.Any() && @operator == DataFilterOperator.Contains)
            {
                return $"{property} in {enumerableValueAsString}";
            }

            if (enumerableValue.Any() && @operator == DataFilterOperator.DoesNotContain)
            {
                return $"not({property} in {enumerableValueAsString})";
            }

            if (enumerableValue.Any() && @operator == DataFilterOperator.In)
            {
                return $"{property}/any(i:{enumerableValueAsStringOrForAny})";
            }

            if (enumerableValue.Any() && @operator == DataFilterOperator.NotIn)
            {
                return $"not({property}/any(i: {enumerableValueAsStringOrForAny}))";
            }

            if (enumerableValue.Any() && @operator == DataFilterOperator.Equals)
            {
                return "(" + string.Join(" and ",
                               (enumerableValue.ElementType == typeof(string)
                                   ? enumerableValue.Cast<string>().Select(i => $@"{property}/any(i: i eq '{i}')").Cast<object>()
                                   : enumerableValue.Cast<object>().Select(i => $@"{property}/any(i: i eq {i})").Cast<object>()))
                           + ")";
            }

            if (enumerableValue.Any() && @operator == DataFilterOperator.NotEquals)
            {
                return "not (" + string.Join(" and ",
                                   (enumerableValue.ElementType == typeof(string)
                                       ? enumerableValue.Cast<string>().Select(i => $@"{property}/any(i: i eq '{i}')").Cast<object>()
                                       : enumerableValue.Cast<object>().Select(i => $@"{property}/any(i: i eq {i})").Cast<object>()))
                               + ")";
            }

            return "";
        }

        protected virtual string GenerateNumericODataFilterExpression(string property, DataFilterOperator @operator, object? propertyValue)
        {
            return $"{property} {DataFilterOperators[@operator]} {GetValueAsString(propertyValue)}";
        }

        protected virtual string GenerateBoolODataFilterExpression(string property, DataFilterOperator @operator, object? propertyValue)
        {
            var value = GetValueAsString(propertyValue);
            var dataFilterOperator = DataFilterOperators[@operator];
            if (@operator == DataFilterOperator.IsNull || @operator == DataFilterOperator.IsNotNull)
            {
                return $"{property} {dataFilterOperator} null";
            }

            if (@operator == DataFilterOperator.IsEmpty || @operator == DataFilterOperator.IsNotEmpty)
            {
                return $"{property} {dataFilterOperator} ''";
            }

            if (!string.IsNullOrEmpty(value))
            {
                return $"{property} eq {value.ToLower()}";
            }

            return "";
        }

        protected virtual string GenerateDateODataFilterExpression(string property, DataFilterOperator @operator, object? propertyValue, DateDisplayMode dateDisplayMode)
        {
            var dataFilterOperator = DataFilterOperators[@operator];
            if (@operator == DataFilterOperator.IsNull || @operator == DataFilterOperator.IsNotNull)
            {
                return $"{property} {dataFilterOperator} null";
            }

            if (@operator == DataFilterOperator.IsEmpty || @operator == DataFilterOperator.IsNotEmpty)
            {
                return $"{property} {dataFilterOperator} ''";
            }

            if (@operator == DataFilterOperator.Equals && propertyValue is DateTime startDateRaw)
            {
                var startDateLocal = dateTimeConverter.UtcToDisplay(startDateRaw, dateDisplayMode).Date;
                var endDateLocal = startDateLocal.AddDays(1);
                var startDateUtc = dateTimeConverter.DisplayToUtc(startDateLocal, dateDisplayMode);
                var endDateUtc = dateTimeConverter.DisplayToUtc(endDateLocal, dateDisplayMode);

                return $"({property} ge {startDateUtc.ToString(UtcDateTimeFormat)} and {property} lt {endDateUtc.ToString(UtcDateTimeFormat)})";
            }

            if (@operator == DataFilterOperator.Equals && propertyValue is DateTimeOffset startDateOffset)
            {
                var startDateLocal = dateTimeConverter.UtcToDisplay(startDateOffset.UtcDateTime, dateDisplayMode).Date;
                var endDateLocal = startDateLocal.AddDays(1);
                var startDateUtc = dateTimeConverter.DisplayToUtc(startDateLocal, dateDisplayMode);
                var endDateUtc = dateTimeConverter.DisplayToUtc(endDateLocal, dateDisplayMode);

                return $"({property} ge {startDateUtc.ToString(UtcDateTimeFormat)} and {property} lt {endDateUtc.ToString(UtcDateTimeFormat)})";
            }

            string valueAsString;
            if (propertyValue is DateTime propertyDateTime)
            {
                valueAsString = propertyDateTime.ToString(UtcDateTimeFormat);
            }
            else if (propertyValue is DateTimeOffset propertyDateTimeOffset)
            {
                valueAsString = propertyDateTimeOffset.UtcDateTime.ToString(UtcDateTimeFormat);
            }
            else
            {
                throw new ArgumentException($"Unexpected type for propertyValue: {propertyValue?.GetType()?.Name ?? "null"}", nameof(propertyValue));
            }

            return $"{property} {dataFilterOperator} {valueAsString}";
        }

        protected virtual string GenerateGuidODataFilterExpression(string property, DataFilterOperator @operator, object? propertyValue)
        {
            if (propertyValue is IEnumerable propertyValueEnumerable && propertyValue is not string /* strings are IEnumerable<char> */)
            {
                var propertyValueExpression = $"({string.Join(",", propertyValueEnumerable.Cast<object>().Select(v => v.ToString()))})";

                if (@operator == DataFilterOperator.In)
                {
                    return $"{property} in {propertyValueExpression}";
                }

                if (@operator == DataFilterOperator.NotIn)
                {
                    // "not in" seems to not be an official verb
                    // https://stackoverflow.com/a/63644880
                    return $"{property} in {propertyValueExpression} eq false";
                }

                throw new NotImplementedException($"Filter operator '{@operator}' not implemented for guid filtering");
            }
            else
            {
                return $"{property} {DataFilterOperators[@operator]} {GetValueAsString(propertyValue)}";
            }
        }

        public virtual string GenerateODataOrderByExpression(DataQuerySort sort)
        {
            return $"{sort.Field.ODataPath} {(sort.Descending ? "desc" : "asc")}";
        }

        protected string GetODataValue(object value)
        {
            ArgumentNullException.ThrowIfNull(nameof(value));

            return value switch
            {
                string => $"'{value}'",
                DateTimeOffset dateTimeOffset => dateTimeOffset.UtcDateTime.ToString("o"),
                DateTime dateTime => dateTime.ToString("o"),
                _ => value.ToString()!
            };
        }

        protected bool IsDate(DataQueryFilter filter) 
        {
            var filterType = filter.FilterDefinition.FormFieldDefinition.FilterType ?? filter.FilterDefinition.FormFieldDefinition.PropertyType;
            return
               filter.FilterDefinition.FormFieldDefinition.PropertyType == typeof(DateTime)
            || filter.FilterDefinition.FormFieldDefinition.PropertyType == typeof(DateTimeOffset);
        }

        protected static bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type) || typeof(IEnumerable<>).IsAssignableFrom(type);
        }

        protected static bool IsEnum(Type source)
        {
            if (source == null)
                return false;

            return source.IsEnum;
        }

        protected static bool IsNullableEnum(Type source)
        {
            if (source == null)
                return false;

            var u = Nullable.GetUnderlyingType(source);
            return (u != null) && u.IsEnum;
        }

        protected static bool IsNumeric(Type source)
        {
            if (source == null)
                return false;

            var type = source.IsGenericType ? source.GetGenericArguments()[0] : source;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        protected static string? GetValueAsString(object? value)
        {
            if (value == null)
            {
                return null;
            }

            var propertyType = value.GetType();
            if (IsEnumerable(propertyType) && propertyType != typeof(string))
            {
                return null;
            }

            return value.ToString();
        }
    }
}
