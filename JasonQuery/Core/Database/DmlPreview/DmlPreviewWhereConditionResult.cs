namespace JasonQuery.Core.Database.DmlPreview
{
    internal sealed class DmlPreviewWhereConditionResult
    {
        private DmlPreviewWhereConditionResult()
        {
        }

        public bool Success { get; private set; }

        public string Condition { get; private set; } = string.Empty;

        public DmlPreviewWhereConditionSource Source { get; private set; }

        public DmlPreviewWhereConditionFailureKind FailureKind { get; private set; }

        public static DmlPreviewWhereConditionResult Succeeded(string condition, DmlPreviewWhereConditionSource source)
        {
            return new DmlPreviewWhereConditionResult
            {
                Success = true,
                Condition = condition ?? string.Empty,
                Source = source,
                FailureKind = DmlPreviewWhereConditionFailureKind.None
            };
        }

        public static DmlPreviewWhereConditionResult Failed(DmlPreviewWhereConditionFailureKind failureKind)
        {
            return new DmlPreviewWhereConditionResult
            {
                Success = false,
                Condition = string.Empty,
                Source = DmlPreviewWhereConditionSource.None,
                FailureKind = failureKind
            };
        }
    }
}
