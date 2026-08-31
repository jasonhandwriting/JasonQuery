using C1.Win.C1TrueDBGrid;
using IconLibrary;
using C1.Win.C1Command;
using C1.Win.C1Input;
using JasonLibrary.Core;
using JasonLibrary.UI.Controls;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Database.CreateScript.PostgreSql;

namespace JasonQuery.UI.Helpers
{
    public static class UIHelper
    {
        public static void SetC1ComboBoxItemsFromDictionary(C1ComboBox cBox, Dictionary<string, string> dic, bool bKey = false)
        {
            cBox.Items.Clear();

            foreach (var OneItem in dic)
            {
                cBox.Items.Add(bKey ? OneItem.Key : OneItem.Value);
            }
        }

        public static void SetC1ComboBoxItemsFromDataTable(C1ComboBox cBox, DataTable dt, int iColIndex = 0)
        {
            cBox.Items.Clear();

            if (dt == null)
            {
                return;
            }

            foreach (DataRow row in dt.Rows)
            {
                cBox.Items.Add(row.Field<string>(iColIndex));
            }
        }

        public static void SetC1TextBoxText(C1TextBox c1Text, string sText)
        {
            var bVisible = c1Text.Visible;

            try
            {
                c1Text.Visible = true;
                c1Text.Text = sText;
                c1Text.Visible = bVisible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(text: $"SetC1TextBoxText\r\n\r\n{ex.Message}");
            }
        }

        public static void SetC1ComboboxTextIgnoreVisible(C1ComboBox c1Text, string sText)
        {
            var bEnable = c1Text.Enabled;
            var bVisible = c1Text.Visible;
            var dStyle = c1Text.DropDownStyle;

            try
            {
                c1Text.Visible = true;
                c1Text.Enabled = true;

                try
                {
                    c1Text.DropDownStyle = DropDownStyle.Default;
                    c1Text.Text = sText;
                }
                catch (Exception)
                {
                    c1Text.DropDownStyle = DropDownStyle.DropDownList;
                    c1Text.Text = sText;
                }

                c1Text.DropDownStyle = dStyle;
                c1Text.Enabled = bEnable;
                c1Text.Visible = bVisible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(text: $"SetC1ComboboxText\r\n\r\n{ex.Message}");
            }
        }

        public static void SelectC1ComboBoxItemByText(C1ComboBox cBox, string text)
        {
            if (cBox == null)
            {
                return;
            }

            var index = cBox.Items.IndexOf(text);

            if (index >= 0)
            {
                cBox.SelectedIndex = index;
            }
        }

        public static void SetDockingTabColor(C1DockingTab c1Tab, Color cBackColorSelected, Color cForeColorSelected, Color cForeColor)
        {
            foreach (Control tab in c1Tab.Controls)
            {
                var tabPage = (C1DockingTabPage)tab;

                tabPage.TabBackColorSelected = cBackColorSelected;
                tabPage.TabForeColorSelected = cForeColorSelected;
                tabPage.TabForeColor = cForeColor;
            }
        }

        /// <summary>
        /// 依據傳入的屬性名稱與預設值，從 SystemConfig 讀取寬、高設定，找不到則回傳預設值。
        /// </summary>
        /// <param name="domainUser">DomainUser</param>
        /// <param name="widthName">AttributeName 中代表「寬」的名稱</param>
        /// <param name="heightName">AttributeName 中代表「高」的名稱</param>
        /// <param name="defaultWidth">找不到寬時的預設值</param>
        /// <param name="defaultHeight">找不到高時的預設值</param>
        /// <returns>Tuple：Item1 = 寬, Item2 = 高</returns>
        public static (int Width, int Height) GetFormWidthHeightSettings(string domainUser, string widthName, string heightName, int defaultWidth, int defaultHeight)
        {
            int width2 = defaultWidth; //預設值
            int height2 = defaultHeight; //預設值
            var sbSql = new StringBuilder();

            //取得指定表單的寬、高數值
            sbSql.AppendLine("SELECT AttributeName, AttributeValue");
            sbSql.AppendLine("  FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{domainUser}'");
            sbSql.AppendLine("   AND AttributeKey = 'GlobalConfig'");
            sbSql.Append($"   AND (AttributeName = '{widthName}' OR AttributeName = '{heightName}')");

            var sql = sbSql.ToString();
            var dtTemp = JasonQueryRepository.ExecQuery(sql);

            if (dtTemp?.Rows.Count > 0)
            {
                foreach (DataRow dr in dtTemp?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var attributeName = dr.GetSafeString("AttributeName");
                    var attributeValue = dr.GetSafeString("AttributeValue");

                    if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(attributeValue))
                    {
                        continue;
                    }

                    if (!int.TryParse(attributeValue, out int parsed))
                    {
                        continue;
                    }

                    if (attributeName == widthName)
                    {
                        width2 = parsed;
                    }
                    else if (attributeName == heightName)
                    {
                        height2 = parsed;
                    }
                }
            }

            return (width2, height2);
        }

