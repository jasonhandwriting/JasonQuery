using JasonQuery.Core.Database.Connection;
using System;

namespace JasonQuery.Core.Database.DdlPreview
{
    internal static class TableDdlSqlBuilder
    {
        public static string Build(TableDdlRequest request)
        {
            if (request == null)
            {
                return string.Empty;
            }

            switch (request.Operation)
            {
                case TableDdlOperation.Comment:
                    {
                        return BuildComment(request);
                    }
                case TableDdlOperation.Drop:
                    {
                        return BuildDrop(request);
                    }
                case TableDdlOperation.Rename:
                    {
                        return BuildRename(request);
                    }
                case TableDdlOperation.Truncate:
                    {
                        return BuildTruncate(request);
                    }
                case TableDdlOperation.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildComment(TableDdlRequest request)
        {
            var tableName = BuildQualifiedTableName(request);

            if (string.IsNullOrEmpty(tableName))
            {
                return string.Empty;
            }

            var comment = EscapeSqlString(request.Comment);

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return $"COMMENT ON TABLE {tableName} IS '{comment}';";
                    }
                case DataSourceType.SqlServer:
                    {
                        var procedureName = BuildSqlServerProcedureName(request.SchemaDatabase, request.HasExistingSqlServerComment ? "sp_updateextendedproperty" : "sp_addextendedproperty");
                        var schemaValue = GetIdentifierValue(DataSourceType.SqlServer, request.SchemaName);
                        var tableValue = GetIdentifierValue(DataSourceType.SqlServer, request.TableName);

                        if (string.IsNullOrEmpty(procedureName) || string.IsNullOrEmpty(schemaValue) || string.IsNullOrEmpty(tableValue))
                        {
                            return string.Empty;
                        }

                        return $"EXEC {procedureName} "
                               + $"@name=N'MS_Description', "
                               + $"@value=N'{comment}', "
                               + $"@level0type=N'SCHEMA', "
                               + $"@level0name=N'{EscapeSqlString(schemaValue)}', "
                               + $"@level1type=N'TABLE', "
                               + $"@level1name=N'{EscapeSqlString(tableValue)}';";
                    }
                case DataSourceType.MySql:
                    {
                        return $"ALTER TABLE {tableName} COMMENT = '{comment}';";
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildDrop(TableDdlRequest request)
        {
            var tableName = BuildQualifiedTableName(request);

            if (string.IsNullOrEmpty(tableName))
            {
                return string.Empty;
            }

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        var cascadeConstraints = request.OracleCascadeConstraints ? " CASCADE CONSTRAINTS" : string.Empty;
                        var purge = request.OraclePurge ? " PURGE" : string.Empty;

                        return $"DROP TABLE {tableName}{cascadeConstraints}{purge};";
                    }
                case DataSourceType.PostgreSql:
                    {
                        var cascade = request.PostgreSqlDropCascade ? " CASCADE" : string.Empty;

                        return $"DROP TABLE {tableName}{cascade};";
                    }
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        return $"DROP TABLE {tableName};";
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildRename(TableDdlRequest request)
        {
            const DdlNewIdentifierCaseMode caseMode = DdlNewIdentifierCaseMode.PreserveInput;

            var tableName = BuildQualifiedTableName(request);
            var newTableName = DdlIdentifierQuotingPolicy.QuoteNewIdentifier(request.DataSourceType, request.NewTableName, caseMode);

            if (string.IsNullOrEmpty(tableName)
                || string.IsNullOrEmpty(newTableName)
                || DdlIdentifierQuotingPolicy.AreExistingAndNewEquivalent(request.DataSourceType, request.TableName, request.NewTableName, caseMode))
            {
                return string.Empty;
            }

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return $"ALTER TABLE {tableName} RENAME TO {newTableName};";
                    }
                case DataSourceType.SqlServer:
                    {
                        var procedureName = BuildSqlServerProcedureName(request.SchemaDatabase, "sp_rename");
                        var objectName = DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.SqlServer, request.SchemaName, request.TableName);
                        var newNameValue = DdlIdentifierQuotingPolicy.GetNewIdentifierValue(DataSourceType.SqlServer, request.NewTableName, caseMode);

                        if (string.IsNullOrEmpty(procedureName) || string.IsNullOrEmpty(objectName) || string.IsNullOrEmpty(newNameValue))
                        {
                            return string.Empty;
                        }

                        return $"EXEC {procedureName} "
                               + $"@objname=N'{EscapeSqlString(objectName)}', "
                               + $"@newname=N'{EscapeSqlString(newNameValue)}';";
                    }
                case DataSourceType.MySql:
                    {
                        var databaseName = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.MySql, GetMySqlDatabaseName(request));
                        var renamedTableName = DdlIdentifierQuotingPolicy.QuoteNewIdentifier(DataSourceType.MySql, request.NewTableName, caseMode);

                        if (string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(renamedTableName))
                        {
                            return string.Empty;
                        }

                        return $"RENAME TABLE {tableName} "
                               + $"TO {databaseName}.{renamedTableName};";
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildTruncate(TableDdlRequest request)
        {
            var tableName = BuildQualifiedTableName(request);

            if (string.IsNullOrEmpty(tableName))
            {
                return string.Empty;
            }

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        var storage = request.OracleReuseStorage ? " REUSE STORAGE" : string.Empty;

                        return $"TRUNCATE TABLE {tableName}{storage};";
                    }
                case DataSourceType.PostgreSql:
                    {
                        var only = request.PostgreSqlOnly ? "ONLY " : string.Empty;
                        var restartIdentity = request.PostgreSqlRestartIdentity ? " RESTART IDENTITY" : string.Empty;
                        var cascade = request.PostgreSqlTruncateCascade ? " CASCADE" : string.Empty;

                        return $"TRUNCATE TABLE {only}{tableName}{restartIdentity}{cascade};";
                    }
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        return $"TRUNCATE TABLE {tableName};";
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildQualifiedTableName(TableDdlRequest request)
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
                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(DataSourceType.MySql, GetMySqlDatabaseName(request), request.TableName);
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string GetMySqlDatabaseName(TableDdlRequest request)
        {
            return !string.IsNullOrWhiteSpace(request.SchemaDatabase) ? request.SchemaDatabase : request.SchemaName;
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
            return DdlIdentifierQuotingPolicy.GetExistingIdentifierValue
            (
                dataSourceType,
                identifier
            );
        }

        private static string EscapeSqlString(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
