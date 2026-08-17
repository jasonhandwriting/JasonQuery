using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;

namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal static class ColumnAddSqlBuilder
    {
        public static ColumnAddBuildResult Build(ColumnAddRequest request)
        {
            if (request == null || request.Column == null)
            {
                return Failure(ColumnAddBuildFailureKind.MissingRequest,
                               "An Add Column request is required.");
            }

            if (request.DataSourceType == DataSourceType.None)
            {
                return Failure(ColumnAddBuildFailureKind.UnsupportedDataSource,
                               "The current database type is not supported.");
            }

            if (string.IsNullOrWhiteSpace(request.TableName))
            {
                return Failure(ColumnAddBuildFailureKind.MissingTableName,
                               "A table name is required.");
            }

            var rawColumnName = (request.Column.ColumnName ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(rawColumnName))
            {
                return Failure(ColumnAddBuildFailureKind.MissingColumnName,
                               "A column name is required.");
            }

            if (!DdlInputSafetyValidator.IsSafeSingleIdentifier(rawColumnName, request.DataSourceType))
            {
                return Failure(ColumnAddBuildFailureKind.UnsafeColumnName,
                               "The column name is invalid or contains unsupported characters.");
            }

            var columnName = DdlIdentifierQuotingPolicy.QuoteNewIdentifier(request.DataSourceType, rawColumnName);

            if (string.IsNullOrEmpty(columnName))
            {
                return Failure(ColumnAddBuildFailureKind.UnsafeColumnName,
                               "The column name is invalid or contains unsupported characters.");
            }

            var typeDefinition = ResolveTypeDefinition(request.DataSourceType, request.Column);

            if (typeDefinition == null)
            {
                return Failure(ColumnAddBuildFailureKind.MissingDataType,
                               "A data type is required.");
            }

            var typeResult = ColumnTypeSqlBuilder.Build(typeDefinition, request.Column);

            if (!typeResult.Succeeded)
            {
                return Failure(ColumnAddBuildFailureKind.InvalidDataType, typeResult.ErrorMessage);
            }

            var defaultResult = ColumnDefaultValueFormatter.Format(request.DataSourceType, typeDefinition, request.Column);

            if (!defaultResult.Succeeded)
            {
                return Failure(ColumnAddBuildFailureKind.InvalidDefaultValue, defaultResult.ErrorMessage, typeResult.ResolvedDataType);
            }

            var tableName = BuildTableName(request);

            if (string.IsNullOrEmpty(tableName))
            {
                return Failure(ColumnAddBuildFailureKind.MissingTableName,
                               "The database or schema name required by the current database is missing.",
                               typeResult.ResolvedDataType);
            }

            var nullClause = request.Column.NullAllowed ? " NULL" : " NOT NULL";
            string sql;

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        sql = $"ALTER TABLE {tableName} ADD {columnName} "
                              + $"{typeResult.ResolvedDataType}"
                              + $"{defaultResult.SqlClause}"
                              + $"{nullClause};";
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        sql = $"ALTER TABLE {tableName} ADD COLUMN {columnName} "
                              + $"{typeResult.ResolvedDataType}"
                              + $"{defaultResult.SqlClause}"
                              + $"{nullClause};";
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        sql = $"ALTER TABLE {tableName} ADD {columnName} "
                              + $"{typeResult.ResolvedDataType}"
                              + $"{nullClause}"
                              + $"{defaultResult.SqlClause};";
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        sql = $"ALTER TABLE {tableName} ADD COLUMN {columnName} "
                              + $"{typeResult.ResolvedDataType}"
                              + $"{nullClause}"
                              + $"{defaultResult.SqlClause};";
                        break;
                    }
                case DataSourceType.None:
                default:
                    {
                        return Failure(ColumnAddBuildFailureKind.UnsupportedDataSource,
                                       "The current database type is not supported.",
                                       typeResult.ResolvedDataType);
                    }
            }

            return new ColumnAddBuildResult
            {
                Succeeded = true,
                Sql = sql,
                ResolvedDataType = typeResult.ResolvedDataType,
                FailureKind = ColumnAddBuildFailureKind.None,
                ErrorMessage = string.Empty
            };
        }

        private static ColumnTypeDefinition ResolveTypeDefinition(DataSourceType dataSourceType, ColumnDefinition column)
        {
            if (!string.IsNullOrWhiteSpace(column.TypeKey))
            {
                var catalogType = ColumnTypeCatalog.Find(dataSourceType, column.TypeKey);

                if (catalogType != null)
                {
                    return catalogType;
                }
            }

            if (string.IsNullOrWhiteSpace(column.CustomTypeText))
            {
                return null;
            }

            return ColumnTypeCatalog.CreateCustom(dataSourceType, column.CustomTypeText.Trim());
        }

        private static string BuildTableName(ColumnAddRequest request)
        {
            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName
                        (
                            request.DataSourceType,
                            request.SchemaName,
                            request.TableName
                        );
                    }
                case DataSourceType.SqlServer:
                    {
                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName
                        (
                            DataSourceType.SqlServer,
                            request.SchemaDatabase,
                            request.SchemaName,
                            request.TableName
                        );
                    }
                case DataSourceType.MySql:
                    {
                        return DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName
                        (
                            DataSourceType.MySql,
                            request.SchemaDatabase,
                            request.TableName
                        );
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static ColumnAddBuildResult Failure(ColumnAddBuildFailureKind failureKind, string errorMessage, string resolvedDataType = "")
        {
            return new ColumnAddBuildResult
            {
                Succeeded = false,
                Sql = string.Empty,
                ResolvedDataType = resolvedDataType ?? string.Empty,
                FailureKind = failureKind,
                ErrorMessage = errorMessage ?? string.Empty
            };
        }
    }
}