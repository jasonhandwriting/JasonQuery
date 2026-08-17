using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.MySql.Table
{
    internal static class MySqlTableRowCountMapBuilder
    {
        public static Dictionary<(string SchemaName, string TableName), long> Build(DataTable dtTableInfo)
        {
            return (dtTableInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                   .ToDictionary
                    (
                        r => (
                                 SchemaName: r.GetSafeString("DbName"),
                                 TableName: r.GetSafeString("TableName")
                             ),
                        r => Math.Max(0L, r.GetSafeLong("RowCount"))
                    );
        }
    }
}