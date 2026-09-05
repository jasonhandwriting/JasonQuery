using System.IO;

namespace JasonQuery.Core.Security.Database
{
    public interface IDatabaseSecurityFreshInstallDatabase
    {
        void CreateEncryptedDatabase(Stream plaintextTemplateStream, string destinationDatabaseFilePath, string databasePassword);

        bool CanOpen(string databaseFilePath, string databasePassword);

        bool CanOpenWithoutPassword(string databaseFilePath);
    }
}
