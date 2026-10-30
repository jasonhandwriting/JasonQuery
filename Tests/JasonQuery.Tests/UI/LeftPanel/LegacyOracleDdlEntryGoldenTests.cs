using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class LegacyOracleDdlEntryGoldenTests
    {
        private static Type Policy()
        {
            var result = typeof(EditTableForm).Assembly.GetType("JasonQuery.UI.Helpers.LegacyOracleDdlEntryPolicy");

            Assert.IsNotNull(result, "Missing Legacy Oracle DDL fail-closed entry policy");
            return result;
        }

        private static bool Allowed(DataSourceType menuSource, DataSourceType currentSource)
        {
            var method = Policy().GetMethod("CanOpenStructuralDdl", BindingFlags.Static | BindingFlags.Public);

            Assert.IsNotNull(method);
            return (bool)method.Invoke(null, new object[] { menuSource, currentSource });
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G01_ExistingTableFormRetainsExplicitIdentityOptIn()
        {
            var prop = typeof(EditTableForm).GetProperty("OracleTargetIdentity", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(prop);
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G02_ExistingColumnFormRetainsExplicitIdentityOptIn()
        {
            var prop = typeof(EditColumnForm).GetProperty("OracleTargetIdentity", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(prop);
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G03_LegacyOracleEntryPolicyIsPresent()
        {
            Assert.IsNotNull(Policy());
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G04_LegacyOracleTableMenuIsDenied()
        {
            Assert.IsFalse(Allowed(DataSourceType.Oracle, DataSourceType.Oracle));
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G05_LegacyOracleColumnMenuIsDenied()
        {
            Assert.IsFalse(Allowed(DataSourceType.Oracle, DataSourceType.Oracle));
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G06_LegacyOracleEditorTableEntryIsDenied()
        {
            Assert.IsFalse(Allowed(DataSourceType.Oracle, DataSourceType.Oracle));
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G07_SourceSwitchOracleToPostgreSqlIsDenied()
        {
            Assert.IsFalse(Allowed(DataSourceType.Oracle, DataSourceType.PostgreSql));
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G08_SourceSwitchPostgreSqlToOracleIsDenied()
        {
            Assert.IsFalse(Allowed(DataSourceType.PostgreSql, DataSourceType.Oracle));
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G09_StablePostgreSqlEditorAndColumnEntryRemainAllowed()
        {
            Assert.IsTrue(Allowed(DataSourceType.PostgreSql, DataSourceType.PostgreSql));
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G10_StableSqlServerEntryRemainsAllowed()
        {
            Assert.IsTrue(Allowed(DataSourceType.SqlServer, DataSourceType.SqlServer));
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G11_StableMySqlEntryRemainsAllowed()
        {
            Assert.IsTrue(Allowed(DataSourceType.MySql, DataSourceType.MySql));
        }

        [TestMethod, TestCategory("F02B3B2L")]
        public void G12_DisconnectedSourceFailsClosed()
        {
            Assert.IsFalse(Allowed(DataSourceType.None, DataSourceType.None));
        }
    }
}
