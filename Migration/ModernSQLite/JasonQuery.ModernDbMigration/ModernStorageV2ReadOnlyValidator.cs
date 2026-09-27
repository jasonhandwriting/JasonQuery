using JasonQuery.Core.Security.JasonQueryDb;
using SQLitePCL;
using System;
using System.IO;
using System.Security.Cryptography;

namespace JasonQuery.ModernDbMigration
{
    /// <summary>
    /// Revalidates an existing encrypted Storage V2 candidate without opening it writable.
    /// </summary>
    internal sealed class ModernStorageV2ReadOnlyValidator
    {
        public void Validate(string databaseFilePath, byte[] databasePasswordUtf8)
        {
            ValidateRequest(databaseFilePath, databasePasswordUtf8);
            ModernSqlCipherRuntime.InitializeAndValidate();

            var fullPath = Path.GetFullPath(databaseFilePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException
                (
                    "The Storage V2 validation database was not found.",
                    fullPath
                );
            }

            EnsureNoSqliteSidecars(fullPath);

            var before = FileSnapshot.Capture(fullPath);
            sqlite3 db = null;

            try
            {
                db = ModernSqlCipherRuntime.OpenReadOnly
                (
                    fullPath,
                    databasePasswordUtf8
                );

                if (raw.sqlite3_db_readonly(db, "main") != 1)
                {
                    throw new InvalidDataException
                    (
                        "The Storage V2 validation database was not opened read-only."
                    );
                }

                ModernSqlCipherRuntime.Exec(db, "PRAGMA query_only = ON;");

                ModernSqlCipherRuntime.AssertEqual
                (
                    1L,
                    ModernSqlCipherRuntime.QueryInt64(db, "PRAGMA query_only;"),
                    "query_only"
                );

                ModernSqlCipherRuntime.ValidateFrozenProfile(db);
                ModernSqlCipherRuntime.ValidateIntegrity(db);
            }
            finally
            {
                ModernSqlCipherRuntime.CloseDatabase(db);

                var after = FileSnapshot.Capture(fullPath);

                if (!before.Equals(after))
                {
                    throw new InvalidDataException
                    (
                        "The Storage V2 candidate changed during read-only validation."
                    );
                }

                EnsureNoSqliteSidecars(fullPath);
            }
        }

        private static void ValidateRequest(string databaseFilePath, byte[] databasePasswordUtf8)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException
                (
                    "A Storage V2 database path is required.",
                    nameof(databaseFilePath)
                );
            }

            if (databasePasswordUtf8 == null || databasePasswordUtf8.Length <= 0 || databasePasswordUtf8.Length > JasonQueryDbStorageV2ReadOnlyValidatorProtocol.MaxDatabasePasswordUtf8Bytes)
            {
                throw new ArgumentException
                (
                    "A bounded database password is required.",
                    nameof(databasePasswordUtf8)
                );
            }
        }

        private static void EnsureNoSqliteSidecars(string databaseFilePath)
        {
            foreach (var suffix in new[] { "-journal", "-wal", "-shm" })
            {
                if (File.Exists(databaseFilePath + suffix) || Directory.Exists(databaseFilePath + suffix))
                {
                    throw new InvalidDataException
                    (
                        "The Storage V2 validation database has an active SQLite sidecar."
                    );
                }
            }
        }

        private sealed class FileSnapshot
        {
            private FileSnapshot(long length, DateTime lastWriteTimeUtc, byte[] sha256)
            {
                Length = length;
                LastWriteTimeUtc = lastWriteTimeUtc;
                Sha256 = sha256;
            }

            public long Length { get; }

            public DateTime LastWriteTimeUtc { get; }

            public byte[] Sha256 { get; }

            public static FileSnapshot Capture(string filePath)
            {
                var info = new FileInfo(filePath);

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var sha256 = SHA256.Create())
                {
                    return new FileSnapshot
                    (
                        info.Length,
                        info.LastWriteTimeUtc,
                        sha256.ComputeHash(stream)
                    );
                }
            }

            public bool Equals(FileSnapshot other)
            {
                if (other == null || Length != other.Length || LastWriteTimeUtc != other.LastWriteTimeUtc || Sha256.Length != other.Sha256.Length)
                {
                    return false;
                }

                var difference = 0;

                for (var index = 0; index < Sha256.Length; index++)
                {
                    difference |= Sha256[index] ^ other.Sha256[index];
                }

                return difference == 0;
            }
        }
    }
}
