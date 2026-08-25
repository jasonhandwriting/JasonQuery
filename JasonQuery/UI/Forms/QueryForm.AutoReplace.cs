using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void InitializeAutoReplaceOnFormLoad()
        {
            if (MyLibrary.EnableAutoReplace)
            {
                using (TraceLogger.Time("Get/Set AutoReplace Info"))
                {
                    CreateAndGetAutoReplaceInfoTable();
                }

                splitContainer3.SplitterWidth = 2;
                return;
            }

            tabAutoReplace.TabVisible = false;
        }

        private void ApplyAutoReplaceTheme()
        {
            if (!MyLibrary.EnableAutoReplace)
            {
                return;
            }

            if (MyLibrary.IsDarkMode)
            {
                c1ThemeController1.SetTheme(c1GridAutoReplaceInfo, "VS2013Dark");
                c1GridAutoReplaceInfo.BackColor = ColorTranslator.FromHtml("#2D2D30");
            }
            else
            {
                c1ThemeController1.SetTheme(c1GridAutoReplaceInfo, "(default)");
                c1GridAutoReplaceInfo.BackColor = ColorTranslator.FromHtml("#F0F0F0");
            }
        }

        private void ApplyAutoReplaceLocalizedCaptions()
        {
            c1GridAutoReplaceInfo.Columns[AutoReplaceColumn.Keyword].Caption =
                LocalizationHelper.GetLanguageString
                (
                    c1GridAutoReplaceInfo.Columns[AutoReplaceColumn.Keyword].Caption,
                    "form",
                    "OptionsForm",
                    "gridheader",
                    "Keyword",
                    "Text"
                );

            c1GridAutoReplaceInfo.Columns[AutoReplaceColumn.Replacement].Caption =
                LocalizationHelper.GetLanguageString
                (
                    c1GridAutoReplaceInfo.Columns[AutoReplaceColumn.Replacement].Caption,
                    "form",
                    "OptionsForm",
                    "gridheader",
                    "Replacement",
                    "Text"
                );
        }

        private void ApplyAutoReplaceLayoutAdjustments()
        {
            lblAutoReplacePosition.Text = lblAutoReplace.Text;
            btnHelp_AutoReplace.Location = new Point(lblAutoReplacePosition.Left + lblAutoReplacePosition.Width, btnHelp_AutoReplace.Top);
        }

        private void CreateAndGetAutoReplaceInfoTable(DataTable dt = null, bool isReserve = false)
        {
            try
            {
                InitializeAutoReplaceHeaderAndTable();

                if (isReserve)
                {
                    FillAutoReplaceRowsFromReservedTable(dt);
                }
                else
                {
                    FillAutoReplaceRowsFromRepository(ref dt);
                    AppendAutoReplaceBlankRows();
                }

                BindAutoReplaceGridAndColumns();

                if (!isReserve)
                {
                    UpdateAutoReplaceDictionary();
                }

                ResizeAutoReplaceRows();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void InitializeAutoReplaceHeaderAndTable()
        {
            _gridHeaderAutoReplacements = new List<string>();
            _gridHeaderAutoReplacements.Add("PID");

            _languageText = LocalizationHelper.GetLanguageString("Keyword", "form", "OptionsForm", "gridheader", "Keyword", "Text");
            _gridHeaderAutoReplacements.Add(_languageText);
            _columnKeywordName = _languageText;

            _languageText = LocalizationHelper.GetLanguageString("Replacement", "form", "OptionsForm", "gridheader", "Replacement", "Text");
            _gridHeaderAutoReplacements.Add(_languageText);
            _columnReplacementName = _languageText;

            _dtAutoReplaceInfoTable = new DataTable();
            _dtAutoReplaceInfoTable.Columns.Add(_gridHeaderAutoReplacements[AutoReplaceColumn.Pid]);
            _dtAutoReplaceInfoTable.Columns.Add(_gridHeaderAutoReplacements[AutoReplaceColumn.Keyword]);
            _dtAutoReplaceInfoTable.Columns.Add(_gridHeaderAutoReplacements[AutoReplaceColumn.Replacement]);
        }

        private void FillAutoReplaceRowsFromReservedTable(DataTable dt)
        {
            if (dt?.Rows.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var dr = dt.Rows[i];

                AddAutoReplaceRow
                (
                    dr.GetSafeString("PID"),
                    dr.GetSafeString(AutoReplaceColumn.Keyword),
                    dr.GetSafeString(AutoReplaceColumn.Replacement)
                );
            }
        }

        private void FillAutoReplaceRowsFromRepository(ref DataTable dt)
        {
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT PID, AttributeValue FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'AutoReplaceConfig'");
            sbSql.Append("   AND AttributeName = 'AutoReplace'");

            var sql = sbSql.ToString();

            dt = JasonQueryRepository.ExecQuery(sql);

            if (dt?.Rows.Count <= 0)
            {
                return;
            }

            for (var row = 0; row < dt.Rows.Count; row++)
            {
                var dr = dt.Rows[row];
                var attributeValue = dr.GetSafeString("AttributeValue");
                var parts = attributeValue.Split(new[] { MyGlobal.Separator3s }, StringSplitOptions.None);

                AddAutoReplaceRow(dr.GetSafeString("PID"), parts.Length > 0 ? parts[0] : string.Empty, parts.Length > 1 ? parts[1] : string.Empty);
            }
        }

        private void AppendAutoReplaceBlankRows()
        {
            for (var i = 0; i < 10; i++)
            {
                AddAutoReplaceRow((i + 101).ToString(), string.Empty, string.Empty);
            }
        }

        private void AddAutoReplaceRow(string pid, string keyword, string replacement)
        {
            _rowAutoReplaceInfo = _dtAutoReplaceInfoTable.NewRow();
            _rowAutoReplaceInfo[AutoReplaceColumn.Pid] = pid;
            _rowAutoReplaceInfo[AutoReplaceColumn.Keyword] = keyword;
            _rowAutoReplaceInfo[AutoReplaceColumn.Replacement] = replacement;
            _dtAutoReplaceInfoTable.Rows.Add(_rowAutoReplaceInfo);
        }

        private void BindAutoReplaceGridAndColumns()
        {
            c1GridAutoReplaceInfo.DataSource = _dtAutoReplaceInfoTable;
            GridHelper.SetGridVisualStyle(c1GridAutoReplaceInfo, 10);

            c1GridAutoReplaceInfo.Splits[0].DisplayColumns[0].Visible = false;
            c1GridAutoReplaceInfo.Splits[0].DisplayColumns[0].Frozen = true;
            c1GridAutoReplaceInfo.Splits[0].DisplayColumns[1].AutoSize();

            if (c1GridAutoReplaceInfo.Splits[0].DisplayColumns[1].Width > 100)
            {
                c1GridAutoReplaceInfo.Splits[0].DisplayColumns[1].Width = 70;
            }

            c1GridAutoReplaceInfo.Splits[0].DisplayColumns[2].Width = c1GridAutoReplaceInfo.Width - c1GridAutoReplaceInfo.Splits[0].DisplayColumns[1].Width - 22;

            if (c1GridAutoReplaceInfo.Splits[0].DisplayColumns[2].Width < 60)
            {
                c1GridAutoReplaceInfo.Splits[0].DisplayColumns[2].Width = 60;
            }
        }

        private void ResizeAutoReplaceRows()
        {
            c1GridAutoReplaceInfo.AllowRowSizing = RowSizingEnum.IndividualRows;
            c1GridAutoReplaceInfo.Splits[0].ColumnCaptionHeight = 20;

            var rowCount = c1GridAutoReplaceInfo.Splits[0].Rows.Count;

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                c1GridAutoReplaceInfo.Splits[0].Rows[rowIndex].AutoSize();

                if (c1GridAutoReplaceInfo.Splits[0].Rows[rowIndex].Height > 48)
                {
                    c1GridAutoReplaceInfo.Splits[0].Rows[rowIndex].Height = 48;
                }

                if (c1GridAutoReplaceInfo.Splits[0].Rows[rowIndex].Height <= 20)
                {
                    c1GridAutoReplaceInfo.Splits[0].Rows[rowIndex].Height = 20;
                }

                c1GridAutoReplaceInfo.Splits[0].Rows[rowIndex].Height += 1;
            }
        }

        private void c1GridAutoReplaceInfo_AfterUpdate(object sender, EventArgs e)
        {
            UpdateAutoReplaceDictionary();
        }

        private void UpdateAutoReplaceDictionary()
        {
            _autoReplacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var rowCount = c1GridAutoReplaceInfo.Splits[0].Rows.Count;

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var dataRowIndex = c1GridAutoReplaceInfo.Splits[0].Rows[rowIndex].DataRowIndex;
                var keyword = c1GridAutoReplaceInfo[dataRowIndex, AutoReplaceColumn.Keyword]?.ToString() ?? string.Empty;
                var replacement = c1GridAutoReplaceInfo[dataRowIndex, AutoReplaceColumn.Replacement]?.ToString() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(keyword) && !string.IsNullOrWhiteSpace(replacement))
                {
                    _autoReplacements[keyword] = replacement;
                }
            }
        }

        private void c1GridAutoReplaceInfo_Enter(object sender, EventArgs e)
        {
            tsAutoReplace.BackColor = _toolstripFocused;
        }

        private void c1GridAutoReplaceInfo_Leave(object sender, EventArgs e)
        {
            tsAutoReplace.BackColor = _toolstripUnfocused;
            UpdateAutoReplaceDictionary();
        }

        private void c1GridAutoReplaceInfo_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateCursor("AutoReplace");
        }

        private void btnHelp_AutoReplace_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString
            (
                "Auto Replace allows you to bind frequently used query fragments to short keywords.\r\n\r\n" +
                "After typing a keyword and pressing the space key, JasonQuery automatically expands it into the predefined replacement.\r\n\r\n" +
                "This feature is useful for:\r\n" +
                "• Frequently used query templates\r\n" +
                "• Repetitive SQL structures\r\n" +
                "• Quickly inserting long statements or clauses\r\n\r\n" +
                "You can press Ctrl+Z at any time to undo the replacement.",
                "form",
                "OptionsForm",
                "msg",
                "Help_AutoReplace",
                "Text"
            );

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ResizeAutoReplaceReplacementColumn()
        {
            if (!MyLibrary.EnableAutoReplace)
            {
                return;
            }

            c1GridAutoReplaceInfo.Splits[0].DisplayColumns[2].Width = c1GridAutoReplaceInfo.Width - c1GridAutoReplaceInfo.Splits[0].DisplayColumns[1].Width - 22;

            if (c1GridAutoReplaceInfo.Splits[0].DisplayColumns[2].Width < 60)
            {
                c1GridAutoReplaceInfo.Splits[0].DisplayColumns[2].Width = 60;
            }

            c1GridAutoReplaceInfo.Refresh();
        }
    }
}
