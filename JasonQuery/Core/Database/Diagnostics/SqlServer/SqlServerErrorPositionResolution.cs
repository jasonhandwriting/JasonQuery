using JasonQuery.Core.Database.Diagnostics.Common;

namespace JasonQuery.Core.Database.Diagnostics.SqlServer
{
    internal sealed class SqlServerErrorPositionResolution
    {
        public SqlErrorResolutionResult PositionResult { get; set; } = new SqlErrorResolutionResult();

        public string ErrorMessage { get; set; } = string.Empty;

        public bool UsedSecondaryErrorMessage { get; set; }

        public bool ShouldAppendStringConcatenationHint { get; set; }
    }
}