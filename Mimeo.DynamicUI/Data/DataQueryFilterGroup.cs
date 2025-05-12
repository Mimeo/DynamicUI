using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mimeo.DynamicUI.Data
{
    public class DataQueryFilterGroup : DataQueryFilterBase
    {
        /// <summary>
        /// Specific property-based filters
        /// </summary>
        public List<DataQueryFilterBase> Filters { get; set; } = [];

        /// <summary>
        /// The conjunction (e.g. and vs. or) used to combine <see cref="Filters"/>
        /// </summary>
        public DataFilterConjunction FiltersConjunction { get; set; } = DataFilterConjunction.And;

        public override DataQueryFilterBase Clone()
        {
            return new DataQueryFilterGroup
            {
                Filters = Filters.Select(f => f.Clone()).ToList()
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not DataQueryFilterGroup other) 
            {
                return false;
            }

            return FiltersConjunction == other.FiltersConjunction && Filters.SequenceEqual(other.Filters);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FiltersConjunction, Filters);
        }
    }
}
