using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationTestTableFixture : IDisposable
    {
        private readonly IntegrationTestReaderClient _client;
        private readonly IntegrationTestSqlDialect _dialect;
        private bool _isPrepared;

        public IntegrationTestTableFixture(IntegrationTestDatabaseSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _client = new IntegrationTestReaderClient(settings);
            _dialect = IntegrationTestSqlDialect.Create(settings);
        }

        public IntegrationTestDatabaseSettings Settings { get; private set; }
        public IntegrationTestReaderClient Client => _client;
        public IntegrationTestSqlDialect Dialect => _dialect;

        public void Prepare()
        {
            ConnectRequired();
            DropTableIfPresent(true);
            ExecuteDdlAndReconnect(_dialect.BuildCreateTableSql());
            _isPrepared = true;

            for (var id = 1; id <= 5; id++)
            {
                ExecuteRequired(_dialect.BuildInsertSql(id));
            }

            CommitAndReconnect();
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

                DropTableIfPresent(false);
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

        private void DropTableIfPresent(bool reconnectAfterDrop)
        {
            _client.ExecuteNonQuery(_dialect.BuildDropTableSql(), out var errorMessage, out var hasPendingTransactionAfterExecute,
                                    out var isTransactionClosedBySqlCommand);

            if (!string.IsNullOrEmpty(errorMessage) && !_dialect.IsExpectedDropMissingError(errorMessage))
            {
                Assert.Fail($"Unable to remove the integration-test table for {Settings.SourceType}: {errorMessage}");
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                _client.Disconnect();

                if (reconnectAfterDrop)
                {
                    ConnectRequired();
                }

                return;
            }

            if (!isTransactionClosedBySqlCommand)
            {
                var commitMessage = _client.Commit();

                Assert.AreEqual(string.Empty, commitMessage, $"DDL commit failed for {Settings.SourceType}: {commitMessage}");
            }

            Assert.IsTrue(_client.Disconnect(), $"Disconnect failed for {Settings.SourceType}.");

            if (reconnectAfterDrop)
            {
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