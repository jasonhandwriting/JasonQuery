using System.Data;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support
{
    internal static class SpaceAutoCompleteDataFilter
    {
        public static DataTable FilterByKeyword(DataTable source, string keyword)
        {
            return QueryEditorAutoCompleteDataFilter.FilterSpaceByKeyword(source, keyword);
        }
    }
}