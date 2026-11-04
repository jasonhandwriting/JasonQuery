using JasonQuery.Core.Database.Metadata.Oracle;
using JasonQuery.Core.Database.Metadata.Oracle.QueryEditor;
using JasonQuery.Core.Database.Metadata.Oracle.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.Core.Database.Metadata.Oracle
{
    [TestClass]
    [TestCategory("F7L2B2C_QUERY")]
    public sealed class OracleTrustedMetadataQueryGoldenTests
    {
        private static OracleMetadataContext CreateContext(Func<string, DataTable> query, bool needRows = false)
        {
            var target = new DataTable();

            target.Columns.Add("SchemaObject", typeof(string));
            target.Columns.Add("SchemaType", typeof(string));
            target.Columns.Add("SchemaName", typeof(string));
            target.Columns.Add("ColumnInfo", typeof(string));
            target.Columns.Add("OracleMetadataOwner", typeof(string));

            return new OracleMetadataContext
            {
                TargetSchemaTable = target,
                DbConnectionName = "TEST",
                DbUserNameUppercase = "JQ98XX",
                NeedSchemaRows = needRows,
                SortByColumnName = true,
                ExecuteQuery = query
            };
        }

        private static void AssertRejectedWithoutNullReference(Action operation)
        {
            Exception error = null;

            try
            {
                operation();
            }
            catch (Exception ex)
            {
                error = ex;
            }

            Assert.IsNotNull(error, "A failed metadata query returning null must not be accepted as an empty result.");
            Assert.IsFalse(error is NullReferenceException, "Reject a null result explicitly rather than crashing on it.");
        }

        [TestMethod]
        public void Q01_FailedTableAndViewNameQueryMustNotSilentlySucceed()
        {
            DataTable original = new DataTable();

            original.Columns.Add("TableName", typeof(string));
            original.Rows.Add("KNOWN_TABLE");

            AssertRejectedWithoutNullReference
            (
                () => OracleQueryEditorMetadataLoader.LoadTableAndViewNames
                (
                    ref original,
                    sql => null,
                    "JQ98XX"
                )
            );
        }

        [TestMethod]
        public void Q02_FailedTableInfoQueryMustNotBecomeEmptySchema()
        {
            var context = CreateContext(sql => null);

            AssertRejectedWithoutNullReference(() => OracleTableMetadataOrganizer.Organize(context));
        }

        [TestMethod]
        public void Q03_FailedTableQuantityQueryMustNotBecomeZeroTables()
        {
            var queryCount = 0;

            var context = CreateContext
            (
                sql =>
                {
                    queryCount++;
                    return queryCount == 1 ? new DataTable() : null;
                },
                true
            );

            AssertRejectedWithoutNullReference(() => OracleTableMetadataOrganizer.Organize(context));
        }

        [TestMethod]
        public void Q04_ValidEmptyTableInfoQueryIsAllowed()
        {
            var context = CreateContext(sql => new DataTable());
            var names = OracleTableMetadataOrganizer.Organize(context);

            Assert.IsNotNull(names);
            Assert.AreEqual(0, names.Count);
        }

        [TestMethod]
        public void Q05_ValidEmptyTableAndViewNameQueryIsAllowed()
        {
            DataTable table = null;

            OracleQueryEditorMetadataLoader.LoadTableAndViewNames(ref table, sql => new DataTable(), "JQ98XX");

            Assert.IsNotNull(table);
            Assert.AreEqual(0, table.Rows.Count);
        }

        [TestMethod]
        public void Q06_NullTableAndViewNameDelegateIsRejected()
        {
            DataTable table = null;

            AssertRejectedWithoutNullReference
            (
                () => OracleQueryEditorMetadataLoader.LoadTableAndViewNames(ref table, null, "JQ98XX")
            );
        }

        [TestMethod]
        public void Q07_NullTableMetadataContextIsRejected()
        {
            AssertRejectedWithoutNullReference(() => OracleTableMetadataOrganizer.Organize(null));
        }

        [TestMethod]
        public void Q08_NullTableMetadataTargetIsRejected()
        {
            var context = CreateContext(sql => new DataTable());

            context.TargetSchemaTable = null;

            AssertRejectedWithoutNullReference(() => OracleTableMetadataOrganizer.Organize(context));
        }

        [TestMethod]
        public void Q09_NullTableMetadataQueryDelegateIsRejected()
        {
            var context = CreateContext(null);

            AssertRejectedWithoutNullReference(() => OracleTableMetadataOrganizer.Organize(context));
        }

        [TestMethod]
        public void Q10_NullSchemaMetadataLoadContextIsRejected()
        {
            AssertRejectedWithoutNullReference
            (
                () => OracleQueryEditorMetadataLoader.LoadSchemaMetadata(null, new OracleQueryEditorMetadataLoadOptions())
            );
        }
    }
}
