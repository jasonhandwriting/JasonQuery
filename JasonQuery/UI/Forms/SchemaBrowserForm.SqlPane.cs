using JasonLibrary.Core;
using JasonLibrary.UI.Controls;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using ScintillaNET;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void editorSqlPane_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ClearSqlPaneHighlightSelectionCopyState();
                return;
            }

            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            //Select All
            _sqlPaneContextMenu.Items[0].Enabled = !string.IsNullOrEmpty(editorSqlPane.Text);

            //Copy
            _sqlPaneContextMenu.Items[1].Enabled = !string.IsNullOrEmpty(editorSqlPane.SelectedText); //判斷是否有選取文字，決定功能表項目可不可用

            //Save As
            _sqlPaneContextMenu.Items[2].Enabled = !string.IsNullOrEmpty(editorSqlPane.Text);

            editorSqlPane.ContextMenuStrip = _sqlPaneContextMenu;

            if (MyLibrary.IsDarkMode)
            {
                _sqlPaneContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _sqlPaneContextMenu.ForeColor = Color.White;
                _sqlPaneContextMenu.RenderMode = ToolStripRenderMode.System;
            }

            _sqlPaneContextMenu.Show(editorSqlPane, new Point(e.X, e.Y));
        }

        private void editorSqlPane_MouseUp(object sender, MouseEventArgs e)
        {
            btnSelectAllSqlPane.Enabled = !string.IsNullOrEmpty(editorSqlPane.Text);
            btnCopySqlPane.Enabled = !string.IsNullOrEmpty(editorSqlPane.SelectedText);
        }

        private void editorSqlPane_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; //其他按鍵，忽略！
        }

        private void editorSqlPane_KeyDown(object sender, KeyEventArgs e)
        {
            if (TryHandleSqlPaneCopyShortcutKeyDown(e))
            {
                return;
            }

            switch (e.KeyData)
            {
                //停用 Enter, Ctrl+V, Ctrl+X, Delete, Back
                case Keys.Enter:
                case Keys.Control | Keys.V:
                case Keys.Control | Keys.X:
                case Keys.Delete:
                case Keys.Back:
                    {
                        e.Handled = true;
                        return;
                    }
                case Keys.Control | Keys.A:
                    {
                        editorSqlPane.SelectionStart = 0;
                        editorSqlPane.SelectionEnd = editorSqlPane.Text.Length;
                        e.Handled = false;
                        return;
                    }
            }

            if (e.KeyData == (Keys.Control | Keys.Home) || e.KeyData == (Keys.Control | Keys.End) || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
            {
                //以下，當放大縮小後，即時調整 line number 的寬度，避免因為放大時，line number 最左側的數字會看不見
                var start = editorSqlPane.SelectionStart;

                editorSqlPane.ReadOnly = false;
                editorSqlPane.Text += "\r\n";
                editorSqlPane.Text = editorSqlPane.Text.Substring(0, editorSqlPane.Text.Length - 2);
                editorSqlPane.SelectionStart = start;
                editorSqlPane.ReadOnly = true;
                editorSqlPane.ScrollCaret();
                return;
            }

            switch (e.KeyCode)
            {
                //原本的上下左右鍵，可以發揮作用；加上 Shift Key，一樣可以發揮作用！
                case Keys.Left:
                case Keys.Shift | Keys.Left:
                case Keys.Right:
                case Keys.Shift | Keys.Right:
                case Keys.Up:
                case Keys.Shift | Keys.Up:
                case Keys.Down:
                case Keys.Shift | Keys.Down:
                case Keys.Home:
                case Keys.End:
                    {
                        e.Handled = false;
                        break;
                    }
                default:
                    {
                        e.Handled = true; //其他按鍵，忽略！
                        break;
                    }
            }
        }

        private void editorSqlPane_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (IsSqlEditorCopyShortcut(e))
                {
                    CopySqlPaneSelection("editorSqlPane_KeyUp(CtrlC)");
                    return;
                }
            }
            finally
            {
                ClearSqlPaneHighlightSelectionCopyState();

                btnSelectAllSqlPane.Enabled = !string.IsNullOrEmpty(editorSqlPane.Text);
                btnCopySqlPane.Enabled = !string.IsNullOrEmpty(editorSqlPane.SelectedText);
            }
        }

        private void editorSqlPane_Enter(object sender, EventArgs e)
        {
            _findAndReplace.Scintilla = (ScintillaEditor)sender;
        }

        private void btnWordWrapSqlPane_Click(object sender, EventArgs e)
        {
            if (editorSqlPane.WrapMode == WrapMode.Word)
            {
                btnWordWrapSqlPane.Visible = true;
                btnWordWrap2SqlPane.Visible = false;
                editorSqlPane.WrapMode = WrapMode.None;
            }
            else
            {
                btnWordWrapSqlPane.Visible = false;
                btnWordWrap2SqlPane.Visible = true;
                editorSqlPane.WrapMode = WrapMode.Word;
            }

            //加上以下這個指令，取消 Word Wrap 後，Focus 才不會跑到最底部！
            editorSqlPane.ScrollCaret();
        }

        private void btnSelectAllSqlPane_Click(object sender, EventArgs e)
        {
            editorSqlPane.SelectionStart = 0;
            editorSqlPane.SelectionEnd = editorSqlPane.Text.Length;
        }

        private void btnCopySqlPane_Click(object sender, EventArgs e)
        {
            CopySqlPane();
        }

        private void CopySqlPane()
        {
            if (string.IsNullOrEmpty(editorSqlPane.Text))
            {
                return;
            }

            if (string.IsNullOrEmpty(editorSqlPane.SelectedText) && !_isSqlPaneHighlightSelectionCopyMode)
            {
                return;
            }

            CopySqlPaneSelection("CopySqlPane");
        }

        private void btnSaveAsSqlPane_Click(object sender, EventArgs e)
        {
            SaveAsSqlPane();
        }

        private void SaveAsSqlPane()
        {
            if (string.IsNullOrEmpty(editorSqlPane.Text))
            {
                return;
            }

            var sf = new SaveFileDialog
            {
                Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text"),
                FileName = string.Empty,
                RestoreDirectory = true,
                Filter = @"Query file (*.sql)|*.sql|All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text")).Replace("Query file", LocalizationHelper.GetLanguageString("Query file", "Global", "Global", "msg", "QueryFile", "Text"))
            };

            if (sf.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(Path.GetExtension(sf.FileName)))
                {
                    sf.FileName += ".sql";
                }

                try
                {
                    TextEngine.WriteContentToFile(editorSqlPane.Text, sf.FileName, TextEncodes.UTF8);
                }
                catch (Exception ex)
                {
                    var temp = LocalizationHelper.GetLanguageString("Please check if this file is opened in another program.", "form", GetType().Name, "msg", "SaveFailedCheck", "Text");

                    MessageBox.Show($"{temp}\r\n\r\n{sf.FileName}\r\n\r\n{ex.Message}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
