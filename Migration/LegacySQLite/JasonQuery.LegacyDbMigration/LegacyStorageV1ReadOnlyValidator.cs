using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace JasonQuery.LegacyDbMigration
{
    /// <summary>
    /// Opens historical Storage V1 only for read validation.
    ///
    /// This component must never create, migrate, rekey, checkpoint, or otherwise
    /// modify the source JasonQuery.db. The modern parent process remains
    /// responsible for Storage V2 creation and migration orchestration.
    /// </summary>
    internal sealed class LegacyStorageV1ReadOnlyValidator
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public void Validate(string databaseFilePath, byte[] databasePasswordUtf8)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new InvalidDataException
                (
                    "The historical Storage V1 source path is invalid."
                );
            }

            if (databasePasswordUtf8 == null || databasePasswordUtf8.Length == 0)
            {
                throw new InvalidDataException
                (
                    "The historical Storage V1 database credential is missing."
                );
            }

            var fullPath = Path.GetFullPath(databaseFilePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException
                (
                    "The historical Storage V1 source database was not found."
                );
            }

            var databaseBefore = FileSnapshot.Capture(fullPath);
            var walPath = fullPath + "-wal";
            var shmPath = fullPath + "-shm";
            var journalPath = fullPath + "-journal";
            var walBefore = FileSnapshot.Capture(walPath);
            var shmBefore = FileSnapshot.Capture(shmPath);
            var journalBefore = FileSnapshot.Capture(journalPath);

            try
            {
                StrictUtf8.GetCharCount(databasePasswordUtf8);

                using (var connection = CreateReadOnlyConnection(fullPath, databasePasswordUtf8))
                using (var command = new SQLiteCommand("SELECT 1 FROM SystemConfig WHERE 1 = 2", connection))
                {
                    command.ExecuteScalar();
                }
            }
            finally
            {
                try
                {
                    SQLiteConnection.ClearAllPools();
                }
                finally
                {
                    var databaseAfter = FileSnapshot.Capture(fullPath);
                    var walAfter = FileSnapshot.Capture(walPath);
                    var shmAfter = FileSnapshot.Capture(shmPath);
                    var journalAfter = FileSnapshot.Capture(journalPath);

                    EnsureUnchanged
                    (
                        databaseBefore,
                        databaseAfter,
                        "historical Storage V1 source database"
                    );

                    EnsureUnchanged
                    (
                        walBefore,
                        walAfter,
                        "historical Storage V1 WAL sidecar"
                    );

                    EnsureUnchanged
                    (
                        shmBefore,
                        shmAfter,
                        "historical Storage V1 SHM sidecar"
                    );

                    EnsureUnchanged
                    (
                        journalBefore,
                        journalAfter,
                        "historical Storage V1 rollback-journal sidecar"
                    );
                }
            }
        }

        private static SQLiteConnection CreateReadOnlyConnection(string databaseFilePath, byte[] databasePasswordUtf8)
        {
            var connectionStringBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = databaseFilePath,
                Version = 3,
                ReadOnly = true,
                FailIfMissing = true,
                Pooling = false
            };

            var connection = new SQLiteConnection
            {
                ConnectionString = connectionStringBuilder.ConnectionString
            };

            try
            {
                connection.SetPassword(databasePasswordUtf8);
                connection.Open();

                if (connection.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException
                    (
                        "The historical Storage V1 database did not open."
                    );
                }

                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private static void EnsureUnchanged(FileSnapshot before, FileSnapshot after, string description)
        {
            if (before.Exists != after.Exists)
            {
                throw new InvalidDataException
                (
                    $"The {description} existence changed during read-only validation."
                );
            }

            if (!before.Exists)
            {
                return;
            }

            if (before.Length != after.Length || before.LastWriteTimeUtc != after.LastWriteTimeUtc)
            {
                throw new InvalidDataException
                (
                    $"The {description} changed during read-only validation."
                );
            }
        }

        private sealed class FileSnapshot
        {
            private FileSnapshot(bool exists, long length, DateTime lastWriteTimeUtc)
            {
                Exists = exists;
                Length = length;
                LastWriteTimeUtc = lastWriteTimeUtc;
            }

            public bool Exists { get; }

            public long Length { get; }

            public DateTime LastWriteTimeUtc { get; }

            public static FileSnapshot Capture(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    return new FileSnapshot
                    (
                        false,
                        0,
                        DateTime.MinValue
                    );
                }

                var fileInfo = new FileInfo(filePath);

                return new FileSnapshot
                (
                    true,
                    fileInfo.Length,
                    fileInfo.LastWriteTimeUtc
                );
            }
        }
    }
}
