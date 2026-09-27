using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageV2ReadOnlyValidationProcessRunnerTests
    {
        [TestMethod]
        public void Validate_MissingModernHelper_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                Assert.ThrowsExactly<FileNotFoundException>
                (
                    () => scope.Run(Path.Combine(scope.DirectoryPath, "Missing.exe"))
                );
            }
        }

        [TestMethod]
        public void Validate_MissingCandidate_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                File.Delete(scope.CandidatePath);

                Assert.ThrowsExactly<FileNotFoundException>
                (
                    () => scope.Run(scope.HelperPath)
                );
            }
        }

        [TestMethod]
        public void Validate_NullPassword_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                var runner = new JasonQueryDbStorageV2ReadOnlyValidationProcessRunner();

                Assert.ThrowsExactly<ArgumentException>
                (
                    () => runner.Validate(scope.HelperPath, scope.CandidatePath, null)
                );
            }
        }

        [TestMethod]
        public void Validate_ExistingJournalSidecar_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                File.WriteAllBytes(scope.CandidatePath + "-journal", new byte[] { 1 });

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Run(scope.HelperPath)
                );
            }
        }

        [TestMethod]
        public void Validate_ExistingWalSidecar_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                File.WriteAllBytes(scope.CandidatePath + "-wal", new byte[] { 1 });

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Run(scope.HelperPath)
                );
            }
        }

        [TestMethod]
        public void Validate_ExistingShmSidecar_FailsBeforeLaunch()
        {
            using (var scope = new TestScope())
            {
                File.WriteAllBytes(scope.CandidatePath + "-shm", new byte[] { 1 });

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Run(scope.HelperPath)
                );
            }
        }

        [TestMethod]
        public void Result_RequiresSha256()
        {
            Assert.ThrowsExactly<ArgumentException>
            (
                () => new JasonQueryDbStorageV2ReadOnlyValidationResult(null)
            );
        }

        [TestMethod]
        public void Result_PreservesCandidateSha256()
        {
            var value = new string('A', 64);
            var result = new JasonQueryDbStorageV2ReadOnlyValidationResult(value);

            Assert.AreEqual(value, result.CandidateDatabaseSha256);
        }

        private sealed class TestScope : IDisposable
        {
            public TestScope()
            {
                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-R4B-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);

                HelperPath = Path.Combine(DirectoryPath, "Modern.exe");
                CandidatePath = Path.Combine(DirectoryPath, "Candidate.db");

                Password = Encoding.UTF8.GetBytes
                (
                    Convert.ToBase64String(new byte[32])
                );

                File.WriteAllBytes(HelperPath, new byte[] { 1 });
                File.WriteAllBytes(CandidatePath, new byte[] { 2, 3, 4 });
            }

            public string DirectoryPath { get; }

            public string HelperPath { get; }

            public string CandidatePath { get; }

            public byte[] Password { get; }

            public JasonQueryDbStorageV2ReadOnlyValidationResult Run(string helperPath)
            {
                var runner = new JasonQueryDbStorageV2ReadOnlyValidationProcessRunner();

                return runner.Validate
                (
                    helperPath,
                    CandidatePath,
                    Password
                );
            }

            public void Dispose()
            {
                Array.Clear(Password, 0, Password.Length);

                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }
    }
}
