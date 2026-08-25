using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Builders;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.Core.Text;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers
{
    internal sealed class QueryEditorAutoCompletePeriodResolver
    {
        private enum PeriodResolveSourceKind
        {
            None = 0,
            Schema,
            WithAs,
            Subquery,
            ObjectAlias
        }

        private enum PeriodResolveIntent
        {
            None = 0,
            BuildFromSchema,
            BuildFromSql
        }

        private sealed class PeriodResolveState
        {
            public string AliasName { get; set; }

            public int TriggerPosition { get; set; }

            public PeriodResolveSourceKind SourceKind { get; set; }

            public PeriodResolveIntent ResolveIntent { get; set; }

            public string AutoCompleteSql { get; set; }

            public bool HasAlias
            {
                get { return !string.IsNullOrWhiteSpace(AliasName); }
            }
        }

        private static PeriodResolveIntent ResolveIntent(PeriodResolveSourceKind sourceKind)
        {
            switch (sourceKind)
            {
                case PeriodResolveSourceKind.Schema:
                    {
                        return PeriodResolveIntent.BuildFromSchema;
                    }
                case PeriodResolveSourceKind.WithAs:
                case PeriodResolveSourceKind.Subquery:
                case PeriodResolveSourceKind.ObjectAlias:
                    {
                        return PeriodResolveIntent.BuildFromSql;
                    }
                default:
                    {
                        return PeriodResolveIntent.None;
                    }
            }
        }

        private bool ResolveSourceKind(QueryEditorAutoCompletePeriodResolveContext context, string aliasName, string sqlBlockUpper,
                                       string sqlBlock, out PeriodResolveSourceKind sourceKind, out string autoCompleteSql)
        {
            sourceKind = PeriodResolveSourceKind.None;
            autoCompleteSql = string.Empty;

            if (string.IsNullOrWhiteSpace(aliasName) || string.IsNullOrWhiteSpace(sqlBlock))
            {
                return false;
            }

            if (TryResolveWithAsSql(aliasName, sqlBlockUpper, sqlBlock, out autoCompleteSql))
            {
                sourceKind = PeriodResolveSourceKind.WithAs;
                return true;
            }

            if (TryResolveSubquerySql(aliasName, sqlBlockUpper, sqlBlock, out autoCompleteSql))
            {
                sourceKind = PeriodResolveSourceKind.Subquery;
                return true;
            }

            if (TryResolveObjectSql(context, aliasName, sqlBlock, out autoCompleteSql))
            {
                sourceKind = PeriodResolveSourceKind.ObjectAlias;
                return true;
            }

            sourceKind = PeriodResolveSourceKind.Schema;
            autoCompleteSql = string.Empty;
            return true;
        }

        private readonly QueryEditorAutoCompleteSchemaQueryExecutor _schemaQueryExecutor;

        public QueryEditorAutoCompletePeriodResolver(QueryEditorAutoCompleteSchemaQueryExecutor schemaQueryExecutor)
        {
            _schemaQueryExecutor = schemaQueryExecutor ?? throw new ArgumentNullException(nameof(schemaQueryExecutor));
        }

        public bool TryResolve(QueryEditorAutoCompletePeriodResolveContext context, int positionNew, out QueryEditorAutoCompleteRequest request)
        {
            return TryResolve(context, positionNew, out request, out _);
        }

        public bool TryResolve(QueryEditorAutoCompletePeriodResolveContext context, int positionNew, out QueryEditorAutoCompleteRequest request, out bool queryExecutionFailed)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();
            request = null;
            queryExecutionFailed = false;

            if (!TryAnalyzeContext(context, positionNew, out var resolveState))
            {
                return false;
            }

            if (!TryBuildAutoCompleteTable(context, resolveState, out var dtPeriod, out queryExecutionFailed))
            {
                return false;
            }

            request = new QueryEditorAutoCompleteRequest
            {
                TriggerPosition = resolveState.TriggerPosition,
                Data = dtPeriod
            };

            return true;
        }

        private bool TryAnalyzeContext(QueryEditorAutoCompletePeriodResolveContext context, int triggerPositionOverride, out PeriodResolveState resolveState)
        {
            resolveState = null;

            var triggerPosition = triggerPositionOverride == 0 ? context.Editor.CurrentPosition : triggerPositionOverride;

            if (triggerPosition < 2)
            {
                return false;
            }

            if (!TryGetAliasName(context, triggerPositionOverride, out var aliasName))
            {
                return false;
            }

            var sqlBlockRaw = context.SelectCurrentBlock(false);

            if (string.IsNullOrWhiteSpace(sqlBlockRaw))
            {
                return false;
            }

            var sqlBlockUpper = (context.FormatSql(sqlBlockRaw, true, true) ?? string.Empty).Trim();
            var sqlBlockForResolve = (context.FormatSql(sqlBlockRaw, false, false) ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(sqlBlockUpper) || string.IsNullOrWhiteSpace(sqlBlockForResolve))
            {
                return false;
            }

            if (!ResolveSourceKind(context, aliasName, sqlBlockUpper, sqlBlockForResolve, out var sourceKind, out var autoCompleteSql))
            {
                return false;
            }

            var resolveIntent = ResolveIntent(sourceKind);

            if (resolveIntent == PeriodResolveIntent.None)
            {
                return false;
            }

            resolveState = new PeriodResolveState
            {
                AliasName = aliasName,
                TriggerPosition = triggerPosition,
                SourceKind = sourceKind,
                ResolveIntent = resolveIntent,
                AutoCompleteSql = autoCompleteSql ?? string.Empty
            };

            return true;
        }

        private bool TryGetAliasName(QueryEditorAutoCompletePeriodResolveContext context, int positionNew, out string aliasName)
        {
            aliasName = string.Empty;

            var text = context.Editor.Text ?? string.Empty;
            var periodPosition = positionNew == 0 ? context.Editor.CurrentPosition - 2 : positionNew - 2;

            if (periodPosition < 0 || periodPosition >= text.Length)
            {
                return false;
            }

            if (!IsAliasNameChar(text[periodPosition], context.IsDataSourceSqlServer))
            {
                return false;
            }

            var startIndex = periodPosition;

            while (startIndex >= 0 && IsAliasNameChar(text[startIndex], context.IsDataSourceSqlServer))
            {
                startIndex--;
            }

            if (startIndex >= 0 && (text[startIndex] == '\'' || text[startIndex] == '"'))
            {
                return false;
            }

            aliasName = text.Substring(startIndex + 1, periodPosition - startIndex).Trim();

            return !string.IsNullOrWhiteSpace(aliasName);
        }

        private static bool IsAliasNameChar(char ch, bool IsSqlServer)
        {
            return IsSqlServer ? TextHelper.IsEngAlphabetOrNumber(ch, '_', '.') : TextHelper.IsEngAlphabetOrNumber(ch, '_');
        }

        private bool TryBuildAutoCompleteTable(QueryEditorAutoCompletePeriodResolveContext context, PeriodResolveState resolveState,
                                               out DataTable dtPeriod, out bool queryExecutionFailed)
        {
            dtPeriod = null;
            queryExecutionFailed = false;

            if (resolveState == null)
            {
                return false;
            }

            switch (resolveState.ResolveIntent)
            {
                case PeriodResolveIntent.BuildFromSchema:
                    {
                        var dtSchemaData = QueryEditorAutoCompleteSchemaResolver.DetectSchemaForPeriod
                        (
                            context.CurrentSourceType,
                            resolveState.AliasName
                        );

                        if (dtSchemaData == null || dtSchemaData.Rows.Count == 0)
                        {
                            DataTableLifecycleHelper.DisposeDataTable(ref dtSchemaData);
                            return false;
                        }

                        RemoveEmptyBaseTableNameColumn(dtSchemaData);

                        dtPeriod = dtSchemaData;
                        return true;
                    }
                case PeriodResolveIntent.BuildFromSql:
                    {
                        if (string.IsNullOrWhiteSpace(resolveState.AutoCompleteSql))
                        {
                            return false;
                        }

                        var sqlPrompt = SqlTraceHelper.BuildHeaderNewLine("---Get the AutoComplete List (When the . key is pressed)");
                        var sql = string.Format("{0}{1}", sqlPrompt, resolveState.AutoCompleteSql);

                        if (!_schemaQueryExecutor.TryExecute(sql, "When the . key is pressed", out var dtSchemaTable, out queryExecutionFailed))
                        {
                            return false;
                        }

                        if (dtSchemaTable == null || dtSchemaTable.Rows.Count == 0)
                        {
                            DataTableLifecycleHelper.DisposeDataTable(ref dtSchemaTable);
                            return false;
                        }

                        DataTable dtSourceRows = null;

                        try
                        {
                            dtSourceRows = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForPeriod
                            (
                                dtSchemaTable,
                                context.CurrentSourceType
                            );

                            RemoveEmptyBaseTableNameColumn(dtSourceRows);

                            dtPeriod = dtSourceRows;
                            dtSourceRows = null;

                            return dtPeriod != null && dtPeriod.Rows.Count > 0;
                        }
                        finally
                        {
                            DataTableLifecycleHelper.DisposeDataTable(ref dtSourceRows);
                            DataTableLifecycleHelper.DisposeDataTable(ref dtSchemaTable);
                        }
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool TryResolveWithAsSql(string aliasName, string sqlBlockUpper, string sqlBlock, out string sql)
        {
            sql = string.Empty;

            if (string.IsNullOrWhiteSpace(aliasName) || string.IsNullOrWhiteSpace(sqlBlockUpper) || string.IsNullOrWhiteSpace(sqlBlock))
            {
                return false;
            }

            var pattern = string.Format("{0} AS (", aliasName);

            if (!TryFindPatternWithLeftBoundary(sqlBlock, pattern, out var aliasIndex))
            {
                return false;
            }

            sql = QueryEditorAutoCompleteWithAsResolver.GetAutoCompleteSqlForWithAs(sqlBlock, aliasIndex + pattern.Length - 1);
            return !string.IsNullOrWhiteSpace(sql);
        }

        private static bool TryResolveSubquerySql(string aliasName, string sqlBlockUpper, string sqlBlock, out string sql)
        {
            sql = string.Empty;

            if (string.IsNullOrWhiteSpace(aliasName) || string.IsNullOrWhiteSpace(sqlBlockUpper) || string.IsNullOrWhiteSpace(sqlBlock))
            {
                return false;
            }

            var trimmedSql = sqlBlock.TrimEnd();
            var aliasPattern = string.Format(") {0}", aliasName);
            var aliasAsPattern = string.Format(") AS {0}", aliasName);
            var startIndex = -1;

            if (TryFindPatternWithTrailingBoundary(trimmedSql, aliasAsPattern, out startIndex, searchFromEnd: true))
            {
                //matched ") AS alias"
            }
            else if (TryFindPatternWithTrailingBoundary(trimmedSql, aliasPattern, out startIndex, searchFromEnd: true))
            {
                //matched ") alias"
            }
            else
            {
                return false;
            }

            sql = QueryEditorAutoCompleteSubqueryResolver.GetAutoCompleteSqlForSubquery(trimmedSql, startIndex)?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sql))
            {
                return false;
            }

            return sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryResolveObjectSql(QueryEditorAutoCompletePeriodResolveContext context, string aliasName, string sqlBlock, out string sql)
        {
            sql = string.Empty;

            if (context == null || string.IsNullOrWhiteSpace(aliasName) || string.IsNullOrWhiteSpace(sqlBlock))
            {
                return false;
            }

            if (!TryFindAliasDeclarationTokenIndex(sqlBlock, aliasName, out var aliasTokenIndex, searchFromEnd: true))
            {
                return false;
            }

            var objectName = GetIdentifierBeforePosition(sqlBlock, aliasTokenIndex - 1, context.IsDataSourceSqlServer, context.IsDataSourceMySql);

            if (string.Equals(objectName, "AS", StringComparison.OrdinalIgnoreCase))
            {
                var asPattern = string.Format("AS {0}", aliasName);

                if (!TryFindPatternWithTrailingBoundary(sqlBlock, asPattern, out var asAliasTokenIndex, searchFromEnd: true))
                {
                    return false;
                }

                objectName = GetIdentifierBeforePosition(sqlBlock, asAliasTokenIndex - 1, context.IsDataSourceSqlServer, context.IsDataSourceMySql);
            }

            if (string.IsNullOrWhiteSpace(objectName))
            {
                return false;
            }

            if (string.Equals(objectName, "FROM", StringComparison.OrdinalIgnoreCase))
            {
                if (TryFindQualifiedAccessIndex(sqlBlock, aliasName, out _))
                {
                    sql = string.Format("SELECT * FROM {0} WHERE 1 = 2", aliasName);
                    return true;
                }

                return false;
            }

            sql = string.Format("SELECT * FROM {0} WHERE 1 = 2", objectName);
            return true;
        }

        private static string GetIdentifierBeforePosition(string text, int endIndex, bool IsSqlServer, bool IsMySql)
        {
            if (string.IsNullOrWhiteSpace(text) || endIndex < 0)
            {
                return string.Empty;
            }

            while (endIndex >= 0 && char.IsWhiteSpace(text[endIndex]))
            {
                endIndex--;
            }

            if (endIndex < 0)
            {
                return string.Empty;
            }

            for (var i = endIndex; i >= 0; i--)
            {
                var currentChar = text[i];
                var sqlServerChar = TextHelper.IsEngAlphabetOrNumber(currentChar, '[', ']');
                var mySqlChar = TextHelper.IsEngAlphabetOrNumber(currentChar, '`');

                if (TextHelper.IsEngAlphabetOrNumber(currentChar, '.', '_', '"') || (IsSqlServer && sqlServerChar) || (IsMySql && mySqlChar))
                {
                    continue;
                }

                if (endIndex - i < 1)
                {
                    return string.Empty;
                }

                return text.Substring(i + 1, endIndex - i).Trim();
            }

            return text.Substring(0, endIndex + 1).Trim();
        }

        private static bool TryFindAliasDeclarationTokenIndex(string text, string aliasName, out int index, bool searchFromEnd = false)
        {
            index = -1;

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(aliasName))
            {
                return false;
            }

            if (!searchFromEnd)
            {
                for (var i = 0; i <= text.Length - aliasName.Length; i++)
                {
                    if (string.Compare(text, i, aliasName, 0, aliasName.Length, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        continue;
                    }

                    if (IsIdentifierBoundary(text, i - 1) && IsAliasTrailingBoundary(text, i + aliasName.Length))
                    {
                        index = i;
                        return true;
                    }
                }

                return false;
            }

            for (var i = text.Length - aliasName.Length; i >= 0; i--)
            {
                if (string.Compare(text, i, aliasName, 0, aliasName.Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }

                if (IsIdentifierBoundary(text, i - 1) && IsAliasTrailingBoundary(text, i + aliasName.Length))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindPatternWithLeftBoundary(string text, string pattern, out int index)
        {
            index = -1;

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(pattern))
            {
                return false;
            }

            for (var i = 0; i <= text.Length - pattern.Length; i++)
            {
                if (string.Compare(text, i, pattern, 0, pattern.Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }

                if (IsIdentifierBoundary(text, i - 1))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindPatternWithTrailingBoundary(string text, string pattern, out int index, bool searchFromEnd = false)
        {
            index = -1;

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(pattern))
            {
                return false;
            }

            if (!searchFromEnd)
            {
                for (var i = 0; i <= text.Length - pattern.Length; i++)
                {
                    if (string.Compare(text, i, pattern, 0, pattern.Length, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        continue;
                    }

                    if (IsAliasTrailingBoundary(text, i + pattern.Length))
                    {
                        index = i;
                        return true;
                    }
                }

                return false;
            }

            for (var i = text.Length - pattern.Length; i >= 0; i--)
            {
                if (string.Compare(text, i, pattern, 0, pattern.Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }

                if (IsAliasTrailingBoundary(text, i + pattern.Length))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindQualifiedAccessIndex(string text, string aliasName, out int index)
        {
            index = -1;

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(aliasName))
            {
                return false;
            }

            var pattern = string.Format("{0}.", aliasName);

            for (var i = 0; i <= text.Length - pattern.Length; i++)
            {
                if (string.Compare(text, i, pattern, 0, pattern.Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }

                if (IsIdentifierBoundary(text, i - 1))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        private static bool IsIdentifierBoundary(string text, int index)
        {
            if (index < 0 || index >= text.Length)
            {
                return true;
            }

            return !char.IsLetterOrDigit(text[index]) && text[index] != '_';
        }

        private static bool IsAliasTrailingBoundary(string text, int index)
        {
            if (index < 0 || index >= text.Length)
            {
                return true;
            }

            var ch = text[index];

            return char.IsWhiteSpace(ch) || ch == ',' || ch == ')' || ch == ';';
        }

        private void RemoveEmptyBaseTableNameColumn(DataTable dt)
        {
            if (dt == null || !dt.Columns.Contains("BaseTableName"))
            {
                return;
            }

            var hasBaseTableName = dt.AsEnumerable().Any(dr => !string.IsNullOrWhiteSpace(dr.GetSafeString("BaseTableName")));

            if (!hasBaseTableName)
            {
                DataTableColumnHelper.SafeRemoveColumn(dt, "BaseTableName");
            }
        }
    }
}
