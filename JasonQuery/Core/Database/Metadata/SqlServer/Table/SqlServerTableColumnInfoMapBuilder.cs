using JasonQuery.Core.Data.DataRows;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.SqlServer.Table
{
    internal sealed class SqlServerTableColumnInfoMapBuilder
    {
        public Dictionary<(string DbName, string SchemaDbo, string TableName), List<DataRow>> Build(DataTable dtTableColumnInfo)
        {
            return (dtTableColumnInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                    .GroupBy
                     (
                         rowData =>
                         (
                             DbName: rowData.GetSafeString("DbName"),
                             SchemaDbo: rowData.GetSafeString("SchemaDbo"),
                             TableName: rowData.GetSafeString("Table_Name")
                         )
                     )
                    .ToDictionary
                     (
                         group => group.Key,
                         group => group.ToList()
                     );
        }
    }
}
