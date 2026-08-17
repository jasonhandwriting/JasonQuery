using JasonQuery.Core.Database.Diagnostics.Common;
using JasonQuery.Core.Database.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.Diagnostics.Oracle
{
    internal static class OracleErrorPositionResolver
    {
        private sealed class ExecutionRange
        {
            public int Start { get; set; }

            public int End { get; set; }

            public bool IsBasedOnExecutedSql { get; set; }

            public string SqlText { get; set; } = string.Empty; //用來記錄這個 range 到底是用哪一份 SQL 找出來的

            public int Length => Math.Max(0, End - Start);
        }

        private sealed class TextCandidate
        {
            public string Text { get; set; } = string.Empty;

            public int Priority { get; set; }
        }

        private sealed class CandidateMatch
        {
            public bool Found { get; set; }

            public int Position { get; set; }

            public string Text { get; set; } = string.Empty;

            public int Distance { get; set; } = int.MaxValue;

            public int Priority { get; set; } = int.MaxValue;
        }

        public static SqlErrorResolutionResult Resolve(OracleErrorPositionRequest request)
        {
            var result = new SqlErrorResolutionResult();

            if (request == null || string.IsNullOrEmpty(request.EditorSql))
            {
                return result;
            }

            var normalizedCode = OracleErrorCodeParser.Normalize
            (
                request.ErrorCode,
                request.ErrorMessage
            );

            var range = ResolveExecutionRange(request);

            var reportedPosition = NormalizeReportedPosition
            (
                request,
                range,
                out var hadUtf8Adjustment,
                out var hasManyNonAsciiCharacters
            );

            result.NormalizedErrorCode = normalizedCode;
            result.Position = reportedPosition;
            result.HadUtf8PositionAdjustment = hadUtf8Adjustment;
            result.HasManyNonAsciiCharactersBeforePosition = hasManyNonAsciiCharacters;

            if (TryResolveMalformedSelectListToken(request.EditorSql, range, normalizedCode, out var malformedPosition, out var malformedTarget))
            {
                return CreateTargetResult
                (
                    result,
                    malformedPosition,
                    malformedTarget,
                    shouldSetSquiggle: true
                );
            }

            if (TryResolveInvalidTableAliasAsKeyword(request.EditorSql, range, normalizedCode, reportedPosition, out var asPosition))
            {
                return CreateTargetResult
                (
                    result,
                    asPosition,
                    "AS",
                    shouldSetSquiggle: true
                );
            }

            if (TryResolveMissingObjectTarget(request.EditorSql, range, normalizedCode, reportedPosition, out var objectPosition, out var objectTarget))
            {
                return CreateTargetResult
                (
                    result,
                    objectPosition,
                    objectTarget,
                    shouldSetSquiggle: true
                );
            }

            if (TryResolveQuotedIdentifierTarget(request.EditorSql, range, request.ErrorMessage, normalizedCode, reportedPosition,
                                                 out var quotedPosition, out var quotedTarget))
            {
                return CreateTargetResult
                (
                    result,
                    quotedPosition,
                    quotedTarget,
                    shouldSetSquiggle: true
                );
            }

            if (TryResolveKnownLiteralTarget(request.EditorSql, range, normalizedCode, reportedPosition, out var literalPosition, out var literalTarget))
            {
                return CreateTargetResult
                (
                    result,
                    literalPosition,
                    literalTarget,
                    shouldSetSquiggle: true
                );
            }

            if (TryResolveMissingExpressionAtEnd(request.EditorSql, range, normalizedCode, out var missingExpressionPosition))
            {
                return CreateTargetResult
                (
                    result,
                    missingExpressionPosition,
                    " ",
                    shouldSetSquiggle: false
                );
            }

            if (TryResolveParameterMappedPosition(request, request.ReportedPosition, out var mappedPosition, out var mappedParameterName))
            {
                var target = mappedParameterName;

                if (string.IsNullOrEmpty(target))
                {
                    target = ExtractTokenAtOrNearPosition
                    (
                        request.EditorSql,
                        mappedPosition,
                        range
                    );
                }

                return CreateTargetResult
                (
                    result,
                    mappedPosition,
                    target,
                    shouldSetSquiggle: !string.IsNullOrWhiteSpace(target)
                );
            }

            var fallbackTarget = ExtractTokenAtOrNearPosition
            (
                request.EditorSql,
                reportedPosition,
                range
            );

            if (!string.IsNullOrEmpty(fallbackTarget))
            {
                var fallbackPosition = FindTokenStartNearPosition
                (
                    request.EditorSql,
                    fallbackTarget,
                    reportedPosition,
                    range
                );

                result.UsedReportedPositionFallback = true;

                return CreateTargetResult
                (
                    result,
                    fallbackPosition,
                    fallbackTarget,
                    shouldSetSquiggle: !string.IsNullOrWhiteSpace(fallbackTarget)
                );
            }

            result.Found = reportedPosition >= range.Start
                           && reportedPosition < range.End;
            result.ShouldSetSquiggle = false;
            result.UsedReportedPositionFallback = true;

            return result;
        }

        private static SqlErrorResolutionResult CreateTargetResult(SqlErrorResolutionResult result, int position, string target, bool shouldSetSquiggle)
        {
            result.Found = position >= 0;
            result.Position = Math.Max(0, position);
            result.TargetText = target ?? string.Empty;
            result.Length = result.TargetText.Length;
            result.ShouldSetSquiggle = shouldSetSquiggle && result.Length > 0;

            return result;
        }

        private static ExecutionRange ResolveExecutionRange(OracleErrorPositionRequest request)
        {
            var editorSql = request.EditorSql ?? string.Empty;

            var preferredStart = Clamp
            (
                request.PreferredExecutionStart,
                0,
                editorSql.Length
            );

            foreach (var candidate in BuildOracleExecutionRangeSqlCandidates(request.ExecutedSql))
            {
                if (TryCreateExecutionRange(editorSql, candidate, preferredStart, isBasedOnExecutedSql: true, out var executedRange))
                {
                    return executedRange;
                }
            }

            var originalExecutedSql = !string.IsNullOrEmpty(request.OriginalExecutedSql)
                                      ? request.OriginalExecutedSql
                                      : request.ExecutedSql;

            foreach (var candidate in BuildOracleExecutionRangeSqlCandidates(originalExecutedSql))
            {
                if (TryCreateExecutionRange(editorSql, candidate, preferredStart, isBasedOnExecutedSql: false, out var originalRange))
                {
                    return originalRange;
                }
            }

            var end = editorSql.Length;

            if (end <= preferredStart)
            {
                end = preferredStart;
            }

            return new ExecutionRange
            {
                Start = preferredStart,
                End = end,
                IsBasedOnExecutedSql = false,
                SqlText = editorSql.Substring(preferredStart, end - preferredStart)
            };
        }

        private static bool TryCreateExecutionRange(string editorSql, string targetSql, int preferredStart, bool isBasedOnExecutedSql, out ExecutionRange range)
        {
            range = null;

            if (string.IsNullOrEmpty(editorSql) || string.IsNullOrEmpty(targetSql))
            {
                return false;
            }

            var start = FindExecutionStart(editorSql, targetSql, preferredStart);

            if (start < 0)
            {
                return false;
            }

            var end = Clamp
            (
                start + targetSql.Length,
                start,
                editorSql.Length
            );

            if (end <= start)
            {
                return false;
            }

            range = new ExecutionRange
            {
                Start = start,
                End = end,
                IsBasedOnExecutedSql = isBasedOnExecutedSql,
                SqlText = targetSql
            };

            return true;
        }

        private static IEnumerable<string> BuildOracleExecutionRangeSqlCandidates(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                yield break;
            }

            var withoutLeadingTrivia = RemoveLeadingOracleSqlTrivia(sql);

            if (!string.IsNullOrEmpty(withoutLeadingTrivia))
            {
                yield return withoutLeadingTrivia;

                var withoutLeadingTriviaAndTerminator = TrimTrailingStatementTerminator(withoutLeadingTrivia);

                if (!string.Equals(withoutLeadingTriviaAndTerminator, withoutLeadingTrivia, StringComparison.Ordinal))
                {
                    yield return withoutLeadingTriviaAndTerminator;
                }
            }

            var originalTrimmedEnd = sql.TrimEnd();

            if (!string.IsNullOrEmpty(originalTrimmedEnd)
                && !string.Equals(originalTrimmedEnd, withoutLeadingTrivia, StringComparison.Ordinal))
            {
                yield return originalTrimmedEnd;

                var originalWithoutTerminator = TrimTrailingStatementTerminator(originalTrimmedEnd);

                if (!string.Equals(originalWithoutTerminator, originalTrimmedEnd, StringComparison.Ordinal))
                {
                    yield return originalWithoutTerminator;
                }
            }
        }

        private static string RemoveLeadingOracleSqlTrivia(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return string.Empty;
            }

            var index = 0;

            while (index < sql.Length)
            {
                while (index < sql.Length && char.IsWhiteSpace(sql[index]))
                {
                    index++;
                }

                if (index + 1 < sql.Length && sql[index] == '-' && sql[index + 1] == '-')
                {
                    index += 2;

                    while (index < sql.Length && sql[index] != '\r' && sql[index] != '\n')
                    {
                        index++;
                    }

                    continue;
                }

                if (index + 1 < sql.Length && sql[index] == '/' && sql[index + 1] == '*')
                {
                    //Oracle hint /*+ ... */ 不視為可移除的普通註釋
                    if (index + 2 < sql.Length && sql[index + 2] == '+')
                    {
                        break;
                    }

                    var endComment = sql.IndexOf("*/", index + 2, StringComparison.Ordinal);

                    if (endComment < 0)
                    {
                        return string.Empty;
                    }

                    index = endComment + 2;
                    continue;
                }

                break;
            }

            //這個方法只負責移除開頭的空白與註釋；結尾空白可能是錯誤游標的實際目標，必須保留
            return index >= sql.Length ? string.Empty : sql.Substring(index);
        }

        private static string TrimTrailingStatementTerminator(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return string.Empty;
            }

            var value = sql.TrimEnd();

            if (value.EndsWith(";", StringComparison.Ordinal))
            {
                value = value.Substring(0, value.Length - 1).TrimEnd();
            }

            return value;
        }

        private static int FindExecutionStart(string editorSql, string executedSql, int preferredStart)
        {
            if (string.IsNullOrEmpty(editorSql))
            {
                return -1;
            }

            if (string.IsNullOrEmpty(executedSql))
            {
                return preferredStart;
            }

            if (preferredStart + executedSql.Length <= editorSql.Length
                && string.Equals(editorSql.Substring(preferredStart, executedSql.Length), executedSql, StringComparison.Ordinal))
            {
                return preferredStart;
            }

            var index = editorSql.IndexOf
            (
                executedSql,
                preferredStart,
                StringComparison.Ordinal
            );

            if (index >= 0)
            {
                return index;
            }

            return editorSql.IndexOf
            (
                executedSql,
                StringComparison.Ordinal
            );
        }

        private static int NormalizeReportedPosition(OracleErrorPositionRequest request, ExecutionRange range, out bool hadUtf8Adjustment, out bool hasManyNonAsciiCharacters)
        {
            hadUtf8Adjustment = false;
            hasManyNonAsciiCharacters = false;

            var editorSql = request.EditorSql ?? string.Empty;
            var rawReportedPosition = MapOracleReportedPositionToEditorSql(request, range);

            var reportedPosition = Clamp
            (
                rawReportedPosition,
                range.Start,
                Math.Max(range.Start, range.End - 1)
            );

            var rangeSqlText = range.SqlText ?? string.Empty;

            if (string.IsNullOrEmpty(rangeSqlText))
            {
                return reportedPosition;
            }

            var relativePosition = rawReportedPosition - range.Start;

            if (relativePosition <= 0)
            {
                return reportedPosition;
            }

            var safeCharacterLength = Math.Min
            (
                relativePosition,
                rangeSqlText.Length
            );

            var prefix = rangeSqlText.Substring
            (
                0,
                safeCharacterLength
            );

            var nonAsciiCount = prefix.Count(value => value > 127);

            hasManyNonAsciiCharacters = nonAsciiCount >= 20;

            if (nonAsciiCount == 0)
            {
                return reportedPosition;
            }

            var mappedRelativePosition = MapUtf8ByteOffsetToUtf16Index
            (
                rangeSqlText,
                relativePosition
            );

            if (mappedRelativePosition < 0 || mappedRelativePosition == relativePosition)
            {
                return reportedPosition;
            }

            hadUtf8Adjustment = true;

            return Clamp
            (
                range.Start + mappedRelativePosition,
                range.Start,
                Math.Max(range.Start, range.End - 1)
            );
        }

        private static int MapOracleReportedPositionToEditorSql(OracleErrorPositionRequest request, ExecutionRange range)
        {
            if (request == null || range == null)
            {
                return 0;
            }

            var reportedPosition = request.ReportedPosition;

            if (!range.IsBasedOnExecutedSql)
            {
                return reportedPosition;
            }

            if (range.Start <= 0)
            {
                return reportedPosition;
            }

            var executedSql = !string.IsNullOrEmpty(range.SqlText)
                              ? range.SqlText
                              : request.ExecutedSql ?? string.Empty;

            if (string.IsNullOrEmpty(executedSql))
            {
                return reportedPosition;
            }

            if (reportedPosition < 0 || reportedPosition >= executedSql.Length)
            {
                return reportedPosition;
            }

            var mappedPosition = range.Start + reportedPosition;

            if (mappedPosition < range.Start || mappedPosition >= range.End)
            {
                return reportedPosition;
            }

            //如果 reportedPosition 明顯還在執行 SQL 範圍之前，代表它是 ExecutedSql 的相對位置。
            if (reportedPosition < range.Start)
            {
                return mappedPosition;
            }

            //保守判斷：
            //若 editorSql[reportedPosition] 比較不像 ExecutedSql[reportedPosition]，而 editorSql[range.Start + reportedPosition] 比較像，則視為 ExecutedSql 相對位置
            var editorSql = request.EditorSql ?? string.Empty;
            var rawScore = CountMatchingCharacters(editorSql, reportedPosition, executedSql, reportedPosition, 12);
            var mappedScore = CountMatchingCharacters(editorSql, mappedPosition, executedSql, reportedPosition, 12);

            return mappedScore > rawScore ? mappedPosition : reportedPosition;
        }

        private static int CountMatchingCharacters(string left, int leftStart, string right, int rightStart, int maxLength)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
            {
                return 0;
            }

            if (leftStart < 0 || rightStart < 0 || leftStart >= left.Length || rightStart >= right.Length)
            {
                return 0;
            }

            var count = 0;
            var length = Math.Min(maxLength, Math.Min(left.Length - leftStart, right.Length - rightStart));

            for (var i = 0; i < length; i++)
            {
                if (left[leftStart + i] != right[rightStart + i])
                {
                    break;
                }

                count++;
            }

            return count;
        }

        private static int MapUtf8ByteOffsetToUtf16Index(string text, int byteOffset)
        {
            if (string.IsNullOrEmpty(text) || byteOffset <= 0)
            {
                return 0;
            }

            var bytesConsumed = 0;

            for (var index = 0; index < text.Length; index++)
            {
                var characterLength = char.IsHighSurrogate(text[index]) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]) ? 2 : 1;
                var value = text.Substring(index, characterLength);
                var valueByteCount = Encoding.UTF8.GetByteCount(value);

                if (bytesConsumed + valueByteCount > byteOffset)
                {
                    return index;
                }

                bytesConsumed += valueByteCount;

                if (bytesConsumed == byteOffset)
                {
                    return index + characterLength;
                }

                if (characterLength == 2)
                {
                    index++;
                }
            }

            return text.Length;
        }

        private static bool TryResolveMalformedSelectListToken(string editorSql, ExecutionRange range, string normalizedCode, out int position, out string target)
        {
            position = -1;
            target = string.Empty;

            if (!OracleErrorCodeParser.Is(normalizedCode, "00923") && !OracleErrorCodeParser.Is(normalizedCode, "00972"))
            {
                return false;
            }

            var sql = editorSql.Substring(range.Start, range.Length);

            var selectMatch = Regex.Match
            (
                sql,
                @"\bSELECT\b",
                RegexOptions.IgnoreCase
            );

            if (!selectMatch.Success)
            {
                return false;
            }

            var fromMatch = Regex.Match
            (
                sql.Substring(selectMatch.Index + selectMatch.Length),
                @"\bFROM\b",
                RegexOptions.IgnoreCase
            );

            if (!fromMatch.Success)
            {
                return false;
            }

            var selectListStart = selectMatch.Index + selectMatch.Length;
            var selectListLength = fromMatch.Index;

            var selectList = sql.Substring
            (
                selectListStart,
                selectListLength
            );

            foreach (Match tokenMatch in Regex.Matches(selectList, @"[^\s,]+"))
            {
                var token = tokenMatch.Value.TrimEnd(',');
                var quoteCount = token.Count(value => value == '"');
                var hasBracket = token.IndexOf('[') >= 0 || token.IndexOf(']') >= 0;
                var hasUnmatchedQuote = quoteCount % 2 != 0;

                if (!hasBracket && !hasUnmatchedQuote)
                {
                    continue;
                }

                position = range.Start + selectListStart + tokenMatch.Index;
                target = token;

                return true;
            }

            return false;
        }

        private static bool TryResolveInvalidTableAliasAsKeyword(string editorSql, ExecutionRange range, string normalizedCode, int reportedPosition, out int position)
        {
            position = -1;

            if (!OracleErrorCodeParser.Is(normalizedCode, "00933"))
            {
                return false;
            }

            var bestDistance = int.MaxValue;

            foreach (Match keywordMatch in Regex.Matches(editorSql.Substring(range.Start, range.Length), @"\b(?:FROM|JOIN)\b", RegexOptions.IgnoreCase))
            {
                var objectStart = range.Start + keywordMatch.Index + keywordMatch.Length;

                if (!TryReadQualifiedIdentifier(editorSql, objectStart, range.End, out _, out var objectEnd))
                {
                    continue;
                }

                var nextWordStart = SkipWhiteSpace
                (
                    editorSql,
                    objectEnd,
                    range.End
                );

                if (!MatchesWordAt(editorSql, nextWordStart, "AS", range.End))
                {
                    continue;
                }

                var distance = Math.Abs(nextWordStart - reportedPosition);

                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                position = nextWordStart;
            }

            return position >= 0;
        }

        private static bool TryResolveMissingObjectTarget(string editorSql, ExecutionRange range, string normalizedCode, int reportedPosition, out int position, out string target)
        {
            position = -1;
            target = string.Empty;

            if (!OracleErrorCodeParser.Is(normalizedCode, "00942"))
            {
                return false;
            }

            var bestDistance = int.MaxValue;

            foreach (Match keywordMatch in Regex.Matches(editorSql.Substring(range.Start, range.Length), @"\b(?:FROM|JOIN|UPDATE|INTO)\b", RegexOptions.IgnoreCase))
            {
                var objectStart = range.Start + keywordMatch.Index + keywordMatch.Length;

                if (!TryReadQualifiedIdentifier(editorSql, objectStart, range.End, out var objectText, out _))
                {
                    continue;
                }

                var exactPosition = editorSql.IndexOf
                (
                    objectText,
                    objectStart,
                    StringComparison.Ordinal
                );

                if (exactPosition < 0)
                {
                    continue;
                }

                var distance = Math.Abs(exactPosition - reportedPosition);

                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                position = exactPosition;
                target = objectText;
            }

            return position >= 0;
        }

        private static bool TryResolveQuotedIdentifierTarget(string editorSql, ExecutionRange range, string errorMessage, string normalizedCode,
                                                             int reportedPosition, out int position, out string target)
        {
            position = -1;
            target = string.Empty;

            if (!TryExtractQuotedIdentifierSegments(errorMessage, out var rawPath, out var segments))
            {
                return false;
            }

            var candidates = BuildOracleIdentifierCandidates
            (
                rawPath,
                segments,
                normalizedCode
            );

            var match = FindBestCandidate
            (
                editorSql,
                range,
                candidates,
                reportedPosition
            );

            if (!match.Found)
            {
                return false;
            }

            position = match.Position;
            target = match.Text;

            return true;
        }

        private static bool TryExtractQuotedIdentifierSegments(string errorMessage, out string rawPath, out string[] segments)
        {
            rawPath = string.Empty;
            segments = new string[0];

            if (string.IsNullOrEmpty(errorMessage))
            {
                return false;
            }

            var matches = Regex.Matches
            (
                errorMessage,
                "\"(?:[^\"]|\"\")*\"(?:\\s*\\.\\s*\"(?:[^\"]|\"\")*\")*"
            );

            if (matches.Count == 0)
            {
                return false;
            }

            var selectedMatch = matches.Cast<Match>()
                                       .OrderByDescending(match => match.Length)
                                       .ThenByDescending(match => match.Index)
                                       .First();

            rawPath = selectedMatch.Value;
            segments = Regex.Matches(rawPath, "\"(?:[^\"]|\"\")*\"")
                            .Cast<Match>()
                            .Select(match => UnquoteOracleIdentifier(match.Value))
                            .ToArray();

            return segments.Length > 0;
        }

        private static List<TextCandidate> BuildOracleIdentifierCandidates(string rawPath, string[] segments, string normalizedCode)
        {
            var candidates = new List<TextCandidate>();

            if (segments == null || segments.Length == 0)
            {
                return candidates;
            }

            if (OracleErrorCodeParser.Is(normalizedCode, "01400"))
            {
                var leaf = segments[segments.Length - 1];

                AddCandidate(candidates, QuoteOracleIdentifier(leaf), 0);
                AddCandidate(candidates, leaf, 1);

                return candidates;
            }

            AddCandidate(candidates, rawPath, 0);

            if (segments.Length >= 2)
            {
                var first = segments[segments.Length - 2];
                var second = segments[segments.Length - 1];

                AddCandidate
                (
                    candidates,
                    $"{QuoteOracleIdentifier(first)}.{QuoteOracleIdentifier(second)}",
                    1
                );
                AddCandidate
                (
                    candidates,
                    $"{QuoteOracleIdentifier(first)}.{second}",
                    2
                );
                AddCandidate
                (
                    candidates,
                    $"{first}.{QuoteOracleIdentifier(second)}",
                    3
                );
                AddCandidate
                (
                    candidates,
                    $"{first}.{second}",
                    4
                );
                AddCandidate
                (
                    candidates,
                    QuoteOracleIdentifier($"{first}.{second}"),
                    5
                );
            }

            var leafSegment = segments[segments.Length - 1];

            AddCandidate(candidates, QuoteOracleIdentifier(leafSegment), 6);
            AddCandidate(candidates, leafSegment, 7);

            return candidates;
        }

        private static void AddCandidate(ICollection<TextCandidate> candidates, string text, int priority)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (candidates.Any
                (
                    item => string.Equals
                    (
                        item.Text,
                        text,
                        StringComparison.OrdinalIgnoreCase
                    )
                ))
            {
                return;
            }

            candidates.Add
            (
                new TextCandidate
                {
                    Text = text,
                    Priority = priority
                }
            );
        }

        private static CandidateMatch FindBestCandidate(string source, ExecutionRange range, IEnumerable<TextCandidate> candidates, int reportedPosition)
        {
            var result = new CandidateMatch();

            foreach (var candidate in candidates ?? Enumerable.Empty<TextCandidate>())
            {
                var searchIndex = range.Start;

                while (searchIndex < range.End)
                {
                    var index = source.IndexOf
                    (
                        candidate.Text,
                        searchIndex,
                        range.End - searchIndex,
                        StringComparison.OrdinalIgnoreCase
                    );

                    if (index < 0)
                    {
                        break;
                    }

                    if (IsCandidateBoundaryMatch(source, index, candidate.Text.Length, range))
                    {
                        var distance = Math.Abs(index - reportedPosition);
                        var isBetter = !result.Found
                                       || distance < result.Distance
                                       || distance == result.Distance && candidate.Priority < result.Priority
                                       || distance == result.Distance && candidate.Priority == result.Priority && candidate.Text.Length > result.Text.Length;

                        if (isBetter)
                        {
                            result.Found = true;
                            result.Position = index;

                            result.Text = source.Substring
                            (
                                index,
                                candidate.Text.Length
                            );

                            result.Distance = distance;
                            result.Priority = candidate.Priority;
                        }
                    }

                    searchIndex = index + Math.Max(1, candidate.Text.Length);
                }
            }

            return result;
        }

        private static bool IsCandidateBoundaryMatch(string source, int position, int length, ExecutionRange range)
        {
            if (position < range.Start || length <= 0 || position + length > range.End)
            {
                return false;
            }

            var first = source[position];
            var last = source[position + length - 1];

            if (first == '"' || last == '"')
            {
                return true;
            }

            if (position > range.Start && IsOracleIdentifierPart(source[position - 1]))
            {
                return false;
            }

            var right = position + length;

            return right >= range.End || !IsOracleIdentifierPart(source[right]);
        }

        private static bool IsOracleIdentifierPart(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_' || value == '$' || value == '#';
        }

        private static bool TryResolveKnownLiteralTarget(string editorSql, ExecutionRange range, string normalizedCode, int reportedPosition,
                                                         out int position, out string target)
        {
            position = -1;
            target = string.Empty;

            if (!OracleErrorCodeParser.Is(normalizedCode, "01722") && !OracleErrorCodeParser.Is(normalizedCode, "01843")
                && !OracleErrorCodeParser.Is(normalizedCode, "01847"))
            {
                return false;
            }

            var bestDistance = int.MaxValue;

            foreach (Match match in Regex.Matches(editorSql.Substring(range.Start, range.Length), @"'(?:''|[^'])*'"))
            {
                var absolutePosition = range.Start + match.Index;
                var distance = Math.Abs(absolutePosition - reportedPosition);

                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                position = absolutePosition;
                target = match.Value;
            }

            return position >= 0;
        }

        private static bool TryResolveMissingExpressionAtEnd(string editorSql, ExecutionRange range, string normalizedCode, out int position)
        {
            position = -1;

            if (!OracleErrorCodeParser.Is(normalizedCode, "00936"))
            {
                return false;
            }

            var executedText = editorSql.Substring(range.Start, range.Length);
            var trimmedEnd = executedText.TrimEnd('\r', '\n', ' ', '\t', ';');

            if (!Regex.IsMatch(trimmedEnd, @"\bWHERE$", RegexOptions.IgnoreCase))
            {
                return false;
            }

            position = range.Start + trimmedEnd.Length;

            if (position >= range.End)
            {
                position = Math.Max(range.Start, range.End - 1);
            }

            return true;
        }

        private static bool TryResolveParameterMappedPosition(OracleErrorPositionRequest request, int reportedPosition, out int mappedPosition, out string mappedParameterName)
        {
            mappedPosition = reportedPosition;
            mappedParameterName = string.Empty;

            if (string.IsNullOrEmpty(request.ParameterPositionMapping))
            {
                return false;
            }

            return SqlParameterPositionMapper.TryMapExecutedSqlPositionToOriginalSqlPosition
            (
                request.ParameterPositionMapping,
                request.ParameterStartPosition,
                reportedPosition,
                out mappedPosition,
                out mappedParameterName
            );
        }

        private static string ExtractTokenAtOrNearPosition(string source, int position, ExecutionRange range)
        {
            if (string.IsNullOrEmpty(source) || range.End <= range.Start)
            {
                return string.Empty;
            }

            var safePosition = Clamp
            (
                position,
                range.Start,
                range.End - 1
            );

            if (char.IsWhiteSpace(source[safePosition]))
            {
                var forward = safePosition;

                while (forward < range.End && char.IsWhiteSpace(source[forward]))
                {
                    forward++;
                }

                if (forward < range.End)
                {
                    safePosition = forward;
                }
                else
                {
                    var backward = safePosition;

                    while (backward >= range.Start && char.IsWhiteSpace(source[backward]))
                    {
                        backward--;
                    }

                    if (backward < range.Start)
                    {
                        return string.Empty;
                    }

                    safePosition = backward;
                }
            }

            if (source[safePosition] == '\'')
            {
                return ExtractSingleQuotedLiteral(source, safePosition, range);
            }

            if (source[safePosition] == '"')
            {
                return ExtractDoubleQuotedIdentifier(source, safePosition, range);
            }

            var start = safePosition;
            var end = safePosition + 1;

            while (start > range.Start && !IsFallbackTokenTerminator(source[start - 1]))
            {
                start--;
            }

            while (end < range.End && !IsFallbackTokenTerminator(source[end]))
            {
                end++;
            }

            return end > start ? source.Substring(start, end - start) : string.Empty;
        }

        private static string ExtractSingleQuotedLiteral(string source, int position, ExecutionRange range)
        {
            var start = position;

            while (start > range.Start)
            {
                start--;

                if (source[start] == '\'')
                {
                    break;
                }
            }

            var end = position + 1;

            while (end < range.End)
            {
                if (source[end] == '\'')
                {
                    end++;
                    break;
                }

                end++;
            }

            return end > start ? source.Substring(start, end - start) : string.Empty;
        }

        private static string ExtractDoubleQuotedIdentifier(string source, int position, ExecutionRange range)
        {
            var start = position;

            while (start > range.Start && source[start] != '"')
            {
                start--;
            }

            var end = position + 1;

            while (end < range.End)
            {
                if (source[end] == '"')
                {
                    end++;
                    break;
                }

                end++;
            }

            return end > start ? source.Substring(start, end - start) : string.Empty;
        }

        private static int FindTokenStartNearPosition(string source, string target, int reportedPosition, ExecutionRange range)
        {
            if (string.IsNullOrEmpty(target))
            {
                return reportedPosition;
            }

            var bestPosition = -1;
            var bestDistance = int.MaxValue;
            var searchIndex = range.Start;

            while (searchIndex < range.End)
            {
                var index = source.IndexOf
                (
                    target,
                    searchIndex,
                    range.End - searchIndex,
                    StringComparison.OrdinalIgnoreCase
                );

                if (index < 0)
                {
                    break;
                }

                var distance = Math.Abs(index - reportedPosition);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPosition = index;
                }

                searchIndex = index + Math.Max(1, target.Length);
            }

            return bestPosition >= 0 ? bestPosition : reportedPosition;
        }

        private static bool IsFallbackTokenTerminator(char value)
        {
            if (char.IsWhiteSpace(value))
            {
                return true;
            }

            switch (value)
            {
                case ',':
                case ';':
                case ')':
                case '(':
                case '=':
                case '+':
                case '-':
                case '*':
                case '/':
                case ':':
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool TryReadQualifiedIdentifier(string source, int start, int end, out string identifier, out int identifierEnd)
        {
            identifier = string.Empty;
            identifierEnd = start;

            var index = SkipWhiteSpace(source, start, end);

            if (index >= end || source[index] == '(')
            {
                return false;
            }

            var tokenStart = index;

            if (!TryReadIdentifierSegment(source, ref index, end))
            {
                return false;
            }

            while (index < end)
            {
                var beforeDot = index;

                index = SkipWhiteSpace(source, index, end);

                if (index >= end || source[index] != '.')
                {
                    index = beforeDot;
                    break;
                }

                index++;
                index = SkipWhiteSpace(source, index, end);

                if (!TryReadIdentifierSegment(source, ref index, end))
                {
                    return false;
                }
            }

            identifierEnd = index;
            identifier = source.Substring(tokenStart, identifierEnd - tokenStart).Trim();

            return !string.IsNullOrEmpty(identifier);
        }

        private static bool TryReadIdentifierSegment(string source, ref int index, int end)
        {
            if (index >= end)
            {
                return false;
            }

            if (source[index] == '"')
            {
                index++;

                while (index < end)
                {
                    if (source[index] != '"')
                    {
                        index++;
                        continue;
                    }

                    if (index + 1 < end && source[index + 1] == '"')
                    {
                        index += 2;
                        continue;
                    }

                    index++;
                    return true;
                }

                return false;
            }

            var start = index;

            while (index < end && IsOracleIdentifierPart(source[index]))
            {
                index++;
            }

            return index > start;
        }

        private static int SkipWhiteSpace(string source, int start, int end)
        {
            var index = Math.Max(0, start);

            while (index < end && char.IsWhiteSpace(source[index]))
            {
                index++;
            }

            return index;
        }

        private static bool MatchesWordAt(string source, int position, string word, int end)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(word) || position < 0 || position + word.Length > end)
            {
                return false;
            }

            if (!string.Equals(source.Substring(position, word.Length), word, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var right = position + word.Length;

            return right >= end || !IsOracleIdentifierPart(source[right]);
        }

        private static string QuoteOracleIdentifier(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private static string UnquoteOracleIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2 || value[0] != '"' || value[value.Length - 1] != '"')
            {
                return value ?? string.Empty;
            }

            return value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            if (maximum < minimum)
            {
                return minimum;
            }

            if (value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }
    }
}