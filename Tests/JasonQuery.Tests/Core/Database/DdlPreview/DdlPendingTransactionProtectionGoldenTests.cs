using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    //F7-S1-A: policy-only Golden RED. Does not open DB connections or dispatch DDL.
    //Against baseline e3081da6: G01/G02 GREEN; 25 data rows expected RED.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class DdlPendingTransactionProtectionGoldenTests
    {
        private const string PolicyName = "JasonQuery.Core.Database.DdlPreview.DdlPendingTransactionProtectionPolicy";

        //Planned pure-policy signature:
        //Evaluate(DataSourceType provider, string operation, bool? pendingTransaction)
        //pendingTransaction: true = Pending, false = explicitly verified Clean, null = Unknown.
        //Never derive an explicitly verified Clean state from an unavailable Reader.
        private static object Evaluate(string providerName, string operation, bool? pendingTransaction)
        {
            var assembly = typeof(TableDdlRequest).Assembly;
            var type = assembly.GetType(PolicyName, false);

            Assert.IsNotNull(type, "Expected RED: F7-S1 transaction protection policy is not yet implemented.");

            var method = type.GetMethod
            (
                "Evaluate",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(DataSourceType), typeof(string), typeof(bool?) },
                null
            );

            Assert.IsNotNull(method, "Expected RED: Evaluate(DataSourceType, string, bool?) is missing.");

            var provider = (DataSourceType)Enum.Parse(typeof(DataSourceType), providerName);
            var result = method.Invoke(null, new object[] { provider, operation, pendingTransaction });

            Assert.IsNotNull(result, "Expected RED: the policy returned a null decision.");
            return result;
        }

        private static bool Flag(object decision, string propertyName)
        {
            var property = decision.GetType().GetProperty
            (
                propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            Assert.IsNotNull(property, "Expected RED: decision property missing: " + propertyName);

            var value = property.GetValue(decision, null);

            Assert.IsInstanceOfType(value, typeof(bool));
            return (bool)value;
        }

        private static string Reason(object decision)
        {
            var property = decision.GetType().GetProperty
            (
                "ReasonCode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            Assert.IsNotNull(property, "Expected RED: decision.ReasonCode is missing.");
            return property.GetValue(decision, null) as string;
        }

        [TestMethod]
        public void G01_ExistingOracleGateRejectsPendingBeforeAnyCallback()
        {
            var type = typeof(TableDdlRequest).Assembly.GetType
            (
                "JasonQuery.Core.Database.DdlPreview.OracleDdlGuiGate", true
            );

            var method = type.GetMethod("TryExecuteF7", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);

            var calls = 0;

            Func<string> callback = () => { calls++; return string.Empty; };

            var result = (string)method.Invoke(null, new object[] { true, callback, callback, callback });

            Assert.AreEqual("Q298_TRANSACTION_PENDING", result);
            Assert.AreEqual(0, calls, "No connection, verification or DDL callback may run.");
        }

        [TestMethod]
        public void G02_ExistingMySqlGateRejectsPendingBeforeAnyCallback()
        {
            var type = typeof(TableDdlRequest).Assembly.GetType
            (
                "JasonQuery.Core.Database.DdlPreview.NonOracleSchemaExplorerDdlExecuteGate", true
            );

            var method = type.GetMethod("TryExecuteF7", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);

            var calls = 0;

            Func<string> callback = () => { calls++; return string.Empty; };
            Func<Guid> readConnection = () => { calls++; return Guid.NewGuid(); };
            Func<Guid, string> dispatch = id => { calls++; return string.Empty; };

            var args = new object[]
            {
                DataSourceType.MySql, true, true, callback, callback, readConnection, dispatch
            };

            var result = (string)method.Invoke(null, args);

            Assert.AreEqual("F7U2C2_TRANSACTION_UNSAFE", result);
            Assert.AreEqual(0, calls, "A Yes/No confirmation must never bypass pending-transaction blocking.");
        }

        //Every Oracle/MySQL Table and Column DDL operation must use the same pending gate, regardless of legacy versus new F7 entry point.
        [TestMethod]
        [DataRow("Oracle", "Table.Comment")]
        [DataRow("Oracle", "Table.Rename")]
        [DataRow("Oracle", "Table.Drop")]
        [DataRow("Oracle", "Table.Truncate")]
        [DataRow("Oracle", "Column.Add")]
        [DataRow("Oracle", "Column.Comment")]
        [DataRow("Oracle", "Column.Rename")]
        [DataRow("Oracle", "Column.Drop")]
        [DataRow("MySql", "Table.Comment")]
        [DataRow("MySql", "Table.Rename")]
        [DataRow("MySql", "Table.Drop")]
        [DataRow("MySql", "Table.Truncate")]
        [DataRow("MySql", "Column.Add")]
        [DataRow("MySql", "Column.Comment")]
        [DataRow("MySql", "Column.Rename")]
        [DataRow("MySql", "Column.Drop")]
        public void G03_AllImplicitCommitDdlOperationsBlockKnownPending(string provider, string operation)
        {
            var decision = Evaluate(provider, operation, true);

            Assert.IsTrue(Flag(decision, "BlockExecute"));
            Assert.IsTrue(Flag(decision, "ShowWarningDialog"));
            Assert.IsTrue(Flag(decision, "RequiresFormReopen"));
            Assert.IsTrue(Flag(decision, "AllowSqlPreview"));
            Assert.AreEqual("S1_TRANSACTION_PENDING", Reason(decision));
        }

        [TestMethod]
        [DataRow("Oracle")]
        [DataRow("MySql")]
        public void G04_UnknownTransactionStateMustFailClosed(string provider)
        {
            var decision = Evaluate(provider, "Table.Drop", null);

            Assert.IsTrue(Flag(decision, "BlockExecute"));
            Assert.IsTrue(Flag(decision, "ShowWarningDialog"));
            Assert.IsTrue(Flag(decision, "RequiresFormReopen"));
            Assert.IsTrue(Flag(decision, "AllowSqlPreview"));
            Assert.AreEqual("S1_TRANSACTION_UNKNOWN", Reason(decision));
        }

        [TestMethod]
        [DataRow("Oracle")]
        [DataRow("MySql")]
        public void G05_VerifiedCleanDoesNotGrantDdlAuthorization(string provider)
        {
            var decision = Evaluate(provider, "Column.Add", false);

            Assert.IsFalse(Flag(decision, "BlockExecute"));
            Assert.IsFalse(Flag(decision, "ShowWarningDialog"));
            Assert.IsFalse(Flag(decision, "RequiresFormReopen"));
            Assert.IsTrue(Flag(decision, "AllowSqlPreview"));
            Assert.AreEqual("S1_CLEAR", Reason(decision));
            //A Clean decision only permits the existing SQL, identity and connection gates to run.
        }

        [TestMethod]
        [DataRow("PostgreSql")]
        [DataRow("SqlServer")]
        public void G06_OtherProvidersRemainOutsideS1Policy(string provider)
        {
            var decision = Evaluate(provider, "Table.Drop", true);

            Assert.IsFalse(Flag(decision, "BlockExecute"));
            Assert.IsFalse(Flag(decision, "ShowWarningDialog"));
            Assert.AreEqual("S1_NOT_APPLICABLE", Reason(decision));
            //PostgreSQL/SQL Server still use their existing independent transaction gates.
        }

        [TestMethod]
        [DataRow("Oracle")]
        [DataRow("MySql")]
        public void G07_UnknownOperationCannotObtainS1Permission(string provider)
        {
            var decision = Evaluate(provider, "Table.UnexpectedOperation", false);

            Assert.IsTrue(Flag(decision, "BlockExecute"));
        }

        [TestMethod]
        public void G08_UnknownProviderCannotObtainS1Permission()
        {
            var decision = Evaluate("None", "Table.Drop", false);

            Assert.IsTrue(Flag(decision, "BlockExecute"));
        }
    }
}
