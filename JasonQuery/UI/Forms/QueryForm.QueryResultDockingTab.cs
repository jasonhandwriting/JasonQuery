using C1.Win.C1Command;
using C1.Win.C1TrueDBGrid;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void ApplyQueryResultDockingTabState()
        {
            SetDockingTabControl();
        }

        private void SetDockingTabControl(bool isSendTabKey = false)
        {
            //isSendTabKey 目前原方法未使用，先保留參數以避免影響既有呼叫端
            ApplyQueryResultTabSelection();
            ApplyQueryResultTabVisibility();
            ApplyQueryResultTabTitle();
        }

        private void ApplyQueryResultTabVisibility()
        {
            var hasUsableQueryResult = ShouldEnableQueryResultGridCommands();
            var hasGridFindText = !string.IsNullOrEmpty(cboFindGrid.Text);
            var hasPrimaryGridRows = HasPrimaryGridRows();

            //tsDataGrid.Enabled = hasUsableQueryResult; //每個要獨立設定
            btnExportToFile.Enabled = hasUsableQueryResult;
            btnAutoSort.Enabled = hasUsableQueryResult;
            lblFindGrid.Enabled = hasUsableQueryResult;

            btnFindNextGrid.Enabled = hasGridFindText;
            btnFindPreviousGrid.Enabled = hasGridFindText;
            btnCountGrid.Enabled = hasGridFindText;
            btnHighlightAllGrid.Enabled = hasGridFindText;
            btnClearHighlightsGrid.Enabled = hasGridFindText;

            btnOptions.Enabled = true;

            //變更工具列按鈕的 Enable 狀態
            chkShowFilterRow.Enabled = hasUsableQueryResult;
            chkSize.Enabled = hasPrimaryGridRows && !chkRawDataMode.Checked;
            cboFindGrid.Enabled = hasUsableQueryResult;
        }

        private void ApplyQueryResultTabSelection()
        {
            if (IsMessageOrSqlHistoryTabSelected())
            {
                return;
            }

            if (!HasAnyQueryResultHostControl())
            {
                return;
            }

            c1TrueDBGrid1.Focus();
        }

        private void ApplyQueryResultTabTitle()
        {
            //目前原本的 SetDockingTabControl() 沒有修改 Tab Title
            //先保留此入口，後續如果要整理 tabDataGrid / 多查詢結果頁籤名稱，可集中放在這裡
        }

        private bool ShouldEnableQueryResultGridCommands()
        {
            if (IsMessageOrSqlHistoryTabSelected())
            {
                return false;
            }

            if (!HasAnyQueryResultHostControl())
            {
                return false;
            }

            return HasPrimaryResultData();
        }

        private bool IsMessageOrSqlHistoryTabSelected()
        {
            var selectedTabName = c1DockingTab1.SelectedTab?.Name ?? string.Empty;

            return string.Equals(selectedTabName, "tabMessage", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(selectedTabName, "tabSqlHistory", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasPrimaryResultData()
        {
            var dt = c1TrueDBGrid1.DataSource as DataTable;

            return dt?.Rows.Count > 0;
        }

        private bool HasPrimaryGridRows()
        {
            return c1TrueDBGrid1.Splits[_splitsIndex].Rows.Count > 0;
        }

        private bool HasAnyQueryResultHostControl()
        {
            foreach (Control tab in c1DockingTab1.TabPages)
            {
                var tabPage = tab as C1DockingTabPage;

                if (tabPage == null)
                {
                    continue;
                }

                foreach (Control ctrlTab in tabPage.Controls)
                {
                    if (ctrlTab is C1TrueDBGrid)
                    {
                        return true;
                    }

                    if (ctrlTab is SplitContainer)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
