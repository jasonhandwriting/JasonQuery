using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata.MySql;
using JasonQuery.Core.Database.Metadata.MySql.QueryEditor;
using JasonQuery.Core.Database.Metadata.PostgreSql.QueryEditor;
using JasonQuery.Core.Database.Metadata.SqlServer;
using JasonQuery.Core.Database.Metadata.SqlServer.QueryEditor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.Metadata
{
    //S1B2: require an actual production-wired metadata atomic core, not a test-only shim.
    //PostgreSQL = 1 names + 4 schema queries; SQL Server = 9 schema + 2 lookup; MySQL = 7 schema + 2 lookup. All query results here are intentionally empty but valid.
    [TestClass]
    [DoNotParallelize]
    [TestCategory("L2S1B2")]
    public sealed class ThreeProviderMetadataAtomicPublicationGoldenTests
    {
        private const BindingFlags CoreFlags = BindingFlags.Static | BindingFlags.NonPublic;

        private sealed class Batch
        {
            public DataTable Schema;
            public DataTable Names;
            public DataTable DatabaseNames;
            public DataTable AutoComplete;
            public string Functions;
            public string Tables;
            public string Triggers;
            public string Views;
            public DataSourceType SourceType;
            public string DatabaseName;
            public string ConnectionName;
        }

        private static Batch Capture()
        {
            return new Batch
            {
                Schema = DatabaseSqlExecutor.dtSchema,
                Names = DatabaseSqlExecutor.dtTableAndViews,
                DatabaseNames = DatabaseSqlExecutor.dtDatabaseName,
                AutoComplete = MyGlobal.dtAutoCompleteForAll,
                Functions = MyLibrary.KeywordsUserDefinedFunctions,
                Tables = MyLibrary.KeywordsUserDefinedTables,
                Triggers = MyLibrary.KeywordsUserDefinedTriggers,
                Views = MyLibrary.KeywordsUserDefinedViews,
                SourceType = DatabaseSqlExecutor.CurrentDataSource,
                DatabaseName = DatabaseSqlExecutor.DatabaseName,
                ConnectionName = DatabaseSqlExecutor.DbConnectionName
            };
        }

        private static void Restore(Batch state)
        {
            DatabaseSqlExecutor.dtSchema = state.Schema;
            DatabaseSqlExecutor.dtTableAndViews = state.Names;
            DatabaseSqlExecutor.dtDatabaseName = state.DatabaseNames;
            MyGlobal.dtAutoCompleteForAll = state.AutoComplete;
            MyLibrary.KeywordsUserDefinedFunctions = state.Functions;
            MyLibrary.KeywordsUserDefinedTables = state.Tables;
            MyLibrary.KeywordsUserDefinedTriggers = state.Triggers;
            MyLibrary.KeywordsUserDefinedViews = state.Views;
            DatabaseSqlExecutor.CurrentDataSource = state.SourceType;
            DatabaseSqlExecutor.DatabaseName = state.DatabaseName;
            DatabaseSqlExecutor.DbConnectionName = state.ConnectionName;
        }

        private static DataTable Marker(string label)
        {
            var table = new DataTable();

            table.Columns.Add("Marker");
            table.Rows.Add(label);

            return table;
        }

        private static void WithOldBatch(DataSourceType source, Action<Batch> action)
        {
            var original = Capture();

            try
            {
                DatabaseSqlExecutor.CurrentDataSource = source;
                DatabaseSqlExecutor.DatabaseName = "S1B2_TestDatabase";
                DatabaseSqlExecutor.DbConnectionName = "S1B2_TestConnection";
                DatabaseSqlExecutor.dtSchema = Marker("SCHEMA_OLD");
                DatabaseSqlExecutor.dtTableAndViews = Marker("NAMES_OLD");
                DatabaseSqlExecutor.dtDatabaseName = Marker("DBNAMES_OLD");
                MyGlobal.dtAutoCompleteForAll = Marker("AUTOCOMPLETE_OLD");
                MyLibrary.KeywordsUserDefinedFunctions = "FUNCTIONS_OLD";
                MyLibrary.KeywordsUserDefinedTables = "TABLES_OLD";
                MyLibrary.KeywordsUserDefinedTriggers = "TRIGGERS_OLD";
                MyLibrary.KeywordsUserDefinedViews = "VIEWS_OLD";

                action(Capture());
            }
            finally
            {
                Restore(original);
            }
        }

        private static void AssertOldBatch(Batch previous)
        {
            Assert.AreSame(previous.Schema, DatabaseSqlExecutor.dtSchema, "Schema changed before complete success.");
            Assert.AreSame(previous.Names, DatabaseSqlExecutor.dtTableAndViews, "Table/View names changed before complete success.");
            Assert.AreSame(previous.DatabaseNames, DatabaseSqlExecutor.dtDatabaseName, "Database names changed before complete success.");
            Assert.AreSame(previous.AutoComplete, MyGlobal.dtAutoCompleteForAll, "AutoComplete changed before complete success.");
            Assert.AreEqual(previous.Functions, MyLibrary.KeywordsUserDefinedFunctions);
            Assert.AreEqual(previous.Tables, MyLibrary.KeywordsUserDefinedTables);
            Assert.AreEqual(previous.Triggers, MyLibrary.KeywordsUserDefinedTriggers);
            Assert.AreEqual(previous.Views, MyLibrary.KeywordsUserDefinedViews);
        }

        private static MethodInfo RequireAtomicCore()
        {
            var method = typeof(DatabaseSqlExecutor).GetMethod
            (
                "RunThreeProviderMetadataAtomicLoad",
                CoreFlags,
                null,
                new[] { typeof(DataSourceType), typeof(Func<string, DataTable>), typeof(bool) },
                null
            );

            Assert.IsNotNull(method, "Missing real S1B2 three-provider atomic core with injected query delegate.");
            Assert.AreEqual(typeof(void), method.ReturnType);

            return method;
        }

        private static void RunAtomic(DataSourceType source, Func<string, DataTable> query)
        {
            try
            {
                RequireAtomicCore().Invoke(null, new object[] { source, query, false });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        private static bool CallsMethod(MethodInfo caller, MethodInfo callee)
        {
            Assert.IsNotNull(caller);

            var body = caller.GetMethodBody();

            Assert.IsNotNull(body);

            var il = body.GetILAsByteArray();

            Assert.IsNotNull(il);

            for (var i = 0; i + 4 < il.Length; i++)
            {
                if ((il[i] == 0x28 || il[i] == 0x6F) && BitConverter.ToInt32(il, i + 1) == callee.MetadataToken)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AssertProductionRoute(string methodName)
        {
            var entry = typeof(DatabaseSqlExecutor).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);

            Assert.IsNotNull(entry);
            Assert.IsTrue(CallsMethod(entry, RequireAtomicCore()), methodName + " must call the same atomic core as the Golden tests.");
        }

        private static void AssertLateFailure(DataSourceType source, int failAt, bool timeout)
        {
            WithOldBatch(source, old =>
            {
                var calls = 0;
                Exception error = null;

                try
                {
                    RunAtomic(source, sql =>
                    {
                        calls++;

                        if (calls == failAt)
                        {
                            if (timeout)
                            {
                                throw new TimeoutException("Injected S1B2 late metadata timeout");
                            }

                            return null;
                        }

                        return new DataTable();
                    });
                }
                catch (Exception ex)
                {
                    error = ex;
                }

                Assert.AreEqual(failAt, calls, "Test must reach the selected late metadata SQL query.");
                Assert.IsNotNull(error, "Failure must abort publication.");

                if (timeout)
                {
                    Assert.IsInstanceOfType(error, typeof(TimeoutException));
                }
                else
                {
                    Assert.IsInstanceOfType(error, typeof(InvalidOperationException));
                    Assert.Contains("Metadata query returned null", error.Message);
                }

                AssertOldBatch(old);
            });
        }

        private static void AssertEmptySuccess(DataSourceType source, int expectedCalls, bool hasDatabaseLookup)
        {
            WithOldBatch(source, old =>
            {
                var calls = 0;

                RunAtomic(source, sql =>
                {
                    calls++;
                    return new DataTable();
                });

                Assert.IsTrue(calls >= expectedCalls, "The complete metadata and lookup batch must run.");
                Assert.IsNotNull(DatabaseSqlExecutor.dtSchema);
                Assert.IsNotNull(DatabaseSqlExecutor.dtTableAndViews);
                Assert.IsNotNull(MyGlobal.dtAutoCompleteForAll);
                Assert.AreNotSame(old.Schema, DatabaseSqlExecutor.dtSchema);
                Assert.AreNotSame(old.Names, DatabaseSqlExecutor.dtTableAndViews);
                Assert.AreNotSame(old.AutoComplete, MyGlobal.dtAutoCompleteForAll);

                if (hasDatabaseLookup)
                {
                    Assert.IsNotNull(DatabaseSqlExecutor.dtDatabaseName);
                    Assert.AreNotSame(old.DatabaseNames, DatabaseSqlExecutor.dtDatabaseName);
                }
            });
        }

        [TestMethod]
        public void S1B201_PostgreSql_NullGuardBaseline()
        {
            DataTable names = new DataTable();
            var previous = names;

            Assert.ThrowsExactly<InvalidOperationException>(() => PostgreSqlQueryEditorMetadataLoader.LoadTableAndViewNames(ref names, sql => null));
            Assert.AreSame(previous, names);
        }

        [TestMethod]
        public void S1B202_SqlServer_NullGuardBaseline()
        {
            var context = new SqlServerMetadataContext { databaseName = "S1B2_DB", dtSchema = new DataTable() };
            var options = new SqlServerQueryEditorMetadataLoadOptions { sortByColumnName = "Ordinal_Position;" };

            Assert.ThrowsExactly<InvalidOperationException>(() => new SqlServerQueryEditorMetadataLoader().Load(context, options, sql => null));
        }

        [TestMethod]
        public void S1B203_MySql_NullGuardBaseline()
        {
            var context = new MySqlMetadataContext
            {
                DatabaseName = "S1B2_DB",
                SchemaTable = new DataTable(),
                ExecuteQuery = sql => null
            };

            Assert.ThrowsExactly<InvalidOperationException>(() => MySqlQueryEditorMetadataLoader.LoadSchemaMetadata(context, new MySqlQueryEditorMetadataLoadOptions()));
            Assert.IsNull(context.ResultSchemaTable);
        }

        [TestMethod]
        public void S1B204_PostgreSql_ProductionEntryMustUseAtomicCore()
        {
            AssertProductionRoute("UpdateSchemaData_PostgreSql");
        }

        [TestMethod]
        public void S1B205_SqlServer_ProductionEntryMustUseAtomicCore()
        {
            AssertProductionRoute("UpdateSchemaData_SqlServer");
        }

        [TestMethod]
        public void S1B206_MySql_ProductionEntryMustUseAtomicCore()
        {
            AssertProductionRoute("UpdateSchemaData_MySql");
        }

        [TestMethod]
        public void S1B207_PostgreSql_LateNullPreservesOldBatch()
        {
            AssertLateFailure(DataSourceType.PostgreSql, 5, false);
        }

        [TestMethod]
        public void S1B208_SqlServer_LookupNullPreservesOldBatch()
        {
            AssertLateFailure(DataSourceType.SqlServer, 11, false);
        }

        [TestMethod]
        public void S1B209_MySql_LookupNullPreservesOldBatch()
        {
            AssertLateFailure(DataSourceType.MySql, 9, false);
        }

        [TestMethod]
        public void S1B210_PostgreSql_LateTimeoutPreservesOldBatch()
        {
            AssertLateFailure(DataSourceType.PostgreSql, 5, true);
        }

        [TestMethod]
        public void S1B211_SqlServer_LookupTimeoutPreservesOldBatch()
        {
            AssertLateFailure(DataSourceType.SqlServer, 11, true);
        }

        [TestMethod]
        public void S1B212_MySql_LookupTimeoutPreservesOldBatch()
        {
            AssertLateFailure(DataSourceType.MySql, 9, true);
        }

        [TestMethod]
        public void S1B213_PostgreSql_ValidEmptyBatchPublishes()
        {
            AssertEmptySuccess(DataSourceType.PostgreSql, 5, false);
        }

        [TestMethod]
        public void S1B214_SqlServer_ValidEmptyBatchPublishes()
        {
            AssertEmptySuccess(DataSourceType.SqlServer, 11, true);
        }

        [TestMethod]
        public void S1B215_MySql_ValidEmptyBatchPublishes()
        {
            AssertEmptySuccess(DataSourceType.MySql, 9, true);
        }
    }
}
