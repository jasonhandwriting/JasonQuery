using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationTestSpecialTypeFixture : IDisposable
    {
        private readonly IntegrationTestReaderClient _client;
        private readonly IntegrationTestSpecialTypeSqlDialect _dialect;
        private bool _isPrepared;

        public IntegrationTestSpecialTypeFixture(IntegrationTestDatabaseSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _client = new IntegrationTestReaderClient(settings);
            _dialect = IntegrationTestSpecialTypeSqlDialect.Create(settings);
        }

        public IntegrationTestDatabaseSettings Settings { get; private set; }
        public IntegrationTestReaderClient Client => _client;
        public IntegrationTestSpecialTypeSqlDialect Dialect => _dialect;

        public void Prepare()
        {
            Settings.RequireConfigured();
            ConnectRequired();
            DropTablesIfPresent();
            _isPrepared = true;

            foreach (var sql in _dialect.BuildCreateTableSqls())
            {
                ExecuteDdlAndReconnect(sql);
            }

            foreach (var sql in _dialect.BuildInsertSqls())
            {
                ExecuteRequired(sql);
            }

            CommitAndReconnect();
        }

        public IntegrationTestPagedQueryResult ExecuteMainQuery()
        {
            return _client.ExecutePaged(_dialect.BuildMainSelectSql(), 0, 10);
        }

        public IntegrationTestPagedQueryResult ExecuteOracleLongQuery()
        {
            return _client.ExecutePaged(_dialect.BuildOracleLongSelectSql(), 0, 10);
        }

        public IntegrationTestPagedQueryResult ExecuteOracleLongRawQuery()
        {
            return _client.ExecutePaged(_dialect.BuildOracleLongRawSelectSql(), 0, 10);
        }

        public void Dispose()
        {
            try
            {
                _client.Rollback();
                _client.Disconnect();

                if (!_isPrepared)
                {
                    return;
                }

                var connectError = _client.Connect();

                if (!string.IsNullOrEmpty(connectError))
                {
                    return;
                }

                DropTablesIfPresent();
            }
            finally
            {
                _client.Dispose();
            }
        }

        private void ConnectRequired()
        {
            var errorMessage = _client.Connect();

            Assert.AreEqual(string.Empty, errorMessage, $"Unable to connect to {Settings.SourceType}: {errorMessage}");
            Assert.AreEqual(ConnectionState.Open, _client.State);
        }

        private void ExecuteRequired(string sql)
        {
            _client.ExecuteNonQuery(sql, out var errorMessage);

            Assert.AreEqual(string.Empty, errorMessage, $"SQL failed for {Settings.SourceType}:\r\n{errorMessage}\r\n\r\n{sql}");
        }

        private void ExecuteDdlAndReconnect(string sql)
        {
            _client.ExecuteNonQuery(sql, out var errorMessage, out var hasPendingTransactionAfterExecute, out var isTransactionClosedBySqlCommand);

            Assert.AreEqual(string.Empty, errorMessage, $"DDL failed for {Settings.SourceType}:\r\n{errorMessage}\r\n\r\n{sql}");

            FinalizeDdlAndReconnect(isTransactionClosedBySqlCommand);
        }

        private void DropTablesIfPresent()
        {
            foreach (var sql in _dialect.BuildDropTableSqls())
            {
                _client.ExecuteNonQuery(sql, out var errorMessage, out var hasPendingTransactionAfterExecute, out var isTransactionClosedBySqlCommand);

                if (!string.IsNullOrEmpty(errorMessage) && !_dialect.IsExpectedDropMissingError(errorMessage))
                {
                    Assert.Fail($"Unable to remove an integration-test table for {Settings.SourceType}: {errorMessage}\r\n\r\n{sql}");
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    _client.Disconnect();
                    ConnectRequired();
                    continue;
                }

                if (!isTransactionClosedBySqlCommand)
                {
                    var commitMessage = _client.Commit();

                    Assert.AreEqual(string.Empty, commitMessage, $"DDL commit failed for {Settings.SourceType}: {commitMessage}");
                }

                Assert.IsTrue(_client.Disconnect(), $"Disconnect failed for {Settings.SourceType}.");
                ConnectRequired();
            }
        }

        private void FinalizeDdlAndReconnect(bool isTransactionClosedBySqlCommand)
        {
            if (!isTransactionClosedBySqlCommand)
            {
                var commitMessage = _client.Commit();

                Assert.AreEqual(string.Empty, commitMessage, $"DDL commit failed for {Settings.SourceType}: {commitMessage}");
            }

            Assert.IsTrue(_client.Disconnect(), $"Disconnect failed for {Settings.SourceType}.");
            ConnectRequired();
        }

        private void CommitAndReconnect()
        {
            var commitMessage = _client.Commit();

            Assert.AreEqual(string.Empty, commitMessage, $"Commit failed for {Settings.SourceType}: {commitMessage}");
            Assert.IsTrue(_client.Disconnect(), $"Disconnect failed for {Settings.SourceType}.");
            ConnectRequired();
        }
    }
}