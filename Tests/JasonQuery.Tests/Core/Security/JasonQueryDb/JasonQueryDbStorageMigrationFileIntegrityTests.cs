using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationFileIntegrityTests
    {
        [TestMethod]
        public void ComputeSha256_ReturnsCanonicalUppercaseHex()
        {
            using (var scope = new TestScope())
            {
                File.WriteAllBytes(scope.FilePath, Encoding.ASCII.GetBytes("abc"));

                Assert.AreEqual
                (
                    "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD",
                    JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(scope.FilePath)
                );
            }
        }

        [TestMethod]
        public void MatchesSha256_ReturnsExpectedResult()
        {
            using (var scope = new TestScope())
            {
                File.WriteAllText(scope.FilePath, "payload", Encoding.UTF8);

                var expected = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(scope.FilePath);

                Assert.IsTrue(JasonQueryDbStorageMigrationFileIntegrity.MatchesSha256(scope.FilePath, expected));

                Assert.IsFalse
                (
                    JasonQueryDbStorageMigrationFileIntegrity.MatchesSha256
                    (
                        Path.Combine(scope.DirectoryPath, "missing.bin"),
                        expected
                    )
                );
            }
        }

        [TestMethod]
        public void ComputeSha256_MissingFileThrows()
        {
            using (var scope = new TestScope())
            {
                Assert.ThrowsExactly<FileNotFoundException>
                (
                    () => JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(scope.FilePath)
                );
            }
        }

        [TestMethod]
        public void MatchesSha256_RejectsNonCanonicalExpectedHash()
        {
            using (var scope = new TestScope())
            {
                File.WriteAllText(scope.FilePath, "payload", Encoding.UTF8);

                Assert.ThrowsExactly<ArgumentException>
                (
                    () => JasonQueryDbStorageMigrationFileIntegrity.MatchesSha256
                    (
                        scope.FilePath,
                        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
                    )
                );
            }
        }

        private sealed class TestScope : IDisposable
        {
            public TestScope()
            {
                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-Step389F-R4A-FileIntegrity-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);
                FilePath = Path.Combine(DirectoryPath, "artifact.bin");
            }

            public string DirectoryPath { get; }

            public string FilePath { get; }

            public void Dispose()
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }
    }
}
