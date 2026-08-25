using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateMetadataSettingsContractTests
    {
        [DataTestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        [DataRow("OfficialWebsite", UpdateMetadataSourceKind.OfficialWebsite)]
        [DataRow("github", UpdateMetadataSourceKind.GitHub)]
        [DataRow("LocalFolder", UpdateMetadataSourceKind.LocalFolder)]
        public void ParseSource_KnownValue_ReturnsExpected(string value, UpdateMetadataSourceKind expected)
        {
            Assert.AreEqual(expected, UpdateMetadataSettingsContract.ParseSource(value));
        }

        [DataTestMethod]
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
    }
}
