namespace Mimeo.DynamicUI.Data
{
    public class DataFieldDefinition
    {
        public DataFieldDefinition(FormFieldDefinition formFieldDefinition, DataFieldDefinition? parent = null)
        {
            FormFieldDefinition = formFieldDefinition;
            Parent = parent;
        }

        public DataFieldDefinition(string className, Type propertyType, string propertyName, DataFieldDefinition? parent = null)
            : this(new FormFieldDefinition(className, propertyType, propertyName), parent)
        {
        }

        public DataFieldDefinition(Type classType, Type propertyType, string propertyName, DataFieldDefinition? parent = null)
            : this(new FormFieldDefinition(classType, propertyType, propertyName), parent)
        {
        }

        public FormFieldDefinition FormFieldDefinition { get; }

        public DataFieldDefinition? Parent { get; }

        public bool IsCollection => FormFieldDefinition is IListFieldDefinition;

        public bool IsInCollection => Parent != null && Parent.IsCollection;

        public List<DataFieldDefinition> Path
        {
            get
            {
                _path ??= GetPath().ToList();
                return _path;
            }
        }
        private List<DataFieldDefinition>? _path;

        public List<string> PropertyNamePath
        {
            get
            {
                _propertyNamePath ??= GetPropertyNamePath().ToList();
                return _propertyNamePath;
            }
        }
        private List<string>? _propertyNamePath;

        public string ODataPath
        {
            get
            {
                _odataPath ??= GetODataPath();
                return _odataPath;
            }
        }
        private string? _odataPath;

        public IEnumerable<FormFieldDefinition>? GetCollectionItemFields()
        {
            var listFieldDefinition = FormFieldDefinition as IListFieldDefinition;
            if (listFieldDefinition == null)
            {
                return null;
            }

            var newItem = listFieldDefinition.CreateNewItem();
            if (newItem == null)
            {
                return null;
            }

            if (newItem is not ViewModel viewModel)
            {
                return null;
            }

            return viewModel.GetSearchForm().Values;
        }

        public IEnumerable<DataFieldDefinition> GetParentPath()
        {
            if (Parent != null)
            {
                foreach (var parentPath in Parent.GetPath())
                {
                    yield return parentPath;
                }
            }
        }

        public IEnumerable<DataFieldDefinition> GetPath()
        {
            foreach (var parentPath in GetParentPath())
            {
                yield return parentPath;
            }
            yield return this;
        }

        public IEnumerable<string> GetPropertyNamePath()
        {
            if (Parent != null)
            {
                foreach (var parentPath in Parent.GetPropertyNamePath())
                {
                    yield return parentPath;
                }
            }

            yield return FormFieldDefinition.PropertyName;
        }

        public string GetODataPath() => string.Join("/", GetPropertyNamePath());

        public string GetCSharpPath() => string.Join(".", GetPropertyNamePath());

        public override bool Equals(object? obj)
        {
            if (obj is not DataFieldDefinition other)
            {
                return false;
            }

            return this.ODataPath == other.ODataPath;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ODataPath);
        }

        public static bool operator ==(DataFieldDefinition? left, DataFieldDefinition? right)
        {
            if (left is null && right is null)
            {
                return true;
            }
            else if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(DataFieldDefinition? left, DataFieldDefinition? right)
        {
            return !(left == right);
        }
    }
}
