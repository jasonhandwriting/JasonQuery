using System;

namespace JasonQuery.UI.LeftPanel
{
    //L2B-2b: value contract only. In production, ONLY OracleReader may establish a verified scope from SYS_CONTEXT queried on its own opened OracleConnection. UI-provided strings are not proof.
    public sealed class OracleVerifiedSessionScope
    {
        public string SessionUser { get; set; }
        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
        public Guid ConnectionId { get; set; }
    }

    //L2B-2b: exact scope decisions; only OracleReader may supply authoritative live values.
    //The policy does not connect, query, commit, rollback, refresh metadata or execute DDL.
    public static class OracleVerifiedSessionScopePolicy
    {
        public static bool TryCreate(string sessionUser, string databaseName, string containerName, Guid connectionId, out OracleVerifiedSessionScope verifiedScope)
        {
            verifiedScope = null;

            if (string.IsNullOrWhiteSpace(sessionUser) || string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(containerName) || connectionId == Guid.Empty)
            {
                return false;
            }

            verifiedScope = new OracleVerifiedSessionScope
            {
                SessionUser = sessionUser,
                DatabaseName = databaseName,
                ContainerName = containerName,
                ConnectionId = connectionId
            };

            return true;
        }

        public static bool MatchesLogicalScope(OracleVerifiedSessionScope current, string expectedUser, string expectedDatabase, string expectedContainer)
        {
            return current != null
                   && current.ConnectionId != Guid.Empty
                   && !string.IsNullOrWhiteSpace(current.SessionUser)
                   && !string.IsNullOrWhiteSpace(current.DatabaseName)
                   && !string.IsNullOrWhiteSpace(current.ContainerName)
                   && !string.IsNullOrWhiteSpace(expectedUser)
                   && !string.IsNullOrWhiteSpace(expectedDatabase)
                   && !string.IsNullOrWhiteSpace(expectedContainer)
                   && string.Equals(current.SessionUser, expectedUser, StringComparison.Ordinal)
                   && string.Equals(current.DatabaseName, expectedDatabase, StringComparison.Ordinal)
                   && string.Equals(current.ContainerName, expectedContainer, StringComparison.Ordinal);
        }

        public static bool IsNewVerifiedSession(OracleVerifiedSessionScope previous, OracleVerifiedSessionScope current)
        {
            return previous != null
                   && current != null
                   && MatchesLogicalScope(previous, previous.SessionUser, previous.DatabaseName, previous.ContainerName)
                   && MatchesLogicalScope(current, previous.SessionUser, previous.DatabaseName, previous.ContainerName)
                   && previous.ConnectionId != current.ConnectionId;
        }

        public static bool CanBindMetadataToSession(OracleVerifiedSessionScope current, Guid requiredConnectionId, string expectedUser, string expectedDatabase, string expectedContainer)
        {
            return requiredConnectionId != Guid.Empty
                   && MatchesLogicalScope(current, expectedUser, expectedDatabase, expectedContainer)
                   && current.ConnectionId == requiredConnectionId;
        }
    }
}
