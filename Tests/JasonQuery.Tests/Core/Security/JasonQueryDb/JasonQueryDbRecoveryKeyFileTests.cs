using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbRecoveryKeyFileTests
    {
        [TestMethod]
        public void SaveAndVerify_RoundTripsCanonicalRecoveryKey()
        {
            using (var scope = new FileScope())
            using (var key = JasonQueryDbRecoveryKeyCodec.Generate())
            {
                var encoded = JasonQueryDbRecoveryKeyCodec.Encode(key);

                JasonQueryDbRecoveryKeyFile.SaveAndVerify(scope.FilePath, encoded, key.KeyId);

                Assert.AreEqual(encoded, JasonQueryDbRecoveryKeyFile.ReadEncodedKey(scope.FilePath));
            }
        }

        [TestMethod]
        public void SaveAndVerify_WritesUtf8WithoutBomOrNewline()
        {
            using (var scope = new FileScope())
            using (var key = JasonQueryDbRecoveryKeyCodec.Generate())
            {
                var encoded = JasonQueryDbRecoveryKeyCodec.Encode(key);

                JasonQueryDbRecoveryKeyFile.SaveAndVerify(scope.FilePath, encoded, key.KeyId);

                var bytes = File.ReadAllBytes(scope.FilePath);

                Assert.AreEqual((byte)'J', bytes[0]);
                Assert.AreEqual((byte)'Q', bytes[1]);
                Assert.AreEqual((byte)'1', bytes[2]);
                Assert.AreNotEqual(0x0A, bytes[bytes.Length - 1]);
            }
        }

        [TestMethod]
        public void SaveAndVerify_ExistingFile_RejectsWithoutOverwrite()
        {
            using (var scope = new FileScope())
            using (var key = JasonQueryDbRecoveryKeyCodec.Generate())
            {
                File.WriteAllText(scope.FilePath, "existing", Encoding.ASCII);

                var before = File.ReadAllBytes(scope.FilePath);

                Assert.ThrowsExactly<IOException>
                (
                    () => JasonQueryDbRecoveryKeyFile.SaveAndVerify
                    (
                        scope.FilePath,
                        JasonQueryDbRecoveryKeyCodec.Encode(key),
                        key.KeyId
                    )
                );

                CollectionAssert.AreEqual(before, File.ReadAllBytes(scope.FilePath));
            }
        }

        [TestMethod]
        public void SaveAndVerify_WrongExtension_Rejects()
        {
            using (var scope = new FileScope(".txt"))
            using (var key = JasonQueryDbRecoveryKeyCodec.Generate())
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbRecoveryKeyFile.SaveAndVerify
                    (
                        scope.FilePath,
                        JasonQueryDbRecoveryKeyCodec.Encode(key),
                        key.KeyId
                    )
                );
            }
        }

        [TestMethod]
        public void SaveAndVerify_MalformedKey_Rejects()
        {
            using (var scope = new FileScope())
            {
                Assert.ThrowsExactly<FormatException>
                (
                    () => JasonQueryDbRecoveryKeyFile.SaveAndVerify(scope.FilePath, "not-a-recovery-key", "A1B2C3D4")
                );
            }
        }

        [TestMethod]
        public void SaveAndVerify_KeyIdMismatch_Rejects()
        {
            using (var scope = new FileScope())
            using (var key = JasonQueryDbRecoveryKeyCodec.Generate())
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbRecoveryKeyFile.SaveAndVerify
                    (
                        scope.FilePath,
                        JasonQueryDbRecoveryKeyCodec.Encode(key),
                        "A1B2C3D4"
                    )
                );
            }
        }

        [TestMethod]
        public void ReadEncodedKey_MissingFile_Rejects()
        {
            using (var scope = new FileScope())
            {
                Assert.ThrowsExactly<FileNotFoundException>
                (
                    () => JasonQueryDbRecoveryKeyFile.ReadEncodedKey(scope.FilePath)
                );
            }
        }

        [TestMethod]
        public void ReadEncodedKey_OversizedFile_Rejects()
        {
            using (var scope = new FileScope())
            {
                File.WriteAllBytes(scope.FilePath, new byte[2048]);

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbRecoveryKeyFile.ReadEncodedKey(scope.FilePath)
                );
            }
        }

        [TestMethod]
        public void ReadEncodedKey_InvalidUtf8_Rejects()
        {
            using (var scope = new FileScope())
            {
                File.WriteAllBytes(scope.FilePath, new byte[] { 0xC3, 0x28 });

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => JasonQueryDbRecoveryKeyFile.ReadEncodedKey(scope.FilePath)
                );
            }
        }

        [TestMethod]
        public void TryDelete_RemovesRecoveryKeyFile()
        {
            using (var scope = new FileScope())
            {
                File.WriteAllText(scope.FilePath, "temporary", Encoding.ASCII);

                JasonQueryDbRecoveryKeyFile.TryDelete(scope.FilePath);

                Assert.IsFalse(File.Exists(scope.FilePath));
            }
        }

        private sealed class FileScope : IDisposable
        {
            public FileScope(string extension = JasonQueryDbRecoveryKeyFile.FileExtension)
            {
                DirectoryPath = Path.Combine(Path.GetTempPath(), "JasonQuery-R4F2E5-B3-KeyFile-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(DirectoryPath);
                FilePath = Path.Combine(DirectoryPath, "recovery" + extension);
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
