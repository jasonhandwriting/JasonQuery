using System.Data;

namespace JasonQuery.Core.SchemaExplorer.Selection
{
    public interface ISchemaExplorerRowReader
    {
        bool IsGroupRow(int displayRowIndex);

        int GetGroupLevel(int displayRowIndex);

        string GetGroupedText(int displayRowIndex);

        string GetSchemaTypeFromGroupStartIndex(int displayRowIndex);

        string FindPreviousGroupText(int displayRowIndex, int groupLevel);

        DataRow GetDataRow(int displayRowIndex);

        DataTable GetSchemaDataTable();

        void SetCurrentRow(int displayRowIndex);
    }
}
