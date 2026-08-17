using JasonQuery.Core.Data.DataRows;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.MySql.Table
{
    internal static class MySqlTableColumnInfoMapBuilder
    {
        public static Dictionary<(string SchemaName, string TableName), List<DataRow>> Build(DataTable dtTableColumnInfo)
        {
            return (dtTableColumnInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                   .GroupBy
                    (
                        r => (
                                 SchemaName: r.GetSafeString("DbName"),
                                 TableName: r.GetSafeString("TableName")
                             )
                    )
                   .ToDictionary
                    (
                        g => g.Key,
                        g => g.ToList()
                    );
        }
    }
}