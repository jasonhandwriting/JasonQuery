using JasonQuery.Core.Security.ConnectionCredentials;
using JasonQuery.Database.Internal.Runtime.Modern;
using JasonQuery.ModernDbMigration;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace JasonQuery.ModernDbRuntime
{
    internal static class ModernStorageV2FreshDatabaseCreator
    {
        private const string AttachedSchemaName = "freshv2";
        private const string MetadataTableName = "JasonQuerySecurityMetadata";
        private const string MetadataVersionKey = "ConnectionCredentialStorageVersion";
        private const string MetadataUniqueIndexName = "UX_JasonQuerySecurityMetadata_MetadataKey";

        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        internal static void Create(string databasePath, byte[] databasePasswordUtf8, byte[] plaintextTemplateBytes)
        {
            ValidateCreateArguments(databasePath, databasePasswordUtf8, plaintextTemplateBytes);
            ModernSqlCipherRuntime.InitializeAndValidate();

            var fullDatabasePath = Path.GetFullPath(databasePath);

            if (File.Exists(fullDatabasePath))
            {
                throw new IOException("The fresh Storage V2 database destination already exists.");
            }

            sqlite3 sourceDatabase = null;
            IntPtr templateBuffer = IntPtr.Zero;

            try
            {
                OpenDeserializedTemplate
                (
                    plaintextTemplateBytes,
                    out sourceDatabase,
                    out templateBuffer
                );

                ValidateSourceTemplate(sourceDatabase);

                var sourceUserVersion = QueryInt64(sourceDatabase, "PRAGMA user_version;");
                var sourceApplicationId = QueryInt64(sourceDatabase, "PRAGMA application_id;");
                var sourceAutoVacuum = QueryInt64(sourceDatabase, "PRAGMA auto_vacuum;");
                var sourceEncoding = QueryText(sourceDatabase, "PRAGMA encoding;");
                var sourceLogicalDigest = ComputeLogicalDigest(sourceDatabase);

                ValidateAutoVacuum(sourceAutoVacuum);
                ValidateEncoding(sourceEncoding);

                AttachEncryptedTarget
                (
                    sourceDatabase,
                    fullDatabasePath,
                    databasePasswordUtf8
                );

                var attached = true;

                try
                {
                    ApplyAttachedFrozenCipherProfile(sourceDatabase);
                    ConfigureAttachedWriter(sourceDatabase);

                    ConfigureAttachedDatabaseProperties
                    (
                        sourceDatabase,
                        sourceAutoVacuum,
                        sourceEncoding
                    );

                    ValidateAttachedFrozenCipherProfile(sourceDatabase);
                    ValidateAttachedWriter(sourceDatabase);

                    ExecuteToCompletion
                    (
                        sourceDatabase,
                        "SELECT sqlcipher_export('" + AttachedSchemaName + "');",
                        "sqlcipher_export"
                    );

                    SetAttachedIntegerPragma
                    (
                        sourceDatabase,
                        "user_version",
                        sourceUserVersion
                    );

                    SetAttachedIntegerPragma
                    (
                        sourceDatabase,
                        "application_id",
                        sourceApplicationId
                    );

                    ValidateAttachedProperties
                    (
                        sourceDatabase,
                        sourceUserVersion,
                        sourceApplicationId,
                        sourceAutoVacuum,
                        sourceEncoding
                    );

                    ValidateSystemConfigAnchor
                    (
                        sourceDatabase,
                        AttachedSchemaName + ".sqlite_schema"
                    );
                }
                finally
                {
                    if (attached)
                    {
                        ExecuteToCompletion
                        (
                            sourceDatabase,
                            "DETACH DATABASE " + AttachedSchemaName + ";",
                            "DETACH encrypted fresh target"
                        );

                        attached = false;
                    }
                }

                ModernSqlCipherRuntime.DurablyFlushFile(fullDatabasePath);

                ValidateEncryptedTargetBeforeMarker
                (
                    fullDatabasePath,
                    databasePasswordUtf8,
                    sourceLogicalDigest,
                    sourceUserVersion,
                    sourceApplicationId,
                    sourceAutoVacuum,
                    sourceEncoding
                );

                InitializeConnectionCredentialStorageVersion
                (
                    fullDatabasePath,
                    databasePasswordUtf8
                );

                ValidateFinalEncryptedTarget
                (
                    fullDatabasePath,
                    databasePasswordUtf8
                );

                ModernSqlCipherRuntime.DurablyFlushFile(fullDatabasePath);
                ModernSqlCipherRuntime.EnsureNoSidecars(fullDatabasePath);
            }
            catch
            {
                DeleteFileIfExists(fullDatabasePath);
                DeleteFileIfExists(fullDatabasePath + "-journal");
                DeleteFileIfExists(fullDatabasePath + "-wal");
                DeleteFileIfExists(fullDatabasePath + "-shm");
                throw;
            }
            finally
            {
                ModernSqlCipherRuntime.CloseDatabase(sourceDatabase);

                if (templateBuffer != IntPtr.Zero)
                {
                    raw.sqlite3_free(templateBuffer);
                }
            }
        }

        private static void OpenDeserializedTemplate(byte[] templateBytes, out sqlite3 database, out IntPtr templateBuffer)
        {
            database = null;
            templateBuffer = IntPtr.Zero;

            var flags = raw.SQLITE_OPEN_READWRITE | raw.SQLITE_OPEN_CREATE | raw.SQLITE_OPEN_FULLMUTEX | raw.SQLITE_OPEN_PRIVATECACHE;

            ModernSqlCipherRuntime.CheckRc
            (
                database,
                raw.sqlite3_open_v2(":memory:", out database, flags, null),
                "open fresh-template in-memory source"
            );

            ModernSqlCipherRuntime.Exec(database, "PRAGMA cipher_log_level = NONE;");

            templateBuffer = raw.sqlite3_malloc64(templateBytes.LongLength);

            if (templateBuffer == IntPtr.Zero)
            {
                throw new OutOfMemoryException
                (
                    "sqlite3_malloc64 could not allocate the fresh-template deserialize buffer."
                );
            }

            Marshal.Copy(templateBytes, 0, templateBuffer, templateBytes.Length);

            ModernSqlCipherRuntime.CheckRc
            (
                database,
                raw.sqlite3_deserialize
                (
                    database,
                    "main",
                    templateBuffer,
                    templateBytes.LongLength,
                    templateBytes.LongLength,
                    raw.SQLITE_DESERIALIZE_READONLY
                ),
                "sqlite3_deserialize fresh template"
            );
        }

        private static void ValidateSourceTemplate(sqlite3 database)
        {
            if (!string.Equals(QueryText(database, "PRAGMA integrity_check;"), "ok", StringComparison.Ordinal))
            {
                throw new InvalidDataException("The in-memory JasonQuery database template failed integrity_check.");
            }

            ValidateSystemConfigAnchor(database, "sqlite_schema");
        }

        private static void AttachEncryptedTarget(sqlite3 database, string databasePath, byte[] databasePasswordUtf8)
        {
            sqlite3_stmt statement = null;
            byte[] pathUtf8 = null;

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_prepare_v2
                    (
                        database,
                        "ATTACH DATABASE ?1 AS " + AttachedSchemaName + " KEY ?2;",
                        out statement
                    ),
                    "prepare parameterized fresh-target ATTACH"
                );

                pathUtf8 = StrictUtf8.GetBytes(databasePath);

                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_bind_text(statement, 1, new ReadOnlySpan<byte>(pathUtf8)),
                    "bind fresh-target database path"
                );

                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_bind_blob(statement, 2, new ReadOnlySpan<byte>(databasePasswordUtf8)),
                    "bind fresh-target credential"
                );

                StepExpectDone(database, statement, "parameterized fresh-target ATTACH");
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
                Clear(pathUtf8);
            }

            if (!File.Exists(databasePath))
            {
                throw new InvalidDataException("Parameterized ATTACH did not create the encrypted fresh target.");
            }
        }

        private static void ApplyAttachedFrozenCipherProfile(sqlite3 database)
        {
            ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".cipher_compatibility = 4;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".cipher_page_size = 4096;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".kdf_iter = 256000;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".cipher_kdf_algorithm = PBKDF2_HMAC_SHA512;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".cipher_hmac_algorithm = HMAC_SHA512;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".cipher_use_hmac = ON;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".cipher_plaintext_header_size = 0;");
        }

        private static void ConfigureAttachedWriter(sqlite3 database)
        {
            QueryText(database, "PRAGMA " + AttachedSchemaName + ".journal_mode = DELETE;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".synchronous = FULL;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA temp_store = MEMORY;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA foreign_keys = OFF;");
        }

        private static void ConfigureAttachedDatabaseProperties(sqlite3 database, long autoVacuum, string encoding)
        {
            SetAttachedIntegerPragma(database, "auto_vacuum", autoVacuum);

            if (string.Equals(encoding, "UTF-8", StringComparison.Ordinal))
            {
                ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".encoding = 'UTF-8';");
                return;
            }

            if (string.Equals(encoding, "UTF-16le", StringComparison.Ordinal))
            {
                ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".encoding = 'UTF-16le';");
                return;
            }

            ModernSqlCipherRuntime.Exec(database, "PRAGMA " + AttachedSchemaName + ".encoding = 'UTF-16be';");
        }

        private static void ValidateAttachedFrozenCipherProfile(sqlite3 database)
        {
            AssertEqual(4096L, QueryInt64(database, "PRAGMA " + AttachedSchemaName + ".cipher_page_size;"), "attached cipher_page_size");
            AssertEqual(256000L, QueryInt64(database, "PRAGMA " + AttachedSchemaName + ".kdf_iter;"), "attached kdf_iter");
            AssertEqual("PBKDF2_HMAC_SHA512", QueryText(database, "PRAGMA " + AttachedSchemaName + ".cipher_kdf_algorithm;"), "attached cipher_kdf_algorithm");
            AssertEqual("HMAC_SHA512", QueryText(database, "PRAGMA " + AttachedSchemaName + ".cipher_hmac_algorithm;"), "attached cipher_hmac_algorithm");
            AssertEqual(1L, QueryInt64(database, "PRAGMA " + AttachedSchemaName + ".cipher_use_hmac;"), "attached cipher_use_hmac");
            AssertEqual(0L, QueryInt64(database, "PRAGMA " + AttachedSchemaName + ".cipher_plaintext_header_size;"), "attached cipher_plaintext_header_size");
        }

        private static void ValidateAttachedWriter(sqlite3 database)
        {
            AssertEqual
            (
                "delete",
                (QueryText(database, "PRAGMA " + AttachedSchemaName + ".journal_mode;") ?? string.Empty).ToLowerInvariant(),
                "attached journal_mode"
            );

            AssertEqual(2L, QueryInt64(database, "PRAGMA " + AttachedSchemaName + ".synchronous;"), "attached synchronous");
        }

        private static void ValidateAttachedProperties(sqlite3 database, long userVersion, long applicationId, long autoVacuum, string encoding)
        {
            AssertEqual(userVersion, QueryInt64(database, "PRAGMA " + AttachedSchemaName + ".user_version;"), "attached user_version");
            AssertEqual(applicationId, QueryInt64(database, "PRAGMA " + AttachedSchemaName + ".application_id;"), "attached application_id");
            AssertEqual(autoVacuum, QueryInt64(database, "PRAGMA " + AttachedSchemaName + ".auto_vacuum;"), "attached auto_vacuum");
            AssertEqual(encoding, QueryText(database, "PRAGMA " + AttachedSchemaName + ".encoding;"), "attached encoding");
        }

        private static void ValidateEncryptedTargetBeforeMarker(string databasePath, byte[] databasePasswordUtf8, string sourceLogicalDigest, long sourceUserVersion,
                                                                long sourceApplicationId, long sourceAutoVacuum, string sourceEncoding)
        {
            sqlite3 database = null;

            try
            {
                database = ModernSqlCipherRuntime.OpenReadWrite(databasePath, databasePasswordUtf8);
                ModernSqlCipherRuntime.ConfigureWriter(database);
                ModernSqlCipherRuntime.ValidateFrozenProfile(database);
                ModernSqlCipherRuntime.ValidateWriterPragmas(database);
                ModernSqlCipherRuntime.ValidateIntegrity(database);
                ValidateSystemConfigAnchor(database, "sqlite_schema");

                AssertEqual(sourceUserVersion, QueryInt64(database, "PRAGMA user_version;"), "fresh target user_version");
                AssertEqual(sourceApplicationId, QueryInt64(database, "PRAGMA application_id;"), "fresh target application_id");
                AssertEqual(sourceAutoVacuum, QueryInt64(database, "PRAGMA auto_vacuum;"), "fresh target auto_vacuum");
                AssertEqual(sourceEncoding, QueryText(database, "PRAGMA encoding;"), "fresh target encoding");

                var targetLogicalDigest = ComputeLogicalDigest(database);

                if (!string.Equals(sourceLogicalDigest, targetLogicalDigest, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The fresh Storage V2 logical database content changed during sqlcipher_export.");
                }
            }
            finally
            {
                ModernSqlCipherRuntime.CloseDatabase(database);
            }
        }

        private static void InitializeConnectionCredentialStorageVersion(string databasePath, byte[] databasePasswordUtf8)
        {
            sqlite3 database = null;

            try
            {
                database = ModernSqlCipherRuntime.OpenReadWrite(databasePath, databasePasswordUtf8);
                ModernSqlCipherRuntime.ConfigureWriter(database);
                EnsureMigrationCompatibleMetadataTable(database);

                ModernSqlCipherRuntime.Exec(database, "BEGIN IMMEDIATE;");

                try
                {
                    var currentVersion = ConnectionCredentialStorageContract.CurrentVersion.ToString(CultureInfo.InvariantCulture);
                    sqlite3_stmt update = null;

                    try
                    {
                        ModernSqlCipherRuntime.CheckRc
                        (
                            database,
                            raw.sqlite3_prepare_v2
                            (
                                database,
                                "UPDATE [" + MetadataTableName + "] SET [MetadataValue] = ?1 WHERE [MetadataKey] = ?2;",
                                out update
                            ),
                            "prepare connection credential storage marker update"
                        );

                        BindText(database, update, 1, currentVersion, "bind connection credential storage marker value");
                        BindText(database, update, 2, MetadataVersionKey, "bind connection credential storage marker key");
                        StepExpectDone(database, update, "update connection credential storage marker");
                    }
                    finally
                    {
                        ModernSqlCipherRuntime.FinalizeStatement(update);
                    }

                    var changedRows = QueryInt64(database, "SELECT changes();");

                    if (changedRows > 1)
                    {
                        throw new InvalidDataException("Multiple connection credential storage markers were updated.");
                    }

                    if (changedRows == 0)
                    {
                        sqlite3_stmt insert = null;

                        try
                        {
                            ModernSqlCipherRuntime.CheckRc
                            (
                                database,
                                raw.sqlite3_prepare_v2
                                (
                                    database,
                                    "INSERT INTO [" + MetadataTableName + "] ([MetadataKey], [MetadataValue]) VALUES (?1, ?2);",
                                    out insert
                                ),
                                "prepare connection credential storage marker insert"
                            );

                            BindText(database, insert, 1, MetadataVersionKey, "bind inserted connection credential storage marker key");
                            BindText(database, insert, 2, currentVersion, "bind inserted connection credential storage marker value");
                            StepExpectDone(database, insert, "insert connection credential storage marker");
                        }
                        finally
                        {
                            ModernSqlCipherRuntime.FinalizeStatement(insert);
                        }

                        if (QueryInt64(database, "SELECT changes();") != 1L)
                        {
                            throw new InvalidDataException("The connection credential storage version marker could not be inserted.");
                        }
                    }

                    ModernSqlCipherRuntime.Exec(database, "COMMIT;");
                }
                catch
                {
                    ModernSqlCipherRuntime.TryRollback(database);
                    throw;
                }

                ValidateConnectionCredentialStorageVersion(database);
                ModernSqlCipherRuntime.ValidateIntegrity(database);
            }
            finally
            {
                ModernSqlCipherRuntime.CloseDatabase(database);
            }
        }

        private static void EnsureMigrationCompatibleMetadataTable(sqlite3 database)
        {
            var tableCount = QueryInt64
            (
                database,
                "SELECT count(*) FROM sqlite_schema WHERE type='table' AND name='" + MetadataTableName + "';"
            );

            if (tableCount == 0)
            {
                ModernSqlCipherRuntime.Exec
                (
                    database,
                    "CREATE TABLE [" + MetadataTableName + "] (" +
                    "[PID] INTEGER PRIMARY KEY, " +
                    "[MetadataKey] TEXT NOT NULL, " +
                    "[MetadataValue] TEXT NOT NULL);"
                );

                ModernSqlCipherRuntime.Exec
                (
                    database,
                    "CREATE UNIQUE INDEX [" + MetadataUniqueIndexName + "] " +
                    "ON [" + MetadataTableName + "] ([MetadataKey]);"
                );

                return;
            }

            if (tableCount != 1)
            {
                throw new InvalidDataException("The JasonQuery security metadata table is ambiguous.");
            }

            ValidateMigrationCompatibleMetadataColumns(database);
            ValidateMigrationCompatibleMetadataIndex(database);
        }

        private static void ValidateMigrationCompatibleMetadataColumns(sqlite3 database)
        {
            sqlite3_stmt statement = null;
            var columns = new List<MetadataColumn>();

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_prepare_v2(database, "PRAGMA table_info('" + MetadataTableName + "');", out statement),
                    "read fresh security metadata columns"
                );

                while (true)
                {
                    var rc = raw.sqlite3_step(statement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        break;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        ModernSqlCipherRuntime.CheckRc(database, rc, "step fresh security metadata columns");
                    }

                    columns.Add
                    (
                        new MetadataColumn
                        (
                            checked((int)raw.sqlite3_column_int64(statement, 0)),
                            ReadText(statement, 1),
                            ReadText(statement, 2),
                            raw.sqlite3_column_int64(statement, 3) != 0,
                            checked((int)raw.sqlite3_column_int64(statement, 5))
                        )
                    );
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }

            if (columns.Count != 3 || !columns[0].Matches(0, "PID", "INTEGER", false, 1) || !columns[1].Matches(1, "MetadataKey", "TEXT", true, 0) || !columns[2].Matches(2, "MetadataValue", "TEXT", true, 0))
            {
                throw new InvalidDataException
                (
                    "The JasonQuery security metadata table is not in the migration-compatible shape required by fresh Storage V2."
                );
            }
        }

        private static void ValidateMigrationCompatibleMetadataIndex(sqlite3 database)
        {
            sqlite3_stmt statement = null;
            var indexes = new List<MetadataIndex>();

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_prepare_v2(database, "PRAGMA index_list('" + MetadataTableName + "');", out statement),
                    "read fresh security metadata indexes"
                );

                while (true)
                {
                    var rc = raw.sqlite3_step(statement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        break;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        ModernSqlCipherRuntime.CheckRc(database, rc, "step fresh security metadata indexes");
                    }

                    indexes.Add
                    (
                        new MetadataIndex
                        (
                            ReadText(statement, 1),
                            raw.sqlite3_column_int64(statement, 2) != 0,
                            ReadText(statement, 3),
                            raw.sqlite3_column_int64(statement, 4) != 0
                        )
                    );
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }

            if (indexes.Count != 1 || !string.Equals(indexes[0].Name, MetadataUniqueIndexName, StringComparison.Ordinal)
                || !indexes[0].Unique || !string.Equals(indexes[0].Origin, "c", StringComparison.Ordinal) || indexes[0].Partial)
            {
                throw new InvalidDataException
                (
                    "The JasonQuery security metadata unique-index shape is not migration-compatible."
                );
            }

            statement = null;
            var indexColumns = new List<string>();

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_prepare_v2(database, "PRAGMA index_info('" + MetadataUniqueIndexName + "');", out statement),
                    "read fresh security metadata unique-index columns"
                );

                while (true)
                {
                    var rc = raw.sqlite3_step(statement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        break;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        ModernSqlCipherRuntime.CheckRc(database, rc, "step fresh security metadata unique-index columns");
                    }

                    indexColumns.Add(ReadText(statement, 2));
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }

            if (indexColumns.Count != 1 || !string.Equals(indexColumns[0], "MetadataKey", StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    "The JasonQuery security metadata unique index does not target MetadataKey exactly."
                );
            }
        }

        private static void ValidateConnectionCredentialStorageVersion(sqlite3 database)
        {
            sqlite3_stmt statement = null;
            byte[] keyBytes = null;

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_prepare_v2
                    (
                        database,
                        "SELECT [MetadataValue] FROM [" + MetadataTableName + "] WHERE [MetadataKey] = ?1;",
                        out statement
                    ),
                    "prepare connection credential storage marker validation"
                );

                keyBytes = StrictUtf8.GetBytes(MetadataVersionKey);

                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_bind_text(statement, 1, new ReadOnlySpan<byte>(keyBytes)),
                    "bind connection credential storage marker validation key"
                );

                var rc = raw.sqlite3_step(statement);

                if (rc != raw.SQLITE_ROW)
                {
                    ModernSqlCipherRuntime.CheckRc(database, rc, "read connection credential storage marker");
                }

                var persistedValue = ReadText(statement, 0);

                if (raw.sqlite3_step(statement) != raw.SQLITE_DONE)
                {
                    throw new InvalidDataException("Multiple connection credential storage markers exist.");
                }

                var expectedValue = ConnectionCredentialStorageContract.CurrentVersion.ToString(CultureInfo.InvariantCulture);

                if (!string.Equals(persistedValue, expectedValue, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The fresh Storage V2 connection credential storage marker is invalid.");
                }

                ConnectionCredentialStorageContract.EnsureV2Ready
                (
                    int.Parse(persistedValue, NumberStyles.Integer, CultureInfo.InvariantCulture)
                );
            }
            finally
            {
                Clear(keyBytes);
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }
        }

        private static void ValidateFinalEncryptedTarget(string databasePath, byte[] databasePasswordUtf8)
        {
            sqlite3 database = null;

            try
            {
                database = ModernSqlCipherRuntime.OpenReadOnly(databasePath, databasePasswordUtf8);
                ModernSqlCipherRuntime.ValidateFrozenProfile(database);
                ModernSqlCipherRuntime.ValidateIntegrity(database);
                ValidateSystemConfigAnchor(database, "sqlite_schema");
                ValidateMigrationCompatibleMetadataColumns(database);
                ValidateMigrationCompatibleMetadataIndex(database);
                ValidateConnectionCredentialStorageVersion(database);
            }
            finally
            {
                ModernSqlCipherRuntime.CloseDatabase(database);
            }
        }

        private static string ComputeLogicalDigest(sqlite3 database)
        {
            using (var canonical = new MemoryStream())
            using (var writer = new BinaryWriter(canonical, StrictUtf8, true))
            {
                writer.Write(QueryInt64(database, "PRAGMA user_version;"));
                writer.Write(QueryInt64(database, "PRAGMA application_id;"));
                writer.Write(QueryInt64(database, "PRAGMA auto_vacuum;"));
                writer.Write(QueryText(database, "PRAGMA encoding;") ?? string.Empty);

                WriteSchemaDigest(writer, database);

                var tableNames = ReadSingleTextColumn
                (
                    database,
                    "SELECT name FROM sqlite_schema WHERE type='table' ORDER BY name COLLATE BINARY;"
                );

                writer.Write(tableNames.Count);

                for (var tableIndex = 0; tableIndex < tableNames.Count; tableIndex++)
                {
                    var tableName = tableNames[tableIndex];

                    writer.Write(tableName);

                    var rowDigests = ReadRowDigests
                    (
                        database,
                        "SELECT * FROM " + QuoteIdentifier(tableName) + ";"
                    );

                    rowDigests.Sort(StringComparer.Ordinal);
                    writer.Write(rowDigests.Count);

                    for (var rowIndex = 0; rowIndex < rowDigests.Count; rowIndex++)
                    {
                        writer.Write(rowDigests[rowIndex]);
                    }
                }

                writer.Flush();
                canonical.Position = 0;

                using (var sha256 = SHA256.Create())
                {
                    return ToHex(sha256.ComputeHash(canonical));
                }
            }
        }

        private static void WriteSchemaDigest(BinaryWriter writer, sqlite3 database)
        {
            sqlite3_stmt statement = null;
            var rows = new List<string>();

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_prepare_v2
                    (
                        database,
                        "SELECT type,name,tbl_name,COALESCE(sql,'') FROM sqlite_schema " +
                        "ORDER BY type COLLATE BINARY,name COLLATE BINARY,tbl_name COLLATE BINARY;",
                        out statement
                    ),
                    "prepare fresh-template schema digest"
                );

                while (true)
                {
                    var rc = raw.sqlite3_step(statement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        break;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        ModernSqlCipherRuntime.CheckRc(database, rc, "step fresh-template schema digest");
                    }

                    rows.Add
                    (
                        ReadText(statement, 0) + "\u001F" +
                        ReadText(statement, 1) + "\u001F" +
                        ReadText(statement, 2) + "\u001F" +
                        ReadText(statement, 3)
                    );
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }

            writer.Write(rows.Count);

            for (var index = 0; index < rows.Count; index++)
            {
                writer.Write(rows[index]);
            }
        }

        private static List<string> ReadSingleTextColumn(sqlite3 database, string sql)
        {
            var result = new List<string>();
            sqlite3_stmt statement = null;

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_prepare_v2(database, sql, out statement),
                    "prepare fresh logical text-list query"
                );

                while (true)
                {
                    var rc = raw.sqlite3_step(statement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        break;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        ModernSqlCipherRuntime.CheckRc(database, rc, "step fresh logical text-list query");
                    }

                    result.Add(ReadText(statement, 0));
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }

            return result;
        }

        private static List<string> ReadRowDigests(sqlite3 database, string sql)
        {
            var rows = new List<string>();
            sqlite3_stmt statement = null;

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_prepare_v2(database, sql, out statement),
                    "prepare fresh logical table digest"
                );

                var columnCount = raw.sqlite3_column_count(statement);

                while (true)
                {
                    var rc = raw.sqlite3_step(statement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        break;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        ModernSqlCipherRuntime.CheckRc(database, rc, "step fresh logical table digest");
                    }

                    using (var row = new MemoryStream())
                    using (var writer = new BinaryWriter(row, StrictUtf8, true))
                    {
                        writer.Write(columnCount);

                        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                        {
                            WriteValue(writer, statement, columnIndex);
                        }

                        writer.Flush();
                        row.Position = 0;

                        using (var sha256 = SHA256.Create())
                        {
                            rows.Add(ToHex(sha256.ComputeHash(row)));
                        }
                    }
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }

            return rows;
        }

        private static void WriteValue(BinaryWriter writer, sqlite3_stmt statement, int columnIndex)
        {
            var type = raw.sqlite3_column_type(statement, columnIndex);

            writer.Write((byte)type);

            switch (type)
            {
                case raw.SQLITE_NULL:
                    {
                        return;
                    }
                case raw.SQLITE_INTEGER:
                    {
                        writer.Write(raw.sqlite3_column_int64(statement, columnIndex));
                        return;
                    }
                case raw.SQLITE_FLOAT:
                    {
                        writer.Write(BitConverter.DoubleToInt64Bits(raw.sqlite3_column_double(statement, columnIndex)));
                        return;
                    }
                case raw.SQLITE_TEXT:
                    {
                        var value = StrictUtf8.GetBytes(ReadText(statement, columnIndex));

                        writer.Write(value.Length);
                        writer.Write(value);
                        Clear(value);

                        return;
                    }
                case raw.SQLITE_BLOB:
                    {
                        var value = raw.sqlite3_column_blob(statement, columnIndex).ToArray();

                        writer.Write(value.Length);
                        writer.Write(value);
                        Clear(value);

                        return;
                    }
                default:
                    {
                        throw new InvalidDataException("Unexpected SQLite value type in fresh logical digest.");
                    }
            }
        }

        private static void ValidateSystemConfigAnchor(sqlite3 database, string schemaTable)
        {
            var count = QueryInt64
            (
                database,
                "SELECT count(*) FROM " + schemaTable + " WHERE type='table' AND name='SystemConfig';"
            );

            if (count != 1L)
            {
                throw new InvalidDataException("The fresh JasonQuery database does not contain the SystemConfig template anchor.");
            }
        }

        private static void SetAttachedIntegerPragma(sqlite3 database, string pragmaName, long value)
        {
            ModernSqlCipherRuntime.Exec
            (
                database,
                "PRAGMA " +
                AttachedSchemaName +
                "." +
                pragmaName +
                " = " +
                value.ToString(CultureInfo.InvariantCulture) +
                ";"
            );
        }

        private static void BindText(sqlite3 database, sqlite3_stmt statement, int index, string value, string operation)
        {
            var bytes = StrictUtf8.GetBytes(value);

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_bind_text(statement, index, new ReadOnlySpan<byte>(bytes)),
                    operation
                );
            }
            finally
            {
                Clear(bytes);
            }
        }

        private static void ExecuteToCompletion(sqlite3 database, string sql, string operation)
        {
            sqlite3_stmt statement = null;

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    database,
                    raw.sqlite3_prepare_v2(database, sql, out statement),
                    operation + " prepare"
                );

                while (true)
                {
                    var rc = raw.sqlite3_step(statement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        return;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        ModernSqlCipherRuntime.CheckRc(database, rc, operation + " step");
                    }
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }
        }

        private static void StepExpectDone(sqlite3 database, sqlite3_stmt statement, string operation)
        {
            var rc = raw.sqlite3_step(statement);

            if (rc != raw.SQLITE_DONE)
            {
                ModernSqlCipherRuntime.CheckRc(database, rc, operation);
            }
        }

        private static long QueryInt64(sqlite3 database, string sql)
        {
            return ModernSqlCipherRuntime.QueryInt64(database, sql);
        }

        private static string QueryText(sqlite3 database, string sql)
        {
            return ModernSqlCipherRuntime.QueryText(database, sql);
        }

        private static string ReadText(sqlite3_stmt statement, int columnIndex)
        {
            if (raw.sqlite3_column_type(statement, columnIndex) == raw.SQLITE_NULL)
            {
                return string.Empty;
            }

            return raw.sqlite3_column_text(statement, columnIndex).utf8_to_string() ?? string.Empty;
        }

        private static string QuoteIdentifier(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static void AssertEqual(long expected, long actual, string description)
        {
            if (expected != actual)
            {
                throw new InvalidDataException
                (
                    description +
                    " mismatch. Expected=" +
                    expected.ToString(CultureInfo.InvariantCulture) +
                    " Actual=" +
                    actual.ToString(CultureInfo.InvariantCulture)
                );
            }
        }

        private static void AssertEqual(string expected, string actual, string description)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    description +
                    " mismatch. Expected='" +
                    (expected ?? string.Empty) +
                    "' Actual='" +
                    (actual ?? string.Empty) +
                    "'."
                );
            }
        }

        private static void ValidateAutoVacuum(long value)
        {
            if (value < 0L || value > 2L)
            {
                throw new InvalidDataException("The fresh template auto_vacuum value is unsupported.");
            }
        }

        private static void ValidateEncoding(string value)
        {
            if (string.Equals(value, "UTF-8", StringComparison.Ordinal) || string.Equals(value, "UTF-16le", StringComparison.Ordinal) || string.Equals(value, "UTF-16be", StringComparison.Ordinal))
            {
                return;
            }

            throw new InvalidDataException("The fresh template SQLite encoding is unsupported.");
        }

        private static void ValidateCreateArguments(string databasePath, byte[] databasePasswordUtf8, byte[] templateBytes)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A fresh Storage V2 database path is required.", nameof(databasePath));
            }

            if (databasePasswordUtf8 == null || databasePasswordUtf8.Length == 0)
            {
                throw new ArgumentException("A fresh Storage V2 database credential is required.", nameof(databasePasswordUtf8));
            }

            if (templateBytes == null || templateBytes.Length == 0 || templateBytes.Length > ModernDatabaseRuntimeProtocol.MaxFreshTemplateBytes)
            {
                throw new ArgumentException("The fresh Storage V2 template payload is invalid.", nameof(templateBytes));
            }
        }

        private static string ToHex(byte[] value)
        {
            var builder = new StringBuilder(value.Length * 2);

            for (var index = 0; index < value.Length; index++)
            {
                builder.Append(value[index].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static void DeleteFileIfExists(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static void Clear(byte[] value)
        {
            if (value != null)
            {
                Array.Clear(value, 0, value.Length);
            }
        }

        private sealed class MetadataColumn
        {
            internal MetadataColumn(int cid, string name, string declaredType, bool notNull, int primaryKeyOrdinal)
            {
                Cid = cid;
                Name = name;
                DeclaredType = declaredType;
                NotNull = notNull;
                PrimaryKeyOrdinal = primaryKeyOrdinal;
            }

            internal int Cid { get; }

            internal string Name { get; }

            internal string DeclaredType { get; }

            internal bool NotNull { get; }

            internal int PrimaryKeyOrdinal { get; }

            internal bool Matches(int cid, string name, string declaredType, bool notNull, int primaryKeyOrdinal)
            {
                return Cid == cid && string.Equals(Name, name, StringComparison.Ordinal) && string.Equals(DeclaredType, declaredType, StringComparison.OrdinalIgnoreCase)
                       && NotNull == notNull && PrimaryKeyOrdinal == primaryKeyOrdinal;
            }
        }

        private sealed class MetadataIndex
        {
            internal MetadataIndex(string name, bool unique, string origin, bool partial)
            {
                Name = name;
                Unique = unique;
                Origin = origin;
                Partial = partial;
            }

            internal string Name { get; }

            internal bool Unique { get; }

            internal string Origin { get; }

            internal bool Partial { get; }
        }
    }
}
