
namespace Mimeo.DynamicUI
{
    public class ListItem : ViewModel, IEquatable<ListItem>
    {
        public ListItem(string item)
        {
            Name = item ?? throw new ArgumentNullException(nameof(item));
            Value = item ?? throw new ArgumentNullException(nameof(item));
        }
        public ListItem(string name, string value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The user-friendly display name of the item
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The value of the item
        /// </summary>
        public string Value { get; }

        public override bool Equals(object? obj)
        {
            return obj is ListItem other && other.Value == this.Value;
        }

        public bool Equals(ListItem? other)
        {
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        protected override IEnumerable<FormFieldDefinition> GetListFormFields()
        {
            yield return FormField(() => Name);
        }

        protected override IEnumerable<FormFieldDefinition> GetEditFormFields()
        {
            throw new NotSupportedException();
        }
    }
}
