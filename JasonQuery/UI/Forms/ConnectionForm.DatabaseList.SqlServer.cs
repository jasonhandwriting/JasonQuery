using C1.Win.C1Input;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private void cboDatabase_SQLServer_BeforeDropDownOpen(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                c1GridSQLServer.Visible = false;

                if (_isComboBoxKeypressForDatabase)
                {
                    _isComboBoxKeypressForDatabase = false;
                    return;
                }

                UpdateSQLServerDatabaseList();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboDatabase_SQLServer_DropDownClosed(object sender, DropDownClosedEventArgs e)
        {
            if (_isKeyPressTab)
            {
                _isKeyPressTab = false;
                c1GridSQLServer.Visible = false;
                return;
            }
        }

        private void cboDatabase_SQLServer_DropDownOpened(object sender, EventArgs e)
        {
            c1GridSQLServer.Visible = false;
        }

        private void UpdateSQLServerDatabaseList()
        {
            cboDatabase_SQLServer.Items.Clear();

            Cursor = Cursors.WaitCursor;

            if (!CheckData(false, false))
            {
                Cursor = Cursors.Default;
                return;
            }

            ConnectToDatabase_SqlServer(true, true); //UpdateSQLServerDatabaseList

            //以下還原，才不會引發錯誤 & 變更 Tab Color
            ResetTransientConnectionState();

            Cursor = Cursors.Default;
        }

        private int c1GridSQLServer_Filter(string condition0)
        {
            if (_dtDatabaseListInfo_SqlServer == null)
            {
                return 0;
            }

            var dataView = _dtDatabaseListInfo_SqlServer.DefaultView;

            try
            {
                var condition = $"[Name] LIKE '*{condition0}*'";

                dataView.RowFilter = condition;

                if (dataView.Count == 0)
                {
                    dataView.RowFilter = "[Name] LIKE '*'";
                }

                //20220915 將前面幾個字母相符的優先顯示在最前面
                #region
                dataView.Sort = "Name";

                var dtSorted = dataView.ToTable();

                dtSorted.Columns.Add("Sort", typeof(int));

                var j = -1000;
                var filterKeyword = TextHelper.GetStringBetween(condition, "'", "'").Replace("*", string.Empty);
                var count = dtSorted.Rows.Count;

                for (var i = 0; i < count; i++)
                {
                    var name = dtSorted.Rows[i].GetSafeString("Name");

                    if (name.Length >= filterKeyword.Length && name.StartsWith(filterKeyword, StringComparison.OrdinalIgnoreCase))
                    {
                        dtSorted.Rows[i]["Sort"] = j;
                        j++;
                    }
                    else
                    {
                        dtSorted.Rows[i]["Sort"] = i;
                    }
                }

                dataView = dtSorted.DefaultView;
                dataView.Sort = "Sort, Name";
                dtSorted = dataView.ToTable();
                dtSorted.Columns.Remove("Sort");
                #endregion

                c1GridSQLServer.DataSource = dtSorted;

                return dtSorted.Rows.Count;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 0; //按下 Ctrl+J 可能會進到這個例外錯誤
            }
        }

        private void c1GridSQLServer_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13 && e.KeyChar != 9)
            {
                return;
            }

            _isKeyPressTab = true;
            c1GridSQLServer_PasteColumnName();
            _isKeyPressTab = true;
        }

        private void c1GridSQLServer_PasteColumnName()
        {
            var cellText = c1GridSQLServer[c1GridSQLServer.Row, 0].ToString();

            cboDatabase_SQLServer.Text = cellText;
            c1GridSQLServer.Visible = false;
            cboDatabase_SQLServer.Focus();
            _isKeyPressTab = true;
        }

        private void c1GridSQLServer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            _isKeyPressTab = true;
            c1GridSQLServer_PasteColumnName();
            _isKeyPressTab = true;
        }

        private void cboDatabase_SQLServer_Enter(object sender, EventArgs e)
        {
            cboDatabase_SQLServer_EnterOrFocus();
        }

        private void cboDatabase_SQLServer_EnterOrFocus()
        {
            try
            {
                if (cboDatabase_SQLServer.Items.Count == 0 && CheckData(false, false))
                {
                    UpdateSQLServerDatabaseList();
                }

                if (_isKeyPressTab)
                {
                    _isKeyPressTab = false;
                    return;
                }

                var rowCount = c1GridSQLServer_Filter(cboDatabase_SQLServer.Text);

                if (rowCount > 0)
                {
                    ResizeAutoCompleteGrid(c1GridSQLServer, rowCount, 0);
                    c1GridSQLServer.Visible = true;
                }
                else
                {
                    c1GridSQLServer.Visible = false;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboDatabase_SQLServer_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        c1GridSQLServer.Focus();
                        e.SuppressKeyPress = false;
                        break;
                    }
                case Keys.Delete:
                    {
                        _isKeyPressDelete = true; //不能在此處理 Delete 按鍵，因為此時的 cboDatabase_SQLServer.Text 的值是按下 Delete 鍵之前的！
                        break;
                    }
            }
        }

        private void cboDatabase_SQLServer_KeyPress(object sender, KeyPressEventArgs e)
        {
            _isComboBoxKeypressForDatabase = true;
        }

        private void cboDatabase_SQLServer_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (_isKeyPressTab)
                {
                    _isKeyPressTab = false; //Tab 會回到這裡，不重複處理！
                }
                else if (_isKeyPressEsc)
                {
                    _isKeyPressEsc = false; //ESC 會回到這裡，不重複處理！
                }
                else
                {
                    var rowCount = c1GridSQLServer_Filter(cboDatabase_SQLServer.Text);

                    if (rowCount > 0)
                    {
                        ResizeAutoCompleteGrid(c1GridSQLServer, rowCount, 0);
                        c1GridSQLServer.Visible = true;
                    }
                    else
                    {
                        c1GridSQLServer.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboDatabase_SQLServer_Leave(object sender, EventArgs e)
        {
            if (!c1GridSQLServer.Focused)
            {
                c1GridSQLServer.Visible = false;
            }
        }

        private void cboDatabase_SQLServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isComboBoxKeypressForDatabase)
            {
                return; //鍵盤輸入，忽略！
            }

            _isKeyPressTab = true;
        }

        private void cboDatabase_SQLServer_TextChanged(object sender, EventArgs e)
        {
            if (cboDatabase_SQLServer.Items.Count == 0)
            {
                return;
            }

            if (_isKeyPressDelete)
            {
                _isKeyPressDelete = false;
            }
            else
            {
                return;
            }

            var rowCount = c1GridSQLServer_Filter(cboDatabase_SQLServer.Text);

            if (rowCount > 0)
            {
                ResizeAutoCompleteGrid(c1GridSQLServer, rowCount, 0);
                c1GridSQLServer.Visible = true;
            }
            else
            {
                c1GridSQLServer.Visible = false;
            }
        }
    }
}
