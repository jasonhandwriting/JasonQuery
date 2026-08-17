namespace JasonQuery.Core.Database.Diagnostics.Common
{
    internal sealed class SqlErrorResolutionResult
    {
        public bool Found { get; set; }

        public int Position { get; set; }

        public int Length { get; set; }

        public string TargetText { get; set; } = string.Empty;

        public string NormalizedErrorCode { get; set; } = string.Empty;

        public bool ShouldSetSquiggle { get; set; }

        public bool UsedReportedPositionFallback { get; set; }

        public bool HadUtf8PositionAdjustment { get; set; }

        public bool HasManyNonAsciiCharactersBeforePosition { get; set; }
    }
}