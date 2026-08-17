using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.CreateScript.Table;
using System;
using System.Data;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.SqlServer
{
    internal static class SqlServerTableCreateScriptAssembler
    {
        public static string Build(SqlServerCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            var sql = string.Empty;
            var sbScript = new StringBuilder();
            const string PlaceholderDefault = "```DEFAULT```";

            sql = SqlServerTableCreateScriptSqlBuilder.BuildTableObjectInfoSql(request);

            var dtObjectInfo = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);
            var createDate = string.Empty;
            var modifyDate = string.Empty;

            if (dtObjectInfo?.Rows.Count > 0)
            {
                var drObject = dtObjectInfo.Rows[0];

                request.ObjectID = drObject.GetSafeString("Object_ID");
                createDate = drObject.GetSafeDateTimeText("Create_Date", request.DateTimeFormat);
                modifyDate = drObject.GetSafeDateTimeText("Modify_Date", request.DateTimeFormat);
            }

            sbScript.AppendLine($"-- Table: {request.SchemaDbo}.{request.SchemaName}");
            sbScript.AppendLine($"-- Created: {createDate}");
            sbScript.AppendLine($"-- Modified: {modifyDate}");
            sbScript.AppendLine();
            sbScript.AppendLine($"-- DROP TABLE [{request.SchemaDbo}].[{request.SchemaName}];");
            sbScript.AppendLine();
            sbScript.AppendLine($"CREATE TABLE [{request.SchemaDbo}].[{request.SchemaName}]");
            sbScript.AppendLine("(");

            sql = SqlServerTableCreateScriptSqlBuilder.BuildTableConstraintSql(request);

            var dtConstraint = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

            if (dtConstraint != null)
            {
                DataTableSortHelper.SortDataTable(dtConstraint, "Constraint_Name, Ordinal_Position");
            }

            sql = SqlServerTableCreateScriptSqlBuilder.BuildColumnCommentSql(request);

            var dtColumnComment = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

            sql = SqlServerTableCreateScriptSqlBuilder.BuildColumnInfoSql(request);

            var dtColumnInfo = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);
            var primaryKeyConstraintName = string.Empty;
            var hasForeignKey = (dtConstraint?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                .Any(dr => string.Equals(dr.GetSafeString("Constraint_Type"), "FOREIGN KEY", StringComparison.OrdinalIgnoreCase));

            if (dtColumnInfo?.Rows.Count > 0)
            {
                for (var i = 0; i < dtColumnInfo.Rows.Count; i++)
                {
                    var drColumn = dtColumnInfo.Rows[i];
                    var columnName = drColumn.GetSafeString("Column_Name");
                    var lineBreak = i == dtColumnInfo.Rows.Count - 1 ? string.Empty : ",\r\n";

                    sbScript.Append(BuildSqlServerColumnDefinition(drColumn, columnName, PlaceholderDefault, lineBreak));

                    if (string.IsNullOrWhiteSpace(primaryKeyConstraintName) && dtConstraint?.Rows.Count > 0)
                    {
                        var drPk = DataTableSearchHelper.FindDataTableFirstRow(dtConstraint, ("Column_Name", columnName));

                        if (drPk != null && string.Equals(drPk.GetSafeString("Constraint_Type"), "PRIMARY KEY", StringComparison.OrdinalIgnoreCase))
                        {
                            primaryKeyConstraintName = drPk.GetSafeString("Constraint_Name");
                        }
                    }

                    var columnDefault = drColumn.GetSafeString("Column_Default");

                    columnDefault = SqlServerCreateScriptExecutionHelper.RemoveWrappingParentheses(columnDefault, 2);

                    var defaultSql = string.IsNullOrWhiteSpace(columnDefault) ? string.Empty : $" DEFAULT {columnDefault}";

                    sbScript.Replace(PlaceholderDefault, defaultSql);
                }

                sql = SqlServerTableCreateScriptSqlBuilder.BuildPrimaryKeyWithSql(request, primaryKeyConstraintName);

                var dtPkWith = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

                if (dtPkWith?.Rows.Count > 0)
                {
                    sbScript.AppendLine(",");
                    sbScript.AppendLine($"    CONSTRAINT [{primaryKeyConstraintName}] PRIMARY KEY CLUSTERED");
                    sbScript.AppendLine("    (");

                    sql = SqlServerTableCreateScriptSqlBuilder.BuildPrimaryKeyColumnOrderSql(request, primaryKeyConstraintName);

                    var dtPkColumns = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

                    for (var i = 0; i < (dtPkColumns?.Rows.Count ?? 0); i++)
                    {
                        var drPkColumn = dtPkColumns.Rows[i];
                        var columnID = drPkColumn.GetSafeInt("Column_ID");
                        var columnName = string.Empty;

                        foreach (DataRow drColumn in dtColumnInfo.Rows)
                        {
                            if (drColumn.GetSafeInt("Ordinal_Position") == columnID)
                            {
                                columnName = drColumn.GetSafeString("Column_Name");
                                break;
                            }
                        }

                        var sortDirection = string.Equals(drPkColumn.GetSafeString("Is_Descending_Key"), "TRUE", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
                        var lineBreak = i == dtPkColumns.Rows.Count - 1 ? "\r\n" : ",\r\n";

                        sbScript.Append($"        [{columnName}] {sortDirection}{lineBreak}");
                    }

                    var drPkWith = dtPkWith.Rows[0];
                    var padded = string.Equals(drPkWith.GetSafeString("Is_Padded"), "TRUE", StringComparison.OrdinalIgnoreCase) ? "ON" : "OFF";
                    var noRecompute = string.Equals(drPkWith.GetSafeString("No_Recompute"), "TRUE", StringComparison.OrdinalIgnoreCase) ? "ON" : "OFF";
                    var ignoreDupKey = string.Equals(drPkWith.GetSafeString("Ignore_Dup_Key"), "TRUE", StringComparison.OrdinalIgnoreCase) ? "ON" : "OFF";
                    var allowRowLocks = string.Equals(drPkWith.GetSafeString("Allow_Row_Locks"), "TRUE", StringComparison.OrdinalIgnoreCase) ? "ON" : "OFF";
                    var allowPageLocks = string.Equals(drPkWith.GetSafeString("Allow_Page_Locks"), "TRUE", StringComparison.OrdinalIgnoreCase) ? "ON" : "OFF";
                    var groupName = drPkWith.GetSafeString("GroupName");

                    sbScript.AppendLine("    )");
                    sbScript.AppendLine("    WITH");
                    sbScript.AppendLine("    (");
                    sbScript.AppendLine($"        PAD_INDEX = {padded}, STATISTICS_NORECOMPUTE = {noRecompute}, IGNORE_DUP_KEY = {ignoreDupKey},");

                    var finalIndexOptionLine = $"        ALLOW_ROW_LOCKS = {allowRowLocks}, " + $"ALLOW_PAGE_LOCKS = {allowPageLocks}";

                    if (request.SupportsOptimizeForSequentialKey)
                    {
                        var optimizeForSequentialKey = string.Equals
                        (
                            drPkWith.GetSafeString
                            (
                                "Optimize_For_Sequential_Key"
                            ),
                            "TRUE",
                            StringComparison.OrdinalIgnoreCase
                        ) ? "ON" : "OFF";

                        finalIndexOptionLine += $", OPTIMIZE_FOR_SEQUENTIAL_KEY = " + $"{optimizeForSequentialKey}";
                    }

                    sbScript.AppendLine(finalIndexOptionLine);
                    sbScript.AppendLine($"    ) ON [{groupName}]");
                    sbScript.AppendLine($") ON [{groupName}] TEXTIMAGE_ON [{groupName}]");
                    sbScript.AppendLine("GO");
                    sbScript.AppendLine();
                }
                else
                {
                    sbScript.AppendLine();
                    sbScript.AppendLine(")");
                    sbScript.AppendLine("GO");
                    sbScript.AppendLine();
                }

                sql = SqlServerTableCreateScriptSqlBuilder.BuildDefaultConstraintSql(request);

                var dtConstraintDefault = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

                foreach (DataRow drDefault in dtConstraintDefault?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var constraintName = drDefault.GetSafeString("DefaultConstraintName");
                    var columnName = drDefault.GetSafeString("ColumnName");
                    var defaultValue = drDefault.GetSafeString("DefaultValue");

                    sbScript.AppendLine($"ALTER TABLE [{request.SchemaDbo}].[{request.SchemaName}] ADD CONSTRAINT [{constraintName}] DEFAULT {defaultValue} FOR [{columnName}]");
                    sbScript.AppendLine("GO");
                    sbScript.AppendLine();
                }

                if (hasForeignKey)
                {
                    sql = SqlServerTableCreateScriptSqlBuilder.BuildForeignKeySql(request);

                    var dtConstraintFk = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

                    foreach (var group in (dtConstraintFk?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                           .GroupBy(dr => dr.GetSafeString("ForeignKeyName"), StringComparer.OrdinalIgnoreCase))
                    {
                        var drFirst = group.First();
                        var constraintName = drFirst.GetSafeString("ForeignKeyName");
                        var thisTable = drFirst.GetSafeString("ThisTable");
                        var referencedTable = drFirst.GetSafeString("ReferencedTable");
                        var onUpdateRule = drFirst.GetSafeString("OnUpdateRule").Replace("_", " ");
                        var onDeleteRule = drFirst.GetSafeString("OnDeleteRule").Replace("_", " ");
                        string thisColumns;
                        string referencedColumns;

                        if (group.Count() == 1)
                        {
                            thisColumns = $"[{drFirst.GetSafeString("ThisColumn")}]";
                            referencedColumns = $"[{drFirst.GetSafeString("ReferencedColumn")}]";
                        }
                        else
                        {
                            var sbThisColumns = new StringBuilder();
                            var sbReferencedColumns = new StringBuilder();

                            foreach (var drFk in group)
                            {
                                sbThisColumns.Append($"[{drFk.GetSafeString("ThisColumn")}], ");
                                sbReferencedColumns.Append($"[{drFk.GetSafeString("ReferencedColumn")}], ");
                            }

                            if (sbThisColumns.Length >= 2)
                            {
                                sbThisColumns.Length -= 2;
                            }

                            if (sbReferencedColumns.Length >= 2)
                            {
                                sbReferencedColumns.Length -= 2;
                            }

                            thisColumns = sbThisColumns.ToString();
                            referencedColumns = sbReferencedColumns.ToString();
                        }

                        sbScript.AppendLine($"ALTER TABLE [{request.SchemaDbo}].[{thisTable}]  WITH CHECK ADD  CONSTRAINT [{constraintName}] FOREIGN KEY({thisColumns})");
                        sbScript.AppendLine($"REFERENCES [{request.SchemaDbo}].[{referencedTable}] ({referencedColumns})");
                        sbScript.AppendLine($"ON UPDATE {onUpdateRule}");
                        sbScript.AppendLine($"ON DELETE {onDeleteRule}");
                        sbScript.AppendLine("GO");
                        sbScript.AppendLine();
                        sbScript.AppendLine($"ALTER TABLE [{request.SchemaDbo}].[{request.SchemaName}] CHECK CONSTRAINT [{constraintName}]");
                        sbScript.AppendLine("GO");
                        sbScript.AppendLine();
                    }
                }

                foreach (DataRow drCheck in dtConstraint?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    if (!string.Equals(drCheck.GetSafeString("Constraint_Type"), "CHECK", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var constraintName = drCheck.GetSafeString("Constraint_Name");
                    var checkClause = drCheck.GetSafeString("Check_Clause");

                    sbScript.AppendLine($"ALTER TABLE [{request.SchemaDbo}].[{request.SchemaName}] WITH CHECK ADD CONSTRAINT [{constraintName}] CHECK ({checkClause})");
                    sbScript.AppendLine("GO");
                    sbScript.AppendLine();
                    sbScript.AppendLine($"ALTER TABLE [{request.SchemaDbo}].[{request.SchemaName}] CHECK CONSTRAINT [{constraintName}]");
                    sbScript.AppendLine("GO");
                    sbScript.AppendLine();
                }

                sql = SqlServerTableCreateScriptSqlBuilder.BuildTableCommentSql(request);

                var dtTableComment = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

                if (dtTableComment?.Rows.Count > 0)
                {
                    var comment = dtTableComment.Rows[0].GetSafeString("Comment").Replace("'", "''");

                    sbScript.AppendLine("--Table's Comment");
                    sbScript.AppendLine($"EXEC {request.SchemaNode}.sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{comment}', @level0type=N'SCHEMA', @level0name=N'{request.SchemaDbo}', @level1type=N'TABLE', @level1name=N'{request.SchemaName}'");
                    sbScript.AppendLine("GO");
                    sbScript.AppendLine();
                }

                var columnCommentHeader = "--Column Comment";

                foreach (DataRow drColumnComment in dtColumnComment?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var columnName = drColumnComment.GetSafeString("Column_Name");
                    var comment = drColumnComment.GetSafeString("Comment").Replace("'", "''");

                    sbScript.AppendLine(columnCommentHeader);
                    sbScript.AppendLine($"EXEC {request.SchemaNode}.sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{comment}', @level0type=N'SCHEMA', @level0name=N'{request.SchemaDbo}', @level1type=N'TABLE', @level1name=N'{request.SchemaName}', @level2type=N'COLUMN', @level2name=N'{columnName}'");
                    sbScript.AppendLine("GO");
                    sbScript.AppendLine();

                    columnCommentHeader = string.Empty;
                }

                sql = SqlServerTableCreateScriptSqlBuilder.BuildNonClusteredIndexSummarySql(request);

                var dtIndexes = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

                foreach (DataRow drIndexSummary in dtIndexes?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var indexName = drIndexSummary.GetSafeString("Name");

                    sql = SqlServerTableCreateScriptSqlBuilder.BuildNonClusteredIndexColumnSql(request, request.ObjectID, indexName);

                    var dtIndexColumns = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

                    if (dtIndexColumns?.Rows.Count <= 0)
                    {
                        continue;
                    }

                    var temp = sbScript.ToString();

                    if (temp.EndsWith("GO", StringComparison.Ordinal))
                    {
                        sbScript.AppendLine();
                        sbScript.AppendLine();
                    }

                    sbScript.Append($"CREATE NONCLUSTERED INDEX {indexName} ON {request.SchemaDbo}.{request.SchemaName}");
                    sbScript.AppendLine();
                    sbScript.AppendLine("(");

                    for (var i = 0; i < dtIndexColumns.Rows.Count; i++)
                    {
                        var drIndexColumn = dtIndexColumns.Rows[i];
                        var indexColumnName = drIndexColumn.GetSafeString("Index_Column_Name");
                        var indexColumnID = drIndexColumn.GetSafeString("Index_Column_ID");
                        var linePrefix = i == 0 ? "    " : ",\r\n    ";

                        sbScript.Append(linePrefix);
                        sbScript.Append($"[{indexColumnName}]");

                        sql = SqlServerTableCreateScriptSqlBuilder.BuildIndexSortDirectionSql(request, indexName, indexColumnID);

                        var dtIndexSort = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

                        if (dtIndexSort?.Rows.Count > 0)
                        {
                            sbScript.Append(string.Equals(dtIndexSort.Rows[0].GetSafeString("Is_Descending_Key"), "TRUE", StringComparison.OrdinalIgnoreCase) ? " DESC" : " ASC");
                        }
                    }

                    var padded = string.Equals(drIndexSummary.GetSafeString("Is_Padded"), "FALSE", StringComparison.OrdinalIgnoreCase) ? "OFF" : "ON";
                    var noRecompute = string.Equals(drIndexSummary.GetSafeString("No_Recompute"), "FALSE", StringComparison.OrdinalIgnoreCase) ? "OFF" : "ON";
                    var allowRowLocks = string.Equals(drIndexSummary.GetSafeString("Allow_Row_Locks"), "FALSE", StringComparison.OrdinalIgnoreCase) ? "OFF" : "ON";
                    var allowPageLocks = string.Equals(drIndexSummary.GetSafeString("Allow_Page_Locks"), "FALSE", StringComparison.OrdinalIgnoreCase) ? "OFF" : "ON";
                    var groupName = drIndexSummary.GetSafeString("GroupName");

                    sbScript.AppendLine();
                    sbScript.AppendLine(")");
                    sbScript.AppendLine("WITH");
                    sbScript.AppendLine("(");
                    sbScript.AppendLine($"    PAD_INDEX = {padded},");
                    sbScript.AppendLine("    DROP_EXISTING = OFF,");
                    sbScript.AppendLine($"    STATISTICS_NORECOMPUTE = {noRecompute},");
                    sbScript.AppendLine("    SORT_IN_TEMPDB = OFF,");
                    sbScript.AppendLine("    ONLINE = OFF,");
                    sbScript.AppendLine($"    ALLOW_ROW_LOCKS = {allowRowLocks},");
                    sbScript.AppendLine($"    ALLOW_PAGE_LOCKS = {allowPageLocks}");
                    sbScript.AppendLine(")");
                    sbScript.AppendLine($"ON [{groupName}]");
                    sbScript.AppendLine("GO");
                }
            }

            return sbScript.ToString().TrimEnd('\r', '\n');
        }

        private static string BuildSqlServerColumnDefinition(DataRow drColumn, string columnName, string placeholderDefault, string lineBreak)
        {
            var dataType = drColumn.GetSafeString("Data_Type");
            var nullable = string.Equals(drColumn.GetSafeString("Is_Nullable"), "NO", StringComparison.OrdinalIgnoreCase) ? " NOT NULL" : " NULL";
            var suffix = $"{placeholderDefault}{nullable}";
            var characterMaximumLength = drColumn.GetSafeString("Character_Maximum_Length");

            switch (dataType.ToUpperInvariant())
            {
                case "NCHAR":
                case "BINARY":
                case "CHAR":
                    {
                        return $"    [{columnName}] [{dataType}]({characterMaximumLength}){suffix}{lineBreak}";
                    }
                case "NVARCHAR":
                case "VBINARY":
                case "VARCHAR":
                case "VARBINARY":
                    {
                        return characterMaximumLength == "-1" ? $"    [{columnName}] [{dataType}](max){suffix}{lineBreak}"
                                                              : $"    [{columnName}] [{dataType}]({characterMaximumLength}){suffix}{lineBreak}";
                    }
                case "DECIMAL":
                case "NUMERIC":
                    {
                        return $"    [{columnName}] [{dataType}]({drColumn.GetSafeInt("Numeric_Precision")}, {drColumn.GetSafeInt("Numeric_Scale")}){suffix}{lineBreak}";
                    }
                case "TIME":
                case "DATETIME2":
                case "DATETIMEOFFSET":
                    {
                        return $"    [{columnName}] [{dataType}]({drColumn.GetSafeString("DateTime_Precision")}){suffix}{lineBreak}";
                    }
                default:
                    {
                        return $"    [{columnName}] [{dataType}]{suffix}{lineBreak}";
                    }
            }
        }
    }
}