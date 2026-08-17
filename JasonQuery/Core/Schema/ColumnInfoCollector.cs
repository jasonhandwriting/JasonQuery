using JasonLibrary.Core.Schema;
using System;
using System.Collections.Generic;

namespace JasonQuery.Core.Schema
{
    public sealed class ColumnInfoCollector
    {
        private readonly Dictionary<string, ColumnInfo> _map = new Dictionary<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase);

        public void AddOrUpdate(ColumnInfo info)
        {
            if (info == null || string.IsNullOrWhiteSpace(info.ColumnName))
            {
                return;
            }

            _map[info.ColumnName] = info;
        }

        public bool TryGet(string sColumnName, out ColumnInfo info)
        {
            return _map.TryGetValue(sColumnName, out info);
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