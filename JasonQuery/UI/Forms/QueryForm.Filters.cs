using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        //FilterChange captures each column's actual input while the C1 event is active.
        //Only applying RowFilter is deferred; never reread FilterText in BeginInvoke.
        private readonly Dictionary<C1TrueDBGrid, PendingQueryResultFilter> _pendingQueryResultMultiFilters = new Dictionary<C1TrueDBGrid, PendingQueryResultFilter>();

        private sealed class PendingQueryResultFilter
        {
            internal PendingQueryResultFilter(DataTable sourceTable, List<QueryResultFilterInput> inputs)
            {
                SourceTable = sourceTable;
                Inputs = inputs;
            }

            internal DataTable SourceTable { get; }
            internal List<QueryResultFilterInput> Inputs { get; }
        }

        private static readonly ConditionalWeakTable<DataTable, NumericFilterGuardCache> _queryResultNumericFilterCaches = new ConditionalWeakTable<DataTable, NumericFilterGuardCache>();

        internal sealed class QueryResultFilterInput
        {
            internal QueryResultFilterInput(string dataField, string filterText, bool isNumeric)
            {
                DataField = dataField;
                FilterText = filterText;
                IsNumeric = isNumeric;
            }

            internal string DataField { get; }
            internal string FilterText { get; }
            internal bool IsNumeric { get; }
        }

        private sealed class NumericFilterGuardCache
        {
            private const int MaxInvalidValues = 128;
            private const int MaxGuardLength = 12000;

            private readonly DataTable _sourceTable;
            private readonly Dictionary<string, string> _guards = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, HashSet<string>> _guardInvalidValues = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            private string _candidateExpression;
            private Dictionary<string, string> _candidateGuards;
            private string _activeExpression;
            private Dictionary<string, string> _activeGuards;
            private Dictionary<string, HashSet<string>> _activeInvalidValues;

            internal NumericFilterGuardCache(DataTable sourceTable)
            {
                _sourceTable = sourceTable;

                //A proposed cell value must be guarded BEFORE DataView reevaluates the existing RowFilter. RowChanged/ColumnChanged occur too late.
                sourceTable.ColumnChanging += OnColumnChanging;
                sourceTable.RowChanged += (sender, args) => ClearCachedGuards();
                sourceTable.RowDeleted += (sender, args) => ClearCachedGuards();
                sourceTable.ColumnChanged += (sender, args) => ClearCachedGuards();
                sourceTable.TableCleared += (sender, args) => ClearCachedGuards();
            }

            private void ClearCachedGuards()
            {
                _guards.Clear();
                _guardInvalidValues.Clear();

                //Keep the guards belonging to the active RowFilter. It may still be evaluated before a later filter event regenerates the expression.
            }

            internal bool TryGetGuard(DataTable sourceTable, string field, out string guard)
            {
                if (_guards.TryGetValue(field, out guard))
                {
                    return guard != null;
                }

                var invalidValues = new HashSet<string>(StringComparer.Ordinal);

                foreach (DataRow row in sourceTable.Rows)
                {
                    if (row.RowState == DataRowState.Deleted || row.IsNull(field))
                    {
                        continue;
                    }

                    var value = Convert.ToString(row[field], CultureInfo.InvariantCulture) ?? string.Empty;

                    if (TryParseCanonicalFilterDecimal(value, out _))
                    {
                        continue;
                    }

                    invalidValues.Add(value);

                    if (invalidValues.Count > MaxInvalidValues)
                    {
                        guard = null;
                        _guards[field] = null;
                        _guardInvalidValues.Remove(field);

                        return false;
                    }
                }

                if (!TryCreateGuard(field, invalidValues, out guard))
                {
                    _guards[field] = null;
                    _guardInvalidValues.Remove(field);

                    return false;
                }

                _guards[field] = guard;
                _guardInvalidValues[field] = invalidValues;

                return true;
            }

            private static bool TryCreateGuard(string field, HashSet<string> invalidValues, out string guard)
            {
                var columnReference = QuoteFilterField(field);
                var result = new StringBuilder(columnReference).Append(" IS NULL");

                if (invalidValues.Count > MaxInvalidValues)
                {
                    guard = null;
                    return false;
                }

                if (invalidValues.Count > 0)
                {
                    result.Append(" OR ").Append(columnReference).Append(" IN (");

                    var first = true;

                    foreach (var value in invalidValues)
                    {
                        if (!first)
                        {
                            result.Append(", ");
                        }

                        first = false;
                        result.Append("'").Append(EscapeFilterLiteral(value)).Append("'");

                        if (result.Length > MaxGuardLength)
                        {
                            guard = null;
                            return false;
                        }
                    }

                    result.Append(")");
                }

                guard = result.ToString();
                return guard.Length <= MaxGuardLength;
            }

            internal void RecordCandidate(string expression, Dictionary<string, string> numericGuards)
            {
                _candidateExpression = expression;
                _candidateGuards = new Dictionary<string, string>(numericGuards, StringComparer.OrdinalIgnoreCase);
            }

            internal void Activate(string expression)
            {
                if (!string.Equals(_candidateExpression, expression, StringComparison.Ordinal))
                {
                    return;
                }

                _activeExpression = expression;
                _activeGuards = new Dictionary<string, string>(_candidateGuards, StringComparer.OrdinalIgnoreCase);
                _activeInvalidValues = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

                foreach (var entry in _activeGuards)
                {
                    if (_guardInvalidValues.TryGetValue(entry.Key, out var invalidValues))
                    {
                        _activeInvalidValues[entry.Key] = new HashSet<string>(invalidValues, StringComparer.Ordinal);
                    }
                }
            }

            private void OnColumnChanging(object sender, DataColumnChangeEventArgs args)
            {
                if (_activeGuards == null || args?.Column == null || !_activeGuards.TryGetValue(args.Column.ColumnName, out var oldGuard))
                {
                    return;
                }

                var view = _sourceTable.DefaultView;

                if (!string.Equals(view.RowFilter, _activeExpression, StringComparison.Ordinal) || args.ProposedValue == null || args.ProposedValue == DBNull.Value)
                {
                    return;
                }

                var proposed = Convert.ToString(args.ProposedValue, CultureInfo.InvariantCulture) ?? string.Empty;

                if (TryParseCanonicalFilterDecimal(proposed, out _))
                {
                    return;
                }

                if (!_activeInvalidValues.TryGetValue(args.Column.ColumnName, out var currentInvalidValues))
                {
                    DeactivateUnsafeFilter(view, "Numeric guard state was missing during a column change.");
                    return;
                }

                if (currentInvalidValues.Contains(proposed))
                {
                    return;
                }

                var expandedInvalidValues = new HashSet<string>(currentInvalidValues, StringComparer.Ordinal) { proposed };

                if (!TryCreateGuard(args.Column.ColumnName, expandedInvalidValues, out var newGuard))
                {
                    DeactivateUnsafeFilter(view, "Numeric guard exceeded its safety limits during a column change.");
                    return;
                }

                //Replace the exact previously generated guard, not arbitrary column names or display captions. Rebuild before the edit commits.
                var oldClause = "IIF((" + oldGuard + "), False,";
                var newClause = "IIF((" + newGuard + "), False,";

                if (!_activeExpression.Contains(oldClause))
                {
                    DeactivateUnsafeFilter(view, "Active numeric guard did not match its expression.");
                    return;
                }

                var expandedExpression = _activeExpression.Replace(oldClause, newClause);

                try
                {
                    view.RowFilter = expandedExpression;
                    _activeExpression = expandedExpression;
                    _activeGuards[args.Column.ColumnName] = newGuard;
                    _activeInvalidValues[args.Column.ColumnName] = expandedInvalidValues;
                }
                catch (Exception ex) when (ex is EvaluateException || ex is SyntaxErrorException || ex is InvalidCastException || ex is FormatException || ex is OverflowException || ex is ArgumentException)
                {
                    DeactivateUnsafeFilter(view, "Could not extend numeric guard: " + ex.Message);
                }
            }

            private void DeactivateUnsafeFilter(DataView view, string reason)
            {
                //An unguarded Decimal conversion must never remain active when the next row value could be nonnumeric. Fall back safely.
                _activeExpression = null;
                _activeGuards = null;
                _activeInvalidValues = null;

                try
                {
                    view.RowFilter = string.Empty;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Query Result Filter fail-safe could not clear RowFilter: " + ex.Message);
                }

                Trace.WriteLine("Query Result Filter fail-safe cleared RowFilter: " + reason);
            }
        }

        //ComponentOne recommends FilterChange for custom filtering (AllowFilter=false).
        //Filter's e.Condition is not a dependable source of user-entered text.
        private void C1TrueDBGrid_FilterChange(object sender, EventArgs e)
        {
            var grid = sender as C1TrueDBGrid;

            if (grid == null || grid.IsDisposed || !grid.IsHandleCreated || !(grid.DataSource is DataTable sourceTable))
            {
                return;
            }

            //Capture synchronously: the filter editor's text can differ from C1DataColumn.FilterText by the time BeginInvoke executes.
            var inputs = CaptureQueryResultMultiFilterInputs(grid, sourceTable);
            var pending = new PendingQueryResultFilter(sourceTable, inputs);

            if (_pendingQueryResultMultiFilters.ContainsKey(grid))
            {
                //During fast typing the last observed snapshot must win.
                _pendingQueryResultMultiFilters[grid] = pending;
                return;
            }

            _pendingQueryResultMultiFilters.Add(grid, pending);

            try
            {
                grid.BeginInvoke((MethodInvoker)(() =>
                {
                    if (!_pendingQueryResultMultiFilters.TryGetValue(grid, out var current))
                    {
                        return;
                    }

                    _pendingQueryResultMultiFilters.Remove(grid);

                    if (grid.IsDisposed || grid.Disposing || !ReferenceEquals(grid.DataSource, current.SourceTable))
                    {
                        return;
                    }

                    try
                    {
                        if (TryApplyQueryResultFilter(current.SourceTable, current.Inputs, out var changed) && changed)
                        {
                            ClearGridHighlightsBeforeFilter();
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowExceptionMessage(ex);
                    }
                }));
            }
            catch (InvalidOperationException)
            {
                _pendingQueryResultMultiFilters.Remove(grid);
            }
        }

        private List<QueryResultFilterInput> CaptureQueryResultMultiFilterInputs(C1TrueDBGrid grid, DataTable sourceTable)
        {
            var inputs = new List<QueryResultFilterInput>();

            for (var i = 0; i < grid.Columns.Count; i++)
            {
                var column = grid.Columns[i];
                var field = column.DataField;

                if (string.IsNullOrWhiteSpace(field) || !sourceTable.Columns.Contains(field))
                {
                    continue;
                }

                var isNumeric = _columnInfoCollector != null && _columnInfoCollector.TryGet(field, out var info) && info != null && info.CategoryDataTypeKind == CategoryDataTypeKind.Number;

                inputs.Add(new QueryResultFilterInput(field, column.FilterText, isNumeric));
            }

            return inputs;
        }

        //Apply and activate the numeric safety guards as one operation.
        //Tests exercise this same path without creating a C1TrueDBGrid.
        internal static bool TryApplyQueryResultFilter(DataTable sourceTable, IEnumerable<QueryResultFilterInput> inputs, out bool changed)
        {
            changed = false;

            if (!TryBuildQueryResultFilter(sourceTable, inputs, out var expression))
            {
                return false;
            }

            var view = sourceTable.DefaultView;
            var previousFilter = view.RowFilter;
            var cache = _queryResultNumericFilterCaches.GetValue(sourceTable, table => new NumericFilterGuardCache(table));

            try
            {
                if (!string.Equals(previousFilter, expression, StringComparison.Ordinal))
                {
                    view.RowFilter = expression;
                    changed = true;
                }

                cache.Activate(expression);
                return true;
            }
            catch (Exception ex) when (ex is EvaluateException || ex is SyntaxErrorException || ex is InvalidCastException || ex is FormatException || ex is OverflowException || ex is ArgumentException)
            {
                try
                {
                    view.RowFilter = previousFilter;
                }
                catch (Exception rollbackException)
                {
                    Trace.WriteLine("Query Result Filter rollback failed: " + rollbackException.Message);
                }

                changed = false;
                Trace.WriteLine("Query Result Filter condition rejected: " + ex.Message);

                return false;
            }
        }

        //This has no dependency on C1 and can be exercised by Golden tests.
        internal static bool TryBuildQueryResultFilter(DataTable sourceTable, IEnumerable<QueryResultFilterInput> inputs, out string expression)
        {
            expression = null;

            if (sourceTable == null || inputs == null)
            {
                return false;
            }

            var predicates = new List<string>();
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var numericGuards = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var cache = _queryResultNumericFilterCaches.GetValue(sourceTable, table => new NumericFilterGuardCache(table));

            foreach (var input in inputs)
            {
                if (input == null || string.IsNullOrWhiteSpace(input.DataField) || !sourceTable.Columns.Contains(input.DataField) || !processed.Add(input.DataField))
                {
                    continue;
                }

                var value = input.FilterText?.Trim() ?? string.Empty;

                if (value.Length == 0)
                {
                    continue;
                }

                var field = QuoteFilterField(input.DataField);

                if (input.IsNumeric && IsUnsupportedNumericSyntax(value))
                {
                    return false;
                }

                if (input.IsNumeric && TryReadNumericOperator(value, out var op, out var operand))
                {
                    if (!TryParseCanonicalFilterDecimal(operand, out var number) || !cache.TryGetGuard(sourceTable, input.DataField, out var guard))
                    {
                        return false;
                    }

                    numericGuards[input.DataField] = guard;

                    var canonicalNumber = number.ToString(CultureInfo.InvariantCulture);
                    var numericValue = "CONVERT(" + field + ", 'System.Decimal')";
                    var numericConstant = "CONVERT('" + canonicalNumber + "', 'System.Decimal')";

                    predicates.Add("IIF((" + guard + "), False, " + numericValue + " " + op + " " + numericConstant + ")");
                    continue;
                }

                //DataView LIKE accepts text, not JasonQuery's LargeTextDataType/LargeBinaryDataType wrappers. Convert the displayed value to a string without changing the DataTable schema or loading full LOB content.
                var textField = sourceTable.Columns[input.DataField].DataType == typeof(string) ? field : "CONVERT(" + field + ", 'System.String')";

                predicates.Add(textField + " LIKE '*" + EscapeLikeContainsText(value) + "*'");
            }

            expression = string.Join(" AND ", predicates);
            cache.RecordCandidate(expression, numericGuards);

            return true;
        }

        private static bool IsUnsupportedNumericSyntax(string value)
        {
            return value.StartsWith("BETWEEN ", StringComparison.OrdinalIgnoreCase) || value.StartsWith("IN ", StringComparison.OrdinalIgnoreCase) || value.StartsWith("OR ", StringComparison.OrdinalIgnoreCase) || value.StartsWith("LIKE ", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryReadNumericOperator(string value, out string op, out string operand)
        {
            op = null;
            operand = null;

            foreach (var candidate in new[] { ">=", "<=", "!=", "<>", ">", "<", "=" })
            {
                if (!value.StartsWith(candidate, StringComparison.Ordinal))
                {
                    continue;
                }

                op = candidate == "!=" ? "<>" : candidate;
                operand = value.Substring(candidate.Length).Trim();

                return true;
            }

            return false;
        }

        private static bool TryParseCanonicalFilterDecimal(string text, out decimal number)
        {
            number = 0m;

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var pos = text[0] == '-' || text[0] == '+' ? 1 : 0;
            var digits = 0;
            var dots = 0;

            for (var i = pos; i < text.Length; i++)
            {
                if (text[i] >= '0' && text[i] <= '9')
                {
                    digits++;
                    continue;
                }

                if (text[i] == '.' && dots++ == 0)
                {
                    continue;
                }

                return false;
            }

            if (digits == 0)
            {
                return false;
            }

            return decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number);
        }

        private static string QuoteFilterField(string field)
        {
            return "[" + field.Replace("\\", "\\\\").Replace("]", "\\]") + "]";
        }

        private static string EscapeFilterLiteral(string text)
        {
            return (text ?? string.Empty).Replace("'", "''");
        }

        private static string EscapeLikeContainsText(string text)
        {
            var escaped = new StringBuilder();

            foreach (var c in text)
            {
                switch (c)
                {
                    case '*':
                        {
                            escaped.Append("[*]");
                            break;
                        }
                    case '%':
                        {
                            escaped.Append("[%]");
                            break;
                        }
                    case '[':
                        {
                            escaped.Append("[[]");
                            break;
                        }
                    case ']':
                        {
                            escaped.Append("[]]");
                            break;
                        }
                    case '\'':
                        {
                            escaped.Append("''");
                            break;
                        }
                    default:
                        {
                            escaped.Append(c);
                            break;
                        }
                }
            }

            return escaped.ToString();
        }

        private void C1GridSqlNavigator_Filter(object sender, FilterEventArgs e)
        {
            try
            {
                ApplyGridFilter
                (
                    c1GridSqlNavigator,
                    e.Condition,
                    shouldClearGridHighlights: false,
                    useColumnTag: true
                );
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        //SQL Navigator keeps its existing contract; this change is limited to Query Result grids.
        private void ApplyGridFilter(C1TrueDBGrid grid, string condition, bool shouldClearGridHighlights, bool useColumnTag)
        {
            if (grid?.DataSource == null)
            {
                return;
            }

            var dataView = ((DataTable)grid.DataSource).DefaultView;

            if (dataView.RowFilter == condition)
            {
                return;
            }

            if (shouldClearGridHighlights)
            {
                ClearGridHighlightsBeforeFilter();
            }

            dataView.RowFilter = BuildWildcardFilterCondition(grid, condition, useColumnTag);
        }

        private void ClearGridHighlightsBeforeFilter()
        {
            if (TextHelper.GetSafeString(btnHighlightAllGrid.Tag) != "1")
            {
                return;
            }

            //20230824 如果使用者有 Highlight 關鍵字，要先清空，否則會有殘留問題
            btnHighlightAllGrid.Tag = "0";
            btnClearHighlightsGrid.PerformClick();
        }

        private string BuildWildcardFilterCondition(C1TrueDBGrid grid, string condition, bool useColumnTag)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return condition;
            }

            var normalizedCondition = condition;
            var count = grid.Splits[_splitsIndex].DisplayColumns.Count;

            for (var columnIndex = 0; columnIndex < count; columnIndex++)
            {
                var token = useColumnTag ? TextHelper.GetSafeString(grid.Columns[columnIndex].Tag) : grid.Columns[columnIndex].Caption;

                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                var columnToken = $"[{token}]";

                if (!normalizedCondition.Contains(columnToken))
                {
                    continue;
                }

                var tokenIndex = normalizedCondition.IndexOf(columnToken, StringComparison.Ordinal);

                if (tokenIndex < 0)
                {
                    continue;
                }

                var quoteIndex = normalizedCondition.IndexOf('\'', tokenIndex);

                if (quoteIndex < 0)
                {
                    continue;
                }

                normalizedCondition = normalizedCondition.Insert(quoteIndex + 1, "*");
            }

            return normalizedCondition;
        }
    }
}
