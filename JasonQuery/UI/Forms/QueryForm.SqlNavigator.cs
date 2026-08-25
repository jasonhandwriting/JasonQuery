using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Text;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
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

        private void RefreshSqlNavigatorPanel()
        {
            var selectedTabName = GetSelectedLeftPanelTabName();

            c1GridSqlNavigator.Filter -= C1GridSqlNavigator_Filter;

            _dtSqlNavigatorTable = BuildSqlNavigatorTable();

            ApplySqlNavigatorTableToGrid();
            RestoreLeftPanelSelectedTab(selectedTabName);
            ApplySqlNavigatorGridRowStyle();
        }

        private string GetSelectedLeftPanelTabName()
        {
            return c1DockingTab2.SelectedTab?.Name ?? string.Empty;
        }

        private DataTable BuildSqlNavigatorTable()
        {
            var table = CreateSqlNavigatorTable();
            var lines = editor.Text.Split(new[] { "\r\n" }, StringSplitOptions.None);

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
                    position += 2;

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
            c1GridSqlNavigator.AllowRowSizing = RowSizingEnum.IndividualRows;
            c1GridSqlNavigator.RowHeight = _sqlNavigatorRowHeight * 2 - 3;

            var rowCount = c1GridSqlNavigator.Splits[0].Rows.Count;

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var cellValue = TextHelper.GetSafeString(c1GridSqlNavigator.Columns[1].CellValue(rowIndex));

                if (!cellValue.Contains("\r\n"))
                {
                    c1GridSqlNavigator.Splits[0].Rows[rowIndex].AutoSize();
                }
            }
        }
    }
}
