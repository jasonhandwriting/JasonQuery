using JasonLibrary.Core;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void editorSqlPreview_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (IsSqlEditorCopyShortcut(e))
                {
                    CopySqlPreviewSelection("editorSqlPreview_KeyUp(CtrlC)");
                    return;
                }
            }
            finally
            {
                ClearSqlPreviewHighlightSelectionCopyState();
            }
        }

        private void editorSqlPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ClearSqlPreviewHighlightSelectionCopyState();
                return;
            }

            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.SelectAll].Enabled = !string.IsNullOrEmpty(editorSqlPreview.Text);
            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Copy].Enabled = !string.IsNullOrEmpty(editorSqlPreview.SelectedText); //判斷是否有選取文字，決定功能表項目可不可用
            _sqlPreviewContextMenu.Items[SqlPreviewColumn.SaveAs].Enabled = !string.IsNullOrEmpty(editorSqlPreview.Text);
            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Apply].Enabled = btnApplyEditSqlPreview.Enabled;
            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Cancel].Enabled = btnCancelEditSqlPreview.Enabled;
            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Commit].Enabled = btnCommitSqlPreview.Enabled;
            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Rollback].Enabled = btnRollbackSqlPreview.Enabled;

            editorSqlPreview.ContextMenuStrip = _sqlPreviewContextMenu;

            if (MyLibrary.IsDarkMode)
            {
                _sqlPreviewContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _sqlPreviewContextMenu.ForeColor = Color.White;
                _sqlPreviewContextMenu.RenderMode = ToolStripRenderMode.System;
            }

            _sqlPreviewContextMenu.Show(editorSqlPreview, new Point(e.X, e.Y));
        }

        private void editorSqlPreview_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; //其他按鍵，忽略！
        }

        private void editorSqlPreview_KeyDown(object sender, KeyEventArgs e)
        {
            if (TryHandleSqlPreviewCopyShortcutKeyDown(e))
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
                        editorSqlPreview.SelectionStart = 0;
                        editorSqlPreview.SelectionEnd = editorSqlPreview.Text.Length;
                        e.Handled = false;
                        return;
                    }
            }

            if (e.KeyData == (Keys.Control | Keys.Home) || e.KeyData == (Keys.Control | Keys.End) || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
            {
                //以下，當放大縮小後，即時調整 line number 的寬度，避免因為放大時，line number 最左側的數字會看不見
                var start = editorSqlPreview.SelectionStart;

                editorSqlPreview.ReadOnly = false;
                editorSqlPreview.Text += "\r\n";
                editorSqlPreview.Text = editorSqlPreview.Text.Substring(0, editorSqlPreview.Text.Length - 2);
                editorSqlPreview.SelectionStart = start;
                editorSqlPreview.ReadOnly = true;
                editorSqlPreview.ScrollCaret();
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
    }
}