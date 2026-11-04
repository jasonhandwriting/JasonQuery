using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Metadata.MySql;
using JasonQuery.Core.Database.Metadata.MySql.QueryEditor;
using JasonQuery.Core.Database.Metadata.PostgreSql.QueryEditor;
using JasonQuery.Core.Database.Metadata.SqlServer;
using JasonQuery.Core.Database.Metadata.SqlServer.QueryEditor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.Core.Database.Metadata
{
    //S1-A: negative-injection contract for metadata query failure propagation.
    //A valid empty DataTable is NOT an error; a null DataTable is an error.
    //Each provider must reject null at the failing query, before later queries run.
    [TestClass]
    [TestCategory("L2S1A")]
    public sealed class ThreeProviderMetadataFailurePropagationGoldenTests
    {
        private const string NullQueryError = "Metadata query returned null";

        private string _functions;
        private string _tables;
        private string _triggers;
        private string _views;
        private DataTable _autoComplete;

        [TestInitialize]
        public void PreserveSharedKeywordState()
        {
            _functions = MyLibrary.KeywordsUserDefinedFunctions;
            _tables = MyLibrary.KeywordsUserDefinedTables;
            _triggers = MyLibrary.KeywordsUserDefinedTriggers;
            _views = MyLibrary.KeywordsUserDefinedViews;
            _autoComplete = MyGlobal.dtAutoCompleteForAll;
        }

        [TestCleanup]
        public void RestoreSharedKeywordState()
        {
            MyLibrary.KeywordsUserDefinedFunctions = _functions;
            MyLibrary.KeywordsUserDefinedTables = _tables;
            MyLibrary.KeywordsUserDefinedTriggers = _triggers;
            MyLibrary.KeywordsUserDefinedViews = _views;
            MyGlobal.dtAutoCompleteForAll = _autoComplete;
        }

        private static DataTable NewEmptySchema()
        {
            var table = new DataTable();

            table.Columns.Add("SchemaObject");
            table.Columns.Add("SchemaNode");
            table.Columns.Add("SchemaType");
            table.Columns.Add("SchemaName");

            return table;
        }

        private static PostgreSqlQueryEditorMetadataLoadOptions NewPostgreSqlOptions()
        {
            return new PostgreSqlQueryEditorMetadataLoadOptions
            {
                TargetSchemaTable = NewEmptySchema(),
                DbConnectionName = "L2S1A_FakeDatabase"
            };
        }

        private static SqlServerMetadataContext NewSqlServerContext()
        {
            return new SqlServerMetadataContext
            {
                connectionName = "L2S1A_FakeConnection",
                databaseName = "L2S1A_FakeDatabase",
                dtSchema = NewEmptySchema()
            };
        }

        private static SqlServerQueryEditorMetadataLoadOptions NewSqlServerOptions()
        {
            return new SqlServerQueryEditorMetadataLoadOptions
            {
                sortByColumnName = "Ordinal_Position;"
            };
        }

        private static MySqlMetadataContext NewMySqlContext(Func<string, DataTable> query)
        {
            return new MySqlMetadataContext
            {
                ConnectionName = "L2S1A_FakeConnection",
                DatabaseName = "L2S1A_FakeDatabase",
                SchemaTable = NewEmptySchema(),
                ExecuteQuery = query
            };
        }

        private static void AssertExplicitNullFailure(Action load)
        {
            var error = Assert.ThrowsExactly<InvalidOperationException>(load);

            Assert.Contains(NullQueryError, error.Message);
        }

        [TestMethod]
        public void S1A01_PostgreSql_NullNameQueryMustFailWithoutReplacingCache()
        {
            var previous = NewEmptySchema();
            var original = previous;

            AssertExplicitNullFailure(() => PostgreSqlQueryEditorMetadataLoader.LoadTableAndViewNames(ref previous, sql => null));

            Assert.AreSame(original, previous, "Do not replace the current cached names on failure.");
        }

        [TestMethod]
        public void S1A02_PostgreSql_TimeoutIsPropagated()
        {
            var previous = NewEmptySchema();
            var original = previous;

            Assert.ThrowsExactly<TimeoutException>(() =>
            {
                PostgreSqlQueryEditorMetadataLoader.LoadTableAndViewNames(ref previous, sql =>
                {
                    throw new TimeoutException("Injected PostgreSQL metadata timeout");
                });
            });

            Assert.AreSame(original, previous);
        }

        [TestMethod]
        public void S1A03_PostgreSql_NullSecondSchemaQueryStopsImmediately()
        {
            var calls = 0;

            AssertExplicitNullFailure(() => PostgreSqlQueryEditorMetadataLoader.LoadSchemaMetadata(NewPostgreSqlOptions(), sql =>
            {
                calls++;
                return calls == 2 ? null : new DataTable();
            }));

            Assert.AreEqual(2, calls, "Stop at the failed PostgreSQL metadata query.");
        }

        [TestMethod]
        public void S1A04_SqlServer_NullFirstQueryMustFail()
        {
            var calls = 0;

            AssertExplicitNullFailure(() => new SqlServerQueryEditorMetadataLoader().Load(NewSqlServerContext(), NewSqlServerOptions(), sql =>
            {
                calls++;
                return null;
            }));

            Assert.AreEqual(1, calls, "Stop before the next SQL Server metadata query.");
        }

        [TestMethod]
        public void S1A05_SqlServer_TimeoutIsPropagated()
        {
            Assert.ThrowsExactly<TimeoutException>(() =>
            {
                new SqlServerQueryEditorMetadataLoader().Load(NewSqlServerContext(), NewSqlServerOptions(), sql =>
                {
                    throw new TimeoutException("Injected SQL Server metadata timeout");
                });
            });
        }

        [TestMethod]
        public void S1A06_SqlServer_NullSecondQueryStopsImmediately()
        {
            var calls = 0;

            AssertExplicitNullFailure(() => new SqlServerQueryEditorMetadataLoader().Load(NewSqlServerContext(), NewSqlServerOptions(), sql =>
            {
                calls++;
                return calls == 2 ? null : new DataTable();
            }));

            Assert.AreEqual(2, calls, "Stop at the failed SQL Server metadata query.");
        }

        [TestMethod]
        public void S1A07_MySql_NullFirstQueryMustFail()
        {
            var calls = 0;

            var context = NewMySqlContext(sql =>
            {
                calls++;
                return null;
            });

            AssertExplicitNullFailure(() => MySqlQueryEditorMetadataLoader.LoadSchemaMetadata(context, new MySqlQueryEditorMetadataLoadOptions()));

            Assert.AreEqual(1, calls, "Stop before the next MySQL metadata query.");
            Assert.IsNull(context.ResultSchemaTable, "Do not publish an incomplete MySQL schema.");
        }

        [TestMethod]
        public void S1A08_MySql_TimeoutIsPropagated()
        {
            var context = NewMySqlContext(sql =>
            {
                throw new TimeoutException("Injected MySQL metadata timeout");
            });

            Assert.ThrowsExactly<TimeoutException>(() =>
            {
                MySqlQueryEditorMetadataLoader.LoadSchemaMetadata(context, new MySqlQueryEditorMetadataLoadOptions());
            });

            Assert.IsNull(context.ResultSchemaTable);
        }

        [TestMethod]
        public void S1A09_MySql_NullSecondQueryStopsImmediately()
        {
            var calls = 0;

            var context = NewMySqlContext(sql =>
            {
                calls++;
                return calls == 2 ? null : new DataTable();
            });

            AssertExplicitNullFailure(() => MySqlQueryEditorMetadataLoader.LoadSchemaMetadata(context, new MySqlQueryEditorMetadataLoadOptions()));

            Assert.AreEqual(2, calls, "Stop at the failed MySQL metadata query.");
            Assert.IsNull(context.ResultSchemaTable);
        }
    }
}
