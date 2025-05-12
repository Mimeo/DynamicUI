namespace Mimeo.DynamicUI.Data
{
    public enum ImportExportDataType
    {
        None = 0,
        CSV,
        JSON
            
        // Note: The UI feeds the lowercase string value directly to the file extension (file.[value]) and mime type (text/[value])
        // If adding a new data type, either ensure this shortcut still works or build a better solution
    }
}
