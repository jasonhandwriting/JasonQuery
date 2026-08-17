using JasonQuery.Core.Data.DataRows;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.SqlServer.View
{
    internal sealed class SqlServerViewColumnInfoMapBuilder
    {
        public Dictionary<(string DbName, string SchemaDbo, string ViewName), List<DataRow>> Build(DataTable dtViewColumnInfo)
        {
            return (dtViewColumnInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                    .GroupBy
                     (
                         rowData =>
                         (
                             DbName: rowData.GetSafeString("DbName"),
                             SchemaDbo: rowData.GetSafeString("SchemaDbo"),
                             ViewName: rowData.GetSafeString("ViewName")
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