using JasonQuery.Core.Database.Diagnostics.Common;

namespace JasonQuery.Core.Database.Diagnostics.MySql
{
    internal sealed class MySqlErrorPositionResolution
    {
        public SqlErrorResolutionResult PositionResult { get; set; } = new SqlErrorResolutionResult();

        public string ErrorMessage { get; set; } = string.Empty;

        public MySqlErrorMessageKind MessageKind { get; set; }

        public bool ShouldShowSelectDatabaseFirstMessage { get; set; }
    }
}
