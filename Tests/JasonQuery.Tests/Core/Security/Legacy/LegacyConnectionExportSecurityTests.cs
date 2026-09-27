using JasonQuery.Core.Security.Legacy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Security.Legacy
{
    [TestClass]
    public class LegacyConnectionExportSecurityTests
    {
        [TestMethod]
        public void CreateArchivePassword_UsesHistoricalFormat()
        {
            var result = LegacyConnectionExportSecurity.CreateArchivePassword("Abc123!");

            Assert.AreEqual("jasonquery1231Abc123!exportDB!nf0", result);
        }

        [TestMethod]
        public void CreateArchivePassword_ThrowsForEmptyPassword()
        {
            Assert.ThrowsExactly<ArgumentException>(() => LegacyConnectionExportSecurity.CreateArchivePassword(string.Empty));
        }
    }
}
