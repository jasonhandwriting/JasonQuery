using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateReleaseSelectorTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void FindUpdate_ProductionInstallation_IgnoresNewerTestRelease()
        {
            var manifest = CreateManifest
            (
                CreateRelease("v0.93.0", false),
                CreateRelease("v0.93.1", true)
            );

            var result = new UpdateReleaseSelector().FindUpdate
            (
                manifest,
                "0.92",
                UpdateChannel.Production
            );

            Assert.IsNotNull(result);
            Assert.AreEqual("0.93.0", result.Version);
            Assert.AreEqual(UpdateChannel.Production, result.Channel);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void FindUpdate_TestInstallation_SelectsNewerTestRelease()
        {
            var manifest = CreateManifest
            (
                CreateRelease("v0.93.0", false),
                CreateRelease("v0.93.1", true)
            );

            var result = new UpdateReleaseSelector().FindUpdate
            (
                manifest,
                "0.92.9",
                UpdateChannel.Test
            );

            Assert.IsNotNull(result);
            Assert.AreEqual("0.93.1", result.Version);
            Assert.AreEqual(UpdateChannel.Test, result.Channel);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void FindUpdate_EqualOnlineVersions_PrefersProductionRelease()
        {
            var manifest = CreateManifest
            (
                CreateRelease("v0.93.0", true),
                CreateRelease("v0.93.0", false)
            );

            var result = new UpdateReleaseSelector().FindUpdate
            (
                manifest,
                "0.92.9",
                UpdateChannel.Test
            );

            Assert.IsNotNull(result);
            Assert.AreEqual(UpdateChannel.Production, result.Channel);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void FindUpdate_EquivalentTwoAndThreePartVersion_ReturnsNull()
        {
            var manifest = CreateManifest(CreateRelease("v0.92.0", false));

            var result = new UpdateReleaseSelector().FindUpdate
            (
                manifest,
                "0.92",
                UpdateChannel.Production
            );

            Assert.IsNull(result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void FindUpdate_DraftOrInvalidDigest_ReturnsNull()
        {
            var draft = CreateRelease("v0.93.0", false);

            draft.Draft = true;

            var invalidDigest = CreateRelease("v0.94.0", false);

            invalidDigest.Assets[0].Digest = "sha256:invalid";

            var result = new UpdateReleaseSelector().FindUpdate
            (
                CreateManifest(draft, invalidDigest),
                "0.92",
                UpdateChannel.Production
            );

            Assert.IsNull(result);
        }

        private static UpdateMetadataManifest CreateManifest(params UpdateReleaseMetadata[] releases)
        {
            return new UpdateMetadataManifest
            {
                SchemaVersion = 1,
                Product = "JasonQuery",
                Releases = new List<UpdateReleaseMetadata>(releases)
            };
        }

        private static UpdateReleaseMetadata CreateRelease(string version, bool prerelease)
        {
            var packageName = prerelease ? UpdateMetadataSettingsContract.TestPackageFileName : UpdateMetadataSettingsContract.ProductionPackageFileName;

            return new UpdateReleaseMetadata
            {
                TagName = version,
                Prerelease = prerelease,
                Assets = new List<UpdateAssetMetadata>
                {
                    new UpdateAssetMetadata
                    {
                        Name = packageName,
                        State = "uploaded",
                        Size = 100,
                        Digest = "sha256:" + new string('a', 64),
                        BrowserDownloadUrl = "https://example.test/" + packageName
                    }
                }
            };
        }
    }
}