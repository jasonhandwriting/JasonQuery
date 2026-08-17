using JasonLibrary.Core.Database.Enums;

namespace JasonLibrary.Core.Text.Formatting
{
    public interface ISqlFormatterEngine
    {
        SqlFormatterEngineKind Kind { get; }

        bool Supports(DatabaseProviderKind providerKind);

        SqlFormatResult Format(SqlFormatRequest request);
    }
}