        public static void GenerateRightMenuForCopyOnly(bool isFromSchemaBrowser, ContextMenuStrip schemaBrowserContextMenu, C1TrueDBGrid c1Grid, ScintillaEditor editor, string title1, string title2, string schemaName, int x, int y, bool isExtraPaste = true, string schemaNode = "", string tableName = "", string columnType = "", string databaseName = "", string schemaType = "", string packageSpecBody = "", string schemaDbo = "")
        {
            var i = 0;
            var languageText = string.Empty;

            schemaBrowserContextMenu.Items.Add(schemaName);
            schemaBrowserContextMenu.Items[i].Enabled = false;

            i++;
            schemaBrowserContextMenu.Items.Add("-");

            if (!isExtraPaste)
            {
                if (!isFromSchemaBrowser)
                {
                    //20250917 加上 Show Create Script 功能表
                    languageText = LocalizationHelper.GetLanguageString("Show DDL Statement", "Global", "Global", "msg", "ShowDDL", "Text");
                    schemaBrowserContextMenu.Items.Add(languageText);

                    i++;

                    var currentItem = schemaBrowserContextMenu.Items[i];

                    currentItem.AccessibleDescription = $"{schemaType};{schemaName};{schemaNode};{schemaDbo};{packageSpecBody}";

                    currentItem.Click += delegate (object sender, EventArgs e)
                    {
                        var item = (ToolStripItem)sender;
                        var parts = item.AccessibleDescription.Split(new[] { ";" }, StringSplitOptions.None);

                        ShowDDL(parts[0], parts[1], parts[2], parts[3], parts[4]);
                    };

                    schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");

                    i++;
                    schemaBrowserContextMenu.Items.Add("-");
                }
            }
            else
            {
                var temp01 = LocalizationHelper.GetLanguageString("Add Column...", "form", "EditColumnForm", "object", "AddColumn", "Text");

                //20241116 針對欄位，新增 Comment/Drop/Rename 功能表
                languageText = $"{temp01}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditColumnForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Column Add.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = tableName;
                        form.ColumnName = schemaName;
                        form.ColumnType = columnType;
                        form.SchemaDatabase = databaseName;
                        form.EditMode = EditColumnMode.Add;

                        ApplyEditColumnFormSize(form);
                        form.ShowDialog();
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Add.ico");

                temp01 = LocalizationHelper.GetLanguageString("Comment Column...", "form", "EditColumnForm", "object", "CommentColumn", "Text");
                languageText = $"{temp01}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditColumnForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Column Comment.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = tableName;
                        form.ColumnName = schemaName;
                        form.ColumnType = columnType;
                        form.SchemaDatabase = databaseName;
                        form.EditMode = EditColumnMode.Comment;

                        ApplyEditColumnFormSize(form);
                        form.ShowDialog();
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Comment.ico");

                temp01 = LocalizationHelper.GetLanguageString("Drop Column...", "form", "EditColumnForm", "object", "DropColumn", "Text");
                languageText = $"{temp01}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditColumnForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Column Drop.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = tableName;
                        form.ColumnName = schemaName;
                        form.ColumnType = columnType;
                        form.SchemaDatabase = databaseName;
                        form.EditMode = EditColumnMode.Drop;

                        ApplyEditColumnFormSize(form);
                        form.ShowDialog();

                        //20250204 更新 Schema 訊息
                        if (form.Result != null && form.Result == "REFRESH")
                        {
                            MyGlobal.GlobalTemp4 = "REFRESH";
                            MyGlobal.GlobalTemp5 = "UpdateSchemaInformation`";
                        }
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Drop.ico");

                temp01 = LocalizationHelper.GetLanguageString("Rename Column...", "form", "EditColumnForm", "object", "RenameColumn", "Text");
                languageText = $"{temp01}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditColumnForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Column Rename.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = tableName;
                        form.ColumnName = schemaName;
                        form.ColumnType = columnType;
                        form.SchemaDatabase = databaseName;
                        form.EditMode = EditColumnMode.Rename;

                        ApplyEditColumnFormSize(form);
                        form.ShowDialog();

                        //20250204 更新 Schema 訊息
                        if (form.Result != null && form.Result == "REFRESH")
                        {
                            MyGlobal.GlobalTemp4 = "REFRESH";
                            MyGlobal.GlobalTemp5 = "UpdateSchemaInformation`";
                        }
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Rename.ico");

                i++;
                schemaBrowserContextMenu.Items.Add("-");
            }

            schemaBrowserContextMenu.Items.Add(title1); //Copy to Clipboard

            i++;

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                Clipboard.SetDataObject(schemaName, false);
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.C;

            schemaBrowserContextMenu.Items.Add(title2); //Paste to Query Editor

            i++;

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}";
                }
                else
                {
                    Clipboard.SetDataObject(schemaName, false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.V;

            if (isExtraPaste)
            {
                var sText = LocalizationHelper.GetLanguageString("(Double-Click to quickly paste)", "form", "QueryForm", "msg", "DoubleClickPaste", "Text");

                sText = isFromSchemaBrowser ? string.Empty : $"  {sText}";
                schemaBrowserContextMenu.Items.Add($"{title2} + \", \"{sText}"); //Paste to Query Editor + ", "

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    if (isFromSchemaBrowser)
                    {
                        MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}, ";
                    }
                    else
                    {
                        Clipboard.SetDataObject($"{schemaName}, ", false);
                        editor.Paste();

                        if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                        {
                            editor.Focus();
                        }
                    }
                };

                ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;

                schemaBrowserContextMenu.Items.Add($"{title2} + \", \\r\\n\""); //Paste to Query Editor + ", \r\n"

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    if (isFromSchemaBrowser)
                    {
                        MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}, \r\n";
                    }
                    else
                    {
                        Clipboard.SetDataObject($"{schemaName}, \r\n", false);
                        editor.Paste();

                        if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                        {
                            editor.Focus();
                        }
                    }
                };

                ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Alt | Keys.Shift | Keys.V;

                schemaBrowserContextMenu.Items.Add($"{title2} + \"\\r\\n\""); //Paste to Query Editor + "\r\n"

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    if (isFromSchemaBrowser)
                    {
                        MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}\r\n";
                    }
                    else
                    {
                        Clipboard.SetDataObject($"{schemaName}\r\n", false);
                        editor.Paste();

                        if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                        {
                            editor.Focus();
                        }
                    }
                };

                ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.Shift | Keys.V;
            }

            c1Grid.ContextMenuStrip = schemaBrowserContextMenu;

            if (MyLibrary.IsDarkMode)
            {
                schemaBrowserContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                schemaBrowserContextMenu.ForeColor = Color.White;
                schemaBrowserContextMenu.RenderMode = ToolStripRenderMode.System;
                //cMenuSchemaBrowser.ShowImageMargin = false;
            }

            schemaBrowserContextMenu.Show(c1Grid, new Point(x, y));
        }

        public static void ShowDDL(string schemaType, string schemaName, string schemaNode, string schemaDbo, string packageSpecBody = "", string subTitle = "")
        {
            try
            {
                var text = subTitle != string.Empty ? subTitle : LocalizationHelper.GetLanguageString("DDL Statement Viewer", "Global", "Global", "msg", "DDLViewer", "Text");

                using (var form = new SqlStatementViewerForm())
                {
                    form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");
                    form.Title = text;
                    form.SchemaType = schemaType;
                    form.SchemaName = schemaName;

                    switch (DatabaseSqlExecutor.CurrentDataSource)
                    {
                        case DataSourceType.Oracle:
                            {
                                form.CellText = DatabaseSqlExecutor.GetCreateScript_Oracle(schemaType, schemaName.ToUpper(), packageSpecBody);
                                break;
                            }
                        case DataSourceType.PostgreSql:
                            {
                                var script = DatabaseSqlExecutor.GetCreateScript_PostgreSql(schemaNode, schemaType, schemaName);

                                if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
                                {
                                    form.CellText = PostgreSqlCreateTableScriptBeautifier.Beautify(script);
                                }
                                else
                                {
                                    form.CellText = script;
                                }

                                break;
                            }
                        case DataSourceType.SqlServer:
                            {
                                form.CellText = DatabaseSqlExecutor.GetCreateScript_SqlServer(schemaType, schemaNode, schemaDbo, schemaName);
                                break;
                            }
                        case DataSourceType.MySql:
                            {
                                form.CellText = DatabaseSqlExecutor.GetCreateScript_MySql(schemaType, schemaNode, schemaName);
                                break;
                            }
                    }

                    //20250426 改寫 Width/Height 取值方法
                    var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "SQLStatementViewerFormWidth", "SQLStatementViewerFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                    form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public static void GenerateRightMenuForCopyOnly_SchemaBrowser(ContextMenuStrip cMenuSchemaBrowser, C1TrueDBGrid c1Grid, string sTitle1, string sTitle2, string sName, int iX, int iY)
        {
            var i = 0;

            cMenuSchemaBrowser.Items.Add(sName);
            cMenuSchemaBrowser.Items[i].Enabled = false;

            i++;
            cMenuSchemaBrowser.Items.Add("-");

            cMenuSchemaBrowser.Items.Add(sTitle1); //Copy to Clipboard

            i++;

            cMenuSchemaBrowser.Items[i].Click += delegate
            {
                Clipboard.SetDataObject(sName, false);
            };

            cMenuSchemaBrowser.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");

            cMenuSchemaBrowser.Items.Add($"{sTitle2} + \"\\r\\n\""); //Paste to Query Editor + "\r\n"

            i++;

            cMenuSchemaBrowser.Items[i].Click += delegate
            {
                Clipboard.SetDataObject($"{sName}\r\n", false);
                MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{sName}";
            };

            cMenuSchemaBrowser.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor 16x16.ico");
            ((ToolStripMenuItem)cMenuSchemaBrowser.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.Shift | Keys.V;

            c1Grid.ContextMenuStrip = cMenuSchemaBrowser;

            if (MyLibrary.IsDarkMode)
            {
                cMenuSchemaBrowser.BackColor = ColorTranslator.FromHtml("#2D2D30");
                cMenuSchemaBrowser.ForeColor = Color.White;
                cMenuSchemaBrowser.RenderMode = ToolStripRenderMode.System;
            }

            cMenuSchemaBrowser.Show(c1Grid, new Point(iX, iY));
        }

        public static void GenerateRightMenuForCopy_Oracle(bool isFromSchemaBrowser, C1TrueDBGrid c1GridSchemaBrowser, ContextMenuStrip schemaBrowserContextMenu, ScintillaEditor editor, string accessibleDescription, string schemaType, string title1, string title2, string schemaName, int x, int y, string packageSpecBody = "")
        {
            var i = 0;
            var languageText = string.Empty;

            schemaBrowserContextMenu.Items.Add(schemaName);
            schemaBrowserContextMenu.Items[i].Enabled = false;

            i++;
            schemaBrowserContextMenu.Items.Add("-");

            if (!isFromSchemaBrowser)
            {
                //20250917 加上 Show Create Script 功能表
                languageText = LocalizationHelper.GetLanguageString("Show DDL Statement", "Global", "Global", "msg", "ShowDDL", "Text");
                schemaBrowserContextMenu.Items.Add(languageText);

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    try
                    {
                        using (var form = new SqlStatementViewerForm())
                        {
                            form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");
                            form.Title = LocalizationHelper.GetLanguageString("DDL Statement Viewer", "Global", "Global", "msg", "DDLViewer", "Text");
                            form.SchemaType = schemaType;
                            form.SchemaName = schemaName;
                            form.CellText = DatabaseSqlExecutor.GetCreateScript_Oracle(schemaType, schemaName.ToUpper(), "sPackageSpecBody");

                            var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "SQLStatementViewerFormWidth", "SQLStatementViewerFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                            form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                            form.ShowDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");

                i++;
                schemaBrowserContextMenu.Items.Add("-");
            }

            languageText = LocalizationHelper.GetLanguageString("Schema Browser", "form", "SchemaBrowserForm", "object", "this", "Text");
            languageText = $"{languageText}...";

            i++;
            schemaBrowserContextMenu.Items.Add(languageText);
            schemaBrowserContextMenu.Items[i].Enabled = !isFromSchemaBrowser;

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                //從 Query Editor 過來的！
                if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables) || SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Views))
                {
                    MyGlobal.DisplaySchemaBrowser(string.Empty, schemaType, schemaName);
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Schema Browser 16x16.ico");

            i++;
            languageText = LocalizationHelper.GetLanguageString("Generate SQL Statement", "form", "GenerateSqlForm", "object", "this", "Text");
            languageText = $"{languageText}...";
            schemaBrowserContextMenu.Items.Add(languageText);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                using (var form = new GenerateSqlForm())
                {
                    form.SchemaType = schemaType;
                    form.SchemaName = schemaName;
                    form.AccessibleDescriptionString = accessibleDescription;

                    //20250426 改寫 Width/Height 取值方法
                    var (formWidth, formHeight) = GetFormWidthHeightSettings(MyGlobal.DomainUser, "GenerateSqlFormWidth", "GenerateSqlFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                    form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                    form.ShowDialog();
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Generate SQL Statement 16x16.ico");

            //20241103 針對 Table，新增 Comment/Drop/Rename/Truncate 右鍵選單
            #region
            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
            {
                i++;
                schemaBrowserContextMenu.Items.Add("-");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Comment Table", "form", "EditTableForm", "object", "CommentTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Comment.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = DatabaseSqlExecutor.DbUser;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Comment;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Comment.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Drop Table", "form", "EditTableForm", "object", "DropTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Drop.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = DatabaseSqlExecutor.DbUser;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Drop;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();

                        //20250204 更新 Schema 訊息
                        if (form.Result != null && form.Result == "REFRESH")
                        {
                            MyGlobal.GlobalTemp4 = "REFRESH";
                            MyGlobal.GlobalTemp5 = "UpdateSchemaInformation`";
                        }
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Drop.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Rename Table", "form", "EditTableForm", "object", "RenameTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Rename.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = DatabaseSqlExecutor.DbUser;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Rename;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();

                        //20250204 更新 Schema 訊息
                        if (form.Result != null && form.Result == "REFRESH")
                        {
                            MyGlobal.GlobalTemp4 = "REFRESH";
                            MyGlobal.GlobalTemp5 = "UpdateSchemaInformation`";
                        }
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Rename.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Truncate Table", "form", "EditTableForm", "object", "TruncateTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Truncate.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = DatabaseSqlExecutor.DbUser;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Truncate;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Truncate.ico");
            }
            #endregion

            i++;
            schemaBrowserContextMenu.Items.Add("-");

            i++;
            schemaBrowserContextMenu.Items.Add(title1);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                Clipboard.SetDataObject(schemaName, false);
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.C;

            i++;

            //20230909 新增 select * from XXX
            schemaBrowserContextMenu.Items.Add($"{title2} SELECT * FROM {schemaName}");

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`SELECT * FROM {schemaName}";
                }
                else
                {
                    Clipboard.SetDataObject($"SELECT * FROM {schemaName}", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add(title2);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}";
                }
                else
                {
                    Clipboard.SetDataObject(schemaName, false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.V;

            if (1 == 1)
            {
                i++;
                schemaBrowserContextMenu.Items.Add($"{title2} + \", \""); //Paste to Query Editor + ", "

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    if (isFromSchemaBrowser)
                    {
                        MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}, ";
                    }
                    else
                    {
                        Clipboard.SetDataObject($"{schemaName}, ", false);
                        editor.Paste();

                        if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                        {
                            editor.Focus();
                        }
                    }
                };

                ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;

                i++;
                schemaBrowserContextMenu.Items.Add($"{title2} + \", \\r\\n\""); //Paste to Query Editor + ", \r\n"

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    if (isFromSchemaBrowser)
                    {
                        MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}, \r\n";
                    }
                    else
                    {
                        Clipboard.SetDataObject($"{schemaName}, \r\n", false);
                        editor.Paste();

                        if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                        {
                            editor.Focus();
                        }
                    }
                };

                ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Alt | Keys.Shift | Keys.V;

                i++;
                schemaBrowserContextMenu.Items.Add($"{title2} + \"\\r\\n\""); //Paste to Query Editor + "\r\n"

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    if (isFromSchemaBrowser)
                    {
                        MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}\r\n";
                    }
                    else
                    {
                        Clipboard.SetDataObject($"{schemaName}\r\n", false);
                        editor.Paste();

                        if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                        {
                            editor.Focus();
                        }
                    }
                };

                ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.Shift | Keys.V;
            }

            c1GridSchemaBrowser.ContextMenuStrip = schemaBrowserContextMenu;

            if (MyLibrary.IsDarkMode)
            {
                schemaBrowserContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                schemaBrowserContextMenu.ForeColor = Color.White;
                schemaBrowserContextMenu.RenderMode = ToolStripRenderMode.System;
                //cMenuSchemaBrowser.ShowImageMargin = false;
            }

            schemaBrowserContextMenu.Show(c1GridSchemaBrowser, new Point(x, y));
        }

        public static void GenerateRightMenuForCopy_PostgreSql(bool isFromSchemaBrowser, C1TrueDBGrid c1GridSchemaBrowser, ContextMenuStrip schemaBrowserContextMenu, ScintillaEditor editor, string accessibleDescription, string schemaNode, string schemaType, string title1, string title2, string schemaName, int x, int y)
        {
            var i = 0;
            var languageText = string.Empty;

            schemaBrowserContextMenu.Items.Add($"{schemaNode}.{schemaName}");
            schemaBrowserContextMenu.Items[i].Enabled = false;

            i++;
            schemaBrowserContextMenu.Items.Add("-");

            if (!isFromSchemaBrowser)
            {
                //20250917 加上 Show Create Script 功能表
                languageText = LocalizationHelper.GetLanguageString("Show DDL Statement", "Global", "Global", "msg", "ShowDDL", "Text");
                schemaBrowserContextMenu.Items.Add(languageText);

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    try
                    {
                        using (var form = new SqlStatementViewerForm())
                        {
                            form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");
                            form.Title = LocalizationHelper.GetLanguageString("DDL Statement Viewer", "Global", "Global", "msg", "DDLViewer", "Text");
                            form.SchemaType = schemaType;
                            form.SchemaName = schemaName;

                            var script = DatabaseSqlExecutor.GetCreateScript_PostgreSql(schemaNode, schemaType, schemaName);

                            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
                            {
                                form.CellText = PostgreSqlCreateTableScriptBeautifier.Beautify(script);
                            }
                            else
                            {
                                form.CellText = script;
                            }

                            var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "SQLStatementViewerFormWidth", "SQLStatementViewerFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                            form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                            form.ShowDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");

                i++;
                schemaBrowserContextMenu.Items.Add("-");
            }

            languageText = LocalizationHelper.GetLanguageString("Schema Browser", "form", "SchemaBrowserForm", "object", "this", "Text");
            languageText = $"{languageText}...";

            i++;
            schemaBrowserContextMenu.Items.Add(languageText);
            schemaBrowserContextMenu.Items[i].Enabled = !isFromSchemaBrowser;

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
                {
                    MyGlobal.DisplaySchemaBrowser(schemaNode, schemaType, schemaName);
                }
                else if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Views))
                {
                    MyGlobal.DisplaySchemaBrowser(schemaNode, schemaType, schemaName);
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Schema Browser 16x16.ico");

            i++;
            languageText = LocalizationHelper.GetLanguageString("Generate SQL Statement", "form", "GenerateSqlForm", "object", "this", "Text");
            languageText = $"{languageText}...";
            schemaBrowserContextMenu.Items.Add(languageText);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                using (var form = new GenerateSqlForm())
                {
                    form.SchemaNode = schemaNode;
                    form.SchemaName = schemaName;
                    form.SchemaType = schemaType;
                    form.AccessibleDescriptionString = accessibleDescription;

                    //20250426 改寫 Width/Height 取值方法
                    var (formWidth, formHeight) = GetFormWidthHeightSettings(MyGlobal.DomainUser, "GenerateSqlFormWidth", "GenerateSqlFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                    form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                    form.ShowDialog();
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Generate SQL Statement 16x16.ico");

            //20241109 針對 Table，新增 Comment/Drop/Rename/Truncate 右鍵選單
            #region
            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
            {
                i++;
                schemaBrowserContextMenu.Items.Add("-");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Comment Table", "form", "EditTableForm", "object", "CommentTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Comment.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Comment;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Comment.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Drop Table", "form", "EditTableForm", "object", "DropTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Drop.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Drop;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();

                        //20250204 更新 Schema 訊息
                        if (form.Result != null && form.Result == "REFRESH")
                        {
                            MyGlobal.GlobalTemp4 = "REFRESH";
                            MyGlobal.GlobalTemp5 = "UpdateSchemaInformation`";
                        }
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Drop.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Rename Table", "form", "EditTableForm", "object", "RenameTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Rename.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Rename;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();

                        //20250204 更新 Schema 訊息
                        if (form.Result != null && form.Result == "REFRESH")
                        {
                            MyGlobal.GlobalTemp4 = "REFRESH";
                            MyGlobal.GlobalTemp5 = "UpdateSchemaInformation`";
                        }
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Rename.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Truncate Table", "form", "EditTableForm", "object", "TruncateTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Truncate.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Truncate;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Truncate.ico");
            }
            #endregion

            i++;
            schemaBrowserContextMenu.Items.Add("-");

            i++;
            schemaBrowserContextMenu.Items.Add(title1);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                Clipboard.SetDataObject(schemaName, false);
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.C;

            i++;

            //20230909 新增 select * from XXX
            schemaBrowserContextMenu.Items.Add($"{title2} select * from {schemaNode}.{schemaName}");

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`select * from {schemaNode}.{schemaName}";
                }
                else
                {
                    Clipboard.SetDataObject($"select * from {schemaNode}.{schemaName}", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add(title2);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}";
                }
                else
                {
                    Clipboard.SetDataObject(schemaName, false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add($"{title2} + \", \""); //Paste to Query Editor + ", "

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}, ";
                }
                else
                {
                    Clipboard.SetDataObject($"{schemaName}, ", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add($"{title2} + \", \\r\\n\""); //Paste to Query Editor + ", \r\n"

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}, \r\n";
                }
                else
                {
                    Clipboard.SetDataObject($"{schemaName}, \r\n", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Alt | Keys.Shift | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add($"{title2} + \"\\r\\n\""); //Paste to Query Editor + "\r\n"

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}\r\n";
                }
                else
                {
                    Clipboard.SetDataObject($"{schemaName}\r\n", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.Shift | Keys.V;

            c1GridSchemaBrowser.ContextMenuStrip = schemaBrowserContextMenu;

            if (MyLibrary.IsDarkMode)
            {
                schemaBrowserContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                schemaBrowserContextMenu.ForeColor = Color.White;
                schemaBrowserContextMenu.RenderMode = ToolStripRenderMode.System;
                //cMenuSchemaBrowser.ShowImageMargin = false;
            }

            schemaBrowserContextMenu.Show(c1GridSchemaBrowser, new Point(x, y));
        }

        public static void GenerateRightMenuForCopy_SqlServer(bool isFromSchemaBrowser, C1TrueDBGrid c1GridSchemaBrowser, ContextMenuStrip schemaBrowserContextMenu, ScintillaEditor editor, string accessibleDescription, string schemaNode, string schemaDbo, string schemaType, string title1, string title2, string schemaName, int x, int y, string objectId)
        {
            var i = 0;
            var languageText = string.Empty;

            schemaBrowserContextMenu.Items.Add(schemaName);
            schemaBrowserContextMenu.Items[i].Enabled = false;

            i++;
            schemaBrowserContextMenu.Items.Add("-");

            if (!isFromSchemaBrowser)
            {
                //20250917 加上 Show Create Script 功能表
                languageText = LocalizationHelper.GetLanguageString("Show DDL Statement", "Global", "Global", "msg", "ShowDDL", "Text");
                schemaBrowserContextMenu.Items.Add(languageText);

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    try
                    {
                        using (var form = new SqlStatementViewerForm())
                        {
                            form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");
                            form.Title = LocalizationHelper.GetLanguageString("DDL Statement Viewer", "Global", "Global", "msg", "DDLViewer", "Text");
                            form.SchemaType = schemaType;
                            form.SchemaName = schemaName;

                            form.CellText = DatabaseSqlExecutor.GetCreateScript_SqlServer(schemaType, schemaNode, schemaDbo, schemaName, objectId);

                            var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "SQLStatementViewerFormWidth", "SQLStatementViewerFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                            form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                            form.ShowDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");

                i++;
                schemaBrowserContextMenu.Items.Add("-");
            }

            languageText = LocalizationHelper.GetLanguageString("Schema Browser", "form", "SchemaBrowserForm", "object", "this", "Text");
            languageText = $"{languageText}...";

            i++;
            schemaBrowserContextMenu.Items.Add(languageText);
            schemaBrowserContextMenu.Items[i].Enabled = !isFromSchemaBrowser;

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
                {
                    MyGlobal.DisplaySchemaBrowser(schemaNode, schemaType, schemaName, schemaDbo, objectId);
                }
                else if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Views))
                {
                    MyGlobal.DisplaySchemaBrowser(schemaNode, schemaType, schemaName, schemaDbo, objectId);
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Schema Browser 16x16.ico");

            i++;
            languageText = LocalizationHelper.GetLanguageString("Generate SQL Statement", "form", "GenerateSqlForm", "object", "this", "Text");
            languageText = $"{languageText}...";
            schemaBrowserContextMenu.Items.Add(languageText);

            schemaName = schemaName.Replace($"{schemaDbo}.", string.Empty);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                using (var form = new GenerateSqlForm())
                {
                    form.SchemaNode = schemaNode;
                    form.SchemaType = schemaType;
                    form.SchemaName = schemaName;
                    form.SchemaDbo = schemaDbo;
                    form.ObjectId = objectId;
                    form.AccessibleDescriptionString = accessibleDescription;

                    //20250426 改寫 Width/Height 取值方法
                    var (formWidth, formHeight) = GetFormWidthHeightSettings(MyGlobal.DomainUser, "GenerateSqlFormWidth", "GenerateSqlFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                    form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                    form.ShowDialog();
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Generate SQL Statement 16x16.ico");

            //20241109 針對 Table，新增 Comment/Drop/Rename/Truncate 右鍵選單
            #region
            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
            {
                i++;
                schemaBrowserContextMenu.Items.Add("-");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Comment Table", "form", "EditTableForm", "object", "CommentTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Comment.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaDatabase = schemaNode;
                        form.SchemaName = schemaDbo;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Comment;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Comment.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Drop Table", "form", "EditTableForm", "object", "DropTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Drop.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaDatabase = schemaNode;
                        form.SchemaName = schemaDbo;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Drop;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();

                        //20250204 更新 Schema 訊息
                        if (form.Result != null && form.Result == "REFRESH")
                        {
                            MyGlobal.GlobalTemp4 = "REFRESH";
                            MyGlobal.GlobalTemp5 = "UpdateSchemaInformation`";
                        }
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Drop.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Rename Table", "form", "EditTableForm", "object", "RenameTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Rename.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaDatabase = schemaNode;
                        form.SchemaName = schemaDbo;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Rename;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();

                        //20250204 更新 Schema 訊息
                        if (form.Result != null && form.Result == "REFRESH")
                        {
                            MyGlobal.GlobalTemp4 = "REFRESH";
                            MyGlobal.GlobalTemp5 = "UpdateSchemaInformation`";
                        }
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Rename.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Truncate Table", "form", "EditTableForm", "object", "TruncateTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Truncate.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaDatabase = schemaNode;
                        form.SchemaName = schemaDbo;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Truncate;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Truncate.ico");
            }
            #endregion

            i++;
            schemaBrowserContextMenu.Items.Add("-");

            i++;
            schemaBrowserContextMenu.Items.Add(title1);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                Clipboard.SetDataObject(schemaName, false);
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.C;

            i++;

            //20230909 新增 select * from XXX
            schemaBrowserContextMenu.Items.Add($"{title2} select * from {schemaDbo}.{schemaName}");

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`select * from {schemaDbo}.{schemaName}";
                }
                else
                {
                    Clipboard.SetDataObject($"select * from {schemaDbo}.{schemaName}", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add(title2);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}";
                }
                else
                {
                    Clipboard.SetDataObject(schemaName, false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add($"{title2} + \", \""); //Paste to Query Editor + ", "

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}, ";
                }
                else
                {
                    Clipboard.SetDataObject($"{schemaName}, ", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add($"{title2} + \", \\r\\n\""); //Paste to Query Editor + ", \r\n"

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}, \r\n";
                }
                else
                {
                    Clipboard.SetDataObject($"{schemaName}, \r\n", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Alt | Keys.Shift | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add($"{title2} + \"\\r\\n\""); //Paste to Query Editor + "\r\n"

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}\r\n";
                }
                else
                {
                    Clipboard.SetDataObject($"{schemaName}\r\n", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.Shift | Keys.V;

            c1GridSchemaBrowser.ContextMenuStrip = schemaBrowserContextMenu;

            if (MyLibrary.IsDarkMode)
            {
                schemaBrowserContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                schemaBrowserContextMenu.ForeColor = Color.White;
                schemaBrowserContextMenu.RenderMode = ToolStripRenderMode.System;
                //cMenuSchemaBrowser.ShowImageMargin = false;
            }

            schemaBrowserContextMenu.Show(c1GridSchemaBrowser, new Point(x, y));
        }

        public static void GenerateRightMenuForCopy_MySql(bool isFromSchemaBrowser, C1TrueDBGrid c1GridSchemaBrowser, ContextMenuStrip schemaBrowserContextMenu, ScintillaEditor editor, string accessibleDescription, string schemaNode, string schemaType, string title1, string title2, string schemaName, int x, int y)
        {
            var i = 0;
            var languageText = string.Empty;

            schemaBrowserContextMenu.Items.Add(schemaName);
            schemaBrowserContextMenu.Items[i].Enabled = false;

            i++;
            schemaBrowserContextMenu.Items.Add("-");

            if (!isFromSchemaBrowser)
            {
                //20250917 加上 Show Create Script 功能表
                languageText = LocalizationHelper.GetLanguageString("Show DDL Statement", "Global", "Global", "msg", "ShowDDL", "Text");
                schemaBrowserContextMenu.Items.Add(languageText);

                i++;

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    try
                    {
                        using (var form = new SqlStatementViewerForm())
                        {
                            form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");
                            form.Title = LocalizationHelper.GetLanguageString("DDL Statement Viewer", "Global", "Global", "msg", "DDLViewer", "Text");
                            form.SchemaType = schemaType;
                            form.SchemaName = schemaName;

                            form.CellText = DatabaseSqlExecutor.GetCreateScript_MySql(schemaType, schemaNode, schemaName);

                            var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "SQLStatementViewerFormWidth", "SQLStatementViewerFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                            form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                            form.ShowDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");

                i++;
                schemaBrowserContextMenu.Items.Add("-");
            }

            languageText = LocalizationHelper.GetLanguageString("Schema Browser", "form", "SchemaBrowserForm", "object", "this", "Text");
            languageText = $"{languageText}...";

            i++;
            schemaBrowserContextMenu.Items.Add(languageText);
            schemaBrowserContextMenu.Items[i].Enabled = !isFromSchemaBrowser;

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
                {
                    MyGlobal.DisplaySchemaBrowser(schemaNode, schemaType, schemaName);
                }
                else if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Views))
                {
                    MyGlobal.DisplaySchemaBrowser(schemaNode, schemaType, schemaName);
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Schema Browser 16x16.ico");

            i++;
            languageText = LocalizationHelper.GetLanguageString("Generate SQL Statement", "form", "GenerateSqlForm", "object", "this", "Text");
            languageText = $"{languageText}...";
            schemaBrowserContextMenu.Items.Add(languageText);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                using (var form = new GenerateSqlForm())
                {
                    form.SchemaNode = schemaNode;
                    form.SchemaType = schemaType;
                    form.SchemaName = schemaName;
                    form.AccessibleDescriptionString = accessibleDescription;

                    //20250426 改寫 Width/Height 取值方法
                    var (formWidth, formHeight) = GetFormWidthHeightSettings(MyGlobal.DomainUser, "GenerateSqlFormWidth", "GenerateSqlFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                    form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                    form.ShowDialog();
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Generate SQL Statement 16x16.ico");

            //20241109 針對 Table，新增 Comment/Drop/Rename/Truncate 右鍵選單
            #region
            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
            {
                i++;
                schemaBrowserContextMenu.Items.Add("-");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Comment Table", "form", "EditTableForm", "object", "CommentTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Comment.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Comment;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Comment.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Drop Table", "form", "EditTableForm", "object", "DropTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Drop.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Drop;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();

                        //20250204 更新 Schema 訊息
                        if (form.Result != null && form.Result == "REFRESH")
                        {
                            MyGlobal.GlobalTemp4 = "REFRESH";
                            MyGlobal.GlobalTemp5 = "UpdateSchemaInformation`";
                        }
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Drop.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Rename Table", "form", "EditTableForm", "object", "RenameTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Rename.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Rename;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();

                        //20250204 更新 Schema 訊息
                        if (form.Result != null && form.Result == "REFRESH")
                        {
                            MyGlobal.GlobalTemp4 = "REFRESH";
                            MyGlobal.GlobalTemp5 = "UpdateSchemaInformation`";
                        }
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Rename.ico");

                i++;
                languageText = LocalizationHelper.GetLanguageString("Truncate Table", "form", "EditTableForm", "object", "TruncateTable", "Text");
                languageText = $"{languageText}...";
                schemaBrowserContextMenu.Items.Add(languageText);

                schemaBrowserContextMenu.Items[i].Click += delegate
                {
                    using (var form = new EditTableForm())
                    {
                        form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Table Truncate.ico");
                        form.ShowInTaskbar = false;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.SchemaName = schemaNode;
                        form.TableName = schemaName;
                        form.EditMode = EditTableMode.Truncate;

                        ApplyEditTableFormSize(form);
                        form.ShowDialog();
                    }
                };

                schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Table Truncate.ico");
            }
            #endregion

            i++;
            schemaBrowserContextMenu.Items.Add("-");

            i++;
            schemaBrowserContextMenu.Items.Add(title1);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                Clipboard.SetDataObject(schemaName, false);
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.C;

            i++;

            //20230909 新增 select * from XXX
            schemaBrowserContextMenu.Items.Add($"{title2} select * from {schemaName}");

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`select * from {schemaName}";
                }
                else
                {
                    Clipboard.SetDataObject($"select * from {schemaName}", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add(title2);

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}";
                }
                else
                {
                    Clipboard.SetDataObject(schemaName, false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            schemaBrowserContextMenu.Items[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor 16x16.ico");
            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add($"{title2} + \", \""); //Paste to Query Editor + ", "

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}, ";
                }
                else
                {
                    Clipboard.SetDataObject($"{schemaName}, ", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add($"{title2} + \", \\r\\n\""); //Paste to Query Editor + ", \r\n"

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}, \r\n";
                }
                else
                {
                    Clipboard.SetDataObject($"{schemaName}, \r\n", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Alt | Keys.Shift | Keys.V;

            i++;
            schemaBrowserContextMenu.Items.Add($"{title2} + \"\\r\\n\""); //Paste to Query Editor + "\r\n"

            schemaBrowserContextMenu.Items[i].Click += delegate
            {
                if (isFromSchemaBrowser)
                {
                    MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{schemaName}\r\n";
                }
                else
                {
                    Clipboard.SetDataObject($"{schemaName}\r\n", false);
                    editor.Paste();

                    if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                    {
                        editor.Focus();
                    }
                }
            };

            ((ToolStripMenuItem)schemaBrowserContextMenu.Items[i]).ShortcutKeys = Keys.Control | Keys.Alt | Keys.Shift | Keys.V;

            c1GridSchemaBrowser.ContextMenuStrip = schemaBrowserContextMenu;

            if (MyLibrary.IsDarkMode)
            {
                schemaBrowserContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                schemaBrowserContextMenu.ForeColor = Color.White;
                schemaBrowserContextMenu.RenderMode = ToolStripRenderMode.System;
            }

            schemaBrowserContextMenu.Show(c1GridSchemaBrowser, new Point(x, y));
        }

        public static void ApplyEditColumnFormSize(EditColumnForm form)
        {
            var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "EditColumnFormWidth", "EditColumnFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

            form.ClientSize = new Size(formWidth - 16, formHeight - 38);
        }

        public static void ApplyEditTableFormSize(EditTableForm form)
        {
            var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "EditTableFormWidth", "EditTableFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

            form.ClientSize = new Size(formWidth - 16, formHeight - 38);
        }
    }
}
