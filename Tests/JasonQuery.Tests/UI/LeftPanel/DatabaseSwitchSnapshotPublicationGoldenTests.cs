using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //C1: a completed provider metadata load, not a database-name change, authorizes MainForm to publish the new database's read-only F7 snapshot.
    [TestClass]
    [TestCategory("F7L2B2C1C1")]
    public sealed class DatabaseSwitchSnapshotPublicationGoldenTests
    {
        private static DataTable Metadata()
        {
            var table = new DataTable();

            table.Columns.Add("SchemaObject");
            table.Columns.Add("SchemaType");
            table.Columns.Add("SchemaName");

            return table;
        }

        private static MethodInfo RequireMethod(string name, BindingFlags flags)
        {
            var method = typeof(MainForm).GetMethod(name, flags);

            Assert.IsNotNull(method, "Missing C1 database-switch integration: " + name);
            return method;
        }

        private static bool CanPublish(DataSourceType expected, DataSourceType current, string expectedScope, string currentScope, DataTable prior, DataTable loaded)
        {
            var method = RequireMethod("CanPublishCompletedDatabaseSwitch", BindingFlags.NonPublic | BindingFlags.Static);

            return (bool)method.Invoke(null, new object[] { expected, current, expectedScope, currentScope, prior, loaded });
        }

        private static int CountDirectCalls(string sourceName, string calledName)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var source = RequireMethod(sourceName, flags);
            var target = RequireMethod(calledName, flags);
            var il = source.GetMethodBody()?.GetILAsByteArray();

            Assert.IsNotNull(il);
            Assert.AreSame(source.Module, target.Module);

            var count = 0;

            for (var i = 0; i < il.Length - 4; i++)
            {
                //Check both call and callvirt tokens for framework/compiler compatibility.
                if ((il[i] == 0x28 || il[i] == 0x6F) && BitConverter.ToInt32(il, i + 1) == target.MetadataToken)
                {
                    count++;
                }
            }

            return count;
        }

        [TestMethod]
        public void C101_PostgreSqlMenuCompletionNotifiesSharedF7()
        {
            Assert.IsTrue(CountDirectCalls("SwitchDatabase_PostgreSql", "CompleteSharedSchemaDatabaseSwitch") >= 1);
        }

        [TestMethod]
        public void C102_SqlServerAndMySqlCompletionEventsNotifySharedF7()
        {
            Assert.IsTrue(CountDirectCalls("ValueUpdated", "CompleteSharedSchemaDatabaseSwitch") >= 2, "Both SQL Server and MySQL completion paths must notify F7, even with only one QueryForm tab.");
        }

        [TestMethod]
        public void C103_BothUseMenusInvalidateOldScopeBeforeCompletion()
        {
            Assert.IsTrue(CountDirectCalls("SwitchDatabase_SqlServer", "BeginSharedSchemaDatabaseSwitch") >= 1);
            Assert.IsTrue(CountDirectCalls("SwitchDatabase_MySql", "BeginSharedSchemaDatabaseSwitch") >= 1);
        }

        [TestMethod]
        public void C104_DirectUseReceivesPendingScopeBeforeMetadataLoad()
        {
            Assert.IsTrue(CountDirectCalls("ValueUpdated", "BeginSharedSchemaDatabaseSwitch") >= 1);
        }

        [TestMethod]
        public void C105_ThreeProvidersAcceptFreshSameScopeMetadata()
        {
            foreach (var provider in new[] { DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                Assert.IsTrue(CanPublish(provider, provider, "db|A", "db|A", Metadata(), Metadata()), provider.ToString());
            }
        }

        [TestMethod]
        public void C106_SwitchWithoutReloadCannotRestampOldMetadata()
        {
            var previous = Metadata();

            Assert.IsFalse(CanPublish(DataSourceType.PostgreSql, DataSourceType.PostgreSql, "pg|B", "pg|B", previous, previous));
        }

        [TestMethod]
        public void C107_ScopeChangeDuringReloadRejectsPublication()
        {
            Assert.IsFalse(CanPublish(DataSourceType.SqlServer, DataSourceType.SqlServer, "sql|B", "sql|C", Metadata(), Metadata()));
        }

        [TestMethod]
        public void C108_ProviderMismatchRejectsPublication()
        {
            Assert.IsFalse(CanPublish(DataSourceType.MySql, DataSourceType.SqlServer, "mysql|B", "mysql|B", Metadata(), Metadata()));
        }

        [TestMethod]
        public void C109_NullOrIncompleteMetadataRejectsPublication()
        {
            var previous = Metadata();

            Assert.IsFalse(CanPublish(DataSourceType.MySql, DataSourceType.MySql, "mysql|B", "mysql|B", previous, null));

            var incomplete = new DataTable();

            incomplete.Columns.Add("SchemaObject");

            Assert.IsFalse(CanPublish(DataSourceType.MySql, DataSourceType.MySql, "mysql|B", "mysql|B", previous, incomplete));
        }

        [TestMethod]
        public void C110_OracleCannotEnterNonOracleSwitchPublicationPath()
        {
            Assert.IsFalse(CanPublish(DataSourceType.Oracle, DataSourceType.Oracle, "oracle|XEPDB1", "oracle|XEPDB1", Metadata(), Metadata()));
        }

        [TestMethod]
        public void C111_EmptyScopeCannotQualifyReload()
        {
            Assert.IsFalse(CanPublish(DataSourceType.PostgreSql, DataSourceType.PostgreSql, string.Empty, string.Empty, Metadata(), Metadata()));
        }

        [TestMethod]
        public void C112_NoProviderCannotQualifyReload()
        {
            Assert.IsFalse(CanPublish(DataSourceType.None, DataSourceType.None, "db|B", "db|B", Metadata(), Metadata()));
        }
    }
}
