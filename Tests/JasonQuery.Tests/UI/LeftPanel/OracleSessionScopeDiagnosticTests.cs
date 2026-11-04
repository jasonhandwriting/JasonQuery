using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F7L2B2BDIAG")]
    public sealed class OracleSessionScopeDiagnosticTests
    {
        //Exercise the existing private formatter without opening a database connection.
        private static string Format(string code, string detail)
        {
            var method = typeof(OracleReader).GetMethod
            (
                "BuildSessionScopeDiagnosticErrorMessage",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            Assert.IsNotNull(method, "OracleReader must retain the shared diagnostic formatter.");
            Assert.AreEqual(typeof(string), method.ReturnType);
            Assert.IsTrue(method.IsPrivate);
            Assert.IsTrue(method.IsStatic);

            return (string)method.Invoke(null, new object[] { code, detail });
        }

        private static string ScopeSql()
        {
            var field = typeof(OracleReader).GetField
            (
                "SessionScopeSql",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            Assert.IsNotNull(field, "Diagnostic SQL must refer to the Reader's actual verification SQL.");
            Assert.IsTrue(field.IsLiteral);

            return (string)field.GetRawConstantValue();
        }

        [TestMethod]
        public void D01_NoRowsErrorRetainsCodeAndSpecificReason()
        {
            var message = Format("JQ-ORA-SCOPE-001", "The identity query returned no rows.");

            StringAssert.Contains(message, "JQ-ORA-SCOPE-001");
            StringAssert.Contains(message, "The identity query returned no rows.");
        }

        [TestMethod]
        public void D02_MissingValueErrorRetainsCodeAndExactMissingField()
        {
            var message = Format("JQ-ORA-SCOPE-002", "Missing value(s): CON_NAME.");

            StringAssert.Contains(message, "JQ-ORA-SCOPE-002");
            StringAssert.Contains(message, "Missing value(s): CON_NAME.");
        }

        [TestMethod]
        public void D03_DiagnosticSqlMatchesActualReaderVerificationSql()
        {
            var sql = ScopeSql();
            var message = Format("JQ-ORA-SCOPE-001", "No rows.");

            StringAssert.Contains(message, sql + ";");
            Assert.AreEqual(1, message.Split(new[] { sql + ";" }, StringSplitOptions.None).Length - 1);
        }

        [TestMethod]
        public void D04_BothErrorCodesShareTheSameDiagnosticSql()
        {
            var sql = ScopeSql() + ";";
            var noRows = Format("JQ-ORA-SCOPE-001", "No rows.");
            var missing = Format("JQ-ORA-SCOPE-002", "Missing value(s): SESSION_USER, DB_NAME, CON_NAME.");

            StringAssert.Contains(noRows, sql);
            StringAssert.Contains(missing, sql);
            Assert.AreEqual(1, missing.Split(new[] { sql }, StringSplitOptions.None).Length - 1);
        }

        [TestMethod]
        public void D05_ActualDiagnosticQueryIsReadOnlySessionScopeSelect()
        {
            var sql = ScopeSql().Trim();

            Assert.IsTrue(sql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase));
            StringAssert.Contains(sql, "SYS_CONTEXT('USERENV','SESSION_USER')");
            StringAssert.Contains(sql, "SYS_CONTEXT('USERENV','DB_NAME')");
            StringAssert.Contains(sql, "SYS_CONTEXT('USERENV','CON_NAME')");
            Assert.IsTrue(sql.EndsWith(" FROM DUAL", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(sql.Contains(";"), "The verification command must be a single SELECT statement.");
        }

        [TestMethod]
        public void D06_BothErrorsExplainConnectionWasRejectedForSafety()
        {
            foreach (var code in new[] { "JQ-ORA-SCOPE-001", "JQ-ORA-SCOPE-002" })
            {
                StringAssert.Contains(Format(code, "Diagnostic failure."), "rejected this connection for safety");
            }
        }

        [TestMethod]
        public void D07_ExternalTroubleshootingToolsAreNamed()
        {
            var message = Format("JQ-ORA-SCOPE-001", "No rows.");

            StringAssert.Contains(message, "SQL Developer");
            StringAssert.Contains(message, "SQL*Plus");
            StringAssert.Contains(message, "same Oracle account and connection settings");
        }

        [TestMethod]
        public void D08_ExternalSessionIsNotMisrepresentedAsOriginalSession()
        {
            var message = Format("JQ-ORA-SCOPE-002", "Missing value(s): DB_NAME.");

            StringAssert.Contains(message, "separate session");
            StringAssert.Contains(message, "cannot verify the identity of the rejected JasonQuery session");
        }

        [TestMethod]
        public void D09_MessageExplainsWhatToShareWithDba()
        {
            var message = Format("JQ-ORA-SCOPE-001", "No rows.");

            StringAssert.Contains(message, "error code");
            StringAssert.Contains(message, "SQL result");
            StringAssert.Contains(message, "DBA");
        }

        [TestMethod]
        public void D10_MultipleMissingFieldNamesArePreservedWithoutGuessing()
        {
            const string detail = "Missing value(s): SESSION_USER, DB_NAME, CON_NAME.";
            var message = Format("JQ-ORA-SCOPE-002", detail);

            StringAssert.Contains(message, detail);
            Assert.IsFalse(message.Contains("password"));
        }
    }
}
