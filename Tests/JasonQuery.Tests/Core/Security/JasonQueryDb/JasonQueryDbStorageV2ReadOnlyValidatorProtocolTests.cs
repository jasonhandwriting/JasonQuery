using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageV2ReadOnlyValidatorProtocolTests
    {
        [TestMethod]
        public void Contract_UsesDedicatedMagicAndV2Target()
        {
            Assert.AreEqual("JQMV", RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol), nameof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol.Magic)));
            Assert.AreEqual(1, RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol), nameof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol.CurrentVersion)));
            Assert.AreEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.ModernVersion)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol), nameof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol.TargetStorageFormatVersion)));
        }

        [TestMethod]
        public void Request_RoundTripsPathAndCanonicalPassword()
        {
            var password = CreateCanonicalPassword();

            try
            {
                using (var stream = new MemoryStream())
                {
                    JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteRequest
                    (
                        stream,
                        @"C:\Temp\Candidate.db",
                        password
                    );

                    stream.Position = 0;

                    using (var request = JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadRequest(stream))
                    {
                        Assert.AreEqual(@"C:\Temp\Candidate.db", request.DatabaseFilePath);
                        CollectionAssert.AreEqual(password, request.DatabasePasswordUtf8);
                    }
                }
            }
            finally
            {
                Array.Clear(password, 0, password.Length);
            }
        }

        [TestMethod]
        public void Request_Dispose_ClearsPasswordBuffer()
        {
            var password = CreateCanonicalPassword();

            try
            {
                using (var stream = new MemoryStream())
                {
                    JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteRequest(stream, "Candidate.db", password);
                    stream.Position = 0;

                    var request = JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadRequest(stream);
                    var requestPassword = request.DatabasePasswordUtf8;

                    request.Dispose();

                    CollectionAssert.AreEqual(new byte[requestPassword.Length], requestPassword);

                    Assert.ThrowsExactly<ObjectDisposedException>
                    (
                        () => { var unused = request.DatabasePasswordUtf8; }
                    );
                }
            }
            finally
            {
                Array.Clear(password, 0, password.Length);
            }
        }

        [TestMethod]
        public void Request_WriteNullStream_Throws()
        {
            var password = CreateCanonicalPassword();

            try
            {
                Assert.ThrowsExactly<ArgumentNullException>
                (
                    () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteRequest
                    (
                        null,
                        "Candidate.db",
                        password
                    )
                );
            }
            finally
            {
                Array.Clear(password, 0, password.Length);
            }
        }

        [TestMethod]
        public void Request_WriteEmptyPath_Throws()
        {
            var password = CreateCanonicalPassword();

            try
            {
                using (var stream = new MemoryStream())
                {
                    Assert.ThrowsExactly<ArgumentException>
                    (
                        () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteRequest
                        (
                            stream,
                            " ",
                            password
                        )
                    );
                }
            }
            finally
            {
                Array.Clear(password, 0, password.Length);
            }
        }

        [TestMethod]
        public void Request_NullPassword_Throws()
        {
            using (var stream = new MemoryStream())
            {
                Assert.ThrowsExactly<ArgumentException>
                (
                    () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteRequest
                    (
                        stream,
                        "Candidate.db",
                        null
                    )
                );
            }
        }

        [TestMethod]
        public void Request_NonCanonicalPassword_Throws()
        {
            using (var stream = new MemoryStream())
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteRequest
                    (
                        stream,
                        "Candidate.db",
                        Encoding.UTF8.GetBytes("not-a-canonical-database-password")
                    )
                );
            }
        }

        [TestMethod]
        public void Request_InvalidMagic_Throws()
        {
            var bytes = CreateValidRequestBytes();

            bytes[0] ^= 0x7F;

            using (var stream = new MemoryStream(bytes))
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadRequest(stream)
                );
            }
        }

        [TestMethod]
        public void Request_TruncatedMagic_Throws()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("JQM")))
            {
                Assert.ThrowsExactly<EndOfStreamException>
                (
                    () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadRequest(stream)
                );
            }
        }

        [TestMethod]
        public void Request_UnsupportedVersion_Throws()
        {
            var bytes = CreateValidRequestBytes();

            WriteInt32(bytes, 4, JasonQueryDbStorageV2ReadOnlyValidatorProtocol.CurrentVersion + 1);

            using (var stream = new MemoryStream(bytes))
            {
                Assert.ThrowsExactly<NotSupportedException>
                (
                    () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadRequest(stream)
                );
            }
        }

        [TestMethod]
        public void Request_InvalidFrameKind_Throws()
        {
            var bytes = CreateValidRequestBytes();

            bytes[8] = 99;

            using (var stream = new MemoryStream(bytes))
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadRequest(stream)
                );
            }
        }

        [TestMethod]
        public void Request_UnsupportedTargetStorageVersion_Throws()
        {
            var bytes = CreateValidRequestBytes();

            WriteInt32
            (
                bytes,
                9,
                JasonQueryDbStorageV2ReadOnlyValidatorProtocol.TargetStorageFormatVersion + 1
            );

            using (var stream = new MemoryStream(bytes))
            {
                Assert.ThrowsExactly<NotSupportedException>
                (
                    () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadRequest(stream)
                );
            }
        }

        [TestMethod]
        public void Response_RoundTripsValidatedStatus()
        {
            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteStorageV2ValidatedResponse(stream);
                stream.Position = 0;

                var response = JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadResponse(stream);

                Assert.AreEqual
                (
                    JasonQueryDbStorageV2ReadOnlyValidatorResponseStatus.StorageV2Validated,
                    response.Status
                );

                Assert.AreEqual(string.Empty, response.Message);
            }
        }

        [TestMethod]
        public void Response_InvalidMagic_Throws()
        {
            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteStorageV2ValidatedResponse(stream);

                var bytes = stream.ToArray();

                bytes[0] ^= 0x7F;

                using (var invalid = new MemoryStream(bytes))
                {
                    Assert.ThrowsExactly<InvalidDataException>
                    (
                        () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadResponse(invalid)
                    );
                }
            }
        }

        [TestMethod]
        public void Response_UnknownStatus_Throws()
        {
            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteStorageV2ValidatedResponse(stream);

                var bytes = stream.ToArray();

                bytes[9] = 99;

                using (var invalid = new MemoryStream(bytes))
                {
                    Assert.ThrowsExactly<NotSupportedException>
                    (
                        () => JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadResponse(invalid)
                    );
                }
            }
        }

        [TestMethod]
        public void MagicMatches_RecognizesOnlyValidatorMagic()
        {
            Assert.IsTrue
            (
                JasonQueryDbStorageV2ReadOnlyValidatorProtocol.MagicMatches
                (
                    Encoding.ASCII.GetBytes("JQMV")
                )
            );

            Assert.IsFalse
            (
                JasonQueryDbStorageV2ReadOnlyValidatorProtocol.MagicMatches
                (
                    Encoding.ASCII.GetBytes("JQMW")
                )
            );
        }

        [TestMethod]
        public void Magic_IsDistinctFromCandidateWriterMagic()
        {
            Assert.AreNotEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageV2CandidateWriterProtocol), nameof(JasonQueryDbStorageV2CandidateWriterProtocol.Magic)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol), nameof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol.Magic)));
        }

        [TestMethod]
        public void Limits_MatchExistingBoundedMigrationContracts()
        {
            Assert.AreEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationWireProtocol), nameof(JasonQueryDbStorageMigrationWireProtocol.MaxDatabasePathUtf8Bytes)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol), nameof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol.MaxDatabasePathUtf8Bytes)));
            Assert.AreEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationWireProtocol), nameof(JasonQueryDbStorageMigrationWireProtocol.MaxDatabasePasswordUtf8Bytes)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol), nameof(JasonQueryDbStorageV2ReadOnlyValidatorProtocol.MaxDatabasePasswordUtf8Bytes)));
        }

        private static byte[] CreateValidRequestBytes()
        {
            var password = CreateCanonicalPassword();

            try
            {
                using (var stream = new MemoryStream())
                {
                    JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteRequest
                    (
                        stream,
                        "Candidate.db",
                        password
                    );

                    return stream.ToArray();
                }
            }
            finally
            {
                Array.Clear(password, 0, password.Length);
            }
        }

        private static byte[] CreateCanonicalPassword()
        {
            var key = new byte[32];

            for (var index = 0; index < key.Length; index++)
            {
                key[index] = (byte)(index + 1);
            }

            try
            {
                return Encoding.UTF8.GetBytes(Convert.ToBase64String(key));
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
            }
        }

        private static void WriteInt32(byte[] bytes, int offset, int value)
        {
            var raw = BitConverter.GetBytes(value);

            try
            {
                Buffer.BlockCopy(raw, 0, bytes, offset, raw.Length);
            }
            finally
            {
                Array.Clear(raw, 0, raw.Length);
            }
        }
    }
}
