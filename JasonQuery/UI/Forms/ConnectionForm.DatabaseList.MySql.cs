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
        private void cboDatabase_MySQL_BeforeDropDownOpen(object sender, System.ComponentModel.CancelEventArgs e)
        {
            c1GridMySQL.Visible = false;

            if (_isComboBoxKeypressForDatabase)
            {
                _isComboBoxKeypressForDatabase = false;
                return;
            }

            UpdateMySQLDatabaseList();
        }

        private void cboDatabase_MySQL_DropDownClosed(object sender, DropDownClosedEventArgs e)
        {
            if (_isKeyPressTab)
            {
                _isKeyPressTab = false;
                c1GridMySQL.Visible = false;
                return;
            }
        }

        private void cboDatabase_MySQL_DropDownOpened(object sender, EventArgs e)
        {
            c1GridMySQL.Visible = false;
        }

        private void UpdateMySQLDatabaseList()
        {
            cboDatabase_MySQL.Items.Clear();

            Cursor = Cursors.WaitCursor;

            if (!CheckData(false, false, true))
            {
                Cursor = Cursors.Default;
                return;
            }

            ConnectToDatabase_MySql(true, true);

            //以下還原，才不會引發錯誤 & 變更 Tab Color
            ResetTransientConnectionState();

            Cursor = Cursors.Default;
        }

        private void cboDatabase_MySQL_KeyUp(object sender, KeyEventArgs e)
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
                var rowCount = c1GridMySQL_Filter(cboDatabase_MySQL.Text);

                if (rowCount > 0)
                {
                    ResizeAutoCompleteGrid(c1GridMySQL, rowCount, 0);
                    c1GridMySQL.Visible = true;
                }
                else
                {
                    c1GridMySQL.Visible = false;
                }
            }
        }

        private void cboDatabase_MySQL_Leave(object sender, EventArgs e)
        {
            if (!c1GridMySQL.Focused)
            {
                c1GridMySQL.Visible = false;
            }
        }

        private void cboDatabase_MySQL_KeyPress(object sender, KeyPressEventArgs e)
        {
            _isComboBoxKeypressForDatabase = true;
        }

        private void cboDatabase_MySQL_Enter(object sender, EventArgs e)
        {
            cboDatabase_MySQL_EnterOrFocus();
        }

        private void cboDatabase_MySQL_EnterOrFocus()
        {
            try
            {
                if (cboDatabase_MySQL.Items.Count == 0 && CheckData(false, false))
                {
                    UpdateMySQLDatabaseList();
                }

                if (_isKeyPressTab)
                {
                    _isKeyPressTab = false;
                    return;
                }

                var rowCount = c1GridMySQL_Filter(cboDatabase_MySQL.Text);

                if (rowCount > 0)
                {
                    ResizeAutoCompleteGrid(c1GridMySQL, rowCount, 0);
                    c1GridMySQL.Visible = true;
                }
                else
                {
                    c1GridMySQL.Visible = false;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private int c1GridMySQL_Filter(string condition0)
        {
            if (_dtDatabaseListInfo_MySql == null)
            {
                return 0;
            }

            var dataView = _dtDatabaseListInfo_MySql.DefaultView;

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

                c1GridMySQL.DataSource = dtSorted;

                return dtSorted.Rows.Count;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 0; //按下 Ctrl+J 可能會進到這個例外錯誤
            }
        }

        private void c1GridMySQL_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13 && e.KeyChar != 9)
            {
                return;
            }

            _isKeyPressTab = true;
            c1GridMySQL_PasteColumnName();
            _isKeyPressTab = true;
        }

        private void c1GridMySQL_PasteColumnName()
        {
            var cellText = c1GridMySQL[c1GridMySQL.Row, 0].ToString();

            cboDatabase_MySQL.Text = cellText;
            c1GridMySQL.Visible = false;
            cboDatabase_MySQL.Focus();
            _isKeyPressTab = true;
        }

        private void c1GridMySQL_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            _isKeyPressTab = true;
            c1GridMySQL_PasteColumnName();
            _isKeyPressTab = true;
        }

        private void cboDatabase_MySQL_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        c1GridMySQL.Focus();
                        e.SuppressKeyPress = false;
                        break;
                    }
                case Keys.Delete:
                    {
                        _isKeyPressDelete = true; //不能在此處理 Delete 按鍵，因為此時的 cboDatabase_MySQL.Text 的值是按下 Delete 鍵之前的！
                        break;
                    }
            }
        }

        private void cboDatabase_MySQL_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isComboBoxKeypressForDatabase)
            {
                return; //鍵盤輸入，忽略！
            }

            _isKeyPressTab = true;
        }

        private void cboDatabase_MySQL_TextChanged(object sender, EventArgs e)
        {
            if (cboDatabase_MySQL.Items.Count == 0)
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

            var rowCount = c1GridMySQL_Filter(cboDatabase_MySQL.Text);

            if (rowCount > 0)
            {
                ResizeAutoCompleteGrid(c1GridMySQL, rowCount, 0);
                c1GridMySQL.Visible = true;
            }
            else
            {
                c1GridMySQL.Visible = false;
            }
        }
    }
}