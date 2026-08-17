using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationTestTransactionFixture : IDisposable
    {
        private bool _prepared;

        public IntegrationTestTransactionFixture(IntegrationTestDatabaseSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Dialect = IntegrationTestTransactionSqlDialect.Create(settings);
            SessionA = new IntegrationTestReaderClient(settings);
            SessionB = new IntegrationTestReaderClient(settings);
        }

        public IntegrationTestDatabaseSettings Settings { get; private set; }
        public IntegrationTestTransactionSqlDialect Dialect { get; private set; }
        public IntegrationTestReaderClient SessionA { get; private set; }
        public IntegrationTestReaderClient SessionB { get; private set; }

        public void Prepare()
        {
            ConnectRequired(SessionA, "A");
            DropIfPresent(SessionA, Dialect.BuildDropTransactionTableSql());
            ExecuteDdlAndReconnect(SessionA, Dialect.BuildCreateTransactionTableSql());
            ExecuteRequired(SessionA, Dialect.BuildInsertSql(1, "Original"));
            CommitAndReconnect(SessionA);
            ConnectRequired(SessionB, "B");
            _prepared = true;
        }

        public void ReconnectSessionA()
        {
            Reconnect(SessionA, "A");
        }

        public void ReconnectSessionB()
        {
            Reconnect(SessionB, "B");
        }

        public void DropDdlTableIfPresent()
        {
            DropIfPresent(SessionA, Dialect.BuildDropDdlTableSql());
        }

        public string ReadName(IntegrationTestReaderClient client, int id)
        {
            var result = client.ExecutePaged(Dialect.BuildSelectByIdSql(id), 0, 1);

            if (!string.IsNullOrEmpty(result.ErrorMessage) || result.Data == null || result.Data.Rows.Count == 0)
            {
                return null;
            }

            return Convert.ToString(result.Data.Rows[0]["NAME"]);
        }

        public bool RowExists(IntegrationTestReaderClient client, int id)
        {
            return ReadName(client, id) != null;
        }

        public string ReadObserverName(IntegrationTestReaderClient client, int id)
        {
            var result = client.ExecutePaged(Dialect.BuildObserverSelectByIdSql(id), 0, 1);

            if (!string.IsNullOrEmpty(result.ErrorMessage) || result.Data == null || result.Data.Rows.Count == 0)
            {
                return null;
            }

            return Convert.ToString(result.Data.Rows[0]["NAME"]);
        }

        public bool ObserverRowExists(IntegrationTestReaderClient client, int id)
        {
            return ReadObserverName(client, id) != null;
        }

        public bool DdlTableExists(IntegrationTestReaderClient client)
        {
            var result = client.ExecutePaged($"SELECT * FROM {Dialect.DdlTable}", 0, 1);
            return string.IsNullOrEmpty(result.ErrorMessage);
        }

        public void Dispose()
        {
            try
            {
                SessionA.Rollback();
                SessionB.Rollback();
                SessionA.Disconnect();
                SessionB.Disconnect();

                if (_prepared)
                {
                    var message = SessionA.Connect();

                    if (string.IsNullOrEmpty(message))
                    {
                        DropIfPresent(SessionA, Dialect.BuildDropDdlTableSql());
                        DropIfPresent(SessionA, Dialect.BuildDropTransactionTableSql());
                    }
                }
            }
            finally
            {
                SessionB.Dispose();
                SessionA.Dispose();
            }
        }

        private void ExecuteRequired(IntegrationTestReaderClient client, string sql)
        {
            client.ExecuteNonQuery(sql, out var errorMessage);
            Assert.AreEqual(string.Empty, errorMessage, $"SQL failed for {Settings.SourceType}:\r\n{errorMessage}\r\n\r\n{sql}");
        }

        private void ExecuteDdlAndReconnect(IntegrationTestReaderClient client, string sql)
        {
            client.ExecuteNonQuery(sql, out var errorMessage, out var pending, out var closed);
            Assert.AreEqual(string.Empty, errorMessage, $"DDL failed for {Settings.SourceType}:\r\n{errorMessage}\r\n\r\n{sql}");

            if (!closed)
            {
                Assert.AreEqual(string.Empty, client.Commit(), $"DDL commit failed for {Settings.SourceType}.");
            }

            Reconnect(client, "DDL");
        }

        private void DropIfPresent(IntegrationTestReaderClient client, string sql)
        {
            client.ExecuteNonQuery(sql, out var errorMessage, out var pending, out var closed);

            if (!string.IsNullOrEmpty(errorMessage) && !Dialect.IsExpectedDropMissingError(errorMessage))
            {
                Assert.Fail($"Unable to remove integration-test table for {Settings.SourceType}: {errorMessage}");
            }

            if (string.IsNullOrEmpty(errorMessage) && !closed)
            {
                Assert.AreEqual(string.Empty, client.Commit());
            }

            Reconnect(client, "Drop");
        }

        private void CommitAndReconnect(IntegrationTestReaderClient client)
        {
            Assert.AreEqual(string.Empty, client.Commit(), $"Commit failed for {Settings.SourceType}.");
            Reconnect(client, "Commit");
        }

        private void Reconnect(IntegrationTestReaderClient client, string sessionName)
        {
            client.Disconnect();
            ConnectRequired(client, sessionName);
        }

        private void ConnectRequired(IntegrationTestReaderClient client, string sessionName)
        {
            var errorMessage = client.Connect();
            Assert.AreEqual(string.Empty, errorMessage, $"Unable to connect Session {sessionName} to {Settings.SourceType}: {errorMessage}");
            Assert.AreEqual(ConnectionState.Open, client.State);
        }
    }
}
