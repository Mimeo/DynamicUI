namespace Mimeo.DynamicUI.Demo.Shared.Models
{
    public class TestModel
    {
        public Guid Id { get; set; }
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
        public List<string> MultiSelect { get; set; } = new();
        public string? ComboBox { get; set; }
        public string? Color { get; set; }
        public SubModelSimple Section { get; set; } = new();
        public List<string> StringList { get; set; } = new();
        public List<SubModelSimple> SimpleModelList { get; set; } = new();
        public List<SubModelAdvanced> AdvancedModelList { get; set; } = new();
        public string? NullableString { get; set; }
        public bool Enabled { get; set; }
        public bool EnableHiddenProperties { get; set; }
        public string? HiddenString { get; set; }
        public Guid? RelatedModelId { get; set; }
        public List<Guid>? RelatedModelIds { get; set; }
        public int CustomFormField { get; set; }

        public class SubModelSimple
        {
            public string? Property1 { get; set; }
            public int Property2 { get; set; }
        }

        public class SubModelAdvanced
        {
            public string? Property1 { get; set; }
            public int Property2 { get; set; }
            public List<string> SubList { get; set; } = [];
        }
    }
}
