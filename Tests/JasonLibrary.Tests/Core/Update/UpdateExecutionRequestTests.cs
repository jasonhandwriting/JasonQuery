using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Updater.Core;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateExecutionRequestTests
    {
        [TestMethod]
        [TestCategory("Update")]
        public void Write_ProducesRequestThatUpdaterCanRead()
        {
            var packagePath = Path.Combine(Path.GetTempPath(), "JasonQuery64.zip");
            var request = CreateRequest(packagePath);
            var requestPath = UpdateExecutionRequestFile.Write(request);

            try
            {
                var updaterRequest = UpdaterRequest.Load(requestPath);

                Assert.AreEqual(request.SourceKind, updaterRequest.SourceKind);
                Assert.AreEqual(request.PackageLocation, updaterRequest.PackageLocation);
                Assert.AreEqual(request.ExpectedSize, updaterRequest.ExpectedSize);
                Assert.AreEqual(request.ExpectedDigest, updaterRequest.ExpectedDigest);
                Assert.AreEqual(request.TargetVersion, updaterRequest.TargetVersion);
            }
            finally
            {
                UpdateExecutionRequestFile.TryDelete(requestPath);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void Validate_RejectsSourceAndLocationTypeMismatch()
        {
            var request = CreateRequest("https://example.test/JasonQuery64.zip");

            request.SourceKind = UpdateMetadataSourceKind.GitHub.ToString();
            request.PackageIsLocal = true;

            Assert.ThrowsExactly<FormatException>(() => request.Validate());
        }

        [TestMethod]
        [TestCategory("Update")]
        public void Validate_RejectsAutomaticDowngrade()
        {
            var request = CreateRequest(Path.Combine(Path.GetTempPath(), "JasonQuery64.zip"));

            request.InstalledVersion = "0.95.0";
            request.TargetVersion = "0.94.0";

            Assert.ThrowsExactly<FormatException>(() => request.Validate());
        }

        [TestMethod]
        [TestCategory("Update")]
        public void Validate_TreatsMissingThirdComponentAsZero()
        {
            var request = CreateRequest(Path.Combine(Path.GetTempPath(), "JasonQuery64.zip"));

            request.InstalledVersion = "0.95";
            request.TargetVersion = "0.95.0";

            Assert.ThrowsExactly<FormatException>(() => request.Validate());
        }

        private static UpdateExecutionRequest CreateRequest(string packageLocation)
        {
            return new UpdateExecutionRequest
            {
                LocalizationCode = "en-US",
                LocalizationFile = "english.xml",
                Environment = "PROD",
                SourceKind = UpdateMetadataSourceKind.LocalFolder.ToString(),
                PackageLocation = packageLocation,
                PackageIsLocal = true,
                PackageName = "JasonQuery64.zip",
                ExpectedSize = 123,
                ExpectedDigest = "sha256:" + new string('a', 64),
                InstalledVersion = "0.94.0",
                TargetVersion = "0.95.0"
            };
        }
    }
}
