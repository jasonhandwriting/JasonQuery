using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;
using System.Linq;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory("F7L2B2C")]
    public sealed class SharedF7StartupAndProvenanceGoldenTests
    {
        private static readonly Guid VerifiedConnectionId = new Guid("F1B20000-0000-4000-8000-000000000001");

        private static SharedLeftPanelHostState Connected()
        {
            return SharedLeftPanelHostStatePolicy.Refresh
            (
                SharedLeftPanelHostStatePolicy.Empty,
                true,
                true,
                "SQL Editor 1",
                LeftPanelTargetKind.QueryForm
            );
        }

        //B2c-3C2: pure UI eligibility is separate from metadata provenance or DDL authorization.
        //The request is explicit; avoid the retired unnamed 7-argument bool array.
        private static bool AutoShow(SharedLeftPanelHostState state, DataSourceType sourceType, bool trusted = true, bool pendingTransaction = false, bool userSelectedDifferentTab = false, bool alreadySelected = false, string publishedScope = null)
        {
            var assembly = typeof(SharedSchemaMetadataSnapshot).Assembly;
            var requestType = assembly.GetType("JasonQuery.UI.LeftPanel.SharedSchemaExplorerStartupDecisionContext", false);

            Assert.IsNotNull(requestType, "B2c-3C3 startup decision context is not yet implemented.");

            var policyType = assembly.GetType("JasonQuery.UI.LeftPanel.SharedSchemaExplorerStartupDecisionPolicy", false);

            Assert.IsNotNull(policyType, "B2c-3C3 startup decision policy is not yet implemented.");

            var decision = policyType.GetMethod
            (
                "MayAutoSelectF7",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { requestType },
                null
            );

            Assert.IsNotNull(decision, "Startup must use one typed decision request, not positional booleans.");
            Assert.AreEqual(typeof(bool), decision.ReturnType);

            var request = Activator.CreateInstance(requestType, true);
            var scope = sourceType == DataSourceType.Oracle ? "Oracle:initial-scope" : "PostgreSql:initial-scope";

            SetStartupValue(request, "HostState", state);
            SetStartupValue(request, "SourceType", sourceType);
            SetStartupValue(request, "WorkspaceVisible", true);
            SetStartupValue(request, "WorkspaceWidth", 1200);
            SetStartupValue(request, "CurrentDatabaseScope", scope);
            SetStartupValue(request, "PublishedDatabaseScope", publishedScope ?? scope);
            SetStartupValue(request, "PublishedConnectionRevision", state.ConnectionRevision);
            SetStartupValue(request, "HasTrustedPublishedMetadata", trusted);
            SetStartupValue(request, "HasPendingOrUnknownTransaction", pendingTransaction);
            SetStartupValue(request, "IsInitialLoad", true);
            SetStartupValue(request, "UserSelectedDifferentLeftTab", userSelectedDifferentTab);
            SetStartupValue(request, "AlreadyAutoSelectedForConnection", alreadySelected);

            return (bool)decision.Invoke(null, new[] { request });
        }

        private static void SetStartupValue(object request, string name, object value)
        {
            var property = request.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsNotNull(property, "Missing explicit startup decision property: " + name);
            Assert.IsTrue(property.CanWrite, "Startup decision property must be initialized: " + name);

            property.SetValue(request, value);
        }

        private static OracleVerifiedSessionScope VerifiedScope(Guid connectionId)
        {
            return new OracleVerifiedSessionScope
            {
                ConnectionId = connectionId,
                SessionUser = "JQ98XX",
                DatabaseName = "XE",
                ContainerName = "XEPDB1"
            };
        }

        private static DataTable FakeOracleResult()
        {
            //Same metadata columns as the already-GREEN B3B2 continuity fixture.
            var table = new DataTable();

            table.Columns.Add("TableName", typeof(string));
            table.Columns.Add("FunctionName", typeof(string));
            table.Columns.Add("TriggerName", typeof(string));
            table.Columns.Add("ViewName", typeof(string));
            table.Columns.Add("ColumnName", typeof(string));
            table.Columns.Add("OracleMetadataOwner", typeof(string));
            table.Columns.Add("DataType", typeof(string));
            table.Columns.Add("DataLength", typeof(string));
            table.Columns.Add("Scale", typeof(string));
            table.Columns.Add("ColumnType", typeof(string));
            table.Rows.Add("T1", "F1", "TR1", "V1", "C1", "JQ98XX", "VARCHAR2", "50", "", "VARCHAR2");

            return table;
        }

        private static MethodInfo CheckedOracleCore()
        {
            var method = typeof(DatabaseSqlExecutor).GetMethod
            (
                "RunOracleMetadataAtomicLoadWithVerifiedScope",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(Func<string, DataTable>), typeof(bool), typeof(Func<OracleVerifiedSessionScope>) },
                null
            );

            Assert.IsNotNull(method, "The B2c-3B2 checked Oracle core must remain present.");
            Assert.IsTrue(method.IsPrivate);

            return method;
        }

        private static object InvokeCore(MethodInfo method, object[] arguments)
        {
            try
            {
                return method.Invoke(null, arguments);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        }

        //The test gets a REAL loader-issued receipt through the private checked core.
        //It never constructs a mutable SharedOracleMetadataProvenance as authority.
        private static void WithUncorrelatedOracleReceipt(Action<DataTable, object, OracleVerifiedSessionScope> action)
        {
            var oldSchema = DatabaseSqlExecutor.dtSchema;
            var oldNames = DatabaseSqlExecutor.dtTableAndViews;
            var oldAutoComplete = MyGlobal.dtAutoCompleteForAll;
            var oldFunctions = MyLibrary.KeywordsUserDefinedFunctions;
            var oldTables = MyLibrary.KeywordsUserDefinedTables;
            var oldTriggers = MyLibrary.KeywordsUserDefinedTriggers;
            var oldViews = MyLibrary.KeywordsUserDefinedViews;
            var oldSource = DatabaseSqlExecutor.CurrentDataSource;
            var oldIsShow = MyGlobal.IsShowColumnInfo;
            var oldShow = MyGlobal.ShowColumnInfo;
            var receiptField = typeof(DatabaseSqlExecutor).GetField("_lastOracleCompletedLoadReceipt", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(receiptField);

            var oldReceipt = receiptField.GetValue(null);

            try
            {
                DatabaseSqlExecutor.CurrentDataSource = DataSourceType.Oracle;
                MyGlobal.IsShowColumnInfo = true;
                MyGlobal.ShowColumnInfo = -1;

                var scope = VerifiedScope(VerifiedConnectionId);

                var receipt = InvokeCore
                (
                    CheckedOracleCore(),
                    new object[]
                    {
                        new Func<string, DataTable>(sql => FakeOracleResult()),
                        true,
                        new Func<OracleVerifiedSessionScope>(() => VerifiedScope(VerifiedConnectionId))
                    }
                );

                Assert.IsNotNull(receipt);
                Assert.AreSame(DatabaseSqlExecutor.dtSchema, receipt.GetType().GetProperty("SourceTable", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(receipt));

                action(DatabaseSqlExecutor.dtSchema, receipt, scope);
            }
            finally
            {
                DatabaseSqlExecutor.dtSchema = oldSchema;
                DatabaseSqlExecutor.dtTableAndViews = oldNames;

                MyGlobal.dtAutoCompleteForAll = oldAutoComplete;

                MyLibrary.KeywordsUserDefinedFunctions = oldFunctions;
                MyLibrary.KeywordsUserDefinedTables = oldTables;
                MyLibrary.KeywordsUserDefinedTriggers = oldTriggers;
                MyLibrary.KeywordsUserDefinedViews = oldViews;

                DatabaseSqlExecutor.CurrentDataSource = oldSource;

                MyGlobal.IsShowColumnInfo = oldIsShow;
                MyGlobal.ShowColumnInfo = oldShow;

                receiptField.SetValue(null, oldReceipt);
            }
        }

        private static bool TryCaptureWithReceipt(DataTable source, object receipt, OracleVerifiedSessionScope scope, out SharedSchemaMetadataSnapshot snapshot)
        {
            snapshot = null;

            var receiptType = receipt.GetType();

            var method = typeof(SharedSchemaMetadataSnapshotPolicy).GetMethod
            (
                "TryCaptureQualifiedOracle",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(DataTable), typeof(bool), typeof(int), typeof(string), typeof(OracleVerifiedSessionScope), receiptType, typeof(SharedSchemaMetadataSnapshot).MakeByRefType() },
                null
            );

            Assert.IsNotNull(method, "Qualified Oracle bridge must require the INTERNAL completed-load receipt.");
            Assert.IsFalse(method.IsPublic);

            var arguments = new object[] { source, false, Connected().ConnectionRevision, "Oracle:initial-scope", scope, receipt, null };
            var accepted = (bool)method.Invoke(null, arguments);

            snapshot = arguments[6] as SharedSchemaMetadataSnapshot;
            return accepted;
        }

        private static bool ContainsCallTo(MethodInfo caller, MethodInfo callee)
        {
            var body = caller?.GetMethodBody()?.GetILAsByteArray();

            if (body == null || callee == null)
            {
                return false;
            }

            var token = BitConverter.GetBytes(callee.MetadataToken);

            for (var index = 0; index + 4 < body.Length; index++)
            {
                if (body[index] != 0x28 && body[index] != 0x6f)
                {
                    continue;
                }

                if (body[index + 1] == token[0] && body[index + 2] == token[1] && body[index + 3] == token[2] && body[index + 4] == token[3])
                {
                    return true;
                }
            }

            return false;
        }

        [TestMethod]
        public void C01_ConnectedWideWorkspaceCanOfferF7Preview()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.CanOffer(Connected(), DataSourceType.Oracle, true, 1200));
        }

        [TestMethod]
        public void C02_NarrowWorkspaceMustNotForcePreview()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(Connected(), DataSourceType.Oracle, true, 500));
        }

        [TestMethod]
        public void C03_DisconnectedWorkspaceCannotOfferPreview()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(SharedLeftPanelHostStatePolicy.Empty, DataSourceType.Oracle, true, 1200));
        }

        [TestMethod]
        public void C04_HiddenWorkspaceCannotOfferPreview()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(Connected(), DataSourceType.Oracle, false, 1200));
        }

        [TestMethod]
        public void C05_SourceNoneCannotOfferPreview()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(Connected(), DataSourceType.None, true, 1200));
        }

        [TestMethod]
        public void C06_DisconnectedWorkspaceMustNotShowSharedPanel()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.ShouldShowWorkspace(SharedLeftPanelHostStatePolicy.Empty, true));
        }

        [TestMethod]
        public void C07_ConnectedWorkspaceMayShowSharedPanel()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldShowWorkspace(Connected(), true));
        }

        [TestMethod]
        public void C08_UnboundPreviewNeedsInitialBinding()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldRefreshOnShow(Connected(), null, DataSourceType.Oracle, false));
        }

        [TestMethod]
        public void C09_TrustedInitialOracleSchemaMayAutoShowF7()
        {
            Assert.IsTrue(AutoShow(Connected(), DataSourceType.Oracle));
        }

        [TestMethod]
        public void C10_TrustedInitialPostgreSqlSchemaMayAutoShowF7()
        {
            Assert.IsTrue(AutoShow(Connected(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void C11_TrustedSchemaAfterNewLogicalConnectionMayAutoShowF7()
        {
            var disconnected = SharedLeftPanelHostStatePolicy.Refresh(Connected(), false, false, null, LeftPanelTargetKind.None);
            var reconnected = SharedLeftPanelHostStatePolicy.Refresh(disconnected, true, true, "SQL Editor 2", LeftPanelTargetKind.QueryForm);

            Assert.IsTrue(reconnected.ConnectionRevision > Connected().ConnectionRevision);
            Assert.IsTrue(AutoShow(reconnected, DataSourceType.Oracle));
        }

        [TestMethod]
        public void C12_UncorrelatedOracleReceiptMustFailClosed()
        {
            WithUncorrelatedOracleReceipt((source, receipt, scope) =>
            {
                Assert.IsFalse(TryCaptureWithReceipt(source, receipt, scope, out var snapshot));
                Assert.IsNull(snapshot);
            });
        }

        [TestMethod]
        public void C13_PhysicalReceiptCannotBeRetrospectivelyGrantedLogicalQualification()
        {
            //Read-only F7 does not need a DDL-level credential. A completed physical load must NOT become qualified merely because the UI currently reports a revision.
            WithUncorrelatedOracleReceipt((source, receipt, scope) =>
            {
                var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var type = receipt.GetType();

                Assert.AreSame(source, type.GetProperty("SourceTable", flags).GetValue(receipt));
                Assert.AreEqual(0, type.GetProperty("TrustedLogicalConnectionRevision", flags).GetValue(receipt));
                Assert.IsFalse(TryCaptureWithReceipt(source, receipt, scope, out var qualified));
                Assert.IsNull(qualified);
            });
        }

        [TestMethod]
        public void C14_PermanentWorkspaceVisibilityDoesNotImplyQualification()
        {
            //A visible workspace is not a trusted Oracle snapshot or a DDL Execute grant.
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldKeepPermanentWorkspaceVisible(Connected(), true, true));
        }

        [TestMethod]
        public void C15_SameRevisionDifferentLogicalDatabaseScopeMustNotBind()
        {
            var source = new DataTable();

            source.Columns.Add("SchemaObject");
            source.Columns.Add("SchemaNode");
            source.Columns.Add("SchemaType");
            source.Columns.Add("SchemaName");
            source.Rows.Add("DbA", "public", "Tables", "T1");

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCaptureForScope
            (
                source, DataSourceType.PostgreSql, false, Connected().ConnectionRevision, "PostgreSql:DbA", out var snapshot
            ));

            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBindForScope
            (
                Connected(), snapshot, DataSourceType.PostgreSql, "PostgreSql:DbB"
            ));
        }

        [TestMethod]
        public void C16_OracleCannotUseSourceNeutralScopeCaptureToBecomeQualified()
        {
            var source = new DataTable();

            source.Columns.Add("SchemaObject");
            source.Columns.Add("SchemaType");
            source.Columns.Add("SchemaName");

            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCaptureForScope
            (
                source, DataSourceType.Oracle, false, Connected().ConnectionRevision, "Oracle:initial-scope", out var snapshot
            ));

            Assert.IsNull(snapshot);
        }

        [TestMethod]
        public void C17_QualifiedBridgeRejectsCopiedTableAndChangedPhysicalSession()
        {
            WithUncorrelatedOracleReceipt((source, receipt, scope) =>
            {
                Assert.IsFalse(TryCaptureWithReceipt(source.Copy(), receipt, scope, out var copySnapshot));
                Assert.IsNull(copySnapshot);
                Assert.IsFalse(TryCaptureWithReceipt(source, receipt, VerifiedScope(Guid.NewGuid()), out var wrongSession));
                Assert.IsNull(wrongSession);
            });
        }

        [TestMethod]
        public void C19_UntrustedMetadataMustNotAutoSelectF7()
        {
            Assert.IsFalse(AutoShow(Connected(), DataSourceType.Oracle, trusted: false));
        }

        [TestMethod]
        public void C20_PendingOrUnknownTransactionMustNotAutoSelectF7()
        {
            Assert.IsFalse(AutoShow(Connected(), DataSourceType.Oracle, pendingTransaction: true));
        }

        [TestMethod]
        public void C21_UserSelectedAnotherLeftTabMustNotBeHijacked()
        {
            Assert.IsFalse(AutoShow(Connected(), DataSourceType.PostgreSql, userSelectedDifferentTab: true));
        }

        [TestMethod]
        public void C22_AlreadyAutoSelectedConnectionMustNotSelectTwice()
        {
            Assert.IsFalse(AutoShow(Connected(), DataSourceType.PostgreSql, alreadySelected: true));
        }

        [TestMethod]
        public void C23_ConnectedSameRevisionButDifferentScopeMustNotAutoSelect()
        {
            Assert.IsFalse(AutoShow(Connected(), DataSourceType.PostgreSql, publishedScope: "PostgreSql:other-database"));
        }

        [TestMethod]
        public void C18_MainFormSuccessNotificationMustUseStartupDecision()
        {
            //The real post-load callback must use the policy; a test-only policy is insufficient.
            var assembly = typeof(DatabaseSqlExecutor).Assembly;
            var policyType = assembly.GetType("JasonQuery.UI.LeftPanel.SharedSchemaExplorerStartupDecisionPolicy", false);

            Assert.IsNotNull(policyType, "Startup policy is not yet implemented.");

            var decision = policyType.GetMethod("MayAutoSelectF7", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(decision);

            var mainForm = assembly.GetType("JasonQuery.UI.Forms.MainForm", true);
            var callback = mainForm.GetMethod("TryBindSharedSchemaExplorerAfterLegacyReload", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(callback);
            Assert.IsTrue(ContainsCallTo(callback, decision), "MainForm must apply the policy in the existing successful-load callback; no second SQL load.");
        }
    }
}
