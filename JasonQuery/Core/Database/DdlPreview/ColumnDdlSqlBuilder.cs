using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using System;

namespace JasonQuery.Core.Database.DdlPreview
{
    internal static class ColumnDdlSqlBuilder
    {
        public static string Build(ColumnDdlRequest request)
        {
            if (request == null)
            {
                return string.Empty;
            }

            switch (request.Operation)
            {
                case ColumnDdlOperation.Comment:
                    {
                        return BuildComment(request);
                    }
                case ColumnDdlOperation.Drop:
                    {
                        return BuildDrop(request);
                    }
                case ColumnDdlOperation.Rename:
                    {
                        return BuildRename(request);
                    }
                case ColumnDdlOperation.Add:
                    {
                        return BuildAdd(request);
                    }
                case ColumnDdlOperation.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildComment(ColumnDdlRequest request)
        {
            var comment = EscapeSqlString(request.Comment);

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        var columnName = DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(request.DataSourceType, request.SchemaName, request.TableName, request.ColumnName);

                        return string.IsNullOrEmpty(columnName) ? string.Empty : $"COMMENT ON COLUMN {columnName} IS '{comment}';";
                    }
                case DataSourceType.SqlServer:
                    {
                        var procedureName = BuildSqlServerProcedureName(request.SchemaDatabase, request.HasExistingSqlServerComment ? "sp_updateextendedproperty" : "sp_addextendedproperty");
                        var schemaValue = GetIdentifierValue(DataSourceType.SqlServer, request.SchemaName);
                        var tableValue = GetIdentifierValue(DataSourceType.SqlServer, request.TableName);
                        var columnValue = GetIdentifierValue(DataSourceType.SqlServer, request.ColumnName);

                        if (string.IsNullOrEmpty(procedureName) || string.IsNullOrEmpty(schemaValue) || string.IsNullOrEmpty(tableValue) || string.IsNullOrEmpty(columnValue))
                        {
                            return string.Empty;
                        }

                        return $"EXEC {procedureName} "
                               + $"@name=N'MS_Description', "
                               + $"@value=N'{comment}', "
                               + $"@level0type=N'SCHEMA', "
                               + $"@level0name=N'{EscapeSqlString(schemaValue)}', "
                               + $"@level1type=N'TABLE', "
                               + $"@level1name=N'{EscapeSqlString(tableValue)}', "
                               + $"@level2type=N'COLUMN', "
                               + $"@level2name=N'{EscapeSqlString(columnValue)}';";
                    }
                case DataSourceType.MySql:
                    {
                        if (string.IsNullOrWhiteSpace(request.ColumnType))
                        {
                            return string.Empty;
                        }

                        var tableName = BuildQualifiedTableName(request);
                        var columnName = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.MySql, request.ColumnName);

                        if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(columnName))
                        {
                            return string.Empty;
                        }

                        var columnType = NormalizeMySqlColumnTypeForComment(request.ColumnType);

                        return $"ALTER TABLE {tableName} "
                               + $"CHANGE COLUMN {columnName} {columnName} "
                               + $"{columnType} COMMENT '{comment}';";
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildDrop(ColumnDdlRequest request)
        {
            var tableName = BuildQualifiedTableName(request);
            var columnName = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(request.DataSourceType, request.ColumnName);

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(columnName))
            {
                return string.Empty;
            }

            return $"ALTER TABLE {tableName} DROP COLUMN {columnName};";
        }

        private static string BuildRename(ColumnDdlRequest request)
        {
            if (DdlIdentifierQuotingPolicy.AreExistingAndNewEquivalent(request.DataSourceType, request.ColumnName, request.NewColumnName))
            {
                return string.Empty;
            }

            var tableName = BuildQualifiedTableName(request);
            var columnName = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(request.DataSourceType, request.ColumnName);
            var newColumnName = DdlIdentifierQuotingPolicy.QuoteNewIdentifier(request.DataSourceType, request.NewColumnName);

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(newColumnName))
            {
                return string.Empty;
            }

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return $"ALTER TABLE {tableName} "
                               + $"RENAME COLUMN {columnName} TO {newColumnName};";
                    }
                case DataSourceType.SqlServer:
                    {
                        var procedureName = BuildSqlServerProcedureName(request.SchemaDatabase, "sp_rename");
                        var objectName = DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.SqlServer, request.SchemaName, request.TableName, request.ColumnName);
                        var newNameValue = DdlIdentifierQuotingPolicy.GetNewIdentifierValue(DataSourceType.SqlServer, request.NewColumnName);

                        if (string.IsNullOrEmpty(procedureName) || string.IsNullOrEmpty(objectName) || string.IsNullOrEmpty(newNameValue))
                        {
                            return string.Empty;
                        }

                        return $"EXEC {procedureName} "
                               + $"@objname=N'{EscapeSqlString(objectName)}', "
                               + $"@newname=N'{EscapeSqlString(newNameValue)}', "
                               + $"@objtype=N'COLUMN';";
                    }
                case DataSourceType.MySql:
                    {
                        if (string.IsNullOrWhiteSpace(request.ColumnType))
                        {
                            return string.Empty;
                        }

                        return $"ALTER TABLE {tableName} "
                               + $"CHANGE COLUMN {columnName} {newColumnName} "
                               + $"{request.ColumnType};";
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildAdd(ColumnDdlRequest request)
        {
            var result = ColumnAddSqlBuilder.Build
            (
                new ColumnAddRequest
                {
                    DataSourceType = request.DataSourceType,
                    SchemaDatabase = request.SchemaDatabase,
                    SchemaName = request.SchemaName,
                    TableName = request.TableName,
                    Column = request.ColumnDefinition
                }
            );

            return result.Succeeded ? result.Sql : string.Empty;
        }

        private static string BuildQualifiedTableName(
            ColumnDdlRequest request)
        {
            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(request.DataSourceType, request.SchemaName, request.TableName);
                    }
                case DataSourceType.SqlServer:
                    {
                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.SqlServer, request.SchemaDatabase,
                                                                                     request.SchemaName, request.TableName);
                    }
                case DataSourceType.MySql:
                    {
                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.MySql, request.SchemaDatabase, request.TableName);
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildSqlServerProcedureName(string databaseName, string procedureName)
        {
            if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(procedureName))
            {
                return string.Empty;
            }

            return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName
            (
                DataSourceType.SqlServer,
                databaseName,
                "sys",
                procedureName
            );
        }

        private static string GetIdentifierValue(DataSourceType dataSourceType, string identifier)
        {
            return DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(dataSourceType, identifier);
        }

        private static string NormalizeMySqlColumnTypeForComment(string columnType)
        {
            return (columnType ?? string.Empty).Replace(" (", "(").Trim();
        }

        private static string EscapeSqlString(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}