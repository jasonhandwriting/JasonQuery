using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void btnSqlPreview_Click(object sender, EventArgs e)
        {
            tabSchemaBrowser.SelectedTab = tabSqlPreview;
        }

        private void tabSqlPreview_Enter(object sender, EventArgs e)
        {
            GenerateSqlPreview();
        }

        private void GenerateSqlPreview()
        {
            btnApplyEditSqlPreview.Enabled = btnApplyEditData.Enabled;
            btnCancelEditSqlPreview.Enabled = btnCancelEditData.Enabled;
            btnCommitSqlPreview.Enabled = btnCommitData.Enabled;
            btnRollbackSqlPreview.Enabled = btnRollbackData.Enabled;

            try
            {
                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            GenerateSqlPreview_Oracle();
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            GenerateSqlPreview_PostgreSql();
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            GenerateSqlPreview_SqlServer();
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            GenerateSqlPreview_MySql();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (_currentSourceType == DataSourceType.Oracle && _oracleSqlPreviewValidationFailed)
            {
                //20260719 針對 Oracle，如果是主鍵或不允許為空的欄位，不可以輸入空字串 ''，因為 '' 對 Oracle 而言等同於 NULL
                return;
            }

            var directBinaryNotes = BuildDirectBinaryPreviewNotes();

            if (!string.IsNullOrWhiteSpace(directBinaryNotes))
            {
                editorSqlPreview.ReadOnly = false;
                editorSqlPreview.Text = string.IsNullOrWhiteSpace(editorSqlPreview.Text) ? directBinaryNotes : $"{editorSqlPreview.Text.TrimEnd()}\r\n\r\n{directBinaryNotes}";
                editorSqlPreview.ReadOnly = true;
            }

            if (!string.IsNullOrEmpty(editorSqlPreview.Text))
            {
                editorSqlPreview.ReadOnly = false;
                editorSqlPreview.Text = $"{_sqlPreviewHint1}\r\n{_sqlPreviewHint2}\r\n\r\n{editorSqlPreview.Text}";
                editorSqlPreview.ReadOnly = true;
            }

            //20240504 切換到 editorSql 會重複觸發 tabSqlPreview_Enter，故以下先解除綁定，再重新綁定
            tabSqlPreview.Enter -= new EventHandler(tabSqlPreview_Enter);
            editorSqlPreview.Focus();
            tabSqlPreview.Enter += new EventHandler(tabSqlPreview_Enter);
        }

        private string GetFormalDateTimeValue(string dateTimeValue) //轉換成指定的日期格式 yyyy-MM-dd (後面字串維持不變)，如果無法轉換成日期格式，則原樣回傳
        {
            var dateValue = TextHelper.GetSafeSubstring(dateTimeValue, 0, 10);
            var timeValue = TextHelper.GetSafeSubstring(dateTimeValue, 11);
            var isValidDateTime = DateTime.TryParse(dateValue, out var dateValue2);

            if (isValidDateTime)
            {
                var dateValueNew = dateValue2.ToString("yyyy-MM-dd"); //20240722 日期的分隔符號要置換為 -，否則會報錯

                return $"{dateValueNew}{timeValue}";
            }
            else
            {
                return $"{dateValue}{timeValue}";
            }
        }

        //回傳「日期+時間+毫秒」
        private string GetOracleTimeStamp(DateTime timeMilliseconds, string columnDataType)
        {
            var milliseconds = string.Empty;
            var numericScale = 0;

            if (columnDataType.IndexOf('(') >= 0 && columnDataType.IndexOf(')') >= 0)
            {
                var temp = TextHelper.GetStringBetween2(columnDataType, "(", ")", true);

                int.TryParse(temp, out numericScale);
            }

            var timeOfDayMilliseconds = timeMilliseconds.TimeOfDay.ToString();

            if (timeOfDayMilliseconds.Contains("."))
            {
                var temp = timeOfDayMilliseconds.IndexOf('.');
                var timeOfDayMillisecondsTemp = timeOfDayMilliseconds.Substring(temp + 1);

                timeOfDayMilliseconds = $".{timeOfDayMillisecondsTemp}";
            }
            else
            {
                //沒有包含 . 符號，表示毫秒值為 0
                timeOfDayMilliseconds = new string('0', numericScale);

                if (!string.IsNullOrEmpty(timeOfDayMilliseconds))
                {
                    timeOfDayMilliseconds = $".{timeOfDayMilliseconds}";
                }
            }

            if (timeOfDayMilliseconds.Length >= numericScale + 1)
            {
                milliseconds = timeOfDayMilliseconds.Substring(0, numericScale + 1);
            }
            else
            {
                var temp = new string('0', numericScale - timeOfDayMilliseconds.Length);

                milliseconds = $"{timeOfDayMilliseconds}{temp}";
            }

            return timeMilliseconds + milliseconds;
        }

        private IEnumerable<IGrouping<int, Point>> GetModifiedCellDisplayRowGroups()
        {
            var visibleRowCount = GetVisibleDataGridRowCount();

            if (visibleRowCount <= 0)
            {
                return Enumerable.Empty<IGrouping<int, Point>>();
            }

            return _modifiedCells.Where(cell => cell.Y >= 0 && cell.Y < visibleRowCount)
                                 .OrderBy(cell => cell.Y)
                                 .ThenBy(cell => cell.X)
                                 .GroupBy(cell => cell.Y);
        }
    }
}
