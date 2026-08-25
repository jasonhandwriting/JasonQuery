using JasonQuery.Core.Database.Diagnostics.Common;
using JasonQuery.Core.Database.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.Diagnostics.SqlServer
{
    internal static class SqlServerErrorPositionResolver
    {
        private sealed class ExecutionContext
        {
            public int EditorStart { get; set; }

            public string OriginalSql { get; set; } = string.Empty;

            public string ExecutedSql { get; set; } = string.Empty;
        }

        private sealed class CandidateMatch
        {
            public bool Found { get; set; }

            public int Position { get; set; }

            public string Text { get; set; } = string.Empty;

            public int Distance { get; set; } = int.MaxValue;
        }

        public static SqlServerErrorPositionResolution Resolve(SqlServerErrorPositionRequest request)
        {
            var output = new SqlServerErrorPositionResolution
            {
                ErrorMessage = request?.ErrorMessage ?? string.Empty
            };

            if (request == null || string.IsNullOrEmpty(request.EditorSql))
            {
                return output;
            }

            var context = CreateExecutionContext(request);
            var secondaryItems = SqlServerSecondaryErrorParser.Parse
            (
                request.SecondaryErrorMessage
            );

            CandidateMatch match = null;

            foreach (var item in secondaryItems)
            {
                if (TryResolveSecondaryItem(context.ExecutedSql, item, out var current))
                {
                    //SQL Server 可能一次回傳多筆錯誤。畫面上的完整錯誤訊息
                    //以最後一筆為結尾，因此 Cursor 與 Pointer 也採最後一筆有效目標。
                    match = current;
                }
            }

            if (match != null && match.Found)
            {
                output.UsedSecondaryErrorMessage = true;
                output.PositionResult = MapMatchToEditor(request, context, match);

                output.ShouldAppendStringConcatenationHint = ShouldAppendStringConcatenationHint
                (
                    output.ErrorMessage,
                    context.ExecutedSql,
                    match.Position,
                    match.Text
                );

                return output;
            }

            var target = ExtractTargetFromErrorMessage(output.ErrorMessage);

            var errorLine = ResolveErrorLine
            (
                request.ErrorCode,
                output.ErrorMessage
            );

            if (!string.IsNullOrEmpty(target) && TryFindTarget(context.ExecutedSql, target, errorLine, preferredPositionInLine: 0, out match))
            {
                output.PositionResult = MapMatchToEditor(request, context, match);

                output.ShouldAppendStringConcatenationHint = ShouldAppendStringConcatenationHint
                (
                    output.ErrorMessage,
                    context.ExecutedSql,
                    match.Position,
                    match.Text
                );

                return output;
            }

            output.PositionResult = CreateReportedPositionFallback
            (
                request
            );

            return output;
        }

        private static ExecutionContext CreateExecutionContext(SqlServerErrorPositionRequest request)
        {
            var originalSql = !string.IsNullOrEmpty(request.OriginalExecutedSql)
                              ? request.OriginalExecutedSql
                              : request.ExecutedSql;

            var editorStart = FindExecutionStart
            (
                request.EditorSql,
                originalSql,
                request.PreferredExecutionStart
            );

            return new ExecutionContext
            {
                EditorStart = Math.Max(0, editorStart),
                OriginalSql = originalSql ?? string.Empty,
                ExecutedSql = request.ExecutedSql ?? string.Empty
            };
        }

        private static int FindExecutionStart(string editorSql, string executedSql, int preferredStart)
        {
            if (string.IsNullOrEmpty(editorSql))
            {
                return 0;
            }

            preferredStart = Clamp(preferredStart, 0, editorSql.Length);

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

            index = editorSql.IndexOf
            (
                executedSql,
                StringComparison.Ordinal
            );

            return index >= 0 ? index : preferredStart;
        }

        private static bool TryResolveSecondaryItem(string executedSql, SqlServerSecondaryErrorItem item, out CandidateMatch match)
        {
            match = null;

            if (item == null || string.IsNullOrEmpty(executedSql) || string.IsNullOrEmpty(item.TargetText))
            {
                return false;
            }

            return TryFindTarget
            (
                executedSql,
                NormalizeErrorTarget(item.TargetText),
                item.LineNumber,
                item.PositionInLine,
                out match
            );
        }

        private static bool TryFindTarget(string sql, string target, int lineNumber, int preferredPositionInLine, out CandidateMatch match)
        {
            match = null;

            if (string.IsNullOrEmpty(sql) || string.IsNullOrEmpty(target))
            {
                return false;
            }

            var lines = SplitLines(sql);

            if (lineNumber > 0 && lineNumber <= lines.Length)
            {
                var lineStart = GetLineStart(lines, lineNumber - 1);

                var lineMatch = FindBestCandidateInText
                (
                    lines[lineNumber - 1],
                    target,
                    preferredPositionInLine
                );

                if (lineMatch.Found)
                {
                    lineMatch.Position += lineStart;
                    match = lineMatch;
                    return true;
                }
            }

            var fullMatch = FindBestCandidateInText
            (
                sql,
                target,
                preferredPosition: 0
            );

            if (!fullMatch.Found)
            {
                return false;
            }

            match = fullMatch;
            return true;
        }

        private static CandidateMatch FindBestCandidateInText(string text, string target, int preferredPosition)
        {
            var best = new CandidateMatch();

            foreach (var candidate in BuildTargetCandidates(target))
            {
                var searchStart = 0;

                while (searchStart <= text.Length)
                {
                    var index = text.IndexOf
                    (
                        candidate,
                        searchStart,
                        StringComparison.OrdinalIgnoreCase
                    );

                    if (index < 0)
                    {
                        break;
                    }

                    if (!IsInsideSingleQuotedString(text, index))
                    {
                        var distance = Math.Abs(index - preferredPosition);

                        if (!best.Found || distance < best.Distance || distance == best.Distance && candidate.Length > best.Text.Length)
                        {
                            best.Found = true;
                            best.Position = index;
                            best.Text = text.Substring(index, candidate.Length);
                            best.Distance = distance;
                        }
                    }

                    searchStart = index + Math.Max(1, candidate.Length);
                }
            }

            return best;
        }

        private static IReadOnlyList<string> BuildTargetCandidates(string target)
        {
            var result = new List<string>();
            var normalized = NormalizeErrorTarget(target);

            AddCandidate(result, normalized);

            if (normalized.IndexOf('.') >= 0)
            {
                var parts = normalized.Split
                (
                    new[] { '.' },
                    StringSplitOptions.RemoveEmptyEntries
                );

                if (parts.Length >= 2 && parts.Length <= 3)
                {
                    var combinations = 1 << parts.Length;

                    for (var mask = 0; mask < combinations; mask++)
                    {
                        var values = new string[parts.Length];

                        for (var i = 0; i < parts.Length; i++)
                        {
                            values[i] = (mask & 1 << i) != 0 ? QuoteBracket(parts[i]) : parts[i];
                        }

                        AddCandidate(result, string.Join(".", values));
                    }

                    AddCandidate(result, QuoteBracket(normalized));
                }
            }
            else
            {
                AddCandidate(result, QuoteBracket(normalized));
                AddCandidate(result, $"\"{normalized.Replace("\"", "\"\"")}\"");
            }

            return result.Where(value => !string.IsNullOrEmpty(value))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderByDescending(value => value.Length)
                         .ToArray();
        }

        private static void AddCandidate(ICollection<string> values, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                values.Add(value);
            }
        }

        private static string QuoteBracket(string value)
        {
            return $"[{(value ?? string.Empty).Replace("]", "]]" )}]";
        }

        private static string NormalizeErrorTarget(string target)
        {
            var result = (target ?? string.Empty).Trim();

            if (result.Length >= 2 && result[0] == '\'' && result[result.Length - 1] == '\'')
            {
                result = result.Substring(1, result.Length - 2);
            }

            return result.Replace("''", "'");
        }

        private static string ExtractTargetFromErrorMessage(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                return string.Empty;
            }

            var prefixes = new[]
            {
                "Invalid object name",
                "Invalid column name",
                "Incorrect syntax near the keyword",
                "Incorrect syntax near"
            };

            var selectedIndex = -1;
            var selectedPrefix = string.Empty;

            foreach (var prefix in prefixes)
            {
                var index = errorMessage.LastIndexOf
                (
                    prefix,
                    StringComparison.OrdinalIgnoreCase
                );

                if (index > selectedIndex)
                {
                    selectedIndex = index;
                    selectedPrefix = prefix;
                }
            }

            if (selectedIndex < 0)
            {
                return string.Empty;
            }

            var quoteStart = errorMessage.IndexOf
            (
                '\'',
                selectedIndex + selectedPrefix.Length
            );

            if (quoteStart < 0)
            {
                return string.Empty;
            }

            var lineEnd = errorMessage.IndexOfAny
            (
                new[] { '\r', '\n' },
                quoteStart + 1
            );

            if (lineEnd < 0)
            {
                lineEnd = errorMessage.Length;
            }

            var quoteSearchCount = lineEnd - quoteStart - 1;

            if (quoteSearchCount <= 0)
            {
                return string.Empty;
            }

            var quoteEnd = errorMessage.LastIndexOf
            (
                '\'',
                lineEnd - 1,
                quoteSearchCount
            );

            if (quoteEnd <= quoteStart)
            {
                return string.Empty;
            }

            return NormalizeErrorTarget
            (
                errorMessage.Substring
                (
                    quoteStart + 1,
                    quoteEnd - quoteStart - 1
                )
            );
        }

        private static int ResolveErrorLine(string errorCode, string errorMessage)
        {
            var text = $"{errorCode}\r\n{errorMessage}";

            var matches = Regex.Matches
            (
                text,
                @"(?:ErrorLine\s*:|\bLine\b|列)\s*:?[ ]*(?<line>\d+)",
                RegexOptions.IgnoreCase
            );

            if (matches.Count == 0)
            {
                return -1;
            }

            var value = matches[matches.Count - 1].Groups["line"].Value;

            return int.TryParse(value, out var lineNumber) ? lineNumber : -1;
        }

        private static SqlErrorResolutionResult MapMatchToEditor(SqlServerErrorPositionRequest request, ExecutionContext context, CandidateMatch match)
        {
            var position = context.EditorStart + match.Position;
            var target = match.Text;

            if (!string.IsNullOrEmpty(request.ParameterPositionMapping)
                && SqlParameterPositionMapper.TryMapExecutedSqlPositionToOriginalSqlPosition
                   (
                       request.ParameterPositionMapping,
                       request.ParameterStartPosition,
                       request.ParameterStartPosition + match.Position,
                       out var mappedPosition,
                       out var mappedParameterName
                   ))
            {
                position = mappedPosition;

                if (!string.IsNullOrEmpty(mappedParameterName))
                {
                    target = mappedParameterName;
                }
            }

            return new SqlErrorResolutionResult
            {
                Found = position >= 0,
                Position = Math.Max(0, position),
                Length = target?.Length ?? 0,
                TargetText = target ?? string.Empty,
                ShouldSetSquiggle = !string.IsNullOrEmpty(target)
            };
        }

        private static SqlErrorResolutionResult CreateReportedPositionFallback(SqlServerErrorPositionRequest request)
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

        private static bool ShouldAppendStringConcatenationHint(string errorMessage, string sql, int position, string target)
        {
            if ((errorMessage ?? string.Empty).IndexOf("Incorrect syntax near '|'", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            if (string.Equals(target, "|", StringComparison.Ordinal))
            {
                return true;
            }

            return position >= 0 && position < (sql ?? string.Empty).Length && sql[position] == '|';
        }

        private static string[] SplitLines(string sql)
        {
            return (sql ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split(new[] { '\n' });
        }

        private static int GetLineStart(string[] lines, int lineIndex)
        {
            var result = 0;

            for (var i = 0; i < lineIndex && i < lines.Length; i++)
            {
                //QueryForm／Reader 都以 CRLF 作為 SQL 的標準換行。
                result += lines[i].Length + 2;
            }

            return result;
        }

        private static bool IsInsideSingleQuotedString(string text, int index)
        {
            var insideSingle = false;
            var insideDouble = false;
            var insideBracket = false;

            for (var i = 0; i < index && i < text.Length; i++)
            {
                var current = text[i];

                if (insideSingle)
                {
                    if (current == '\'' && i + 1 < index && text[i + 1] == '\'')
                    {
                        i++;
                        continue;
                    }

                    if (current == '\'')
                    {
                        insideSingle = false;
                    }

                    continue;
                }

                if (insideDouble)
                {
                    if (current == '"' && i + 1 < index && text[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    if (current == '"')
                    {
                        insideDouble = false;
                    }

                    continue;
                }

                if (insideBracket)
                {
                    if (current == ']' && i + 1 < index && text[i + 1] == ']')
                    {
                        i++;
                        continue;
                    }

                    if (current == ']')
                    {
                        insideBracket = false;
                    }

                    continue;
                }

                if (current == '\'')
                {
                    insideSingle = true;
                }
                else if (current == '"')
                {
                    insideDouble = true;
                }
                else if (current == '[')
                {
                    insideBracket = true;
                }
            }

            return insideSingle;
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
