using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //B2c-3B2 Golden-first: injected scope observations simulate connection changes; no Oracle connection is opened.
    //The injectable method is PRIVATE. Production must obtain observations from its OracleReader, never from a UI caller.
    [TestClass]
    [DoNotParallelize]
    [TestCategory("F7B2C3B2")]
    public sealed class OracleLoaderReceiptContinuityGoldenTests
    {
        private static readonly Guid SessionOne = new Guid("B2C30000-0000-4000-8000-000000000021");
        private static readonly Guid SessionTwo = new Guid("B2C30000-0000-4000-8000-000000000022");

        private sealed class PublishedState
        {
            public DataTable Schema;
            public DataTable Names;
            public DataTable AutoComplete;
            public string Functions;
            public string Tables;
            public string Triggers;
            public string Views;
            public bool IsShowColumnInfo;
            public int ShowColumnInfo;
            public DataSourceType SourceType;
        }

        private static PublishedState Capture()
        {
            return new PublishedState
            {
                Schema = DatabaseSqlExecutor.dtSchema,
                Names = DatabaseSqlExecutor.dtTableAndViews,
                AutoComplete = MyGlobal.dtAutoCompleteForAll,
                Functions = MyLibrary.KeywordsUserDefinedFunctions,
                Tables = MyLibrary.KeywordsUserDefinedTables,
                Triggers = MyLibrary.KeywordsUserDefinedTriggers,
                Views = MyLibrary.KeywordsUserDefinedViews,
                IsShowColumnInfo = MyGlobal.IsShowColumnInfo,
                ShowColumnInfo = MyGlobal.ShowColumnInfo,
                SourceType = DatabaseSqlExecutor.CurrentDataSource
            };
        }

        private static void Restore(PublishedState state)
        {
            DatabaseSqlExecutor.dtSchema = state.Schema;
            DatabaseSqlExecutor.dtTableAndViews = state.Names;
            MyGlobal.dtAutoCompleteForAll = state.AutoComplete;
            MyLibrary.KeywordsUserDefinedFunctions = state.Functions;
            MyLibrary.KeywordsUserDefinedTables = state.Tables;
            MyLibrary.KeywordsUserDefinedTriggers = state.Triggers;
            MyLibrary.KeywordsUserDefinedViews = state.Views;
            MyGlobal.IsShowColumnInfo = state.IsShowColumnInfo;
            MyGlobal.ShowColumnInfo = state.ShowColumnInfo;
            DatabaseSqlExecutor.CurrentDataSource = state.SourceType;
        }

        private static DataTable Marker(string value)
        {
            var table = new DataTable();

            table.Columns.Add("Marker", typeof(string));
            table.Rows.Add(value);

            return table;
        }

        private static void WithOldBatch(Action<PublishedState> action)
        {
            var original = Capture();

            try
            {
                DatabaseSqlExecutor.CurrentDataSource = DataSourceType.Oracle;
                MyGlobal.IsShowColumnInfo = true;
                MyGlobal.ShowColumnInfo = -1;
                DatabaseSqlExecutor.dtSchema = Marker("OLD_SCHEMA");
                DatabaseSqlExecutor.dtTableAndViews = Marker("OLD_NAMES");
                MyGlobal.dtAutoCompleteForAll = Marker("OLD_AC");
                MyLibrary.KeywordsUserDefinedFunctions = "OLD_FUNCTIONS";
                MyLibrary.KeywordsUserDefinedTables = "OLD_TABLES";
                MyLibrary.KeywordsUserDefinedTriggers = "OLD_TRIGGERS";
                MyLibrary.KeywordsUserDefinedViews = "OLD_VIEWS";
                action(Capture());
            }
            finally
            {
                Restore(original);
            }
        }

        private static void AssertBatchUnchanged(PublishedState previous)
        {
            Assert.AreSame(previous.Schema, DatabaseSqlExecutor.dtSchema, "Old Schema was replaced.");
            Assert.AreSame(previous.Names, DatabaseSqlExecutor.dtTableAndViews, "Old table/view names were replaced.");
            Assert.AreSame(previous.AutoComplete, MyGlobal.dtAutoCompleteForAll, "Old AutoComplete was replaced.");
            Assert.AreEqual(previous.Functions, MyLibrary.KeywordsUserDefinedFunctions);
            Assert.AreEqual(previous.Tables, MyLibrary.KeywordsUserDefinedTables);
            Assert.AreEqual(previous.Triggers, MyLibrary.KeywordsUserDefinedTriggers);
            Assert.AreEqual(previous.Views, MyLibrary.KeywordsUserDefinedViews);
        }

        private static OracleVerifiedSessionScope Scope(Guid connectionId, string user = "JQ98XX", string database = "XE", string container = "XEPDB1")
        {
            return new OracleVerifiedSessionScope
            {
                ConnectionId = connectionId,
                SessionUser = user,
                DatabaseName = database,
                ContainerName = container
            };
        }

        private static DataTable FakeResult()
        {
            //Match the existing AP07 Oracle organizer fixture; every query can be exercised without DB access.
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
            dt.Rows.Add("T1", "F1", "TR1", "V1", "C1", "JQ98XX", "VARCHAR2", "50", "", "VARCHAR2");

            return dt;
        }

        private static MethodInfo RequireCheckedCore()
        {
            //A distinct name is intentional: preexisting AP tests and B3B109 use GetMethod("RunOracleMetadataAtomicLoad") without overload resolution.
            var method = typeof(DatabaseSqlExecutor).GetMethod
            (
                "RunOracleMetadataAtomicLoadWithVerifiedScope",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(Func<string, DataTable>), typeof(bool), typeof(Func<OracleVerifiedSessionScope>) },
                null
            );

            Assert.IsNotNull(method, "B2c-3B2 requires a private Oracle query/scope observation seam used by the production loader.");
            Assert.IsTrue(method.IsPrivate, "The scope-injection seam must not be exposed as an application API.");

            var receiptType = typeof(DatabaseSqlExecutor).Assembly.GetType("JasonQuery.UI.LeftPanel.SharedOracleMetadataCompletedLoadReceipt", false);

            Assert.IsNotNull(receiptType, "Only an internal typed completed-load receipt is an acceptable result.");
            Assert.AreEqual(receiptType, method.ReturnType);

            return method;
        }

        private static object RunChecked(MethodInfo method, Func<string, DataTable> query, Func<OracleVerifiedSessionScope> observer)
        {
            try
            {
                return method.Invoke(null, new object[] { query, true, observer });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        private static void AssertRejectedAndPreserved(Func<int, OracleVerifiedSessionScope> scopeForObservation, Func<int, DataTable> queryForCall, bool requireQuery)
        {
            WithOldBatch(previous =>
            {
                var queryCount = 0;
                var observations = 0;
                var method = RequireCheckedCore();
                Exception failure = null;
                object receipt = null;

                try
                {
                    receipt = RunChecked
                    (
                        method,
                        sql =>
                        {
                            queryCount++;
                            return queryForCall(queryCount);
                        },
                        () =>
                        {
                            observations++;
                            return scopeForObservation(observations);
                        }
                    );
                }
                catch (Exception ex)
                {
                    failure = ex;
                }

                Assert.IsNull(receipt, "A rejected batch must never issue a completed-load receipt.");
                Assert.IsNotNull(failure, "Invalid scope or failed query must abort the batch explicitly.");

                if (requireQuery)
                {
                    Assert.IsTrue(queryCount > 0, "Injected query-failure or scope-change path was not exercised.");
                }

                AssertBatchUnchanged(previous);
            });
        }

        [TestMethod]
        public void B3B2R01_BaselineRejectsDifferentPhysicalConnection()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.CanBindMetadataToSession(Scope(SessionTwo), SessionOne, "JQ98XX", "XE", "XEPDB1"));
        }

        [TestMethod]
        public void B3B2R02_BaselineRejectsWrongPdb()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.MatchesLogicalScope(Scope(SessionOne, container: "OTHER_PDB"), "JQ98XX", "XE", "XEPDB1"));
        }

        [TestMethod]
        public void B3B2R03_BaselineRequiresAllVerifiedScopeValues()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.TryCreate("JQ98XX", "XE", "", SessionOne, out var scope));
            Assert.IsNull(scope);
        }

        [TestMethod]
        public void B3B2R04_BaselineDetectsSameUserReconnect()
        {
            Assert.IsTrue(OracleVerifiedSessionScopePolicy.IsNewVerifiedSession(Scope(SessionOne), Scope(SessionTwo)));
        }

        [TestMethod]
        public void B3B2R05_CheckedCoreRequiresPrivateTypedReceipt()
        {
            RequireCheckedCore();
        }

        [TestMethod]
        public void B3B2R06_SameUserReconnectedMidBatchMustNotPublish()
        {
            var connection = SessionOne;

            AssertRejectedAndPreserved
            (
                observation => Scope(connection),
                call =>
                {
                    if (call == 2)
                    {
                        connection = SessionTwo;
                    }
                    return FakeResult();
                },
                true
            );
        }

        [TestMethod]
        public void B3B2R07_ContainerChangedMidBatchMustNotPublish()
        {
            var pdb = "XEPDB1";

            AssertRejectedAndPreserved
            (
                observation => Scope(SessionOne, container: pdb),
                call =>
                {
                    if (call == 2)
                    {
                        pdb = "OTHER_PDB";
                    }
                    return FakeResult();
                },
                true
            );
        }

        [TestMethod]
        public void B3B2R08_DatabaseChangedMidBatchMustNotPublish()
        {
            var database = "XE";

            AssertRejectedAndPreserved
            (
                observation => Scope(SessionOne, database: database),
                call =>
                {
                    if (call == 2)
                    {
                        database = "OTHER_DB";
                    }
                    return FakeResult();
                },
                true
            );
        }

        [TestMethod]
        public void B3B2R09_SessionUserChangedMidBatchMustNotPublish()
        {
            var user = "JQ98XX";

            AssertRejectedAndPreserved
            (
                observation => Scope(SessionOne, user: user),
                call =>
                {
                    if (call == 2)
                    {
                        user = "OTHER_USER";
                    }
                    return FakeResult();
                },
                true
            );
        }

        [TestMethod]
        public void B3B2R10_UnknownScopeBeforeFirstQueryMustReject()
        {
            WithOldBatch(previous =>
            {
                var method = RequireCheckedCore();
                var queryCount = 0;
                Exception failure = null;

                try
                {
                    var result = RunChecked(method, sql => { queryCount++; return FakeResult(); }, () => null);

                    Assert.IsNull(result, "Unknown session cannot issue a receipt.");
                }
                catch (Exception ex)
                {
                    failure = ex;
                }

                Assert.AreEqual(0, queryCount, "Must check the verified scope before the first metadata query.");
                Assert.IsNotNull(failure, "A missing verified session must be explicitly rejected.");

                AssertBatchUnchanged(previous);
            });
        }

        [TestMethod]
        public void B3B2R11_NullQueryResultPreservesOldBatch()
        {
            AssertRejectedAndPreserved(observation => Scope(SessionOne), call => call == 3 ? null : FakeResult(), true);
        }

        [TestMethod]
        public void B3B2R12_ThrownTimeoutPreservesOldBatch()
        {
            AssertRejectedAndPreserved
            (
                observation => Scope(SessionOne),
                call =>
                {
                    if (call == 5)
                    {
                        throw new TimeoutException("Injected Oracle metadata timeout");
                    }
                    return FakeResult();
                },
                true
            );
        }

        [TestMethod]
        public void B3B2R13_SuccessReceiptMustBindExactPublishedDataTable()
        {
            WithOldBatch(previous =>
            {
                var method = RequireCheckedCore();
                var count = 0;
                var receipt = RunChecked(method, sql => { count++; return FakeResult(); }, () => Scope(SessionOne));

                Assert.IsTrue(count >= 5, "Real metadata organizer queries must execute.");
                Assert.IsNotNull(receipt, "Only a completed coherent batch may issue the typed receipt.");
                Assert.AreNotSame(previous.Schema, DatabaseSqlExecutor.dtSchema);

                var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var sourceProperty = receipt.GetType().GetProperty("SourceTable", flags);
                var connectionProperty = receipt.GetType().GetProperty("ConnectionId", flags);
                var loadProperty = receipt.GetType().GetProperty("CompletedLoadId", flags);

                Assert.IsNotNull(sourceProperty);
                Assert.IsNotNull(connectionProperty);
                Assert.IsNotNull(loadProperty);
                Assert.AreSame(DatabaseSqlExecutor.dtSchema, sourceProperty.GetValue(receipt));
                Assert.AreEqual(SessionOne, connectionProperty.GetValue(receipt));
                Assert.AreNotEqual(Guid.Empty, loadProperty.GetValue(receipt));
            });
        }

        [TestMethod]
        public void B3B2R14_VerifiedScopeMustBeObservedBeforeAndAfterEachQuery()
        {
            WithOldBatch(previous =>
            {
                var method = RequireCheckedCore();
                var queries = 0;
                var observations = 0;

                RunChecked(method, sql => { queries++; return FakeResult(); }, () => { observations++; return Scope(SessionOne); });

                Assert.IsTrue(queries >= 5);
                Assert.IsTrue(observations >= (queries * 2) + 1, "Require before/after observations for every query and a final pre-publication observation.");
            });
        }

        //L2 closeout: retain the two nonredundant physical-scope regressions after removing the abandoned Logical Generation model and its tests.
        private static bool MethodCalls(MethodInfo caller, MethodInfo callee)
        {
            var bytes = caller?.GetMethodBody()?.GetILAsByteArray();

            if (bytes == null || callee == null)
            {
                return false;
            }

            var token = BitConverter.GetBytes(callee.MetadataToken);

            for (var index = 0; index + 4 < bytes.Length; index++)
            {
                if (bytes[index] != 0x28 && bytes[index] != 0x6f)
                {
                    continue;
                }

                if (bytes[index + 1] == token[0] && bytes[index + 2] == token[1] && bytes[index + 3] == token[2] && bytes[index + 4] == token[3])
                {
                    return true;
                }
            }

            return false;
        }

        [TestMethod]
        public void B3B2R15_ProductionOracleAtomicEntryUsesVerifiedScopeCore()
        {
            var verified = RequireCheckedCore();
            var entry = typeof(DatabaseSqlExecutor).GetMethod("RunOracleMetadataAtomicLoad", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(entry);
            Assert.IsTrue(MethodCalls(entry, verified), "Production Oracle metadata loading must use the verified-session core.");
        }

        [TestMethod]
        public void B3B2R16_ChangedLogicalConnectionSettingsPreservesPreviousBatch()
        {
            WithOldBatch(previous =>
            {
                var priorConnectionName = DatabaseSqlExecutor.DbConnectionName;
                var queryCount = 0;
                Exception failure = null;

                try
                {
                    var method = RequireCheckedCore();

                    try
                    {
                        RunChecked
                        (
                            method,
                            sql =>
                            {
                                queryCount++;

                                if (queryCount == 2)
                                {
                                    DatabaseSqlExecutor.DbConnectionName = (priorConnectionName ?? string.Empty) + "_SWITCHED";
                                }

                                return FakeResult();
                            },
                            () => Scope(SessionOne)
                        );
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                    }
                }
                finally
                {
                    DatabaseSqlExecutor.DbConnectionName = priorConnectionName;
                }

                Assert.IsTrue(queryCount >= 2, "Injected connection-settings switch was not exercised.");
                Assert.IsInstanceOfType(failure, typeof(InvalidOperationException));
                AssertBatchUnchanged(previous);
            });
        }
    }
}
