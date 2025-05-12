using Mimeo.DynamicUI.Data;
using System.Text.Json;

namespace Mimeo.DynamicUI.Blazor.Forms.DataFilter
{
    public class DataQueryFilterGroupSerializationModel
    {
        public List<DataQueryFilterSerializationModel> Filters { get; set; } = [];
        public List<DataQueryFilterGroupSerializationModel> FilterGroups { get; set; } = [];
        public DataFilterConjunction FiltersConjunction { get; set; } = DataFilterConjunction.And;

        public DataQueryFilterGroup ToFilterGroup(FilterViewModel filterViewModel)
        {
            var group = new DataQueryFilterGroup
            {
                FiltersConjunction = FiltersConjunction
            };

            group.Filters.AddRange(Filters.Select(DataQueryFilterBase (f) =>
            {
                var field = filterViewModel.AvailableSearchFields.First(a => a.ODataPath == f.ODataPath);
                return new DataQueryFilter(
                    field,
                    f.Operator,
                    f.Value != null ? JsonSerializer.Deserialize(f.Value, field.FormFieldDefinition.SearchFieldType ?? field.FormFieldDefinition.PropertyType) : null);
            }));

            group.Filters.AddRange(FilterGroups.Select(f => f.ToFilterGroup(filterViewModel)));

            return group;
        }

        public static DataQueryFilterGroupSerializationModel FromFilterGroup(DataQueryFilterGroup filterGroup)
        {
            return new DataQueryFilterGroupSerializationModel
            {
                Filters = filterGroup.Filters.Where(f => f is DataQueryFilter).Cast<DataQueryFilter>().Select(f => DataQueryFilterSerializationModel.FromFilter(f)).ToList(),
                FilterGroups = filterGroup.Filters.Where(f => f is DataQueryFilterGroup).Cast<DataQueryFilterGroup>().Select(f => DataQueryFilterGroupSerializationModel.FromFilterGroup(f)).ToList(),
                FiltersConjunction = filterGroup.FiltersConjunction
            };
        }
    }
}
