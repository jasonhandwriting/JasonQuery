using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting.Engines;
using System;
using System.Collections.Generic;

//解析預設或明確指定的 formatter engine
/*
| DatabaseProviderKind | 預設 engine |
| --- | --- |
| Oracle | Hogimn |
| PostgreSql | Hogimn |
| SqlServer | Microsoft ScriptDOM |
| MySql | Hogimn |
| Sqlite | Hogimn (供未來使用) |
 */
namespace JasonLibrary.Core.Text.Formatting
{
    public sealed class SqlFormatterEngineResolver
    {
        private readonly Dictionary<SqlFormatterEngineKind, ISqlFormatterEngine> _engines;

        public SqlFormatterEngineResolver() : this (new ISqlFormatterEngine[]{new MicrosoftScriptDomSqlFormatterEngine(), new HogimnSqlFormatterEngine()})
        {
        }

        public SqlFormatterEngineResolver(IEnumerable<ISqlFormatterEngine> engines)
        {
            if (engines == null)
            {
                throw new ArgumentNullException(nameof(engines));
            }

            _engines = new Dictionary<SqlFormatterEngineKind, ISqlFormatterEngine>();

            foreach (var engine in engines)
            {
                if (engine == null)
                {
                    throw new ArgumentException("The formatter engine collection cannot contain null entries.", nameof(engines));
                }

                if (_engines.ContainsKey(engine.Kind))
                {
                    throw new ArgumentException("Only one formatter engine can be registered for each engine kind.", nameof(engines));
                }

                _engines.Add(engine.Kind, engine);
            }
        }

        public bool TryResolve(DatabaseProviderKind providerKind, SqlFormatterEngineKind requestedEngineKind,
                               out ISqlFormatterEngine engine, out string errorMessage)
        {
            engine = null;
            errorMessage = string.Empty;

            var descriptor = requestedEngineKind == SqlFormatterEngineKind.Unknown
                             ? SqlFormatterSupportCatalog.GetPlannedDefault(providerKind)
                             : SqlFormatterSupportCatalog.Get(requestedEngineKind);

            if (descriptor == null)
            {
                errorMessage = requestedEngineKind == SqlFormatterEngineKind.Unknown
                               ? "No default formatter engine is configured for the requested database provider."
                               : "The requested formatter engine is not registered in the support catalog.";
                return false;
            }

            if (descriptor.Readiness != SqlFormatterEngineReadiness.Ready)
            {
                errorMessage = "The requested formatter engine is not ready for use.";
                return false;
            }

            if (!descriptor.Supports(providerKind))
            {
                errorMessage = "The requested formatter engine does not support the database provider.";
                return false;
            }

            if (!_engines.TryGetValue(descriptor.Kind, out var registeredEngine))
            {
                errorMessage = "The requested formatter engine implementation is not registered.";
                return false;
            }

            if (!registeredEngine.Supports(providerKind))
            {
                errorMessage = "The registered formatter engine does not support the database provider.";
                return false;
            }

            engine = registeredEngine;
            return true;
        }
    }
}