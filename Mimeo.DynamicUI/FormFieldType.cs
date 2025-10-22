namespace Mimeo.DynamicUI
{
    public enum FormFieldType
    {
        Hidden = 0,

        /// <summary>
        /// A field that accepts user-entered text
        /// </summary>
        Text, 

        /// <summary>
        /// A field that accepts user-entered text but presents some pre-defined values
        /// </summary>
        Combobox,

        Checkbox,

        /// <summary>
        /// A field that presents the user with pre-defined value and allows selection of a single value
        /// </summary>
        SingleSelect,

        /// <summary>
        /// A field that presents the user with a dropdown to select a single pre-defined value
        /// </summary>
        SingleSelectDropdown,

        /// <summary>
        /// A field that presents the user with a dropdown to select a single value from a data source
        /// </summary>
        SingleSelectDataSourceDropdown,

        /// <summary>
        /// A field that presents the user with pre-defined value and allows selection of multiple values
        /// </summary>
        MultiSelect,

        /// <summary>
        /// A field that presents the user with a dropdown to select one or more pre-defined values
        /// </summary>
        MultiSelectDropdown,

        /// <summary>
        /// A field that presents the user with values that come from a data source and allows selection of multiple values
        /// </summary>
        MultiSelectDataSourceDropdown,

        Date,
        Time,
        DateTime,
        Color,
        Integer,
        Decimal, 

        [Obsolete("Use Table, SectionList, or ReorderableSectionList instead")]
        List,

        /// <summary>
        /// Presents a list of items or view models in a table
        /// </summary>
        Table,

        /// <summary>
        /// Presents a list of items or view models as a sequential list of sub-editors
        /// </summary>
        SectionList,

        /// <summary>
        /// Presents a list of items or view models as a sequential list of sub-editors, with the ability to reorder items
        /// </summary>
        ReorderableSectionList,

        /// <summary>
        /// Presents another form field, wrapped with a checkbox that is meant to indicate whether the value is null
        /// </summary>
        Nullable,

        Guid,
        Section,
        Custom
    }
}
