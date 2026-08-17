using JasonQuery.Core.Data.DataRows;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.MySql.View
{
    internal static class MySqlViewColumnInfoMapBuilder
    {
        public static Dictionary<(string SchemaName, string TableName), List<DataRow>> Build(DataTable dtViewColumnInfo)
        {
            return (dtViewColumnInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                   .GroupBy
                    (
                        r => (
                                 SchemaName: r.GetSafeString("DbName"),
                                 TableName: r.GetSafeString("ViewName")
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