using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.UI.Forms;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //B2c-1c-A2: implementation-facing RED contracts, without DB connections or WinForms initialization.
    //R01-R04 inspect compiled IL call edges (not source-text patterns) from the actual MainForm F7 refresh entry.
    //R05-R09 require a scope-aware snapshot API. Oracle must still reject unverified source captures.
    [TestClass]
    [TestCategory("F7L2B2C1CA2")]
    public sealed class FourProviderMetadataRefreshIntegrationRedTests
    {
        private static readonly Guid VerifiedOracleLoadId = new Guid("B2C10000-0000-4000-8000-000000000002");

        private static SharedLeftPanelHostState Connected()
        {
            return SharedLeftPanelHostStatePolicy.Refresh
            (
                SharedLeftPanelHostStatePolicy.Empty,
                true,
                true,
                "Query 1",
                LeftPanelTargetKind.QueryForm
            );
        }

        private static DataTable Source()
        {
            var source = new DataTable("TestMetadata");

            source.Columns.Add("SchemaObject", typeof(string));
            source.Columns.Add("SchemaNode", typeof(string));
            source.Columns.Add("SchemaType", typeof(string));
            source.Columns.Add("SchemaName", typeof(string));
            source.Columns.Add("OracleMetadataOwner", typeof(string));
            source.Rows.Add("T", "public", "Tables", "T", "JQ98XX");

            return source;
        }

        //Only the original verified snapshot may be read after a physical disconnect;
        //this is NOT a path to stamp a new Oracle connection ID on arbitrary cached rows.
        private static SharedSchemaMetadataSnapshot OriginalOracleSnapshot(SharedLeftPanelHostState state)
        {
            SharedSchemaMetadataSnapshot snapshot;

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCapture
            (
                Source(), DataSourceType.Oracle, false, state.ConnectionRevision,
                out snapshot, "JQ98XX", VerifiedOracleLoadId
            ));

            return snapshot;
        }

        private static MethodInfo RequireScopedCapture()
        {
            var method = typeof(SharedSchemaMetadataSnapshotPolicy).GetMethod
            (
                "TryCaptureForScope",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[]
                {
                    typeof(DataTable), typeof(DataSourceType), typeof(bool), typeof(int),
                    typeof(string), typeof(SharedSchemaMetadataSnapshot).MakeByRefType()
                },
                null
            );

            Assert.IsNotNull(method, "RED contract: missing scope-stamped metadata capture entry. This must not silently fall back to the unscoped API.");
            return method;
        }

        private static MethodInfo RequireScopedBind()
        {
            var method = typeof(SharedSchemaMetadataSnapshotPolicy).GetMethod
            (
                "CanBindForScope",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[]
                {
                    typeof(SharedLeftPanelHostState), typeof(SharedSchemaMetadataSnapshot),
                    typeof(DataSourceType), typeof(string)
                },
                null
            );

            Assert.IsNotNull(method, "RED contract: missing database-scope validation for an already-loaded snapshot.");
            return method;
        }

        private static SharedSchemaMetadataSnapshot CaptureScoped(DataSourceType provider, string scope)
        {
            var args = new object[] { Source(), provider, false, Connected().ConnectionRevision, scope, null };

            Assert.IsTrue((bool)RequireScopedCapture().Invoke(null, args));

            var snapshot = args[5] as SharedSchemaMetadataSnapshot;

            Assert.IsNotNull(snapshot);
            return snapshot;
        }

        private static bool CanBindScoped(DataSourceType provider, SharedSchemaMetadataSnapshot snapshot, string scope)
        {
            return (bool)RequireScopedBind().Invoke(null, new object[] { Connected(), snapshot, provider, scope });
        }

        private static void DatabaseSwitchRejectsOldScope(DataSourceType provider, string firstScope, string nextScope)
        {
            var oldSnapshot = CaptureScoped(provider, firstScope);

            Assert.IsTrue(CanBindScoped(provider, oldSnapshot, firstScope), "Original logical database must remain readable.");
            Assert.IsFalse(CanBindScoped(provider, oldSnapshot, nextScope), "A USE/menu database switch must not display old database metadata.");

            var newSnapshot = CaptureScoped(provider, nextScope);

            Assert.IsTrue(CanBindScoped(provider, newSnapshot, nextScope), "The new database's successfully loaded snapshot must be bindable.");
        }

        //Recursively reads actual compiled call/callvirt/newobj edges. It never opens a database.
        //This is a wiring contract, NOT a substitute for the later GUI or transactional smoke tests.
        private static readonly Dictionary<short, OpCode> OpCodeMap = BuildOpcodeMap();

        private static Dictionary<short, OpCode> BuildOpcodeMap()
        {
            var map = new Dictionary<short, OpCode>();

            foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(OpCode))
                {
                    var op = (OpCode)field.GetValue(null);
                    map[op.Value] = op;
                }
            }

            return map;
        }

        private static IEnumerable<MethodBase> CalledMethods(MethodBase method)
        {
            MethodBody body = null;

            try
            {
                body = method.GetMethodBody();
            }
            catch (InvalidOperationException)
            {

            }

            if (body == null)
            {
                yield break;
            }

            var il = body.GetILAsByteArray();
            var offset = 0;

            while (offset < il.Length)
            {
                var code = (short)il[offset++];

                if (code == 0xFE)
                {
                    code = (short)(0xFE00 | il[offset++]);
                }

                OpCode op;

                if (!OpCodeMap.TryGetValue(code, out op))
                {
                    yield break;
                }

                var tokenOffset = offset;
                var tokenSize = 0;

                switch (op.OperandType)
                {
                    case OperandType.InlineNone:
                        {
                            tokenSize = 0;
                            break;
                        }
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineVar:
                        {
                            tokenSize = 1;
                            break;
                        }
                    case OperandType.InlineVar:
                        {
                            tokenSize = 2;
                            break;
                        }
                    case OperandType.InlineI:
                    case OperandType.InlineBrTarget:
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineString:
                    case OperandType.InlineSig:
                    case OperandType.InlineTok:
                    case OperandType.InlineType:
                    case OperandType.ShortInlineR:
                        {
                            tokenSize = 4;
                            break;
                        }
                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                        {
                            tokenSize = 8;
                            break;
                        }
                    case OperandType.InlineSwitch:
                        {
                            if (offset + 4 > il.Length)
                            {
                                yield break;
                            }

                            tokenSize = 4 + BitConverter.ToInt32(il, offset) * 4;
                            break;
                        }
                    default:
                        {
                            yield break;
                        }
                }

                if (tokenSize < 0 || tokenOffset + tokenSize > il.Length)
                {
                    yield break;
                }

                offset += tokenSize;

                if (op != OpCodes.Call && op != OpCodes.Callvirt && op != OpCodes.Newobj)
                {
                    continue;
                }

                MethodBase resolved = null;

                try
                {
                    resolved = method.Module.ResolveMethod
                    (
                        BitConverter.ToInt32(il, tokenOffset),
                        method.DeclaringType != null && method.DeclaringType.IsGenericType ? method.DeclaringType.GetGenericArguments() : null,
                        method is MethodInfo mi && mi.IsGenericMethod ? mi.GetGenericArguments() : null
                    );
                }
                catch (ArgumentException)
                {

                }
                catch (BadImageFormatException)
                {

                }

                if (resolved != null)
                {
                    yield return resolved;
                }
            }
        }

        private static bool ReachesProviderLoader(string loaderMethodName)
        {
            var root = typeof(MainForm).GetMethod
            (
                "RefreshSharedSchemaExplorerPreviewMetadata",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(root, "MainForm F7 refresh entry no longer exists; review the wiring contract before proceeding.");

            var pending = new Stack<KeyValuePair<MethodBase, int>>();
            var visited = new HashSet<MethodBase>();

            pending.Push(new KeyValuePair<MethodBase, int>(root, 0));

            while (pending.Count > 0)
            {
                var entry = pending.Pop();
                var current = entry.Key;

                if (!visited.Add(current) || entry.Value > 9)
                {
                    continue;
                }

                if (current.DeclaringType == typeof(DatabaseSqlExecutor) && current.Name == loaderMethodName)
                {
                    return true;
                }

                if (current.Module.Assembly != typeof(MainForm).Assembly)
                {
                    continue;
                }

                foreach (var called in CalledMethods(current))
                {
                    pending.Push(new KeyValuePair<MethodBase, int>(called, entry.Value + 1));
                }
            }
            return false;
        }

        [TestMethod]
        public void R01_NewF7ManualRefreshReachesOracleMetadataLoader()
        {
            Assert.IsTrue(ReachesProviderLoader("UpdateSchemaData_Oracle"), "RED: new F7 currently rebinds dtSchema without querying Oracle.");
        }

        [TestMethod]
        public void R02_NewF7ManualRefreshReachesPostgreSqlMetadataLoader()
        {
            Assert.IsTrue(ReachesProviderLoader("UpdateSchemaData_PostgreSql"), "RED: new F7 currently rebinds dtSchema without querying PostgreSQL.");
        }

        [TestMethod]
        public void R03_NewF7ManualRefreshReachesSqlServerMetadataLoader()
        {
            Assert.IsTrue(ReachesProviderLoader("UpdateSchemaData_SqlServer"), "RED: new F7 currently rebinds dtSchema without querying SQL Server.");
        }

        [TestMethod]
        public void R04_NewF7ManualRefreshReachesMySqlMetadataLoader()
        {
            Assert.IsTrue(ReachesProviderLoader("UpdateSchemaData_MySql"), "RED: new F7 currently rebinds dtSchema without querying MySQL.");
        }

        [TestMethod]
        public void R05_PostgreSqlMenuSwitchMustRejectPreviousDatabaseSnapshot()
        {
            DatabaseSwitchRejectsOldScope(DataSourceType.PostgreSql, "pg|host|db_a|alice", "pg|host|db_b|alice");
        }

        [TestMethod]
        public void R06_SqlServerUseSwitchMustRejectPreviousDatabaseSnapshot()
        {
            DatabaseSwitchRejectsOldScope(DataSourceType.SqlServer, "mssql|host|db_a|alice", "mssql|host|db_b|alice");
        }

        [TestMethod]
        public void R07_MySqlUseSwitchMustRejectPreviousDatabaseSnapshot()
        {
            DatabaseSwitchRejectsOldScope(DataSourceType.MySql, "mysql|host|db_a|alice", "mysql|host|db_b|alice");
        }

        [TestMethod]
        public void R08_SameLogicalDatabaseMustRemainReusable()
        {
            var scope = "pg|host|db_a|alice";
            var snapshot = CaptureScoped(DataSourceType.PostgreSql, scope);

            Assert.IsTrue(CanBindScoped(DataSourceType.PostgreSql, snapshot, scope));
        }

        [TestMethod]
        public void R09_RefreshFailureRetainsExistingVerifiedSnapshot()
        {
            var state = Connected();
            var existing = OriginalOracleSnapshot(state);

            var policy = typeof(SharedSchemaExplorerPreviewPolicy).GetMethod
            (
                "ShouldRetainPublishedSnapshotAfterFailedRefresh",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(SharedLeftPanelHostState), typeof(SharedSchemaMetadataSnapshot), typeof(DataSourceType) },
                null
            );

            Assert.IsNotNull(policy, "RED: the new F7 lacks a failure-retention decision; its current exception path clears the binding.");
            Assert.IsTrue((bool)policy.Invoke(null, new object[] { state, existing, DataSourceType.Oracle }));
            Assert.IsFalse((bool)policy.Invoke(null, new object[] { state, null, DataSourceType.Oracle }));
        }

        [TestMethod]
        public void R10_ScopeCaptureMustNotBypassOracleProvenanceGate()
        {
            var args = new object[] { Source(), DataSourceType.Oracle, false, Connected().ConnectionRevision, "oracle|host|XEPDB1|JQ98XX", null };

            Assert.IsFalse((bool)RequireScopedCapture().Invoke(null, args), "A plain scope string cannot qualify Oracle metadata without an authoritative completed load.");
        }

        [TestMethod]
        public void R11_PreviouslyVerifiedOracleSnapshotRemainsReadableAfterPhysicalDisconnect()
        {
            var state = Connected();
            var snapshot = OriginalOracleSnapshot(state);

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.CanBind(state, snapshot, DataSourceType.Oracle));
            Assert.IsTrue(SharedSchemaLifecyclePolicy.CanReadCachedMetadata(true));

            //Do not invent a new physical identity merely because cached metadata is readable.
            Assert.IsFalse(SharedSchemaLifecyclePolicy.CanRunSessionBoundAction(true, false));
        }

        [TestMethod]
        public void R12_UnverifiedOracleRecaptureIsStillRejected()
        {
            var state = Connected();
            SharedSchemaMetadataSnapshot candidate;

            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture
            (
                Source(), DataSourceType.Oracle, false, state.ConnectionRevision,
                out candidate, string.Empty, Guid.Empty
            ));

            Assert.IsNull(candidate);
        }
    }
}
