namespace JasonQuery.Core.Database.Metadata.MySql.QueryEditor
{
    internal sealed class MySqlQueryEditorMetadataLoadOptions
    {
        public bool AddSchemaRow { get; set; }

        public bool AutoCompleteFunction { get; set; }

        public bool AutoCompleteTable { get; set; }

        public bool AutoCompleteTrigger { get; set; }

        public bool AutoCompleteView { get; set; }

        public bool FromSchemaBrowser { get; set; }
    }
}
