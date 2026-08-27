using JasonLibrary.Core.Database.Enums;
using System;

//統一建立 request、執行 engine，並在失敗時保留原始 SQL
//SQL Server ScriptDOM 與 PostgreSQL Hogimn 的 `UNION ALL` 空白行回歸測試
namespace JasonLibrary.Core.Text.Formatting
{
    public sealed class SqlFormatterCoordinator
    {
        private readonly SqlFormatterEngineResolver _engineResolver;

        public SqlFormatterCoordinator() : this(new SqlFormatterEngineResolver())
        {
        }

        public SqlFormatterCoordinator(SqlFormatterEngineResolver engineResolver)
        {
            _engineResolver = engineResolver ?? throw new ArgumentNullException(nameof(engineResolver));
        }

        public SqlFormatResult Format(string sql, DatabaseProviderKind providerKind,
                                      SqlFormatterEngineKind requestedEngineKind = SqlFormatterEngineKind.Unknown,
                                      SqlFormatOptions options = null)
        {
            return Format(new SqlFormatRequest(sql, providerKind, requestedEngineKind, options));
        }

        public SqlFormatResult Format(SqlFormatRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!_engineResolver.TryResolve(request.ProviderKind, request.EngineKind, out var engine, out var errorMessage))
            {
                return SqlFormatResult.Failed(request.EngineKind, request.Sql, errorMessage);
            }

            try
            {
                var resolvedRequest = new SqlFormatRequest
                (
                    request.Sql,
                    request.ProviderKind,
                    engine.Kind,
                    request.Options
                );

                var result = engine.Format(resolvedRequest);

                if (result == null)
                {
                    return SqlFormatResult.Failed
                    (
                        engine.Kind,
                        request.Sql,
                        "The formatter engine returned no result."
                    );
                }

                if (result.EngineKind != engine.Kind)
                {
                    return SqlFormatResult.Failed
                    (
                        engine.Kind,
                        request.Sql,
                        "The formatter engine returned a result for a different engine kind."
                    );
                }

                if (!result.Success)
                {
                    return SqlFormatResult.Failed(engine.Kind, request.Sql, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception exception)
            {
                return SqlFormatResult.Failed
                (
                    engine.Kind,
                    request.Sql,
                    "The formatter engine could not complete the request: " + exception.Message
                );
            }
        }
    }
}
