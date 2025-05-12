using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Extensions;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI
{
    public abstract class SelectFormFieldDefinition : FormFieldDefinition
    {
        public SelectFormFieldDefinition(FormFieldType type, Expression<Func<object?>> @for, List<ListItem> items)
            : base(type, @for)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        [Obsolete("Use overload with nameField and ValueField")]
        public SelectFormFieldDefinition(FormFieldType type, Expression<Func<object?>> @for, IListItemDataService itemSource)
            : base(type, @for)
        {
            ItemSource = itemSource ?? throw new ArgumentNullException(nameof(itemSource));
        }

        public SelectFormFieldDefinition(FormFieldType type, Expression<Func<object?>> @for, IReadOnlyDataService itemSource, FormFieldDefinition nameField, FormFieldDefinition valueField, DataQuery? dataQuery = null)
            : base(type, @for)
        {
            ItemSource = itemSource ?? throw new ArgumentNullException(nameof(itemSource));
            ItemSourceNameField = nameField ?? throw new ArgumentNullException(nameof(nameField));
            ItemSourceValueField = valueField ?? throw new ArgumentNullException(nameof(valueField));
            ItemSourceDataQuery = dataQuery;
        }

        /// <summary>
        /// The available items in the dropdown, or null if <see cref="ItemSource"/> is used instead
        /// </summary>
        public List<ListItem>? Items { get; set; }

        /// <summary>
        /// The source of the available items in the dropdown, or null if <see cref="Items"/> is used instead
        /// </summary>
        public IReadOnlyDataService? ItemSource { get; set; }

        /// <summary>
        /// If <see cref="ItemSource"/> is provided, the form field used to represent the name of the item
        /// </summary>
        public FormFieldDefinition? ItemSourceNameField { get; set; }

        /// <summary>
        /// If <see cref="ItemSource"/> is provided, the form field used to represent the value of the item
        /// </summary>
        public FormFieldDefinition? ItemSourceValueField { get; set; }

        /// <summary>
        /// If <see cref="ItemSource"/> is provided, the filters and sort order to merge with the user-defined filter
        /// </summary>
        /// <remarks>
        /// The filters will always be applied, but the sorts will only be applied if no other sort is specified
        /// </remarks>
        public DataQuery? ItemSourceDataQuery { get; set; }
    }

    public class SingleSelectFormFieldDefinition : SelectFormFieldDefinition
    {
        public SingleSelectFormFieldDefinition(Expression<Func<string?>> @for, List<ListItem> items, OrientationValue orientation = OrientationValue.Unspecified)
            : base(FormFieldType.SingleSelect, LinqExtensions.Cast<string?, object?>(@for), items)
        {
            Orientation = orientation;
        }

        public SingleSelectFormFieldDefinition(Expression<Func<string?>> @for, IEnumerable<string> items, OrientationValue orientation = OrientationValue.Unspecified)
            : base(FormFieldType.SingleSelect, LinqExtensions.Cast<string?, object?>(@for), items.Select(i => new ListItem(i)).ToList())
        {
            Orientation = orientation;
        }

        /// <param name="items">Key: the raw value, Value: the value's display name or a language string key</param>
        public SingleSelectFormFieldDefinition(Expression<Func<string?>> @for, IReadOnlyDictionary<string, string> items, OrientationValue orientation = OrientationValue.Unspecified)
            : base(FormFieldType.SingleSelect, LinqExtensions.Cast<string?, object?>(@for), items.Select(kv => new ListItem(kv.Value, kv.Key)).ToList())
        {
            Orientation = orientation;
        }

        public OrientationValue Orientation { get; set; }

        public enum OrientationValue
        {
            Unspecified = 0,
            Vertical,
            Horizontal,
        }
    }

    public class SingleSelectDropDownFormFieldDefinition : SelectFormFieldDefinition
    {
        public SingleSelectDropDownFormFieldDefinition(Expression<Func<string?>> @for, List<ListItem> items)
            : base(FormFieldType.SingleSelectDropdown, LinqExtensions.Cast<string?, object?>(@for), items)
        {
        }

        public SingleSelectDropDownFormFieldDefinition(Expression<Func<string?>> @for, IEnumerable<string> items)
            : base(FormFieldType.SingleSelectDropdown, LinqExtensions.Cast<string?, object?>(@for), items.Select(i => new ListItem(i)).ToList())
        {
        }

        /// <param name="items">Key: the raw value, Value: the value's display name or a language string key</param>
        public SingleSelectDropDownFormFieldDefinition(Expression<Func<string?>> @for, IReadOnlyDictionary<string, string> items)
            : base(FormFieldType.SingleSelectDropdown, LinqExtensions.Cast<string?, object?>(@for), items.Select(kv => new ListItem(kv.Value, kv.Key)).ToList())
        {
        }

        [Obsolete("Use overload with nameField and ValueField")]
        public SingleSelectDropDownFormFieldDefinition(Expression<Func<string?>> @for, IListItemDataService itemSource, DataQuery? dataQuery = null)
            : base(FormFieldType.SingleSelectDataSourceDropdown, LinqExtensions.Cast<string?, object?>(@for), itemSource)
        {
            ItemSource = itemSource ?? throw new ArgumentNullException(nameof(itemSource));
            ItemSourceDataQuery = dataQuery;
        }

        public SingleSelectDropDownFormFieldDefinition(Expression<Func<object?>> @for, IReadOnlyDataService itemSource, FormFieldDefinition nameField, FormFieldDefinition valueField, DataQuery? dataQuery = null)
            : base(FormFieldType.SingleSelectDataSourceDropdown, @for, itemSource, nameField, valueField, dataQuery)
        {
        }
    }

    public class MultiSelectFormFieldDefinition : SelectFormFieldDefinition
    {
        public MultiSelectFormFieldDefinition(Expression<Func<List<string>?>> @for, List<ListItem> items, OrientationValue orientation = OrientationValue.Unspecified)
            : base(FormFieldType.MultiSelect, LinqExtensions.Cast<List<string>?, object?>(@for), items)
        {
            Orientation = orientation;
        }

        public MultiSelectFormFieldDefinition(Expression<Func<List<string>?>> @for, IEnumerable<string> items, OrientationValue orientation = OrientationValue.Unspecified)
            : base(FormFieldType.MultiSelect, LinqExtensions.Cast<List<string>?, object?>(@for), items.Select(i => new ListItem(i)).ToList())
        {
        }

        public OrientationValue Orientation { get; set; }

        public enum OrientationValue
        {
            Unspecified = 0,
            Vertical,
            Horizontal,
        }
    }

    public class MultiSelectDropDownFormFieldDefinition : SelectFormFieldDefinition
    {
        public MultiSelectDropDownFormFieldDefinition(Expression<Func<List<string>?>> @for, List<ListItem> items)
            : base(FormFieldType.MultiSelectDropdown, LinqExtensions.Cast<List<string>?, object?>(@for), items)
        {
        }

        public MultiSelectDropDownFormFieldDefinition(Expression<Func<List<string>?>> @for, IEnumerable<string> items)
            : base(FormFieldType.MultiSelectDropdown, LinqExtensions.Cast<List<string>?, object?>(@for), items.Select(i => new ListItem(i)).ToList())
        {
        }

        [Obsolete("Use overload with nameField and ValueField")]
        public MultiSelectDropDownFormFieldDefinition(Expression<Func<List<string>?>> @for, IListItemDataService itemSource, DataQuery? dataQuery = null)
            : base(FormFieldType.MultiSelectDataSourceDropdown, LinqExtensions.Cast<List<string>?, object?>(@for), itemSource)
        {
            ItemSource = itemSource ?? throw new ArgumentNullException(nameof(itemSource));
            ItemSourceDataQuery = dataQuery;
        }

        public MultiSelectDropDownFormFieldDefinition(Expression<Func<List<string>?>> @for, IReadOnlyDataService itemSource, FormFieldDefinition nameField, FormFieldDefinition valueField, DataQuery? dataQuery = null)
            : base(FormFieldType.MultiSelectDataSourceDropdown, LinqExtensions.Cast<List<string>?, object?>(@for), itemSource, nameField, valueField, dataQuery)
        {
            if (nameField.PropertyType != typeof(string))
            {
                // because the name is supposed to be something human readable
                throw new ArgumentException("Name field must be a string field");
            }
            if (valueField.PropertyType != typeof(string))
            {
                // Ideally this could be any object, but the plumbing from here to the control becomes challenging
                throw new ArgumentException("Value field must be a string field");
            }
        }
    }
}
