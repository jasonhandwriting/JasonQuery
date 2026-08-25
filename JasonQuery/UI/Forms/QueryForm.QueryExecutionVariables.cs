using JasonQuery.Core.Config;
using JasonQuery.Core.QueryEngine.Editor.Analysis;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private sealed class QueryExecutionVariableResult
        {
            public string QueryText { get; set; } = string.Empty;
            public bool ShouldContinue { get; set; } = true;
        }

        private QueryExecutionVariableResult ResolveQueryExecutionVariables(QueryExecutionTextContext textContext, bool isNextPage)
        {
            ResetQueryExecutionParameterState(textContext);

            var result = new QueryExecutionVariableResult
            {
                QueryText = textContext?.QueryText ?? string.Empty,
                ShouldContinue = true
            };

            var parametersDistinct = QueryEditorParameterAnalyzer.ExtractParametersInfo(result.QueryText, out var parametersAll);

            if (string.IsNullOrEmpty(parametersDistinct))
            {
                _sqlWhenError = result.QueryText;
                return result;
            }

            if (isNextPage)
            {
                result.QueryText = TextHelper.GetSafeString(btnPaginationOff.Tag);
                return result;
            }

            btnPaginationOff.Tag = string.Empty;

            var request = new SqlVariablesForm.SqlVariablesRequest
            {
                SqlText = result.QueryText,
                Variables = parametersDistinct,
                VariablesWithPosition = parametersAll
            };

            using (var form = new SqlVariablesForm(request))
            {
                //20250426 改寫 Width/Height 取值方法
                var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings
                (
                    MyGlobal.DomainUser,
                    "VariablesFormWidth",
                    "VariablesFormHeight",
                    defaultWidth: form.ClientSize.Width,
                    defaultHeight: form.ClientSize.Height
                );

                form.ClientSize = new Size(formWidth - 16, formHeight - 38);

                if (form.ShowDialog(this) != System.Windows.Forms.DialogResult.OK || form.ResolveResult == null)
                {
                    result.QueryText = string.Empty;
                    result.ShouldContinue = false;
                    return result;
                }

                var resolveResult = form.ResolveResult;

                if (!string.IsNullOrEmpty(resolveResult.SqlText))
                {
                    btnPaginationOff.Tag = resolveResult.SqlText;
                }

                result.QueryText = resolveResult.SqlText;
                _queryTextParametersMapping = resolveResult.MappingText;
                _queryTextParametersPositionMapping = resolveResult.PositionMapping;
                _sqlWhenError = editor.SelectedText;
            }

            result.ShouldContinue = !string.IsNullOrWhiteSpace(result.QueryText);

            return result;
        }

        private void ResetQueryExecutionParameterState(QueryExecutionTextContext textContext)
        {
            _queryTextParametersMapping = string.Empty;
            _queryTextParametersPositionMapping = string.Empty;
            _queryTextParametersStart = textContext?.SelectionStart ?? 0;
        }
    }
}
