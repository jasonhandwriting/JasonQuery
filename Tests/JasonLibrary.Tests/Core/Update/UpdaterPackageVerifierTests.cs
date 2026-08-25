using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using Updater.Core;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdaterPackageVerifierTests
    {
        [TestMethod]
        [TestCategory("Update")]
        public void Verify_AcceptsMatchingSizeAndSha256()
        {
            var directory = CreateTemporaryDirectory();
            var packagePath = Path.Combine(directory, "package.zip");

            try
            {
                File.WriteAllText(packagePath, "verified package", Encoding.UTF8);

                var size = new FileInfo(packagePath).Length;
                var digest = "sha256:" + Sha256Digest.ComputeFile(packagePath);
                var result = UpdatePackageVerifier.Verify(packagePath, size, digest);

                Assert.AreEqual(size, result.Size);
                Assert.AreEqual(digest.Substring("sha256:".Length), result.Sha256);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void Verify_RejectsSizeMismatchBeforeDigestCheck()
        {
            var directory = CreateTemporaryDirectory();
            var packagePath = Path.Combine(directory, "package.zip");

            try
            {
                File.WriteAllText(packagePath, "package", Encoding.UTF8);

                var exception = Assert.ThrowsException<UpdatePackageVerificationException>
                (
                    () => UpdatePackageVerifier.Verify(packagePath, 999, "sha256:" + new string('a', 64))
                );

                Assert.AreEqual(UpdatePackageVerificationFailureKind.SizeMismatch, exception.FailureKind);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void Verify_RejectsDigestMismatch()
        {
            var directory = CreateTemporaryDirectory();
            var packagePath = Path.Combine(directory, "package.zip");

            try
            {
                File.WriteAllText(packagePath, "package", Encoding.UTF8);

                var size = new FileInfo(packagePath).Length;

                var exception = Assert.ThrowsException<UpdatePackageVerificationException>
                (
                    () => UpdatePackageVerifier.Verify(packagePath, size, "sha256:" + new string('a', 64))
                );

                Assert.AreEqual(UpdatePackageVerificationFailureKind.DigestMismatch, exception.FailureKind);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static string CreateTemporaryDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "JasonQuery-UpdaterTests", Path.GetRandomFileName());

            Directory.CreateDirectory(path);

            return path;
        }
    }
}
