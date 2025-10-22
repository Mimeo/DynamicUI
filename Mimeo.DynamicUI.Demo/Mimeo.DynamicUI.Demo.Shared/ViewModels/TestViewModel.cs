using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Demo.Shared.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static Mimeo.DynamicUI.Demo.Shared.Models.TestModel;

namespace Mimeo.DynamicUI.Demo.Shared.ViewModels
{
    public class TestViewModel : ViewModel
    {
        public TestViewModel(IReadOnlyDataService<TestViewModel> relatedModelsSource)
        {
            this.relatedModelsSource = relatedModelsSource;
        }

        public TestViewModel(TestModel model, IReadOnlyDataService<TestViewModel> relatedModelsSource)
            : this(relatedModelsSource)
        {
            Id = model.Id.ToString();
            Name = model.Name;
            Description = model.Description;
            HTML = model.HTML;
            JSON = model.JSON;
            Number = model.Number;
            Decimal = model.Decimal;
            DateTimeUtc = model.DateTimeUtc;
            DateTimeOffset = model.DateTimeOffset;
            Time = model.Time;
            SingleSelect = model.SingleSelect;
            MultiSelect = model.MultiSelect;
            ComboBox = model.ComboBox;
            Color = model.Color;
            Section = new SimpleSubViewModel
            {
                Property1 = model.Section.Property1,
                Property2 = model.Section.Property2
            };
            StringList = model.StringList;
            SimpleModelList = model.SimpleModelList.ConvertAll(x => new SimpleSubViewModel
            {
                Property1 = x.Property1,
                Property2 = x.Property2
            });
            AdvancedModelList = model.AdvancedModelList.ConvertAll(x => new AdvancedSubViewModel
            {
                Property1 = x.Property1,
                Property2 = x.Property2,
                SubList = x.SubList
            });
            Enabled = model.Enabled;
            NullableStringEnabled = model.NullableString != null;
            NullableStringValue = model.NullableString;
            EnableHiddenProperties = model.EnableHiddenProperties;
            HiddenString = model.HiddenString;
            RelatedModelId = model.RelatedModelId.GetValueOrDefault() != default ? model.RelatedModelId.ToString() : null;
            RelatedModelIds = model.RelatedModelIds?.Select(g => g.ToString()).ToList();
            CustomFormField = model.CustomFormField;
        }

        public TestModel ToModel()
        {
            return new TestModel
            {
                Id = !string.IsNullOrEmpty(Id) ? Guid.Parse(Id) : default,
                Name = Name,
                Description = Description,
                HTML = HTML,
                JSON = JSON,
                Number = Number,
                Decimal = Decimal,
                DateTimeUtc = DateTimeUtc,
                DateTimeOffset = DateTimeOffset,
                Time = Time,
                SingleSelect = SingleSelect,
                MultiSelect = MultiSelect,
                ComboBox = ComboBox,
                Color = Color,
                Section = new SubModelSimple
                {
                    Property1 = Section.Property1,
                    Property2 = Section.Property2
                },
                StringList = StringList,
                SimpleModelList = SimpleModelList.ConvertAll(x => new SubModelSimple
                {
                    Property1 = x.Property1,
                    Property2 = x.Property2
                }),
                AdvancedModelList = AdvancedModelList.ConvertAll(x => new SubModelAdvanced
                {
                    Property1 = x.Property1,
                    Property2 = x.Property2,
                    SubList = x.SubList
                }),
                Enabled = Enabled,
                NullableString = NullableStringEnabled ? (NullableStringValue ?? "") : null,
                EnableHiddenProperties = EnableHiddenProperties,
                HiddenString = HiddenString,
                RelatedModelId = !string.IsNullOrEmpty(RelatedModelId) ? Guid.Parse(RelatedModelId) : null,
                RelatedModelIds = RelatedModelIds?.Select(id => !string.IsNullOrEmpty(id) ? Guid.Parse(id) : Guid.Empty).ToList(),
                CustomFormField = CustomFormField
            };
        }

        private readonly IReadOnlyDataService<TestViewModel> relatedModelsSource;

