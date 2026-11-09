using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    //Q2-98B2: no database connection, DDL statement, or WinForms window is created.
    //Existing Q298 G01-G05 cover orchestration; these tests target real GUI integration seams.
    [TestClass]
    [TestCategory("Q298B2")]
    public sealed class OracleSchemaExplorerGuiIntegrationGoldenTests
    {
        private static MethodInfo CheckMetadataMethod()
        {
            var helper = typeof(TableDdlRequest).Assembly.GetType("JasonQuery.UI.LeftPanel.OracleSchemaExplorerExecuteTimeHelper", false);

            Assert.IsNotNull(helper, "New shared-F7 Oracle execution must use one common Table/Column coordinator.");

            var method = helper.GetMethod("CheckMetadata", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(string), method.ReturnType);

            return method;
        }

        private static string CheckMetadata(bool receiptCurrent = true, bool scopeCurrent = true, string receiptUser = "JQ98XX", string selectedOwner = "JQ98XX", string sql = "DROP TABLE \"JQ98XX\".\"T1\";", string rebuilt = "DROP TABLE \"JQ98XX\".\"T1\";")
        {
            return (string)CheckMetadataMethod().Invoke(null, new object[] { receiptCurrent, scopeCurrent, receiptUser, selectedOwner, sql, rebuilt });
        }

        [TestMethod]
        public void B201_CurrentCompletedLoadAndExactSqlMayProceedToLiveVerification()
        {
            Assert.AreEqual(string.Empty, CheckMetadata());

            //This is only the first check; the Reader must still verify Session/DB/PDB and target.
        }

        [TestMethod]
        public void B202_StaleMetadataOrChangedLogicalScopeBlocksDispatch()
        {
            Assert.AreEqual("Q298_METADATA_STALE", CheckMetadata(receiptCurrent: false));
            Assert.AreEqual("Q298_METADATA_STALE", CheckMetadata(scopeCurrent: false));
        }

        [TestMethod]
        public void B203_OwnerMismatchOrChangedSqlBlocksDispatchWithReason()
        {
            Assert.AreEqual("Q298_OWNER_MISMATCH", CheckMetadata(selectedOwner: "SYSTEM"));
            Assert.AreEqual("Q298_SQL_CHANGED", CheckMetadata(rebuilt: "DROP TABLE \"JQ98XX\".\"T2\";"));
        }

        [TestMethod]
        public void B204_BothNewF7GuiFormsHaveExplicitExecuteTimeOptIn()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

            var tableOptIn = typeof(EditTableForm).GetProperty("RequireOracleF7ExecuteGate", flags);
            var columnOptIn = typeof(EditColumnForm).GetProperty("RequireOracleF7ExecuteGate", flags);

            Assert.IsNotNull(tableOptIn);
            Assert.IsNotNull(columnOptIn);
            Assert.AreEqual(typeof(bool), tableOptIn.PropertyType);
            Assert.AreEqual(typeof(bool), columnOptIn.PropertyType);
        }

        [TestMethod]
        public void B205_ReaderMustHaveLiveScopeAndExactTargetVerifier()
        {
            //This API must query ONLY the open, verified connection, never call EnsureConnectionOpen.
            var method = typeof(OracleReader).GetMethod
            (
                "VerifyF7DdlTargetOnCurrentConnection",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(Guid), typeof(string), typeof(string), typeof(string), typeof(string),
                        typeof(string), typeof(string), typeof(bool) },
                null
            );

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(string), method.ReturnType);
        }
    }
}
