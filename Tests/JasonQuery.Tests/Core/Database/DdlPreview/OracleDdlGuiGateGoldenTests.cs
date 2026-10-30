using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    [TestClass]
    public sealed class OracleDdlGuiGateGoldenTests
    {
        private static readonly Guid Id = Guid.Parse("11111111-2222-3333-4444-555555555555");

        private static OracleDdlTargetIdentity Selection(string owner = "SYS")
        {
            return new OracleDdlTargetIdentity
            {
                VerifiedSessionUser = owner,
                MetadataOwner = owner,
                SelectedOwner = owner,
                VerifiedConnectionId = Id,
                SelectionConnectionId = Id,
                CurrentConnectionId = Id,
                HasQualifiedSelectedOwner = true
            };
        }

        private static Type Gate()
        {
            var result = typeof(TableDdlRequest).Assembly.GetType("JasonQuery.Core.Database.DdlPreview.OracleDdlGuiGate");

            Assert.IsNotNull(result, "Missing B3-B1 OracleDdlGuiGate");
            return result;
        }

        private static bool Refresh(OracleDdlTargetIdentity identity, string user, Guid connectionId, out OracleDdlTargetIdentity refreshed)
        {
            var method = Gate().GetMethod("TryRefresh", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);

            var args = new object[] { identity, user, connectionId, null };
            var result = (bool)method.Invoke(null, args);

            refreshed = (OracleDdlTargetIdentity)args[3];
            return result;
        }

        private static bool Execute(OracleDdlTargetIdentity identity, string user, Guid connectionId, string displayed, string rebuilt)
        {
            var method = Gate().GetMethod("CanExecute", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);
            return (bool)method.Invoke(null, new object[] { identity, user, connectionId, displayed, rebuilt });
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G01_LegacyOracleTableBuilderStillWorksUntilBridge()
        {
            var request = new TableDdlRequest { DataSourceType = DataSourceType.Oracle, Operation = TableDdlOperation.Drop, SchemaName = "legacy", TableName = "AABBCC" };

            Assert.AreEqual("DROP TABLE \"legacy\".\"AABBCC\";", TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G02_LegacyPostgreSqlBuilderNotChanged()
        {
            var request = new TableDdlRequest { DataSourceType = DataSourceType.PostgreSql, Operation = TableDdlOperation.Drop, SchemaName = "public", TableName = "AABBCC" };

            Assert.AreEqual("DROP TABLE \"public\".\"AABBCC\";", TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G03_TrustedSysRefreshSucceeds()
        {
            Assert.IsTrue(Refresh(Selection(), "SYS", Id, out var qualified));
            Assert.AreEqual("SYS", qualified.MetadataOwner);
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G04_MixedCaseExactOwnerSucceeds()
        {
            Assert.IsTrue(Refresh(Selection("MiXeD"), "MiXeD", Id, out var qualified));
            Assert.AreEqual("MiXeD", qualified.SelectedOwner);
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G05_SessionCaseMismatchFailsClosed()
        {
            Assert.IsFalse(Refresh(Selection("MiXeD"), "MIXED", Id, out _));
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G06_ReconnectAsSameUserFailsClosed()
        {
            Assert.IsFalse(Refresh(Selection(), "SYS", Guid.NewGuid(), out _));
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G07_UnqualifiedOwnerFailsClosed()
        {
            var selection = Selection();

            selection.HasQualifiedSelectedOwner = false;

            Assert.IsFalse(Refresh(selection, "SYS", Id, out _));
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G08_MetadataOwnerMismatchFailsClosed()
        {
            var selection = Selection();

            selection.MetadataOwner = "SYSTEM";

            Assert.IsFalse(Refresh(selection, "SYS", Id, out _));
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G09_CurrentIdentityComesFromReaderNotCaller()
        {
            var selection = Selection();

            selection.CurrentConnectionId = Guid.NewGuid();

            Assert.IsTrue(Refresh(selection, "SYS", Id, out var refreshed));
            Assert.AreEqual(Id, refreshed.CurrentConnectionId);
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G10_ExecutionRejectsModifiedSql()
        {
            Assert.IsFalse(Execute(Selection(), "SYS", Id, "DROP TABLE \"SYS\".\"X\";", "DROP TABLE \"SYS\".\"AABBCC\";"));
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G11_ExecutionAcceptsExactRebuiltSql()
        {
            var sql = "DROP TABLE \"SYS\".\"AABBCC\";";

            Assert.IsTrue(Execute(Selection(), "SYS", Id, sql, sql));
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G12_ExecutionRejectsEmptySql()
        {
            Assert.IsFalse(Execute(Selection(), "SYS", Id, string.Empty, string.Empty));
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G13_TableFormHasExplicitIdentityOptIn()
        {
            var prop = typeof(EditTableForm).GetProperty("OracleTargetIdentity", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(prop);
            Assert.AreEqual(typeof(OracleDdlTargetIdentity), prop.PropertyType);
        }

        [TestMethod, TestCategory("F02B3B1")]
        public void G14_ColumnFormHasExplicitIdentityOptIn()
        {
            var prop = typeof(EditColumnForm).GetProperty("OracleTargetIdentity", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(prop);
            Assert.AreEqual(typeof(OracleDdlTargetIdentity), prop.PropertyType);
        }
    }
}
