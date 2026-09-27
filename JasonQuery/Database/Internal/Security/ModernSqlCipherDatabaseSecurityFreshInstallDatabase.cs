using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Database.Internal.Runtime.Modern;
using System;
using System.Data.Common;
using System.IO;

namespace JasonQuery.Database.Internal.Security
{
    public sealed class ModernSqlCipherDatabaseSecurityFreshInstallDatabase : IJasonQueryDbFreshInstallDatabase
    {
        private readonly ModernSqlCipherDatabaseRuntime _runtime;

        public ModernSqlCipherDatabaseSecurityFreshInstallDatabase() : this(new ModernSqlCipherDatabaseRuntime())
        {
        }

        internal ModernSqlCipherDatabaseSecurityFreshInstallDatabase(ModernSqlCipherDatabaseRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public int StorageFormatVersion => JasonQueryDbStorageFormatContract.ModernVersion;

        public void CreateEncryptedDatabase(Stream plaintextTemplateStream, string destinationDatabaseFilePath, string databasePassword)
        {
            if (plaintextTemplateStream == null)
            {
                throw new ArgumentNullException(nameof(plaintextTemplateStream));
            }

            if (!plaintextTemplateStream.CanRead)
            {
                throw new ArgumentException
                (
                    "The JasonQuery database template stream must be readable.",
                    nameof(plaintextTemplateStream)
                );
            }

            ValidateDatabaseFilePath(destinationDatabaseFilePath);
            ValidateDatabasePassword(databasePassword);

            if (File.Exists(destinationDatabaseFilePath))
            {
                throw new IOException("The fresh Storage V2 database destination already exists.");
            }

            var templateBytes = ReadTemplateBytes(plaintextTemplateStream);

            try
            {
                _runtime.CreateFreshDatabase
                (
                    destinationDatabaseFilePath,
                    databasePassword,
                    templateBytes
                );
            }
            finally
            {
                Array.Clear(templateBytes, 0, templateBytes.Length);
            }
        }

        public bool CanOpen(string databaseFilePath, string databasePassword)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            ValidateDatabasePassword(databasePassword);

            return _runtime.CanOpenDatabase
            (
                CreateConnectionString(databaseFilePath),
                databasePassword
            );
        }

        public bool CanOpenWithoutPassword(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            return _runtime.CanOpenWithoutKey(databaseFilePath);
        }

        private static byte[] ReadTemplateBytes(Stream plaintextTemplateStream)
        {
            using (var buffer = new MemoryStream())
            {
                var chunk = new byte[81920];

                try
                {
                    while (true)
                    {
                        var read = plaintextTemplateStream.Read(chunk, 0, chunk.Length);

                        if (read == 0)
                        {
                            break;
                        }

                        if (buffer.Length + read > ModernDatabaseRuntimeProtocol.MaxFreshTemplateBytes)
                        {
                            throw new InvalidDataException
                            (
                                "The embedded JasonQuery database template exceeds the qualified fresh-install protocol limit."
                            );
                        }

                        buffer.Write(chunk, 0, read);
                    }

                    if (buffer.Length == 0)
                    {
                        throw new InvalidDataException("The embedded JasonQuery database template is empty.");
                    }

                    return buffer.ToArray();
                }
                finally
                {
                    Array.Clear(chunk, 0, chunk.Length);
                }
            }
        }

        private static string CreateConnectionString(string databaseFilePath)
        {
            var builder = new DbConnectionStringBuilder
            {
                ["Data Source"] = Path.GetFullPath(databaseFilePath)
            };

            return builder.ConnectionString;
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
