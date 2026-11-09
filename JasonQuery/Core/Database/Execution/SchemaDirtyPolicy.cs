using System;

namespace JasonQuery.Core.Database.Execution
{
    //F2-C2: a schema change remains dirty until a confirmed metadata refresh.
    internal static class SchemaDirtyPolicy
    {
        internal static SchemaDirtyDecision Evaluate(bool alreadyDirty, bool verifiedStructuralDdl, bool transactionClosed, bool refreshSucceeded)
        {
            var dirty = alreadyDirty || verifiedStructuralDdl;

            if (dirty && transactionClosed && refreshSucceeded)
            {
                dirty = false;
            }

            return new SchemaDirtyDecision
            {
                IsDirty = dirty,
                ShouldRefresh = dirty && transactionClosed && !refreshSucceeded
            };
        }

        //F2-C2-F3B: no DB work. Only a verified, same-scope transaction rollback may restore the trusted pre-DDL F7 snapshot without reloading metadata.
        internal static bool ShouldSkipMetadataReloadForConfirmedRollback(Connection.DataSourceType sourceType, string completedAction, bool transactionSucceeded, bool hasTransactionBoundDirty, bool metadataTrusted, bool sameDatabaseScope, bool otherRefreshRequired)
        {
            return (sourceType == Connection.DataSourceType.PostgreSql || sourceType == Connection.DataSourceType.SqlServer)
                   && string.Equals(completedAction?.Trim(), "ROLLBACK", StringComparison.OrdinalIgnoreCase)
                   && transactionSucceeded
                   && hasTransactionBoundDirty
                   && metadataTrusted
                   && sameDatabaseScope
                   && !otherRefreshRequired;
        }
    }

    internal sealed class SchemaDirtyDecision
    {
        public bool IsDirty { get; set; }
        public bool ShouldRefresh { get; set; }
    }
}
