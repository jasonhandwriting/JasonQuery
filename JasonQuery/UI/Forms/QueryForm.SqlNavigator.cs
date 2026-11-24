using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Text;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.LeftPanel;
using System;
using System.Data;
using System.Text;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private const string LeftPanelTabAutoReplace = "tabAutoReplace";
        private const string LeftPanelTabSchemaInformation = "tabSchemaInformation";
        private const string LeftPanelTabTabList = "tabTabList";
        private const string LeftPanelTabSqlNavigator = "tabSqlNavigator";
        private string _sqlNavigatorSourceEditorText;

        internal event Action<QueryForm> SqlNavigatorRefreshed;

        private void RefreshSqlNavigatorPanel()
        {
            var selectedTabName = GetSelectedLeftPanelTabName();

            c1GridSqlNavigator.Filter -= C1GridSqlNavigator_Filter;

            var editorText = editor.Text;

            _dtSqlNavigatorTable = BuildSqlNavigatorTable(editorText);
            _sqlNavigatorSourceEditorText = editorText;

            ApplySqlNavigatorTableToGrid();
            RestoreLeftPanelSelectedTab(selectedTabName);
            ApplySqlNavigatorGridRowStyle();

            SqlNavigatorRefreshed?.Invoke(this);
        }

        internal SharedSqlNavigatorSnapshot CaptureSharedSqlNavigatorSnapshot()
        {
            return SharedSqlNavigatorSnapshotPolicy.Create
            (
                AccessibleDescription,
                _dtSqlNavigatorTable,
                _sqlNavigatorSourceEditorText
            );
        }

        internal bool IsSharedSqlNavigatorSnapshotCurrent(SharedSqlNavigatorSnapshot snapshot, LeftPanelContext context, string activeIdentity)
        {
            return snapshot != null
                   && !IsDisposed
                   && !Disposing
                   && string.Equals(AccessibleDescription, activeIdentity, StringComparison.Ordinal)
                   && snapshot.IsCurrent(context, activeIdentity, _dtSqlNavigatorTable, editor.Text);
        }

        internal bool TryNavigateSharedSqlNavigatorPosition(SharedSqlNavigatorSnapshot snapshot, LeftPanelContext context, string activeIdentity, int oneBasedPosition)
        {
            if (snapshot == null || IsDisposed || Disposing || InvokeRequired || !_isFormLoadFinished || _isBusy
                || editor == null || editor.IsDisposed || !editor.CanFocus
                || !string.Equals(AccessibleDescription, activeIdentity, StringComparison.Ordinal))
            {
                return false;
            }

            //Revalidate against this QueryForm's live publication and text immediately before moving the caret.
            if (!snapshot.TryResolvePosition(context, activeIdentity, _dtSqlNavigatorTable, editor.Text, oneBasedPosition, out var zeroBasedPosition)
                || zeroBasedPosition >= editor.TextLength)
            {
                return false;
            }

            //Collapse any previous selection, then reuse the existing statement-aware selection path.
            editor.SetSelection(zeroBasedPosition, zeroBasedPosition);
            SelectCurrentBlock();
            editor.Focus();
            editor.ScrollCaret();

            return true;
        }

        private string GetSelectedLeftPanelTabName()
        {
            return c1DockingTab2.SelectedTab?.Name ?? string.Empty;
        }

        private DataTable BuildSqlNavigatorTable(string editorText)
        {
            var table = CreateSqlNavigatorTable();
            var lines = editorText.Split(new[] { "\r\n" }, StringSplitOptions.None);

            var sqlBuilder = new StringBuilder();
            var isSqlBlockStarted = false;
            var sqlBlockStartPosition = -1;
            var position = 1;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    AddSqlNavigatorRowIfNeeded(table, sqlBuilder, sqlBlockStartPosition);

                    sqlBuilder.Clear();
                    isSqlBlockStarted = false;
                    sqlBlockStartPosition = -1;
                    position += line.Length + 2;

                    continue;
                }

                if (!isSqlBlockStarted)
                {
                    isSqlBlockStarted = true;
                    sqlBlockStartPosition = position;
                }

                sqlBuilder.Append(line).Append("\r\n");
                position += line.Length + 2;

                if (i == lines.Length - 1)
                {
                    AddSqlNavigatorRowIfNeeded(table, sqlBuilder, sqlBlockStartPosition);
                }
            }

            return table;
        }

        private static DataTable CreateSqlNavigatorTable()
        {
            var table = new DataTable();

            table.Columns.Add("Type");
            table.Columns.Add("Sql");
            table.Columns.Add("Position", typeof(int));

            return table;
        }

        private void AddSqlNavigatorRowIfNeeded(DataTable table, StringBuilder sqlBuilder, int position)
        {
            var sql = sqlBuilder.ToString().TrimEnd('\r', '\n');

            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            var row = table.NewRow();

            row["Position"] = position;
            row["Type"] = ResolveSqlNavigatorStatementType(sql);
            row["Sql"] = sql;

            table.Rows.Add(row);
        }

        private string ResolveSqlNavigatorStatementType(string sql)
        {
            var sqlStatementType = "Unknown";

            try
            {
                var script = new Devart.Data.Oracle.OracleScript(sql.Trim());

                if (script == null || script.Statements.Count == 0)
                {
                    return sqlStatementType;
                }

                var statement = script.Statements[0];
                var statementType = statement.StatementType.ToString();
                var firstToken = GetFirstToken(statement.Text).ToUpperInvariant();

                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            MyGlobal.OracleReader.GetResultString(statementType, firstToken, 0, out sqlStatementType, sql);
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            MyGlobal.PostgreSqlReader.GetResultString(statementType, firstToken, 0, out sqlStatementType, sql);
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            MyGlobal.SqlServerReader.GetResultString(statementType, firstToken, 0, out sqlStatementType, sql);
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            MyGlobal.MySqlReader.GetResultString(statementType, firstToken, 0, out sqlStatementType, sql);
                            break;
                        }
                }

                return sqlStatementType;
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string GetFirstToken(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var parts = text.Trim().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return parts.Length == 0 ? string.Empty : parts[0];
        }

        private void ApplySqlNavigatorTableToGrid()
        {
            tabSqlNavigator.TabVisible = false;

            if (_dtSqlNavigatorTable.Rows.Count == 0)
            {
                c1GridSqlNavigator.Filter -= C1GridSqlNavigator_Filter;
                tabSqlNavigator.TabVisible = false;
                return;
            }

            c1GridSqlNavigator.DataSource = _dtSqlNavigatorTable;

            c1GridSqlNavigator.Columns[0].Caption = LocalizationHelper.GetLanguageString("Type", "form", GetType().Name, "gridheader", "Type", "Text");
            c1GridSqlNavigator.Columns[1].Caption = LocalizationHelper.GetLanguageString("SQL Statement", "form", GetType().Name, "gridheader", "SQLStatement", "Text");

            c1GridSqlNavigator.Columns[0].Tag = "Type";
            c1GridSqlNavigator.Columns[1].Tag = "Sql";
            c1GridSqlNavigator.Columns[2].Tag = "Position";

            AdjustSqlNavigator();

            c1GridSqlNavigator.AllowFilter = false;
            c1GridSqlNavigator.Filter += C1GridSqlNavigator_Filter;

            tabSqlNavigator.TabVisible = true;
            GridHelper.SetGridHeaderLine(c1GridSqlNavigator);
        }

        private void RestoreLeftPanelSelectedTab(string selectedTabName)
        {
            switch (selectedTabName)
            {
                case LeftPanelTabAutoReplace:
                    {
                        c1DockingTab2.SelectedTab = tabAutoReplace;
                        break;
                    }
                case LeftPanelTabSchemaInformation:
                    {
                        c1DockingTab2.SelectedTab = tabSchemaInformation;
                        break;
                    }
                case LeftPanelTabTabList:
                    {
                        c1DockingTab2.SelectedTab = tabTabList;
                        break;
                    }
                case LeftPanelTabSqlNavigator:
                    {
                        c1DockingTab2.SelectedTab = tabSqlNavigator.TabVisible ? tabSqlNavigator : tabSchemaInformation;
                        break;
                    }
            }
        }

        private void ApplySqlNavigatorGridRowStyle()
        {
            //Temporary legacy protection until QueryForm's F9 grid is retired.
            //Never let a long single-line SQL trigger content-driven row sizing before publication.
            var grid = c1GridSqlNavigator;
            var singleRowHeight = Math.Max(_sqlNavigatorRowHeight, grid.Font.Height + 3);
            var previewHeight = singleRowHeight * 2 - 3;

            grid.AllowRowSizing = RowSizingEnum.None;

            if (grid.RowHeight != previewHeight)
            {
                grid.RowHeight = previewHeight;
            }
        }
    }
}
