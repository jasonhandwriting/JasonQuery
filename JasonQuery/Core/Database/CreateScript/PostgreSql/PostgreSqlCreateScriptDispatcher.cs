using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.CreateScript.PostgreSql.Formatting;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.CreateScript;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.CreateScript.Index;
using System;
using System.Data;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.PostgreSql
{
    internal static class PostgreSqlCreateScriptDispatcher
    {
        public static string Build(string schemaNode, string schemaType, string schemaName, string databaseName, string dbVersion,
                                   Func<string, DataTable> executeQuery)
        {
            if (executeQuery == null)
            {
                throw new ArgumentNullException(nameof(executeQuery));
            }

            var request = PostgreSqlCreateScriptRequest.Create(schemaNode, schemaType, schemaName, databaseName, dbVersion);
            var scripts = string.Empty;

            switch (request.SchemaType)
            {
                case SchemaObjectNames.Tables:
                    {
                        scripts = BuildTableScripts(request, executeQuery);
                        break;
                    }
                case SchemaObjectNames.Views:
                    {
                        scripts = BuildViewScripts(request, executeQuery);
                        break;
                    }
                case SchemaObjectNames.Functions:
                    {
                        scripts = BuildFunctionScripts(request, executeQuery);
                        break;
                    }
                case SchemaObjectNames.Procedures:
                    {
                        scripts = BuildProcedureScripts(request, executeQuery);
                        break;
                    }
                case SchemaObjectNames.Triggers:
                    {
                        scripts = BuildTriggerScripts(request, executeQuery);
                        break;
                    }
                case SchemaObjectNames.Indexes:
                    {
                        scripts = BuildIndexScripts(request, executeQuery);
                        break;
                    }
                case SchemaObjectNames.Packages:
                    {
                        scripts = BuildUnsupportedPackageScripts();
                        break;
                    }
            }

            if (!SchemaObjectTypeHelper.Is(request.SchemaType, SchemaObjectNames.Tables) && !string.IsNullOrWhiteSpace(scripts))
            {
                scripts = PostgreSqlCreateScriptFormatterDispatcher.Format(request.SchemaNode, request.SchemaType, request.SchemaName, scripts);
            }

            return scripts;
        }

        #region Tables
        private static string BuildTableScripts(PostgreSqlCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            var sql = PostgreSqlTableCreateScriptSqlBuilder.BuildTableDefinitionSql(request.SchemaNode, request.ObjectName);
            var dtTableDefinitionInfo = ExecuteQueryAndAppendLog(sql, executeQuery);

            if (dtTableDefinitionInfo == null || dtTableDefinitionInfo.Rows.Count <= 0)
            {
                return string.Empty;
            }

            var sqlScript = dtTableDefinitionInfo.Rows[0].GetSafeString("Sql");
            var oid = dtTableDefinitionInfo.Rows[0].GetSafeString("oid");
            var sbTableScripts = new StringBuilder();

            sbTableScripts.AppendLine($"-- TABLE: {request.SchemaNode}.{request.ObjectName}");
            sbTableScripts.AppendLine($"-- DROP TABLE IF EXISTS {request.SchemaNode}.{request.ObjectName};");
            sbTableScripts.AppendLine();
            sbTableScripts.AppendLine(sqlScript);

            #region Constraint
            var dtTableConstraintInfo = ExecuteQueryAndAppendLog
            (
                PostgreSqlTableCreateScriptSqlBuilder.BuildTableConstraintSql(oid),
                executeQuery
            );

            var tableScripts = sbTableScripts.ToString();

            foreach (DataRow dr in dtTableConstraintInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var constraintName = dr.GetSafeString("conname");
                var info = dr.GetSafeString("info");

                tableScripts = tableScripts.Replace("#$@\r\n", $",\r\n    CONSTRAINT {constraintName} {info}#$@\r\n");
            }

            tableScripts = tableScripts.Replace("#$@", "\r\n);");
            #endregion

            #region Owner
            var dtTableOwnerInfo = ExecuteQueryAndAppendLog
            (
                PostgreSqlTableCreateScriptSqlBuilder.BuildTableOwnerSql(request.SchemaNode, request.ObjectName),
                executeQuery
            );

            if (dtTableOwnerInfo?.Rows.Count > 0)
            {
                tableScripts += "\r\nALTER TABLE IF EXISTS ";
                tableScripts += $"{request.SchemaNode}.{request.ObjectName}\r\n";
                tableScripts += $"    OWNER to {dtTableOwnerInfo.Rows[0].GetSafeString("TableOwner")};";
            }
            #endregion

            #region Grant
            var dtTableGrantInfo = ExecuteQueryAndAppendLog
            (
                PostgreSqlTableCreateScriptSqlBuilder.BuildTableGrantSql(request.SchemaNode, request.ObjectName),
                executeQuery
            );

            if (dtTableGrantInfo?.Rows.Count > 0)
            {
                tableScripts += "\r\n\r\n-- GRANT INFORMATION\r\n\r\n";

                foreach (DataRow dr in dtTableGrantInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    tableScripts += $"{dr.GetSafeString("Grant_Info")}\r\n";
                }
            }
            #endregion

            #region Non-Unique Index
            var dtTableNonUniqueIndexInfo = ExecuteQueryAndAppendLog
            (
                PostgreSqlTableCreateScriptSqlBuilder.BuildTableNonUniqueIndexSql(request.SchemaNode, request.ObjectName),
                executeQuery
            );

            foreach (DataRow dr in dtTableNonUniqueIndexInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var indexName = dr.GetSafeString("IndexName");
                var indexDefinition = dr.GetSafeString("IndexDefinition");

                tableScripts += $"\r\n-- Index: {indexName}\r\n";
                tableScripts += $"-- DROP INDEX IF EXISTS {request.SchemaNode}.{indexName};\r\n\r\n";
                tableScripts += $"{indexDefinition}\r\n";
            }
            #endregion

            #region Table Comment
            var dtTableCommentInfo = ExecuteQueryAndAppendLog
            (
                PostgreSqlTableCreateScriptSqlBuilder.BuildTableCommentSql(request.SchemaNode, request.ObjectName),
                executeQuery
            );

            if (dtTableCommentInfo?.Rows.Count > 0)
            {
                var description = dtTableCommentInfo.Rows[0].GetSafeString("obj_description").Replace("'", "''");

                tableScripts += "\r\n-- TABLE COMMENT INFORMATION\r\n\r\n";
                tableScripts += $"COMMENT ON TABLE {request.SchemaNode}.{request.ObjectName}\r\n";
                tableScripts += $"    IS '{description}';\r\n";
            }
            #endregion

            #region Column Comment
            var dtColumnCommentInfo = ExecuteQueryAndAppendLog
            (
                PostgreSqlTableCreateScriptSqlBuilder.BuildColumnCommentSql(request.DatabaseName, request.SchemaNode, request.ObjectName),
                executeQuery
            );

            if (dtColumnCommentInfo?.Rows.Count > 0)
            {
                tableScripts += "\r\n-- COLUMN COMMENT INFORMATION\r\n\r\n";

                foreach (DataRow dr in dtColumnCommentInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var columnName = dr.GetSafeString("Column_Name");
                    var description = dr.GetSafeString("Description").Replace("'", "''");

                    tableScripts += $"COMMENT ON COLUMN {request.SchemaNode}.{request.ObjectName}.{columnName}\r\n";
                    tableScripts += $"    IS '{description}';\r\n\r\n";
                }
            }

            tableScripts = tableScripts.TrimEnd('\r', '\n');
            #endregion

            #region Table Trigger
            var dtTableTriggerInfo = ExecuteQueryAndAppendLog
            (
                PostgreSqlTableCreateScriptSqlBuilder.BuildTableTriggerSql(request.DatabaseName, request.SchemaNode, request.ObjectName),
                executeQuery
            );

            var sbTriggerScripts = new StringBuilder();

            foreach (DataRow dr in dtTableTriggerInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var triggerName = dr.GetSafeString("Trigger_Name");
                var actionTiming = dr.GetSafeString("Action_Timing");
                var eventManipulation = dr.GetSafeString("Event_Manipulation");
                var actionOrientation = dr.GetSafeString("Action_Orientation");
                var actionStatement = dr.GetSafeString("Action_Statement");

                sbTriggerScripts.AppendLine();
                sbTriggerScripts.AppendLine($"-- TRIGGER:{triggerName} ON {request.SchemaNode}.{request.ObjectName}");
                sbTriggerScripts.AppendLine($"-- DROP TRIGGER {triggerName} ON {request.SchemaNode}.{request.ObjectName}");
                sbTriggerScripts.AppendLine();
                sbTriggerScripts.AppendLine($"CREATE TRIGGER {triggerName}");
                sbTriggerScripts.AppendLine($"  {actionTiming} {eventManipulation}");
                sbTriggerScripts.AppendLine($"  ON {request.SchemaNode}.{request.ObjectName}");
                sbTriggerScripts.AppendLine($"  FOR EACH {actionOrientation}");
                sbTriggerScripts.AppendLine($"  {actionStatement};");
            }

            if (sbTriggerScripts.Length > 0)
            {
                tableScripts += "\r\n";
                tableScripts += sbTriggerScripts.ToString().TrimEnd('\r', '\n');
            }
            #endregion

            return tableScripts.TrimEnd('\r', '\n');
        }
        #endregion

        #region Views
        private static string BuildViewScripts(PostgreSqlCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            var dtViewDefinitionInfo = ExecuteQueryAndAppendLog
            (
                PostgreSqlViewCreateScriptSqlBuilder.BuildViewDefinitionSql(request.SchemaNode, request.ObjectName),
                executeQuery
            );

            if (dtViewDefinitionInfo?.Rows.Count <= 0)
            {
                return string.Empty;
            }

            var scripts = GetFirstColumnText(dtViewDefinitionInfo.Rows[0]);

            var dtViewOwnerInfo = ExecuteQueryAndAppendLog
            (
                PostgreSqlViewCreateScriptSqlBuilder.BuildViewOwnerSql(request.SchemaNode, request.ObjectName),
                executeQuery
            );

            if (dtViewOwnerInfo?.Rows.Count > 0)
            {
                scripts += $"\r\n\r\n{GetFirstColumnText(dtViewOwnerInfo.Rows[0])}";
            }

            return $"-- View: {request.SchemaNode}.{request.ObjectName}\r\n-- DROP VIEW {request.SchemaNode}.{request.ObjectName};\r\n\r\n{scripts}";
        }
        #endregion

        #region Functions
        private static string BuildFunctionScripts(PostgreSqlCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            string sql;

            if (string.IsNullOrWhiteSpace(request.RoutineArguments))
            {
                sql = request.IsVersion11OrAbove
                      ? PostgreSqlRoutineCreateScriptSqlBuilder.BuildFunctionDefinitionSql_Post11(request.SchemaNode, request.ObjectName)
                      : PostgreSqlRoutineCreateScriptSqlBuilder.BuildFunctionDefinitionSql_Pre11(request.SchemaNode, request.ObjectName);
            }
            else
            {
                sql = request.IsVersion11OrAbove
                      ? PostgreSqlRoutineCreateScriptSqlBuilder.BuildSpecificFunctionDefinitionSql_Post11(request.SchemaNode, request.ObjectName, request.RoutineArguments)
                      : PostgreSqlRoutineCreateScriptSqlBuilder.BuildFunctionDefinitionSql_Pre11(request.SchemaNode, request.ObjectName);
            }

            var dtFunctionDefinitionInfo = ExecuteQueryAndAppendLog(sql, executeQuery);
            var scripts = SelectRoutineDefinition(dtFunctionDefinitionInfo, request.RoutineNameLong);

            if (string.IsNullOrWhiteSpace(scripts))
            {
                return string.Empty;
            }

            var functionSignature = ExtractRoutineSignature(scripts);

            return $"-- Function: {request.SchemaNode}.{request.ObjectName}{functionSignature}\r\n" +
                   $"-- DROP FUNCTION {request.SchemaNode}.{request.ObjectName}{functionSignature};\r\n\r\n{scripts}";
        }
        #endregion

        #region Procedures
        private static string BuildProcedureScripts(PostgreSqlCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            if (!request.IsVersion11OrAbove)
            {
                return "-- PostgreSQL 10 及更早版本沒有原生 PROCEDURE 物件。";
            }

            string sql;

            if (string.IsNullOrWhiteSpace(request.RoutineArguments))
            {
                sql = PostgreSqlRoutineCreateScriptSqlBuilder.BuildProcedureDefinitionSql_Post11(request.SchemaNode, request.ObjectName);
            }
            else
            {
                sql = PostgreSqlRoutineCreateScriptSqlBuilder.BuildSpecificProcedureDefinitionSql_Post11(request.SchemaNode, request.ObjectName, request.RoutineArguments);
            }

            var dtProcedureDefinitionInfo = ExecuteQueryAndAppendLog(sql, executeQuery);
            var scripts = SelectRoutineDefinition(dtProcedureDefinitionInfo, request.RoutineNameLong);

            if (string.IsNullOrWhiteSpace(scripts))
            {
                return string.Empty;
            }

            var procedureSignature = ExtractRoutineSignature(scripts);

            return $"-- Procedure: {request.SchemaNode}.{request.ObjectName}{procedureSignature}\r\n" +
                   $"-- DROP PROCEDURE {request.SchemaNode}.{request.ObjectName}{procedureSignature};\r\n\r\n{scripts}";
        }
        #endregion

        #region Trigger Functions
        private static string BuildTriggerScripts(PostgreSqlCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            string sql;

            if (request.IsVersion11OrAbove)
            {
                sql = PostgreSqlTriggerCreateScriptSqlBuilder.BuildTriggerDefinitionSql11(request.SchemaNode, request.ObjectName);
            }
            else
            {
                sql = PostgreSqlTriggerCreateScriptSqlBuilder.BuildTriggerDefinitionSql10(request.SchemaNode, request.ObjectName);
            }

            var dtTriggerInfo = ExecuteQueryAndAppendLog(sql, executeQuery);

            if (dtTriggerInfo?.Rows.Count <= 0)
            {
                return string.Empty;
            }

            var scripts = GetFirstColumnText(dtTriggerInfo.Rows[0]);

            return $"-- Function: {request.SchemaNode}.{request.ObjectName}()\r\n" +
                   $"-- DROP FUNCTION {request.SchemaNode}.{request.ObjectName}();\r\n\r\n{scripts}";
        }
        #endregion

        #region Indexes
        private static string BuildIndexScripts(PostgreSqlCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            var dtIndexInfo = ExecuteQueryAndAppendLog
            (
                PostgreSqlIndexCreateScriptSqlBuilder.BuildIndexDefinitionSql(request.SchemaNode, request.ObjectName),
                executeQuery
            );

            if (dtIndexInfo?.Rows.Count <= 0)
            {
                return string.Empty;
            }

            var dr = dtIndexInfo.Rows[0];
            var indexName = dr.GetSafeString("IndexName");
            var indexDefinition = dr.GetSafeString("IndexDefinition");
            var indexOwner = dr.GetSafeString("IndexOwner");

            var sbScripts = new StringBuilder();

            sbScripts.AppendLine($"-- Index: {request.SchemaNode}.{indexName}");
            sbScripts.AppendLine($"-- DROP INDEX IF EXISTS {request.SchemaNode}.{indexName};");
            sbScripts.AppendLine();
            sbScripts.AppendLine(indexDefinition);

            if (!string.IsNullOrWhiteSpace(indexOwner))
            {
                sbScripts.AppendLine();
                sbScripts.AppendLine($"ALTER INDEX IF EXISTS {request.SchemaNode}.{indexName}");
                sbScripts.Append($"    OWNER TO {indexOwner};");
            }

            return sbScripts.ToString().TrimEnd('\r', '\n');
        }
        #endregion

        #region Packages
        private static string BuildUnsupportedPackageScripts()
        {
            var sb = new StringBuilder();

            sb.AppendLine("-- PostgreSQL community edition does not have a native PACKAGE object.");
            sb.AppendLine("-- Suggested alternatives:");
            sb.AppendLine("-- 1. Group related functions/procedures under the same schema.");
            sb.AppendLine("-- 2. Package related objects as an EXTENSION when appropriate.");

            return sb.ToString().TrimEnd('\r', '\n');
        }
        #endregion

        #region Helpers
        private static DataTable ExecuteQueryAndAppendLog(string sql, Func<string, DataTable> executeQuery)
        {
            return executeQuery(sql);
        }

        private static string GetFirstColumnText(DataRow dr)
        {
            if (dr == null || dr.Table == null || dr.Table.Columns.Count <= 0)
            {
                return string.Empty;
            }

            var columnName = dr.Table.Columns[0].ColumnName;

            return dr.GetSafeString(columnName);
        }

        private static string SelectRoutineDefinition(DataTable dtRoutineInfo, string routineNameLong)
        {
            if (dtRoutineInfo?.Rows.Count <= 0)
            {
                return string.Empty;
            }

            if (dtRoutineInfo.Rows.Count == 1)
            {
                return GetFirstColumnText(dtRoutineInfo.Rows[0]);
            }

            foreach (DataRow dr in dtRoutineInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var tempDefinition = GetFirstColumnText(dr);
                var matchedPos = tempDefinition.IndexOf(routineNameLong, StringComparison.Ordinal);

                if (matchedPos >= 0)
                {
                    return tempDefinition;
                }
            }

            return GetFirstColumnText(dtRoutineInfo.Rows[0]);
        }

        private static string ExtractRoutineSignature(string scripts)
        {
            var start = scripts.IndexOf('(');
            var end = start >= 0 ? scripts.IndexOf(')', start + 1) : -1;

            return start >= 0 && end > start ? scripts.Substring(start, end - start + 1) : "()";
        }
        #endregion
    }
}