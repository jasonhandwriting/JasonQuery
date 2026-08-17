using JasonQuery.Core.Database.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Connection
{
    [TestClass]
    public sealed class SqlServerVersionInfoTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void Parse_Major15_ReturnsSqlServer2019()
        {
            var actual = SqlServerVersionInfo.Parse("15.0.2000.5");

            Assert.IsTrue(actual.IsKnown);
            Assert.AreEqual(15, actual.MajorVersion);
            Assert.AreEqual(0, actual.MinorVersion);
            Assert.AreEqual("2019", actual.ReleaseName);
            Assert.AreEqual("SQL Server 2019", actual.DisplayText);
            Assert.AreEqual("SQL Server 2019 (15.0.2000.5)", actual.DiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void Parse_Major16_ReturnsSqlServer2022()
        {
            var actual = SqlServerVersionInfo.Parse("16.0.1000.6");

            Assert.AreEqual("2022", actual.ReleaseName);
            Assert.AreEqual("SQL Server 2022", actual.DisplayText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void Parse_Major17_ReturnsSqlServer2025()
        {
            var actual = SqlServerVersionInfo.Parse("17.0.1000.7");

            Assert.AreEqual("2025", actual.ReleaseName);
            Assert.AreEqual("SQL Server 2025", actual.DisplayText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void Parse_Version10_50_ReturnsSqlServer2008R2()
        {
            var actual = SqlServerVersionInfo.Parse("10.50.6000.34");

            Assert.AreEqual(10, actual.MajorVersion);
            Assert.AreEqual(50, actual.MinorVersion);
            Assert.AreEqual("2008 R2", actual.ReleaseName);
            Assert.AreEqual("SQL Server 2008 R2", actual.DisplayText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void Parse_UnknownFutureVersion_UsesNumericFallback()
        {
            var actual = SqlServerVersionInfo.Parse("18.2.100.1");

            Assert.IsTrue(actual.IsKnown);
            Assert.AreEqual(string.Empty, actual.ReleaseName);
            Assert.AreEqual("SQL Server 18.2", actual.DisplayText);
            Assert.AreEqual("SQL Server 18.2 (18.2.100.1)", actual.DiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void Parse_EmptyVersion_ReturnsUnknown()
        {
            var actual = SqlServerVersionInfo.Parse("   ");

            Assert.IsFalse(actual.IsKnown);
            Assert.AreEqual(0, actual.MajorVersion);
            Assert.AreEqual(0, actual.MinorVersion);
            Assert.AreEqual(string.Empty, actual.ProductVersion);
            Assert.AreEqual(string.Empty, actual.DisplayText);
            Assert.AreEqual(string.Empty, actual.DiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void Parse_InvalidVersion_PreservesRawVersion()
        {
            var actual = SqlServerVersionInfo.Parse("custom-build");

            Assert.IsFalse(actual.IsKnown);
            Assert.AreEqual("custom-build", actual.ProductVersion);
            Assert.AreEqual("SQL Server custom-build", actual.DisplayText);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DatabaseVersion")]
        [TestCategory("SqlServer")]
        public void SqlServer2016_DoesNotSupportOptimizeForSequentialKey()
        {
            var actual = SqlServerVersionInfo.Parse("13.0.1601.5");

            Assert.AreEqual("2016", actual.ReleaseName);
            Assert.IsFalse(actual.SupportsOptimizeForSequentialKey);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DatabaseVersion")]
        [TestCategory("SqlServer")]
        public void SqlServer2019_SupportsOptimizeForSequentialKey()
        {
            var actual = SqlServerVersionInfo.Parse("15.0.2000.5");

            Assert.AreEqual("2019", actual.ReleaseName);
            Assert.IsTrue(actual.SupportsOptimizeForSequentialKey);
        }
    }
}