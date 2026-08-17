using JasonLibrary.Core.Database.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JasonLibrary.Core.Text.Formatting
{
    public sealed class SqlFormatterEngineDescriptor
    {
        private readonly ReadOnlyCollection<DatabaseProviderKind> _supportedProviders;
        private readonly ReadOnlyCollection<DatabaseProviderKind> _plannedDefaultProviders;

        public SqlFormatterEngineDescriptor(SqlFormatterEngineKind kind, string displayName, string licenseIdentifier, SqlFormatterEngineReadiness readiness,
                                            IEnumerable<DatabaseProviderKind> supportedProviders, IEnumerable<DatabaseProviderKind> plannedDefaultProviders, string assessmentNote)
        {
            Kind = kind;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            LicenseIdentifier = licenseIdentifier ?? throw new ArgumentNullException(nameof(licenseIdentifier));
            Readiness = readiness;
            AssessmentNote = assessmentNote ?? string.Empty;

            _supportedProviders = new ReadOnlyCollection<DatabaseProviderKind>
            (
                (supportedProviders ?? throw new ArgumentNullException(nameof(supportedProviders)))
                .Distinct()
                .ToList()
            );

            _plannedDefaultProviders = new ReadOnlyCollection<DatabaseProviderKind>
            (
                (plannedDefaultProviders ?? throw new ArgumentNullException(nameof(plannedDefaultProviders)))
                .Distinct()
                .ToList()
            );
        }

        public SqlFormatterEngineKind Kind { get; }

        public string DisplayName { get; }

        public string LicenseIdentifier { get; }

        public SqlFormatterEngineReadiness Readiness { get; }

        public string AssessmentNote { get; }

        public IReadOnlyList<DatabaseProviderKind> SupportedProviders => _supportedProviders;

        public IReadOnlyList<DatabaseProviderKind> PlannedDefaultProviders => _plannedDefaultProviders;

        public bool Supports(DatabaseProviderKind providerKind)
        {
            return _supportedProviders.Contains(providerKind);
        }

        public bool IsPlannedDefaultFor(DatabaseProviderKind providerKind)
        {
            return _plannedDefaultProviders.Contains(providerKind);
        }
    }
}