using System;

namespace JasonQuery.Core.Database.Transactions
{
    /// <summary>
    /// Restores only the scope of an internal PostgreSQL metadata read.
    /// It never commits or rolls back the enclosing transaction.
    /// </summary>
    public static class PostgreSqlMetadataSavepointExecutor
    {
        public static T Execute<T>(Action createSavepoint, Func<T> executeQuery, Action rollbackToSavepoint, Action releaseSavepoint)
        {
            if (createSavepoint == null)
            {
                throw new ArgumentNullException(nameof(createSavepoint));
            }

            if (executeQuery == null)
            {
                throw new ArgumentNullException(nameof(executeQuery));
            }

            if (rollbackToSavepoint == null)
            {
                throw new ArgumentNullException(nameof(rollbackToSavepoint));
            }

            if (releaseSavepoint == null)
            {
                throw new ArgumentNullException(nameof(releaseSavepoint));
            }

            try
            {
                createSavepoint();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("PostgreSQL metadata savepoint creation failed; the metadata SQL was not executed. " + "JasonQuery did not roll back the enclosing transaction.", ex);
            }

            Exception failure = null;
            T result = default(T);
            var querySucceeded = false;

            try
            {
                result = executeQuery();
                querySucceeded = true;
            }
            catch (Exception ex)
            {
                failure = ex;
            }

            if (querySucceeded)
            {
                try
                {
                    releaseSavepoint();
                    return result;
                }
                catch (Exception ex)
                {
                    failure = new InvalidOperationException("Metadata savepoint release failed.", ex);
                }
            }

            try
            {
                rollbackToSavepoint();
            }
            catch (Exception rollbackError)
            {
                throw new AggregateException("PostgreSQL metadata failed and ROLLBACK TO SAVEPOINT also failed. " + "The enclosing transaction state is uncertain; resolve it manually. " + "JasonQuery has not issued a full ROLLBACK.", failure, rollbackError);
            }

            try
            {
                releaseSavepoint();
            }
            catch (Exception releaseError)
            {
                throw new AggregateException("PostgreSQL metadata was rolled back to its savepoint, but RELEASE SAVEPOINT failed. " + "The enclosing transaction requires manual review.", failure, releaseError);
            }

            throw new InvalidOperationException("PostgreSQL metadata query failed. The metadata savepoint was restored; " + "the enclosing transaction was not committed or rolled back.", failure);
        }
    }
}
