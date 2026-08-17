using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Trigger
{
    internal static class PostgreSqlTriggerCountMapBuilder
    {
        public static Dictionary<string, int> Build(DataTable dtTriggerInfo)
        {
            if (dtTriggerInfo == null || dtTriggerInfo.Rows.Count == 0)
            {
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }

            return dtTriggerInfo.AsEnumerable()
                                .GroupBy
                                 (
                                     r => r.GetSafeString("DbName"), StringComparer.OrdinalIgnoreCase
                                 )
                                .ToDictionary
                                 (
                                     g => g.Key,
                                     g => g.Count(), StringComparer.OrdinalIgnoreCase
                                 );
        }
    }
}