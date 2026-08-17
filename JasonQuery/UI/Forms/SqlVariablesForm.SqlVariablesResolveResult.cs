namespace JasonQuery.UI.Forms
{
    public partial class SqlVariablesForm
    {
        internal sealed class SqlVariablesResolveResult
        {
            public string SqlText { get; set; } = string.Empty;
            public string MappingText { get; set; } = string.Empty;
            public string PositionMapping { get; set; } = string.Empty;
        }
    }
}