using JasonQuery.Core.Database.Connection;
using System;
using System.Data;

namespace JasonQuery.UI.LeftPanel
{
    //L2B-2a Golden Red: source evidence only. This type does not query Oracle or publish metadata.
    public enum SharedOracleMetadataCaptureKind
    {
        Unknown,
        AuthoritativeCompletedLoad,
        PreviewRebind,
        PartialLoad
    }

    //Provenance must be produced by the future trusted loader, never synthesized from a live Reader at UI bind time.
    //Settable properties are test contract inputs, not independent authentication evidence.
    public sealed class SharedOracleMetadataProvenance
    {
        public DataTable CapturedSourceTable { get; set; }
        public DataSourceType SourceType { get; set; }
        public int ConnectionRevision { get; set; }
        public Guid ConnectionId { get; set; }
        public string VerifiedSessionUser { get; set; }
        public string VerifiedDatabaseName { get; set; }
        public string VerifiedContainerName { get; set; }
        public Guid CompletedLoadId { get; set; }
        public SharedOracleMetadataCaptureKind CaptureKind { get; set; }
        public bool LoadCompletedSuccessfully { get; set; }
    }

    public sealed class SharedOracleMetadataValidationContext
    {
        public bool IsCurrentLogicalSnapshot { get; set; }
        public bool IsCurrentMenuAndTab { get; set; }
        public bool IsExplicitRecoveryClick { get; set; }
        public bool HasPendingOrUnknownTransaction { get; set; }
        public bool IsVerifiedLiveOracleSession { get; set; }
        public bool IsNewDeliberateActionClick { get; set; }
        public bool IsOriginalMenuReplay { get; set; }

        public int CurrentConnectionRevision { get; set; }
        public Guid PreviousConnectionId { get; set; }
        public Guid CurrentConnectionId { get; set; }
        public string CurrentVerifiedSessionUser { get; set; }
        public string CurrentVerifiedDatabaseName { get; set; }
        public string CurrentVerifiedContainerName { get; set; }
        public string ExpectedSessionUser { get; set; }
        public string ExpectedDatabaseName { get; set; }
        public string ExpectedContainerName { get; set; }

        public DataTable CandidateSchemaSource { get; set; }
        public SharedOracleMetadataProvenance Provenance { get; set; }
        public bool ExactRawTableOwnerAndNameReResolved { get; set; }
        public bool ExactRawColumnOwnerTableAndNameReResolved { get; set; }
    }

    //L2B-2a: pure consistency policy. It does not connect to Oracle or publish metadata.
    //In L2B-2b/c the calling code must independently prove database/container identity and trusted load completion.
    public static class SharedOracleMetadataProvenancePolicy
    {
        public static bool CanReadOldCache(SharedOracleMetadataValidationContext context)
        {
            return context != null && context.IsCurrentLogicalSnapshot;
        }

        public static bool CanRequestExplicitRecovery(SharedOracleMetadataValidationContext context)
        {
            return context != null
                   && context.IsCurrentLogicalSnapshot
                   && context.IsCurrentMenuAndTab
                   && context.IsExplicitRecoveryClick
                   && !context.HasPendingOrUnknownTransaction
                   && !context.IsVerifiedLiveOracleSession
                   && context.CurrentConnectionRevision > 0;
        }

        public static bool HasNewPhysicalConnectionIdentity(SharedOracleMetadataValidationContext context)
        {
            return context != null
                   && context.IsVerifiedLiveOracleSession
                   && context.PreviousConnectionId != Guid.Empty
                   && context.CurrentConnectionId != Guid.Empty
                   && context.PreviousConnectionId != context.CurrentConnectionId;
        }

        public static bool CanPublishFreshMetadata(SharedOracleMetadataValidationContext context)
        {
            var source = context?.CandidateSchemaSource;
            var provenance = context?.Provenance;

            //These are consistency checks on trusted-loader evidence, not proof that the UI produced it.
            //The authoritative loader must originate the evidence on the SAME verified connection (L2B-2b/c).
            return context != null
                   && context.IsCurrentLogicalSnapshot
                   && !context.HasPendingOrUnknownTransaction
                   && context.IsVerifiedLiveOracleSession
                   && context.CurrentConnectionRevision > 0
                   && context.CurrentConnectionId != Guid.Empty
                   && provenance != null
                   && source != null
                   && ReferenceEquals(source, provenance.CapturedSourceTable)
                   && source.Columns.Contains("OracleMetadataOwner")
                   && source.Columns.Contains("OracleMetadataTableName")
                   && source.Columns.Contains("OracleMetadataColumnName")
                   && provenance.SourceType == DataSourceType.Oracle
                   && provenance.CaptureKind == SharedOracleMetadataCaptureKind.AuthoritativeCompletedLoad
                   && provenance.LoadCompletedSuccessfully
                   && provenance.CompletedLoadId != Guid.Empty
                   && provenance.ConnectionRevision == context.CurrentConnectionRevision
                   && provenance.ConnectionId == context.CurrentConnectionId
                   && !string.IsNullOrWhiteSpace(context.CurrentVerifiedSessionUser)
                   && !string.IsNullOrWhiteSpace(context.CurrentVerifiedDatabaseName)
                   && !string.IsNullOrWhiteSpace(context.CurrentVerifiedContainerName)
                   && string.Equals(provenance.VerifiedSessionUser, context.CurrentVerifiedSessionUser, StringComparison.Ordinal)
                   && string.Equals(provenance.VerifiedDatabaseName, context.CurrentVerifiedDatabaseName, StringComparison.Ordinal)
                   && string.Equals(provenance.VerifiedContainerName, context.CurrentVerifiedContainerName, StringComparison.Ordinal)
                   && string.Equals(context.ExpectedSessionUser, context.CurrentVerifiedSessionUser, StringComparison.Ordinal)
                   && string.Equals(context.ExpectedDatabaseName, context.CurrentVerifiedDatabaseName, StringComparison.Ordinal)
                   && string.Equals(context.ExpectedContainerName, context.CurrentVerifiedContainerName, StringComparison.Ordinal);
        }

        public static bool CanQualifyFreshTable(SharedOracleMetadataValidationContext context)
        {
            return CanPublishFreshMetadata(context) && context.IsCurrentMenuAndTab && context.ExactRawTableOwnerAndNameReResolved;
        }

        public static bool CanQualifyFreshColumn(SharedOracleMetadataValidationContext context)
        {
            return CanPublishFreshMetadata(context) && context.IsCurrentMenuAndTab && context.ExactRawColumnOwnerTableAndNameReResolved;
        }

        public static bool CanDispatchNewDeliberateAction(SharedOracleMetadataValidationContext context, bool isColumn)
        {
            return context != null
                   && context.IsNewDeliberateActionClick
                   && !context.IsOriginalMenuReplay
                   && context.IsCurrentMenuAndTab
                   && (isColumn ? CanQualifyFreshColumn(context) : CanQualifyFreshTable(context));
        }
    }
}
