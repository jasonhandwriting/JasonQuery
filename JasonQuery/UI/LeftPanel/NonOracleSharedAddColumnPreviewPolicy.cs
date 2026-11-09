using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.SchemaExplorer.Selection;
using System;
using System.Data;

namespace JasonQuery.UI.LeftPanel
{
    //F7-U2-B: a metadata-to-SQL-preview contract, not permission to execute DDL.
    public sealed class NonOracleSharedAddColumnPreviewTarget
    {
        public DataSourceType SourceType { get; internal set; }
        public string SchemaDatabase { get; internal set; } = string.Empty;
        public string SchemaName { get; internal set; } = string.Empty;
        public string TableName { get; internal set; } = string.Empty;
        public bool OfflineSqlPreviewOnly { get; internal set; } = true;
    }

    public static class NonOracleSharedAddColumnPreviewPolicy
    {
        public static bool TryQualifyColumn(SchemaExplorerSelectionInfo selection, DataRow selectedDataRow)
        {
            if (selection == null)
            {
                return false;
            }

            //Clear old qualification first; a rebind or failed check cannot reuse it.
            selection.HasQualifiedNonOracleColumnParent = false;
            selection.NonOracleRawParentTableName = string.Empty;
            selection.NonOracleRawColumnCaption = string.Empty;

            if (!IsNonOracle(selection.SourceType)
                || selection.Kind != SchemaExplorerSelectionKind.ColumnInfo
                || !selection.IsColumnInfoRow || selection.IsGroupRow
                || !SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Tables)
                || string.IsNullOrWhiteSpace(selection.SchemaName)
                || string.IsNullOrWhiteSpace(selection.SchemaNode)
                || selectedDataRow == null || selectedDataRow.Table == null
                || !HasRawColumns(selectedDataRow, "SchemaNode", "SchemaType", "SchemaName", "Schema_Browser"))
            {
                return false;
            }

            var rowScope = Raw(selectedDataRow, "SchemaNode");
            var rowType = Normalize(Raw(selectedDataRow, "SchemaType"));
            var rawTable = Normalize(Raw(selectedDataRow, "SchemaName"));
            var caption = Normalize(Raw(selectedDataRow, "Schema_Browser"));

            if (string.IsNullOrWhiteSpace(rawTable) || string.IsNullOrWhiteSpace(caption)
                || !string.Equals(rowScope, selection.SchemaNode, StringComparison.Ordinal)
                || !string.Equals(rowType, selection.SchemaType, StringComparison.Ordinal)
                || !string.Equals(caption, selection.SchemaName, StringComparison.Ordinal))
            {
                return false;
            }

            if (selection.SourceType == DataSourceType.SqlServer)
            {
                if (!HasRawColumns(selectedDataRow, "SchemaDbo", "ObjectID")
                    || string.IsNullOrWhiteSpace(selection.SchemaDbo)
                    || string.IsNullOrWhiteSpace(selection.ObjectId)
                    || !string.Equals(Raw(selectedDataRow, "SchemaDbo"), selection.SchemaDbo, StringComparison.Ordinal)
                    || !string.Equals(Raw(selectedDataRow, "ObjectID"), selection.ObjectId, StringComparison.Ordinal)
                    || !TryUnprefixSqlServerTable(rawTable, selection.SchemaDbo, out _))
                {
                    return false;
                }
            }

            selection.NonOracleRawParentTableName = rawTable;
            selection.NonOracleRawColumnCaption = caption;
            selection.HasQualifiedNonOracleColumnParent = true;
            return true;
        }

        public static bool IsQualifiedColumn(SchemaExplorerSelectionInfo selection)
        {
            if (selection == null || !selection.HasQualifiedNonOracleColumnParent
                || !IsNonOracle(selection.SourceType)
                || selection.Kind != SchemaExplorerSelectionKind.ColumnInfo
                || !selection.IsColumnInfoRow || selection.IsGroupRow
                || !SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Tables)
                || string.IsNullOrWhiteSpace(selection.SchemaNode)
                || string.IsNullOrWhiteSpace(selection.NonOracleRawParentTableName)
                || string.IsNullOrWhiteSpace(selection.NonOracleRawColumnCaption)
                || !string.Equals(selection.SchemaName, selection.NonOracleRawColumnCaption, StringComparison.Ordinal))
            {
                return false;
            }

