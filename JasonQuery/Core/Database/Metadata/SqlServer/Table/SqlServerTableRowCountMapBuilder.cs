using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.SqlServer.Table
{
    internal sealed class SqlServerTableRowCountMapBuilder
    {
        public Dictionary<(string DbName, string SchemaDbo, string TableName), long> Build(DataTable dtTableRowCount)
        {
            return (dtTableRowCount?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                    .ToDictionary
                     (
                         rowData =>
                         (
                             DbName: rowData.GetSafeString("DbName"),
                             SchemaDbo: rowData.GetSafeString("SchemaName"),
                             TableName: rowData.GetSafeString("TableName")
                         ),
                         rowData => Math.Max(0L, rowData.GetSafeLong("Rows"))
                     );
        }
    }
}