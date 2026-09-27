using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Database.Internal.Runtime.Modern;
using System;
using System.Data.Common;
using System.IO;

namespace JasonQuery.Database.Internal.Security
{
    internal static class StorageAwareDatabaseSecurityMigrationDatabaseFactory
    {
        internal static IJasonQueryDbMigrationDatabase Create(JasonQueryDbSecurityMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            metadata.Validate();

            var route = JasonQueryDbStorageRuntimeRoutingContract.Resolve(metadata.StorageFormatVersion);

            switch (route.RuntimeKind)
            {
                case JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite:
                    {
                        return new SqliteDatabaseSecurityMigrationDatabase();
                    }
                case JasonQueryDbStorageRuntimeKind.ModernSqlCipher:
                    {
                        return new ModernSqlCipherDatabaseSecurityMigrationDatabase();
                    }
                default:
                    {
                        throw new NotSupportedException
                        (
                            $"Storage runtime '{route.RuntimeKind}' is not supported for database-security transitions."
                        );
                    }
            }
        }
    }

    internal sealed class ModernSqlCipherDatabaseSecurityMigrationDatabase : IJasonQueryDbMigrationDatabase
    {
        public bool CanOpen(string databaseFilePath, string databasePassword)
        {
            ValidateDatabaseArguments(databaseFilePath, databasePassword);

            using (var runtime = CreateRuntime())
            {
                return runtime.CanOpenDatabase
                (
                    CreateConnectionString(databaseFilePath),
                    databasePassword
                );
            }
        }

        public void CreateVerifiedCopy(string sourceDatabaseFilePath, string destinationDatabaseFilePath, string databasePassword)
        {
            ValidateDatabaseArguments(sourceDatabaseFilePath, databasePassword);

            if (string.IsNullOrWhiteSpace(destinationDatabaseFilePath))
            {
                throw new ArgumentException
                (
                    "A destination database file path is required.",
                    nameof(destinationDatabaseFilePath)
                );
            }

            var sourcePath = Path.GetFullPath(sourceDatabaseFilePath);
            var destinationPath = Path.GetFullPath(destinationDatabaseFilePath);

            if (string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException
                (
                    "The destination database copy must differ from the source database.",
                    nameof(destinationDatabaseFilePath)
                );
            }

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException
                (
                    "The source JasonQuery database file was not found.",
                    sourcePath
                );
            }

            if (File.Exists(destinationPath))
            {
                throw new IOException("The destination database copy already exists.");
            }

            using (var runtime = CreateRuntime())
            {
                var sourceConnectionString = CreateConnectionString(sourcePath);

                if (!runtime.CanOpenDatabase(sourceConnectionString, databasePassword))
                {
                    throw new InvalidDataException
                    (
                        "The Storage V2 source database could not be validated before creating a security-transition copy."
                    );
                }

                runtime.ExecuteNonQuery
                (
                    sourceConnectionString,
                    databasePassword,
                    "PRAGMA wal_checkpoint(TRUNCATE);",
                    null
                );
            }

            File.Copy(sourcePath, destinationPath, false);

            try
            {
                using (var runtime = CreateRuntime())
                {
                    if (!runtime.CanOpenDatabase(CreateConnectionString(destinationPath), databasePassword))
                    {
                        throw new InvalidDataException
                        (
                            "The copied Storage V2 database could not be validated."
                        );
                    }
                }
            }
            catch
            {
                DeleteFileIfExists(destinationPath);
                throw;
            }
        }

        public void ChangePassword(string databaseFilePath, string currentDatabasePassword, string newDatabasePassword)
        {
            ValidateDatabaseArguments(databaseFilePath, currentDatabasePassword);

            if (string.IsNullOrWhiteSpace(newDatabasePassword))
            {
                throw new ArgumentException
                (
                    "A new Storage V2 database credential is required.",
                    nameof(newDatabasePassword)
                );
            }

            using (var runtime = CreateRuntime())
            {
                runtime.ChangePassword
                (
                    CreateConnectionString(databaseFilePath),
                    currentDatabasePassword,
                    newDatabasePassword
                );
            }
        }

        private static ModernSqlCipherDatabaseRuntime CreateRuntime()
        {
            ModernSqlCipherDatabaseRuntime runtime = null;

            try
            {
                runtime = new ModernSqlCipherDatabaseRuntime();
                runtime.ProbeQualifiedRuntime();
                return runtime;
            }
            catch
            {
                runtime?.Dispose();
                throw;
            }
        }

        private static string CreateConnectionString(string databaseFilePath)
        {
            var builder = new DbConnectionStringBuilder();

            builder["Data Source"] = Path.GetFullPath(databaseFilePath);
            return builder.ConnectionString;
        }

        private static void ValidateDatabaseArguments(string databaseFilePath, string databasePassword)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException
                (
                    "A database file path is required.",
                    nameof(databaseFilePath)
                );
            }

            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new ArgumentException
                (
                    "A database credential is required.",
                    nameof(databasePassword)
                );
            }
        }

        private static void DeleteFileIfExists(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
