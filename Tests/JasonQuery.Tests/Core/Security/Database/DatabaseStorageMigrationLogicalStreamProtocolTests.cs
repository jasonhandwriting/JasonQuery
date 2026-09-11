using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class DatabaseStorageMigrationLogicalStreamProtocolTests
    {
        [TestMethod]
        public void BeginDatabase_RoundTrips()
        {
            using (var stream = new MemoryStream())
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginDatabase
                (
                    stream,
                    new DatabaseStorageMigrationDatabaseMetadata(0, 0, "UTF-8"),
                    3,
                    3
                );

                stream.Position = 0;

                using (var frame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(DatabaseStorageMigrationLogicalFrameKind.BeginDatabase, frame.Kind);

                    var payload = frame.GetPayload<DatabaseStorageMigrationBeginDatabase>();

                    Assert.AreEqual(0, payload.Metadata.UserVersion);
                    Assert.AreEqual(0, payload.Metadata.ApplicationId);
                    Assert.AreEqual("UTF-8", payload.Metadata.SourceEncodingName);
                    Assert.AreEqual(3, payload.TableCount);
                    Assert.AreEqual(3, payload.SchemaObjectCount);
                }
            }
        }

        [TestMethod]
        public void TableAndColumn_RoundTripHistoricalShape()
        {
            using (var stream = new MemoryStream())
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginTable
                (
                    stream,
                    new DatabaseStorageMigrationTableDefinition
                    (
                        0,
                        "SystemConfig",
                        "CREATE TABLE SystemConfig (PID INTEGER PRIMARY KEY AUTOINCREMENT, AttributeKey TEXT)",
                        2,
                        0,
                        true
                    )
                );

                DatabaseStorageMigrationLogicalStreamProtocol.WriteColumn
                (
                    stream,
                    0,
                    new DatabaseStorageMigrationColumnDefinition
                    (
                        0,
                        "PID",
                        "INTEGER",
                        false,
                        1,
                        0,
                        null
                    )
                );

                stream.Position = 0;

                using (var tableFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var table = tableFrame.GetPayload<DatabaseStorageMigrationTableDefinition>();

                    Assert.AreEqual("SystemConfig", table.Name);
                    Assert.AreEqual(0, table.RowIdAliasColumnCid);
                    Assert.IsTrue(table.HasAutoincrement);
                }

                using (var columnFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var payload = columnFrame.GetPayload<DatabaseStorageMigrationColumnFrame>();

                    Assert.AreEqual(0, payload.TableId);
                    Assert.AreEqual("PID", payload.Column.Name);
                    Assert.AreEqual("INTEGER", payload.Column.DeclaredType);
                    Assert.AreEqual(1, payload.Column.PrimaryKeyOrdinal);
                    Assert.IsNull(payload.Column.DefaultSql);
                }
            }
        }

        [TestMethod]
        public void TypedScalarValues_RoundTrip()
        {
            using (var stream = new MemoryStream())
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteNullValue(stream);
                DatabaseStorageMigrationLogicalStreamProtocol.WriteInt64Value(stream, 123456789L);
                DatabaseStorageMigrationLogicalStreamProtocol.WriteDoubleValue(stream, 123.5D);

                stream.Position = 0;

                using (var nullFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(DatabaseStorageMigrationLogicalFrameKind.NullValue, nullFrame.Kind);
                    Assert.IsNull(nullFrame.Payload);
                }

                using (var intFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(123456789L, intFrame.GetPayload<long>());
                }

                using (var doubleFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(123.5D, doubleFrame.GetPayload<double>());
                }
            }
        }

        [TestMethod]
        public void TextAndBlobChunks_AreBoundedAndDisposable()
        {
            var textBytes = Encoding.UTF8.GetBytes("SELECT '測試'");
            var blobBytes = new byte[] { 0x00, 0x01, 0xFE, 0xFF };

            using (var stream = new MemoryStream())
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginTextUtf8(stream, textBytes.Length);

                DatabaseStorageMigrationLogicalStreamProtocol.WriteTextUtf8Chunk
                (
                    stream,
                    textBytes,
                    0,
                    textBytes.Length
                );

                DatabaseStorageMigrationLogicalStreamProtocol.WriteEndTextUtf8(stream);

                DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginBlob(stream, blobBytes.Length);

                DatabaseStorageMigrationLogicalStreamProtocol.WriteBlobChunk
                (
                    stream,
                    blobBytes,
                    0,
                    blobBytes.Length
                );

                DatabaseStorageMigrationLogicalStreamProtocol.WriteEndBlob(stream);

                stream.Position = 0;

                using (var beginText = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual((long)textBytes.Length, beginText.GetPayload<long>());
                }

                byte[] exposedTextBytes;

                using (var textChunkFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var chunk = textChunkFrame.GetPayload<DatabaseStorageMigrationLogicalChunk>();

                    exposedTextBytes = chunk.Bytes;
                    CollectionAssert.AreEqual(textBytes, exposedTextBytes);
                }

                CollectionAssert.AreEqual
                (
                    new byte[exposedTextBytes.Length],
                    exposedTextBytes
                );

                using (var endText = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(DatabaseStorageMigrationLogicalFrameKind.EndTextUtf8, endText.Kind);
                }

                using (var beginBlob = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual((long)blobBytes.Length, beginBlob.GetPayload<long>());
                }

                using (var blobChunkFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var chunk = blobChunkFrame.GetPayload<DatabaseStorageMigrationLogicalChunk>();

                    CollectionAssert.AreEqual(blobBytes, chunk.Bytes);
                }

                using (var endBlob = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(DatabaseStorageMigrationLogicalFrameKind.EndBlob, endBlob.Kind);
                }
            }
        }

        [TestMethod]
        public void SequenceState_RoundTripsWithoutReplayingSqliteSequenceTable()
        {
            using (var stream = new MemoryStream())
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginSequenceState(stream, 1);

                DatabaseStorageMigrationLogicalStreamProtocol.WriteSequenceEntry
                (
                    stream,
                    new DatabaseStorageMigrationSequenceEntry("SQLHistory", 49821)
                );

                DatabaseStorageMigrationLogicalStreamProtocol.WriteEndSequenceState(stream);

                stream.Position = 0;

                using (var beginFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(1, beginFrame.GetPayload<int>());
                }

                using (var entryFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var entry = entryFrame.GetPayload<DatabaseStorageMigrationSequenceEntry>();

                    Assert.AreEqual("SQLHistory", entry.TableName);
                    Assert.AreEqual(49821L, entry.SequenceValue);
                }

                using (var endFrame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(DatabaseStorageMigrationLogicalFrameKind.EndSequenceState, endFrame.Kind);
                }
            }
        }

        [TestMethod]
        public void SecondarySchemaObject_RoundTrips()
        {
            using (var stream = new MemoryStream())
            {
                var schemaObject = new DatabaseStorageMigrationSecondarySchemaObject
                (
                    DatabaseStorageMigrationSchemaObjectKind.Index,
                    "IX_Test",
                    "Test",
                    "CREATE INDEX IX_Test ON Test(Name)"
                );

                DatabaseStorageMigrationLogicalStreamProtocol.WriteSchemaObject(stream, schemaObject);

                stream.Position = 0;

                using (var frame = DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var actual = frame.GetPayload<DatabaseStorageMigrationSecondarySchemaObject>();

                    Assert.AreEqual(DatabaseStorageMigrationSchemaObjectKind.Index, actual.Kind);
                    Assert.AreEqual("IX_Test", actual.Name);
                    Assert.AreEqual("Test", actual.TableName);
                    Assert.AreEqual("CREATE INDEX IX_Test ON Test(Name)", actual.OriginalSql);
                }
            }
        }

        [TestMethod]
        public void OversizedChunk_IsRejectedBeforeWrite()
        {
            var bytes = new byte[DatabaseStorageMigrationLogicalStreamContract.MaxValueChunkBytes + 1];

            using (var stream = new MemoryStream())
            {
                Assert.ThrowsException<InvalidDataException>
                (
                    () => DatabaseStorageMigrationLogicalStreamProtocol.WriteBlobChunk
                    (
                        stream,
                        bytes,
                        0,
                        bytes.Length
                    )
                );

                Assert.AreEqual(0L, stream.Length);
            }
        }

        [TestMethod]
        public void UnknownFrameKind_FailsClosed()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("JQSM"));
                writer.Write(DatabaseStorageMigrationLogicalStreamContract.CurrentVersion);
                writer.Write((byte)255);
                writer.Flush();

                stream.Position = 0;

                Assert.ThrowsException<NotSupportedException>
                (
                    () => DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream)
                );
            }
        }

        [TestMethod]
        public void InvalidUtf8Identifier_FailsClosed()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("JQSM"));
                writer.Write(DatabaseStorageMigrationLogicalStreamContract.CurrentVersion);
                writer.Write((byte)DatabaseStorageMigrationLogicalFrameKind.SequenceEntry);

                writer.Write(2);
                writer.Write(new byte[] { 0xC3, 0x28 });
                writer.Write(1L);
                writer.Flush();

                stream.Position = 0;

                Assert.ThrowsException<InvalidDataException>
                (
                    () => DatabaseStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream)
                );
            }
        }

        [TestMethod]
        public void LogicalStreamReadyResponse_RoundTrips()
        {
            using (var stream = new MemoryStream())
            {
                DatabaseStorageMigrationWireProtocol.WriteStorageV1LogicalStreamReadyResponse(stream);

                stream.Position = 0;

                var response = DatabaseStorageMigrationWireProtocol.ReadResponse(stream);

                Assert.AreEqual
                (
                    DatabaseStorageMigrationWireResponseStatus.StorageV1LogicalStreamReady,
                    response.Status
                );
            }
        }
    }
}
