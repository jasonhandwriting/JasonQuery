using JasonQuery.Core.Database.Connection;

namespace JasonQuery.Core.Database.Transactions.LockingQueries
{
    internal static class DatabaseLockingQueryExecutionPolicy
    {
        public static DatabaseLockingQueryExecutionDecision Evaluate(DataSourceType dataSourceType, string sql, bool pagingRequested)
        {
            var detection = DatabaseLockingQueryDetector.Detect(dataSourceType, sql);

            return new DatabaseLockingQueryExecutionDecision
            {
                Detection = detection,
                RequiresConfirmation = detection.IsLockingQuery,
                ShouldUsePagedExecution =
                    pagingRequested && !detection.IsLockingQuery,
                ShouldUseSinglePassReader =
                    detection.IsLockingQuery,
                ShouldKeepConnectionOpenAfterSuccess =
                    detection.IsLockingQuery,
                ShouldMarkPendingAfterSuccess =
                    detection.IsLockingQuery
            };
        }

        public static bool ShouldUseSinglePassReader(DataSourceType dataSourceType, string sql)
        {
            return DatabaseLockingQueryDetector.Detect(dataSourceType, sql).IsLockingQuery;
        }
    }
}
