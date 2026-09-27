using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationProcessRunnerTests
    {
        [TestMethod]
        public void StreamBridge_BufferSize_Is64KiB()
        {
            Assert.AreEqual(64 * 1024, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationStreamBridge), nameof(JasonQueryDbStorageMigrationStreamBridge.BufferSize)));
        }

        [TestMethod]
        public void StreamBridge_EmptyStream_ReturnsZero()
        {
            using (var source = new MemoryStream())
            using (var destination = new MemoryStream())
            {
                var copied = JasonQueryDbStorageMigrationStreamBridge.Forward(source, destination);

                Assert.AreEqual(0L, copied);
                Assert.AreEqual(0L, destination.Length);
            }
        }

        [TestMethod]
        public void StreamBridge_LargePayload_CopiesExactly()
        {
            var payload = CreatePayload(JasonQueryDbStorageMigrationStreamBridge.BufferSize * 3 + 173);

            using (var source = new MemoryStream(payload, false))
            using (var destination = new MemoryStream())
            {
                var copied = JasonQueryDbStorageMigrationStreamBridge.Forward(source, destination);

                CollectionAssert.AreEqual(payload, destination.ToArray());
                Assert.AreEqual(payload.LongLength, copied);
            }
        }

        [TestMethod]
        public void StreamBridge_ReadRequestsNeverExceed64KiB()
        {
            var payload = CreatePayload(JasonQueryDbStorageMigrationStreamBridge.BufferSize * 2 + 31);

            using (var source = new TrackingReadStream(payload))
            using (var destination = new TrackingWriteStream())
            {
                JasonQueryDbStorageMigrationStreamBridge.Forward(source, destination);

                Assert.IsLessThanOrEqualTo(JasonQueryDbStorageMigrationStreamBridge.BufferSize, source.MaximumRequestedReadCount);
                Assert.IsLessThanOrEqualTo(JasonQueryDbStorageMigrationStreamBridge.BufferSize, destination.MaximumWriteCount);
                Assert.AreEqual(payload.LongLength, destination.Length);
            }
        }

        [TestMethod]
        public void StreamBridge_LeavesCallerStreamsOpen()
        {
            using (var source = new MemoryStream(new byte[] { 1, 2, 3 }, false))
            using (var destination = new MemoryStream())
            {
                JasonQueryDbStorageMigrationStreamBridge.Forward(source, destination);

                Assert.IsTrue(source.CanRead);
                Assert.IsTrue(destination.CanWrite);
            }
        }

        [TestMethod]
        public void StreamBridge_NullSource_Throws()
        {
            using (var destination = new MemoryStream())
            {
                Assert.ThrowsExactly<ArgumentNullException>
                (
                    () => JasonQueryDbStorageMigrationStreamBridge.Forward(null, destination)
                );
            }
        }

        [TestMethod]
        public void StreamBridge_NullDestination_Throws()
        {
            using (var source = new MemoryStream())
            {
                Assert.ThrowsExactly<ArgumentNullException>
                (
                    () => JasonQueryDbStorageMigrationStreamBridge.Forward(source, null)
                );
            }
        }

        [TestMethod]
        public void StreamBridge_NonReadableSource_Throws()
        {
            using (var source = new NonReadableStream())
            using (var destination = new MemoryStream())
            {
                Assert.ThrowsExactly<ArgumentException>
                (
                    () => JasonQueryDbStorageMigrationStreamBridge.Forward(source, destination)
                );
            }
        }

        [TestMethod]
        public void StreamBridge_NonWritableDestination_Throws()
        {
            using (var source = new MemoryStream())
            using (var destination = new NonWritableStream())
            {
                Assert.ThrowsExactly<ArgumentException>
                (
                    () => JasonQueryDbStorageMigrationStreamBridge.Forward(source, destination)
                );
            }
        }

        [TestMethod]
        public void ProcessRunner_MissingLegacyHelper_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                Assert.ThrowsExactly<FileNotFoundException>
                (
                    () => scope.Run
                    (
                        Path.Combine(scope.DirectoryPath, "MissingLegacy.exe"),
                        scope.ModernHelperPath,
                        scope.SourceDatabasePath,
                        scope.CandidateDatabasePath,
                        new byte[] { 1 },
                        new byte[] { 2 }
                    )
                );

                Assert.IsFalse(File.Exists(scope.CandidateDatabasePath));
            }
        }

        [TestMethod]
        public void ProcessRunner_SameHelperPath_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                Assert.ThrowsExactly<ArgumentException>
                (
                    () => scope.Run
                    (
                        scope.LegacyHelperPath,
                        scope.LegacyHelperPath,
                        scope.SourceDatabasePath,
                        scope.CandidateDatabasePath,
                        new byte[] { 1 },
                        new byte[] { 2 }
                    )
                );

                Assert.IsFalse(File.Exists(scope.CandidateDatabasePath));
            }
        }

        [TestMethod]
        public void ProcessRunner_MissingSourceDatabase_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                Assert.ThrowsExactly<FileNotFoundException>
                (
                    () => scope.Run
                    (
                        scope.LegacyHelperPath,
                        scope.ModernHelperPath,
                        Path.Combine(scope.DirectoryPath, "Missing.db"),
                        scope.CandidateDatabasePath,
                        new byte[] { 1 },
                        new byte[] { 2 }
                    )
                );

                Assert.IsFalse(File.Exists(scope.CandidateDatabasePath));
            }
        }

        [TestMethod]
        public void ProcessRunner_ExistingCandidate_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                File.WriteAllBytes(scope.CandidateDatabasePath, new byte[] { 9, 8, 7 });

                Assert.ThrowsExactly<IOException>
                (
                    () => scope.Run
                    (
                        scope.LegacyHelperPath,
                        scope.ModernHelperPath,
                        scope.SourceDatabasePath,
                        scope.CandidateDatabasePath,
                        new byte[] { 1 },
                        new byte[] { 2 }
                    )
                );

                CollectionAssert.AreEqual
                (
                    new byte[] { 9, 8, 7 },
                    File.ReadAllBytes(scope.CandidateDatabasePath)
                );
            }
        }

        [TestMethod]
        public void ProcessRunner_CandidateMustShareSourceDirectory()
        {
            using (var scope = new TestScope())
            {
                var otherDirectory = Path.Combine(scope.DirectoryPath, "other");

                Directory.CreateDirectory(otherDirectory);

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Run
                    (
                        scope.LegacyHelperPath,
                        scope.ModernHelperPath,
                        scope.SourceDatabasePath,
                        Path.Combine(otherDirectory, "Candidate.db"),
                        new byte[] { 1 },
                        new byte[] { 2 }
                    )
                );
            }
        }

        [TestMethod]
        public void ProcessRunner_NullSourcePassword_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                Assert.ThrowsExactly<ArgumentException>
                (
                    () => scope.Run
                    (
                        scope.LegacyHelperPath,
                        scope.ModernHelperPath,
                        scope.SourceDatabasePath,
                        scope.CandidateDatabasePath,
                        null,
                        new byte[] { 2 }
                    )
                );

                Assert.IsFalse(File.Exists(scope.CandidateDatabasePath));
            }
        }

        [TestMethod]
        public void ProcessFailure_EndOfStreamDuringLegacyValidation_IsSafeAndSpecific()
        {
            const string secretText = "C:\\Users\\Secret\\JasonQuery.db|password=NeverExpose";

            var failure = JasonQueryDbStorageMigrationProcessFailureFactory.Create
            (
                new EndOfStreamException(secretText),
                JasonQueryDbStorageMigrationProcessPhase.AwaitingStorageV1Validated,
                new JasonQueryDbStorageMigrationHelperObservation(true, true, 17, true),
                new JasonQueryDbStorageMigrationHelperObservation(true, false, null, false)
            );

            Assert.AreEqual(JasonQueryDbStorageMigrationHelperRole.LegacyStorageV1, failure.HelperRole);
            Assert.AreEqual(JasonQueryDbStorageMigrationProcessPhase.AwaitingStorageV1Validated, failure.Phase);
            Assert.AreEqual(JasonQueryDbStorageMigrationProcessFailureKind.ProtocolEndOfStream, failure.FailureKind);
            Assert.IsTrue(failure.ExitCode.HasValue);
            Assert.AreEqual(17, failure.ExitCode.Value);
            Assert.IsTrue(failure.StandardErrorObserved);
            Assert.IsFalse(failure.CandidateCleanupFailed);
            Assert.DoesNotContain(secretText, failure.Message);
        }

        [TestMethod]
        public void ProcessFailure_EndOfStreamDuringCandidateValidation_IsModern()
        {
            var failure = JasonQueryDbStorageMigrationProcessFailureFactory.Create
            (
                new EndOfStreamException(),
                JasonQueryDbStorageMigrationProcessPhase.AwaitingCandidateValidated,
                new JasonQueryDbStorageMigrationHelperObservation(true, true, 0, false),
                new JasonQueryDbStorageMigrationHelperObservation(true, true, 23, true)
            );

            Assert.AreEqual(JasonQueryDbStorageMigrationHelperRole.ModernStorageV2CandidateWriter, failure.HelperRole);
            Assert.AreEqual(JasonQueryDbStorageMigrationProcessFailureKind.ProtocolEndOfStream, failure.FailureKind);
            Assert.IsTrue(failure.ExitCode.HasValue);
            Assert.AreEqual(23, failure.ExitCode.Value);
            Assert.IsTrue(failure.StandardErrorObserved);
        }

        [TestMethod]
        public void ProcessFailure_BridgePrefersLegacyNonZeroExit()
        {
            var role = JasonQueryDbStorageMigrationProcessFailureFactory.ResolveFailureRole
            (
                JasonQueryDbStorageMigrationProcessPhase.BridgingLogicalStream,
                new JasonQueryDbStorageMigrationHelperObservation(true, true, 31, true),
                new JasonQueryDbStorageMigrationHelperObservation(true, false, null, false)
            );

            Assert.AreEqual(JasonQueryDbStorageMigrationHelperRole.LegacyStorageV1, role);
        }

        [TestMethod]
        public void ProcessFailure_BridgeDetectsModernNonZeroExit()
        {
            var role = JasonQueryDbStorageMigrationProcessFailureFactory.ResolveFailureRole
            (
                JasonQueryDbStorageMigrationProcessPhase.BridgingLogicalStream,
                new JasonQueryDbStorageMigrationHelperObservation(true, false, null, false),
                new JasonQueryDbStorageMigrationHelperObservation(true, true, 41, true)
            );

            Assert.AreEqual(JasonQueryDbStorageMigrationHelperRole.ModernStorageV2CandidateWriter, role);
        }

        [TestMethod]
        public void ProcessFailure_BridgeWithNoExitedHelper_IsUndetermined()
        {
            var role = JasonQueryDbStorageMigrationProcessFailureFactory.ResolveFailureRole
            (
                JasonQueryDbStorageMigrationProcessPhase.BridgingLogicalStream,
                new JasonQueryDbStorageMigrationHelperObservation(true, false, null, false),
                new JasonQueryDbStorageMigrationHelperObservation(true, false, null, false)
            );

            Assert.AreEqual(JasonQueryDbStorageMigrationHelperRole.Undetermined, role);
        }

        [TestMethod]
        public void ProcessFailure_IOExceptionMapsToPipeFailure()
        {
            var kind = JasonQueryDbStorageMigrationProcessFailureFactory.ResolveFailureKind
            (
                new IOException("Sensitive pipe detail"),
                JasonQueryDbStorageMigrationProcessPhase.BridgingLogicalStream
            );

            Assert.AreEqual(JasonQueryDbStorageMigrationProcessFailureKind.PipeFailure, kind);
        }

        [TestMethod]
        public void ProcessFailure_InvalidDataMapsToProtocolViolation()
        {
            var kind = JasonQueryDbStorageMigrationProcessFailureFactory.ResolveFailureKind
            (
                new InvalidDataException("Sensitive protocol detail"),
                JasonQueryDbStorageMigrationProcessPhase.AwaitingStorageV1LogicalStreamReady
            );

            Assert.AreEqual(JasonQueryDbStorageMigrationProcessFailureKind.ProtocolViolation, kind);
        }

        [TestMethod]
        public void ProcessFailure_StartPhaseMapsToProcessStartFailure()
        {
            var kind = JasonQueryDbStorageMigrationProcessFailureFactory.ResolveFailureKind
            (
                new InvalidOperationException("Sensitive process path"),
                JasonQueryDbStorageMigrationProcessPhase.StartingModernHelper
            );

            Assert.AreEqual(JasonQueryDbStorageMigrationProcessFailureKind.ProcessStartFailure, kind);
        }

        [TestMethod]
        public void ProcessFailure_MessageContainsOnlySafeDiagnosticFields()
        {
            const string secretPath = "D:\\Secret\\JasonQuery.db";
            const string secretPassword = "SuperSecretPassword";

            var failure = JasonQueryDbStorageMigrationProcessFailureFactory.Create
            (
                new InvalidDataException(secretPath + " " + secretPassword),
                JasonQueryDbStorageMigrationProcessPhase.AwaitingStorageV1Validated,
                new JasonQueryDbStorageMigrationHelperObservation(true, true, 53, true),
                JasonQueryDbStorageMigrationHelperObservation.NotStarted
            );

            Assert.Contains("HelperRole=LegacyStorageV1", failure.Message);
            Assert.Contains("Phase=AwaitingStorageV1Validated", failure.Message);
            Assert.Contains("FailureKind=ProtocolViolation", failure.Message);
            Assert.Contains("ExitCode=53", failure.Message);
            Assert.Contains("StandardErrorObserved=True", failure.Message);
            Assert.DoesNotContain(secretPath, failure.Message);
            Assert.DoesNotContain(secretPassword, failure.Message);
            Assert.IsNull(failure.InnerException);
        }

        [TestMethod]
        public void ProcessFailure_CleanupFailureFlagPreservesDiagnosticIdentity()
        {
            var original = JasonQueryDbStorageMigrationProcessFailureFactory.CreateKnown
            (
                JasonQueryDbStorageMigrationHelperRole.ModernStorageV2CandidateWriter,
                JasonQueryDbStorageMigrationProcessPhase.AwaitingCandidateValidated,
                JasonQueryDbStorageMigrationProcessFailureKind.ProtocolEndOfStream,
                61,
                true
            );

            var withCleanupFailure = JasonQueryDbStorageMigrationProcessFailureFactory.WithCandidateCleanupFailure(original);

            Assert.AreEqual(original.HelperRole, withCleanupFailure.HelperRole);
            Assert.AreEqual(original.Phase, withCleanupFailure.Phase);
            Assert.AreEqual(original.FailureKind, withCleanupFailure.FailureKind);
            Assert.AreEqual(original.ExitCode, withCleanupFailure.ExitCode);
            Assert.AreEqual(original.StandardErrorObserved, withCleanupFailure.StandardErrorObserved);
            Assert.IsTrue(withCleanupFailure.CandidateCleanupFailed);
        }

        private static byte[] CreatePayload(int length)
        {
            var payload = new byte[length];

            for (var index = 0; index < payload.Length; index++)
            {
                payload[index] = (byte)(index % 251);
            }

            return payload;
        }

        private sealed class TestScope : IDisposable
        {
            public TestScope()
            {
                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-Step389F-R3-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);

                LegacyHelperPath = Path.Combine(DirectoryPath, "Legacy.exe");
                ModernHelperPath = Path.Combine(DirectoryPath, "Modern.exe");
                SourceDatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                CandidateDatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db.candidate");

                File.WriteAllBytes(LegacyHelperPath, new byte[] { 1 });
                File.WriteAllBytes(ModernHelperPath, new byte[] { 2 });
                File.WriteAllBytes(SourceDatabasePath, new byte[] { 3 });
            }

            public string DirectoryPath { get; }

            public string LegacyHelperPath { get; }

            public string ModernHelperPath { get; }

            public string SourceDatabasePath { get; }

            public string CandidateDatabasePath { get; }

            public JasonQueryDbStorageMigrationProcessResult Run(string legacyHelper, string modernHelper, string sourceDatabase,
                                                                 string candidateDatabase, byte[] sourcePassword, byte[] targetPassword)
            {
                return new JasonQueryDbStorageMigrationProcessRunner().CreateCandidate
                (
                    legacyHelper,
                    modernHelper,
                    sourceDatabase,
                    candidateDatabase,
                    sourcePassword,
                    targetPassword
                );
            }

            public void Dispose()
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }

        private sealed class TrackingReadStream : MemoryStream
        {
            public TrackingReadStream(byte[] buffer) : base(buffer, false)
            {
            }

            public int MaximumRequestedReadCount { get; private set; }

            public override int Read(byte[] buffer, int offset, int count)
            {
                MaximumRequestedReadCount = Math.Max(MaximumRequestedReadCount, count);
                return base.Read(buffer, offset, count);
            }
        }

        private sealed class TrackingWriteStream : MemoryStream
        {
            public int MaximumWriteCount { get; private set; }

            public override void Write(byte[] buffer, int offset, int count)
            {
                MaximumWriteCount = Math.Max(MaximumWriteCount, count);
                base.Write(buffer, offset, count);
            }
        }

        private sealed class NonReadableStream : Stream
        {
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => 0;

            public override long Position
            {
                get => 0;
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
            }
        }

        private sealed class NonWritableStream : Stream
        {
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => 0;

            public override long Position
            {
                get => 0;
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
    }
}
