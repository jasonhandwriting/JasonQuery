namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models
{
    internal sealed class QueryEditorAutoCompleteSpaceAnalysisResult
    {
        public bool CanResolve { get; set; }

        public QueryEditorAutoCompleteSpaceKeyword Keyword { get; set; }

        public QueryEditorAutoCompleteSpaceIntent Intent { get; set; }

        public AutoCompleteObjectLookupMode LookupMode { get; set; }

        public QueryEditorAutoCompleteSpaceSourceKind SourceKind { get; set; }

        public string ObjectName { get; set; }

        public string AliasName { get; set; }

        public string SourceSql { get; set; }

        public bool TableOnly { get; set; }
    }
}