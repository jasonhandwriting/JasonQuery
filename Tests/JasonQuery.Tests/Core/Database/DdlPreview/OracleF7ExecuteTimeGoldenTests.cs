using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    //Q2-98A RED contracts only. No Oracle connection and no DDL are executed.
    //New F7 may display SQL from its cache, but authority is evaluated on Execute.
    [TestClass]
    [TestCategory("Q298")]
    public sealed class OracleF7ExecuteTimeGoldenTests
    {
        private const string ValidSql = "DROP TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\";";

        private static MethodInfo RequireGateMethod(string name, params Type[] parameterTypes)
        {
            var gateType = typeof(TableDdlRequest).Assembly.GetType
            (
                "JasonQuery.Core.Database.DdlPreview.OracleDdlGuiGate",
                false
            );

            Assert.IsNotNull(gateType, "The existing OracleDdlGuiGate type must remain available.");

            var method = gateType.GetMethod
            (
                name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                parameterTypes,
                null
            );

            Assert.IsNotNull
            (
                method,
                string.Format("Q2-98A requires the minimal execute-time gate method: OracleDdlGuiGate.{0}.", name)
            );

            return method;
        }

        private static bool CanOfferExecuteSql(string sql)
        {
            var method = RequireGateMethod("CanOfferExecuteSql", typeof(string));

            Assert.AreEqual(typeof(bool), method.ReturnType);

            return (bool)method.Invoke(null, new object[] { sql });
        }

        //A small, testable orchestration seam; production delegates remain responsible for trusted reader scope, exact target checks and no-reconnect DDL dispatch.
        //Each callback returns empty string for success, or a diagnostic code for failure.
        private static string TryExecuteF7(bool hasPendingOrUnknownTransaction, Func<string> ensureConnection, Func<string> verifyLiveScopeTargetAndSql, Func<string> executeOnVerifiedConnection)
        {
            var method = RequireGateMethod
            (
                "TryExecuteF7",
                typeof(bool),
                typeof(Func<string>),
                typeof(Func<string>),
                typeof(Func<string>)
            );

            Assert.AreEqual(typeof(string), method.ReturnType);

            return (string)method.Invoke
            (
                null,
                new object[]
                {
                    hasPendingOrUnknownTransaction,
                    ensureConnection,
                    verifyLiveScopeTargetAndSql,
                    executeOnVerifiedConnection
                }
            );
        }

        [TestMethod]
        public void G01_ValidSqlOffersExecuteWithoutRequiringAnAlreadyOpenSession()
        {
            //Physical disconnection is normal after a query in JasonQuery.
            //The SQL/field state, not Reader.GetState(), determines button availability.
            Assert.IsTrue(CanOfferExecuteSql(ValidSql));
            Assert.IsFalse(CanOfferExecuteSql("   "));
            Assert.IsFalse(CanOfferExecuteSql(null));
        }

        [TestMethod]
        public void G02_ClosedSessionCanReconnectVerifyAndDispatchInOneClick()
        {
            var calls = new List<string>();

            var result = TryExecuteF7
            (
                false,
                () => { calls.Add("ensure-open"); return string.Empty; },
                () => { calls.Add("verify-new-session-and-target"); return string.Empty; },
                () => { calls.Add("guarded-execute"); return string.Empty; }
            );

            Assert.AreEqual(string.Empty, result, "A normal reconnection must not become an error or require another click.");

            CollectionAssert.AreEqual
            (
                new[] { "ensure-open", "verify-new-session-and-target", "guarded-execute" },
                calls.ToArray(),
                "Connection establishment precedes validation; DDL runs only after validation."
            );
        }

        [TestMethod]
        public void G03_RealScopeTargetOrSqlFailureStopsDispatchAndPreservesReason()
        {
            foreach (var reason in new[] { "Q298_SCOPE_MISMATCH", "Q298_TARGET_NOT_FOUND", "Q298_SQL_CHANGED" })
            {
                var calls = new List<string>();

                var result = TryExecuteF7
                (
                    false,
                    () => { calls.Add("ensure-open"); return string.Empty; },
                    () => { calls.Add("verify"); return reason; },
                    () => { calls.Add("guarded-execute"); return string.Empty; }
                );

                Assert.AreEqual(reason, result, "The UI must receive a specific failure reason.");
                CollectionAssert.AreEqual(new[] { "ensure-open", "verify" }, calls.ToArray());
            }
        }

        [TestMethod]
        public void G04_UnsafeTransactionOrConnectionFailureStopsWithExplicitReason()
        {
            var calls = new List<string>();

            var pending = TryExecuteF7
            (
                true,
                () => { calls.Add("ensure-open"); return string.Empty; },
                () => { calls.Add("verify"); return string.Empty; },
                () => { calls.Add("guarded-execute"); return string.Empty; }
            );

            Assert.AreEqual("Q298_TRANSACTION_PENDING", pending);
            Assert.AreEqual(0, calls.Count, "An unsafe transaction must not trigger an implicit reconnect.");

            var failedConnection = TryExecuteF7
            (
                false,
                () => { calls.Add("ensure-open"); return "Q298_CONNECTION_FAILED"; },
                () => { calls.Add("verify"); return string.Empty; },
                () => { calls.Add("guarded-execute"); return string.Empty; }
            );

            Assert.AreEqual("Q298_CONNECTION_FAILED", failedConnection);
            CollectionAssert.AreEqual(new[] { "ensure-open" }, calls.ToArray());
        }

        [TestMethod]
        public void G05_VerifiedDdlDispatchHasNoHiddenReconnectOrRetry()
        {
            //This separate Reader entry is essential: the existing generic ExecuteSingleNonQuery calls EnsureConnectionOpen, which may reconnect AFTER a form has validated the previously observed physical session.
            var guardedEntry = typeof(OracleReader).GetMethod
            (
                "ExecuteSingleNonQueryOnVerifiedConnection",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(string),
                    typeof(Guid),
                    typeof(string).MakeByRefType(),
                    typeof(bool).MakeByRefType(),
                    typeof(bool).MakeByRefType()
                },
                null
            );

            Assert.IsNotNull
            (
                guardedEntry,
                "Q2-98 requires a DDL-only Reader entry that checks the expected physical ConnectionId and never silently calls EnsureConnectionOpen."
            );

            Assert.AreEqual(typeof(int), guardedEntry.ReturnType);

            var calls = new List<string>();

            var result = TryExecuteF7
            (
                false,
                () => { calls.Add("ensure-open"); return string.Empty; },
                () => { calls.Add("verify"); return string.Empty; },
                () => { calls.Add("guarded-execute"); return "Q298_SESSION_CHANGED"; }
            );

            Assert.AreEqual("Q298_SESSION_CHANGED", result);

            CollectionAssert.AreEqual
            (
                new[] { "ensure-open", "verify", "guarded-execute" },
                calls.ToArray(),
                "A changed session cannot cause an unvalidated reconnect/retry."
            );
        }
    }
}
