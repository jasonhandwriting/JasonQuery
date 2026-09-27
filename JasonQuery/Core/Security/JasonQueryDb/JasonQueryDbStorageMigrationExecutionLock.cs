using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Cross-process guard for the point-of-no-return portion of physical storage migration.
    /// A per-database named mutex is combined with a same-directory FileShare.None lock file.
    /// </summary>
    internal sealed class JasonQueryDbStorageMigrationExecutionLock : IDisposable
    {
        private const string MutexPrefix = "Local\\JasonQuery.DatabaseStorageMigration.";

        private readonly string _lockFilePath;
        private Mutex _mutex;
        private FileStream _lockStream;
        private bool _mutexOwned;
        private bool _disposed;

        private JasonQueryDbStorageMigrationExecutionLock(string lockFilePath, Mutex mutex, bool mutexOwned, FileStream lockStream)
        {
            _lockFilePath = lockFilePath;
            _mutex = mutex;
            _mutexOwned = mutexOwned;
            _lockStream = lockStream;
        }

        public string LockFilePath => _lockFilePath;

        public static JasonQueryDbStorageMigrationExecutionLock Acquire(string databaseFilePath)
        {
            return AcquireCore(databaseFilePath, FileMode.OpenOrCreate);
        }

        internal static JasonQueryDbStorageMigrationExecutionLock AcquireForNewMigration(string databaseFilePath)
        {
            return AcquireCore(databaseFilePath, FileMode.CreateNew);
        }

        private static JasonQueryDbStorageMigrationExecutionLock AcquireCore(string databaseFilePath, FileMode lockFileMode)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException
                (
                    "A database file path is required.",
                    nameof(databaseFilePath)
                );
            }

            var normalizedDatabaseFilePath = Path.GetFullPath(databaseFilePath);

            var lockFilePath = JasonQueryDbStorageMigrationArtifactPaths.GetLockFilePath
            (
                normalizedDatabaseFilePath
            );

            var mutexName = CreateMutexName(normalizedDatabaseFilePath);
            var mutex = new Mutex(false, mutexName);
            var mutexOwned = false;
            FileStream lockStream = null;

            try
            {
                try
                {
                    mutexOwned = mutex.WaitOne(0);
                }
                catch (AbandonedMutexException)
                {
                    mutexOwned = true;
                }

                if (!mutexOwned)
                {
                    throw new InvalidOperationException
                    (
                        "Another database storage-migration executor currently owns the migration mutex."
                    );
                }

                lockStream = new FileStream
                (
                    lockFilePath,
                    lockFileMode,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    1,
                    FileOptions.WriteThrough
                );

                lockStream.Flush(true);

                return new JasonQueryDbStorageMigrationExecutionLock
                (
                    lockFilePath,
                    mutex,
                    true,
                    lockStream
                );
            }
            catch
            {
                if (lockStream != null)
                {
                    lockStream.Dispose();
                }

                if (mutexOwned)
                {
                    mutex.ReleaseMutex();
                }

                mutex.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Exception deleteFailure = null;

            if (_lockStream != null)
            {
                _lockStream.Dispose();
                _lockStream = null;
            }

            try
            {
                if (File.Exists(_lockFilePath))
                {
                    File.Delete(_lockFilePath);
                }
            }
            catch (Exception ex)
            {
                deleteFailure = ex;
            }
            finally
            {
                if (_mutexOwned && _mutex != null)
                {
                    _mutex.ReleaseMutex();
                    _mutexOwned = false;
                }

                if (_mutex != null)
                {
                    _mutex.Dispose();
                    _mutex = null;
                }
            }

            if (deleteFailure != null)
            {
                throw new IOException
                (
                    "The database storage-migration lock file could not be deleted.",
                    deleteFailure
                );
            }
        }

        private static string CreateMutexName(string normalizedDatabaseFilePath)
        {
            var normalizedIdentity = normalizedDatabaseFilePath.ToUpperInvariant();
            var bytes = Encoding.UTF8.GetBytes(normalizedIdentity);
            byte[] hash = null;

            try
            {
                using (var sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(bytes);
                }

                return MutexPrefix + BitConverter.ToString(hash).Replace("-", string.Empty);
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);

                if (hash != null)
                {
                    Array.Clear(hash, 0, hash.Length);
                }
            }
        }
    }
}
