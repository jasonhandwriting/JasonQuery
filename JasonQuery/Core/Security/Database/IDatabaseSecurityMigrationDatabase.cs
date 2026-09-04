using System;

namespace JasonQuery.Core.Security.Database
{
    public interface IDatabaseSecurityMigrationDatabase
    {
        bool CanOpen(string databaseFilePath, string databasePassword);

        void CreateVerifiedCopy(string sourceDatabaseFilePath, string destinationDatabaseFilePath, string databasePassword);

        void ChangePassword(string databaseFilePath, string currentDatabasePassword, string newDatabasePassword);
    }
}
