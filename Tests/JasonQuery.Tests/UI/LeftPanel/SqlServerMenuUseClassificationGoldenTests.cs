using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //C1b: the SQL Server menu must send USE as the first SQL token.
    //The existing Reader then raises the database-switch completion signal.
    [TestClass]
    [TestCategory("F7L2B2C1C1B")]
    public sealed class SqlServerMenuUseClassificationGoldenTests
    {
        private static MethodInfo RequireBuilder()
        {
            var method = typeof(QueryForm).GetMethod
            (
                "BuildSqlServerMenuUseSql",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            Assert.IsNotNull(method, "SQL Server menu USE builder is missing.");
            return method;
        }

        private static string Build(string database)
        {
            return (string)RequireBuilder().Invoke(null, new object[] { database });
        }

        [TestMethod]
        public void C1B01_SqlServerMenuCommandStartsWithUseNotComment()
        {
            Assert.AreEqual("USE master", Build("master"));
        }

        [TestMethod]
        public void C1B02_SqlServerMenuCommandPreservesSelectedDatabase()
        {
            Assert.AreEqual("USE MyDB", Build("MyDB"));
        }

        private static bool CallsMethod(MethodInfo caller, MethodInfo target)
        {
            var body = caller.GetMethodBody();

            Assert.IsNotNull(body);

            var il = body.GetILAsByteArray();

            Assert.IsNotNull(il);

            for (var i = 0; i + 4 < il.Length; i++)
            {
                if ((il[i] == 0x28 || il[i] == 0x6F) && BitConverter.ToInt32(il, i + 1) == target.MetadataToken)
                {
                    return true;
                }
            }

            return false;
        }

        [TestMethod]
        public void C1B03_QueryFormMenuDispatchUsesDedicatedCommandBuilder()
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var timer = typeof(QueryForm).GetMethod("tmrMother2Child_Tick", flags);
            var dispatcher = typeof(QueryForm).GetMethod("DispatchSqlServerMenuUseOnce", flags);
            var builder = RequireBuilder();

            Assert.IsNotNull(timer, "SQL Server menu timer is missing.");
            Assert.IsNotNull(dispatcher, "SQL Server one-shot dispatcher is missing.");
            Assert.IsTrue(CallsMethod(timer, dispatcher), "Menu timer must call the one-shot dispatcher.");
            Assert.IsTrue(CallsMethod(dispatcher, builder), "One-shot dispatcher must call the SQL Server USE builder.");
        }

        [TestMethod]
        public void C1B04_ExistingReaderClassifiesUseAsDatabaseSwitch()
        {
            var reader = new SqlServerReader();
            var statementType = string.Empty;
            var result = reader.GetResultString("UNKNOWN", "USE", 0, out statementType, "USE master");

            Assert.AreEqual("Use script - Database changed!", result);
            Assert.AreEqual("Use", statementType);
        }

        [TestMethod]
        public void C1B05_ExistingReaderDoesNotClassifyCommentAsUse()
        {
            var reader = new SqlServerReader();
            var statementType = string.Empty;
            var result = reader.GetResultString("UNKNOWN", "--SWITCH", 0, out statementType);

            Assert.AreNotEqual("Use script - Database changed!", result);
            Assert.AreNotEqual("Use", statementType);
        }
    }
}
