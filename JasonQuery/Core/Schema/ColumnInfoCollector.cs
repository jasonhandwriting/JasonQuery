using JasonLibrary.Core.Schema;
using System;
using System.Collections.Generic;

namespace JasonQuery.Core.Schema
{
    public sealed class ColumnInfoCollector
    {
        private const char SourceKeySeparator = '\u001F';

        private readonly Dictionary<string, ColumnInfo> _map = new Dictionary<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ColumnInfo> _sourceMap = new Dictionary<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase);

        public void AddOrUpdate(ColumnInfo info)
        {
            if (info == null || string.IsNullOrWhiteSpace(info.ColumnName))
            {
                return;
            }

            _map[info.ColumnName] = info;
            _sourceMap[BuildSourceKey(info.ColumnName, info.BaseSchemaName, info.BaseTableName)] = info;
        }

        public bool TryGet(string sColumnName, out ColumnInfo info)
        {
            return _map.TryGetValue(sColumnName, out info);
        }

        public bool TryGet(string sColumnName, string sBaseSchemaName, string sBaseTableName, out ColumnInfo info)
        {
            var key = BuildSourceKey(sColumnName, sBaseSchemaName, sBaseTableName);

            if (_sourceMap.TryGetValue(key, out info))
            {
                return true;
            }

            return TryGet(sColumnName, out info);
        }

        private static string BuildSourceKey(string columnName, string baseSchemaName, string baseTableName)
        {
            return string.Concat
            (
                columnName ?? string.Empty,
                SourceKeySeparator,
                baseSchemaName ?? string.Empty,
                SourceKeySeparator,
                baseTableName ?? string.Empty
            );
        }

        /// <summary>
        /// Returns all collected column metadata as a read-only collection.
        /// The returned collection represents a stable snapshot and must not be modified.
        /// </summary>
        public IReadOnlyCollection<ColumnInfo> GetAll()
        {
            return _map.Values;
        }
    }
}