        [Required]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? HTML { get; set; }
        public string? JSON { get; set; }
        public int Number { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public TimeSpan Time { get; set; }
        public string? SingleSelect { get; set; }
        public List<string> MultiSelect { get; set; } = [];
        public string? ComboBox { get; set; }
        public string? Color { get; set; }
        public SimpleSubViewModel Section { get; set; } = new();
        public List<string> StringList { get; set; } = [];
        public List<SimpleSubViewModel> SimpleModelList { get; set; } = [];
        public List<AdvancedSubViewModel> AdvancedModelList { get; set; } = [];
        public bool Enabled { get; set; }
        public bool NullableStringEnabled { get; set; }
        public string? NullableStringValue { get; set; }
        public bool EnableHiddenProperties { get; set; }
        public string? HiddenString { get; set; }
        public string? RelatedModelId { get; set; }
        public List<string>? RelatedModelIds { get; set; }
        public int CustomFormField { get; set; }

        protected override IEnumerable<FormFieldDefinition> GetListFormFields()
        {
            var idField = new FormFieldDefinition(FormFieldType.Guid, () => Id)
            {
                ReadOnly = Id != default,  // Id is read-only when editing an existing item
                FilterType = typeof(Guid)
            };
            yield return idField;
            yield return FormField(() => Name);
            yield return FormField(() => Number);
            yield return FormField(() => Decimal);
            yield return FormField(() => DateTimeUtc, dateDisplayMode: DateDisplayMode.UserLocal);
            yield return Table(() => StringList);
        }

        protected override IEnumerable<FormFieldDefinition> GetDropDownListFormFields()
        {
            var idField = new FormFieldDefinition(FormFieldType.Guid, () => Id)
            {
                ReadOnly = Id != default,  // Id is read-only when editing an existing item
                FilterType = typeof(Guid)
            };
            yield return idField;
            yield return FormField(() => Name);
        }

        protected override IEnumerable<FormFieldDefinition> GetEditFormFields()
        {
            var hiddenFormField = new FormFieldDefinition(FormFieldType.Text, () => HiddenString)
            {
                IsVisible = EnableHiddenProperties
            };

            var idField = new FormFieldDefinition(FormFieldType.Guid, () => Id)
            {
                ReadOnly = Id != default,  // Id is read-only when editing an existing item
                FilterType = typeof(Guid)
            };
            yield return idField;
            yield return FormField(() => Name);
            yield return FormField(() => Description, textType: TextType.MultiLine);
            yield return FormField(() => HTML, textType: TextType.HTML);
            yield return FormField(() => JSON, textType: TextType.Code, codeLanguage: "json", collapsed: true);
            yield return FormField(() => Number);
            yield return FormField(() => Decimal, decimalPlaces: 2);
            yield return FormField(() => DateTimeUtc, dateDisplayMode: DateDisplayMode.UserLocal);
            yield return FormField(() => DateTimeOffset, dateDisplayMode: DateDisplayMode.UserLocal);
            yield return FormField(FormFieldType.Time, () => Time);
            yield return new SingleSelectFormFieldDefinition(() => SingleSelect, SingleSelectItems);
            yield return new MultiSelectFormFieldDefinition(() => MultiSelect, MultiSelectItems);
            yield return FormField(() => ComboBox, textType: TextType.SingleLine, items: ["Option 1", "option2languagekey", "Option 3"]);
            yield return new FormFieldDefinition(FormFieldType.Color, () => Color);
            yield return FormField(() => Section);
            yield return Table(() => StringList);

            // Depending on the structure of the view model, sometimes a table is more appropriate
            yield return Table(() => SimpleModelList);

            // but for sufficiently large view models, a section list is easier on the user
            yield return SectionList(() => AdvancedModelList, m => m.FormField(() => m.Property1));

            yield return new SingleSelectDropDownFormFieldDefinition(() => RelatedModelId, relatedModelsSource, FormField(() => Name), idField);
            yield return new MultiSelectDropDownFormFieldDefinition(() => RelatedModelIds, relatedModelsSource, FormField(() => Name), idField);
            yield return FormField(() => Enabled);
            yield return new NullableFormFieldDefinition(() => NullableStringEnabled, FormField(() => NullableStringValue));
            yield return new FormFieldDefinition(FormFieldType.Checkbox, () => EnableHiddenProperties)
            {
                OnValueChanged = value =>
                {
                    hiddenFormField.IsVisible = value is true;
                }
            };
            yield return hiddenFormField;
            yield return new CustomFormFieldDefinition(() => CustomFormField);
        }

        protected override IEnumerable<FormFieldDefinition> GetSearchFormFields()
        {
            var hiddenFormField = new FormFieldDefinition(FormFieldType.Text, () => HiddenString)
            {
                IsVisible = EnableHiddenProperties
            };

            yield return new FormFieldDefinition(FormFieldType.Guid, () => Id)
            {
                ReadOnly = Id != default,  // Id is read-only when editing an existing item
                FilterType = typeof(Guid)
            };
            yield return FormField(() => Name);
            yield return FormField(() => Description, textType: TextType.MultiLine);
            yield return FormField(() => HTML, textType: TextType.HTML);
            yield return FormField(() => Number);
            yield return FormField(() => Decimal, decimalPlaces: 2);
            yield return new DateSearchFieldDefinition(() => DateTimeUtc, dateDisplayMode: DateDisplayMode.UserLocal);
            yield return FormField(() => DateTimeOffset, dateDisplayMode: DateDisplayMode.UserLocal);
            yield return FormField(FormFieldType.Time, () => Time);
            yield return new SingleSelectDropDownFormFieldDefinition(() => SingleSelect, SingleSelectItems);
            yield return new MultiSelectDropDownFormFieldDefinition(() => MultiSelect, MultiSelectItems);
            yield return FormField(() => ComboBox, textType: TextType.SingleLine, items: ["Option 1", "option2languagekey", "Option 3"]);
            yield return new FormFieldDefinition(FormFieldType.Color, () => Color);
            yield return FormField(() => Section);
            yield return Table(() => StringList);
            yield return Table(() => SimpleModelList);
            yield return SectionList(() => AdvancedModelList);
            yield return FormField(() => Enabled);
            yield return new NullableFormFieldDefinition(() => NullableStringEnabled, FormField(() => NullableStringValue));
            yield return new FormFieldDefinition(FormFieldType.Checkbox, () => EnableHiddenProperties)
            {
                OnValueChanged = value =>
                {
                    hiddenFormField.IsVisible = value is true;
                }
            };
            yield return hiddenFormField;

        }

        public class SimpleSubViewModel : ViewModel
        {
            public string? Property1
            {
                get => _property1;
                set
                {
                    _property1 = value;
                    RaisePropertyChanged(nameof(Property1));
                }
            }
            private string? _property1;

            public int Property2
            {
                get => _property2;
                set
                {
                    _property2 = value;
                    RaisePropertyChanged(nameof(Property2));
                }
            }
            private int _property2;

            protected override IEnumerable<FormFieldDefinition> GetEditFormFields()
            {
                yield return FormField(() => Property1);
                yield return FormField(() => Property2);
            }
        }

        public class AdvancedSubViewModel : ViewModel
        {
            public string? Property1
            {
                get => _property1;
                set
                {
                    _property1 = value;
                    RaisePropertyChanged(nameof(Property1));
                }
            }
            private string? _property1;

            public int Property2
            {
                get => _property2;
                set
                {
                    _property2 = value;
                    RaisePropertyChanged(nameof(Property2));
                }
            }
            private int _property2;

            public List<string> SubList { get; set; } = [];

            protected override IEnumerable<FormFieldDefinition> GetEditFormFields()
            {
                yield return FormField(() => Property1);
                yield return FormField(() => Property2);
                yield return Table(() => SubList);
            }
        }

        private static List<ListItem> SingleSelectItems =>
        [
            new ListItem("Option 1", "option1"),
            new ListItem("option2languagekey", "option2"),
            new ListItem("Option 3", "option3")
        ];

        private static List<ListItem> MultiSelectItems =>
        [
            new ListItem("Option 1", "option1"),
            new ListItem("Option 2", "option2"),
            new ListItem("Option 3", "option3")
        ];
    }
}
