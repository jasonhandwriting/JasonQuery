using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public interface IJasonQueryDbFreshInstallDatabase
    {
        int StorageFormatVersion { get; }

        void CreateEncryptedDatabase(Stream plaintextTemplateStream, string destinationDatabaseFilePath, string databasePassword);

        bool CanOpen(string databaseFilePath, string databasePassword);

        bool CanOpenWithoutPassword(string databaseFilePath);
    }
}
