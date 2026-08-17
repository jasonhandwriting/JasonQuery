using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Connection
{
    [TestClass]
    [DoNotParallelize]
    public sealed class DatabaseSqlExecutorVersionStateTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ResetVersionState();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ResetVersionState();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        [TestCategory("State")]
        public void SetSqlServerVersion_UpdatesAllVersionProperties()
        {
            DatabaseSqlExecutor.SetSqlServerVersion("15.0.2000.5");

            Assert.AreEqual("15.0.2000.5", DatabaseSqlExecutor.DbServerVersion);
            Assert.AreEqual(15, DatabaseSqlExecutor.SqlServerVersion.MajorVersion);
            Assert.AreEqual("SQL Server 2019", DatabaseSqlExecutor.SqlServerVersion.DisplayText);
            Assert.AreEqual(DataSourceType.SqlServer, DatabaseSqlExecutor.ServerVersionInfo.DataSourceType);
            Assert.AreEqual("SQL Server 2019", DatabaseSqlExecutor.DatabaseVersionDisplayText);
            Assert.AreEqual("SQL Server 2019 (15.0.2000.5)", DatabaseSqlExecutor.DatabaseVersionDiagnosticText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DatabaseVersion")]
        [TestCategory("State")]
        public void ClearDbServerVersion_ClearsAllVersionProperties()
        {
            DatabaseSqlExecutor.SetSqlServerVersion("15.0.2000.5");

            DatabaseSqlExecutor.ClearDbServerVersion();

            Assert.AreEqual(string.Empty, DatabaseSqlExecutor.DbServerVersion);
            Assert.IsFalse(DatabaseSqlExecutor.SqlServerVersion.IsKnown);
            Assert.IsFalse(DatabaseSqlExecutor.ServerVersionInfo.IsKnown);
            Assert.AreEqual(string.Empty, DatabaseSqlExecutor.DatabaseVersionDisplayText);
            Assert.AreEqual(string.Empty, DatabaseSqlExecutor.DatabaseVersionDiagnosticText);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DatabaseVersion")]
        [TestCategory("State")]
        public void SwitchDatabaseType_AfterConnectionReset_DoesNotRetainPreviousSqlServerVersion()
        {
            DatabaseSqlExecutor.SetSqlServerVersion("15.0.2000.5");

            //ConnectionForm.ApplyConnectionContextFromForm(...) performs this reset before applying a newly selected database connection.
            DatabaseSqlExecutor.ClearDbServerVersion();
            DatabaseSqlExecutor.SetDatabaseServerVersion(DataSourceType.Oracle, "18.0.0.0.0");

            Assert.AreEqual("Oracle 18.0", DatabaseSqlExecutor.DatabaseVersionDisplayText);
            Assert.AreEqual("Oracle 18.0 (18.0.0.0.0)", DatabaseSqlExecutor.DatabaseVersionDiagnosticText);
            Assert.AreEqual(string.Empty, DatabaseSqlExecutor.DbServerVersion);
            Assert.IsFalse(DatabaseSqlExecutor.SqlServerVersion.IsKnown);
        }

        private static void ResetVersionState()
        {
            DatabaseSqlExecutor.ResetCurrentConnection();
            DatabaseSqlExecutor.ClearDbServerVersion();
            DatabaseSqlExecutor.CurrentDataSource = DataSourceType.None;
            DatabaseSqlExecutor.DataSourceDisplayName = string.Empty;
        }
    }
}