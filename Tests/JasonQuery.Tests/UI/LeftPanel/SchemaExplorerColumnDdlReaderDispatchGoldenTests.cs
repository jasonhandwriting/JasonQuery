using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C2B2: Reader API contract only; never opens a database or dispatches SQL.
    //The operation-specific SQL gate is covered by the existing C313/C314 and B401-B408 Goldens.
    [TestClass]
    [TestCategory("F7U3C2B2")]
    public sealed class SchemaExplorerColumnDdlReaderDispatchGoldenTests
    {
        private static MethodInfo RequireColumnEntry(Type readerType, bool postgreSql)
        {
            var parameters = postgreSql
                ? new[]
                {
                    typeof(string), typeof(Guid), typeof(ColumnDdlOperation),
                    typeof(string).MakeByRefType(), typeof(bool).MakeByRefType(),
                    typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType()
                }
                : new[]
                {
                    typeof(string), typeof(Guid), typeof(ColumnDdlOperation),
                    typeof(string).MakeByRefType(), typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType()
                };

            var method = readerType.GetMethod
            (
                "ExecuteColumnDdlOnVerifiedConnection",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                parameters,
                null
            );

            Assert.IsNotNull(method, readerType.Name + " must expose an operation-aware Existing Column entry, separate from Add Column.");
            Assert.AreEqual(typeof(int), method.ReturnType);
            Assert.IsTrue(method.IsAssembly, readerType.Name + " must not expose the new privileged entry publicly.");

            return method;
        }

        [TestMethod]
        public void B501_PostgreSqlColumnEntryRetainsSavepointAndDispatchReceipt()
        {
            Assert.IsNotNull(RequireColumnEntry(typeof(PostgreSqlReader), true));
        }

        [TestMethod]
        public void B502_SqlServerColumnEntryRequiresNamedOperation()
        {
            Assert.IsNotNull(RequireColumnEntry(typeof(SqlServerReader), false));
        }

        [TestMethod]
        public void B503_MySqlColumnEntryRequiresNamedOperation()
        {
            Assert.IsNotNull(RequireColumnEntry(typeof(MySqlReader), false));
        }

        [TestMethod]
        public void B504_PreexistingAddColumnVerifiedReaderEntriesRemainPresent()
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;

            var types = new[]
            {
                typeof(string), typeof(Guid), typeof(string).MakeByRefType(),
                typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType()
            };

            foreach (var reader in new[] { typeof(PostgreSqlReader), typeof(SqlServerReader), typeof(MySqlReader) })
            {
                Assert.IsNotNull(reader.GetMethod("ExecuteSingleNonQueryOnVerifiedConnection", flags, null, types, null), reader.Name);
            }
        }
    }
}
