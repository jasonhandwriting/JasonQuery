using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public sealed class JasonQueryDbStorageV2CandidateWriterProtocolTests
    {
        [TestMethod]
        public void Request_RoundTripsCandidatePathAndPasswordWithoutStringConversion()
        {
            var password = Encoding.UTF8.GetBytes(Convert.ToBase64String(Enumerable.Range(0, 32).Select(value => (byte)value).ToArray()));

            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageV2CandidateWriterProtocol.WriteRequest(stream, @"D:\Candidate\JasonQuery.db.candidate", password);
                stream.Position = 0;

                using (var request = JasonQueryDbStorageV2CandidateWriterProtocol.ReadRequest(stream))
                {
                    Assert.AreEqual(@"D:\Candidate\JasonQuery.db.candidate", request.CandidateDatabasePath);
                    CollectionAssert.AreEqual(password, request.DatabasePasswordUtf8);
                }
            }
        }

        [TestMethod]
        public void Request_DisposeClearsOwnedPasswordBytes()
        {
            var password = CreateDatabasePassword();
            byte[] ownedBytes;

            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageV2CandidateWriterProtocol.WriteRequest(stream, @"D:\Candidate\JasonQuery.db.candidate", password);
                stream.Position = 0;

                var request = JasonQueryDbStorageV2CandidateWriterProtocol.ReadRequest(stream);

                ownedBytes = request.DatabasePasswordUtf8;
                Assert.IsTrue(ownedBytes.Any(value => value != 0));
                request.Dispose();
            }

            Assert.IsTrue(ownedBytes.All(value => value == 0));
        }

        [TestMethod]
        public void Request_RejectsUnexpectedTargetStorageFormat()
        {
            var password = CreateDatabasePassword();

            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageV2CandidateWriterProtocol.WriteRequest(stream, @"D:\Candidate\JasonQuery.db.candidate", password);

                var bytes = stream.ToArray();
                var targetOffset = Encoding.ASCII.GetByteCount(JasonQueryDbStorageV2CandidateWriterProtocol.Magic) + sizeof(int) + sizeof(byte);
                var unsupported = BitConverter.GetBytes(JasonQueryDbStorageV2CandidateWriterProtocol.TargetStorageFormatVersion + 1);

                Buffer.BlockCopy(unsupported, 0, bytes, targetOffset, unsupported.Length);

                using (var invalidStream = new MemoryStream(bytes, false))
                {
                    Assert.ThrowsExactly<NotSupportedException>
                    (
                        () => JasonQueryDbStorageV2CandidateWriterProtocol.ReadRequest(invalidStream)
                    );
                }
            }
        }

        [TestMethod]
        public void Request_RejectsTruncatedPasswordPayload()
        {
            var password = CreateDatabasePassword();

            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageV2CandidateWriterProtocol.WriteRequest(stream, @"D:\Candidate\JasonQuery.db.candidate", password);

                var bytes = stream.ToArray();

                using (var truncated = new MemoryStream(bytes, 0, bytes.Length - 1, false, true))
                {
                    Assert.ThrowsExactly<EndOfStreamException>
                    (
                        () => JasonQueryDbStorageV2CandidateWriterProtocol.ReadRequest(truncated)
                    );
                }
            }
        }

        [TestMethod]
        public void Response_RoundTripsCandidateValidatedStatus()
        {
            using (var stream = new MemoryStream())
            {
                JasonQueryDbStorageV2CandidateWriterProtocol.WriteCandidateValidatedResponse(stream);
                stream.Position = 0;

                var response = JasonQueryDbStorageV2CandidateWriterProtocol.ReadResponse(stream);

                Assert.AreEqual(JasonQueryDbStorageV2CandidateWriterResponseStatus.CandidateValidated, response.Status);
                Assert.AreEqual(string.Empty, response.Message);
            }
        }

        private static byte[] CreateDatabasePassword()
        {
            var key = Enumerable.Range(1, 32).Select(value => (byte)value).ToArray();

            return Encoding.UTF8.GetBytes(Convert.ToBase64String(key));
        }
    }
}
