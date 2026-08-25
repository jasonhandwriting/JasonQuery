using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private void TextBox_Numeric_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtPort_SQLServer_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPort_SQLServer.Text) || txtPort_SQLServer.Text == "0")
            {
                txtPort_SQLServer.Text = @"1433";
            }
        }

        private void txtPort_MySQL_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPort_MySQL.Text) || txtPort_MySQL.Text == "0")
            {
                txtPort_MySQL.Text = @"3306";
            }
        }

        private void nudQueryTimeout_Enter(object sender, EventArgs e)
        {
            var nud = sender as NumericUpDown;
            UpDownBase text = nud;

            nud.Select(0, text.Text.Length);
        }

        private void nudQueryTimeout_Leave(object sender, EventArgs e)
        {
            var nud = sender as NumericUpDown;
            UpDownBase text = nud;

            if (string.IsNullOrEmpty(text.Text))
            {
                nud.UpButton();
                nud.DownButton();
                nud.Value = 30;
            }

            if (nud.Value < 30 && nud.Value != 0)
            {
                nud.Value = 30;
            }
        }

        private void nudQueryTimeout_MouseClick(object sender, MouseEventArgs e)
        {
            var nud = sender as NumericUpDown;
            UpDownBase text = nud;

            nud.Select(0, text.Text.Length);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Down:
                    {
                        if (c1GridPostgreSQL.Visible && cboDatabase_PostgreSQL.Focused)
                        {
                            c1GridPostgreSQL.Focus();
                            return true;
                        }
                        else if (c1GridSQLServer.Visible && cboDatabase_SQLServer.Focused)
                        {
                            c1GridSQLServer.Focus();
                            return true;
                        }
                        else if (c1GridMySQL.Visible && cboDatabase_MySQL.Focused)
                        {
                            c1GridMySQL.Focus();
                            return true;
                        }

                        break;
                    }
                case Keys.Tab: //Tab
                case Keys.Enter: //Enter
                    {
                        _isKeyPressTab = true;

                        if (c1GridPostgreSQL.Visible)
                        {
                            c1GridPostgreSQL_PasteColumnName();
                            c1GridPostgreSQL.Visible = false;
                            return true;
                        }
                        else if (c1GridSQLServer.Visible)
                        {
                            c1GridSQLServer_PasteColumnName();
                            c1GridSQLServer.Visible = false;
                            return true;
                        }
                        else if (c1GridMySQL.Visible)
                        {
                            c1GridMySQL_PasteColumnName();
                            c1GridMySQL.Visible = false;
                            return true;
                        }

                        break;
                    }
                case Keys.Escape:
                    {
                        _isKeyPressEsc = true;

                        if (c1GridPostgreSQL.Visible)
                        {
                            c1GridPostgreSQL.Visible = false;
                            cboDatabase_PostgreSQL.Focus();
                            return true;
                        }
                        else if (c1GridSQLServer.Visible)
                        {
                            c1GridSQLServer.Visible = false;
                            cboDatabase_SQLServer.Focus();
                            return true;
                        }
                        else if (c1GridMySQL.Visible)
                        {
                            c1GridMySQL.Visible = false;
                            cboDatabase_MySQL.Focus();
                            return true;
                        }

                        break;
                    }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
