namespace JasonLibrary.Core.Text.Formatting
{
    public sealed class SqlTokenSemanticValidationResult
    {
        private SqlTokenSemanticValidationResult(bool isSafe, int tokenIndex, string originalToken,
                                                 string formattedToken, string errorMessage)
        {
            IsSafe = isSafe;
            TokenIndex = tokenIndex;
            OriginalToken = originalToken ?? string.Empty;
            FormattedToken = formattedToken ?? string.Empty;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        public bool IsSafe { get; }

        public int TokenIndex { get; }

        public string OriginalToken { get; }

        public string FormattedToken { get; }

        public string ErrorMessage { get; }

        internal static SqlTokenSemanticValidationResult Safe()
        {
            return new SqlTokenSemanticValidationResult(true, -1, string.Empty, string.Empty, string.Empty);
        }

        internal static SqlTokenSemanticValidationResult Unsafe(int tokenIndex, string originalToken,
                                                                 string formattedToken, string errorMessage)
        {
            return new SqlTokenSemanticValidationResult(false, tokenIndex, originalToken, formattedToken, errorMessage);
        }
    }
}
