namespace JasonQuery.Core.Database.Metadata.Oracle.QueryEditor
{
    internal sealed class OracleQueryEditorMetadataLoadOptions
    {
        public bool EnableFunctionAutoComplete { get; set; }

        public bool EnableTableAutoComplete { get; set; }

        public bool EnableTriggerAutoComplete { get; set; }

        public bool EnableViewAutoComplete { get; set; }
    }
}