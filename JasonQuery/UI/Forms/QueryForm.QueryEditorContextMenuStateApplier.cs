using JasonLibrary.Core;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private static class QueryEditorContextMenuStateApplier
        {
            public static void Apply(QueryForm owner, EditorRightClickContext context)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException(nameof(owner));
                }

                if (context == null)
                {
                    context = new EditorRightClickContext();
                }

                ApplyEditorExecuteMenuState(owner);
                ApplyEditorSchemaMenuState(owner, context);
                ApplyEditorTableMenuState(owner, context);
                ApplyEditorBasicEditMenuState(owner, context);
            }

            public static void ApplyDarkModeStyle(QueryForm owner)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException(nameof(owner));
                }

                owner.editor.ContextMenuStrip = owner._queryEditorContextMenu;

                if (!MyLibrary.IsDarkMode)
                {
                    return;
                }

                owner._queryEditorContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                owner._queryEditorContextMenu.ForeColor = Color.White;
                owner._queryEditorContextMenu.RenderMode = ToolStripRenderMode.System;
            }

            public static void Show(QueryForm owner, MouseEventArgs e)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException(nameof(owner));
                }

                if (e == null)
                {
                    return;
                }

                owner.editor.ContextMenuStrip = owner._queryEditorContextMenu;
                owner._queryEditorContextMenu.Show(owner.editor, new Point(e.X, e.Y));
            }

            private static void ApplyEditorExecuteMenuState(QueryForm owner)
            {
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Execute].Enabled = owner.btnQuery.Enabled;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.ExecuteCurrentBlock].Enabled = owner.btnExecuteCurrentBlock.Enabled;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.ExecuteCurrentLine].Enabled = owner.btnExecuteCurrentLine.Enabled;
            }

            private static void ApplyEditorSchemaMenuState(QueryForm owner, EditorRightClickContext context)
            {
                owner._queryEditorContextMenu.Items[QueryEditorColumn.SchemaBrowser].Visible = !context.IsTableOrView;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.SchemaBrowser].Enabled = false;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.ShowCreateScript].Visible = context.IsTableOrView;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.SchemaBrowserTable].Visible = context.IsTable;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.SchemaBrowserView].Visible = context.IsView;

                var schemaText = BuildEditorSchemaMenuSuffix(owner, context);

                if (context.IsTableOrView)
                {
                    owner._queryEditorContextMenu.Items[QueryEditorColumn.ShowCreateScript].Text =
                        $"{owner._queryEditorContextMenu.Items[QueryEditorColumn.ShowCreateScript].Tag}{schemaText}";

                    owner.SetEditorObjectMenuPayload
                    (
                        QueryEditorColumn.ShowCreateScript,
                        owner.CreateEditorObjectMenuPayload(context)
                    );
                }

                if (context.IsTable)
                {
                    owner._queryEditorContextMenu.Items[QueryEditorColumn.SchemaBrowserTable].Text =
                        $"{owner._queryEditorContextMenu.Items[QueryEditorColumn.SchemaBrowserTable].Tag}{schemaText}";

                    owner.SetEditorObjectMenuPayload
                    (
                        QueryEditorColumn.SchemaBrowserTable,
                        owner.CreateEditorObjectMenuPayload(context)
                    );
                }

                if (context.IsView)
                {
                    owner._queryEditorContextMenu.Items[QueryEditorColumn.SchemaBrowserView].Text =
                        $"{owner._queryEditorContextMenu.Items[QueryEditorColumn.SchemaBrowserView].Tag}{schemaText}";

                    owner.SetEditorObjectMenuPayload
                    (
                        QueryEditorColumn.SchemaBrowserView,
                        owner.CreateEditorObjectMenuPayload(context)
                    );
                }

                owner._queryEditorContextMenu.Items[QueryEditorColumn.GenerateSql].Enabled = context.IsTableOrView;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.GenerateSql].Text =
                    $"{owner._queryEditorContextMenu.Items[QueryEditorColumn.GenerateSql].Tag}{schemaText}";

                var generateSqlPayload = owner.CreateEditorObjectMenuPayload(context);
                generateSqlPayload.AccessibleDescription = owner.AccessibleDescription;

                owner.SetEditorObjectMenuPayload(QueryEditorColumn.GenerateSql, generateSqlPayload);
            }

            private static string BuildEditorSchemaMenuSuffix(QueryForm owner, EditorRightClickContext context)
            {
                if (owner == null || context == null || !context.IsTableOrView)
                {
                    return string.Empty;
                }

                var wordPrefix = string.Empty;

                if (owner.IsPostgreSql && !string.IsNullOrWhiteSpace(context.SchemaNode))
                {
                    wordPrefix = $"{context.SchemaNode}.";
                }

                if (owner.IsSqlServer && !string.IsNullOrWhiteSpace(context.SchemaDbo))
                {
                    wordPrefix = $"{context.SchemaDbo}.";
                }

                if (owner.IsOracle && !string.IsNullOrWhiteSpace(context.SchemaNode))
                {
                    wordPrefix = $"{context.SchemaNode}.";
                }

                if (owner.IsMySql && !string.IsNullOrWhiteSpace(context.SchemaNode))
                {
                    wordPrefix = $"{context.SchemaNode}.";
                }

                return $" - {wordPrefix}{context.Word}";
            }

            private static void ApplyEditorTableMenuState(QueryForm owner, EditorRightClickContext context)
            {
                var schemaText = BuildEditorSchemaMenuSuffix(owner, context);

                owner._queryEditorContextMenu.Items[QueryEditorColumn.Dash3].Visible = context.IsTable;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.TableComment].Visible = context.IsTable;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.TableDrop].Visible = context.IsTable;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.TableRename].Visible = context.IsTable;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.TableTruncate].Visible = context.IsTable;

                owner._queryEditorContextMenu.Items[QueryEditorColumn.TableComment].Text =
                    $"{owner._queryEditorContextMenu.Items[QueryEditorColumn.TableComment].Tag}{schemaText}";

                owner._queryEditorContextMenu.Items[QueryEditorColumn.TableDrop].Text =
                    $"{owner._queryEditorContextMenu.Items[QueryEditorColumn.TableDrop].Tag}{schemaText}";

                owner._queryEditorContextMenu.Items[QueryEditorColumn.TableRename].Text =
                    $"{owner._queryEditorContextMenu.Items[QueryEditorColumn.TableRename].Tag}{schemaText}";

                owner._queryEditorContextMenu.Items[QueryEditorColumn.TableTruncate].Text =
                    $"{owner._queryEditorContextMenu.Items[QueryEditorColumn.TableTruncate].Tag}{schemaText}";

                var payload = owner.CreateEditorObjectMenuPayload(context);

                owner.SetEditorObjectMenuPayload(QueryEditorColumn.TableComment, payload);
                owner.SetEditorObjectMenuPayload(QueryEditorColumn.TableDrop, payload);
                owner.SetEditorObjectMenuPayload(QueryEditorColumn.TableRename, payload);
                owner.SetEditorObjectMenuPayload(QueryEditorColumn.TableTruncate, payload);
            }

            private static void ApplyEditorBasicEditMenuState(QueryForm owner, EditorRightClickContext context)
            {
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Undo].Enabled = owner.editor.CanUndo;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Redo].Enabled = owner.editor.CanRedo;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Cut].Enabled = context.IsSelected;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Copy].Enabled = context.IsSelected;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.CopyTo].Enabled = false;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.CopyTo].Visible = false;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Paste].Enabled = owner.editor.CanPaste;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Delete].Enabled = context.IsSelected;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.DeleteCurrentLine].Enabled = owner.editor.CanPaste;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.FindAndReplace].Enabled = !string.IsNullOrEmpty(owner.editor.Text);
                owner._queryEditorContextMenu.Items[QueryEditorColumn.SelectAll].Enabled = !string.IsNullOrEmpty(owner.editor.Text);
                owner._queryEditorContextMenu.Items[QueryEditorColumn.SelectCurrentBlock].Enabled = !string.IsNullOrEmpty(owner.editor.Text);
                owner._queryEditorContextMenu.Items[QueryEditorColumn.SelectCurrentLine].Enabled = !string.IsNullOrEmpty(owner.editor.Text);
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Code2SQL].Enabled = context.HasSelectedNonWhiteSpaceText;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.SQL2Code].Enabled = context.HasSelectedNonWhiteSpaceText;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Comment].Enabled = context.IsSelected;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.RemoveComment].Enabled = context.IsSelected;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Indent].Enabled = context.IsSelected;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.Unindent].Enabled = context.IsSelected;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.UpperCase].Enabled = context.IsSelected;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.LowerCase].Enabled = context.IsSelected;
                owner._queryEditorContextMenu.Items[QueryEditorColumn.SQLFormatter].Enabled = context.HasSelectedNonWhiteSpaceText;
            }
        }
    }
}
