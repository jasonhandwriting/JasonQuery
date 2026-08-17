using JasonQuery.Core.Database.Connection;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models
{
    internal sealed class QueryEditorAutoCompleteSpaceAnalysisRequest
    {
        public string Sql { get; set; }

        public int CaretPosition { get; set; }

        public DataSourceType DataSourceType { get; set; }
    }
}