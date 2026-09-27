using JasonQuery.Core.Security.JasonQueryDb;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace JasonQuery.ModernDbMigration
{
    internal static class ModernSqlCipherRuntime
    {
        internal const string QualifiedNativeSha256 = "25852CE7A4067CC79E73309D26C1AD9B5706E876BBDF48BE9A25379180FB9A07";
        internal const string QualifiedSqliteVersion = "3.53.3";
        internal const string QualifiedSqlCipherVersionPrefix = "4.17.0";
        internal const string QualifiedCipherProvider = "openssl";
        internal const string QualifiedOpenSslVersionPrefix = "OpenSSL 3.5.8";
        internal const string QualifiedManagedAssemblyVersion = "3.0.5.3129";

        private static bool _initialized;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

        internal static void InitializeAndValidate()
        {
            if (_initialized)
            {
                return;
            }

            if (!Environment.Is64BitProcess)
            {
                throw new PlatformNotSupportedException("Storage V2 migration requires an x64 process.");
            }

            var provider = new SQLite3Provider_sqlcipher();

            raw.SetProvider(provider);
            raw.FreezeProvider();

            var nativeName = ((ISQLite3Provider)provider).GetNativeLibraryName();

            if (!string.Equals(nativeName, "sqlcipher", StringComparison.Ordinal))
            {
                throw new InvalidDataException("The modern SQLite provider is not bound to the qualified sqlcipher library name.");
            }

            var sqliteVersion = raw.sqlite3_libversion().utf8_to_string();

            if (!string.Equals(sqliteVersion, QualifiedSqliteVersion, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The modern SQLite runtime version is not the qualified version.");
            }

            EnsureAssemblyVersion(typeof(raw).Assembly, "SQLitePCLRaw.core");
            EnsureAssemblyVersion(typeof(SQLite3Provider_sqlcipher).Assembly, "SQLitePCLRaw.provider.sqlcipher");

            var nativePath = GetLoadedNativeModulePath("sqlcipher.dll");
            var nativeHash = ComputeFileSha256(nativePath);

            if (!string.Equals(nativeHash, QualifiedNativeSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The loaded sqlcipher.dll does not match the qualified native runtime.");
            }

            _initialized = true;
        }

        internal static sqlite3 OpenCreateNew(string databasePath, byte[] databasePasswordUtf8)
        {
            if (File.Exists(databasePath))
            {
                throw new IOException("The Storage V2 candidate database already exists.");
            }

            return Open(databasePath, databasePasswordUtf8, raw.SQLITE_OPEN_READWRITE | raw.SQLITE_OPEN_CREATE);
        }

        internal static sqlite3 OpenReadWrite(string databasePath, byte[] databasePasswordUtf8)
        {
            return Open(databasePath, databasePasswordUtf8, raw.SQLITE_OPEN_READWRITE);
        }

        internal static sqlite3 OpenReadOnly(string databasePath, byte[] databasePasswordUtf8)
        {
            return Open(databasePath, databasePasswordUtf8, raw.SQLITE_OPEN_READONLY);
        }

        internal static sqlite3 OpenWithoutKeyReadOnly(string databasePath)
        {
            InitializeAndValidate();

            sqlite3 db = null;

            var flags = raw.SQLITE_OPEN_READONLY | raw.SQLITE_OPEN_FULLMUTEX | raw.SQLITE_OPEN_PRIVATECACHE;
            var rc = raw.sqlite3_open_v2(databasePath, out db, flags, null);

            if (rc != raw.SQLITE_OK)
            {
                CloseDatabase(db);
                return null;
            }

            try
            {
                DisableInternalLogging(db);
                return db;
            }
            catch
            {
                CloseDatabase(db);
                throw;
            }
        }

        internal static void Rekey(sqlite3 db, byte[] newDatabasePasswordUtf8)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (newDatabasePasswordUtf8 == null || newDatabasePasswordUtf8.Length == 0)
            {
                throw new ArgumentException("A new Storage V2 database credential is required.", nameof(newDatabasePasswordUtf8));
            }

            CheckRc
            (
                db,
                raw.sqlite3_rekey(db, new ReadOnlySpan<byte>(newDatabasePasswordUtf8)),
                "sqlite3_rekey"
            );
        }

        internal static void ApplyFrozenCipherProfile(sqlite3 db)
        {
            Exec(db, "PRAGMA cipher_compatibility = 4;");
            Exec(db, "PRAGMA cipher_page_size = 4096;");
            Exec(db, "PRAGMA kdf_iter = 256000;");
            Exec(db, "PRAGMA cipher_kdf_algorithm = PBKDF2_HMAC_SHA512;");
            Exec(db, "PRAGMA cipher_hmac_algorithm = HMAC_SHA512;");
            Exec(db, "PRAGMA cipher_use_hmac = ON;");
            Exec(db, "PRAGMA cipher_plaintext_header_size = 0;");
        }

        internal static void ConfigureWriter(sqlite3 db)
        {
            Exec(db, "PRAGMA journal_mode = DELETE;");
            Exec(db, "PRAGMA synchronous = FULL;");
            Exec(db, "PRAGMA temp_store = MEMORY;");
            Exec(db, "PRAGMA foreign_keys = OFF;");
        }

        internal static void ValidateFrozenProfile(sqlite3 db)
        {
            AssertEqual(4096L, QueryInt64(db, "PRAGMA cipher_page_size;"), "cipher_page_size");
            AssertEqual(256000L, QueryInt64(db, "PRAGMA kdf_iter;"), "kdf_iter");
            AssertEqual("PBKDF2_HMAC_SHA512", QueryText(db, "PRAGMA cipher_kdf_algorithm;"), "cipher_kdf_algorithm");
            AssertEqual("HMAC_SHA512", QueryText(db, "PRAGMA cipher_hmac_algorithm;"), "cipher_hmac_algorithm");
            AssertEqual(1L, QueryInt64(db, "PRAGMA cipher_use_hmac;"), "cipher_use_hmac");
            AssertEqual(0L, QueryInt64(db, "PRAGMA cipher_plaintext_header_size;"), "cipher_plaintext_header_size");
            AssertEqual(4096L, QueryInt64(db, "PRAGMA page_size;"), "page_size");

            var sqliteLengthLimit = raw.sqlite3_limit(db, raw.SQLITE_LIMIT_LENGTH, -1);

            if ((long)sqliteLengthLimit < JasonQueryDbStorageMigrationLogicalStreamContract.MaxTextUtf8Bytes
                || (long)sqliteLengthLimit < JasonQueryDbStorageMigrationLogicalStreamContract.MaxBlobBytes)
            {
                throw new InvalidDataException("The SQLite length limit is below the frozen logical migration value limit.");
            }

            var cipherVersion = QueryText(db, "PRAGMA cipher_version;");

            if (string.IsNullOrEmpty(cipherVersion) || !cipherVersion.StartsWith(QualifiedSqlCipherVersionPrefix, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The candidate is not using the qualified SQLCipher version.");
            }

            AssertEqual(QualifiedCipherProvider, QueryText(db, "PRAGMA cipher_provider;"), "cipher_provider");

            var providerVersion = QueryText(db, "PRAGMA cipher_provider_version;");

            if (string.IsNullOrEmpty(providerVersion) || !providerVersion.StartsWith(QualifiedOpenSslVersionPrefix, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The candidate is not using the qualified OpenSSL provider version.");
            }
        }

        internal static void ValidateWriterPragmas(sqlite3 db)
        {
            AssertEqual("delete", (QueryText(db, "PRAGMA journal_mode;") ?? string.Empty).ToLowerInvariant(), "journal_mode");
            AssertEqual(2L, QueryInt64(db, "PRAGMA synchronous;"), "synchronous");
            AssertEqual(2L, QueryInt64(db, "PRAGMA temp_store;"), "temp_store");
        }

        internal static void ValidateIntegrity(sqlite3 db)
        {
            if (CountRows(db, "PRAGMA cipher_integrity_check;") != 0)
            {
                throw new InvalidDataException("SQLCipher integrity validation returned one or more error rows.");
            }

            AssertEqual("ok", QueryText(db, "PRAGMA integrity_check;"), "integrity_check");
        }

        internal static void Exec(sqlite3 db, string sql)
        {
            string errorMessage;
            var rc = raw.sqlite3_exec(db, sql, out errorMessage);

            if (rc != raw.SQLITE_OK)
            {
                throw CreateSqliteException(db, rc, "SQLite execution", errorMessage);
            }
        }

        internal static void ExecSingleStatement(sqlite3 db, string sql, string operation)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new InvalidDataException(operation + " SQL is empty.");
            }

            sqlite3_stmt statement = null;

            try
            {
                string tail;

                CheckRc(db, raw.sqlite3_prepare_v2(db, sql, out statement, out tail), operation + " prepare");

                if (statement == null)
                {
                    throw new InvalidDataException(operation + " did not produce a SQLite statement.");
                }

                if (!IsOnlySqlTailWhitespace(tail))
                {
                    throw new InvalidDataException(operation + " contains more than one SQL statement.");
                }

                CheckStepDone(db, statement, operation);
            }
            finally
            {
                FinalizeStatement(statement);
            }
        }

        internal static string QueryText(sqlite3 db, string sql)
        {
            sqlite3_stmt statement = null;

            try
            {
                CheckRc(db, raw.sqlite3_prepare_v2(db, sql, out statement), "prepare text query");

                var rc = raw.sqlite3_step(statement);

                if (rc != raw.SQLITE_ROW)
                {
                    CheckRc(db, rc, "step text query");
                    throw new InvalidDataException("The text query returned no row.");
                }

                if (raw.sqlite3_column_type(statement, 0) == raw.SQLITE_NULL)
                {
                    return null;
                }

                return raw.sqlite3_column_text(statement, 0).utf8_to_string();
            }
            finally
            {
                FinalizeStatement(statement);
            }
        }

        internal static long QueryInt64(sqlite3 db, string sql)
        {
            sqlite3_stmt statement = null;

            try
            {
                CheckRc(db, raw.sqlite3_prepare_v2(db, sql, out statement), "prepare integer query");

                var rc = raw.sqlite3_step(statement);

                if (rc != raw.SQLITE_ROW)
                {
                    CheckRc(db, rc, "step integer query");
                    throw new InvalidDataException("The integer query returned no row.");
                }

                return raw.sqlite3_column_int64(statement, 0);
            }
            finally
            {
                FinalizeStatement(statement);
            }
        }

        internal static int CountRows(sqlite3 db, string sql)
        {
            sqlite3_stmt statement = null;
            var count = 0;

            try
            {
                CheckRc(db, raw.sqlite3_prepare_v2(db, sql, out statement), "prepare row-count query");

                while (true)
                {
                    var rc = raw.sqlite3_step(statement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        return count;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        CheckRc(db, rc, "step row-count query");
                    }

                    count = checked(count + 1);
                }
            }
            finally
            {
                FinalizeStatement(statement);
            }
        }

        internal static void CheckStepDone(sqlite3 db, sqlite3_stmt statement, string operation)
        {
            var rc = raw.sqlite3_step(statement);

            if (rc != raw.SQLITE_DONE)
            {
                CheckRc(db, rc, operation);
                throw new InvalidDataException(operation + " did not finish with SQLITE_DONE.");
            }
        }

        internal static void CheckRc(sqlite3 db, int rc, string operation)
        {
            if (rc == raw.SQLITE_OK)
            {
                return;
            }

            throw CreateSqliteException(db, rc, operation, null);
        }

        internal static void FinalizeStatement(sqlite3_stmt statement)
        {
            if (statement != null)
            {
                raw.sqlite3_finalize(statement);
            }
        }

        internal static void CloseBlob(sqlite3_blob blob)
        {
            if (blob != null)
            {
                raw.sqlite3_blob_close(blob);
            }
        }

        internal static void CloseDatabase(sqlite3 db)
        {
            if (db != null)
            {
                raw.sqlite3_close_v2(db);
            }
        }

        internal static void TryRollback(sqlite3 db)
        {
            if (db == null)
            {
                return;
            }

            try
            {
                raw.sqlite3_exec(db, "ROLLBACK;");
            }
            catch
            {
            }
        }

        internal static void DurablyFlushFile(string databasePath)
        {
            using (var stream = new FileStream(databasePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.WriteThrough))
            {
                stream.Flush(true);
            }
        }

        internal static bool StartsWithPlainSqliteHeader(string databasePath)
        {
            var magic = Encoding.ASCII.GetBytes("SQLite format 3" + (char)0);
            var buffer = new byte[magic.Length];

            using (var stream = new FileStream(databasePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var read = stream.Read(buffer, 0, buffer.Length);

                if (read != buffer.Length)
                {
                    return false;
                }
            }

            for (var index = 0; index < magic.Length; index++)
            {
                if (buffer[index] != magic[index])
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool IsReadRejectedWithKey(string databasePath, byte[] databasePasswordUtf8)
        {
            sqlite3 db = null;

            try
            {
                db = OpenReadOnly(databasePath, databasePasswordUtf8);
                sqlite3_stmt statement = null;

                try
                {
                    var rc = raw.sqlite3_prepare_v2(db, "SELECT count(*) FROM sqlite_schema;", out statement);

                    if (rc != raw.SQLITE_OK)
                    {
                        return true;
                    }

                    return raw.sqlite3_step(statement) != raw.SQLITE_ROW;
                }
                finally
                {
                    FinalizeStatement(statement);
                }
            }
            catch
            {
                return true;
            }
            finally
            {
                CloseDatabase(db);
            }
        }

        internal static bool ContainsByteSequence(string filePath, byte[] sequence)
        {
            if (sequence == null || sequence.Length == 0)
            {
                throw new ArgumentException("A non-empty byte sequence is required.", nameof(sequence));
            }

            var buffer = new byte[(64 * 1024) + sequence.Length - 1];
            var retained = 0;

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    while (true)
                    {
                        var read = stream.Read(buffer, retained, (64 * 1024));
                        var total = retained + read;
                        var lastStart = total - sequence.Length;

                        for (var start = 0; start <= lastStart; start++)
                        {
                            var matched = true;

                            for (var index = 0; index < sequence.Length; index++)
                            {
                                if (buffer[start + index] != sequence[index])
                                {
                                    matched = false;
                                    break;
                                }
                            }

                            if (matched)
                            {
                                return true;
                            }
                        }

                        if (read == 0)
                        {
                            return false;
                        }

                        retained = Math.Min(sequence.Length - 1, total);

                        if (retained > 0)
                        {
                            Buffer.BlockCopy(buffer, total - retained, buffer, 0, retained);
                        }
                    }
                }
            }
            finally
            {
                Array.Clear(buffer, 0, buffer.Length);
            }
        }

        internal static bool IsReadRejectedWithoutKey(string databasePath)
        {
            sqlite3 db = null;

            try
            {
                db = OpenWithoutKeyReadOnly(databasePath);

                if (db == null)
                {
                    return true;
                }

                sqlite3_stmt statement = null;

                try
                {
                    var rc = raw.sqlite3_prepare_v2(db, "SELECT count(*) FROM sqlite_schema;", out statement);

                    if (rc != raw.SQLITE_OK)
                    {
                        return true;
                    }

                    return raw.sqlite3_step(statement) != raw.SQLITE_ROW;
                }
                finally
                {
                    FinalizeStatement(statement);
                }
            }
            catch
            {
                return true;
            }
            finally
            {
                CloseDatabase(db);
            }
        }

        internal static void EnsureNoSidecars(string databasePath)
        {
            var sidecars = new[]
            {
                databasePath + "-journal",
                databasePath + "-wal",
                databasePath + "-shm"
            };

            foreach (var sidecar in sidecars)
            {
                if (File.Exists(sidecar))
                {
                    throw new InvalidDataException("The Storage V2 candidate left a SQLite sidecar file after close.");
                }
            }
        }

        internal static void DeleteCandidateArtifactsBestEffort(string databasePath)
        {
            DeleteFileBestEffort(databasePath + "-journal");
            DeleteFileBestEffort(databasePath + "-wal");
            DeleteFileBestEffort(databasePath + "-shm");
            DeleteFileBestEffort(databasePath);
        }

        internal static string QuoteIdentifier(string identifier)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureIdentifier(identifier, nameof(identifier));
            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        internal static void AssertEqual<T>(T expected, T actual, string gate)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidDataException
                (
                    gate + " failed. Expected=" + Convert.ToString(expected, CultureInfo.InvariantCulture) +
                    ", Actual=" + Convert.ToString(actual, CultureInfo.InvariantCulture)
                );
            }
        }

        internal static void ClearBytes(byte[] value)
        {
            if (value != null)
            {
                Array.Clear(value, 0, value.Length);
            }
        }

        private static sqlite3 Open(string databasePath, byte[] databasePasswordUtf8, int accessFlags)
        {
            InitializeAndValidate();

            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A database path is required.", nameof(databasePath));
            }

            if (databasePasswordUtf8 == null || databasePasswordUtf8.Length <= 0)
            {
                throw new ArgumentException("A database password is required.", nameof(databasePasswordUtf8));
            }

            sqlite3 db = null;
            var flags = accessFlags | raw.SQLITE_OPEN_FULLMUTEX | raw.SQLITE_OPEN_PRIVATECACHE;
            var rc = raw.sqlite3_open_v2(databasePath, out db, flags, null);

            if (rc != raw.SQLITE_OK)
            {
                var exception = CreateSqliteException(db, rc, "open database", null);

                CloseDatabase(db);
                throw exception;
            }

            try
            {
                DisableInternalLogging(db);
                CheckRc(db, raw.sqlite3_key(db, new ReadOnlySpan<byte>(databasePasswordUtf8)), "sqlite3_key");
                ApplyFrozenCipherProfile(db);
                return db;
            }
            catch
            {
                CloseDatabase(db);
                throw;
            }
        }

        private static void DisableInternalLogging(sqlite3 db)
        {
            //SQLCipher defaults to STDERR logging on Windows. Keep the migration helper's STDERR diagnostics-only.
            Exec(db, "PRAGMA cipher_log_level = NONE;");
        }

        private static void EnsureAssemblyVersion(Assembly assembly, string description)
        {
            var version = assembly.GetName().Version == null ? string.Empty : assembly.GetName().Version.ToString();

            if (!string.Equals(version, QualifiedManagedAssemblyVersion, StringComparison.Ordinal))
            {
                throw new InvalidDataException(description + " assembly version is not the qualified version.");
            }
        }

        private static string GetLoadedNativeModulePath(string moduleName)
        {
            var module = GetModuleHandle(moduleName);

            if (module == IntPtr.Zero)
            {
                throw new InvalidOperationException("The qualified native SQLCipher module is not loaded.");
            }

            var buffer = new StringBuilder(32768);
            var length = GetModuleFileName(module, buffer, buffer.Capacity);

            if (length == 0)
            {
                throw new InvalidOperationException("The loaded SQLCipher module path could not be resolved.");
            }

            return Path.GetFullPath(buffer.ToString());
        }

        private static string ComputeFileSha256(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var hash = SHA256.Create())
            {
                var bytes = hash.ComputeHash(stream);
                var builder = new StringBuilder(bytes.Length * 2);

                for (var index = 0; index < bytes.Length; index++)
                {
                    builder.Append(bytes[index].ToString("X2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }

        private static InvalidDataException CreateSqliteException(sqlite3 db, int rc, string operation, string explicitMessage)
        {
            var message = explicitMessage;

            if (string.IsNullOrEmpty(message))
            {
                message = db == null ? raw.sqlite3_errstr(rc).utf8_to_string() : raw.sqlite3_errmsg(db).utf8_to_string();
            }

            return new InvalidDataException
            (
                operation + " failed with SQLite result " + rc.ToString(CultureInfo.InvariantCulture) + ": " + message
            );
        }

        private static bool IsOnlySqlTailWhitespace(string tail)
        {
            if (string.IsNullOrWhiteSpace(tail))
            {
                return true;
            }

            for (var index = 0; index < tail.Length; index++)
            {
                var value = tail[index];

                if (char.IsWhiteSpace(value) || value == ';' || value == '\0')
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static void DeleteFileBestEffort(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
            }
        }
    }
}
