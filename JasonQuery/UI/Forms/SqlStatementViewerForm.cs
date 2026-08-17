using JasonLibrary.Core;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public sealed partial class SqlStatementViewerForm : Form
    {
        private ContextMenuStrip _cMenu = new ContextMenuStrip();

        public string Title { get; set; }
        public string CellText { get; set; }
        public string SchemaType { get; set; }
        public string SchemaName { get; set; }

        public SqlStatementViewerForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                LocalizationHelper.ApplyLanguageInfo(this, false);

                if (!string.IsNullOrEmpty(Title))
                {
                    Text = Title;
                }

                var sSchemaTypeInfo = LocalizationHelper.GetLanguageString(SchemaType, "Global", "Global", "msg", SchemaType, "Text");

                if (!string.IsNullOrEmpty(SchemaType) && !string.IsNullOrEmpty(SchemaName))
                {
                    Text += $" - {sSchemaTypeInfo} {SchemaName}";
                }

                if (MyLibrary.IsDarkMode)
                {
                    C1.Win.C1Themes.C1ThemeController.ApplicationTheme = "VS2013Dark";
                }

                ApplySqlStyler();

                editorCellViewer.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
                editorCellViewer.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                editorCellViewer.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));

                editorCellViewer.Text = CellText;
                editorCellViewer.ReadOnly = true;

                var languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text");

                _cMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_cMenu.Items[0]).ShortcutKeys = (Keys.Control | Keys.A);

                _cMenu.Items[0].Click += delegate
                {
                    editorCellViewer.SelectionStart = 0;
                    editorCellViewer.SelectionEnd = editorCellViewer.Text.Length;
                };

                _cMenu.Items.Add("-");

                languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text");
                _cMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_cMenu.Items[2]).ShortcutKeys = (Keys.Control | Keys.C);

                _cMenu.Items[2].Click += delegate
                {
                    editorCellViewer.Copy();
                };

                if (MyLibrary.WordWrap)
                {
                    btnWordWrap.PerformClick();
                }
                else
                {
                    btnWordWrap2.PerformClick();
                }

                switch (MyLibrary.WordWrapIndentMode)
                {
                    case "Fixed":
                        {
                            editorCellViewer.WrapIndentMode = ScintillaNET.WrapIndentMode.Fixed;
                            break;
                        }
                    case "Indent":
                        {
                            editorCellViewer.WrapIndentMode = ScintillaNET.WrapIndentMode.Indent;
                            break;
                        }
                    default:
                        {
                            editorCellViewer.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
                            break;
                        }
                }

                if (MyLibrary.ShowAllCharacters)
                {
                    editorCellViewer.ViewEol = true;
                    editorCellViewer.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
                    btnShowAllCharacters.Visible = false;
                    btnShowAllCharacters2.Visible = true;
                }
                else
                {
                    editorCellViewer.ViewEol = false;
                    editorCellViewer.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
                    btnShowAllCharacters.Visible = true;
                    btnShowAllCharacters2.Visible = false;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "SQLStatementViewerFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "SQLStatementViewerFormHeight", Size.Height.ToString());
        }

        private void btnWordWrap_Click(object sender, EventArgs e)
        {
            if (editorCellViewer.WrapMode == ScintillaNET.WrapMode.Word)
            {
                btnWordWrap.Visible = true;
                btnWordWrap2.Visible = false;
                editorCellViewer.WrapMode = ScintillaNET.WrapMode.None;
            }
            else
            {
                btnWordWrap.Visible = false;
                btnWordWrap2.Visible = true;
                editorCellViewer.WrapMode = ScintillaNET.WrapMode.Word;
                editorCellViewer.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);
            }

            //加上以下這個指令，取消 Word Wrap 後，Focus 才不會跑到最底部！
            editorCellViewer.ScrollCaret();
        }

        private void btnShowAllCharacters_Click(object sender, EventArgs e)
        {
            if (editorCellViewer.ViewEol)
            {
                btnShowAllCharacters.Visible = true;
                btnShowAllCharacters2.Visible = false;
                editorCellViewer.ViewEol = false;
                editorCellViewer.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
            }
            else
            {
                btnShowAllCharacters.Visible = false;
                btnShowAllCharacters2.Visible = true;
                editorCellViewer.ViewEol = true;
                editorCellViewer.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
            }
        }

        private void editorCellViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            //Select All
            _cMenu.Items[0].Enabled = !string.IsNullOrEmpty(editorCellViewer.Text);

            //Copy
            _cMenu.Items[2].Enabled = !string.IsNullOrEmpty(editorCellViewer.SelectedText); //判斷是否有選取文字，決定功能表項目可不可用

            editorCellViewer.ContextMenuStrip = _cMenu;

            if (MyLibrary.IsDarkMode)
            {
                _cMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _cMenu.ForeColor = Color.White;
                _cMenu.RenderMode = ToolStripRenderMode.System;
                //_cMenu.ShowImageMargin = false;
            }

            _cMenu.Show(editorCellViewer, new Point(e.X, e.Y));
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            editorCellViewer.Copy();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            editorCellViewer.SelectionStart = 0;
            editorCellViewer.SelectionEnd = editorCellViewer.Text.Length;
        }

        private void ApplySqlStyler()
        {
            editorCellViewer.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
            editorCellViewer.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
            editorCellViewer.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));

            SqlStyler.ColorEditorBackground = MyLibrary.ColorEditorBackground;
            SqlStyler.ColorTextIdentifier = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorComments = MyLibrary.ColorComments;
            SqlStyler.ColorNumber = MyLibrary.ColorNumber;
            SqlStyler.ColorString = MyLibrary.ColorString;
            SqlStyler.ColorCharacter = MyLibrary.ColorCharacter;
            SqlStyler.ColorOperatorSymbol = MyLibrary.ColorOperatorSymbol;
            SqlStyler.ColorUserDefinedTablesViews = MyLibrary.ColorUserDefinedTablesViews;
            SqlStyler.ColorUserDefinedFunctionsTriggers = MyLibrary.ColorUserDefinedFunctionsTriggers;
            SqlStyler.ColorOperatorKeywords = MyLibrary.ColorOperatorKeywords;
            SqlStyler.ColorBuiltInFunctions = MyLibrary.ColorBuiltInFunctions;
            SqlStyler.ColorBuiltInKeywords = MyLibrary.ColorBuiltInKeywords;
            SqlStyler.ColorUserDefinedKeywords = MyLibrary.ColorUserDefinedKeywords;
            SqlStyler.IsKeywordFontBold = MyLibrary.KeywordFontBold;

            SqlStyler.KeywordsUserDefinedTables = MyLibrary.KeywordsUserDefinedTables;
            SqlStyler.KeywordsUserDefinedViews = MyLibrary.KeywordsUserDefinedViews;
            SqlStyler.KeywordsUserDefinedFunctions = MyLibrary.KeywordsUserDefinedFunctions;
            SqlStyler.KeywordsUserDefinedTriggers = MyLibrary.KeywordsUserDefinedTriggers;
            SqlStyler.KeywordsOperatorKeywords = MyLibrary.KeywordsOperatorKeywords;
            SqlStyler.KeywordsBuiltInFunctions = MyLibrary.KeywordsBuiltInFunctions;
            SqlStyler.KeywordsBuiltInKeywords = MyLibrary.KeywordsBuiltInKeywords;
            SqlStyler.KeywordsUserDefinedKeywords = MyLibrary.KeywordsUserDefinedKeywords;

            editorCellViewer.Styler = new SqlStyler();
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            SaveAsSqlPane();
        }

        private void SaveAsSqlPane()
        {
            if (string.IsNullOrEmpty(editorCellViewer.Text))
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
                    //File.WriteAllText(sf.FileName, editorSqlPane.Text, Encoding.UTF8);
                    TextEngine.WriteContentToFile(editorCellViewer.Text, sf.FileName, TextEncodes.UTF8);
                }
                catch (Exception ex) //20250816 此處沒有取得行號
                {
                    var temp = LocalizationHelper.GetLanguageString("Please check if this file is opened in another program.", "SaveFailedCheck", GetType().Name, "msg", "SaveFailed1", "Text");

                    MessageBox.Show($"{temp}\r\n\r\n{sf.FileName}\r\n\r\n{ex.Message}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}