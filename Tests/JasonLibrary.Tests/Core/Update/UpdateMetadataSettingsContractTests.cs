using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateMetadataSettingsContractTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        [DataRow("OfficialWebsite", UpdateMetadataSourceKind.OfficialWebsite)]
        [DataRow("LocalFolder", UpdateMetadataSourceKind.LocalFolder)]
        public void ParseSource_EnabledValue_ReturnsExpected(string value, UpdateMetadataSourceKind expected)
        {
            Assert.AreEqual(expected, UpdateMetadataSettingsContract.ParseSource(value));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void ParseSource_LegacyGitHubValue_ReturnsOfficialWebsite()
        {
            Assert.AreEqual
            (
                UpdateMetadataSourceKind.OfficialWebsite,
                UpdateMetadataSettingsContract.ParseSource("github")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("Unknown")]
        [DataRow("999")]
        public void ParseSource_InvalidValue_ReturnsOfficialWebsite(string value)
        {
            Assert.AreEqual
            (
                UpdateMetadataSourceKind.OfficialWebsite,
                UpdateMetadataSettingsContract.ParseSource(value)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void OfficialWebsiteContract_UsesCanonicalBareDomain()
        {
            Assert.AreEqual("https://jasonquery.org", UpdateMetadataSettingsContract.OfficialWebsiteBaseUrl);

            Assert.AreEqual
            (
                "https://jasonquery.org/JasonQueryUpdate/jasonquery-update.json",
                UpdateMetadataSettingsContract.OfficialWebsiteMetadataUrl
            );

            Assert.IsFalse(UpdateMetadataSettingsContract.OfficialWebsiteMetadataUrl.Contains("www."));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void CompanyUpdatePackageContract_UsesProductionStartingVersion()
        {
            Assert.AreEqual("JasonQuery-Company-Update-v", UpdateMetadataSettingsContract.CompanyUpdatePackageFileNamePrefix);
            Assert.AreEqual("0.95.0", UpdateMetadataSettingsContract.CompanyUpdatePackageMinimumVersion);
        }
    }
}
