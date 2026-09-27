using JasonQuery.Database.Internal.Runtime.Modern;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace JasonQuery.Tests.Database.Internal.Runtime.Modern
{
    [TestClass]
    public class ModernDatabaseRuntimeProtocolTests
    {
        [TestMethod]
        public void RequestFrame_RoundTrip_PreservesIdentity()
        {
            using (var stream = new MemoryStream())
            {
                ModernDatabaseRuntimeProtocol.WriteRequest(stream, 42, ModernDatabaseRuntimeOperation.Ping, null);
                stream.Position = 0;

                Assert.IsTrue(ModernDatabaseRuntimeProtocol.TryReadFrame(stream, out var frame));
                Assert.AreEqual(42L, frame.RequestId);
                Assert.AreEqual(ModernDatabaseRuntimeOperation.Ping, frame.Operation);
                Assert.AreEqual(ModernDatabaseRuntimeFrameKind.Request, frame.Kind);
                Assert.IsEmpty(frame.Payload);
            }
        }

        [TestMethod]
        public void OpenPayload_RoundTrip_PreservesCredentialBytesExactly()
        {
            var credential = new byte[] { 0, 1, 2, 127, 128, 254, 255 };

            using (var stream = new MemoryStream())
            {
                ModernDatabaseRuntimeProtocol.WriteRequest
                (
                    stream,
                    7,
                    ModernDatabaseRuntimeOperation.Open,
                    writer =>
                    {
                        ModernDatabaseRuntimeProtocol.WriteString(writer, @"D:\Temp\JasonQuery.db");
                        ModernDatabaseRuntimeProtocol.WriteByteArray(writer, credential);
                    }
                );

                stream.Position = 0;
                Assert.IsTrue(ModernDatabaseRuntimeProtocol.TryReadFrame(stream, out var frame));

                using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                {
                    Assert.AreEqual(@"D:\Temp\JasonQuery.db", ModernDatabaseRuntimeProtocol.ReadString(reader));
                    CollectionAssert.AreEqual(credential, ModernDatabaseRuntimeProtocol.ReadByteArray(reader));
                    ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                }
            }
        }

        [TestMethod]
        public void RekeyPayload_RoundTrip_PreservesCredentialBytesExactly()
        {
            var credential = new byte[] { 255, 128, 64, 32, 16, 8, 4, 2, 1, 0 };

            using (var stream = new MemoryStream())
            {
                ModernDatabaseRuntimeProtocol.WriteRequest
                (
                    stream,
                    8,
                    ModernDatabaseRuntimeOperation.Rekey,
                    writer => ModernDatabaseRuntimeProtocol.WriteByteArray(writer, credential)
                );

                stream.Position = 0;
                Assert.IsTrue(ModernDatabaseRuntimeProtocol.TryReadFrame(stream, out var frame));
                Assert.AreEqual(ModernDatabaseRuntimeOperation.Rekey, frame.Operation);

                using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                {
                    CollectionAssert.AreEqual(credential, ModernDatabaseRuntimeProtocol.ReadByteArray(reader));
                    ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                }
            }
        }

        [TestMethod]
        public void Parameters_RoundTrip_PreservesSensitiveStringWithoutSqlLiteralConcatenation()
        {
            var parameters = new List<ModernDatabaseRuntimeParameter>
            {
                new ModernDatabaseRuntimeParameter("@Password", "  O'Brien-資料庫\t2026!  ")
            };

            using (var payload = new MemoryStream())
            using (var writer = new BinaryWriter(payload))
            {
                ModernDatabaseRuntimeProtocol.WriteParameters(writer, parameters);
                writer.Flush();
                payload.Position = 0;

                using (var reader = new BinaryReader(payload))
                {
                    var result = ModernDatabaseRuntimeProtocol.ReadParameters(reader);

                    Assert.HasCount(1, result);
                    Assert.AreEqual("@Password", result[0].Name);
                    Assert.AreEqual("  O'Brien-資料庫\t2026!  ", result[0].Value);
                }
            }
        }

        [TestMethod]
        public void ValueRoundTrip_PreservesNativeRuntimeValueKinds()
        {
            var values = new object[]
            {
                DBNull.Value,
                123L,
                12.5d,
                "資料庫",
                new byte[] { 1, 2, 3 },
                123.45m,
                true,
                new DateTime(2026, 9, 12, 20, 47, 0, DateTimeKind.Local),
                new Guid("11111111-2222-3333-4444-555555555555")
            };

            foreach (var value in values)
            {
                using (var payload = new MemoryStream())
                using (var writer = new BinaryWriter(payload))
                {
                    ModernDatabaseRuntimeProtocol.WriteValue(writer, value);
                    writer.Flush();
                    payload.Position = 0;

                    using (var reader = new BinaryReader(payload))
                    {
                        var result = ModernDatabaseRuntimeProtocol.ReadValue(reader);

                        if (value is byte[] expectedBytes)
                        {
                            CollectionAssert.AreEqual(expectedBytes, (byte[])result);
                        }
                        else
                        {
                            Assert.AreEqual(value, result);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void QueryResult_RoundTrip_PreservesRowsAndBlob()
        {
            var expected = new ModernDatabaseRuntimeQueryResult
            (
                new[] { "Id", "Name", "Payload" },
                new List<object[]>
                {
                    new object[] { 1L, "JasonQuery", new byte[] { 10, 20, 30 } },
                    new object[] { 2L, DBNull.Value, Array.Empty<byte>() }
                }
            );

            using (var payload = new MemoryStream())
            using (var writer = new BinaryWriter(payload))
            {
                ModernDatabaseRuntimeProtocol.WriteQueryResult(writer, expected);
                writer.Flush();
                payload.Position = 0;

                using (var reader = new BinaryReader(payload))
                {
                    var actual = ModernDatabaseRuntimeProtocol.ReadQueryResult(reader);

                    CollectionAssert.AreEqual(expected.ColumnNames, actual.ColumnNames);

                    Assert.HasCount(2, actual.Rows);
                    Assert.AreEqual(1L, actual.Rows[0][0]);
                    Assert.AreEqual("JasonQuery", actual.Rows[0][1]);

                    CollectionAssert.AreEqual(new byte[] { 10, 20, 30 }, (byte[])actual.Rows[0][2]);

                    Assert.AreEqual(DBNull.Value, actual.Rows[1][1]);
                }
            }
        }

        [TestMethod]
        public void FailureResponse_RoundTrip_PreservesSanitizedErrorContract()
        {
            using (var stream = new MemoryStream())
            {
                ModernDatabaseRuntimeProtocol.WriteFailureResponse
                (
                    stream,
                    9,
                    ModernDatabaseRuntimeOperation.ExecuteQuery,
                    "InvalidDataException",
                    "query failed"
                );

                stream.Position = 0;
                Assert.IsTrue(ModernDatabaseRuntimeProtocol.TryReadFrame(stream, out var frame));
                Assert.AreEqual(ModernDatabaseRuntimeResponseStatus.Failure, frame.ResponseStatus);

                using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                {
                    Assert.AreEqual("InvalidDataException", ModernDatabaseRuntimeProtocol.ReadString(reader));
                    Assert.AreEqual("query failed", ModernDatabaseRuntimeProtocol.ReadString(reader));
                }
            }
        }

        [TestMethod]
        public void ProtocolVersion_RemainsExplicitlyVersioned()
        {
            Assert.AreEqual(3, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(ModernDatabaseRuntimeProtocol), nameof(ModernDatabaseRuntimeProtocol.ProtocolVersion)));
        }

        [TestMethod]
        public void FreshOperations_RemainExplicitlyNumbered()
        {
            Assert.AreEqual(9, (byte)JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(ModernDatabaseRuntimeOperation), nameof(ModernDatabaseRuntimeOperation.CreateFreshDatabase)));
            Assert.AreEqual(10, (byte)JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(ModernDatabaseRuntimeOperation), nameof(ModernDatabaseRuntimeOperation.CanOpenWithoutKey)));
        }

        [TestMethod]
        public void CreateFreshDatabasePayload_RoundTrip_PreservesPathCredentialAndTemplateBytes()
        {
            var credential = new byte[] { 0, 1, 2, 128, 254, 255 };
            var templateBytes = new byte[] { 10, 20, 30, 40, 50 };

            using (var stream = new MemoryStream())
            {
                ModernDatabaseRuntimeProtocol.WriteRequest
                (
                    stream,
                    10,
                    ModernDatabaseRuntimeOperation.CreateFreshDatabase,
                    writer =>
                    {
                        ModernDatabaseRuntimeProtocol.WriteString(writer, @"D:\Temp\FreshJasonQuery.db");
                        ModernDatabaseRuntimeProtocol.WriteByteArray(writer, credential);
                        ModernDatabaseRuntimeProtocol.WriteByteArray(writer, templateBytes);
                    }
                );

                stream.Position = 0;
                Assert.IsTrue(ModernDatabaseRuntimeProtocol.TryReadFrame(stream, out var frame));
                Assert.AreEqual(ModernDatabaseRuntimeOperation.CreateFreshDatabase, frame.Operation);

                using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                {
                    Assert.AreEqual(@"D:\Temp\FreshJasonQuery.db", ModernDatabaseRuntimeProtocol.ReadString(reader));
                    CollectionAssert.AreEqual(credential, ModernDatabaseRuntimeProtocol.ReadByteArray(reader));
                    CollectionAssert.AreEqual(templateBytes, ModernDatabaseRuntimeProtocol.ReadByteArray(reader));
                    ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                }
            }
        }

        [TestMethod]
        public void FreshTemplateLimit_IsBoundedToSixteenMiB()
        {
            Assert.AreEqual(16 * 1024 * 1024, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(ModernDatabaseRuntimeProtocol), nameof(ModernDatabaseRuntimeProtocol.MaxFreshTemplateBytes)));
            Assert.IsLessThan(ModernDatabaseRuntimeProtocol.MaxFrameBytes, ModernDatabaseRuntimeProtocol.MaxFreshTemplateBytes);
        }

        [TestMethod]
        public void FrameLimit_IsBounded()
        {
            Assert.IsGreaterThan(0, ModernDatabaseRuntimeProtocol.MaxFrameBytes);
            Assert.IsLessThanOrEqualTo(128 * 1024 * 1024, ModernDatabaseRuntimeProtocol.MaxFrameBytes);
        }
    }
}
