namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private sealed class QueryExecutionPipelineRunContext
        {
            public QueryExecutionContext ExecutionContext { get; set; } = new QueryExecutionContext();
            public QueryExecutionKindResult QueryKind { get; set; } = new QueryExecutionKindResult();

            public string QueryText { get; set; } = string.Empty;
            public string PayloadText { get; set; } = string.Empty;

            public bool ShouldContinue => ExecutionContext?.ShouldContinue ?? false;
            public int SelectionStart => ExecutionContext?.SelectionStart ?? 0;
            public string CrLfOffsetText => ExecutionContext?.CrLfOffsetText ?? string.Empty;
            public bool HasExecutableStatement => QueryKind?.HasExecutableStatement ?? false;
        }

        private QueryExecutionPipelineRunContext CreateQueryExecutionPipelineRunContext(QueryExecutionContext executionContext)
        {
            return new QueryExecutionPipelineRunContext
            {
                ExecutionContext = executionContext ?? new QueryExecutionContext(),
                QueryText = executionContext?.QueryText ?? string.Empty
            };
        }

        private bool ShouldContinueQueryExecutionPipelineRun(QueryExecutionPipelineRunContext runContext)
        {
            if (runContext == null)
            {
                return false;
            }

            if (!runContext.ShouldContinue)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(runContext.QueryText);
        }

        private void AnalyzeQueryExecutionPipelineRunKind(QueryExecutionPipelineRunContext runContext)
        {
            if (runContext == null)
            {
                return;
            }

            runContext.QueryKind = AnalyzeQueryExecutionKind(runContext.QueryText);
        }

        private void BuildQueryExecutionPipelinePayloadText(QueryExecutionPipelineRunContext runContext)
        {
            if (runContext == null)
            {
                return;
            }

            runContext.PayloadText = BuildQueryExecutionPayloadText
            (
                runContext.QueryText,
                runContext.SelectionStart,
                runContext.CrLfOffsetText
            );
        }

        private void AppendQueryExecutionPipelinePagingPayloadIfNeeded(QueryExecutionPipelineRunContext runContext)
        {
            if (runContext == null)
            {
                return;
            }

            runContext.PayloadText = AppendQueryExecutionPagingPayloadIfNeeded
            (
                runContext.PayloadText,
                runContext.QueryKind
            );
        }
    }
}
