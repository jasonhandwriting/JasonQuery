using JasonLibrary.Core.Text.Formatting;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using JasonQuery.Core.QueryEngine.Editor.Editing.AutoReplace;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void InitializeEditorServices()
        {
            var textEditor = new ScintillaTextEditorAdapter(editor);

            _editorIndentService = new EditorIndentService(textEditor);
            _editorCommentService = new EditorCommentService(textEditor, () => HideAutoCompleteGrid());
            _editorTextCaseService = new EditorTextCaseService(textEditor);
            _editorWhitespaceCleanupService = new EditorWhitespaceCleanupService(textEditor);
            _editorBlockSelectionService = new EditorBlockSelectionService(textEditor);
            _editorLineSelectionService = new EditorLineSelectionService(textEditor);
            _editorAutoReplaceService = new EditorAutoReplaceService();
            _queryEditorCtrlJPeriodResolver = new QueryEditorCtrlJPeriodResolver(textEditor);

            var sqlFormatterCoordinator = new SqlFormatterCoordinator();

            _editorSqlFormatterService = new EditorSqlFormatterService
            (
                textEditor,
                request => sqlFormatterCoordinator.Format(request)
            );
        }
    }
}
