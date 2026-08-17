using JasonLibrary.Core.Database.Enums;
using System;

namespace JasonLibrary.Core.Text.Formatting
{
    public sealed class SqlFormatRequest
    {
        public SqlFormatRequest(string sql, DatabaseProviderKind providerKind, SqlFormatterEngineKind engineKind, SqlFormatOptions options = null)
        {
            Sql = sql ?? throw new ArgumentNullException(nameof(sql));
            ProviderKind = providerKind;
            EngineKind = engineKind;
            Options = options ?? new SqlFormatOptions();
        }

        public string Sql { get; }

        public DatabaseProviderKind ProviderKind { get; }

        public SqlFormatterEngineKind EngineKind { get; }

        public SqlFormatOptions Options { get; }
    }
}