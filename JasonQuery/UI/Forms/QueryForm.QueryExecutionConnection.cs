using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Database.Providers.Readers;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private bool TryPrepareQueryExecutionReader()
        {
            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return TryPrepareQueryReader
                        (
                            MyGlobal.OracleReader,
                            nameof(OracleReader),
                            reader => reader.TryEnsureConnectionOpen(out var errorMessage) ? string.Empty : errorMessage,
                            reader =>
                            {
                                reader.QueryCompleted -= QueryCompleted;
                                reader.QueryCompleted += QueryCompleted;
                            }
                        );
                    }

                case DataSourceType.PostgreSql:
                    {
                        return TryPrepareQueryReader
                        (
                            MyGlobal.PostgreSqlReader,
                            nameof(PostgreSqlReader),
                            reader => reader.TryEnsureConnectionOpen(out var errorMessage) ? string.Empty : errorMessage,
                            reader =>
                            {
                                reader.QueryCompleted -= QueryCompleted;
                                reader.QueryCompleted += QueryCompleted;
                            }
                        );
                    }

                case DataSourceType.SqlServer:
                    {
                        return TryPrepareQueryReader
                        (
                            MyGlobal.SqlServerReader,
                            nameof(SqlServerReader),
                            reader => reader.TryEnsureConnectionOpen(out var errorMessage) ? string.Empty : errorMessage,
                            reader =>
                            {
                                reader.QueryCompleted -= QueryCompleted;
                                reader.QueryCompleted += QueryCompleted;
                            }
                        );
                    }

                case DataSourceType.MySql:
                    {
                        return TryPrepareQueryReader
                        (
                            MyGlobal.MySqlReader,
                            nameof(MySqlReader),
                            reader => reader.TryEnsureConnectionOpen(out var errorMessage) ? string.Empty : errorMessage,
                            reader =>
                            {
                                reader.QueryCompleted -= QueryCompleted;
                                reader.QueryCompleted += QueryCompleted;
                            }
                        );
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryPrepareQueryReader<TReader>(TReader reader, string readerName, Func<TReader, string> ensureConnection,
                                                    Action<TReader> subscribeCompleted) where TReader : class
        {
            if (reader == null)
            {
                throw new InvalidOperationException($"{readerName} has not been initialized.");
            }

            var errorMessage = ensureConnection(reader);

            if (!ShowQueryConnectionErrorIfNeeded(errorMessage))
            {
                return false;
            }

            subscribeCompleted(reader);
            return true;
        }

        private bool ShowQueryConnectionErrorIfNeeded(string result)
        {
            if (string.IsNullOrEmpty(result))
            {
                return true;
            }

            MessageBox.Show(result, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }
    }
}
