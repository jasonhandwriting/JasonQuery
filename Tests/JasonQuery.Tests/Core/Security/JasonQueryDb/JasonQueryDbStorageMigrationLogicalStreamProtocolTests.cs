using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationLogicalStreamProtocolTests
    {
        [TestMethod]
        public void BeginDatabase_RoundTrips()
        {
            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteBeginDatabase
                (
                    stream,
                    new JasonQueryDbStorageMigrationDatabaseMetadata(0, 0, "UTF-8"),
                    3,
                    3
                );

                stream.Position = 0;

                using (var frame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(JasonQueryDbStorageMigrationLogicalFrameKind.BeginDatabase, frame.Kind);

                    var payload = frame.GetPayload<JasonQueryDbStorageMigrationBeginDatabase>();

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
                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteBeginTable
                (
                    stream,
                    new JasonQueryDbStorageMigrationTableDefinition
                    (
                        0,
                        "SystemConfig",
                        "CREATE TABLE SystemConfig (PID INTEGER PRIMARY KEY AUTOINCREMENT, AttributeKey TEXT)",
                        2,
                        0,
                        true
                    )
                );

                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteColumn
                (
                    stream,
                    0,
                    new JasonQueryDbStorageMigrationColumnDefinition
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

                using (var tableFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var table = tableFrame.GetPayload<JasonQueryDbStorageMigrationTableDefinition>();

                    Assert.AreEqual("SystemConfig", table.Name);
                    Assert.AreEqual(0, table.RowIdAliasColumnCid);
                    Assert.IsTrue(table.HasAutoincrement);
                }

                using (var columnFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var payload = columnFrame.GetPayload<JasonQueryDbStorageMigrationColumnFrame>();

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
                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteNullValue(stream);
                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteInt64Value(stream, 123456789L);
                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteDoubleValue(stream, 123.5D);

                stream.Position = 0;

                using (var nullFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(JasonQueryDbStorageMigrationLogicalFrameKind.NullValue, nullFrame.Kind);
                    Assert.IsNull(nullFrame.Payload);
                }

                using (var intFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(123456789L, intFrame.GetPayload<long>());
                }

                using (var doubleFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
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
                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteBeginTextUtf8(stream, textBytes.Length);

                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteTextUtf8Chunk
                (
                    stream,
                    textBytes,
                    0,
                    textBytes.Length
                );

                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteEndTextUtf8(stream);

                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteBeginBlob(stream, blobBytes.Length);

                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteBlobChunk
                (
                    stream,
                    blobBytes,
                    0,
                    blobBytes.Length
                );

                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteEndBlob(stream);

                stream.Position = 0;

                using (var beginText = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual((long)textBytes.Length, beginText.GetPayload<long>());
                }

                byte[] exposedTextBytes;

                using (var textChunkFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var chunk = textChunkFrame.GetPayload<JasonQueryDbStorageMigrationLogicalChunk>();

                    exposedTextBytes = chunk.Bytes;
                    CollectionAssert.AreEqual(textBytes, exposedTextBytes);
                }

                CollectionAssert.AreEqual
                (
                    new byte[exposedTextBytes.Length],
                    exposedTextBytes
                );

                using (var endText = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(JasonQueryDbStorageMigrationLogicalFrameKind.EndTextUtf8, endText.Kind);
                }

                using (var beginBlob = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual((long)blobBytes.Length, beginBlob.GetPayload<long>());
                }

                using (var blobChunkFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var chunk = blobChunkFrame.GetPayload<JasonQueryDbStorageMigrationLogicalChunk>();

                    CollectionAssert.AreEqual(blobBytes, chunk.Bytes);
                }

                using (var endBlob = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(JasonQueryDbStorageMigrationLogicalFrameKind.EndBlob, endBlob.Kind);
                }
            }
        }

        [TestMethod]
        public void SequenceState_RoundTripsWithoutReplayingSqliteSequenceTable()
        {
            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteBeginSequenceState(stream, 1);

                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteSequenceEntry
                (
                    stream,
                    new JasonQueryDbStorageMigrationSequenceEntry("SQLHistory", 49821)
                );

                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteEndSequenceState(stream);

                stream.Position = 0;

                using (var beginFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(1, beginFrame.GetPayload<int>());
                }

                using (var entryFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var entry = entryFrame.GetPayload<JasonQueryDbStorageMigrationSequenceEntry>();

                    Assert.AreEqual("SQLHistory", entry.TableName);
                    Assert.AreEqual(49821L, entry.SequenceValue);
                }

                using (var endFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    Assert.AreEqual(JasonQueryDbStorageMigrationLogicalFrameKind.EndSequenceState, endFrame.Kind);
                }
            }
        }

        [TestMethod]
        public void SecondarySchemaObject_RoundTrips()
        {
            using (var stream = new MemoryStream())
            {
                var schemaObject = new JasonQueryDbStorageMigrationSecondarySchemaObject
                (
                    JasonQueryDbStorageMigrationSchemaObjectKind.Index,
                    "IX_Test",
                    "Test",
                    "CREATE INDEX IX_Test ON Test(Name)"
                );

                JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteSchemaObject(stream, schemaObject);

                stream.Position = 0;

                using (var frame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream))
                {
                    var actual = frame.GetPayload<JasonQueryDbStorageMigrationSecondarySchemaObject>();

                    Assert.AreEqual(JasonQueryDbStorageMigrationSchemaObjectKind.Index, actual.Kind);
                    Assert.AreEqual("IX_Test", actual.Name);
                    Assert.AreEqual("Test", actual.TableName);
                    Assert.AreEqual("CREATE INDEX IX_Test ON Test(Name)", actual.OriginalSql);
                }
            }
        }

        [TestMethod]
        public void OversizedChunk_IsRejectedBeforeWrite()
        {
            var bytes = new byte[JasonQueryDbStorageMigrationLogicalStreamContract.MaxValueChunkBytes + 1];

            using (var stream = new MemoryStream())
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbStorageMigrationLogicalStreamProtocol.WriteBlobChunk
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
                writer.Write(JasonQueryDbStorageMigrationLogicalStreamContract.CurrentVersion);
                writer.Write((byte)255);
                writer.Flush();

                stream.Position = 0;

                Assert.ThrowsExactly<NotSupportedException>
                (
                    () => JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream)
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
                writer.Write(JasonQueryDbStorageMigrationLogicalStreamContract.CurrentVersion);
                writer.Write((byte)JasonQueryDbStorageMigrationLogicalFrameKind.SequenceEntry);

                writer.Write(2);
                writer.Write(new byte[] { 0xC3, 0x28 });
                writer.Write(1L);
                writer.Flush();

                stream.Position = 0;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(stream)
                );
            }
        }

        [TestMethod]
        public void LogicalStreamReadyResponse_RoundTrips()
        {
            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageMigrationWireProtocol.WriteStorageV1LogicalStreamReadyResponse(stream);

                stream.Position = 0;

                var response = JasonQueryDbStorageMigrationWireProtocol.ReadResponse(stream);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationWireResponseStatus.StorageV1LogicalStreamReady,
                    response.Status
                );
            }
        }
    }
}
