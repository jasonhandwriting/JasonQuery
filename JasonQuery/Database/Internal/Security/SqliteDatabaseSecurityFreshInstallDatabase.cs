using JasonQuery.Core.Security.Database;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace JasonQuery.Database.Internal.Security
{
    public sealed class SqliteDatabaseSecurityFreshInstallDatabase : IDatabaseSecurityFreshInstallDatabase
    {
        public void CreateEncryptedDatabase(Stream plaintextTemplateStream, string destinationDatabaseFilePath, string databasePassword)
        {
            if (plaintextTemplateStream == null)
            {
                throw new ArgumentNullException(nameof(plaintextTemplateStream));
            }

            if (!plaintextTemplateStream.CanRead)
            {
                throw new ArgumentException("The JasonQuery database template stream must be readable.", nameof(plaintextTemplateStream));
            }

            ValidateDatabaseFilePath(destinationDatabaseFilePath);
            ValidateDatabasePassword(databasePassword);

            if (File.Exists(destinationDatabaseFilePath))
            {
                throw new IOException("The fresh-install database destination already exists.");
            }

            try
            {
                CopyTemplateDurably(plaintextTemplateStream, destinationDatabaseFilePath);

                if (!CanOpenWithoutPassword(destinationDatabaseFilePath))
                {
                    throw new InvalidDataException
                    (
                        "The embedded JasonQuery database template is not a valid plaintext template."
                    );
                }

                using (var connection = OpenConnectionWithoutPassword(destinationDatabaseFilePath))
                using (var command = new SQLiteCommand("SELECT 1 FROM SystemConfig WHERE 1 = 2", connection))
                {
                    command.ExecuteScalar();
                    connection.ChangePassword(databasePassword);
                }

                SQLiteConnection.ClearAllPools();

                if (!CanOpen(destinationDatabaseFilePath, databasePassword))
                {
                    throw new InvalidDataException
                    (
                        "The fresh JasonQuery database could not be validated after applying its generated database key."
                    );
                }

                if (CanOpenWithoutPassword(destinationDatabaseFilePath))
                {
                    throw new InvalidDataException
                    (
                        "The fresh JasonQuery database remained accessible without its generated database key."
                    );
                }
            }
            catch
            {
                SQLiteConnection.ClearAllPools();

                if (File.Exists(destinationDatabaseFilePath))
                {
                    File.Delete(destinationDatabaseFilePath);
                }

                throw;
            }
        }

        public bool CanOpen(string databaseFilePath, string databasePassword)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            ValidateDatabasePassword(databasePassword);

            try
            {
                using (var connection = OpenConnection(databaseFilePath, databasePassword))
                using (var command = new SQLiteCommand("SELECT 1 FROM SystemConfig WHERE 1 = 2", connection))
                {
                    command.ExecuteScalar();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CanOpenWithoutPassword(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            try
            {
                using (var connection = OpenConnectionWithoutPassword(databaseFilePath))
                using (var command = new SQLiteCommand("SELECT 1 FROM SystemConfig WHERE 1 = 2", connection))
                {
                    command.ExecuteScalar();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void CopyTemplateDurably(Stream plaintextTemplateStream, string destinationDatabaseFilePath)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(destinationDatabaseFilePath));

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (plaintextTemplateStream.CanSeek)
            {
                plaintextTemplateStream.Position = 0;
            }

            using (var fileStream = new FileStream(destinationDatabaseFilePath, FileMode.CreateNew, FileAccess.Write,
                                                   FileShare.None, 81920, FileOptions.WriteThrough))
            {
                plaintextTemplateStream.CopyTo(fileStream);
                fileStream.Flush(true);
            }
        }

        private static SQLiteConnection OpenConnection(string databaseFilePath, string databasePassword)
        {
            var connection = CreateConnection(databaseFilePath);

            try
            {
                connection.SetPassword(databasePassword);
                connection.Open();
                ValidateOpenConnection(connection);

                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private static SQLiteConnection OpenConnectionWithoutPassword(string databaseFilePath)
        {
            var connection = CreateConnection(databaseFilePath);

            try
            {
                connection.Open();
                ValidateOpenConnection(connection);

                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private static SQLiteConnection CreateConnection(string databaseFilePath)
        {
            return new SQLiteConnection
            {
                ConnectionString = $"Data Source={Path.GetFullPath(databaseFilePath)};Version=3;New=False;Compress=True;"
            };
        }

        private static void ValidateOpenConnection(SQLiteConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("The JasonQuery database connection did not open.");
            }
        }

        private static void ValidateDatabaseFilePath(string databaseFilePath)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }
        }

        private static void ValidateDatabasePassword(string databasePassword)
        {
            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new ArgumentException("A database password is required.", nameof(databasePassword));
            }
        }
    }
}
