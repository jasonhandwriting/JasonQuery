using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata.Oracle;
using JasonQuery.Core.Database.Metadata.Oracle.Function;
using JasonQuery.Core.Database.Metadata.Oracle.QueryEditor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.Metadata.Oracle
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory("F7L2B2C_ATOMIC")]
    public sealed class OracleAtomicMetadataPublicationGoldenTests
    {
        private const BindingFlags AtomicFlags = BindingFlags.Static | BindingFlags.NonPublic;

        private sealed class GlobalState
        {
            public DataTable Schema;
            public DataTable TableAndViews;
            public DataTable AutoComplete;
            public string Functions;
            public string Tables;
            public string Triggers;
            public string Views;
        }

        private static GlobalState Capture()
        {
            return new GlobalState
            {
                Schema = DatabaseSqlExecutor.dtSchema,
                TableAndViews = DatabaseSqlExecutor.dtTableAndViews,
                AutoComplete = MyGlobal.dtAutoCompleteForAll,
                Functions = MyLibrary.KeywordsUserDefinedFunctions,
                Tables = MyLibrary.KeywordsUserDefinedTables,
                Triggers = MyLibrary.KeywordsUserDefinedTriggers,
                Views = MyLibrary.KeywordsUserDefinedViews
            };
        }

        private static void Restore(GlobalState state)
        {
            DatabaseSqlExecutor.dtSchema = state.Schema;
            DatabaseSqlExecutor.dtTableAndViews = state.TableAndViews;
            MyGlobal.dtAutoCompleteForAll = state.AutoComplete;
            MyLibrary.KeywordsUserDefinedFunctions = state.Functions;
            MyLibrary.KeywordsUserDefinedTables = state.Tables;
            MyLibrary.KeywordsUserDefinedTriggers = state.Triggers;
            MyLibrary.KeywordsUserDefinedViews = state.Views;
        }

        private static DataTable Marker(string value)
        {
            var dt = new DataTable();

            dt.Columns.Add("Marker", typeof(string));
            dt.Rows.Add(value);

            return dt;
        }

        private static void SetOldBatch()
        {
            DatabaseSqlExecutor.dtSchema = Marker("SCHEMA_OLD");
            DatabaseSqlExecutor.dtTableAndViews = Marker("TABLES_OLD");
            MyGlobal.dtAutoCompleteForAll = Marker("AUTOCOMPLETE_OLD");
            MyLibrary.KeywordsUserDefinedFunctions = "FUNCTIONS_OLD";
            MyLibrary.KeywordsUserDefinedTables = "TABLES_OLD";
            MyLibrary.KeywordsUserDefinedTriggers = "TRIGGERS_OLD";
            MyLibrary.KeywordsUserDefinedViews = "VIEWS_OLD";
        }

        private static void AssertSameBatch(GlobalState expected)
        {
            Assert.AreSame(expected.Schema, DatabaseSqlExecutor.dtSchema, "Schema was published prematurely.");
            Assert.AreSame(expected.TableAndViews, DatabaseSqlExecutor.dtTableAndViews, "Table/View cache was published prematurely.");
            Assert.AreSame(expected.AutoComplete, MyGlobal.dtAutoCompleteForAll, "AutoComplete was published prematurely.");
            Assert.AreEqual(expected.Functions, MyLibrary.KeywordsUserDefinedFunctions);
            Assert.AreEqual(expected.Tables, MyLibrary.KeywordsUserDefinedTables);
            Assert.AreEqual(expected.Triggers, MyLibrary.KeywordsUserDefinedTriggers);
            Assert.AreEqual(expected.Views, MyLibrary.KeywordsUserDefinedViews);
        }

        private static void WithOldBatch(Action<GlobalState> action)
        {
            var original = Capture();

            try
            {
                SetOldBatch();
                action(Capture());
            }
            finally
            {
                Restore(original);
            }
        }

        //Production entry point contract: UpdateSchemaData_Oracle must use this same non-UI core. Its query delegate lets Golden Tests run without Oracle or a Grid.
        private static MethodInfo RequireAtomicCore()
        {
            var method = typeof(DatabaseSqlExecutor).GetMethod
            (
                "RunOracleMetadataAtomicLoad",
                AtomicFlags,
                null,
                new[] { typeof(Func<string, DataTable>), typeof(bool) },
                null
            );

            Assert.IsNotNull(method, "B2c-1b requires the real production Oracle loader to retain its injectable query signature.");

            var receiptType = typeof(DatabaseSqlExecutor).Assembly.GetType
            (
                "JasonQuery.UI.LeftPanel.SharedOracleMetadataCompletedLoadReceipt",
                false
            );

            Assert.IsNotNull(receiptType, "The production Oracle loader must return an internal completed-load receipt.");
            Assert.AreEqual(receiptType, method.ReturnType);

            //AP04-AP07 use the same candidate preparation/publication core, but not a forged live Oracle scope.
            var testSeam = typeof(DatabaseSqlExecutor).GetMethod
            (
                "RunOracleMetadataAtomicLoadUnverifiedForTest",
                AtomicFlags,
                null,
                new[] { typeof(Func<string, DataTable>), typeof(bool) },
                null
            );

            Assert.IsNotNull(testSeam);
            Assert.IsTrue(testSeam.IsPrivate);
            Assert.AreEqual(typeof(void), testSeam.ReturnType);

            return testSeam;
        }

        private static void RunAtomic(MethodInfo method, Func<string, DataTable> query)
        {
            try
            {
                method.Invoke(null, new object[] { query, false });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        private static DataTable FakeResult(string batchName)
        {
            var dt = new DataTable();

            dt.Columns.Add("TableName", typeof(string));
            dt.Columns.Add("FunctionName", typeof(string));
            dt.Columns.Add("TriggerName", typeof(string));
            dt.Columns.Add("ViewName", typeof(string));
            dt.Columns.Add("ColumnName", typeof(string));
            dt.Columns.Add("OracleMetadataOwner", typeof(string));
            dt.Columns.Add("DataType", typeof(string));
            dt.Columns.Add("DataLength", typeof(string));
            dt.Columns.Add("Scale", typeof(string));
            dt.Columns.Add("ColumnType", typeof(string));
            dt.Rows.Add(batchName + "_TABLE", batchName + "_FUNCTION", batchName + "_TRIGGER", batchName + "_VIEW", "C1", "JQ98XX", "VARCHAR2", "50", "", "VARCHAR2");

            return dt;
        }

        private static void AssertFailurePreservesOldBatch(MethodInfo method, int failAt, bool throwError)
        {
            WithOldBatch(old =>
            {
                var count = 0;

                Exception caught = null;

                try
                {
                    RunAtomic(method, sql =>
                    {
                        count++;

                        if (count == failAt)
                        {
                            if (throwError)
                            {
                                throw new InvalidOperationException("Injected Oracle metadata failure");
                            }
                            return null;
                        }

                        return FakeResult("CANDIDATE");
                    });
                }
                catch (Exception ex)
                {
                    caught = ex;
                }

                Assert.IsTrue(count >= failAt, "Injected failure path was not exercised.");
                Assert.IsNotNull(caught, "Failed Oracle metadata query must abort publication.");
                Assert.IsFalse(caught is NullReferenceException, "Null query results require explicit rejection.");

                AssertSameBatch(old);
            });
        }

        [TestMethod]
        public void AP01_BaselineFailedTableViewQueryDoesNotReplaceCache()
        {
            DataTable existing = Marker("OLD");

            var before = existing;

            try
            {
                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => OracleQueryEditorMetadataLoader.LoadTableAndViewNames(ref existing, sql => null, "JQ98XX")
                );

                Assert.AreSame(before, existing);
            }
            finally
            {
                existing?.Dispose();
            }
        }

        [TestMethod]
        public void AP02_BaselineFunctionNullQueryIsRejected()
        {
            var context = new OracleMetadataContext
            {
                TargetSchemaTable = new DataTable(),
                DbConnectionName = "TEST",
                DbUserNameUppercase = "JQ98XX",
                NeedSchemaRows = false,
                ExecuteQuery = sql => null
            };

            Assert.ThrowsExactly<InvalidOperationException>(() => OracleFunctionMetadataOrganizer.Organize(context));
        }

        [TestMethod]
        public void AP03_ProductionAtomicCoreHasInjectableQueryEntry()
        {
            RequireAtomicCore();
        }

        [TestMethod]
        public void AP04_EarlyQueryFailurePreservesEveryPublishedCache()
        {
            AssertFailurePreservesOldBatch(RequireAtomicCore(), 1, false);
        }

        [TestMethod]
        public void AP05_TableMetadataFailurePreservesEveryPublishedCache()
        {
            AssertFailurePreservesOldBatch(RequireAtomicCore(), 3, false);
        }

        [TestMethod]
        public void AP06_LateMetadataFailurePreservesEveryPublishedCache()
        {
            AssertFailurePreservesOldBatch(RequireAtomicCore(), 5, false);
        }

        [TestMethod]
        public void AP07_SuccessfulBatchPublishesAllCachesAndKeywords()
        {
            var method = RequireAtomicCore();

            WithOldBatch(old =>
            {
                var count = 0;

                RunAtomic(method, sql =>
                {
                    count++;
                    return FakeResult("NEW_BATCH");
                });

                Assert.IsTrue(count >= 5);
                Assert.IsNotNull(DatabaseSqlExecutor.dtSchema);
                Assert.IsNotNull(DatabaseSqlExecutor.dtTableAndViews);
                Assert.IsNotNull(MyGlobal.dtAutoCompleteForAll);
                Assert.AreNotSame(old.Schema, DatabaseSqlExecutor.dtSchema);
                Assert.AreNotSame(old.TableAndViews, DatabaseSqlExecutor.dtTableAndViews);
                Assert.AreNotSame(old.AutoComplete, MyGlobal.dtAutoCompleteForAll);
                Assert.IsTrue((MyLibrary.KeywordsUserDefinedFunctions ?? "").Contains("NEW_BATCH_FUNCTION"));
                Assert.IsTrue((MyLibrary.KeywordsUserDefinedTables ?? "").Contains("NEW_BATCH_TABLE"));
                Assert.IsTrue((MyLibrary.KeywordsUserDefinedTriggers ?? "").Contains("NEW_BATCH_TRIGGER"));
                Assert.IsTrue((MyLibrary.KeywordsUserDefinedViews ?? "").Contains("NEW_BATCH_VIEW"));
            });
        }

        [TestMethod]
        public void AP08_SuccessfulEmptySchemaIsNotMistakenForFailure()
        {
            var method = RequireAtomicCore();

            WithOldBatch(old =>
            {
                var count = 0;

                RunAtomic(method, sql =>
                {
                    count++;
                    return new DataTable();
                });

                Assert.IsTrue(count >= 5);
                Assert.IsNotNull(DatabaseSqlExecutor.dtSchema);
                Assert.IsNotNull(DatabaseSqlExecutor.dtTableAndViews);
                Assert.AreNotSame(old.Schema, DatabaseSqlExecutor.dtSchema);
                Assert.AreNotSame(old.TableAndViews, DatabaseSqlExecutor.dtTableAndViews);
            });
        }

        [TestMethod]
        public void AP09_ThrownQueryExceptionNeverPublishesPartialBatch()
        {
            AssertFailurePreservesOldBatch(RequireAtomicCore(), 4, true);
        }

        [TestMethod]
        public void AP10_FailedReloadAfterSuccessPreservesSuccessfulBatch()
        {
            var method = RequireAtomicCore();

            WithOldBatch(old =>
            {
                RunAtomic(method, sql => FakeResult("GOOD_BATCH"));

                var good = Capture();
                Exception caught = null;
                var calls = 0;

                try
                {
                    RunAtomic(method, sql =>
                    {
                        calls++;
                        return calls == 5 ? null : FakeResult("BAD_BATCH");
                    });
                }
                catch (Exception ex)
                {
                    caught = ex;
                }

                Assert.IsTrue(calls >= 5);
                Assert.IsNotNull(caught);

                AssertSameBatch(good);
            });
        }
    }
}
