using JasonQuery.Core.Database.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.DmlPreview
{
    internal static class DmlPreviewWhereConditionBuilder
    {
        public static DmlPreviewWhereConditionResult Build(DmlPreviewWhereConditionRequest request)
        {
            if (request == null || request.CurrentRow == null || request.OriginalTable == null || string.IsNullOrWhiteSpace(request.RowIdentityColumnName))
            {
                return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.InvalidRequest);
            }

            if (!DmlPreviewOriginalRowResolver.TryResolve(request.CurrentRow, request.OriginalTable, request.RowIdentityColumnName, out DataRow originalRow))
            {
                return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.OriginalRowNotFound);
            }

            if (request.HasDeclaredPrimaryKey)
            {
                return BuildPrimaryKeyCondition(request, originalRow);
            }

            return BuildPhysicalRowIdentifierCondition(request, originalRow);
        }

        private static DmlPreviewWhereConditionResult BuildPrimaryKeyCondition(DmlPreviewWhereConditionRequest request, DataRow originalRow)
        {
            var primaryKeyColumns = (request.PrimaryKeyColumns ?? Enumerable.Empty<DmlPreviewPrimaryKeyColumn>()).ToList();

            if (primaryKeyColumns.Count == 0)
            {
                return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PrimaryKeyMetadataMissing);
            }

            var normalizedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var primaryKeyColumn in primaryKeyColumns)
            {
                if (primaryKeyColumn == null || string.IsNullOrWhiteSpace(primaryKeyColumn.ColumnName))
                {
                    return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PrimaryKeyColumnNameMissing);
                }

                if (!normalizedNames.Add(primaryKeyColumn.ColumnName.Trim()))
                {
                    return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.DuplicatePrimaryKeyColumn);
                }

                if (primaryKeyColumn.ColumnInfo == null)
                {
                    return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PrimaryKeyColumnInfoMissing);
                }
            }

            var orderedColumns = primaryKeyColumns.OrderBy(column => column.OrdinalPosition)
                                                  .ThenBy
                                                   (
                                                       column => column.ColumnName,
                                                       StringComparer.OrdinalIgnoreCase
                                                   )
                                                  .ToList();

            var conditions = new List<string>();
            var nullKeyword = request.UseUpperCaseKeywords ? "NULL" : "null";
            var isKeyword = request.UseUpperCaseKeywords ? " IS " : " is ";
            var andKeyword = request.UseUpperCaseKeywords ? "AND" : "and";

            foreach (var primaryKeyColumn in orderedColumns)
            {
                var columnName = primaryKeyColumn.ColumnName.Trim();

                if (originalRow.Table == null || !originalRow.Table.Columns.Contains(columnName))
                {
                    return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PrimaryKeyValueColumnMissing);
                }

                var originalValue = originalRow[columnName] == DBNull.Value ? "NULL" : originalRow[columnName]?.ToString();

                var sqlValue = DmlPreviewSqlLiteralFormatter.Format
                               (
                                   new DmlPreviewLiteralFormatRequest
                                   {
                                       DataSourceType = request.DataSourceType,
                                       CellValue = originalValue,
                                       ColumnInfo = primaryKeyColumn.ColumnInfo,
                                       DefaultValue = string.Empty,
                                       AllowDefaultKeyword = false,
                                       UseUpperCaseKeywords = request.UseUpperCaseKeywords,
                                       NullValueIndicators = request.NullValueIndicators
                                   }
                               );

                if (string.IsNullOrWhiteSpace(sqlValue))
                {
                    return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PrimaryKeyLiteralFormattingFailed);
                }

                var quotedColumnName = DmlPreviewSqlBuilder.QuoteIdentifier(request.DataSourceType, columnName);

                if (string.IsNullOrWhiteSpace(quotedColumnName))
                {
                    return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PrimaryKeyColumnNameMissing);
                }

                if (string.Equals(sqlValue, nullKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add($"{quotedColumnName}{isKeyword}{nullKeyword}");
                }
                else
                {
                    conditions.Add($"{quotedColumnName} = {sqlValue}");
                }
            }

            if (conditions.Count != orderedColumns.Count)
            {
                return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PrimaryKeyMetadataMissing);
            }

            var condition = string.Join($"\r\n   {andKeyword} ", conditions);

            return DmlPreviewWhereConditionResult.Succeeded(condition, DmlPreviewWhereConditionSource.PrimaryKey);
        }

        private static DmlPreviewWhereConditionResult BuildPhysicalRowIdentifierCondition(DmlPreviewWhereConditionRequest request, DataRow originalRow)
        {
            string physicalIdentifierName;

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        physicalIdentifierName = request.UseUpperCaseKeywords ? "ROWID" : "rowid";
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        physicalIdentifierName = request.UseUpperCaseKeywords ? "CTID" : "ctid";
                        break;
                    }
                default:
                    {
                        return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PhysicalRowIdentifierNotSupported);
                    }
            }

            if (originalRow.Table == null || !originalRow.Table.Columns.Contains(request.RowIdentityColumnName))
            {
                return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PhysicalRowIdentifierMissing);
            }

            var value = originalRow[request.RowIdentityColumnName];

            if (value == null || value == DBNull.Value)
            {
                return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PhysicalRowIdentifierMissing);
            }

            var physicalIdentifierValue = value.ToString();

            if (string.IsNullOrEmpty(physicalIdentifierValue))
            {
                return DmlPreviewWhereConditionResult.Failed(DmlPreviewWhereConditionFailureKind.PhysicalRowIdentifierMissing);
            }

            var escapedValue = DmlPreviewSqlLiteralFormatter.EscapeSqlString(physicalIdentifierValue);

            return DmlPreviewWhereConditionResult.Succeeded($"{physicalIdentifierName} = '{escapedValue}'", DmlPreviewWhereConditionSource.PhysicalRowIdentifier);
        }
    }
}