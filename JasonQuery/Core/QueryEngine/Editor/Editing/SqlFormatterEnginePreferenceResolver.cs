using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal sealed class SqlFormatterEngineChoice
    {
        public SqlFormatterEngineChoice(SqlFormatterEngineKind kind, string displayName, bool isRecommended)
        {
            Kind = kind;
            DisplayName = displayName ?? string.Empty;
            IsRecommended = isRecommended;
        }

        public SqlFormatterEngineKind Kind { get; }

        public string DisplayName { get; }

        public bool IsRecommended { get; }

        public bool SupportsMaxLineWidth => Kind == SqlFormatterEngineKind.Hogimn;

        public bool SupportsListItemsPerLine => Kind == SqlFormatterEngineKind.Hogimn || Kind == SqlFormatterEngineKind.MicrosoftScriptDom;
    }

    internal static class SqlFormatterEnginePreferenceResolver
    {
        public static SqlFormatterEngineKind Parse(DatabaseProviderKind providerKind, string storedValue)
        {
            if (!Enum.TryParse(storedValue, true, out SqlFormatterEngineKind engineKind))
            {
                return SqlFormatterEngineKind.Unknown;
            }

            return Normalize(providerKind, engineKind);
        }

        public static SqlFormatterEngineKind Normalize(DatabaseProviderKind providerKind, SqlFormatterEngineKind engineKind)
        {
            if (engineKind == SqlFormatterEngineKind.Unknown)
            {
                return SqlFormatterEngineKind.Unknown;
            }

            var descriptor = SqlFormatterSupportCatalog.Get(engineKind);

            return descriptor != null &&
                   descriptor.Readiness == SqlFormatterEngineReadiness.Ready &&
                   descriptor.Supports(providerKind)
                   ? engineKind
                   : SqlFormatterEngineKind.Unknown;
        }

        public static SqlFormatterEngineKind GetEffectiveEngine(DatabaseProviderKind providerKind, SqlFormatterEngineKind preferredEngineKind)
        {
            var normalized = Normalize(providerKind, preferredEngineKind);

            if (normalized != SqlFormatterEngineKind.Unknown)
            {
                return normalized;
            }

            var plannedDefault = SqlFormatterSupportCatalog.GetPlannedDefault(providerKind);

            return plannedDefault?.Kind ?? SqlFormatterEngineKind.Unknown;
        }

        public static IReadOnlyList<SqlFormatterEngineChoice> GetChoices(DatabaseProviderKind providerKind)
        {
            var plannedDefault = SqlFormatterSupportCatalog.GetPlannedDefault(providerKind);
            var readyEngines = SqlFormatterSupportCatalog.GetReadyEngines(providerKind);
            var showRecommendation = readyEngines.Count > 1;

            var choices = readyEngines.Select
                                       (
                                           engine =>
                                           {
                                               var isRecommended = engine.Kind == plannedDefault?.Kind;
                                               var displayName = engine.DisplayName;

                                               if (showRecommendation && isRecommended)
                                               {
                                                   displayName += " (Recommended)";
                                               }

                                               return new SqlFormatterEngineChoice
                                               (
                                                   engine.Kind,
                                                   displayName,
                                                   isRecommended
                                               );
                                           }
                                       )
                                      .ToList();

            return new ReadOnlyCollection<SqlFormatterEngineChoice>(choices);
        }
    }
}
