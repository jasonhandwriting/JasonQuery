using System;
using JasonLibrary.Core.Text.Formatting;
using JasonQuery.Core.Database.Connection;

//五種資料庫來源類型對應、成功／失敗／空輸出／未變更輸出、選取範圍與 engine override
namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class EditorSqlFormatterService
    {
        private readonly ITextEditor _editor;
        private readonly Func<SqlFormatRequest, SqlFormatResult> _format;

        public EditorSqlFormatterService(ITextEditor editor, Func<SqlFormatRequest, SqlFormatResult> format)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _format = format ?? throw new ArgumentNullException(nameof(format));
        }

        public bool TryFormatSelection(DataSourceType dataSourceType, SqlFormatterEngineKind requestedEngineKind,
                                       SqlFormatOptions options, out string errorMessage)
        {
            errorMessage = string.Empty;

            var selectedText = _editor.SelectedText;

            if (string.IsNullOrWhiteSpace(selectedText))
            {
                return false;
            }

            var request = new SqlFormatRequest
            (
                selectedText,
                DataSourceTypeMapper.ToDatabaseProviderKind(dataSourceType),
                requestedEngineKind,
                options
            );

            var result = _format(request);

            if (result == null)
            {
                errorMessage = "The SQL formatter returned no result.";
                return false;
            }

            if (!result.Success)
            {
                errorMessage = result.ErrorMessage;
                return false;
            }

            if (string.IsNullOrEmpty(result.FormattedSql))
            {
                errorMessage = "The SQL formatter returned empty SQL.";
                return false;
            }

            if (string.Equals(selectedText, result.FormattedSql, StringComparison.Ordinal))
            {
                return true;
            }

            var start = _editor.SelectionStart;
            var end = _editor.SelectionEnd;

            _editor.SelectionStart = start;
            _editor.SelectionEnd = end;
            _editor.ReplaceSelection(result.FormattedSql);
            _editor.SelectionStart = start;
            _editor.SelectionEnd = start + result.FormattedSql.Length;
            _editor.ScrollCaret();

            return true;
        }
    }
}
