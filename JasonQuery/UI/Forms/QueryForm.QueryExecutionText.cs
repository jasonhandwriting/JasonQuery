using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using System.Text.RegularExpressions;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private sealed class QueryExecutionTextContext
        {
            public string QueryText { get; set; } = string.Empty;
            public int SelectionStart { get; set; }
            public string CrLfOffsetText { get; set; } = string.Empty;
        }

        private QueryExecutionTextContext ResolveQueryExecutionText(string sql)
        {
            var context = new QueryExecutionTextContext
            {
                QueryText = string.IsNullOrEmpty(sql) ? editor.SelectedText : sql,
                SelectionStart = editor.SelectionStart
            };

            if (string.IsNullOrEmpty(context.QueryText))
            {
                ApplySelectAllForQueryExecution(context);
            }

            context.QueryText = context.QueryText.Replace(MyGlobal.Separator, " ");
            context.CrLfOffsetText = BuildQueryExecutionCrLfOffsetText(context.SelectionStart);

            return context;
        }

        private void ApplySelectAllForQueryExecution(QueryExecutionTextContext context)
        {
            if (context == null)
            {
                return;
            }

            //使用者沒有選取任何文字，由程式自動全選
            context.SelectionStart = 0;
            context.CrLfOffsetText = MyGlobal.Separator5;
            context.QueryText = editor.Text;

            editor.SelectionStart = 0;
            editor.SelectionEnd = editor.Text.Length;
        }

        private string BuildQueryExecutionCrLfOffsetText(int selectionStart)
        {
            if (_currentSourceType != DataSourceType.SqlServer && _currentSourceType != DataSourceType.MySql)
            {
                return string.Empty;
            }

            if (selectionStart < 0)
            {
                selectionStart = 0;
            }

            if (selectionStart > editor.Text.Length)
            {
                selectionStart = editor.Text.Length;
            }

            var textBeforeSelection = editor.Text.Substring(0, selectionStart);
            var crLfCount = Regex.Matches(textBeforeSelection, "\r\n").Count;

            return $"{MyGlobal.Separator5}{crLfCount}";
        }

        private string AppendQueryExecutionPagingPayloadIfNeeded(string queryText, QueryExecutionKindResult queryKind)
        {
            if (queryKind == null)
            {
                return queryText;
            }

            if (!ShouldAppendQueryExecutionPagingPayload(queryKind))
            {
                return queryText;
            }

            return $"{queryText}{MyGlobal.Separator5}{btnNextPage.Tag}{MyGlobal.Separator5}{btnPaginationOn.Tag}";
        }

        private bool ShouldAppendQueryExecutionPagingPayload(QueryExecutionKindResult queryKind)
        {
            if (queryKind == null)
            {
                return false;
            }

            if (!queryKind.IsQuery)
            {
                return false;
            }

            if (btnPaginationOff.Visible)
            {
                return false;
            }

            if (queryKind.IsExtended)
            {
                return false;
            }

            if (queryKind.IsLockingQuery)
            {
                return false;
            }

            return true;
        }
    }
}
