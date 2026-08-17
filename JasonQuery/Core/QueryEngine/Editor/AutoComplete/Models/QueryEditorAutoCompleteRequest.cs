using System.Data;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models
{
    internal sealed class QueryEditorAutoCompleteRequest
    {
        public int TriggerPosition { get; set; }

        public DataTable Data { get; set; }
    }
}