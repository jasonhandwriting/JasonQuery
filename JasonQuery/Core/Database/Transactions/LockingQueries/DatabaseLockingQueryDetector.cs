using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Sql.Lexing;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.Transactions.LockingQueries
{
    internal static class DatabaseLockingQueryDetector
    {
        private static readonly Regex OracleForUpdateRegex = CreateRegex(@"\bFOR\s+UPDATE\b");

        private static readonly Regex PostgreSqlForNoKeyUpdateRegex = CreateRegex(@"\bFOR\s+NO\s+KEY\s+UPDATE\b");
        private static readonly Regex PostgreSqlForKeyShareRegex = CreateRegex(@"\bFOR\s+KEY\s+SHARE\b");
        private static readonly Regex PostgreSqlForUpdateRegex = CreateRegex(@"\bFOR\s+UPDATE\b");
        private static readonly Regex PostgreSqlForShareRegex = CreateRegex(@"\bFOR\s+SHARE\b");

        private static readonly Regex MySqlLockInShareModeRegex = CreateRegex(@"\bLOCK\s+IN\s+SHARE\s+MODE\b");
        private static readonly Regex MySqlForUpdateRegex = CreateRegex(@"\bFOR\s+UPDATE\b");
        private static readonly Regex MySqlForShareRegex = CreateRegex(@"\bFOR\s+SHARE\b");

        private static readonly Regex SqlServerWithHintRegex = CreateRegex(@"\bWITH\s*\((?<Hints>[^)]*)\)");
        private static readonly Regex SqlServerHintTokenRegex = CreateRegex(@"\b(UPDLOCK|XLOCK|HOLDLOCK|TABLOCKX|SERIALIZABLE)\b");

        private const SqlTokenizerOptions LockingQueryTokenizerOptions = SqlTokenizerOptions.MySqlDashCommentWithoutWhitespace | SqlTokenizerOptions.MySqlBacktickIdentifierAllowsBackslashEscape | SqlTokenizerOptions.MySqlHashStartsCommentInsideWord;

        public static DatabaseLockingQueryDetectionResult Detect(DataSourceType dataSourceType, string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return DatabaseLockingQueryDetectionResult.NotDetected();
            }

            var searchableSql = CreateSearchableSql(sql, dataSourceType);

            searchableSql = LimitToFirstExecutableStatement(searchableSql);

            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return DetectFirst
                        (
                            sql, searchableSql,
                            new[]
                            {
                                CreateRule(OracleForUpdateRegex, DatabaseLockingQueryKind.ForUpdate)
                            }
                        );
                    }
                case DataSourceType.PostgreSql:
                    {
                        return DetectFirst
                        (
                            sql,
                            searchableSql,
                            new[]
                            {
                                CreateRule(PostgreSqlForNoKeyUpdateRegex, DatabaseLockingQueryKind.ForNoKeyUpdate),
                                CreateRule(PostgreSqlForKeyShareRegex, DatabaseLockingQueryKind.ForKeyShare),
                                CreateRule(PostgreSqlForUpdateRegex, DatabaseLockingQueryKind.ForUpdate),
                                CreateRule(PostgreSqlForShareRegex, DatabaseLockingQueryKind.ForShare)
                            }
                        );
                    }
                case DataSourceType.MySql:
                    {
                        return DetectFirst
                        (
                            sql,
                            searchableSql,
                            new[]
                            {
                                CreateRule(MySqlLockInShareModeRegex, DatabaseLockingQueryKind.LockInShareMode),
                                CreateRule(MySqlForUpdateRegex, DatabaseLockingQueryKind.ForUpdate),
                                CreateRule(MySqlForShareRegex, DatabaseLockingQueryKind.ForShare)
                            }
                        );
                    }
                case DataSourceType.SqlServer:
                    {
                        return DetectSqlServerTableHint(sql, searchableSql);
                    }
                default:
                    {
                        return DatabaseLockingQueryDetectionResult.NotDetected();
                    }
            }
        }

        private static DatabaseLockingQueryDetectionResult DetectFirst(string originalSql, string searchableSql, IEnumerable<DetectionRule> rules)
        {
            DatabaseLockingQueryDetectionResult bestResult = null;

            foreach (var rule in rules)
            {
                var match = rule.Pattern.Match(searchableSql);

                if (!match.Success)
                {
                    continue;
                }

                var candidate = CreateDetectedResult(originalSql, match.Index, match.Length, rule.Kind);

                if (bestResult == null || candidate.MatchedPosition < bestResult.MatchedPosition)
                {
                    bestResult = candidate;
                }
            }

            return bestResult ?? DatabaseLockingQueryDetectionResult.NotDetected();
        }

        private static DatabaseLockingQueryDetectionResult DetectSqlServerTableHint(string originalSql, string searchableSql)
        {
            DatabaseLockingQueryDetectionResult bestResult = null;

            foreach (Match withMatch in SqlServerWithHintRegex.Matches(searchableSql))
            {
                var hintsGroup = withMatch.Groups["Hints"];

                foreach (Match hintMatch in SqlServerHintTokenRegex.Matches(hintsGroup.Value))
                {
                    var absolutePosition = hintsGroup.Index + hintMatch.Index;
                    var kind = GetSqlServerHintKind(hintMatch.Value);

                    if (kind == DatabaseLockingQueryKind.None)
                    {
                        continue;
                    }

                    var candidate = CreateDetectedResult(originalSql, absolutePosition, hintMatch.Length, kind);

                    if (bestResult == null || candidate.MatchedPosition < bestResult.MatchedPosition)
                    {
                        bestResult = candidate;
                    }
                }
            }

            return bestResult ?? DatabaseLockingQueryDetectionResult.NotDetected();
        }

        private static DatabaseLockingQueryKind GetSqlServerHintKind(string hint)
        {
            switch ((hint ?? string.Empty).ToUpperInvariant())
            {
                case "UPDLOCK":
                    {
                        return DatabaseLockingQueryKind.SqlServerUpdateLock;
                    }
                case "XLOCK":
                    {
                        return DatabaseLockingQueryKind.SqlServerExclusiveLock;
                    }
                case "HOLDLOCK":
                    {
                        return DatabaseLockingQueryKind.SqlServerHoldLock;
                    }
                case "TABLOCKX":
                    {
                        return DatabaseLockingQueryKind.SqlServerTableExclusiveLock;
                    }
                case "SERIALIZABLE":
                    {
                        return DatabaseLockingQueryKind.SqlServerSerializable;
                    }
                default:
                    {
                        return DatabaseLockingQueryKind.None;
                    }
            }
        }

        private static DatabaseLockingQueryDetectionResult CreateDetectedResult(string originalSql, int position, int length, DatabaseLockingQueryKind kind)
        {
            var matchedText = string.Empty;

            if (!string.IsNullOrEmpty(originalSql) && position >= 0 && length > 0 && position + length <= originalSql.Length)
            {
                matchedText = originalSql.Substring(position, length);
            }

            return new DatabaseLockingQueryDetectionResult
            {
                IsLockingQuery = true,
                Kind = kind,
                MatchedText = matchedText,
                MatchedPosition = position
            };
        }

        private static DetectionRule CreateRule(Regex pattern, DatabaseLockingQueryKind kind)
        {
            return new DetectionRule
            {
                Pattern = pattern,
                Kind = kind
            };
        }

        private static Regex CreateRegex(string pattern)
        {
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
        }

        private static string LimitToFirstExecutableStatement(string searchableSql)
        {
            if (string.IsNullOrEmpty(searchableSql))
            {
                return string.Empty;
            }

            var chars = searchableSql.ToCharArray();
            var statementStart = 0;

            while (statementStart < chars.Length && (char.IsWhiteSpace(chars[statementStart]) || chars[statementStart] == ';'))
            {
                if (chars[statementStart] == ';')
                {
                    chars[statementStart] = ' ';
                }

                statementStart++;
            }

            if (statementStart >= chars.Length)
            {
                return new string(chars);
            }

            var statementEnd = Array.IndexOf(chars, ';', statementStart);

            if (statementEnd < 0)
            {
                return new string(chars);
            }

            for (var index = statementEnd; index < chars.Length; index++)
            {
                MaskCharacter(chars, index);
            }

            return new string(chars);
        }

        private static string CreateSearchableSql(string sql, DataSourceType dataSourceType)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return string.Empty;
            }

            var chars = sql.ToCharArray();
            var tokenizationResult = SqlTokenizer.Tokenize(sql, dataSourceType, LockingQueryTokenizerOptions);

            foreach (var token in tokenizationResult.Tokens)
            {
                if (!token.SuppressesKeywordMatching)
                {
                    continue;
                }

                MaskRange(chars, token.Start, token.EndExclusive);
            }

            return new string(chars);
        }

        internal static string CreateSearchableSqlForGoldenTest(DataSourceType dataSourceType, string sql)
        {
            return CreateSearchableSql(sql ?? string.Empty, dataSourceType);
        }

        private static void MaskRange(char[] chars, int start, int endExclusive)
        {
            for (var index = Math.Max(0, start); index < Math.Min(chars.Length, endExclusive); index++)
            {
                MaskCharacter(chars, index);
            }
        }

        private static void MaskCharacter(char[] chars, int index)
        {
            if (index < 0 || index >= chars.Length)
            {
                return;
            }

            if (chars[index] != '\r' && chars[index] != '\n')
            {
                chars[index] = ' ';
            }
        }

        private sealed class DetectionRule
        {
            public Regex Pattern { get; set; }

            public DatabaseLockingQueryKind Kind { get; set; }
        }
    }
}
