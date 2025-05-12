using Microsoft.Extensions.Localization;
using Mimeo.DynamicUI.Data;
using Mimeo.DynamicUI.Data.OData;

namespace Mimeo.DynamicUI.Blazor.Forms.DataFilter
{
    public class FilterViewModel
    {
        public FilterViewModel(IEnumerable<FormFieldDefinition> availableSearchFields, IStringLocalizer lang, ODataExpressionGenerator oDataExpressionGenerator) 
        {
            AvailableSearchFields = Flatten(availableSearchFields.Select(f => new DataFieldDefinition(f)))
                .Where(f => oDataExpressionGenerator.GetSupportedFilterOperators(f).Any())
                .ToList();
            SearchFieldsDisplay = AvailableSearchFields.ToDictionary(
                f => f.ODataPath, 
                f => string.Join("/", f.Path.Select(p => lang[p.FormFieldDefinition.LanguageKey].ToString()))
                );
        }

        public IReadOnlyList<DataFieldDefinition> AvailableSearchFields { get; }
        public IReadOnlyDictionary<string, string> SearchFieldsDisplay { get; }
        public List<DataQueryFilterBase> Filters { get; set; } = new();
        public string? SearchText { get; set; }

        private static IEnumerable<DataFieldDefinition> Flatten(IEnumerable<DataFieldDefinition> formFieldDefinitions)
        {
            foreach (var formFieldDefinition in formFieldDefinitions)
            {
                yield return formFieldDefinition;
                if (formFieldDefinition.IsCollection)
                {
                    var collectionItemFields = formFieldDefinition.GetCollectionItemFields();
                    if (collectionItemFields != null)
                    {
                        foreach (var child in Flatten(collectionItemFields.Select(f => new DataFieldDefinition(f, formFieldDefinition))))
                        {
                            yield return child;
                        }
                    }
                }
            }
        }
    }
}