            if (selection.SourceType != DataSourceType.SqlServer)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(selection.SchemaDbo)
                   && !string.IsNullOrWhiteSpace(selection.ObjectId)
                   && TryUnprefixSqlServerTable(selection.NonOracleRawParentTableName, selection.SchemaDbo, out _);
        }

        public static bool TryCreate(SchemaExplorerSelectionInfo selection, out NonOracleSharedAddColumnPreviewTarget target)
        {
            target = null;

            if (selection == null || !IsNonOracle(selection.SourceType)
                || !SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Tables)
                || string.IsNullOrWhiteSpace(selection.SchemaNode))
            {
                return false;
            }

            string rawTable;

            if (selection.Kind == SchemaExplorerSelectionKind.ColumnInfo || selection.IsColumnInfoRow)
            {
                if (!IsQualifiedColumn(selection))
                {
                    return false;
                }

                rawTable = selection.NonOracleRawParentTableName;
            }
            else
            {
                if (selection.Kind != SchemaExplorerSelectionKind.SchemaObject
                    || (selection.IsGroupRow && selection.GroupLevel != SchemaExplorerGroupLevelPolicy.GetMinimumObjectGroupLevel(selection.SourceType))
                    || string.IsNullOrWhiteSpace(selection.SchemaName))
                {
                    return false;
                }

                rawTable = selection.SchemaName;
            }

            if (string.IsNullOrWhiteSpace(rawTable))
            {
                return false;
            }

            var qualified = new NonOracleSharedAddColumnPreviewTarget { SourceType = selection.SourceType };

            switch (selection.SourceType)
            {
                case DataSourceType.PostgreSql:
                    {
                        qualified.SchemaName = selection.SchemaNode;
                        qualified.TableName = rawTable;
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        if (string.IsNullOrWhiteSpace(selection.SchemaDbo)
                            || string.IsNullOrWhiteSpace(selection.ObjectId)
                            || !TryUnprefixSqlServerTable(rawTable, selection.SchemaDbo, out var tableName))
                        {
                            return false;
                        }

                        qualified.SchemaDatabase = selection.SchemaNode;
                        qualified.SchemaName = selection.SchemaDbo;
                        qualified.TableName = tableName;
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        qualified.SchemaDatabase = selection.SchemaNode;
                        qualified.TableName = rawTable;
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            if (string.IsNullOrWhiteSpace(qualified.TableName))
            {
                return false;
            }

            target = qualified;
            return true;
        }

        public static bool IsSameSelection(SchemaExplorerSelectionInfo captured, SchemaExplorerSelectionInfo current)
        {
            if (!TryCreate(captured, out var before) || !TryCreate(current, out var after))
            {
                return false;
            }

            return captured.DisplayRowIndex == current.DisplayRowIndex
                   && captured.Kind == current.Kind
                   && captured.IsGroupRow == current.IsGroupRow
                   && captured.GroupLevel == current.GroupLevel
                   && captured.IsColumnInfoRow == current.IsColumnInfoRow
                   && captured.SourceType == current.SourceType
                   && string.Equals(captured.SchemaType, current.SchemaType, StringComparison.Ordinal)
                   && string.Equals(captured.SchemaName, current.SchemaName, StringComparison.Ordinal)
                   && string.Equals(captured.SchemaNode, current.SchemaNode, StringComparison.Ordinal)
                   && string.Equals(captured.SchemaDbo, current.SchemaDbo, StringComparison.Ordinal)
                   && string.Equals(captured.ObjectId, current.ObjectId, StringComparison.Ordinal)
                   && string.Equals(captured.NonOracleRawParentTableName, current.NonOracleRawParentTableName, StringComparison.Ordinal)
                   && string.Equals(captured.NonOracleRawColumnCaption, current.NonOracleRawColumnCaption, StringComparison.Ordinal)
                   && string.Equals(before.SchemaDatabase, after.SchemaDatabase, StringComparison.Ordinal)
                   && string.Equals(before.SchemaName, after.SchemaName, StringComparison.Ordinal)
                   && string.Equals(before.TableName, after.TableName, StringComparison.Ordinal);
        }

        private static bool IsNonOracle(DataSourceType source)
        {
            return source == DataSourceType.PostgreSql
                   || source == DataSourceType.SqlServer
                   || source == DataSourceType.MySql;
        }

        private static bool TryUnprefixSqlServerTable(string qualifiedName, string schema, out string tableName)
        {
            tableName = string.Empty;

            if (string.IsNullOrWhiteSpace(qualifiedName) || string.IsNullOrWhiteSpace(schema))
            {
                return false;
            }

            var prefix = schema + ".";

            if (!qualifiedName.StartsWith(prefix, StringComparison.Ordinal))
            {
                return false;
            }

            tableName = qualifiedName.Substring(prefix.Length);
            return !string.IsNullOrWhiteSpace(tableName);
        }

        private static string Normalize(string value)
        {
            var candidate = value ?? string.Empty;
            var index = candidate.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);
            return index >= 0 ? candidate.Substring(0, index) : candidate;
        }

        private static bool HasRawColumns(DataRow row, params string[] required)
        {
            if (row == null || row.Table == null)
            {
                return false;
            }

            foreach (var name in required)
            {
                if (!row.Table.Columns.Contains(name))
                {
                    return false;
                }
            }

            return true;
        }

        private static string Raw(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName)
                || row.IsNull(columnName))
            {
                return string.Empty;
            }

            return Convert.ToString(row[columnName]);
        }
    }
}
