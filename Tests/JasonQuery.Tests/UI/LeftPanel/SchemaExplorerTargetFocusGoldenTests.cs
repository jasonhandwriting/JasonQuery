using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F2-C3: revised provider-neutral GroupBy restoration contract.
    //No C1 grid, database connection, DDL type or previous display-row index is required.
    [TestClass]
    [TestCategory("F2C3A")]
    public sealed class SchemaExplorerTargetFocusGoldenTests
    {
        private const string PolicyName = "JasonQuery.UI.LeftPanel.SchemaExplorerTargetFocusPolicy";

        private static MethodInfo Method(string name, params Type[] types)
        {
            var type = typeof(SharedSchemaMetadataSnapshot).Assembly.GetType(PolicyName, false);

            Assert.IsNotNull(type);

            var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, types, null);

            Assert.IsNotNull(method, "Missing GroupBy restoration policy method " + name);
            return method;
        }

        private static string[] Path(bool sameScope, bool reloadSucceeded, params string[] nodes)
        {
            var method = Method("BuildRetainedGroupPath", typeof(string[]), typeof(bool), typeof(bool));
            var result = method.Invoke(null, new object[] { nodes, sameScope, reloadSucceeded });

            Assert.IsInstanceOfType(result, typeof(string[]));
            return (string[])result;
        }

        private static int FocusIndex(string oldName, int ordinal, params string[] remainingNames)
        {
            var method = Method("FindNearestFocusIndex", typeof(string), typeof(int), typeof(string[]));
            var result = method.Invoke(null, new object[] { oldName, ordinal, remainingNames });

            Assert.IsInstanceOfType(result, typeof(int));
            return (int)result;
        }

        [TestMethod]
        public void S301_InitialLoadWithoutCapturedGroupKeepsDefaultExpansion()
        {
            CollectionAssert.AreEqual(Array.Empty<string>(), Path(true, true));
        }

        [TestMethod]
        public void S302_PostgreSqlRestoresOnlySelectedPublicAbc0Path()
        {
            CollectionAssert.AreEqual
            (
                new[] { "PostgreSQL", "public", "Tables", "abc0" },
                Path(true, true, "PostgreSQL", "public", "Tables", "abc0")
            );
        }

        [TestMethod]
        public void S303_OracleUsesItsActualGroupLevelsWithoutInventingASchema()
        {
            CollectionAssert.AreEqual
            (
                new[] { "Oracle-JQ98XX", "Tables", "JQ98_B2_C_ADD" },
                Path(true, true, "Oracle-JQ98XX", "Tables", "JQ98_B2_C_ADD")
            );
        }

        [TestMethod]
        public void S304_MySqlAndSqlServerKeepCapturedGroupStructure()
        {
            CollectionAssert.AreEqual
            (
                new[] { "SQL Server", "MyDB", "Tables", "abc0" },
                Path(true, true, "SQL Server", "MyDB", "Tables", "abc0")
            );

            CollectionAssert.AreEqual
            (
                new[] { "MySQL", "sakila", "Tables" },
                Path(true, true, "MySQL", "sakila", "Tables")
            );
        }

        [TestMethod]
        public void S305_ChangedScopeNeverRestoresOldGroupPath()
        {
            CollectionAssert.AreEqual(Array.Empty<string>(), Path(false, true, "PostgreSQL", "public", "Tables"));
        }

        [TestMethod]
        public void S306_FailedReloadNeverRestoresCapturedPath()
        {
            CollectionAssert.AreEqual(Array.Empty<string>(), Path(true, false, "PostgreSQL", "public", "Tables"));
        }

        [TestMethod]
        public void S307_ExistingSelectedColumnKeepsItsExactLocationWithoutDdlInference()
        {
            Assert.AreEqual(1, FocusIndex("c155_uuid", 0, "c154_uuid", "c155_uuid", "c156_uuid"));
        }

        [TestMethod]
        public void S308_MissingColumnFallsBackToClosestSameTableOrdinal()
        {
            Assert.AreEqual(2, FocusIndex("old_column", 3, "c153_uuid", "c154_uuid", "c156_uuid"));
        }

        [TestMethod]
        public void S309_MissingTableFallsBackToClosestSameGroupOrdinal()
        {
            Assert.AreEqual(1, FocusIndex("abc0", 1, "a_numeric_test", "abc1", "countryinfo"));
        }

        [TestMethod]
        public void S310_NoRemainingSiblingNeverInventsASelection()
        {
            Assert.AreEqual(-1, FocusIndex("abc0", 0));
            Assert.AreEqual(-1, FocusIndex("column1", -1, "other_column"));
        }

        [TestMethod]
        public void S311_QuotedIdentifierCaseIsPreserved()
        {
            Assert.AreEqual(1, FocusIndex("NewName", 0, "newname", "NewName"));
        }
    }
}
