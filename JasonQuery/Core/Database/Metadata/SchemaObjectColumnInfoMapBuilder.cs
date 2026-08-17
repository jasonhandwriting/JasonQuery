using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata
{
    internal static class SchemaObjectColumnInfoMapBuilder
    {
        public static Dictionary<(string SchemaName, string ObjectName), List<DataRow>> Build(DataTable dtSource, string schemaColumnName, string objectColumnName)
        {
            if (dtSource == null || dtSource.Rows.Count == 0)
            {
                return new Dictionary<(string SchemaName, string ObjectName), List<DataRow>>();
            }

            if (string.IsNullOrWhiteSpace(schemaColumnName))
            {
                throw new ArgumentException("schemaColumnName cannot be null or empty.", nameof(schemaColumnName));
            }

            if (string.IsNullOrWhiteSpace(objectColumnName))
            {
                throw new ArgumentException("objectColumnName cannot be null or empty.", nameof(objectColumnName));
            }

            return dtSource.AsEnumerable()
                           .GroupBy
                            (
                                r => (
                                         SchemaName: r.GetSafeString(schemaColumnName),
                                         ObjectName: r.GetSafeString(objectColumnName)
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