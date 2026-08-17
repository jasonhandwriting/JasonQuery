namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private sealed class QueryExecutionContext
        {
            public QueryExecutionTextContext TextContext { get; set; } = new QueryExecutionTextContext();
            public string QueryText { get; set; } = string.Empty;
            public bool ShouldContinue { get; set; } = true;
            public int SelectionStart => TextContext?.SelectionStart ?? 0;
            public string CrLfOffsetText => TextContext?.CrLfOffsetText ?? string.Empty;
        }

        private QueryExecutionContext CreateQueryExecutionContext(QueryExecutionTextContext textContext, bool isNextPage)
        {
            var context = new QueryExecutionContext
            {
                TextContext = textContext,
                QueryText = textContext?.QueryText ?? string.Empty,
                ShouldContinue = true
            };

            var variableResult = ResolveQueryExecutionVariables(textContext, isNextPage);

            context.QueryText = variableResult.QueryText;
            context.ShouldContinue = variableResult.ShouldContinue;

            return context;
        }
    }
}