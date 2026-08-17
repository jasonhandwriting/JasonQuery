using System;
using System.Data;

namespace JasonQuery.Core.Database.DmlPreview
{
    internal static class DmlPreviewOriginalRowResolver
    {
        public static bool TryResolve(DataRow currentRow, DataTable originalTable, string rowIdentityColumnName, out DataRow originalRow)
        {
            originalRow = null;

            if (currentRow == null || originalTable == null || originalTable.Rows.Count == 0 || string.IsNullOrWhiteSpace(rowIdentityColumnName))
            {
                return false;
            }

            if (currentRow.Table == null || !currentRow.Table.Columns.Contains(rowIdentityColumnName) || !originalTable.Columns.Contains(rowIdentityColumnName))
            {
                return false;
            }

            var rowIdentity = GetIdentityText(currentRow[rowIdentityColumnName]);

            if (string.IsNullOrEmpty(rowIdentity))
            {
                return false;
            }

            DataRow matchedRow = null;

            foreach (DataRow candidateRow in originalTable.Rows)
            {
                var candidateIdentity = GetIdentityText(candidateRow[rowIdentityColumnName]);

                if (!string.Equals(candidateIdentity, rowIdentity, StringComparison.Ordinal))
                {
                    continue;
                }

                if (matchedRow != null)
                {
                    //A synthetic row identity is expected to be unique.
                    //Multiple matches are unsafe because the original row
                    //cannot be determined unambiguously.
                    return false;
                }

                matchedRow = candidateRow;
            }

            if (matchedRow == null)
            {
                return false;
            }

            originalRow = matchedRow;
            return true;
        }

        private static string GetIdentityText(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            return value.ToString();
        }
    }
}