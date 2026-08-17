using JasonQuery.Core.Database.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Connection
{
    [TestClass]
    public sealed class DatabaseServerVersionInfoTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void Oracle_18_0_0_0_0_DisplaysOracle18_0()
        {
            var actual = DatabaseServerVersionInfo.Create(DataSourceType.Oracle, "18.0.0.0.0");

            Assert.IsTrue(actual.IsKnown);
            Assert.AreEqual(DataSourceType.Oracle, actual.DataSourceType);
            Assert.AreEqual("Oracle", actual.ProductName);
            Assert.AreEqual("18.0", actual.DisplayVersion);
            Assert.AreEqual(18, actual.MajorVersion);
            Assert.AreEqual(0, actual.MinorVersion);
            Assert.AreEqual("Oracle 18.0", actual.DisplayText);
            Assert.AreEqual("Oracle 18.0 (18.0.0.0.0)", actual.DiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void PostgreSql_17_5_DisplaysPostgreSql17_5()
        {
            var actual = DatabaseServerVersionInfo.Create(DataSourceType.PostgreSql, "17.5");

            Assert.AreEqual(DataSourceType.PostgreSql, actual.DataSourceType);
            Assert.AreEqual("PostgreSQL", actual.ProductName);
            Assert.AreEqual("17.5", actual.DisplayVersion);
            Assert.AreEqual(17, actual.MajorVersion);
            Assert.AreEqual(5, actual.MinorVersion);
            Assert.AreEqual("PostgreSQL 17.5", actual.DisplayText);
            Assert.AreEqual("PostgreSQL 17.5", actual.DiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void SqlServer_15_0_2000_5_DisplaysSqlServer2019()
        {
            var actual = DatabaseServerVersionInfo.Create(DataSourceType.SqlServer, "15.0.2000.5");

            Assert.AreEqual(DataSourceType.SqlServer, actual.DataSourceType);
            Assert.AreEqual("SQL Server", actual.ProductName);
            Assert.AreEqual("2019", actual.DisplayVersion);
            Assert.AreEqual(15, actual.MajorVersion);
            Assert.AreEqual(0, actual.MinorVersion);
            Assert.AreEqual("SQL Server 2019", actual.DisplayText);
            Assert.AreEqual("SQL Server 2019 (15.0.2000.5)", actual.DiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void MySql_8_0_34_DisplaysMySql8_0()
        {
            var actual = DatabaseServerVersionInfo.Create(DataSourceType.MySql, "8.0.34");

            Assert.AreEqual(DataSourceType.MySql, actual.DataSourceType);
            Assert.AreEqual("MySQL", actual.ProductName);
            Assert.AreEqual("8.0", actual.DisplayVersion);
            Assert.AreEqual(8, actual.MajorVersion);
            Assert.AreEqual(0, actual.MinorVersion);
            Assert.AreEqual("MySQL 8.0", actual.DisplayText);
            Assert.AreEqual("MySQL 8.0 (8.0.34)", actual.DiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void MySqlVersionWithSuffix_DisplaysMajorMinorOnly()
        {
            var actual = DatabaseServerVersionInfo.Create(DataSourceType.MySql, "8.0.34-commercial");

            Assert.AreEqual("MySQL", actual.ProductName);
            Assert.AreEqual("8.0", actual.DisplayVersion);
            Assert.AreEqual("MySQL 8.0", actual.DisplayText);
            Assert.AreEqual("MySQL 8.0 (8.0.34-commercial)", actual.DiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void MariaDbVersion_DisplaysMariaDbProductName()
        {
            var actual = DatabaseServerVersionInfo.Create(DataSourceType.MySql, "5.5.5-10.11.6-MariaDB");

            Assert.AreEqual(DataSourceType.MySql, actual.DataSourceType);
            Assert.AreEqual("MariaDB", actual.ProductName);
            Assert.AreEqual("10.11", actual.DisplayVersion);
            Assert.AreEqual(10, actual.MajorVersion);
            Assert.AreEqual(11, actual.MinorVersion);
            Assert.AreEqual("MariaDB 10.11", actual.DisplayText);
            Assert.AreEqual("MariaDB 10.11 (5.5.5-10.11.6-MariaDB)", actual.DiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void DiagnosticText_PreservesRawVersion()
        {
            var actual = DatabaseServerVersionInfo.Create(DataSourceType.Oracle, " 19.3.0.0.0 ");

            Assert.AreEqual("19.3.0.0.0", actual.RawVersion);
            Assert.AreEqual("Oracle 19.3", actual.DisplayText);
            Assert.AreEqual("Oracle 19.3 (19.3.0.0.0)", actual.DiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        public void EmptyVersion_ReturnsEmptyDisplayText()
        {
            var actual = DatabaseServerVersionInfo.Create(DataSourceType.PostgreSql, "   ");

            Assert.IsFalse(actual.IsKnown);
            Assert.AreEqual(DataSourceType.None, actual.DataSourceType);
            Assert.AreEqual(string.Empty, actual.RawVersion);
            Assert.AreEqual(string.Empty, actual.DisplayText);
            Assert.AreEqual(string.Empty, actual.DiagnosticText);
        }
    }
}