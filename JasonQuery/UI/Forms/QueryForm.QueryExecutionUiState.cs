using JasonQuery.Core.Config;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void ApplyQueryExecutionTimerBusyState()
        {
            _startTime = DateTime.Now;
            tmrExecTime.Enabled = true;
            tmrQueryTime.Enabled = true;
            _isBusy = true;
        }

        private void ApplyBeforeQueryExecutionUiState(QueryExecutionKindResult queryKind, string queryText)
        {
            if (queryKind == null)
            {
                return;
            }

            if (!queryKind.IsQuery)
            {
                ApplyBeforeNonQueryExecutionUiState(queryKind);
                return;
            }

            ApplyBeforeQueryExecutionQueryState(queryKind, queryText);
        }

        private void ApplyBeforeNonQueryExecutionUiState(QueryExecutionKindResult queryKind)
        {
            //20201108 非查詢，Data Grid 清空！
            c1TrueDBGrid1.DataSource = _dtNullTable;

            if (queryKind.IsCommitRollbackScript) //按下 Commit / Rollback 按鈕；或是執行 Commit / Rollback 指令
            {
                btnQuery.AccessibleDescription = "Query"; //執行 Commit or Rollback 後，會「中斷連線」，故以「Query 狀態」來處理

                //傳遞資訊至 MainForm，更新 Commit/Rollbak 狀態
                TransferValueToMainForm("ExecuteCommitRollback`");
            }
            else
            {
                btnQuery.AccessibleDescription = queryKind.IgnoreCommitRollback ? "Query" : "NonQuery";
            }
        }

        private void ApplyBeforeQueryExecutionQueryState(QueryExecutionKindResult queryKind, string queryText)
        {
            btnQuery.AccessibleDescription = "Query";

            //20240719 取消凍結欄位，查詢其他 SQL 時，才不會有殘留問題
            if (!string.IsNullOrEmpty(_unfreezeColumnName))
            {
                FrozenColumn(false);
            }

            //20201124 針對所有查詢，先切換到 tabDataGrid (查詢結束後，頁籤就會停留在 tabDataGrid，Focus 是在 Edior)
            c1DockingTab1.SelectedTab = tabDataGrid;

            RemoveDynamicQueryResultTabs();

            if (queryKind.QueryCount == 1)
            {
                _queryIndex = 1;
                c1TrueDBGrid1.Enabled = false;
                c1TrueDBGrid1.AccessibleDescription = queryText;
            }
        }

        private void RemoveDynamicQueryResultTabs()
        {
            for (var i = 1; i <= 20; i++)
            {
                if (c1DockingTab1.TabPages.Count <= 2)
                {
                    break; //基本的 2 個頁籤，不用再往下判斷！
                }

                foreach (Control ctrl in c1DockingTab1.TabPages)
                {
                    if (!_originalTabName.Contains($"`{ctrl.Text}`"))
                    {
                        c1DockingTab1.TabPages.Remove(ctrl);
                    }
                }
            }
        }

        private void ApplyQueryExecutionButtonsBusyState()
        {
            btnQuery.Enabled = false;
            btnExecuteCurrentBlock.Enabled = false;
            btnCancelQuery.Enabled = true;
        }

        private void ApplyQueryExecutionMessageState()
        {
            //清除訊息
            UpdateMessage(string.Empty);
            editorMessage.Tag = string.Empty;

            lblInfo.Text = string.Empty;
            lblInfo.Tag = string.Empty;

            //清除波浪底線！
            SetSquiggle(true);
        }

        private string BuildQueryExecutionPayloadText(string queryText, int selectionStart, string crLfOffsetText)
        {
            return $"{AccessibleDescription}{MyGlobal.Separator5}{selectionStart}{crLfOffsetText}{MyGlobal.Separator5}{queryText}";
        }

        private void ApplyQueryExecutionBookmark(bool showIndicator)
        {
            //刪除所有的 Bookmark
            editor.MarkerDeleteAll(-1);

            //Current Line 加上 Bookmark
            if (showIndicator) //20220808 判斷要不要加上「執行 SQL 的指示箭頭」符號 (透過功能表切換資料庫，就不用顯示箭頭指示)
            {
                editor.Lines[editor.CurrentLine].MarkerAdd(BookmarkMarker);
            }
        }
    }
}