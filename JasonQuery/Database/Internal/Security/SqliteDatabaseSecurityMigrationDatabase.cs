using JasonQuery.Core.Security.JasonQueryDb;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace JasonQuery.Database.Internal.Security
{
    public sealed class SqliteDatabaseSecurityMigrationDatabase : IJasonQueryDbMigrationDatabase
    {
        public bool CanOpen(string databaseFilePath, string databasePassword)
        {
            ValidateArguments(databaseFilePath, databasePassword);

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

        public void CreateVerifiedCopy(string sourceDatabaseFilePath, string destinationDatabaseFilePath, string databasePassword)
        {
            ValidateArguments(sourceDatabaseFilePath, databasePassword);

            if (string.IsNullOrWhiteSpace(destinationDatabaseFilePath))
            {
                throw new ArgumentException("A destination database file path is required.", nameof(destinationDatabaseFilePath));
            }

            if (!File.Exists(sourceDatabaseFilePath))
            {
                throw new FileNotFoundException("The source JasonQuery database file was not found.", sourceDatabaseFilePath);
            }

            if (File.Exists(destinationDatabaseFilePath))
            {
                throw new IOException("The destination database copy already exists.");
            }

            PrepareDatabaseForFileCopy(sourceDatabaseFilePath, databasePassword);
            File.Copy(sourceDatabaseFilePath, destinationDatabaseFilePath, false);

            try
            {
                if (!CanOpen(destinationDatabaseFilePath, databasePassword))
                {
                    throw new InvalidDataException("The copied JasonQuery database could not be validated.");
                }
            }
            catch
            {
                if (File.Exists(destinationDatabaseFilePath))
                {
                    File.Delete(destinationDatabaseFilePath);
                }

                throw;
            }
        }

        public void ChangePassword(string databaseFilePath, string currentDatabasePassword, string newDatabasePassword)
        {
            ValidateArguments(databaseFilePath, currentDatabasePassword);

            if (string.IsNullOrWhiteSpace(newDatabasePassword))
            {
                throw new ArgumentException("A new database password is required.", nameof(newDatabasePassword));
            }

            using (var connection = OpenConnection(databaseFilePath, currentDatabasePassword))
            using (var command = new SQLiteCommand("SELECT 1 FROM SystemConfig WHERE 1 = 2", connection))
            {
                command.ExecuteScalar();
                connection.ChangePassword(newDatabasePassword);
            }
        }

        private static void PrepareDatabaseForFileCopy(string databaseFilePath, string databasePassword)
        {
            using (var connection = OpenConnection(databaseFilePath, databasePassword))
            using (var validationCommand = new SQLiteCommand("SELECT 1 FROM SystemConfig WHERE 1 = 2", connection))
            {
                validationCommand.ExecuteScalar();

                using (var checkpointCommand = new SQLiteCommand("PRAGMA wal_checkpoint(TRUNCATE);", connection))
                {
                    checkpointCommand.ExecuteNonQuery();
                }
            }

            SQLiteConnection.ClearAllPools();
        }

        private static SQLiteConnection OpenConnection(string databaseFilePath, string databasePassword)
        {
            var connection = new SQLiteConnection
            {
                ConnectionString = $"Data Source={Path.GetFullPath(databaseFilePath)};Version=3;New=False;Compress=True;"
            };

            try
            {
                connection.SetPassword(databasePassword);
                connection.Open();

                if (connection.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException("The JasonQuery database connection did not open.");
                }

                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private static void ValidateArguments(string databaseFilePath, string databasePassword)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }

            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new ArgumentException("A database password is required.", nameof(databasePassword));
            }
        }
    }
}
