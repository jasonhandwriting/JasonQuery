using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Schema;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.View
{
    internal static class PostgreSqlViewColumnInfoTableBuilder
    {
        public static DataTable Build(DataTable dtSchema, ColumnInfoCollector columnInfoCollector)
        {
            var dtColumnName = CreateColumnInfoTable();

            if (dtSchema == null || columnInfoCollector == null)
            {
                return dtColumnName;
            }

            foreach (DataRow dr in dtSchema.AsEnumerable())
            {
                var columnName = dr.GetSafeString("ColumnName");
                int columnOrdinal = dr.GetSafeInt("ColumnOrdinal");

                if (!columnInfoCollector.TryGet(columnName, out var columnInfo))
                {
                    continue;
                }

                AddRow(dtColumnName, columnName, columnOrdinal, columnInfo.BaseDataType, columnInfo.ColumnSize,
                       columnInfo.NumericScale, columnInfo.NumericPrecision);
            }

            return dtColumnName;
        }

        private static DataTable CreateColumnInfoTable()
        {
            var dtColumnName = new DataTable();

            dtColumnName.Columns.Add(" ");
            dtColumnName.Columns.Add("Column_Name");
            dtColumnName.Columns.Add("Column_ID", typeof(int));
            dtColumnName.Columns.Add("TypeName");
            dtColumnName.Columns.Add("DataTypeName");
            dtColumnName.Columns.Add("DataType");
            dtColumnName.Columns.Add("ColumnSize");
            dtColumnName.Columns.Add("NumericScale");
            dtColumnName.Columns.Add("NumericPrecision");

            return dtColumnName;
        }

        private static void AddRow(DataTable dtColumnName, string columnName, int columnOrdinal, string baseDataType,
                                   int columnSize, int numericScale, int numericPrecision)
        {
            var row = dtColumnName.NewRow();

            row[" "] = "1";
            row["Column_Name"] = columnName;
            row["Column_ID"] = columnOrdinal;
            row["TypeName"] = baseDataType;
            row["DataTypeName"] = baseDataType;
            row["DataType"] = baseDataType;
            row["ColumnSize"] = columnSize;
            row["NumericScale"] = numericScale;
            row["NumericPrecision"] = numericPrecision;

            dtColumnName.Rows.Add(row);
        }
    }
}
