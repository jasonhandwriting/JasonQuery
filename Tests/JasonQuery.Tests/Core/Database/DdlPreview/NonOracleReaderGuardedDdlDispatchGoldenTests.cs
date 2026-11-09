using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    //F7-U2-C3B: these tests MUST NEVER connect to a database or execute DDL.
    //They verify the guard's rejection path, not live target validation.
    [TestClass]
    [TestCategory("F7U2C3B")]
    public sealed class NonOracleReaderGuardedDdlDispatchGoldenTests
    {
        private static void AssertClosedReaderRejected(Type readerType)
        {
            var reader = Activator.CreateInstance(readerType);
            var property = readerType.GetProperty("CurrentOpenConnectionId", BindingFlags.Instance | BindingFlags.Public);

            Assert.IsNotNull(property);
            Assert.AreEqual(Guid.Empty, (Guid)property.GetValue(reader));

            var method = readerType.GetMethod
            (
                "ExecuteSingleNonQueryOnVerifiedConnection",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(string), typeof(Guid), typeof(string).MakeByRefType(), typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType() },
                null
            );

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(int), method.ReturnType);

            //If this invokes ConnectTo() or EnsureConnectionOpen(), test would attempt a real connection.
            var args = new object[]
            {
                "ALTER TABLE public.t ADD COLUMN c integer;",
                Guid.Parse("65922047-ac45-4da2-85b4-dcdd831fda83"),
                null, false, false
            };

            var count = method.Invoke(reader, args);

            Assert.AreEqual(0, count);
            Assert.AreEqual("F7U2C3B_CONNECTION_CHANGED", args[2]);
            Assert.AreEqual(false, args[3]);
            Assert.AreEqual(false, args[4]);
            Assert.AreEqual(ConnectionState.Closed, readerType.GetMethod("GetState").Invoke(reader, null));
            Assert.AreEqual(Guid.Empty, (Guid)property.GetValue(reader));
        }

        [TestMethod]
        public void B301_PostgreSqlClosedReaderRejectsWithoutReconnect()
        {
            AssertClosedReaderRejected(typeof(PostgreSqlReader));
        }

        [TestMethod]
        public void B302_SqlServerClosedReaderRejectsWithoutReconnect()
        {
            AssertClosedReaderRejected(typeof(SqlServerReader));
        }

        [TestMethod]
        public void B303_MySqlClosedReaderRejectsWithoutReconnect()
        {
            AssertClosedReaderRejected(typeof(MySqlReader));
        }
    }
}
