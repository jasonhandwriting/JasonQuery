using IconLibrary;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private static class QueryEditorContextMenuBuilder
        {
            private sealed class EditorMenuDefinition
            {
                public int Index { get; set; }
                public string Text { get; set; } = string.Empty;
                public string IconFileName { get; set; } = string.Empty;
                public string ShortcutKeyDisplayString { get; set; } = string.Empty;
                public bool StoreBaseTextToTag { get; set; }
                public bool IsSeparator => string.Equals(Text, "-", StringComparison.Ordinal);
            }

            public static ContextMenuStrip Build(string ownerFormName)
            {
                var menu = new ContextMenuStrip();

                foreach (var definition in CreateEditorMenuDefinitions(ownerFormName))
                {
                    AddEditorMenuItem(menu, definition);
                }

                return menu;
            }

            /// <summary>
            /// 20260428 未來要新增一個右鍵選單項目，優先檢查三個地方：
            /// 1. MyColumnQueryEditor 新增 index
            /// 2. CreateEditorMenuDefinitions() 新增 definition
            /// 3. BindEditorMenuEvents() 補事件
            /// </summary>
            /// <returns></returns>
            private static List<EditorMenuDefinition> CreateEditorMenuDefinitions(string ownerFormName)
            {
                var schemaBrowserText = LocalizationHelper.GetLanguageString("Schema Browser", "form", "SchemaBrowserForm", "object", "this", "Text");
                var showCreateScriptText = LocalizationHelper.GetLanguageString("Show DDL Statement", "Global", "Global", "msg", "ShowCreateScript", "Text");
                var generateSqlText = LocalizationHelper.GetLanguageString("Generate SQL Statement", "form", "GenerateSqlForm", "object", "this", "Text");

                return new List<EditorMenuDefinition>
                {
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Execute,
                        Text = GetEditorMenuText(ownerFormName, "Execute", "Execute"),
                        IconFileName = "Execute 16x16.ico",
                        ShortcutKeyDisplayString = GetEditorShortcutDisplayText(ownerFormName, "F5", "Execute")
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.ExecuteCurrentBlock,
                        Text = GetEditorMenuText(ownerFormName, "Execute Current Block", "ExecuteCurrentBlock"),
                        IconFileName = "Execute Current Block 16x16.ico",
                        ShortcutKeyDisplayString = GetEditorShortcutDisplayText(ownerFormName, "Ctrl+Enter", "ExecuteCurrentBlock")
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.ExecuteCurrentLine,
                        Text = GetEditorMenuText(ownerFormName, "Execute Current Line", "ExecuteCurrentLine"),
                        IconFileName = "Execute Current Line 16x16.ico",
                        ShortcutKeyDisplayString = GetEditorShortcutDisplayText(ownerFormName, "Shift+Enter", "ExecuteCurrentLine")
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash2,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.SchemaBrowser,
                        Text = schemaBrowserText,
                        IconFileName = "Schema Browser 16x16.ico"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.ShowCreateScript,
                        Text = showCreateScriptText,
                        IconFileName = "Show SQL Statement 16x16.ico",
                        StoreBaseTextToTag = true
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.SchemaBrowserTable,
                        Text = schemaBrowserText,
                        IconFileName = "Schema Browser 16x16.ico",
                        StoreBaseTextToTag = true
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.SchemaBrowserView,
                        Text = schemaBrowserText,
                        IconFileName = "Schema Browser 16x16.ico",
                        StoreBaseTextToTag = true
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.GenerateSql,
                        Text = generateSqlText,
                        IconFileName = "Generate SQL Statement 16x16.ico",
                        StoreBaseTextToTag = true
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash3,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.TableComment,
                        Text = LocalizationHelper.GetLanguageString("Comment Table", "form", "EditTableForm", "object", "CommentTable", "Text"),
                        IconFileName = "Table Comment.ico",
                        StoreBaseTextToTag = true
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.TableDrop,
                        Text = LocalizationHelper.GetLanguageString("Drop Table", "form", "EditTableForm", "object", "DropTable", "Text"),
                        IconFileName = "Table Drop.ico",
                        StoreBaseTextToTag = true
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.TableRename,
                        Text = LocalizationHelper.GetLanguageString("Rename Table", "form", "EditTableForm", "object", "RenameTable", "Text"),
                        IconFileName = "Table Rename.ico",
                        StoreBaseTextToTag = true
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.TableTruncate,
                        Text = LocalizationHelper.GetLanguageString("Truncate Table", "form", "EditTableForm", "object", "TruncateTable", "Text"),
                        IconFileName = "Table Truncate.ico",
                        StoreBaseTextToTag = true
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash4,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Undo,
                        Text = GetEditorMenuText(ownerFormName, "Undo", "Undo"),
                        IconFileName = "Undo 16x16.ico",
                        ShortcutKeyDisplayString = GetMainMenuShortcutDisplayText("Ctrl+Z", "mnuEdit_Undo")
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Redo,
                        Text = GetEditorMenuText(ownerFormName, "Redo", "Redo"),
                        IconFileName = "Redo 16x16.ico",
                        ShortcutKeyDisplayString = GetMainMenuShortcutDisplayText("Ctrl+Y", "mnuEdit_Redo")
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash5,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Cut,
                        Text = GetEditorMenuText(ownerFormName, "Cut", "Cut"),
                        IconFileName = "Cut 16x16.ico",
                        ShortcutKeyDisplayString = GetMainMenuShortcutDisplayText("Ctrl+X", "mnuEdit_Cut")
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Copy,
                        Text = GetEditorMenuText(ownerFormName, "Copy", "Copy"),
                        IconFileName = "Copy 16x16.ico",
                        ShortcutKeyDisplayString = GetMainMenuShortcutDisplayText("Ctrl+C", "mnuEdit_Copy")
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.CopyTo,
                        Text = GetEditorMenuText(ownerFormName, "CopyTo...", "CopyTo")
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Paste,
                        Text = GetEditorMenuText(ownerFormName, "Paste", "Paste"),
                        IconFileName = "Paste 16x16.ico",
                        ShortcutKeyDisplayString = GetMainMenuShortcutDisplayText("Ctrl+V", "mnuEdit_Paste")
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Delete,
                        Text = GetEditorMenuText(ownerFormName, "Delete", "Delete"),
                        IconFileName = "Delete 16x16.ico",
                        ShortcutKeyDisplayString = "Del"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.DeleteCurrentLine,
                        Text = GetEditorMenuText(ownerFormName, "Delete Current Line", "DeleteCurrentLine"),
                        IconFileName = "DeleteCurrentRow 16x16.ico",
                        ShortcutKeyDisplayString = "Ctrl+L"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash10,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.ZoomIn,
                        Text = GetEditorMenuText(ownerFormName, "Zoom In (Ctrl+Mouse Wheel Up)", "ZoomIn"),
                        IconFileName = "Zoom In 16x16.ico"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.ZoomOut,
                        Text = GetEditorMenuText(ownerFormName, "Zoom Out (Ctrl+Mouse Wheel Down)", "ZoomOut"),
                        IconFileName = "Zoom Out 16x16.ico"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash11,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.FindAndReplace,
                        Text = GetEditorMenuText(ownerFormName, "Find And Replace", "FindAndReplace"),
                        IconFileName = "Replace 16x16.ico",
                        ShortcutKeyDisplayString = "Ctrl+F"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash12,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.SelectAll,
                        Text = GetEditorMenuText(ownerFormName, "Select All", "SelectAll"),
                        IconFileName = "Select All 16x16.ico",
                        ShortcutKeyDisplayString = "Ctrl+A"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.SelectCurrentBlock,
                        Text = GetEditorMenuText(ownerFormName, "Select Current Block", "SelectCurrentBlock"),
                        IconFileName = "Select Current Block 16x16.ico",
                        ShortcutKeyDisplayString = "Ctrl+B"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.SelectCurrentLine,
                        Text = GetEditorMenuText(ownerFormName, "Select Current Line", "SelectCurrentLine"),
                        IconFileName = "Select Current Line 16x16.ico"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash13,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Code2SQL,
                        Text = GetEditorMenuText(ownerFormName, "Code to SQL", "Code2SQL"),
                        IconFileName = "Code2SQL.ico"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.SQL2Code,
                        Text = GetEditorMenuText(ownerFormName, "SQL to Code", "SQL2Code"),
                        IconFileName = "SQL2Code.ico"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash14,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Comment,
                        Text = GetEditorMenuText(ownerFormName, "Comment", "Comment"),
                        IconFileName = MyLibrary.IsDarkMode ? "Comment_b 16x16.ico" : "Comment 16x16.ico"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.RemoveComment,
                        Text = GetEditorMenuText(ownerFormName, "Un-Comment", "Uncomment"),
                        IconFileName = MyLibrary.IsDarkMode ? "Uncomment_b 16x16.ico" : "Uncomment 16x16.ico"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash17,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Indent,
                        Text = GetEditorMenuText(ownerFormName, "Indent", "Indent"),
                        IconFileName = "Indent 16x16.ico",
                        ShortcutKeyDisplayString = "Tab"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Unindent,
                        Text = GetEditorMenuText(ownerFormName, "Un-Indent", "Unindent"),
                        IconFileName = "Unindent 16x16.ico",
                        ShortcutKeyDisplayString = "Shift+Tab"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash20,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.UpperCase,
                        Text = GetEditorMenuText(ownerFormName, "UPPER Case", "UpperCase"),
                        IconFileName = "UpperCase 16x16.ico",
                        ShortcutKeyDisplayString = "Ctrl+Shift+U"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.LowerCase,
                        Text = GetEditorMenuText(ownerFormName, "lower Case", "LowerCase"),
                        IconFileName = "LowerCase 16x16.ico",
                        ShortcutKeyDisplayString = "Ctrl+U"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.Dash23,
                        Text = "-"
                    },
                    new EditorMenuDefinition
                    {
                        Index = QueryEditorColumn.SQLFormatter,
                        Text = GetEditorMenuText(ownerFormName, "SQL Formatter", "SQLFormatter"),
                        IconFileName = "SQL Format 16x16.ico"
                    }
                };
            }

            private static void AddEditorMenuItem(ContextMenuStrip menu, EditorMenuDefinition definition)
            {
                if (menu == null)
                {
                    throw new ArgumentNullException(nameof(menu));
                }

                if (definition == null)
                {
                    throw new ArgumentNullException(nameof(definition));
                }

                if (menu.Items.Count != definition.Index)
                {
                    throw new InvalidOperationException
                    (
                        $"Editor context menu item index mismatch. Expected index: {definition.Index}, Actual count: {menu.Items.Count}."
                    );
                }

                var item = CreateEditorMenuItem(definition);

                menu.Items.Add(item);
            }

            private static ToolStripItem CreateEditorMenuItem(EditorMenuDefinition definition)
            {
                if (definition.IsSeparator)
                {
                    return new ToolStripSeparator();
                }

                var item = new ToolStripMenuItem(definition.Text);

                if (!string.IsNullOrEmpty(definition.IconFileName))
                {
                    item.Image = IconManager.GetImage(MyGlobal.IconLibrary, definition.IconFileName);
                }

                if (!string.IsNullOrEmpty(definition.ShortcutKeyDisplayString))
                {
                    item.ShortcutKeyDisplayString = definition.ShortcutKeyDisplayString;
                }

                if (definition.StoreBaseTextToTag)
                {
                    item.Tag = definition.Text;
                }

                return item;
            }

            private static string GetEditorMenuText(string ownerFormName, string defaultText, string key)
            {
                return LocalizationHelper.GetLanguageString(defaultText, "form", ownerFormName, "menueditor", key, "Text");
            }

            private static string GetEditorShortcutDisplayText(string ownerFormName, string defaultText, string key)
            {
                return LocalizationHelper.GetLanguageString
                (
                    defaultText,
                    "form",
                    ownerFormName,
                    "menueditor",
                    key,
                    "ShortcutKeyDisplayString"
                );
            }

            private static string GetMainMenuShortcutDisplayText(string defaultText, string key)
            {
                return LocalizationHelper.GetLanguageString
                (
                    defaultText,
                    "form",
                    "MainForm",
                    "menu",
                    key,
                    "ShortcutKeyDisplayString"
                );
            }
        }
    }
}
