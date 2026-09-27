using JasonQuery.Database.Internal.Runtime;
using JasonQuery.Database.Internal.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace JasonQuery.Tests.Database.Internal.Security
{
    [TestClass]
    public class ConnectionCredentialStorageRuntimeValidatorTests
    {
        [TestMethod]
        public void EnsureV2Ready_NullRuntime_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ConnectionCredentialStorageRuntimeValidator.EnsureV2Ready(null, "Data Source=x", "key"));
        }

        [TestMethod]
        public void EnsureV2Ready_NullTable_FailsClosed()
        {
            Assert.ThrowsExactly<InvalidDataException>(() => ConnectionCredentialStorageRuntimeValidator.EnsureV2Ready(new FakeRuntime(null), "Data Source=x", "key"));
        }

        [TestMethod]
        public void EnsureV2Ready_MissingMarker_FailsClosed()
        {
            Assert.ThrowsExactly<InvalidDataException>(() => ConnectionCredentialStorageRuntimeValidator.EnsureV2Ready(new FakeRuntime(CreateTable()), "Data Source=x", "key"));
        }

        [TestMethod]
        public void EnsureV2Ready_DuplicateMarker_FailsClosed()
        {
            var table = CreateTable("2", "2");

            Assert.ThrowsExactly<InvalidDataException>(() => ConnectionCredentialStorageRuntimeValidator.EnsureV2Ready(new FakeRuntime(table), "Data Source=x", "key"));
        }

        [TestMethod]
        public void EnsureV2Ready_LegacyMarker_FailsClosed()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => ConnectionCredentialStorageRuntimeValidator.EnsureV2Ready(new FakeRuntime(CreateTable("1")), "Data Source=x", "key"));
        }

        [TestMethod]
        public void EnsureV2Ready_CurrentMarker_PassesReadOnlyQuery()
        {
            var runtime = new FakeRuntime(CreateTable("2"));

            ConnectionCredentialStorageRuntimeValidator.EnsureV2Ready(runtime, "Data Source=x", "key");
            Assert.Contains("ConnectionCredentialStorageVersion", runtime.LastSql);
            Assert.AreEqual(1, runtime.QueryCount);
        }

        private static DataTable CreateTable(params string[] values)
        {
            var table = new DataTable();

            table.Columns.Add("MetadataValue", typeof(string));
            foreach (var value in values) table.Rows.Add(value);
            return table;
        }

        private sealed class FakeRuntime : IJasonQueryDatabaseRuntime
        {
            private readonly DataTable _table;
            public FakeRuntime(DataTable table) { _table = table; }
            public int QueryCount { get; private set; }
            public string LastSql { get; private set; }
            public DataTable ExecuteQuery(string connectionString, string databasePassword, string sql) { QueryCount++; LastSql = sql; return _table; }
            public void ExecuteNonQuery(string connectionString, string databasePassword, string sql, IReadOnlyList<JasonQueryDatabaseParameter> parameters) { throw new AssertFailedException("Validator must be read-only."); }
            public void ExecuteBatchNonQuery(string connectionString, string databasePassword, IReadOnlyList<string> sqlStatements) { throw new AssertFailedException("Validator must be read-only."); }
            public bool CanOpenDatabase(string connectionString, string databasePassword) => true;
            public void ChangePassword(string connectionString, string currentDatabasePassword, string newDatabasePassword) { throw new AssertFailedException("Validator must not rekey."); }
        }
    }
}
