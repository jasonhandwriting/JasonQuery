using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Transactions.LockingQueries;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private sealed class QueryExecutionKindResult
        {
            public bool IsQuery { get; set; }
            public int QueryCount { get; set; }
            public bool IsCommitRollbackScript { get; set; }
            public bool IsExtended { get; set; }
            public bool IgnoreCommitRollback { get; set; }

            public DatabaseLockingQueryDetectionResult LockingQueryDetection { get; set; }
                   = DatabaseLockingQueryDetectionResult.NotDetected();

            public bool IsLockingQuery
            {
                get
                {
                    return LockingQueryDetection?.IsLockingQuery ?? false;
                }
            }

            public bool HasExecutableStatement => QueryCount > 0;
        }

        private sealed class SqlStatementInfo
        {
            public int QueryCount { get; set; }
            public string StatementType { get; set; } = string.Empty;
            public string StatementText { get; set; } = string.Empty;
        }

        private QueryExecutionKindResult AnalyzeQueryExecutionKind(string queryText)
        {
            var isQuery = IsQuery
            (
                queryText,
                out var queryCount,
                out var isCommitRollbackScript,
                out var isExtended,
                out var ignoreCommitRollback
            );

            var lockingQueryDetection = isQuery ? DatabaseLockingQueryDetector.Detect(_currentSourceType, queryText)
                                        : DatabaseLockingQueryDetectionResult.NotDetected();

            return new QueryExecutionKindResult
            {
                IsQuery = isQuery,
                QueryCount = queryCount,
                IsCommitRollbackScript = isCommitRollbackScript,
                IsExtended = isExtended,
                IgnoreCommitRollback = ignoreCommitRollback,
                LockingQueryDetection = lockingQueryDetection
            };
        }

        private bool IsQuery(string sql, out int queryCount, out bool isCommitRollback, out bool isExtended, out bool isIgnoreCommitRollback)
        {
            queryCount = 0;
            isCommitRollback = false;
            isExtended = false;
            isIgnoreCommitRollback = false;

            if (string.IsNullOrWhiteSpace(sql))
            {
                return false;
            }

            if (!TryGetFirstSqlStatementInfo(sql, out var statementInfo))
            {
                return false;
            }

            queryCount = statementInfo.QueryCount;

            var statementType = statementInfo.StatementType;
            var statementText = statementInfo.StatementText.TrimStart();
            var result = false;

            if (string.Equals(statementType, "SELECT", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
            }
            else if (string.Equals(statementType, "WITH", StringComparison.OrdinalIgnoreCase))
            {
                result = IsWithSql(sql);
            }
            else if (statementText.StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase)
                     && string.Equals(statementType, "BATCH", StringComparison.OrdinalIgnoreCase))
            {
                result = IsWithSql(sql);
                isExtended = result;
            }

            if (IsSqlServer && IsSqlServerUseOrGoStatement(statementText))
            {
                isIgnoreCommitRollback = true;
            }

            if (IsSqlServer && statementText.StartsWith("EXEC", StringComparison.OrdinalIgnoreCase))
            {
                //EXEC 指令視為 Extended，不走一般分頁查詢
                result = false;
                isExtended = true;

                var cleanedStatement = Regex.Replace(statementText, @"\s+", " ").Trim();

                if (cleanedStatement.StartsWith("EXEC XP_MSVER", StringComparison.OrdinalIgnoreCase))
                {
                    result = true;
                    isExtended = true;
                }
            }

            if (IsMySql && statementText.StartsWith("USE", StringComparison.OrdinalIgnoreCase))
            {
                isIgnoreCommitRollback = true;
            }

            if ((IsMySql || IsPostgreSql) && statementText.StartsWith("SHOW", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                isExtended = true;
            }

            if (IsPostgreSql && statementText.StartsWith("EXPLAIN", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                isExtended = true;
            }

            if (IsCommitOrRollbackStatement(statementType, statementText))
            {
                isCommitRollback = true;
            }

            return result;
        }

        private bool TryGetFirstSqlStatementInfo(string sql, out SqlStatementInfo statementInfo)
        {
            statementInfo = null;

            if (IsSqlServer)
            {
                return TryGetFirstSqlServerStatementInfo(sql, out statementInfo);
            }

            return TryGetFirstStatementInfo(sql, out statementInfo);
        }

        private static bool TryGetFirstStatementInfo(string sql, out SqlStatementInfo statementInfo)
        {
            statementInfo = null;

            using (var script = new Devart.Data.Oracle.OracleScript(sql))
            {
                if (script.Statements.Count == 0)
                {
                    return false;
                }

                statementInfo = new SqlStatementInfo
                {
                    QueryCount = script.Statements.Count,
                    StatementType = script.Statements[0].StatementType.ToString(),
                    StatementText = script.Statements[0].Text
                };

                return true;
            }
        }

        private static bool TryGetFirstSqlServerStatementInfo(string sql, out SqlStatementInfo statementInfo)
        {
            statementInfo = null;

            using (var script = new Devart.Data.SqlServer.SqlScript(sql))
            {
                if (script.Statements.Count == 0)
                {
                    return false;
                }

                statementInfo = new SqlStatementInfo
                {
                    QueryCount = script.Statements.Count,
                    StatementType = script.Statements[0].StatementType.ToString(),
                    StatementText = script.Statements[0].Text
                };

                return true;
            }
        }

        private static bool IsSqlServerUseOrGoStatement(string statementText)
        {
            return statementText.StartsWith("USE", StringComparison.OrdinalIgnoreCase)
                   || statementText.StartsWith("GO", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCommitOrRollbackStatement(string statementType, string statementText)
        {
            var typeCommitRollback = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "COMMIT",
                "ROLLBACK"
            };

            if (typeCommitRollback.Contains(statementType))
            {
                return true;
            }

            var firstWord = GetFirstSqlWord(statementText);

            return typeCommitRollback.Contains(firstWord);
        }

        private static string GetFirstSqlWord(string statementText)
        {
            if (string.IsNullOrWhiteSpace(statementText))
            {
                return string.Empty;
            }

            var match = Regex.Match(statementText.TrimStart(), @"^[A-Za-z_]+");

            return match.Success ? match.Value.ToUpperInvariant() : string.Empty;
        }
    }
}
