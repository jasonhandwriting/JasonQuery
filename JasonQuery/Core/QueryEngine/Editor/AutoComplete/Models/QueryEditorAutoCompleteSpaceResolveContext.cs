using System.Data;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models
{
    internal sealed class QueryEditorAutoCompleteSpaceResolveContext : QueryEditorAutoCompleteResolveContextBase
    {
        public string ConnectionDatabase { get; set; }

        public DataTable TableAndViews { get; set; }
    }
}
