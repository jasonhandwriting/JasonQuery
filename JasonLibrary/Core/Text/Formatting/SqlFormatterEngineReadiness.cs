namespace JasonLibrary.Core.Text.Formatting
{
    public enum SqlFormatterEngineReadiness
    {
        Unknown = 0,
        Ready,
        RequiresSafetyValidation,
        Rejected
    }
}
