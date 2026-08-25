using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.CreateScript.Index;
using System;
using System.Data;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.SqlServer
{
    internal static class SqlServerIndexCreateScriptAssembler
    {
        public static string Build(SqlServerCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            var script = new StringBuilder();

            if (request.SchemaNameWithoutSchemaDbo.IndexOf(" on ", StringComparison.Ordinal) < 0)
            {
                return string.Empty;
            }

            var parts = request.SchemaNameWithoutSchemaDbo.Split(new[] { " on " }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                return string.Empty;
            }

            var indexName = parts[0];
            var tableName = parts[1];

            if (string.IsNullOrWhiteSpace(request.ObjectID))
            {
                var sqlObjectId = SqlServerIndexCreateScriptSqlBuilder.BuildIndexObjectIdSql(request, tableName, indexName);
                var dtObjectInfo = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sqlObjectId, executeQuery);

                request.ObjectID = dtObjectInfo?.Rows.Count > 0 ? dtObjectInfo.Rows[0].GetSafeString("ObjectID") : string.Empty;
            }

            var sqlIndexInfo = SqlServerIndexCreateScriptSqlBuilder.BuildSingleIndexSummarySql(request, tableName, indexName);
            var dtIndexInfo = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sqlIndexInfo, executeQuery);

            if (dtIndexInfo?.Rows.Count <= 0)
            {
                return string.Empty;
            }

            var sqlIndexColumns = SqlServerIndexCreateScriptSqlBuilder.BuildIndexColumnSql(request, request.ObjectID, indexName);
            var dtIndexColumns = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sqlIndexColumns, executeQuery);

            if (dtIndexColumns?.Rows.Count <= 0)
            {
                return string.Empty;
            }

            var drSummary = dtIndexInfo.Rows[0];
            var createDate = drSummary.GetSafeDateTimeText("Create_Date", request.DateTimeFormat);
            var modifyDate = drSummary.GetSafeDateTimeText("Modify_Date", request.DateTimeFormat);
            var typeDesc = drSummary.GetSafeString("Type_Desc");
            var groupName = drSummary.GetSafeString("GroupName");

            script.AppendLine($"-- Index: {indexName}");
            script.AppendLine($"-- Created: {createDate}");
            script.AppendLine($"-- Modified: {modifyDate}");
            script.AppendLine();

            if (string.Equals(typeDesc, "NONCLUSTERED", StringComparison.OrdinalIgnoreCase))
            {
                script.AppendLine($"CREATE NONCLUSTERED INDEX {indexName} ON {request.SchemaDbo}.{tableName}");
            }
            else
            {
                script.AppendLine($"ALTER TABLE {request.SchemaDbo}.{tableName}");
                script.AppendLine($"ADD CONSTRAINT {indexName}");
                script.AppendLine("PRIMARY KEY CLUSTERED");
            }

            script.AppendLine("(");

            for (var i = 0; i < dtIndexColumns.Rows.Count; i++)
            {
                var drIndexColumn = dtIndexColumns.Rows[i];
                var indexColumnName = drIndexColumn.GetSafeString("Index_Column_Name");
                var indexColumnID = drIndexColumn.GetSafeString("Index_Column_ID");

                if (i > 0)
                {
                    script.AppendLine(",");
                }

                script.Append($"    [{indexColumnName}]");

                var sqlSort = SqlServerIndexCreateScriptSqlBuilder.BuildIndexSortDirectionSql(request, indexName, indexColumnID);
                var dtSort = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sqlSort, executeQuery);

                if (dtSort?.Rows.Count > 0)
                {
                    var descendingKey = dtSort.Rows[0].GetSafeString("Is_Descending_Key");
                    var descendingKeyValue = string.Equals(descendingKey, "TRUE", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

                    script.Append(descendingKeyValue);
                }
            }

            script.AppendLine();
            script.AppendLine(")");
            script.AppendLine("WITH");
            script.AppendLine("(");

            var padded = string.Equals(drSummary.GetSafeString("Is_Padded"), "FALSE", StringComparison.OrdinalIgnoreCase) ? "OFF" : "ON";
            var noRecompute = string.Equals(drSummary.GetSafeString("No_Recompute"), "FALSE", StringComparison.OrdinalIgnoreCase) ? "OFF" : "ON";
            var allowPageLocks = string.Equals(drSummary.GetSafeString("Allow_Page_Locks"), "FALSE", StringComparison.OrdinalIgnoreCase) ? "OFF" : "ON";
            var allowRowLocks = string.Equals(drSummary.GetSafeString("Allow_Row_Locks"), "FALSE", StringComparison.OrdinalIgnoreCase) ? "OFF" : "ON";
            var ignoreDupKey = string.Equals(drSummary.GetSafeString("Ignore_Dup_Key"), "FALSE", StringComparison.OrdinalIgnoreCase) ? "OFF" : "ON";

            if (string.Equals(typeDesc, "NONCLUSTERED", StringComparison.OrdinalIgnoreCase))
            {
                script.AppendLine($"    PAD_INDEX = {padded},");
                script.AppendLine("    DROP_EXISTING = OFF,");
                script.AppendLine($"    STATISTICS_NORECOMPUTE = {noRecompute},");
                script.AppendLine("    SORT_IN_TEMPDB = OFF,");
                script.AppendLine("    ONLINE = OFF,");
                script.AppendLine($"    ALLOW_ROW_LOCKS = {allowRowLocks},");
                script.AppendLine($"    ALLOW_PAGE_LOCKS = {allowPageLocks}");
            }
            else
            {
                script.AppendLine($"    PAD_INDEX = {padded},");
                script.AppendLine($"    IGNORE_DUP_KEY = {ignoreDupKey},");
                script.AppendLine($"    STATISTICS_NORECOMPUTE = {noRecompute},");
                script.AppendLine($"    ALLOW_ROW_LOCKS = {allowRowLocks},");
                script.AppendLine($"    ALLOW_PAGE_LOCKS = {allowPageLocks}");
            }

            script.AppendLine(")");
            script.AppendLine($"ON [{groupName}]");
            script.Append("GO");

            return script.ToString().TrimEnd('\r', '\n');
        }
    }
}
