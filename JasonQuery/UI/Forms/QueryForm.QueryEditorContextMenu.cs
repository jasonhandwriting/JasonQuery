using IconLibrary;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private ToolStripMenuItem _menuSql2CSharp1;
        private ToolStripMenuItem _menuSql2CSharp2;
        private ToolStripMenuItem _menuSql2CSharp3;
        private ToolStripMenuItem _menuSql2CSharp4;
        private ToolStripMenuItem _menuSql2VbNet1;
        private ToolStripMenuItem _menuSql2VbNet2;
        private ToolStripMenuItem _menuSql2VbNet3;
        private ToolStripMenuItem _menuSql2Vb61;
        private ToolStripMenuItem _menuSql2Vb62;
        private ToolStripMenuItem _menuSql2Delphi61;
        private ToolStripMenuItem _menuSql2Delphi62;

        private void ApplyEditorMenu() //套用 Editor 右鍵功能表
        {
            InitializeEditorContextMenu();
            BindEditorMenuEvents();
        }

        private void InitializeEditorContextMenu()
        {
            _queryEditorContextMenu = QueryEditorContextMenuBuilder.Build(GetType().Name);
        }

        private void BindEditorMenuClick(int index, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _queryEditorContextMenu.Items[index].Click += (sender, e) => action();
        }

        private void BindMenuClick(ToolStripItem menuItem, Action action)
        {
            if (menuItem == null)
            {
                throw new ArgumentNullException(nameof(menuItem));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            menuItem.Click += (sender, e) => action();
        }

        private void BindPerformClick(ToolStripItem sourceMenuItem, ToolStripItem targetMenuItem)
        {
            if (targetMenuItem == null)
            {
                throw new ArgumentNullException(nameof(targetMenuItem));
            }

            BindMenuClick(sourceMenuItem, () => targetMenuItem.PerformClick());
        }

        private ToolStripMenuItem GetEditorMenuItem(int index)
        {
            return (ToolStripMenuItem)_queryEditorContextMenu.Items[index];
        }

        private string GetEditorObjectText(string defaultText, string key)
        {
            return LocalizationHelper.GetLanguageString(defaultText, "form", GetType().Name, "object", key, "Text");
        }

        private void BindEditorMenuEvents()
        {
            BindEditorExecuteMenuEvents();
            BindEditorSchemaMenuEvents();
            BindEditorTableMenuEvents();
            BindEditorSelectionMenuEvents();
            BindEditorEditMenuEvents();
            BindEditorCodeConversionMenuEvents();
            BindEditorFormatMenuEvents();
        }

        private void BindEditorExecuteMenuEvents()
        {
            BindEditorMenuClick(QueryEditorColumn.Execute, () => btnQuery.PerformClick());
            BindEditorMenuClick(QueryEditorColumn.ExecuteCurrentBlock, () => btnExecuteCurrentBlock.PerformClick());
            BindEditorMenuClick(QueryEditorColumn.ExecuteCurrentLine, () => btnExecuteCurrentLine.PerformClick());
        }

        private void BindEditorSchemaMenuEvents()
        {
            BindEditorMenuClick
            (
                QueryEditorColumn.ShowCreateScript,
                () =>
                {
                    var payload = GetEditorObjectMenuPayload(QueryEditorColumn.ShowCreateScript);
                    var subTitle = LocalizationHelper.GetLanguageString("Show Create Script", "Global", "Global", "msg", "DDLViewerCreateScript", "Text");

                    UIHelper.ShowDDL(payload.SchemaType, payload.SchemaName, payload.SchemaNode, payload.SchemaDbo, string.Empty, subTitle);
                }
            );

            BindEditorMenuClick
            (
                QueryEditorColumn.SchemaBrowserTable,
                () =>
                {
                    var payload = GetEditorObjectMenuPayload(QueryEditorColumn.SchemaBrowserTable);

                    MyGlobal.DisplaySchemaBrowser(payload.SchemaNode, payload.SchemaType, payload.SchemaName, payload.SchemaDbo, payload.ObjectId, true);
                }
            );

            BindEditorMenuClick
            (
                QueryEditorColumn.SchemaBrowserView,
                () =>
                {
                    var payload = GetEditorObjectMenuPayload(QueryEditorColumn.SchemaBrowserView);

                    MyGlobal.DisplaySchemaBrowser(payload.SchemaNode, payload.SchemaType, payload.SchemaName, payload.SchemaDbo, payload.ObjectId);
                }
            );

            BindEditorMenuClick(QueryEditorColumn.GenerateSql, ShowGenerateSqlFormFromEditorMenu);
        }

        private void ShowGenerateSqlFormFromEditorMenu()
        {
            using (var form = new GenerateSqlForm())
            {
                var payload = GetEditorObjectMenuPayload(QueryEditorColumn.GenerateSql);

                form.SchemaNode = payload.SchemaNode;
                form.SchemaType = payload.SchemaType;
                form.SchemaName = payload.SchemaName;
                form.SchemaDbo = payload.SchemaDbo;
                form.ObjectId = payload.ObjectId;
                form.AccessibleDescriptionString = payload.AccessibleDescription;

                //20250426 改寫 Width/Height 取值方法
                var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings
                (
                    MyGlobal.DomainUser,
                    "GenerateSqlFormWidth",
                    "GenerateSqlFormHeight",
                    defaultWidth: form.ClientSize.Width,
                    defaultHeight: form.ClientSize.Height
                );

                form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                form.ShowDialog();
            }
        }

        private void BindEditorTableMenuEvents()
        {
            BindEditorMenuClick(QueryEditorColumn.TableComment, () => ShowEditTableFormFromEditorMenu(QueryEditorColumn.TableComment, EditTableMode.Comment, "Table Comment.ico"));
            BindEditorMenuClick(QueryEditorColumn.TableDrop, () => ShowEditTableFormFromEditorMenu(QueryEditorColumn.TableDrop, EditTableMode.Drop, "Table Drop.ico"));
            BindEditorMenuClick(QueryEditorColumn.TableRename, () => ShowEditTableFormFromEditorMenu(QueryEditorColumn.TableRename, EditTableMode.Rename, "Table Rename.ico"));
            BindEditorMenuClick(QueryEditorColumn.TableTruncate, () => ShowEditTableFormFromEditorMenu(QueryEditorColumn.TableTruncate, EditTableMode.Truncate, "Table Truncate.ico"));
        }

        private void ShowEditTableFormFromEditorMenu(int menuIndex, EditTableMode editMode, string iconFileName)
        {
            using (var form = new EditTableForm())
            {
                var payload = GetEditorObjectMenuPayload(menuIndex);

                form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, iconFileName);
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.SchemaName = IsSqlServer ? payload.SchemaDbo : payload.SchemaNode;
                form.TableName = payload.SchemaName;

                if (editMode == EditTableMode.Comment)
                {
                    form.SchemaDatabase = payload.SchemaNode; //暫時只有 SQL Server 會用到
                }

                form.EditMode = editMode;
                form.ShowDialog();
            }
        }

        private void BindEditorSelectionMenuEvents()
        {
            BindEditorMenuClick
            (
                QueryEditorColumn.DeleteCurrentLine,
                () =>
                {
                    //20250519 透過 SendKeys 達到 Ctrl+L 的效果
                    editor.Focus();
                    SendKeys.SendWait("^L");
                }
            );

            BindEditorMenuClick
            (
                QueryEditorColumn.SelectAll,
                () =>
                {
                    editor.SelectionStart = 0;
                    editor.SelectionEnd = editor.Text.Length;
                }
            );

            BindEditorMenuClick(QueryEditorColumn.SelectCurrentBlock, () => SelectCurrentBlock());
            BindEditorMenuClick(QueryEditorColumn.SelectCurrentLine, () => SelectActiveLine());
        }

        private void BindEditorEditMenuEvents()
        {
            BindEditorMenuClick(QueryEditorColumn.Undo, ExecuteEditorUndo);
            BindEditorMenuClick(QueryEditorColumn.Redo, ExecuteEditorRedo);
            BindEditorMenuClick(QueryEditorColumn.Cut, ExecuteEditorCut);
            BindEditorMenuClick(QueryEditorColumn.Copy, ExecuteEditorCopy);

            BindEditorMenuClick
            (
                QueryEditorColumn.CopyTo,
                () =>
                {
                    //尚未完成
                }
            );

            BindEditorMenuClick(QueryEditorColumn.Paste, ExecuteEditorPaste);

            BindEditorMenuClick
            (
                QueryEditorColumn.Delete,
                () =>
                {
                    editor.ReplaceSelection(string.Empty);
                    editor.SelectionStart = editor.SelectionStart;
                    editor.SelectionEnd = editor.SelectionStart;
                }
            );

            BindEditorMenuClick
            (
                QueryEditorColumn.ZoomIn,
                () =>
                {
                    editor.ZoomIn();
                }
            );

            BindEditorMenuClick
            (
                QueryEditorColumn.ZoomOut,
                () =>
                {
                    editor.ZoomOut();
                }
            );

            BindEditorMenuClick
            (
                QueryEditorColumn.FindAndReplace,
                () =>
                {
                    HideAutoCompleteGrid(false);
                    _findAndReplace.ShowFind();
                }
            );
        }

        private void ExecuteEditorUndo()
        {
            try
            {
                editor.Undo();
                lblInfo.Text = string.Empty;
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void ExecuteEditorRedo()
        {
            try
            {
                editor.Redo();
                lblInfo.Text = string.Empty;
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void ExecuteEditorCut()
        {
            if (string.IsNullOrEmpty(editor.Text))
            {
                return;
            }

            try
            {
                CopySelectedEditorText("ApplyEditorMenu(01)");
                editor.Clear();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void ExecuteEditorCopy()
        {
            if (string.IsNullOrEmpty(editor.Text))
            {
                return;
            }

            try
            {
                Clipboard.Clear();
                CopySelectedEditorText("ApplyEditorMenu(03)");
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void CopySelectedEditorText(string traceSource)
        {
            if (TryCopyHighlightSelectionText(traceSource))
            {
                return;
            }

            if (MyLibrary.CopyAsHTML)
            {
                editor.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
            }
            else
            {
                editor.Copy();
            }
        }

        private void ExecuteEditorPaste()
        {
            try
            {
                var originalText = Clipboard.GetText();

                TextHelper.CopyTextToClipboard(originalText.Replace("\t", "    "), "ApplyEditorMenu(06)");
                editor.Paste();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void BindEditorCodeConversionMenuEvents()
        {
            InitializeCodeToSqlSubMenu();
            InitializeSqlToCodeSubMenu();
        }

        private void InitializeCodeToSqlSubMenu()
        {
            var codeToSqlMenu = GetEditorMenuItem(QueryEditorColumn.Code2SQL);

            AddCodeToSqlMenuItem(codeToSqlMenu, "C# to SQL", "C# 16x16.ico", "C#", "\"", "//");
            AddCodeToSqlMenuItem(codeToSqlMenu, "VB.Net/VB6/VBA to SQL", "VB.Net 16x16.ico", "VB", "\"", "'");
            AddCodeToSqlMenuItem(codeToSqlMenu, "Delphi6 to SQL", "Delphi 16x16.ico", "Delphi", "'", "//");
        }

        private void AddCodeToSqlMenuItem(ToolStripMenuItem parentMenu, string text, string iconFileName, string language, string quoteText, string commentText)
        {
            var item = new ToolStripMenuItem(text)
            {
                Image = IconManager.GetImage(MyGlobal.IconLibrary, iconFileName)
            };

            BindMenuClick
            (
                item,
                () =>
                {
                    if (editor.CanPaste)
                    {
                        CodeToSql(language, quoteText, commentText);
                    }
                }
            );

            parentMenu.DropDownItems.Add(item);
        }

        private void InitializeSqlToCodeSubMenu()
        {
            //20241225 建立第二層選單項目 SQL to Code
            var mnuSql2CSharp = new ToolStripMenuItem("SQL to C#");
            var mnuSql2VbNet = new ToolStripMenuItem("SQL to VB.Net");
            var mnuSql2Vb6 = new ToolStripMenuItem("SQL to VB6/VBA");
            var mnuSql2Delphi6 = new ToolStripMenuItem("SQL to Delphi6");

            mnuSql2CSharp.Image = IconManager.GetImage(MyGlobal.IconLibrary, "C# 16x16.ico");
            mnuSql2VbNet.Image = IconManager.GetImage(MyGlobal.IconLibrary, "VB.Net 16x16.ico");
            mnuSql2Vb6.Image = IconManager.GetImage(MyGlobal.IconLibrary, "VB 16x16.ico");
            mnuSql2Delphi6.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Delphi 16x16.ico");

            InitializeSqlToCodeStyleMenuItems();

            //20241225 將第三層項目加入到第二層
            mnuSql2CSharp.DropDownItems.Add(_menuSql2CSharp1);
            mnuSql2CSharp.DropDownItems.Add(_menuSql2CSharp2);
            mnuSql2CSharp.DropDownItems.Add(_menuSql2CSharp3);
            mnuSql2CSharp.DropDownItems.Add(_menuSql2CSharp4);

            mnuSql2VbNet.DropDownItems.Add(_menuSql2VbNet1);
            mnuSql2VbNet.DropDownItems.Add(_menuSql2VbNet2);
            mnuSql2VbNet.DropDownItems.Add(_menuSql2VbNet3);

            mnuSql2Vb6.DropDownItems.Add(_menuSql2Vb61);
            mnuSql2Vb6.DropDownItems.Add(_menuSql2Vb62);

            mnuSql2Delphi6.DropDownItems.Add(_menuSql2Delphi61);
            mnuSql2Delphi6.DropDownItems.Add(_menuSql2Delphi62);

            //20241225 將第二層項目加入到第一層
            var sqlToCodeMenu = GetEditorMenuItem(QueryEditorColumn.SQL2Code);

            sqlToCodeMenu.DropDownItems.Add(mnuSql2CSharp);
            sqlToCodeMenu.DropDownItems.Add(mnuSql2VbNet);
            sqlToCodeMenu.DropDownItems.Add(mnuSql2Vb6);
            sqlToCodeMenu.DropDownItems.Add(mnuSql2Delphi6);

            BindSqlToCodeStyleMenuEvents();
        }

        private void InitializeSqlToCodeStyleMenuItems()
        {
            //20241225 建立第三層選單項目 樣式1~3
            _menuSql2CSharp1 = new ToolStripMenuItem(GetEditorObjectText("Style 1: Using + operator", "mnuCSharpStyle1"));
            _menuSql2CSharp2 = new ToolStripMenuItem(GetEditorObjectText("Style 2: New line character \r\n", "mnuCSharpStyle2"));
            _menuSql2CSharp3 = new ToolStripMenuItem(GetEditorObjectText("Style 3: New line character Enviroment.NewLine", "mnuCSharpStyle3"));
            _menuSql2CSharp4 = new ToolStripMenuItem(GetEditorObjectText("Style 4: StringBuilder", "mnuCSharpStyle4"));

            _menuSql2VbNet1 = new ToolStripMenuItem(GetEditorObjectText("Style 1: Using && operator", "mnuVBNetStyle1"));
            _menuSql2VbNet2 = new ToolStripMenuItem(GetEditorObjectText("Style 2: New line character VbCrLf", "mnuVBNetStyle2"));
            _menuSql2VbNet3 = new ToolStripMenuItem(GetEditorObjectText("Style 3: New line character Enviroment.NewLine", "mnuVBNetStyle3"));

            _menuSql2Vb61 = new ToolStripMenuItem(GetEditorObjectText("Style 1: Using && operator", "mnuVB6AStyle1"));
            _menuSql2Vb62 = new ToolStripMenuItem(GetEditorObjectText("Style 2: New line character VbCrLf", "mnuVB6AStyle2"));

            _menuSql2Delphi61 = new ToolStripMenuItem(GetEditorObjectText("Style 1: Using + operator", "mnuDelphi6Style1"));
            _menuSql2Delphi62 = new ToolStripMenuItem(GetEditorObjectText("Style 2: New line character #13#10", "mnuDelphi6Style2"));
        }

        private void BindSqlToCodeStyleMenuEvents()
        {
            BindPerformClick(_menuSql2CSharp1, mnuCSharpStyle1);
            BindPerformClick(_menuSql2CSharp2, mnuCSharpStyle2);
            BindPerformClick(_menuSql2CSharp3, mnuCSharpStyle3);
            BindPerformClick(_menuSql2CSharp4, mnuCSharpStyle4);

            BindPerformClick(_menuSql2VbNet1, mnuVBNetStyle1);
            BindPerformClick(_menuSql2VbNet2, mnuVBNetStyle2);
            BindPerformClick(_menuSql2VbNet3, mnuVBNetStyle3);

            BindPerformClick(_menuSql2Vb61, mnuVB6AStyle1);
            BindPerformClick(_menuSql2Vb62, mnuVB6AStyle2);

            BindPerformClick(_menuSql2Delphi61, mnuDelphi6Style1);
            BindPerformClick(_menuSql2Delphi62, mnuDelphi6Style2);
        }

        private void BindEditorFormatMenuEvents()
        {
            BindEditorMenuClick(QueryEditorColumn.Comment, AddComment);
            BindEditorMenuClick(QueryEditorColumn.RemoveComment, RemoveComment);
            BindEditorMenuClick(QueryEditorColumn.Indent, Indent);
            BindEditorMenuClick(QueryEditorColumn.Unindent, Unindent);
            BindEditorMenuClick(QueryEditorColumn.UpperCase, () => ConvertSelectionCase(true));
            BindEditorMenuClick(QueryEditorColumn.LowerCase, () => ConvertSelectionCase(false));
            BindEditorMenuClick(QueryEditorColumn.SQLFormatter, ConvertSQLFormatter);
        }
    }
}
