using IconLibrary;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.LeftPanel;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //L2-C3-C2: guarded read-only object actions. Structural changes remain disabled until L2-C3-D.
    public partial class MainForm
    {
        private void SharedSchemaObjectMenuClick(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem item) || !(item.Tag is SharedSchemaExplorerMenuCommand command) || !item.Enabled || !ReferenceEquals(item.Owner, _sharedSchemaCopyContextMenu))
            {
                return;
            }

            var selection = _sharedSchemaCopySelection;
            var state = CaptureSharedLeftPanelHostState();
            var context = CaptureLeftPanelContext();
            var activeTab = tabControl1.SelectedTab;
            var activeForm = activeTab?.Control;

            if (selection == null || state == null || context == null || !context.IsConnected || !context.SchemaReady
                || state.ConnectionRevision != _sharedSchemaMenuConnectionRevision
                || !ReferenceEquals(activeForm, _sharedSchemaMenuActiveForm)
                || context.ActiveTarget != _sharedSchemaMenuTarget
                || selection.SourceType != _currentSourceType
                || DatabaseSqlExecutor.CurrentDataSource != _currentSourceType
                || !CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid)
                || !SharedSchemaExplorerObjectCommandPolicy.CanExecute(context.ActiveTarget, selection, command))
            {
                return;
            }

            //Tab remains the active, correctly-typed receiving form at execution time.
            if (context.ActiveTarget == LeftPanelTargetKind.QueryForm && !(activeForm is QueryForm))
            {
                return;
            }

            if (context.ActiveTarget == LeftPanelTargetKind.SchemaBrowserForm && !(activeForm is SchemaBrowserForm))
            {
                return;
            }

            try
            {
                switch (command)
                {
                    case SharedSchemaExplorerMenuCommand.ShowDdl:
                        {
                            ShowSharedSchemaDdl(selection);
                            break;
                        }
                    case SharedSchemaExplorerMenuCommand.OpenSchemaBrowser:
                        {
                            OpenSharedSchemaBrowser(selection);
                            break;
                        }
                    case SharedSchemaExplorerMenuCommand.GenerateSqlStatement:
                        {
                            OpenSharedGenerateSql(selection, activeTab.AccessibleDescription);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private static void ShowSharedSchemaDdl(SchemaExplorerSelectionInfo selection)
        {
            if (selection.SourceType != DataSourceType.SqlServer)
            {
                UIHelper.ShowDDL(selection.SchemaType, selection.SchemaName, selection.SchemaNode, selection.SchemaDbo, selection.PackageSpecBody);
                return;
            }

            //Unlike the generic ShowDDL overload, the original SQL Server F7 passes ObjectId.
            using (var form = new SqlStatementViewerForm())
            {
                form.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Show SQL Statement 16x16.ico");
                form.Title = LocalizationHelper.GetLanguageString("DDL Statement Viewer", "Global", "Global", "msg", "DDLViewer", "Text");
                form.SchemaType = selection.SchemaType;
                form.SchemaName = selection.SchemaName;
                form.CellText = DatabaseSqlExecutor.GetCreateScript_SqlServer(selection.SchemaType, selection.SchemaNode, selection.SchemaDbo, selection.SchemaName, selection.ObjectId);

                var size = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "SQLStatementViewerFormWidth", "SQLStatementViewerFormHeight", form.ClientSize.Width, form.ClientSize.Height);

                form.ClientSize = new System.Drawing.Size(size.Width - 16, size.Height - 38);
                form.ShowDialog();
            }
        }

        private static void OpenSharedSchemaBrowser(SchemaExplorerSelectionInfo selection)
        {
            switch (selection.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        MyGlobal.DisplaySchemaBrowser(string.Empty, selection.SchemaType, selection.SchemaName);
                        break;
                    }
                case DataSourceType.PostgreSql:
                case DataSourceType.MySql:
                    {
                        MyGlobal.DisplaySchemaBrowser(selection.SchemaNode, selection.SchemaType, selection.SchemaName);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        MyGlobal.DisplaySchemaBrowser(selection.SchemaNode, selection.SchemaType, selection.SchemaName, selection.SchemaDbo, selection.ObjectId);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private static void OpenSharedGenerateSql(SchemaExplorerSelectionInfo selection, string activeTabIdentity)
        {
            using (var form = new GenerateSqlForm())
            {
                form.SchemaType = selection.SchemaType;
                form.SchemaNode = selection.SchemaNode;
                form.SchemaDbo = selection.SchemaDbo;
                form.SchemaName = SharedSchemaExplorerObjectCommandPolicy.GetGenerateSqlObjectName(selection);
                form.ObjectId = selection.ObjectId;
                form.AccessibleDescriptionString = activeTabIdentity ?? string.Empty;

                var size = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "GenerateSqlFormWidth", "GenerateSqlFormHeight", form.ClientSize.Width, form.ClientSize.Height);

                form.ClientSize = new System.Drawing.Size(size.Width - 16, size.Height - 38);
                form.ShowDialog();
            }
        }
    }
}
