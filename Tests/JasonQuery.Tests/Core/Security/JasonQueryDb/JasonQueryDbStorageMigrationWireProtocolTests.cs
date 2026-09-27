using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationWireProtocolTests
    {
        [TestMethod]
        public void WireConstants_MatchFrozenMigrationProtocolContract()
        {
            Assert.AreEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationProtocolContract), nameof(JasonQueryDbStorageMigrationProtocolContract.Magic)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationWireProtocol), nameof(JasonQueryDbStorageMigrationWireProtocol.Magic)));
            Assert.AreEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationProtocolContract), nameof(JasonQueryDbStorageMigrationProtocolContract.CurrentVersion)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationWireProtocol), nameof(JasonQueryDbStorageMigrationWireProtocol.CurrentVersion)));
            Assert.AreEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationProtocolContract), nameof(JasonQueryDbStorageMigrationProtocolContract.SourceStorageFormatVersion)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationWireProtocol), nameof(JasonQueryDbStorageMigrationWireProtocol.SourceStorageFormatVersion)));
            Assert.AreEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationProtocolContract), nameof(JasonQueryDbStorageMigrationProtocolContract.TargetStorageFormatVersion)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationWireProtocol), nameof(JasonQueryDbStorageMigrationWireProtocol.TargetStorageFormatVersion)));
        }

        [TestMethod]
        public void Request_RoundTripsPathAndSecretBytes()
        {
            var passwordBytes = Encoding.UTF8.GetBytes("Step389C1-Secret-密碼");

            try
            {
                using (var stream = new MemoryStream())
                {
                    JasonQueryDbStorageMigrationWireProtocol.WriteRequest
                    (
                        stream,
                        @"D:\Lab\Historical-JasonQuery.db",
                        passwordBytes
                    );

                    stream.Position = 0;

                    using (var request = JasonQueryDbStorageMigrationWireProtocol.ReadRequest(stream))
                    {
                        Assert.AreEqual
                        (
                            @"D:\Lab\Historical-JasonQuery.db",
                            request.DatabaseFilePath
                        );

                        CollectionAssert.AreEqual
                        (
                            passwordBytes,
                            request.DatabasePasswordUtf8
                        );
                    }
                }
            }
            finally
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
            }
        }

        [TestMethod]
        public void Request_DisposeClearsOwnedSecretBytes()
        {
            var passwordBytes = Encoding.UTF8.GetBytes("Dispose-Me-389C1");

            try
            {
                using (var stream = new MemoryStream())
                {
                    JasonQueryDbStorageMigrationWireProtocol.WriteRequest
                    (
                        stream,
                        @"D:\Lab\JasonQuery.db",
                        passwordBytes
                    );

                    stream.Position = 0;

                    var request = JasonQueryDbStorageMigrationWireProtocol.ReadRequest(stream);
                    var ownedSecret = request.DatabasePasswordUtf8;

                    Assert.IsTrue
                    (
                        ownedSecret.Any(value => value != 0)
                    );

                    request.Dispose();

                    Assert.IsTrue
                    (
                        ownedSecret.All(value => value == 0)
                    );
                }
            }
            finally
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
            }
        }

        [TestMethod]
        public void Request_InvalidMagic_FailsClosed()
        {
            using (var stream = CreateMinimalRequestFrame())
            {
                stream.GetBuffer()[0] = (byte)'X';
                stream.Position = 0;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbStorageMigrationWireProtocol.ReadRequest(stream)
                );
            }
        }

        [TestMethod]
        public void Request_UnsupportedProtocolVersion_FailsClosed()
        {
            using (var stream = CreateMinimalRequestFrame(protocolVersion: 99))
            {
                Assert.ThrowsExactly<NotSupportedException>
                (
                    () => JasonQueryDbStorageMigrationWireProtocol.ReadRequest(stream)
                );
            }
        }

        [TestMethod]
        public void Request_UnsupportedStorageRoute_FailsClosed()
        {
            using (var stream = CreateMinimalRequestFrame(sourceStorageFormatVersion: 2, targetStorageFormatVersion: 1))
            {
                Assert.ThrowsExactly<NotSupportedException>
                (
                    () => JasonQueryDbStorageMigrationWireProtocol.ReadRequest(stream)
                );
            }
        }

        [TestMethod]
        public void Request_OversizedPathLength_FailsClosedBeforeAllocation()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                WriteRequestHeader(writer);

                writer.Write
                (
                    JasonQueryDbStorageMigrationWireProtocol.SourceStorageFormatVersion
                );

                writer.Write
                (
                    JasonQueryDbStorageMigrationWireProtocol.TargetStorageFormatVersion
                );

                writer.Write
                (
                    JasonQueryDbStorageMigrationWireProtocol.MaxDatabasePathUtf8Bytes + 1
                );

                writer.Flush();
                stream.Position = 0;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbStorageMigrationWireProtocol.ReadRequest(stream)
                );
            }
        }

        [TestMethod]
        public void ControlReadyResponse_RoundTripsAsBinaryFrame()
        {
            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageMigrationWireProtocol.WriteControlReadyResponse
                (
                    stream
                );

                stream.Position = 0;

                var response = JasonQueryDbStorageMigrationWireProtocol.ReadResponse(stream);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationWireResponseStatus.ControlChannelReady,
                    response.Status
                );

                Assert.AreEqual
                (
                    "Legacy Storage V1 control channel ready.",
                    response.Message
                );
            }
        }

        [TestMethod]
        public void StorageV1ValidatedResponse_RoundTripsAsBinaryFrame()
        {
            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageMigrationWireProtocol.WriteStorageV1ValidatedResponse
                (
                    stream
                );

                stream.Position = 0;

                var response = JasonQueryDbStorageMigrationWireProtocol.ReadResponse(stream);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationWireResponseStatus.StorageV1Validated,
                    response.Status
                );

                Assert.AreEqual
                (
                    "Legacy Storage V1 source validated read-only.",
                    response.Message
                );
            }
        }

        [TestMethod]
        public void Response_RequestFrameKind_FailsClosed()
        {
            var passwordBytes = Encoding.UTF8.GetBytes("Not-A-Response");

            try
            {
                using (var stream = new MemoryStream())
                {
                    JasonQueryDbStorageMigrationWireProtocol.WriteRequest
                    (
                        stream,
                        @"D:\Lab\JasonQuery.db",
                        passwordBytes
                    );

                    stream.Position = 0;

                    Assert.ThrowsExactly<InvalidDataException>
                    (
                        () => JasonQueryDbStorageMigrationWireProtocol.ReadResponse(stream)
                    );
                }
            }
            finally
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
            }
        }

        private static MemoryStream CreateMinimalRequestFrame(int protocolVersion = JasonQueryDbStorageMigrationWireProtocol.CurrentVersion,
                                                              int sourceStorageFormatVersion = JasonQueryDbStorageMigrationWireProtocol.SourceStorageFormatVersion,
                                                              int targetStorageFormatVersion = JasonQueryDbStorageMigrationWireProtocol.TargetStorageFormatVersion)
        {
            var stream = new MemoryStream();

            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("JQSM"));
                writer.Write(protocolVersion);
                writer.Write((byte)1);
                writer.Write(sourceStorageFormatVersion);
                writer.Write(targetStorageFormatVersion);

                var pathBytes = Encoding.UTF8.GetBytes(@"D:\Lab\JasonQuery.db");
                var passwordBytes = Encoding.UTF8.GetBytes("Step389C1");

                try
                {
                    writer.Write(pathBytes.Length);
                    writer.Write(pathBytes);
                    writer.Write(passwordBytes.Length);
                    writer.Write(passwordBytes);
                    writer.Flush();
                }
                finally
                {
                    Array.Clear(pathBytes, 0, pathBytes.Length);
                    Array.Clear(passwordBytes, 0, passwordBytes.Length);
                }
            }

            stream.Position = 0;
            return stream;
        }

        private static void WriteRequestHeader(BinaryWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes("JQSM"));
            writer.Write(JasonQueryDbStorageMigrationWireProtocol.CurrentVersion);
            writer.Write((byte)1);
        }
    }
}
