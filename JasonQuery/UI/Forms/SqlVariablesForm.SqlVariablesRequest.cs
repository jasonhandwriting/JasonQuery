namespace JasonQuery.UI.Forms
{
    public partial class SqlVariablesForm
    {
        internal sealed class SqlVariablesRequest
        {
            public string SqlText { get; set; } = string.Empty;
            public string Variables { get; set; } = string.Empty;
            public string VariablesWithPosition { get; set; } = string.Empty;
        }
    }
}
