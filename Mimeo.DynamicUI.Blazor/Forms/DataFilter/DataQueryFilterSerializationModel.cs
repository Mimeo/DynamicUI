using Mimeo.DynamicUI.Data;
using System.Text.Json;

namespace Mimeo.DynamicUI.Blazor.Forms.DataFilter
{
    public class DataQueryFilterSerializationModel
    {
        public string? ODataPath { get; set; }
        public DataFilterOperator? Operator { get; set; }
        public string? Value { get; set; }

        public static DataQueryFilterSerializationModel FromFilter(DataQueryFilter filter)
        {
            return new DataQueryFilterSerializationModel
            {
                ODataPath = filter.FilterDefinition.ODataPath,
                Operator = filter.Operator,
                Value = JsonSerializer.Serialize(filter.Value)
            };
        }
    }
}
