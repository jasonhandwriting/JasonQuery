using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Diagnostics.Common;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Sql.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.Diagnostics.MySql
{
    internal static class MySqlErrorPositionResolver
    {
        private const SqlTokenizerOptions SearchableMapTokenizerOptions = SqlTokenizerOptions.MySqlBacktickIdentifierAllowsBackslashEscape | SqlTokenizerOptions.MySqlDoubleQuotedTextAllowsBackslashEscape | SqlTokenizerOptions.MySqlHashStartsCommentInsideWord | SqlTokenizerOptions.DisableNestedBlockComments;

        private sealed class ExecutionContext
        {
            public int EditorStart { get; set; }

            public int ExecutedCoreStart { get; set; }

            public string OriginalSql { get; set; } = string.Empty;

            public string ExecutedSql { get; set; } = string.Empty;
        }

        private sealed class CandidateMatch
        {
            public bool Found { get; set; }

            public int Position { get; set; }

            public string Text { get; set; } = string.Empty;

            public int Score { get; set; } = int.MaxValue;
        }

        private sealed class LineInfo
        {
            public int Start { get; set; }

            public string Text { get; set; } = string.Empty;
        }

        public static MySqlErrorPositionResolution Resolve(MySqlErrorPositionRequest request)
        {
            var output = new MySqlErrorPositionResolution
            {
                ErrorMessage = request?.ErrorMessage ?? string.Empty
            };

            if (request == null || string.IsNullOrEmpty(request.EditorSql))
            {
                return output;
            }

            var messageInfo = MySqlErrorMessageParser.Parse
            (
                output.ErrorMessage
            );

            output.MessageKind = messageInfo.Kind;

            if (messageInfo.Kind == MySqlErrorMessageKind.NoDatabaseSelected)
            {
                output.ShouldShowSelectDatabaseFirstMessage = true;
                output.PositionResult = CreateReportedPositionFallback(request);
                return output;
            }

            var context = CreateExecutionContext(request);

            if (messageInfo.Kind == MySqlErrorMessageKind.SyntaxError && TryResolveSyntaxError(context, messageInfo, out var syntaxMatch, out var syntaxMatchIsOriginal))
            {
                output.PositionResult = syntaxMatchIsOriginal ? MapOriginalMatchToEditor(context, syntaxMatch) : MapExecutedMatchToEditor(request, context, syntaxMatch);

                return output;
            }

            if (!string.IsNullOrEmpty(messageInfo.TargetText))
            {
                var candidates = BuildTargetCandidates
                (
                    messageInfo,
                    request.CurrentDatabaseName
                );

                if (TryFindBestCandidate(context.OriginalSql, candidates, messageInfo, request.ReportedPosition - context.EditorStart, out var originalMatch))
                {
                    output.PositionResult = MapOriginalMatchToEditor
                    (
                        context,
                        originalMatch
                    );

                    return output;
                }

                if (TryFindBestCandidate(context.ExecutedSql, candidates, messageInfo, request.ReportedPosition - request.ParameterStartPosition, out var executedMatch))
                {
                    output.PositionResult = MapExecutedMatchToEditor
                    (
                        request,
                        context,
                        executedMatch
                    );

                    return output;
                }
            }

            output.PositionResult = CreateReportedPositionFallback(request);
            return output;
        }

        private static ExecutionContext CreateExecutionContext(MySqlErrorPositionRequest request)
        {
            var originalSql = !string.IsNullOrEmpty(request.OriginalExecutedSql) ? request.OriginalExecutedSql : request.ExecutedSql;

            var editorStart = FindExecutionStart
            (
                request.EditorSql,
                originalSql,
                request.PreferredExecutionStart
            );

            var executedCoreStart = FindExecutedCoreStart
            (
                request.ExecutedSql,
                originalSql
            );

            return new ExecutionContext
            {
                EditorStart = Math.Max(0, editorStart),
                ExecutedCoreStart = Math.Max(0, executedCoreStart),
                OriginalSql = originalSql ?? string.Empty,
                ExecutedSql = request.ExecutedSql ?? string.Empty
            };
        }

        private static int FindExecutionStart(string editorSql, string originalSql, int preferredStart)
        {
            if (string.IsNullOrEmpty(editorSql))
            {
                return 0;
            }

            preferredStart = Clamp(preferredStart, 0, editorSql.Length);

            if (string.IsNullOrEmpty(originalSql))
            {
                return preferredStart;
            }

            if (preferredStart + originalSql.Length <= editorSql.Length && string.Equals(editorSql.Substring(preferredStart, originalSql.Length), originalSql, StringComparison.Ordinal))
            {
                return preferredStart;
            }

            var index = editorSql.IndexOf(originalSql, preferredStart, StringComparison.Ordinal);

            if (index >= 0)
            {
                return index;
            }

            index = editorSql.IndexOf(originalSql, StringComparison.Ordinal);
            return index >= 0 ? index : preferredStart;
        }

        private static int FindExecutedCoreStart(string executedSql, string originalSql)
        {
            if (string.IsNullOrEmpty(executedSql))
            {
                return 0;
            }

            if (!string.IsNullOrEmpty(originalSql))
            {
                var exactIndex = executedSql.IndexOf(originalSql, StringComparison.Ordinal);

                if (exactIndex >= 0)
                {
                    return exactIndex;
                }
            }

            var wrapperMatch = Regex.Match
            (
                executedSql,
                @"^\s*SELECT\s+\*\s+FROM\s*\(",
                RegexOptions.IgnoreCase
            );

            return wrapperMatch.Success ? wrapperMatch.Index + wrapperMatch.Length : 0;
        }

        private static bool TryResolveSyntaxError(ExecutionContext context, MySqlErrorMessageInfo info, out CandidateMatch match, out bool matchIsOriginal)
        {
            match = null;
            matchIsOriginal = false;

            if (info.LineNumber <= 0)
            {
                return false;
            }

            //MySQL 的 line number 通常對應實際送出的 SQL。若原始 SQL 仍保有相同行數，優先使用原始 SQL，避免自動分頁／包裝 SQL 讓 Editor 定位到 JasonQuery 產生的外層語句
            if (TryResolveSyntaxErrorInText(context.OriginalSql, info, out match))
            {
                matchIsOriginal = true;
                return true;
            }

            if (TryResolveSyntaxErrorInText(context.ExecutedSql, info, out match))
            {
                matchIsOriginal = false;
                return true;
            }

            return false;
        }

        private static bool TryResolveSyntaxErrorInText(string sql, MySqlErrorMessageInfo info, out CandidateMatch match)
        {
            match = null;

            var lines = SplitLines(sql);

            if (info.LineNumber <= 0 || info.LineNumber > lines.Count)
            {
                return false;
            }

            var line = lines[info.LineNumber - 1];

            var target = ResolveSyntaxLineTarget
            (
                line.Text,
                info.NearText,
                out var positionInLine
            );

            if (string.IsNullOrEmpty(target) || positionInLine < 0)
            {
                return false;
            }

            match = new CandidateMatch
            {
                Found = true,
                Position = line.Start + positionInLine,
                Text = target,
                Score = 0
            };

            return true;
        }

        private static string ResolveSyntaxLineTarget(string line, string nearText, out int position)
        {
            position = -1;

            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(nearText))
            {
                var nearToken = ExtractFirstSyntaxToken(nearText);

                if (!string.IsNullOrEmpty(nearToken))
                {
                    position = line.IndexOf
                    (
                        nearToken,
                        StringComparison.OrdinalIgnoreCase
                    );

                    if (position >= 0)
                    {
                        return nearToken;
                    }
                }
            }

            //MySQL 的 -- 註解要求第二個 - 後面必須是空白或控制字元
            //例如 --, 並不是合法註解，應直接標示該錯誤 Token，而不是把整行第一個 SELECT 當成錯誤位置
            var malformedComment = Regex.Match(line, @"--[^\s]+");

            if (malformedComment.Success)
            {
                position = malformedComment.Index;
                return malformedComment.Value;
            }

            var trimmedStart = line.TakeWhile(char.IsWhiteSpace).Count();
            var trimmed = line.Substring(trimmedStart).TrimEnd();

            if (string.IsNullOrEmpty(trimmed))
            {
                return string.Empty;
            }

            var firstToken = ExtractFirstSyntaxToken(trimmed);

            if (string.IsNullOrEmpty(firstToken))
            {
                return string.Empty;
            }

            position = trimmedStart;
            return firstToken;
        }

        private static string ExtractFirstSyntaxToken(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var trimmed = text.TrimStart();
            var length = 0;

            while (length < trimmed.Length)
            {
                var value = trimmed[length];

                if (char.IsWhiteSpace(value) || value == ';' || value == ',' || value == ')' || value == '(')
                {
                    break;
                }

                length++;
            }

            return length <= 0 ? string.Empty : trimmed.Substring(0, length);
        }

        private static List<string> BuildTargetCandidates(MySqlErrorMessageInfo info, string currentDatabaseName)
        {
            var target = (info.TargetText ?? string.Empty).Trim();
            var result = new List<string>();

            if (string.IsNullOrEmpty(target))
            {
                return result;
            }

            if (info.Kind == MySqlErrorMessageKind.TableNotFound)
            {
                target = NormalizeMissingTableTarget
                (
                    target,
                    currentDatabaseName
                );
            }

            AddCandidate(result, target);

            var unquotedTarget = RemoveOuterBackticks(target);

            AddCandidate(result, unquotedTarget);

            var parts = SplitIdentifierParts(unquotedTarget);

            if (parts.Count >= 2)
            {
                var first = parts[parts.Count - 2];
                var second = parts[parts.Count - 1];

                AddCandidate(result, $"{first}.{second}");
                AddCandidate(result, $"`{EscapeBackticks(first)}`.`{EscapeBackticks(second)}`");
                AddCandidate(result, $"`{EscapeBackticks(first)}`.{second}");
                AddCandidate(result, $"{first}.`{EscapeBackticks(second)}`");
                AddCandidate(result, $"`{EscapeBackticks(first)}.{EscapeBackticks(second)}`");
                AddCandidate(result, second);
                AddCandidate(result, $"`{EscapeBackticks(second)}`");
            }
            else
            {
                AddCandidate(result, $"`{EscapeBackticks(unquotedTarget)}`");
            }

            return result.OrderByDescending(value => value.Length).ToList();
        }

        private static string NormalizeMissingTableTarget(string target, string currentDatabaseName)
        {
            var value = RemoveOuterBackticks(target);
            var parts = SplitIdentifierParts(value);

            //SQL：FROM `sakila.film_actor`
            //目前 Database：sakila
            //MySQL Error：sakila.sakila.film_actor
            //只有這種三段以上、第一段為目前 Database 的情況才移除前綴
            if (parts.Count >= 3 && !string.IsNullOrEmpty(currentDatabaseName) && string.Equals(parts[0], RemoveOuterBackticks(currentDatabaseName), StringComparison.OrdinalIgnoreCase))
            {
                parts.RemoveAt(0);
            }

            return string.Join(".", parts);
        }

        private static List<string> SplitIdentifierParts(string value)
        {
            var result = new List<string>();

            foreach (var part in (value ?? string.Empty).Split('.'))
            {
                var normalized = RemoveOuterBackticks(part.Trim());

                if (!string.IsNullOrEmpty(normalized))
                {
                    result.Add(normalized);
                }
            }

            return result;
        }

        private static void AddCandidate(List<string> candidates, string value)
        {
            if (string.IsNullOrEmpty(value) || candidates.Any(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            candidates.Add(value);
        }

        private static bool TryFindBestCandidate(string sql, IEnumerable<string> candidates, MySqlErrorMessageInfo info, int preferredPosition, out CandidateMatch match)
        {
            match = null;

            if (string.IsNullOrEmpty(sql) || candidates == null)
            {
                return false;
            }

            var searchable = BuildSearchableMap(sql);
            var best = new CandidateMatch();
            var clauseRange = ResolveClauseRange(sql, info.ClauseName);

            foreach (var candidate in candidates)
            {
                var start = 0;

                while (start < sql.Length)
                {
                    var index = sql.IndexOf(candidate, start, StringComparison.OrdinalIgnoreCase);

                    if (index < 0)
                    {
                        break;
                    }

                    start = index + Math.Max(1, candidate.Length);

                    if (!IsCandidateSearchable(searchable, index, candidate.Length) || !HasIdentifierBoundaries(sql, index, candidate.Length))
                    {
                        continue;
                    }

                    var score = CalculateCandidateScore(sql, index, candidate, info, preferredPosition, clauseRange);

                    if (score >= best.Score)
                    {
                        continue;
                    }

                    best.Found = true;
                    best.Position = index;
                    best.Text = candidate;
                    best.Score = score;
                }
            }

            if (!best.Found)
            {
                return false;
            }

            match = best;
            return true;
        }

        private static int CalculateCandidateScore(string sql, int index, string candidate, MySqlErrorMessageInfo info, int preferredPosition, Tuple<int, int> clauseRange)
        {
            var score = 0;

            if (clauseRange != null && (index < clauseRange.Item1 || index >= clauseRange.Item2))
            {
                score += 100000;
            }

            if (info.Kind == MySqlErrorMessageKind.DuplicateColumn)
            {
                //重複欄位名稱通常由後方再次出現的欄位所造成的
                score += Math.Max(0, sql.Length - index);
            }
            else if (preferredPosition > 0)
            {
                score += Math.Abs(index - preferredPosition);
            }
            else
            {
                score += index;
            }

            if (info.Kind == MySqlErrorMessageKind.TableNotFound && !IsAfterObjectKeyword(sql, index))
            {
                score += 10000;
            }

            //相同位置優先採用完整的 qualified／quoted source form
            score += Math.Max(0, 200 - candidate.Length);

            return score;
        }

        private static Tuple<int, int> ResolveClauseRange(string sql, string clauseName)
        {
            var clause = (clauseName ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(clause))
            {
                return null;
            }

            string keyword;

            switch (clause)
            {
                case "where clause":
                case "where":
                    {
                        keyword = "WHERE";
                        break;
                    }
                case "field list":
                    {
                        return ResolveSelectListRange(sql);
                    }
                case "order clause":
                case "order by clause":
                    {
                        keyword = "ORDER BY";
                        break;
                    }
                case "on clause":
                    {
                        keyword = "ON";
                        break;
                    }
                default:
                    {
                        return null;
                    }
            }

            var start = LastKeywordIndex(sql, keyword);

            return start < 0 ? null : Tuple.Create(start, sql.Length);
        }

        private static Tuple<int, int> ResolveSelectListRange(string sql)
        {
            var select = Regex.Match(sql ?? string.Empty, @"\bSELECT\b", RegexOptions.IgnoreCase);

            if (!select.Success)
            {
                return null;
            }

            var from = Regex.Match(sql.Substring(select.Index + select.Length), @"\bFROM\b", RegexOptions.IgnoreCase);
            var end = from.Success ? select.Index + select.Length + from.Index : sql.Length;

            return Tuple.Create(select.Index, end);
        }

        private static int LastKeywordIndex(string sql, string keyword)
        {
            var matches = Regex.Matches(sql ?? string.Empty, @"\b" + Regex.Escape(keyword).Replace("\\ ", @"\s+") + @"\b", RegexOptions.IgnoreCase);

            return matches.Count == 0 ? -1 : matches[matches.Count - 1].Index;
        }

        private static bool IsAfterObjectKeyword(string sql, int position)
        {
            if (string.IsNullOrEmpty(sql) || position <= 0)
            {
                return false;
            }

            var prefix = sql.Substring(0, position);
            var match = Regex.Match(prefix, @"(?:\bFROM\b|\bJOIN\b|\bUPDATE\b|\bINTO\b|\bTABLE\b)\s*$", RegexOptions.IgnoreCase);

            return match.Success;
        }

        private static bool[] BuildSearchableMap(string sql)
        {
            var value = sql ?? string.Empty;
            var result = Enumerable.Repeat(true, value.Length).ToArray();

            var tokenizationResult = SqlTokenizer.Tokenize
            (
                value,
                DataSourceType.MySql,
                SearchableMapTokenizerOptions
            );

            foreach (var token in tokenizationResult.Tokens)
            {
                if (!SuppressesCandidateMatching(token))
                {
                    continue;
                }

                MarkUnsearchable(result, token.Start, token.EndExclusive);

                if (token.Kind == SqlTokenKind.LineComment && token.EndExclusive < value.Length && (value[token.EndExclusive] == '\r' || value[token.EndExclusive] == '\n'))
                {
                    //Legacy map marks the first line terminator as part of the line comment.
                    MarkUnsearchable(result, token.EndExclusive, token.EndExclusive + 1);
                }
            }

            return result;
        }

        internal static bool[] BuildSearchableMapForParityTest(string sql)
        {
            return BuildSearchableMap(sql);
        }

        internal static bool[] BuildSearchableMapLegacyForParityTest(string sql)
        {
            return BuildSearchableMapLegacy(sql ?? string.Empty);
        }

        private static bool SuppressesCandidateMatching(SqlToken token)
        {
            if (token == null)
            {
                return false;
            }

            if (token.Kind == SqlTokenKind.StringLiteral || token.IsComment)
            {
                return true;
            }

            //MySQL backtick identifiers must remain searchable. Legacy behavior suppresses
            //double-quoted text, so only the double-quoted delimited-token form is hidden.
            return token.Kind == SqlTokenKind.DelimitedIdentifier && token.Length > 0 && token.Text[0] == '"';
        }

        private static void MarkUnsearchable(bool[] searchable, int start, int endExclusive)
        {
            if (searchable == null || searchable.Length == 0)
            {
                return;
            }

            var from = Math.Max(0, start);
            var to = Math.Min(searchable.Length, endExclusive);

            for (var i = from; i < to; i++)
            {
                searchable[i] = false;
            }
        }

        private static bool[] BuildSearchableMapLegacy(string sql)
        {
            var result = Enumerable.Repeat(true, sql.Length).ToArray();
            var state = 0;

            for (var i = 0; i < sql.Length; i++)
            {
                var current = sql[i];
                var next = i + 1 < sql.Length ? sql[i + 1] : '\0';

                if (state == 1) //single quoted string
                {
                    result[i] = false;

                    if (current == '\\' && next != '\0')
                    {
                        result[i + 1] = false;
                        i++;
                        continue;
                    }

                    if (current == '\'' && next == '\'')
                    {
                        result[i + 1] = false;
                        i++;
                        continue;
                    }

                    if (current == '\'')
                    {
                        state = 0;
                    }

                    continue;
                }

                if (state == 2) //double quoted string
                {
                    result[i] = false;

                    if (current == '\\' && next != '\0')
                    {
                        result[i + 1] = false;
                        i++;
                        continue;
                    }

                    if (current == '"' && next == '"')
                    {
                        result[i + 1] = false;
                        i++;
                        continue;
                    }

                    if (current == '"')
                    {
                        state = 0;
                    }

                    continue;
                }

                if (state == 3) //line comment
                {
                    result[i] = false;

                    if (current == '\r' || current == '\n')
                    {
                        state = 0;
                    }

                    continue;
                }

                if (state == 4) //block comment
                {
                    result[i] = false;

                    if (current == '*' && next == '/')
                    {
                        result[i + 1] = false;
                        i++;
                        state = 0;
                    }

                    continue;
                }

                if (current == '\'')
                {
                    result[i] = false;
                    state = 1;
                    continue;
                }

                if (current == '"')
                {
                    result[i] = false;
                    state = 2;
                    continue;
                }

                if (current == '#')
                {
                    result[i] = false;
                    state = 3;
                    continue;
                }

                if (current == '-' && next == '-' && (i + 2 >= sql.Length || char.IsWhiteSpace(sql[i + 2])))
                {
                    result[i] = false;
                    result[i + 1] = false;
                    i++;
                    state = 3;
                    continue;
                }

                if (current == '/' && next == '*')
                {
                    result[i] = false;
                    result[i + 1] = false;
                    i++;
                    state = 4;
                }
            }

            return result;
        }

        private static bool IsCandidateSearchable(bool[] searchable, int position, int length)
        {
            if (searchable == null || position < 0 || length <= 0 || position + length > searchable.Length)
            {
                return false;
            }

            for (var i = position; i < position + length; i++)
            {
                //Backticks are SQL identifiers and remain searchable
                if (!searchable[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasIdentifierBoundaries(string sql, int position, int length)
        {
            var first = sql[position];
            var last = sql[position + length - 1];

            if (IsIdentifierCharacter(first) && position > 0 && IsIdentifierCharacter(sql[position - 1]))
            {
                return false;
            }

            var end = position + length;

            return !IsIdentifierCharacter(last) || end >= sql.Length || !IsIdentifierCharacter(sql[end]);
        }

        private static bool IsIdentifierCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_' || value == '$';
        }

        private static SqlErrorResolutionResult MapOriginalMatchToEditor(ExecutionContext context, CandidateMatch match)
        {
            return CreateResult
            (
                context.EditorStart + match.Position,
                match.Text
            );
        }

        private static SqlErrorResolutionResult MapExecutedMatchToEditor(MySqlErrorPositionRequest request, ExecutionContext context, CandidateMatch match)
        {
            var relativePosition = Math.Max(0, match.Position - context.ExecutedCoreStart);
            var position = context.EditorStart + relativePosition;
            var target = match.Text;

            if (!string.IsNullOrEmpty(request.ParameterPositionMapping)
                && SqlParameterPositionMapper.TryMapExecutedSqlPositionToOriginalSqlPosition(request.ParameterPositionMapping, request.ParameterStartPosition, request.ParameterStartPosition + relativePosition, out var mappedPosition, out var mappedParameterName))
            {
                position = mappedPosition;

                if (!string.IsNullOrEmpty(mappedParameterName))
                {
                    target = mappedParameterName;
                }
            }

            return CreateResult(position, target);
        }

        private static SqlErrorResolutionResult CreateResult(int position, string target)
        {
            return new SqlErrorResolutionResult
            {
                Found = position >= 0,
                Position = Math.Max(0, position),
                Length = target?.Length ?? 0,
                TargetText = target ?? string.Empty,
                ShouldSetSquiggle = !string.IsNullOrEmpty(target)
            };
        }

        private static SqlErrorResolutionResult CreateReportedPositionFallback(MySqlErrorPositionRequest request)
        {
            var position = request.ReportedPosition + request.PositionOffset;

            position = Clamp
            (
                position,
                0,
                Math.Max(0, (request.EditorSql ?? string.Empty).Length - 1)
            );

            return new SqlErrorResolutionResult
            {
                Found = !string.IsNullOrEmpty(request.EditorSql),
                Position = position,
                Length = 0,
                TargetText = string.Empty,
                ShouldSetSquiggle = false,
                UsedReportedPositionFallback = true
            };
        }

        private static List<LineInfo> SplitLines(string sql)
        {
            var result = new List<LineInfo>();
            var text = sql ?? string.Empty;
            var start = 0;

            for (var i = 0; i <= text.Length; i++)
            {
                if (i < text.Length && text[i] != '\r' && text[i] != '\n')
                {
                    continue;
                }

                result.Add
                (
                    new LineInfo
                    {
                        Start = start,
                        Text = text.Substring(start, i - start)
                    }
                );

                if (i < text.Length && text[i] == '\r'
                    && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                start = i + 1;
            }

            return result;
        }

        private static string RemoveOuterBackticks(string value)
        {
            var text = (value ?? string.Empty).Trim();

            if (text.Length >= 2 && text[0] == '`' && text[text.Length - 1] == '`')
            {
                return text.Substring(1, text.Length - 2).Replace("``", "`");
            }

            return text;
        }

        private static string EscapeBackticks(string value)
        {
            return (value ?? string.Empty).Replace("`", "``");
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }
    }
}
