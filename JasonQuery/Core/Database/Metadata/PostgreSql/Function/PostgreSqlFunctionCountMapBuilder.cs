using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Function
{
    internal static class PostgreSqlFunctionCountMapBuilder
    {
        public static Dictionary<string, int> Build(DataTable dtFunctionInfo)
        {
            if (dtFunctionInfo == null || dtFunctionInfo.Rows.Count == 0)
            {
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }

            return dtFunctionInfo.AsEnumerable()
                                 .GroupBy
                                  (
                                      r => r.GetSafeString("DbName"),
                                      StringComparer.OrdinalIgnoreCase
                                  )
                                 .ToDictionary
                                  (
                                      g => g.Key,
                                      g => g.Count(),
                                      StringComparer.OrdinalIgnoreCase
                                  );
        }
    }
}
