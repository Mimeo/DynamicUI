namespace Mimeo.DynamicUI.Data
{
    public class DataQuery
    {
        public int? Skip { get; set; }
        public int? Top { get; set; }

        /// <summary>
        /// General search text that could be for any supported property
        /// </summary>
        public string? SearchText { get; set; }

        public DataQueryFilterGroup Filter { get; set; } = new();

        /// <summary>
        /// Specific property-based filters
        /// </summary>
        public List<DataQueryFilterBase> Filters
        {
            get => Filter.Filters;
            set => Filter.Filters = value;
        }

        /// <summary>
        /// The conjunction (e.g. and vs. or) used to combine <see cref="Filters"/>
        /// </summary>
        public DataFilterConjunction FiltersConjunction
        {
            get => Filter.FiltersConjunction;
            set => Filter.FiltersConjunction = value;
        }

        public List<DataQuerySort> Sorts { get; set; } = [];

        public DataQuery Clone()
        {
            return new DataQuery
            {
                Skip = this.Skip,
                Top = this.Top,
                SearchText = this.SearchText,
                Filters = this.Filters.Select(f => f.Clone()).ToList(),
                Sorts = this.Sorts.Select(f => f with { } /* clone a record*/).ToList()
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not DataQuery other)
            {
                return false;
            }

            return this.Skip == other.Skip
                && this.Top == other.Top
                && this.SearchText == other.SearchText
                && this.Filters.SequenceEqual(other.Filters)
                && this.Sorts.SequenceEqual(other.Sorts);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Skip, Top, SearchText, Filters, Sorts);
        }

        public static bool operator ==(DataQuery? left, DataQuery? right)
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

        public static bool operator !=(DataQuery? left, DataQuery? right)
        {
            return !(left == right);
        }
    }
}
