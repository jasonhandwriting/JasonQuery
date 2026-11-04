using JasonLibrary.Core;
using JasonQuery.Core.Database.Metadata.Oracle;
using JasonQuery.Core.Database.Metadata.Oracle.Function;
using JasonQuery.Core.Database.Metadata.Oracle.Index;
using JasonQuery.Core.Database.Metadata.Oracle.Package;
using JasonQuery.Core.Database.Metadata.Oracle.Procedure;
using JasonQuery.Core.Database.Metadata.Oracle.QueryEditor;
using JasonQuery.Core.Database.Metadata.Oracle.Trigger;
using JasonQuery.Core.Database.Metadata.Oracle.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.Core.Database.Metadata.Oracle
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory("F7L2B2C_COMPLETE")]
    public sealed class OracleMetadataOrganizerCompletenessGoldenTests
    {
        private static OracleMetadataContext Context(Func<string, DataTable> query, bool needRows = false)
        {
            var table = new DataTable();

            table.Columns.Add("SchemaObject", typeof(string));
            table.Columns.Add("SchemaType", typeof(string));
            table.Columns.Add("SchemaName", typeof(string));
            table.Columns.Add("ColumnInfo", typeof(string));
            table.Columns.Add("OracleMetadataOwner", typeof(string));

            return new OracleMetadataContext
            {
                ExecuteQuery = query,
                TargetSchemaTable = table,
                DbConnectionName = "TEST",
                DbUserNameUppercase = "JQ98XX",
                NeedSchemaRows = needRows,
                SortByColumnName = true
            };
        }

        private static void MustRejectNull(Action work)
        {
            Exception caught = null;

            try
            {
                work();
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught, "A null query result is a failure, not an empty Oracle dictionary result.");
            Assert.IsFalse(caught is NullReferenceException, "Reject null explicitly, not via a null dereference.");
        }

        private static void AssertNoPartialKeywordPublication(int failingQueryIndex, bool needRows = false)
        {
            var functions = MyLibrary.KeywordsUserDefinedFunctions;
            var tables = MyLibrary.KeywordsUserDefinedTables;
            var triggers = MyLibrary.KeywordsUserDefinedTriggers;
            var views = MyLibrary.KeywordsUserDefinedViews;
            var queryCount = 0;
            Exception caught = null;
            bool keywordsUnchanged = false;

            try
            {
                var context = Context(sql =>
                {
                    queryCount++;
                    return queryCount == failingQueryIndex ? null : new DataTable();
                }, needRows);

                OracleQueryEditorMetadataLoader.LoadSchemaMetadata
                (
                    context,
                    new OracleQueryEditorMetadataLoadOptions()
                );
            }
            catch (Exception ex)
            {
                caught = ex;
            }
            finally
            {
                keywordsUnchanged = string.Equals(MyLibrary.KeywordsUserDefinedFunctions, functions, StringComparison.Ordinal)
                                    && string.Equals(MyLibrary.KeywordsUserDefinedTables, tables, StringComparison.Ordinal)
                                    && string.Equals(MyLibrary.KeywordsUserDefinedTriggers, triggers, StringComparison.Ordinal)
                                    && string.Equals(MyLibrary.KeywordsUserDefinedViews, views, StringComparison.Ordinal);

                //These tests use static config; restore it even when Golden Red intentionally fails.
                MyLibrary.KeywordsUserDefinedFunctions = functions;
                MyLibrary.KeywordsUserDefinedTables = tables;
                MyLibrary.KeywordsUserDefinedTriggers = triggers;
                MyLibrary.KeywordsUserDefinedViews = views;
            }

            Assert.IsTrue(queryCount >= failingQueryIndex, "The injected failure must reach its target query.");
            Assert.IsNotNull(caught, "A failed Organizer must fail the entire Schema load.");
            Assert.IsFalse(caught is NullReferenceException, "Reject null explicitly rather than crashing.");
            Assert.IsTrue(keywordsUnchanged, "Failed Schema load must not publish partial Keywords.");
        }

        [TestMethod]
        public void M01_FunctionQueryReturningNullMustFail()
        {
            MustRejectNull(() => OracleFunctionMetadataOrganizer.Organize(Context(sql => null)));
        }

        [TestMethod]
        public void M02_TriggerQueryReturningNullMustFail()
        {
            MustRejectNull(() => OracleTriggerMetadataOrganizer.Organize(Context(sql => null)));
        }

        [TestMethod]
        public void M03_ViewInfoQueryReturningNullMustFail()
        {
            MustRejectNull(() => OracleViewMetadataOrganizer.Organize(Context(sql => null)));
        }

        [TestMethod]
        public void M04_ViewColumnQueryReturningNullMustFail()
        {
            var queryCount = 0;
            var context = Context(sql => ++queryCount == 1 ? new DataTable() : null, true);

            MustRejectNull(() => OracleViewMetadataOrganizer.Organize(context));

            Assert.AreEqual(2, queryCount);
        }

        [TestMethod]
        public void M05_ProcedureQueryReturningNullMustFail()
        {
            MustRejectNull(() => OracleProcedureMetadataOrganizer.Organize(Context(sql => null, true)));
        }

        [TestMethod]
        public void M06_PackageQueryReturningNullMustFail()
        {
            MustRejectNull(() => OraclePackageMetadataOrganizer.Organize(Context(sql => null, true)));
        }

        [TestMethod]
        public void M07_IndexQueryReturningNullMustFail()
        {
            MustRejectNull(() => OracleIndexMetadataOrganizer.Organize(Context(sql => null, true)));
        }

        [TestMethod]
        public void M08_EarlyNullMustFailWholeLoadBeforeKeywordPublication()
        {
            AssertNoPartialKeywordPublication(1);
        }

        [TestMethod]
        public void M09_LateNullMustFailWholeLoadBeforeKeywordPublication()
        {
            //With NeedSchemaRows=true, Index is query 9 after all preceding metadata queries.
            //Verify that a failure at the final Organizer rejects the entire load.
            AssertNoPartialKeywordPublication(9, true);
        }

        [TestMethod]
        public void M10_EmptyFunctionResultIsValid()
        {
            Assert.AreEqual(0, OracleFunctionMetadataOrganizer.Organize(Context(sql => new DataTable())).Count);
        }

        [TestMethod]
        public void M11_EmptyTriggerResultIsValid()
        {
            Assert.AreEqual(0, OracleTriggerMetadataOrganizer.Organize(Context(sql => new DataTable())).Count);
        }

        [TestMethod]
        public void M12_EmptyViewResultIsValid()
        {
            Assert.AreEqual(0, OracleViewMetadataOrganizer.Organize(Context(sql => new DataTable())).Count);
        }

        [TestMethod]
        public void M13_EmptyProcedureResultIsValid()
        {
            OracleProcedureMetadataOrganizer.Organize(Context(sql => new DataTable()));
        }

        [TestMethod]
        public void M14_EmptyPackageResultIsValid()
        {
            OraclePackageMetadataOrganizer.Organize(Context(sql => new DataTable()));
        }

        [TestMethod]
        public void M15_EmptyIndexResultIsValid()
        {
            OracleIndexMetadataOrganizer.Organize(Context(sql => new DataTable()));
        }

        [TestMethod]
        public void M16_EmptyViewWithEmptyColumnResultIsValid()
        {
            var count = 0;

            var result = OracleViewMetadataOrganizer.Organize(Context(sql =>
            {
                count++;
                return new DataTable();
            }, true));

            Assert.AreEqual(2, count);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void M17_ThrownQueryFailureIsNotSwallowed()
        {
            MustRejectNull(() => OracleTriggerMetadataOrganizer.Organize(Context(sql =>
            {
                throw new InvalidOperationException("Injected Oracle query failure");
            })));
        }
    }
}
