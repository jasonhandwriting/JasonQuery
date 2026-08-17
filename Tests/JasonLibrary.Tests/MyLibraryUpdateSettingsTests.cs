using JasonLibrary.Core;
using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core
{
    [TestClass]
    public sealed class MyLibraryUpdateSettingsTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void UpdateMetadataSource_InvalidValue_FallsBackToOfficialWebsite()
        {
            var original = MyLibrary.UpdateMetadataSource;

            try
            {
                MyLibrary.UpdateMetadataSource = (UpdateMetadataSourceKind)999;

                Assert.AreEqual
                (
                    UpdateMetadataSourceKind.OfficialWebsite,
                    MyLibrary.UpdateMetadataSource
                );
            }
            finally
            {
                MyLibrary.UpdateMetadataSource = original;
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void UpdateMetadataLocalFolder_AssignedValue_IsTrimmedAndNullSafe()
        {
            var original = MyLibrary.UpdateMetadataLocalFolder;

            try
            {
                MyLibrary.UpdateMetadataLocalFolder = "  CompanyShare  ";
                Assert.AreEqual("CompanyShare", MyLibrary.UpdateMetadataLocalFolder);

                MyLibrary.UpdateMetadataLocalFolder = null;
                Assert.AreEqual(string.Empty, MyLibrary.UpdateMetadataLocalFolder);
            }
            finally
            {
                MyLibrary.UpdateMetadataLocalFolder = original;
            }
        }
    }
}