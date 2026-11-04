using System;
using System.Data;

namespace JasonQuery.UI.LeftPanel
{
    //Issued only by the successful Oracle Atomic Loader; UI callers cannot construct or mutate it.
    //The logical connection revision is intentionally not authenticated until B2c-3C.
    internal sealed class SharedOracleMetadataCompletedLoadReceipt
    {
        internal SharedOracleMetadataCompletedLoadReceipt(DataTable sourceTable, Guid connectionId, string sessionUser, string databaseName, string containerName, Guid completedLoadId, string logicalScopeKey)
        {
            if (!HasRawOracleIdentifiers(sourceTable) || connectionId == Guid.Empty || completedLoadId == Guid.Empty || string.IsNullOrWhiteSpace(sessionUser) || string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(containerName))
            {
                throw new InvalidOperationException("Incomplete Oracle metadata cannot issue a completed-load receipt.");
            }

            SourceTable = sourceTable;
            ConnectionId = connectionId;
            SessionUser = sessionUser;
            DatabaseName = databaseName;
            ContainerName = containerName;
            CompletedLoadId = completedLoadId;
            LogicalScopeKey = logicalScopeKey ?? string.Empty;
        }

        internal DataTable SourceTable { get; }
        internal Guid ConnectionId { get; }
        internal string SessionUser { get; }
        internal string DatabaseName { get; }
        internal string ContainerName { get; }
        internal Guid CompletedLoadId { get; }
        internal string LogicalScopeKey { get; }

        //Zero is deliberate: an OracleReader physical session is not the MainForm logical revision.
        //B2c-3C must correlate a trusted revision BEFORE enabling qualified capture.
        internal int TrustedLogicalConnectionRevision => 0;

        internal static bool HasRawOracleIdentifiers(DataTable table)
        {
            return table != null
                   && table.Columns.Contains("OracleMetadataOwner")
                   && table.Columns.Contains("OracleMetadataTableName")
                   && table.Columns.Contains("OracleMetadataColumnName");
        }
    }
}
