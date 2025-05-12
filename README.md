# Workshop Dynamic UI

Mimeo Workshop Dynamic UI is a Blazor library designed to create common form UIs automatically from view models.

## Features

- DynamicEditForm: a control that builds a form from a view model.
- ODataGrid: a wrapper around Radzen's DataGrid designed for OData that uses DynamicEditForm to quickly create simple UIs for
    - Viewing items
    - Sorting
    - Filtering
    - Paging
    - Editing
    - Deleting
    - Copying
    - CSV Import/Export
- Localization: Using resx files, form field names and descriptions are shown in the user's language.

## Installation

1. Install Radzen.Blazor using their [installation guide](https://blazor.radzen.com/get-started).
    - Be sure to install radzen components using builder.Services.AddRadzenComponents and adding `<RadzenComponents/>`. 
      At the time of writing, this is described in "6. Use Dialog, Notification, ContextMenu and Tooltip components".
2. Install BlazorPro as described in their [installation guide](https://github.com/EdCharbeneau/BlazorSize/wiki).
    - Be sure to install add `services.AddMediaQueryService();` to dependency injection and `<MediaQueryList>` around the root
3. Install the `Mimeo.DynamicUI` nuget package into the project with your models and view models.		
4. Install the `Mimeo.DynamicUI.Blazor` nuget package into the UI project.
5. Add a resources file with code generation.
6. In dependency injection, add
```csharp
builder.Services.AddLocalization();
builder.Services.AddSingleton<IStringLocalizer, StringLocalizer<Language>>(); // Substitute Language for the name of your resources file.
builder.Services.AddSingleton<IDateTimeConverter, DateTimeConverter>(); // Optionally substitute your own implementation of IDateTimeConverter. See Form Fields/DateTime below for details.
builder.Services.AddSingleton<TaskRunningService>();
```
7. Add `<script src="_content/Mimeo.DynamicUI.Blazor/DynamicUI.js"></script>` to your scripts section
8. Add `<link rel="stylesheet" href="_content/Mimeo.DynamicUI.Blazor/app.css" />` to your styles section
9. Add this to the scripts section, specifically before the blazor.webassembly.js script:
```
<script src="_content/BlazorMonaco/jsInterop.js"></script>
<script src="_content/BlazorMonaco/lib/monaco-editor/min/vs/loader.js"></script>
<script src="_content/BlazorMonaco/lib/monaco-editor/min/vs/editor/editor.main.js"></script>
```

## Usage

Refer to Mimeo.DynamicUI.Demo for examples.

The core of the library is the view model. Simply create a class that inherits Mimeo.DynamicUI.ViewModel, 
and inherit GetEditFormFields and optionally GetListFormFields (GetListFormFields defaults to GetEditFormFields if not overridden, which may be cumbersome for larger view models).

Inside GetEditFormFields and GetListFormFields, return form fields. The simplest way to do so is to use the helper methods on the base class, like so:
```csharp
public string? Name { get; set; }
public int Number { get; set; }
public decimal Decimal { get; set; }
public DateTime DateUtc { get; set; }

protected override IEnumerable<FormFieldDefinition> GetListFormFields()
{
    yield return FormField(() => Name);
    yield return FormField(() => Number);
    yield return FormField(() => Decimal);
    yield return FormField(() => DateUtc);
}
```

The above sample creates 4 form fields, one for text, one for integer, one for decimal, and one for date, each inferred from the type of the property.
Each form field type can be further customized as described below.

After defining form fields, add classname_propertyname and optionally classname_propertyname_desc (both lowercase) to the resources file to set the user-friendly names and descriptions.

To use the form, simply add a DynamicEditForm to a page and bind it to the view model. The form will automatically generate the fields and localize them.

```
<DynamicEditForm ViewModel="@ViewModel" />
```

Alternatively, open an edit modal with the form inside it using DynamicUI's extension methods to Radzen's DialogService.

```csharp
var result = await dialogService.OpenEditDialogAsync(dialogTitle, viewModel);
// Result is true if the user pressed the save button or false if they pressed the cancel button.
```

## Form Fields

Customization options that apply to all form fields:
- ReadOnly: If true, the field will be read-only, and some controls will render as text instead of html controls.
- Disabled: If true, the underlying control will be disabled.
- IsVisible: If false, the field will not be shown. If changed, it will be shown/hidden accordingly.
- OnValueChanged: A callback that is called when the value of the field changes. Useful for setting other fields' IsVisible property.
```csharp
var hiddenFormField = new FormFieldDefinition(FormFieldType.Text, () => HiddenString)
{
    IsVisible = EnableHiddenProperties
};

yield return new FormFieldDefinition(FormFieldType.Checkbox, () => EnableHiddenProperties)
{
    OnValueChanged = value =>
    {
        hiddenFormField.IsVisible = value is true;
    }
};
yield return hiddenFormField;
```

### Checkbox

A simple checkbox that currently has no specific customization.

### Color

A wrapper around Radzen's ColorPicker. The value is a CSS color such as `rgb(0, 255, 127)` or `#00ff7f`.

### DateTime

A date/time picker that can display in multiple time zones. Options:
- Raw - No conversion, the date is displayed as-is.
- Utc - The date is displayed in UTC. Note that at the time of writing, this is currently equivalent to Raw, as the date is expected to already be in UTC.
- UserLocal - The date is displayed in the user's local time zone. Note that at the time of writing, this only works for Blazor WebAssembly.
- ServerLocal - The date is displayed in the server's local time zone. Note that at the time of writing, this does not work for Blazor WebAssembly.

To customize the time zone conversion, inherit DateTimeConverter or implement IDateTimeConverter and add it to dependency injection as described in Installation.

This field supports both DateTime and DateTimeOffset, though DateTimeOffset will sometimes coerce the time zone information to UTC.

Specify the display mode in the form field definition like so:
```csharp
yield return FormField(() => DateUtc, dateDisplayMode: DateDisplayMode.UserLocal);
```

#### `DateSearchFieldDefinition`

This is a special DateTime form field (unofficially a "search" form field) that should only be used when implementing `ViewModel.GetSearchFormFields`.
Under no circumstances should it be used for edit forms, as it is intended for use only when filtering and will break. *You have been warned.*

It supports the same time zone configuration as the original DateTime form field, and it was built to allow for filtering on dates either relatively or absolutely.

```csharp
yield return new DateSearchFieldDefinition(() => DateUtc, dateDisplayMode: DateDisplayMode.UserLocal);
```

### Decimal

A number field designed for decimal values. Customize the number of decimal places like so:
```csharp
yield return FormField(() => Decimal, decimalPlaces: 2);
```

### Guid

A text field designed for Guid values. A button generates a new guid when pressed.

There are no special options for this field, but because this was designed for primary keys, it may be worth making it read-only if the initial value isn't set.

```csharp
yield return FormField(() => Id, readOnly: Id != default);
```

### Integer

A simple number field designed for integer values with no specific customization.

### Single-select Dropdown

A dropdown that allows the user to select a single string from a list.

```csharp
yield return new SingleSelectDropDownFormFieldDefinition(() => SingleSelect, SingleSelectItems);
```

There are 3 ways to specify the items:
- A list of strings
- A dictionary of strings, where the key is the value set to the underlying property, and the value is the display name.
- A list of DropDownItem, which the above dictionary gets translated to.

The display name for the last two options get run through the string localizer.

Note that this is intended for sets of items that are small enough to be stored in memory.
A future improvement could query these from a data service for larger lists, but this is not currently implemented.

### Multi-select Dropdown

Same as the single-select dropdown, but allows the user to select multiple strings and should be bound to a List<string>.

### Nullable

A boolean setting that enables or disabled an arbitrary form field, useful for values that are logically nullable or feature-switched.

```csharp
yield return new NullableFormFieldDefinition(() => NullableStringEnabled, FormField(() => NullableStringValue));
```

### Text

A field for user-entered text. There are multiple modes:
- SingleLine - the default
- MultiLine - a textarea
- HTML - a wrapper around Radzen's HtmlEditor
- Code - a wrapper around Monaco Editor, a web-based version of Visual Studio Code supporting JSON, HTML, CSS, and more.

```csharp
yield return FormField(() => Description, textType: TextType.MultiLine);
yield return FormField(() => JSON, textType: TextType.JSON, codeLanguage: "json");
```

### Time

A time picker that supports only the time of day. No time zone conversion is performed.

### Section

A sub view model displayed as its own section. Useful for logically grouping related fields or showing/hiding many fields at once.

```csharp
public SubViewModel Section { get; set; } = new();

// ...

yield return FormField(() => SubModels);

// ...

public class SubViewModel : ViewModel
{
    public string? Property1 { get; set; }
    public int Property2 { get; set; }

    protected override IEnumerable<FormFieldDefinition> GetEditFormFields()
    {
        yield return FormField(() => Property1);
        yield return FormField(() => Property2);
    }
}
```

### List

A list of sub view models. These are currently displayed in a grid, and the user can sort, add, edit, and delete items.
Note that this is intended for data sets small enough to be stored and edited entirely in memory.

```csharp

public List<SubViewModel> SubModels { get; set; } = new();

// ...

yield return FormField(() => SubModels);

// ...

public class SubViewModel : ViewModel
{
    public string? Property1 { get; set; }
    public int Property2 { get; set; }

    protected override IEnumerable<FormFieldDefinition> GetEditFormFields()
    {
        yield return FormField(() => Property1);
        yield return FormField(() => Property2);
    }
}
```

### Custom

Sometimes the above might not be enough for a very narrow use-case. Your own control can be provided in such a case.

In the view model, specify the form field similar to above
```csharp
yield return new CustomFormFieldDefinition(() => CustomFormField);
```

Then create your own control that inherits FormFieldBase<T>, where T is the type of property you want to edit. The base class has the `Value` property which provides access to the view model's value for that property.
E.g. if CustomFormField is an integer, T should be `int`, or if the control could be reused for lots of types, use `object` and handle the casting of `Value` yourself.

Then register the type with whichever control you're using.

```csharp
    protected Dictionary<string, Type> CustomFormFieldDefinitions = new()
    {
        ["testviewmodel_customformfield"] = typeof(CustomFormField)
    };
```

```
<ODataGrid Service="testDataService" CustomFormFieldTypes="CustomFormFieldDefinitions" />
```

## ODataGrid

The ODataGrid is a wrapper around Radzen's DataGrid that dynamically generates columns from view models.

Its basic usage is as simple as defining a service that implements IDataService and giving it to the ODataGrid.

```
<ODataGrid Service="testDataService" />
```

Other parameters of note:
- ExportDataService - an implementation of IExportDataService that handles import/export of CSV files, and possibly more in the future
- ImportExportRole - the role required to import/export CSV files. Defaults to "can_mass_import_and_export", but this is expected to be application-specific.
- DisableImportExportRoleCheck - if true, the import/export role check is skipped. This is useful when authentication is not set up or if import/export is allowed for everyone.
- ImportClassMapType/ExportClassMapType - Type of a CsvHelper.Configuration.ClassMap that's used to help convert types to and from CSV. This is useful if the data isn't flat and wouldn't fit in a single cell.
- CustomRowActions - For times when Copy/View/Edit/Delete aren't enough options for each row. Possible use case would be to redirect to a new page for more advanced edits, combined with AllowUpdate=false.
- CustomFormFieldTypes - For use with custom form field types, see above.

When implementing IDataService, the following methods must be implemented, while the rest are optional and can simply throw a NotSupportedException:
- SupportsCreate - if true, Create must be implemented
- SupportsUpdate - if true, Update must be implemented
- SupportsCopy - if true, GetCopyModel must be implemented and SupportsCreate must be true
- SupportsDelete - if true, Delete must be implemented
- GetNewModel - returns a new empty view model
- GetModels - gets a list of models
- UseInlineSearch - whether to use Radzen's inline search or a separate search box. Defaults to true for now, but may change in the future.
- FiltersQueryString - if specified, filters and sorts will be stored in the URL using this query string. This is useful for sharing links to specific filters or bookmarking them.
