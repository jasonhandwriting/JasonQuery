using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.SchemaExplorer;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private SchemaExplorerSelectionInfo ResolveSchemaExplorerSelection(int displayRowIndex)
        {
            var request = CreateSchemaExplorerSelectionResolveRequest(displayRowIndex);
            var rowReader = new C1SchemaExplorerRowReader(c1GridSchemaBrowser);

            return SchemaExplorerSelectionResolver.Resolve(request, rowReader);
        }

        private SchemaExplorerSelectionResolveRequest CreateSchemaExplorerSelectionResolveRequest(int displayRowIndex)
        {
            return new SchemaExplorerSelectionResolveRequest
            {
                SourceType = _currentSourceType,
                DisplayRowIndex = displayRowIndex,
                IsExternalSelection = displayRowIndex == -1 || DisplayRowIndex == -1,
                IsShowColumnInfo = MyGlobal.IsShowColumnInfo,

                CurrentSchemaNode = SchemaNode,
                CurrentSchemaType = SchemaType,
                CurrentSchemaName = SchemaName,
                CurrentSchemaDbo = SchemaDbo,
                CurrentObjectId = ObjectId
            };
        }

        private bool DisplaySchemaExplorerSelection(SchemaExplorerSelectionInfo selection)
        {
            if (selection == null || !selection.CanDisplayObject)
            {
                _currentSchemaBrowserSelection = null;
                _schemaBrowserLazyLoadState.Reset(string.Empty);
                return false;
            }

            ApplySchemaExplorerSelectionToFormState(selection);
            ConfigureSchemaBrowserLazySelection(selection);
            return true;
        }

        private void ApplySchemaExplorerSelectionToFormState(SchemaExplorerSelectionInfo selection)
        {
            if (selection == null)
            {
                return;
            }

            if (selection.Kind != SchemaExplorerSelectionKind.ExternalObject && selection.DisplayRowIndex >= 0)
            {
                try
                {
                    c1GridSchemaBrowser.Row = selection.DisplayRowIndex;
                }
                catch
                {
                    //忽略 C1TrueDBGrid 在特殊列或資料重繫結期間可能發生的 Row 設定失敗
                }
            }

            SchemaNode = selection.SchemaNode;
            SchemaType = selection.SchemaType;
            SchemaName = selection.SchemaName;
            SchemaDbo = selection.SchemaDbo;
            ObjectId = selection.ObjectId;
        }
    }
}
