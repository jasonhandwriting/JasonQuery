using JasonQuery.Core.Config;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F7L2B2C1C1C")]
    public sealed class SqlServerMenuOneShotGoldenTests
    {
        private static MethodInfo FindDispatcher()
        {
            return typeof(QueryForm).GetMethod
            (
                "DispatchSqlServerMenuUseOnce",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
        }

        private static MethodInfo RequireDispatcher()
        {
            var method = FindDispatcher();

            Assert.IsNotNull(method, "SQL Server menu one-shot dispatch method is missing.");
            return method;
        }

        private static int FindOperandOffset(byte[] il, byte opcode, int token)
        {
            for (var i = 0; i + 4 < il.Length; i++)
            {
                if (il[i] == opcode && BitConverter.ToInt32(il, i + 1) == token)
                {
                    return i;
                }
            }

            return -1;
        }

        [TestMethod]
        public void C1C01_MenuHasDedicatedOneShotDispatcher()
        {
            Assert.IsNotNull(FindDispatcher());
        }

        [TestMethod]
        public void C1C02_DispatchConsumesPendingEventBeforeLaunchingExecution()
        {
            var method = RequireDispatcher();
            var body = method.GetMethodBody();

            Assert.IsNotNull(body);

            var il = body.GetILAsByteArray();

            Assert.IsNotNull(il);

            var field = typeof(MyGlobal).GetField
            (
                "GlobalTemp",
                BindingFlags.Static | BindingFlags.Public
            );

            Assert.IsNotNull(field);

            var execute = typeof(QueryForm).GetMethod
            (
                "ExecuteQuery",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.IsNotNull(execute);

            var clearAt = FindOperandOffset(il, 0x80, field.MetadataToken); //stsfld
            var executeAt = FindOperandOffset(il, 0x28, execute.MetadataToken); //call

            if (executeAt < 0)
            {
                executeAt = FindOperandOffset(il, 0x6F, execute.MetadataToken); //callvirt
            }

            Assert.IsTrue(clearAt >= 0, "Dispatch must clear MyGlobal.GlobalTemp.");
            Assert.IsTrue(executeAt > clearAt, "Clear request before launching ExecuteQuery.");
        }

        [TestMethod]
        public void C1C03_MenuTimerUsesOneShotDispatcher()
        {
            var method = RequireDispatcher();

            var timer = typeof(QueryForm).GetMethod
            (
                "tmrMother2Child_Tick",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(timer);

            var body = timer.GetMethodBody();

            Assert.IsNotNull(body);

            var il = body.GetILAsByteArray();

            Assert.IsNotNull(il);

            var callerAt = FindOperandOffset(il, 0x28, method.MetadataToken);

            if (callerAt < 0)
            {
                callerAt = FindOperandOffset(il, 0x6F, method.MetadataToken);
            }

            Assert.IsTrue(callerAt >= 0, "Menu timer must invoke one-shot dispatch.");
        }

        [TestMethod]
        public void C1C04_ExistingSqlServerUseBuilderStillProducesPureUse()
        {
            var builder = typeof(QueryForm).GetMethod
            (
                "BuildSqlServerMenuUseSql",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            Assert.IsNotNull(builder);
            Assert.AreEqual("USE master", (string)builder.Invoke(null, new object[] { "master" }));
        }

        [TestMethod]
        public void C1C05_ExistingSqlServerReaderStillClassifiesUse()
        {
            var reader = new SqlServerReader();
            var type = string.Empty;
            var result = reader.GetResultString("UNKNOWN", "USE", 0, out type, "USE master");

            Assert.AreEqual("Use script - Database changed!", result);
            Assert.AreEqual("Use", type);
        }
    }
}
