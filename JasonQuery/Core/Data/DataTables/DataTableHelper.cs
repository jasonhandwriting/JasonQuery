using System.Data;
using JasonQuery.Core.Data.Value;

namespace JasonQuery.Core.Data.DataTables
{
    internal static class DataTableHelper
    {
        public static bool IsDbNull(DataTable dtTable, int rowIndex, int columnIndex)
        {
            if (dtTable == null)
            {
                return false;
            }

            if (rowIndex < 0 || rowIndex >= dtTable.Rows.Count)
            {
                return false;
            }

            if (columnIndex < 0 || columnIndex >= dtTable.Columns.Count)
            {
                return false;
            }

            return DbValueHelper.IsDbNull(dtTable.Rows[rowIndex][columnIndex]);
        }
    }
}
