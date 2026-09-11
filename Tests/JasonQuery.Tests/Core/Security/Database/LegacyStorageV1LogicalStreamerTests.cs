using JasonQuery.Core.Security.Database;
using JasonQuery.LegacyDbMigration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class LegacyStorageV1LogicalStreamerTests
    {
        private const string DatabasePassword = "Step389D2-Test-密碼";

        [TestMethod]
        public void Stream_EmitsSchemaRowsSequenceAndSecondarySchemaDeterministically()
        {
            using (var scope = new TestDatabaseScope())
            {
                CreateQualifiedDatabase(scope.DatabasePath, DatabasePassword);

                var beforeHash = ComputeSha256(scope.DatabasePath);
                var beforeLength = new FileInfo(scope.DatabasePath).Length;
                var beforeWriteTime = File.GetLastWriteTimeUtc(scope.DatabasePath);

                using (var output = new MemoryStream())
                {
                    var streamer = new LegacyStorageV1LogicalStreamer();

                    streamer.Stream
                    (
                        scope.DatabasePath,
                        Encoding.UTF8.GetBytes(DatabasePassword),
                        output
                    );

                    CollectionAssert.AreEqual(beforeHash, ComputeSha256(scope.DatabasePath));
                    Assert.AreEqual(beforeLength, new FileInfo(scope.DatabasePath).Length);
                    Assert.AreEqual(beforeWriteTime, File.GetLastWriteTimeUtc(scope.DatabasePath));
                    Assert.IsFalse(File.Exists(scope.DatabasePath + "-wal"));
                    Assert.IsFalse(File.Exists(scope.DatabasePath + "-shm"));
                    Assert.IsFalse(File.Exists(scope.DatabasePath + "-journal"));

                    output.Position = 0;

                    var ready = DatabaseStorageMigrationWireProtocol.ReadResponse(output);

                    Assert.AreEqual
                    (
                        DatabaseStorageMigrationWireResponseStatus.StorageV1LogicalStreamReady,
                        ready.Status
                    );

                    using (var beginDatabase = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(output))
                    {
                        var payload = beginDatabase.GetPayload<DatabaseStorageMigrationBeginDatabase>();

                        Assert.AreEqual(7, payload.Metadata.UserVersion);
                        Assert.AreEqual(42, payload.Metadata.ApplicationId);
                        Assert.AreEqual("UTF-8", payload.Metadata.SourceEncodingName);
                        Assert.AreEqual(2, payload.TableCount);
                        Assert.AreEqual(5, payload.SchemaObjectCount);
                    }

                    AssertFrameKind
                    (
                        output,
                        DatabaseStorageMigrationLogicalFrameKind.BeginSchema
                    );

                    var tableNames = new List<string>();

                    for (var tableIndex = 0; tableIndex < 2; tableIndex++)
                    {
                        using (var tableFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(output))
                        {
                            var table = tableFrame.GetPayload<DatabaseStorageMigrationTableDefinition>();

                            tableNames.Add(table.Name);

                            for (var columnIndex = 0; columnIndex < table.ColumnCount; columnIndex++)
                            {
                                AssertFrameKind
                                (
                                    output,
                                    DatabaseStorageMigrationLogicalFrameKind.Column
                                );
                            }

                            AssertFrameKind
                            (
                                output,
                                DatabaseStorageMigrationLogicalFrameKind.EndTable
                            );
                        }
                    }

                    CollectionAssert.AreEqual
                    (
                        new[]
                        {
                            "DataTypes",
                            "SystemConfig"
                        },
                        tableNames
                    );

                    AssertFrameKind
                    (
                        output,
                        DatabaseStorageMigrationLogicalFrameKind.EndSchema
                    );

                    var rowValues = ReadAllRows(output, 2);

                    CollectionAssert.AreEqual
                    (
                        new long[] { 1L, 3L },
                        rowValues["DataTypes"].Select(row => (long)row[0]).ToArray()
                    );

                    Assert.AreEqual("第一列🙂", rowValues["DataTypes"][0][1]);

                    CollectionAssert.AreEqual
                    (
                        new byte[] { 0x00, 0x01, 0xFE, 0xFF },
                        (byte[])rowValues["DataTypes"][0][2]
                    );

                    Assert.AreEqual(123.5D, rowValues["DataTypes"][0][3]);
                    Assert.IsNull(rowValues["DataTypes"][0][4]);
                    Assert.AreEqual(99L, rowValues["DataTypes"][0][5]);
                    Assert.AreEqual("dynamic-text", rowValues["DataTypes"][1][5]);

                    Assert.AreEqual
                    (
                        new string
                        (
                            'A',
                            DatabaseStorageMigrationLogicalStreamContract.MaxValueChunkBytes - 1
                        ) + "🙂B",
                        rowValues["DataTypes"][0][6]
                    );

                    Assert.AreEqual("short", rowValues["DataTypes"][1][6]);

                    AssertFrameKind
                    (
                        output,
                        DatabaseStorageMigrationLogicalFrameKind.BeginSequenceState
                    );

                    var sequenceEntries = new List<DatabaseStorageMigrationSequenceEntry>();

                    for (var index = 0; index < 2; index++)
                    {
                        using (var frame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(output))
                        {
                            sequenceEntries.Add
                            (
                                frame.GetPayload<DatabaseStorageMigrationSequenceEntry>()
                            );
                        }
                    }

                    Assert.AreEqual("DataTypes", sequenceEntries[0].TableName);
                    Assert.AreEqual(3L, sequenceEntries[0].SequenceValue);
                    Assert.AreEqual("SystemConfig", sequenceEntries[1].TableName);
                    Assert.AreEqual(1L, sequenceEntries[1].SequenceValue);

                    AssertFrameKind
                    (
                        output,
                        DatabaseStorageMigrationLogicalFrameKind.EndSequenceState
                    );

                    using (var beginSecondary = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(output))
                    {
                        Assert.AreEqual
                        (
                            DatabaseStorageMigrationLogicalFrameKind.BeginSecondarySchema,
                            beginSecondary.Kind
                        );

                        Assert.AreEqual(3, beginSecondary.GetPayload<int>());
                    }

                    var secondaryKinds = new List<DatabaseStorageMigrationSchemaObjectKind>();

                    for (var index = 0; index < 3; index++)
                    {
                        using (var frame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(output))
                        {
                            secondaryKinds.Add
                            (
                                frame.GetPayload<DatabaseStorageMigrationSecondarySchemaObject>().Kind
                            );
                        }
                    }

                    CollectionAssert.AreEqual
                    (
                        new[]
                        {
                            DatabaseStorageMigrationSchemaObjectKind.Index,
                            DatabaseStorageMigrationSchemaObjectKind.View,
                            DatabaseStorageMigrationSchemaObjectKind.Trigger
                        },
                        secondaryKinds
                    );

                    AssertFrameKind
                    (
                        output,
                        DatabaseStorageMigrationLogicalFrameKind.EndSecondarySchema
                    );

                    AssertFrameKind
                    (
                        output,
                        DatabaseStorageMigrationLogicalFrameKind.EndDatabase
                    );

                    AssertFrameKind
                    (
                        output,
                        DatabaseStorageMigrationLogicalFrameKind.EndStream
                    );

                    Assert.AreEqual(output.Length, output.Position);
                }
            }
        }

        [TestMethod]
        public void Stream_RejectsWithoutRowIdTable()
        {
            using (var scope = new TestDatabaseScope())
            {
                CreateDatabase
                (
                    scope.DatabasePath,
                    DatabasePassword,
                    connection =>
                    {
                        ExecuteNonQuery
                        (
                            connection,
                            "CREATE TABLE SystemConfig " +
                            "(PID INTEGER PRIMARY KEY AUTOINCREMENT, Value TEXT)"
                        );

                        ExecuteNonQuery
                        (
                            connection,
                            "CREATE TABLE Unsupported " +
                            "(PID INTEGER PRIMARY KEY, Value TEXT) WITHOUT ROWID"
                        );
                    }
                );

                using (var output = new MemoryStream())
                {
                    var streamer = new LegacyStorageV1LogicalStreamer();

                    Assert.ThrowsException<NotSupportedException>
                    (
                        () => streamer.Stream
                        (
                            scope.DatabasePath,
                            Encoding.UTF8.GetBytes(DatabasePassword),
                            output
                        )
                    );

                    Assert.AreEqual(0L, output.Length);
                }
            }
        }

        [TestMethod]
        public void Stream_RejectsNonRowIdAliasPrimaryKey()
        {
            using (var scope = new TestDatabaseScope())
            {
                CreateDatabase
                (
                    scope.DatabasePath,
                    DatabasePassword,
                    connection =>
                    {
                        ExecuteNonQuery
                        (
                            connection,
                            "CREATE TABLE SystemConfig " +
                            "(PID INTEGER PRIMARY KEY AUTOINCREMENT, Value TEXT)"
                        );

                        ExecuteNonQuery
                        (
                            connection,
                            "CREATE TABLE Unsupported " +
                            "(PID TEXT PRIMARY KEY, Value TEXT)"
                        );
                    }
                );

                using (var output = new MemoryStream())
                {
                    var streamer = new LegacyStorageV1LogicalStreamer();

                    Assert.ThrowsException<NotSupportedException>
                    (
                        () => streamer.Stream
                        (
                            scope.DatabasePath,
                            Encoding.UTF8.GetBytes(DatabasePassword),
                            output
                        )
                    );

                    Assert.AreEqual(0L, output.Length);
                }
            }
        }

        [TestMethod]
        public void Stream_WrongPasswordFailsWithoutLogicalOutput()
        {
            using (var scope = new TestDatabaseScope())
            {
                CreateQualifiedDatabase(scope.DatabasePath, DatabasePassword);

                using (var output = new MemoryStream())
                {
                    var streamer = new LegacyStorageV1LogicalStreamer();

                    Assert.ThrowsException<SQLiteException>
                    (
                        () => streamer.Stream
                        (
                            scope.DatabasePath,
                            Encoding.UTF8.GetBytes("Wrong-Password"),
                            output
                        )
                    );

                    Assert.AreEqual(0L, output.Length);
                }
            }
        }

        private static Dictionary<string, List<object[]>> ReadAllRows(Stream stream, int tableCount)
        {
            var result = new Dictionary<string, List<object[]>>(StringComparer.Ordinal);
            var tableNamesById = new Dictionary<int, string>();

            tableNamesById.Add(0, "DataTypes");
            tableNamesById.Add(1, "SystemConfig");

            for (var tableIndex = 0; tableIndex < tableCount; tableIndex++)
            {
                using (var beginRows = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual
                    (
                        DatabaseStorageMigrationLogicalFrameKind.BeginRows,
                        beginRows.Kind
                    );

                    var tableId = beginRows.GetPayload<int>();
                    var tableName = tableNamesById[tableId];
                    var rows = new List<object[]>();

                    while (true)
                    {
                        using (var frame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                        {
                            if (frame.Kind == DatabaseStorageMigrationLogicalFrameKind.EndRows)
                            {
                                var endRows = frame.GetPayload<DatabaseStorageMigrationEndRows>();

                                Assert.AreEqual(rows.Count, endRows.RowCount);
                                break;
                            }

                            Assert.AreEqual
                            (
                                DatabaseStorageMigrationLogicalFrameKind.BeginRow,
                                frame.Kind
                            );

                            var fieldCount = frame.GetPayload<int>();
                            var values = new object[fieldCount];

                            for (var fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
                            {
                                values[fieldIndex] = ReadValue(stream);
                            }

                            AssertFrameKind
                            (
                                stream,
                                DatabaseStorageMigrationLogicalFrameKind.EndRow
                            );

                            rows.Add(values);
                        }
                    }

                    result.Add(tableName, rows);
                }
            }

            return result;
        }

        private static object ReadValue(Stream stream)
        {
            using (var frame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
            {
                if (frame.Kind == DatabaseStorageMigrationLogicalFrameKind.NullValue)
                {
                    return null;
                }

                if (frame.Kind == DatabaseStorageMigrationLogicalFrameKind.Int64Value)
                {
                    return frame.GetPayload<long>();
                }

                if (frame.Kind == DatabaseStorageMigrationLogicalFrameKind.DoubleValue)
                {
                    return frame.GetPayload<double>();
                }

                if (frame.Kind == DatabaseStorageMigrationLogicalFrameKind.BeginTextUtf8)
                {
                    var expectedLength = frame.GetPayload<long>();

                    var bytes = ReadChunkedValue
                    (
                        stream,
                        DatabaseStorageMigrationLogicalFrameKind.TextUtf8Chunk,
                        DatabaseStorageMigrationLogicalFrameKind.EndTextUtf8
                    );

                    Assert.AreEqual(expectedLength, bytes.LongLength);
                    return new UTF8Encoding(false, true).GetString(bytes);
                }

                if (frame.Kind == DatabaseStorageMigrationLogicalFrameKind.BeginBlob)
                {
                    var expectedLength = frame.GetPayload<long>();

                    var bytes = ReadChunkedValue
                    (
                        stream,
                        DatabaseStorageMigrationLogicalFrameKind.BlobChunk,
                        DatabaseStorageMigrationLogicalFrameKind.EndBlob
                    );

                    Assert.AreEqual(expectedLength, bytes.LongLength);
                    return bytes;
                }

                Assert.Fail("Unexpected value frame: " + frame.Kind);
                return null;
            }
        }

        private static byte[] ReadChunkedValue(Stream stream, DatabaseStorageMigrationLogicalFrameKind chunkKind,
                                               DatabaseStorageMigrationLogicalFrameKind endKind)
        {
            using (var buffer = new MemoryStream())
            {
                while (true)
                {
                    using (var frame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                    {
                        if (frame.Kind == endKind)
                        {
                            return buffer.ToArray();
                        }

                        Assert.AreEqual(chunkKind, frame.Kind);

                        var chunk = frame.GetPayload<DatabaseStorageMigrationLogicalChunk>();

                        buffer.Write(chunk.Bytes, 0, chunk.Bytes.Length);
                    }
                }
            }
        }

        private static void AssertFrameKind(Stream stream, DatabaseStorageMigrationLogicalFrameKind expected)
        {
            using (var frame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
            {
                Assert.AreEqual(expected, frame.Kind);
            }
        }

        private static void CreateQualifiedDatabase(string databasePath, string password )
        {
            CreateDatabase
            (
                databasePath,
                password,
                connection =>
                {
                    ExecuteNonQuery(connection, "PRAGMA user_version = 7");
                    ExecuteNonQuery(connection, "PRAGMA application_id = 42");

                    ExecuteNonQuery
                    (
                        connection,
                        "CREATE TABLE SystemConfig " +
                        "(PID INTEGER PRIMARY KEY AUTOINCREMENT, Value TEXT)"
                    );

                    ExecuteNonQuery
                    (
                        connection,
                        "INSERT INTO SystemConfig (Value) VALUES ('config')"
                    );

                    ExecuteNonQuery
                    (
                        connection,
                        "CREATE TABLE DataTypes " +
                        "(PID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "TextValue TEXT, BlobValue BLOB, RealValue REAL, NullValue TEXT, DynamicValue, " +
                        "LongTextValue TEXT)"
                    );

                    using (var command = new SQLiteCommand("INSERT INTO DataTypes " +
                                                           "(PID, TextValue, BlobValue, RealValue, NullValue, DynamicValue, LongTextValue) " +
                                                           "VALUES (@PID, @TextValue, @BlobValue, @RealValue, @NullValue, @DynamicValue, @LongTextValue)", connection))
                    {
                        command.Parameters.AddWithValue("@PID", 3L);
                        command.Parameters.AddWithValue("@TextValue", "第三列");
                        command.Parameters.AddWithValue("@BlobValue", new byte[] { 0x10, 0x20 });
                        command.Parameters.AddWithValue("@RealValue", -1.25D);
                        command.Parameters.AddWithValue("@NullValue", "not-null");
                        command.Parameters.AddWithValue("@DynamicValue", "dynamic-text");
                        command.Parameters.AddWithValue("@LongTextValue", "short");
                        command.ExecuteNonQuery();

                        command.Parameters["@PID"].Value = 1L;
                        command.Parameters["@TextValue"].Value = "第一列🙂";
                        command.Parameters["@BlobValue"].Value = new byte[] { 0x00, 0x01, 0xFE, 0xFF };
                        command.Parameters["@RealValue"].Value = 123.5D;
                        command.Parameters["@NullValue"].Value = DBNull.Value;
                        command.Parameters["@DynamicValue"].DbType = System.Data.DbType.Int64;
                        command.Parameters["@DynamicValue"].Value = 99L;
                        command.Parameters["@LongTextValue"].Value = new string('A', DatabaseStorageMigrationLogicalStreamContract.MaxValueChunkBytes - 1) + "🙂B";
                        command.ExecuteNonQuery();
                    }

                    ExecuteNonQuery
                    (
                        connection,
                        "CREATE INDEX IX_DataTypes_TextValue ON DataTypes(TextValue)"
                    );

                    ExecuteNonQuery
                    (
                        connection,
                        "CREATE VIEW V_DataTypes AS SELECT PID, TextValue FROM DataTypes"
                    );

                    ExecuteNonQuery
                    (
                        connection,
                        "CREATE TRIGGER TR_DataTypes_Update " +
                        "AFTER UPDATE OF TextValue ON DataTypes " +
                        "BEGIN SELECT 1; END"
                    );
                }
            );
        }

        private static void CreateDatabase(string databasePath, string password, Action<SQLiteConnection> initialize)
        {
            SQLiteConnection.CreateFile(databasePath);

            try
            {
                using (var connection = new SQLiteConnection { ConnectionString = "Data Source=" + databasePath + ";" + "Version=3;New=False;Pooling=False;" })
                {
                    connection.Open();
                    initialize(connection);
                    connection.ChangePassword(password);
                }
            }
            finally
            {
                SQLiteConnection.ClearAllPools();
            }
        }

        private static void ExecuteNonQuery(SQLiteConnection connection, string sql)
        {
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static byte[] ComputeSha256(string path)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                return sha256.ComputeHash(stream);
            }
        }

        private sealed class TestDatabaseScope : IDisposable
        {
            public TestDatabaseScope()
            {
                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-Step389D2-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);
                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public void Dispose()
            {
                SQLiteConnection.ClearAllPools();

                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }
    }
}
