using JasonLibrary.Core.Database.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JasonLibrary.Core.Text.Formatting
{
    public static class SqlFormatterSupportCatalog
    {
        private static readonly ReadOnlyCollection<SqlFormatterEngineDescriptor> _engines
                                = new ReadOnlyCollection<SqlFormatterEngineDescriptor>
        (
            new[]
            {
                new SqlFormatterEngineDescriptor
                (
                    SqlFormatterEngineKind.MicrosoftScriptDom,
                    "Microsoft SQL ScriptDOM",
                    "MIT",
                    SqlFormatterEngineReadiness.Ready,
                    new[] { DatabaseProviderKind.SqlServer },
                    new[] { DatabaseProviderKind.SqlServer },
                    "The SQL Server candidate passed the Step 1 T-SQL formatting checks."
                ),

                new SqlFormatterEngineDescriptor
                (
                    SqlFormatterEngineKind.Hogimn,
                    "Hogimn SQL Formatter",
                    "MIT",
                    SqlFormatterEngineReadiness.Ready,
                    new[]
                    {
                        DatabaseProviderKind.Oracle,
                        DatabaseProviderKind.PostgreSql,
                        DatabaseProviderKind.SqlServer,
                        DatabaseProviderKind.MySql,
                        DatabaseProviderKind.Sqlite
                    },
                    new[]
                    {
                        DatabaseProviderKind.Oracle,
                        DatabaseProviderKind.PostgreSql,
                        DatabaseProviderKind.MySql,
                        DatabaseProviderKind.Sqlite
                    },
                    "Ready behind the dialect-aware token-preservation safety gate; unsafe output falls back to the original SQL."
                ),

                new SqlFormatterEngineDescriptor
                (
                    SqlFormatterEngineKind.Laan,
                    "Laan SQL Formatter",
                    "BSD-3-Clause",
                    SqlFormatterEngineReadiness.Rejected,
                    new[] { DatabaseProviderKind.SqlServer },
                    new DatabaseProviderKind[0],
                    "Rejected after producing invalid output for supported T-SQL scenarios."
                ),

                new SqlFormatterEngineDescriptor
                (
                    SqlFormatterEngineKind.TSqlSharp,
                    "TSqlSharp Formatter",
                    "MIT",
                    SqlFormatterEngineReadiness.Rejected,
                    new[] { DatabaseProviderKind.SqlServer },
                    new DatabaseProviderKind[0],
                    "Rejected because version 0.0.8 targets .NET 9 instead of .NET Framework 4.8."
                )
            }
        );

        public static IReadOnlyList<SqlFormatterEngineDescriptor> Engines => _engines;

        public static SqlFormatterEngineDescriptor Get(SqlFormatterEngineKind engineKind)
        {
            return _engines.FirstOrDefault(engine => engine.Kind == engineKind);
        }

        public static IReadOnlyList<SqlFormatterEngineDescriptor> GetCandidates(DatabaseProviderKind providerKind)
        {
            return _engines.Where(engine => engine.Supports(providerKind))
                           .ToList()
                           .AsReadOnly();
        }

        public static IReadOnlyList<SqlFormatterEngineDescriptor> GetReadyEngines(DatabaseProviderKind providerKind)
        {
            return _engines.Where(engine => engine.Readiness == SqlFormatterEngineReadiness.Ready)
                           .Where(engine => engine.Supports(providerKind))
                           .ToList()
                           .AsReadOnly();
        }

        public static SqlFormatterEngineDescriptor GetPlannedDefault(DatabaseProviderKind providerKind)
        {
            return _engines.FirstOrDefault(engine => engine.IsPlannedDefaultFor(providerKind));
        }
    }
}
