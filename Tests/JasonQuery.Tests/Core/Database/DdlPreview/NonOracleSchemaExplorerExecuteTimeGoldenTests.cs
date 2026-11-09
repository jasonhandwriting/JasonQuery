using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    //F7-U2-C2: RED contracts only. No database connection or DDL is performed.
    //A passing policy test does not replace C3 Reader/GUI integration verification.
    [TestClass]
    [TestCategory("F7U2C2")]
    public sealed class NonOracleSchemaExplorerExecuteTimeGoldenTests
    {
        private const string GateName = "JasonQuery.Core.Database.DdlPreview.NonOracleSchemaExplorerDdlExecuteGate";
        private const string Sql = "ALTER TABLE \"public\".\"CaseTable\" ADD COLUMN \"New\" integer NULL;";
        private static readonly Guid SessionId = Guid.Parse("3eec7dc7-746b-4c90-9195-90c54ec85ca2");

        //This DTO belongs to the tests, not to the production API.
        private sealed class TargetCase
        {
            public DataSourceType Provider;
            public string SelectedDatabase;
            public string SelectedSchema;
            public string SelectedTable;
            public int? SelectedObjectId;
            public string LiveDatabase;
            public string LiveSchema;
            public string LiveTable;
            public int? LiveObjectId;
            public bool QualifiedMetadata = true;
            public bool SameConnection = true;
            public string Preview = Sql;
            public string Rebuilt = Sql;
        }

        private static TargetCase Target(DataSourceType provider)
        {
            var item = new TargetCase
            {
                Provider = provider,
                SelectedDatabase = "TestDb",
                SelectedSchema = "public",
                SelectedTable = "CaseTable",
                LiveDatabase = "TestDb",
                LiveSchema = "public",
                LiveTable = "CaseTable"
            };

            if (provider == DataSourceType.SqlServer)
            {
                item.SelectedSchema = "dbo";
                item.LiveSchema = "dbo";
                item.SelectedTable = "part.name";
                item.LiveTable = "part.name";
                item.SelectedObjectId = 2141;
                item.LiveObjectId = 2141;
            }

            if (provider == DataSourceType.MySql)
            {
                item.SelectedSchema = string.Empty;
                item.LiveSchema = string.Empty;
            }

            return item;
        }

        private static MethodInfo GateMethod(string name, params Type[] parameters)
        {
            var type = typeof(ColumnDdlRequest).Assembly.GetType(GateName, false);

            Assert.IsNotNull(type, "F7-U2-C2 requires a new, provider-independent non-Oracle Execute-Time Gate. The RED baseline must not enable the UI.");

            var method = type.GetMethod
            (
                name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                parameters,
                null
            );

            Assert.IsNotNull(method, string.Format("Missing F7-U2-C2 gate contract: {0}.{1}.", GateName, name));
            Assert.AreEqual(typeof(string), method.ReturnType, "Return empty string on success; a stable diagnostic code on failure.");

            return method;
        }

        private static string Validate(TargetCase item)
        {
            var method = GateMethod
            (
                "ValidateTarget",
                typeof(DataSourceType), typeof(string), typeof(string), typeof(string), typeof(int?),
                typeof(string), typeof(string), typeof(string), typeof(int?),
                typeof(bool), typeof(bool), typeof(string), typeof(string)
            );

            return (string)method.Invoke
            (
                null,
                new object[]
                {
                    item.Provider, item.SelectedDatabase, item.SelectedSchema, item.SelectedTable, item.SelectedObjectId,
                    item.LiveDatabase, item.LiveSchema, item.LiveTable, item.LiveObjectId,
                    item.QualifiedMetadata, item.SameConnection, item.Preview, item.Rebuilt
                }
            );
        }

        private static string TryExecute(DataSourceType provider, bool pendingOrUnknownTransaction, bool mysqlImplicitCommitAcknowledged, Func<string> ensureConnection, Func<string> verifyLiveScopeTargetAndSql, Func<Guid> getCurrentConnectionId, Func<Guid, string> guardedDispatch)
        {
            var method = GateMethod
            (
                "TryExecuteF7",
                typeof(DataSourceType), typeof(bool), typeof(bool),
                typeof(Func<string>), typeof(Func<string>),
                typeof(Func<Guid>), typeof(Func<Guid, string>)
            );

            return (string)method.Invoke
            (
                null,
                new object[]
                {
                    provider, pendingOrUnknownTransaction, mysqlImplicitCommitAcknowledged,
                    ensureConnection, verifyLiveScopeTargetAndSql, getCurrentConnectionId, guardedDispatch
                }
            );
        }

        private static void EqualReason(string expected, TargetCase item)
        {
            Assert.AreEqual(expected, Validate(item));
        }

        [TestMethod]
        public void C201_PostgreSqlExactQualifiedTargetAllowsExecution()
        {
            EqualReason(string.Empty, Target(DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void C202_SqlServerExactDatabaseSchemaTableAndObjectIdAllowExecution()
        {
            EqualReason(string.Empty, Target(DataSourceType.SqlServer));
        }

        [TestMethod]
        public void C203_MySqlExactDatabaseAndTableAllowExecution()
        {
            EqualReason(string.Empty, Target(DataSourceType.MySql));
        }

        [TestMethod]
        public void C204_PostgreSqlDatabaseSwitchRejectsOldSelection()
        {
            var target = Target(DataSourceType.PostgreSql);

            target.LiveDatabase = "OtherDb";

            EqualReason("F7U2C2_DATABASE_MISMATCH", target);
        }

        [TestMethod]
        public void C205_PostgreSqlWrongSchemaRejectsSameTableName()
        {
            var target = Target(DataSourceType.PostgreSql);

            target.LiveSchema = "archive";

            EqualReason("F7U2C2_SCHEMA_MISMATCH", target);
        }

        [TestMethod]
        public void C206_PostgreSqlQuotedTableNameIsCaseSensitive()
        {
            var target = Target(DataSourceType.PostgreSql);

            target.LiveTable = "casetable";

            EqualReason("F7U2C2_TABLE_MISMATCH", target);
        }

        [TestMethod]
        public void C207_SqlServerDatabaseSwitchRejectsSameSchemaAndTable()
        {
            var target = Target(DataSourceType.SqlServer);

            target.LiveDatabase = "OtherDb";

            EqualReason("F7U2C2_DATABASE_MISMATCH", target);
        }

        [TestMethod]
        public void C208_SqlServerReplacedObjectIdRejectsSameName()
        {
            var target = Target(DataSourceType.SqlServer);

            target.LiveObjectId = 2142;

            EqualReason("F7U2C2_OBJECT_ID_MISMATCH", target);
        }

        [TestMethod]
        public void C209_MySqlDatabaseSwitchRejectsSameTableName()
        {
            var target = Target(DataSourceType.MySql);

            target.LiveDatabase = "OtherDb";

            EqualReason("F7U2C2_DATABASE_MISMATCH", target);
        }

        [TestMethod]
        public void C210_MySqlChangedTableRejectsExecution()
        {
            var target = Target(DataSourceType.MySql);

            target.LiveTable = "AnotherTable";

            EqualReason("F7U2C2_TABLE_MISMATCH", target);
        }

        [TestMethod]
        public void C211_UnqualifiedMetadataRejectsEvenMatchingNames()
        {
            var target = Target(DataSourceType.PostgreSql);

            target.QualifiedMetadata = false;

            EqualReason("F7U2C2_METADATA_UNQUALIFIED", target);
        }

        [TestMethod]
        public void C212_ChangedPhysicalConnectionRejectsCachedAuthorization()
        {
            var target = Target(DataSourceType.SqlServer);

            target.SameConnection = false;

            EqualReason("F7U2C2_CONNECTION_CHANGED", target);
        }

        [TestMethod]
        public void C213_ModifiedSqlRejectsByteForByteComparison()
        {
            var target = Target(DataSourceType.PostgreSql);

            target.Preview = Sql + " ";

            EqualReason("F7U2C2_SQL_CHANGED", target);
        }

        [TestMethod]
        public void C214_EmptyOrMissingSqlRejectsExecution()
        {
            var target = Target(DataSourceType.MySql);

            target.Preview = " ";

            EqualReason("F7U2C2_SQL_EMPTY", target);

            target.Preview = null;

            EqualReason("F7U2C2_SQL_EMPTY", target);
        }

        [TestMethod]
        public void C215_MissingTargetAndUnsupportedProviderFailClosed()
        {
            var missing = Target(DataSourceType.PostgreSql);

            missing.SelectedTable = string.Empty;

            EqualReason("F7U2C2_TARGET_INCOMPLETE", missing);

            var unsupported = Target(DataSourceType.Oracle);

            EqualReason("F7U2C2_INVALID_PROVIDER", unsupported);
        }

        [TestMethod]
        public void C216_ClosedConnectionReopensVerifiesAndDispatchesExactlyOnce()
        {
            var calls = new List<string>();

            var result = TryExecute
            (
                DataSourceType.PostgreSql, false, false,
                () => { calls.Add("ensure"); return string.Empty; },
                () => { calls.Add("verify"); return string.Empty; },
                () => { calls.Add("identity"); return SessionId; },
                id => { Assert.AreEqual(SessionId, id); calls.Add("dispatch"); return string.Empty; }
            );

            Assert.AreEqual(string.Empty, result);
            CollectionAssert.AreEqual(new[] { "ensure", "identity", "verify", "identity", "dispatch" }, calls.ToArray());
        }

        [TestMethod]
        public void C217_FailedLiveTargetVerificationNeverDispatches()
        {
            var calls = new List<string>();

            var result = TryExecute
            (
                DataSourceType.SqlServer, false, false,
                () => { calls.Add("ensure"); return string.Empty; },
                () => { calls.Add("verify"); return "F7U2C2_TARGET_NOT_FOUND"; },
                () => { calls.Add("identity"); return SessionId; },
                id => { calls.Add("dispatch"); return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_TARGET_NOT_FOUND", result);
            CollectionAssert.AreEqual(new[] { "ensure", "identity", "verify" }, calls.ToArray());
        }

        [TestMethod]
        public void C218_ConnectionChangeDuringLiveValidationPreventsDispatch()
        {
            var calls = new List<string>();
            var current = SessionId;

            var result = TryExecute
            (
                DataSourceType.PostgreSql, false, false,
                () => { calls.Add("ensure"); return string.Empty; },
                () => { calls.Add("verify"); current = Guid.Parse("06727a37-20b5-4f98-9ed8-2a689455ef0e"); return string.Empty; },
                () => { calls.Add("identity"); return current; },
                id => { calls.Add("dispatch"); return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_CONNECTION_CHANGED", result);
            CollectionAssert.AreEqual(new[] { "ensure", "identity", "verify", "identity" }, calls.ToArray());
        }

        [TestMethod]
        public void C219_ConnectionFailurePreventsVerificationAndDispatch()
        {
            var calls = new List<string>();

            var result = TryExecute
            (
                DataSourceType.MySql, false, true,
                () => { calls.Add("ensure"); return "F7U2C2_CONNECTION_FAILED"; },
                () => { calls.Add("verify"); return string.Empty; },
                () => { calls.Add("identity"); return SessionId; },
                id => { calls.Add("dispatch"); return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_CONNECTION_FAILED", result);
            CollectionAssert.AreEqual(new[] { "ensure" }, calls.ToArray());
        }

        [TestMethod]
        public void C220_PendingOrUnknownTransactionStopsBeforeReconnect()
        {
            var calls = new List<string>();

            var result = TryExecute
            (
                DataSourceType.SqlServer, true, false,
                () => { calls.Add("ensure"); return string.Empty; },
                () => { calls.Add("verify"); return string.Empty; },
                () => { calls.Add("identity"); return SessionId; },
                id => { calls.Add("dispatch"); return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_TRANSACTION_UNSAFE", result);
            Assert.AreEqual(0, calls.Count);
        }

        [TestMethod]
        public void C221_MySqlImplicitCommitRequiresExplicitAcknowledgment()
        {
            var calls = new List<string>();

            var result = TryExecute
            (
                DataSourceType.MySql, false, false,
                () => { calls.Add("ensure"); return string.Empty; },
                () => { calls.Add("verify"); return string.Empty; },
                () => { calls.Add("identity"); return SessionId; },
                id => { calls.Add("dispatch"); return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_MYSQL_IMPLICIT_COMMIT_CONFIRMATION_REQUIRED", result);
            Assert.AreEqual(0, calls.Count);
        }

        [TestMethod]
        public void C222_MySqlAcknowledgmentDoesNotBypassLiveVerification()
        {
            var calls = new List<string>();

            var result = TryExecute
            (
                DataSourceType.MySql, false, true,
                () => { calls.Add("ensure"); return string.Empty; },
                () => { calls.Add("verify"); return string.Empty; },
                () => { calls.Add("identity"); return SessionId; },
                id => { calls.Add("dispatch"); return string.Empty; }
            );

            Assert.AreEqual(string.Empty, result);
            CollectionAssert.AreEqual(new[] { "ensure", "identity", "verify", "identity", "dispatch" }, calls.ToArray());
        }

        [TestMethod]
        public void C223_ThreeReadersExposeNoReconnectGuardedDdlDispatchEntry()
        {
            var signature = new[]
            {
                typeof(string), typeof(Guid), typeof(string).MakeByRefType(),
                typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType()
            };

            foreach (var reader in new[] { typeof(PostgreSqlReader), typeof(SqlServerReader), typeof(MySqlReader) })
            {
                var method = reader.GetMethod
                (
                    "ExecuteSingleNonQueryOnVerifiedConnection",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    signature,
                    null
                );

                Assert.IsNotNull(method, string.Format("{0} must expose a no-reconnect DDL-only entry on the verified physical connection.", reader.Name));
                Assert.AreEqual(typeof(int), method.ReturnType);
            }
        }

        [TestMethod]
        public void C224_UnknownDispatchOutcomeIsNotRetriedOrReportedAsSafeFailure()
        {
            var calls = new List<string>();

            var result = TryExecute
            (
                DataSourceType.PostgreSql, false, false,
                () => { calls.Add("ensure"); return string.Empty; },
                () => { calls.Add("verify"); return string.Empty; },
                () => { calls.Add("identity"); return SessionId; },
                id => { calls.Add("dispatch"); return "F7U2C2_EXECUTION_OUTCOME_UNKNOWN"; }
            );

            Assert.AreEqual("F7U2C2_EXECUTION_OUTCOME_UNKNOWN", result);
            CollectionAssert.AreEqual(new[] { "ensure", "identity", "verify", "identity", "dispatch" }, calls.ToArray());
        }
    }
}
