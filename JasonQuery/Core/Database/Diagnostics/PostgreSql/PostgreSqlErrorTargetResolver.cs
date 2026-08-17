using JasonQuery.Core.Text;
using System;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    /// <summary>
    /// Resolves PostgreSQL error-message targets back to the matching text in the original SQL statement.
    /// </summary>
    internal static class PostgreSqlErrorTargetResolver
    {
        private static readonly string[] SupportedObjectKinds =
        {
            "foreign data wrapper",
            "foreign server",
            "materialized view",
            "large object",
            "foreign table",
            "table",
            "sequence",
            "schema",
            "database",
            "function",
            "procedure",
            "routine",
            "language",
            "domain",
            "type",
            "tablespace",
            "view",
            "relation",
            "parameter"
        };

        public static bool TryResolveDoubleQuotedIdentifierTarget(string sql, string errorCode, int positionOriginal, int positionOffset,
                                                                  string targetWord, out int resolvedPosition, out string resolvedTargetWord)
        {
            resolvedPosition = 0;
            resolvedTargetWord = string.Empty;

            if (string.IsNullOrEmpty(sql))
            {
                return false;
            }

            if (string.IsNullOrEmpty(targetWord))
            {
                return false;
            }

            if (!IsDoubleQuotedIdentifierTargetError(errorCode))
            {
                return false;
            }

            if (targetWord.IndexOf("\"", StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            var expectedPosition = Math.Max(0, positionOriginal + positionOffset);
            var candidates = BuildIdentifierTextCandidates(targetWord);

            return TryFindNearestTextCandidatePosition(sql, candidates, expectedPosition, 5, out resolvedPosition, out resolvedTargetWord);
        }

        public static bool TryResolvePermissionDeniedTarget(string sql, string sqlExecuted, string errorCode, string errorMessage,
                                                            int queryStartPosition, out int resolvedPosition, out string resolvedTargetWord)
        {
            resolvedPosition = 0;
            resolvedTargetWord = string.Empty;

            if (!CanResolveAccessErrorTarget(sql, sqlExecuted, errorCode))
            {
                return false;
            }

            if (!TryExtractAccessErrorTarget(errorMessage, "permission denied for ", out string targetObjectName))
            {
                return false;
            }

            return TryResolveObjectTargetInExecutedRange(sql, sqlExecuted, queryStartPosition, targetObjectName,
                                                         out resolvedPosition, out resolvedTargetWord);
        }

        public static bool TryResolveMustBeOwnerTarget(string sql, string sqlExecuted, string errorCode, string errorMessage,
                                                       int queryStartPosition, out int resolvedPosition, out string resolvedTargetWord)
        {
            resolvedPosition = 0;
            resolvedTargetWord = string.Empty;

            if (!CanResolveAccessErrorTarget(sql, sqlExecuted, errorCode))
            {
                return false;
            }

            if (!TryExtractAccessErrorTarget(errorMessage, "must be owner of ", out string targetObjectName))
            {
                return false;
            }

            return TryResolveObjectTargetInExecutedRange(sql, sqlExecuted, queryStartPosition, targetObjectName,
                                                         out resolvedPosition, out resolvedTargetWord);
        }

        private static bool CanResolveAccessErrorTarget(string sql, string sqlExecuted, string errorCode)
        {
            if (string.IsNullOrEmpty(sql) || string.IsNullOrEmpty(sqlExecuted))
            {
                return false;
            }

            return !string.IsNullOrEmpty(errorCode) && errorCode.IndexOf("42501", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool TryResolveObjectTargetInExecutedRange(string sql, string sqlExecuted, int queryStartPosition, string targetObjectName,
                                                                  out int resolvedPosition, out string resolvedTargetWord)
        {
            var rangeStart = Math.Max(0, queryStartPosition);
            var rangeEnd = Math.Min(sql.Length, rangeStart + sqlExecuted.Length);

            return TryFindObjectTargetInRange(sql, targetObjectName, rangeStart, rangeEnd, out resolvedPosition, out resolvedTargetWord);
        }

        private static bool TryExtractAccessErrorTarget(string errorMessage, string prefix, out string targetObjectName)
        {
            targetObjectName = string.Empty;

            if (string.IsNullOrEmpty(errorMessage) || string.IsNullOrEmpty(prefix))
            {
                return false;
            }

            if (!errorMessage.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var value = errorMessage.Substring(prefix.Length).Trim();

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (var objectKind in SupportedObjectKinds)
            {
                var objectKindPrefix = $"{objectKind} ";

                if (!value.StartsWith(objectKindPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                targetObjectName = value.Substring(objectKindPrefix.Length).Trim();
                targetObjectName = TrimErrorTargetText(targetObjectName);

                return !string.IsNullOrEmpty(targetObjectName);
            }

            return false;
        }

        private static string TrimErrorTargetText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Trim().TrimEnd('.', ',', ';');
        }

        private static bool IsDoubleQuotedIdentifierTargetError(string errorCode)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                return false;
            }

            //42703: undefined_column
            //42P01: undefined_table / relation XXX does not exist
            return errorCode.IndexOf("42703", StringComparison.OrdinalIgnoreCase) >= 0
                   || errorCode.IndexOf("42P01", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string[] BuildIdentifierTextCandidates(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return Array.Empty<string>();
            }

            if (identifier.IndexOf(".", StringComparison.Ordinal) < 0)
            {
                return new[]
                {
                    $"\"{identifier}\"",
                    identifier
                };
            }

            var parts = identifier.Split('.');

            if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
            {
                return new[]
                {
                    $"\"{identifier}\"",
                    identifier
                };
            }

            var schemaName = parts[0];
            var objectName = parts[1];

            return new[]
            {
                $"\"{identifier}\"",
                $"\"{schemaName}\".\"{objectName}\"",
                $"{schemaName}.\"{objectName}\"",
                $"\"{schemaName}\".{objectName}",
                identifier
            };
        }

        private static bool TryFindNearestTextCandidatePosition(string source, string[] candidates, int expectedPosition, int maxDistance,
                                                                out int nearestPosition, out string nearestText)
        {
            nearestPosition = -1;
            nearestText = string.Empty;

            if (string.IsNullOrEmpty(source) || candidates == null || candidates.Length == 0)
            {
                return false;
            }

            var bestDistance = int.MaxValue;

            foreach (var candidate in candidates)
            {
                if (string.IsNullOrEmpty(candidate))
                {
                    continue;
                }

                if (!TryFindNearestTextPosition(source, candidate, expectedPosition, maxDistance, out int candidatePosition))
                {
                    continue;
                }

                var distance = GetIdentifierCandidateDistance(candidate, candidatePosition, expectedPosition);

                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                nearestPosition = candidatePosition;
                nearestText = candidate;
            }

            return nearestPosition >= 0;
        }

        private static bool TryFindNearestTextPosition(string source, string target, int expectedPosition, int maxDistance, out int nearestPosition)
        {
            nearestPosition = -1;

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                return false;
            }

            var bestDistance = int.MaxValue;
            var index = source.IndexOf(target, StringComparison.OrdinalIgnoreCase);

            while (index >= 0)
            {
                var distance = GetIdentifierCandidateDistance(target, index, expectedPosition);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearestPosition = index;
                }

                index = source.IndexOf(target, index + 1, StringComparison.OrdinalIgnoreCase);
            }

            return nearestPosition >= 0 && bestDistance <= maxDistance;
        }

        private static int GetIdentifierCandidateDistance(string candidate, int candidatePosition, int expectedPosition)
        {
            var comparisonPosition = candidatePosition;

            //20260719 ErrorPosition 為識別字內容起點時，若 SQL 使用整段雙引號，例如 "public.custinfo23"，應將候選字串的比較位置視為開頭雙引號後一個字元
            if (!string.IsNullOrEmpty(candidate) && candidate[0] == '"')
            {
                comparisonPosition++;
            }

            return Math.Abs(comparisonPosition - expectedPosition);
        }

        private static bool TryFindObjectTargetInRange(string source, string targetObjectName, int rangeStart, int rangeEnd,
                                                       out int resolvedPosition, out string resolvedTargetWord)
        {
            resolvedPosition = -1;
            resolvedTargetWord = string.Empty;

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(targetObjectName))
            {
                return false;
            }

            rangeStart = Math.Max(0, rangeStart);
            rangeEnd = Math.Min(source.Length, rangeEnd);

            if (rangeStart >= rangeEnd)
            {
                return false;
            }

            var candidates = BuildObjectTargetLeafCandidates(targetObjectName);

            foreach (var candidate in candidates)
            {
                var searchIndex = rangeStart;

                while (searchIndex < rangeEnd)
                {
                    var index = source.IndexOf(candidate, searchIndex, rangeEnd - searchIndex, StringComparison.OrdinalIgnoreCase);

                    if (index < 0)
                    {
                        break;
                    }

                    if (IsObjectTargetCandidateMatch(source, index, candidate.Length, rangeStart, rangeEnd)
                        && TryExpandQualifiedIdentifier(source, index, candidate.Length, rangeStart, rangeEnd,
                                                        out int expandedPosition, out string expandedText))
                    {
                        if (resolvedPosition < 0 || expandedPosition < resolvedPosition)
                        {
                            resolvedPosition = expandedPosition;
                            resolvedTargetWord = expandedText;
                        }
                    }

                    searchIndex = index + candidate.Length;
                }
            }

            return resolvedPosition >= 0;
        }

        private static string[] BuildObjectTargetLeafCandidates(string targetObjectName)
        {
            if (string.IsNullOrEmpty(targetObjectName))
            {
                return Array.Empty<string>();
            }

            if (targetObjectName.IndexOf("\"", StringComparison.Ordinal) >= 0)
            {
                return new[]
                {
                    targetObjectName
                };
            }

            return new[]
            {
                $"\"{targetObjectName}\"",
                targetObjectName
            };
        }

        private static bool IsObjectTargetCandidateMatch(string source, int index, int length, int rangeStart, int rangeEnd)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            if (index < rangeStart || index + length > rangeEnd)
            {
                return false;
            }

            var startsWithQuote = source[index] == '"';
            var endsWithQuote = source[index + length - 1] == '"';

            if (startsWithQuote && endsWithQuote)
            {
                return true;
            }

            var leftIndex = index - 1;

            if (leftIndex >= rangeStart && IsUnquotedIdentifierPart(source[leftIndex]))
            {
                return false;
            }

            var rightIndex = index + length;

            if (rightIndex < rangeEnd && IsUnquotedIdentifierPart(source[rightIndex]))
            {
                return false;
            }

            return true;
        }

        private static bool IsUnquotedIdentifierPart(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }

        private static bool TryExpandQualifiedIdentifier(string source, int targetPosition, int targetLength, int rangeStart, int rangeEnd,
                                                         out int expandedPosition, out string expandedText)
        {
            expandedPosition = targetPosition;
            expandedText = TextHelper.GetSafeSubstring(source, targetPosition, targetLength);

            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            if (targetPosition < rangeStart || targetPosition + targetLength > rangeEnd)
            {
                return false;
            }

            var start = targetPosition;
            var end = targetPosition + targetLength;

            if (start - 1 >= rangeStart && source[start - 1] == '.')
            {
                if (TryFindIdentifierTokenStartBeforeDot(source, start - 2, rangeStart, out int qualifierStart))
                {
                    start = qualifierStart;
                }
            }

            expandedPosition = start;
            expandedText = TextHelper.GetSafeSubstring(source, start, end - start);

            return !string.IsNullOrEmpty(expandedText);
        }

        private static bool TryFindIdentifierTokenStartBeforeDot(string source, int startIndex, int rangeStart, out int tokenStart)
        {
            tokenStart = -1;

            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            if (startIndex < rangeStart)
            {
                return false;
            }

            if (source[startIndex] == '"')
            {
                for (var i = startIndex - 1; i >= rangeStart; i--)
                {
                    if (source[i] != '"')
                    {
                        continue;
                    }

                    tokenStart = i;
                    return true;
                }

                return false;
            }

            var index = startIndex;

            while (index >= rangeStart && IsUnquotedIdentifierPart(source[index]))
            {
                index--;
            }

            tokenStart = index + 1;

            return tokenStart <= startIndex;
        }
    }
}