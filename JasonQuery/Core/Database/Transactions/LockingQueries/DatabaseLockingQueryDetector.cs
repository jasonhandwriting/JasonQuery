using JasonQuery.Core.Database.Connection;
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

        public static DatabaseLockingQueryDetectionResult Detect(DataSourceType dataSourceType, string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return DatabaseLockingQueryDetectionResult.NotDetected();
            }

            var searchableSql = MaskNonExecutableText(sql, dataSourceType);

            searchableSql = LimitToFirstExecutableStatement(searchableSql);

            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return DetectFirst(sql, searchableSql,
                                           new[]
                                           {
                                               CreateRule(
                                                   OracleForUpdateRegex,
                                                   DatabaseLockingQueryKind.ForUpdate)
                                           });
                    }
                case DataSourceType.PostgreSql:
                    {
                        return DetectFirst(sql, searchableSql,
                                           new[]
                                           {
                                               CreateRule(
                                                   PostgreSqlForNoKeyUpdateRegex,
                                                   DatabaseLockingQueryKind.ForNoKeyUpdate),
                                               CreateRule(
                                                   PostgreSqlForKeyShareRegex,
                                                   DatabaseLockingQueryKind.ForKeyShare),
                                               CreateRule(
                                                   PostgreSqlForUpdateRegex,
                                                   DatabaseLockingQueryKind.ForUpdate),
                                               CreateRule(
                                                   PostgreSqlForShareRegex,
                                                   DatabaseLockingQueryKind.ForShare)
                                           });
                    }
                case DataSourceType.MySql:
                    {
                        return DetectFirst(sql, searchableSql,
                                           new[]
                                           {
                                               CreateRule(
                                                   MySqlLockInShareModeRegex,
                                                   DatabaseLockingQueryKind.LockInShareMode),
                                               CreateRule(
                                                   MySqlForUpdateRegex,
                                                   DatabaseLockingQueryKind.ForUpdate),
                                               CreateRule(
                                                   MySqlForShareRegex,
                                                   DatabaseLockingQueryKind.ForShare)
                                           });
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

        private static string MaskNonExecutableText(string sql, DataSourceType dataSourceType)
        {
            var chars = sql.ToCharArray();
            var index = 0;

            while (index < chars.Length)
            {
                if (TryMaskLineComment(chars, ref index, dataSourceType))
                {
                    continue;
                }

                if (TryMaskBlockComment(chars, ref index))
                {
                    continue;
                }

                if (dataSourceType == DataSourceType.PostgreSql && TryMaskDollarQuotedText(chars, ref index))
                {
                    continue;
                }

                if (dataSourceType == DataSourceType.Oracle && TryMaskOracleAlternativeQuotedText(chars, ref index))
                {
                    continue;
                }

                if (chars[index] == '\'')
                {
                    MaskDelimitedText(chars, ref index, '\'', '\'', true, dataSourceType == DataSourceType.MySql);

                    continue;
                }

                if (chars[index] == '"')
                {
                    MaskDelimitedText(chars, ref index, '"', '"', true, false);

                    continue;
                }

                if (dataSourceType == DataSourceType.MySql && chars[index] == '`')
                {
                    MaskDelimitedText(chars, ref index, '`', '`', true, true);

                    continue;
                }

                if (dataSourceType == DataSourceType.SqlServer && chars[index] == '[')
                {
                    MaskDelimitedText(chars, ref index, '[', ']', true, false);

                    continue;
                }

                index++;
            }

            return new string(chars);
        }

        private static bool TryMaskLineComment(char[] chars, ref int index, DataSourceType dataSourceType)
        {
            var isDashComment = index + 1 < chars.Length && chars[index] == '-' && chars[index + 1] == '-';

            var isMySqlHashComment = dataSourceType == DataSourceType.MySql && chars[index] == '#';

            if (!isDashComment && !isMySqlHashComment)
            {
                return false;
            }

            while (index < chars.Length && chars[index] != '\r' && chars[index] != '\n')
            {
                chars[index] = ' ';
                index++;
            }

            return true;
        }

        private static bool TryMaskBlockComment(char[] chars, ref int index)
        {
            if (index + 1 >= chars.Length || chars[index] != '/' || chars[index + 1] != '*')
            {
                return false;
            }

            var depth = 0;

            while (index < chars.Length)
            {
                if (index + 1 < chars.Length && chars[index] == '/' && chars[index + 1] == '*')
                {
                    MaskCharacter(chars, index);
                    MaskCharacter(chars, index + 1);
                    index += 2;
                    depth++;
                    continue;
                }

                if (index + 1 < chars.Length && chars[index] == '*' && chars[index + 1] == '/')
                {
                    MaskCharacter(chars, index);
                    MaskCharacter(chars, index + 1);
                    index += 2;
                    depth--;

                    if (depth <= 0)
                    {
                        break;
                    }

                    continue;
                }

                MaskCharacter(chars, index);
                index++;
            }

            return true;
        }

        private static bool TryMaskDollarQuotedText(char[] chars, ref int index)
        {
            if (chars[index] != '$')
            {
                return false;
            }

            if (!TryReadDollarQuoteDelimiter(chars, index, out string delimiter))
            {
                return false;
            }

            var start = index;
            var contentStart = index + delimiter.Length;
            var closingPosition = IndexOf(chars, delimiter, contentStart);
            var endExclusive = closingPosition >= 0 ? closingPosition + delimiter.Length : chars.Length;

            MaskRange(chars, start, endExclusive);

            index = endExclusive;
            return true;
        }

        private static bool TryReadDollarQuoteDelimiter(char[] chars, int start, out string delimiter)
        {
            delimiter = string.Empty;

            if (start < 0 || start >= chars.Length || chars[start] != '$')
            {
                return false;
            }

            var index = start + 1;

            while (index < chars.Length && (char.IsLetterOrDigit(chars[index]) || chars[index] == '_'))
            {
                index++;
            }

            if (index >= chars.Length || chars[index] != '$')
            {
                return false;
            }

            delimiter = new string(chars, start, index - start + 1);

            return true;
        }

        private static bool TryMaskOracleAlternativeQuotedText(char[] chars, ref int index)
        {
            if (index + 2 >= chars.Length)
            {
                return false;
            }

            if (chars[index] != 'q' && chars[index] != 'Q')
            {
                return false;
            }

            if (chars[index + 1] != '\'')
            {
                return false;
            }

            var openingDelimiter = chars[index + 2];
            var closingDelimiter = GetOracleClosingDelimiter(openingDelimiter);
            var searchIndex = index + 3;
            var endExclusive = chars.Length;

            while (searchIndex + 1 < chars.Length)
            {
                if (chars[searchIndex] == closingDelimiter && chars[searchIndex + 1] == '\'')
                {
                    endExclusive = searchIndex + 2;
                    break;
                }

                searchIndex++;
            }

            MaskRange(chars, index, endExclusive);

            index = endExclusive;
            return true;
        }

        private static char GetOracleClosingDelimiter(char openingDelimiter)
        {
            switch (openingDelimiter)
            {
                case '[':
                    {
                        return ']';
                    }
                case '(':
                    {
                        return ')';
                    }
                case '{':
                    {
                        return '}';
                    }
                case '<':
                    {
                        return '>';
                    }
                default:
                    {
                        return openingDelimiter;
                    }
            }
        }

        private static void MaskDelimitedText(char[] chars, ref int index, char openingDelimiter, char closingDelimiter,
                                              bool allowDoubledClosingDelimiter, bool allowBackslashEscape)
        {
            MaskCharacter(chars, index);
            index++;

            while (index < chars.Length)
            {
                if (allowBackslashEscape && chars[index] == '\\' && index + 1 < chars.Length)
                {
                    MaskCharacter(chars, index);
                    MaskCharacter(chars, index + 1);
                    index += 2;
                    continue;
                }

                if (chars[index] != closingDelimiter)
                {
                    MaskCharacter(chars, index);
                    index++;
                    continue;
                }

                if (allowDoubledClosingDelimiter && index + 1 < chars.Length && chars[index + 1] == closingDelimiter)
                {
                    MaskCharacter(chars, index);
                    MaskCharacter(chars, index + 1);
                    index += 2;
                    continue;
                }

                MaskCharacter(chars, index);
                index++;
                break;
            }
        }

        private static int IndexOf(char[] chars, string value, int startIndex)
        {
            if (chars == null || string.IsNullOrEmpty(value) || startIndex < 0)
            {
                return -1;
            }

            for (var index = startIndex; index + value.Length <= chars.Length; index++)
            {
                var matched = true;

                for (var valueIndex = 0; valueIndex < value.Length; valueIndex++)
                {
                    if (chars[index + valueIndex] == value[valueIndex])
                    {
                        continue;
                    }

                    matched = false;
                    break;
                }

                if (matched)
                {
                    return index;
                }
            }

            return -1;
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
