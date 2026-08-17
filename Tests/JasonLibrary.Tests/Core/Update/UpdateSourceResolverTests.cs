using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateSourceResolverTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void ResolveMetadata_OfficialWebsite_ReturnsConfiguredHttpsUrl()
        {
            var location = UpdateSourceResolver.ResolveMetadata
            (
                UpdateMetadataSourceKind.OfficialWebsite,
                string.Empty
            );

            Assert.IsFalse(location.IsLocalFile);
            Assert.AreEqual(UpdateMetadataSettingsContract.OfficialWebsiteMetadataUrl, location.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void ResolveMetadata_GitHub_ReturnsReleaseListEndpoint()
        {
            var location = UpdateSourceResolver.ResolveMetadata
            (
                UpdateMetadataSourceKind.GitHub,
                string.Empty
            );

            Assert.IsTrue
            (
                location.Value.EndsWith("/releases?per_page=20", StringComparison.Ordinal),
                location.Value
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void ResolveMetadata_LocalFolder_UsesMetadataFileInSelectedFolder()
        {
            var folder = Path.Combine("CompanyShare", "JasonQueryUpdate");

            var location = UpdateSourceResolver.ResolveMetadata
            (
                UpdateMetadataSourceKind.LocalFolder,
                folder
            );

            Assert.IsTrue(location.IsLocalFile);
            Assert.AreEqual(Path.Combine(folder, "jasonquery-update.json"), location.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void ResolvePackage_LocalFolder_IgnoresExternalUrlAndUsesAssetName()
        {
            var folder = Path.Combine("CompanyShare", "JasonQueryUpdate");

            var asset = new UpdateAssetMetadata
            {
                Name = "JasonQuery64.zip",
                BrowserDownloadUrl = "https://www.jasonquery.org/JasonQueryUpdate/JasonQuery64.zip"
            };

            var location = UpdateSourceResolver.ResolvePackage
            (
                UpdateMetadataSourceKind.LocalFolder,
                folder,
                asset
            );

            Assert.IsTrue(location.IsLocalFile);
            Assert.AreEqual(Path.Combine(folder, "JasonQuery64.zip"), location.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void ResolvePackage_InternetSource_RejectsHttpUrl()
        {
            var asset = new UpdateAssetMetadata
            {
                Name = "JasonQuery64.zip",
                BrowserDownloadUrl = "http://example.test/JasonQuery64.zip"
            };

            Assert.ThrowsException<FormatException>
            (
                () => UpdateSourceResolver.ResolvePackage
                (
                    UpdateMetadataSourceKind.OfficialWebsite,
                    string.Empty,
                    asset
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void ResolvePackage_LocalFolder_RejectsPathTraversalInAssetName()
        {
            var asset = new UpdateAssetMetadata { Name = @"..\JasonQuery64.zip" };

            Assert.ThrowsException<FormatException>
            (
                () => UpdateSourceResolver.ResolvePackage
                (
                    UpdateMetadataSourceKind.LocalFolder,
                    "CompanyShare",
                    asset
                )
            );
        }
    }
}