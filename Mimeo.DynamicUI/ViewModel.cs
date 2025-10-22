using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Extensions;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Mimeo.DynamicUI
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual object? GetValue(FormFieldDefinition field)
        {
            var property = this.GetType().GetProperty(field.PropertyName);
            if (property == null)
            {
                return null;
            }

            return property.GetValue(this);
        }

        public virtual void SetValue(FormFieldDefinition field, object? value)
        {
            var property = this.GetType().GetProperty(field.PropertyName);
            if (property == null)
            {
                return;
            }

            property.SetValue(this, value);
            field.OnValueChanged?.Invoke(value);
            RaisePropertyChanged(field.PropertyName);
        }

        /// <summary>
        /// Gets fields that should show up in a list or grid containing serveral view models of the same type
        /// </summary>
        public Dictionary<string, FormFieldDefinition> GetListForm()
        {
            _listForm ??= GetListFormFields().ToDictionary(f => f.PropertyName, f => f);
            return _listForm;
        }
        private Dictionary<string, FormFieldDefinition>? _listForm;

        /// <summary>
        /// Gets fields that should show up in a list or grid containing serveral view models of the same type
        /// </summary>
        public Dictionary<string, FormFieldDefinition> GetDropDownListForm()
        {
            _dropdownListForm ??= GetDropDownListFormFields().ToDictionary(f => f.PropertyName, f => f);
            return _dropdownListForm;
        }
        private Dictionary<string, FormFieldDefinition>? _dropdownListForm;

        public Dictionary<string, FormFieldDefinition> GetEditForm()
        {
            _editForm ??= GetEditFormFields().ToDictionary(f => f.PropertyName, f => f);
            return _editForm;
        }
        private Dictionary<string, FormFieldDefinition>? _editForm;

        public Dictionary<string, FormFieldDefinition> GetSearchForm()
        {
            _searchForm ??= GetSearchFormFields().ToDictionary(f => f.PropertyName, f => f);
            return _searchForm;
        }
        private Dictionary<string, FormFieldDefinition>? _searchForm;

        public Dictionary<string, FormFieldDefinition> GetViewForm()
        {
            _view ??= GetViewFields().ToDictionary(f => f.PropertyName, f => f);
            return _view;
        }
        private Dictionary<string, FormFieldDefinition>? _view;

        /// <summary>
        /// Gets fields that show up in a read-only list of this view model
        /// </summary>
        protected virtual IEnumerable<FormFieldDefinition> GetListFormFields()
        {
            return GetEditFormFields();
        }

        /// <summary>
        /// Gets fields that show up in a read-only dropdown list of this view model
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="GetListFormFields"/>, but the available space is much smaller so fewer columns should be included
        /// </remarks>
        protected virtual IEnumerable<FormFieldDefinition> GetDropDownListFormFields()
        {
            return GetListFormFields();
        }

        /// <summary>
        /// Gets fields that show up in a read-only view of this view model
        /// </summary>
        protected virtual IEnumerable<FormFieldDefinition> GetViewFields()
        {
            return GetEditFormFields();
        }

        /// <summary>
        /// Gets fields that show up in a search form, even if the data is unavailable for a list view
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<FormFieldDefinition> GetSearchFormFields()
        {
            return GetListFormFields();
        }

        /// <summary>
        /// Gets fields that should show up in an edit form for a single view model
        /// </summary>
        protected virtual IEnumerable<FormFieldDefinition> GetEditFormFields()
        {
            return Enumerable.Empty<FormFieldDefinition>();
        }

        public FormFieldDefinition FormField(FormFieldType type, Expression<Func<object?>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new FormFieldDefinition(type, @for) 
            { 
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<string?>> @for, TextType textType = TextType.SingleLine, string? codeLanguage = null, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null, List<string>? items = null)
        {
            return new TextFieldDefinition(@for)
            {
                CodeLanguage = codeLanguage,
                TextType = textType,
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
                Items = items
            }.WithCustomLanguageKey(customLanguageKey);
        }


        public FormFieldDefinition FormField(Expression<Func<bool?>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new FormFieldDefinition(FormFieldType.Checkbox, LinqExtensions.Cast<bool?, object?>(@for))
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<bool>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new FormFieldDefinition(FormFieldType.Checkbox, LinqExtensions.Cast<bool, object?>(@for))
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<int>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new FormFieldDefinition(FormFieldType.Integer, LinqExtensions.Cast<int, object?>(@for))
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<int?>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new FormFieldDefinition(FormFieldType.Integer, LinqExtensions.Cast<int?, object?>(@for))
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<Guid>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new FormFieldDefinition(FormFieldType.Guid, LinqExtensions.Cast<Guid, object?>(@for))
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<decimal?>> @for, int? decimalPlaces = 0, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new DecimalFieldDefinition(@for)
            {
                DecimalPlaces = decimalPlaces ?? 0,
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<decimal>> @for, int? decimalPlaces = 0, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new DecimalFieldDefinition(@for)
            {
                DecimalPlaces = decimalPlaces ?? 0,
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<ViewModel>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new SectionFormFieldDefinition(@for)
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        [Obsolete("Use Table(...), SectionList(...), or ReorderableSectionList(...) instead")]
        public FormFieldDefinition FormField<T>(Expression<Func<List<T>>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null, Func<T>? newItemCreator = null, ListFieldPresentationMode? mode = null)
        {
            return new ListFieldDefinition<T>(@for)
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
                NewItemCreator = newItemCreator,
                PresentationMode = mode ?? ListFieldPresentationMode.Table,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        [Obsolete("Use Table(...), SectionList(...), or ReorderableSectionList(...) instead")]
        public FormFieldDefinition FormField<TViewModel>(Expression<Func<List<TViewModel>>> @for, Func<TViewModel, FormFieldDefinition> headerField, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null, Func<TViewModel>? newItemCreator = null, ListFieldPresentationMode? mode = null)
        {
            return new ListFieldDefinition<TViewModel>(@for)
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
                NewItemCreator = newItemCreator,
                PresentationMode = mode ?? ListFieldPresentationMode.ReorderableSectionList,
                HeaderField = headerField
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition Table<T>(Expression<Func<List<T>>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null, Func<T>? newItemCreator = null)
        {
            return new ListFieldDefinition<T>(FormFieldType.Table, @for)
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
                NewItemCreator = newItemCreator
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition Table<TViewModel>(Expression<Func<List<TViewModel>>> @for, Func<TViewModel, FormFieldDefinition> headerField, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null, Func<TViewModel>? newItemCreator = null)
        {
            return new ListFieldDefinition<TViewModel>(FormFieldType.Table, @for)
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
                NewItemCreator = newItemCreator,
                HeaderField = headerField
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition SectionList<T>(Expression<Func<List<T>>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null, Func<T>? newItemCreator = null)
        {
            return new ListFieldDefinition<T>(FormFieldType.SectionList, @for)
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
                NewItemCreator = newItemCreator
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition SectionList<TViewModel>(Expression<Func<List<TViewModel>>> @for, Func<TViewModel, FormFieldDefinition> headerField, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null, Func<TViewModel>? newItemCreator = null)
        {
            return new ListFieldDefinition<TViewModel>(FormFieldType.SectionList, @for)
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
                NewItemCreator = newItemCreator,
                HeaderField = headerField
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition ReorderableSectionList<T>(Expression<Func<List<T>>> @for, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null, Func<T>? newItemCreator = null)
        {
            return new ListFieldDefinition<T>(FormFieldType.ReorderableSectionList, @for)
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
                NewItemCreator = newItemCreator
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition ReorderableSectionList<TViewModel>(Expression<Func<List<TViewModel>>> @for, Func<TViewModel, FormFieldDefinition> headerField, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null, Func<TViewModel>? newItemCreator = null)
        {
            return new ListFieldDefinition<TViewModel>(FormFieldType.ReorderableSectionList, @for)
            {
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
                NewItemCreator = newItemCreator,
                HeaderField = headerField
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<DateTime>> @for, bool showTime = true, DateDisplayMode dateDisplayMode = DateDisplayMode.Utc, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new DateTimeFieldDefinition(LinqExtensions.Cast<DateTime, object?>(@for))
            {
                ShowTime = showTime,
                DisplayMode = dateDisplayMode,
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<DateTime?>> @for, bool showTime = true, DateDisplayMode dateDisplayMode = DateDisplayMode.Utc, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new DateTimeFieldDefinition(LinqExtensions.Cast<DateTime?, object?>(@for))
            {
                ShowTime = showTime,
                DisplayMode = dateDisplayMode,
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<DateTimeOffset>> @for, bool showTime = true, DateDisplayMode dateDisplayMode = DateDisplayMode.Utc, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new DateTimeFieldDefinition(LinqExtensions.Cast<DateTimeOffset, object?>(@for))
            {
                ShowTime = showTime,
                DisplayMode = dateDisplayMode,
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }

        public FormFieldDefinition FormField(Expression<Func<DateTimeOffset?>> @for, bool showTime = true, DateDisplayMode dateDisplayMode = DateDisplayMode.Utc, bool readOnly = false, bool sortable = true, bool collapsed = false, SortDirection defaultSort = SortDirection.None, bool filterable = true, string? customLanguageKey = null)
        {
            return new DateTimeFieldDefinition(LinqExtensions.Cast<DateTimeOffset?, object?>(@for))
            {
                ShowTime = showTime,
                DisplayMode = dateDisplayMode,
                ReadOnly = readOnly,
                Sortable = sortable,
                Collapsed = collapsed,
                DefaultSortDirection = defaultSort,
                Filterable = filterable,
            }.WithCustomLanguageKey(customLanguageKey);
        }
    }
}
