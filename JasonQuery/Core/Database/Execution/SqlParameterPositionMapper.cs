using System;
using System.Collections.Generic;

namespace JasonQuery.Core.Database.Execution
{
    internal static class SqlParameterPositionMapper
    {
        private sealed class QueryParameterPositionInfo
        {
            public string Name { get; set; } = string.Empty;
            public int OriginalPosition { get; set; }
            public string Value { get; set; } = string.Empty;
            public int NameLength => Name?.Length ?? 0;
            public int ValueLength => Value?.Length ?? 0;
        }

        public static bool TryMapExecutedSqlPositionToOriginalSqlPosition(string parameterPositionMapping, int parameterStartPosition, int executedPosition,
                                                                          out int originalPosition, out string parameterName)
        {
            originalPosition = executedPosition;
            parameterName = string.Empty;

            var parameters = ParseQueryParameterPositionMapping(parameterPositionMapping, parameterStartPosition);

            if (parameters.Count == 0)
            {
                return false;
            }

            var executedMinusOriginalOffset = 0;

            foreach (var parameter in parameters)
            {
                var executedParameterStart = parameter.OriginalPosition + executedMinusOriginalOffset;
                var executedParameterEnd = executedParameterStart + parameter.ValueLength;

                if (executedPosition < executedParameterStart)
                {
                    originalPosition = executedPosition - executedMinusOriginalOffset;
                    return true;
                }

                if (executedPosition >= executedParameterStart && executedPosition < executedParameterEnd)
                {
                    originalPosition = parameter.OriginalPosition;
                    parameterName = parameter.Name;
                    return true;
                }

                executedMinusOriginalOffset += parameter.ValueLength - parameter.NameLength;
            }

            originalPosition = executedPosition - executedMinusOriginalOffset;
            return true;
        }

        public static int ResolveSqlServerExecutedPositionForParameterMapping(string sqlExecuted, string targetWord, string positionText, int fallbackPosition)
        {
            var wordPosition = FindNearestTextPosition(sqlExecuted, targetWord, fallbackPosition);

            if (wordPosition >= 0)
            {
                return wordPosition;
            }

            if (int.TryParse(positionText, out var parsedPosition))
            {
                return parsedPosition;
            }

            return fallbackPosition;
        }

        public static int FindNearestTextPosition(string text, string value, int nearPosition)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
            {
                return -1;
            }

            var bestPosition = -1;
            var bestDistance = int.MaxValue;
            var startIndex = 0;

            while (startIndex < text.Length)
            {
                var index = text.IndexOf(value, startIndex, StringComparison.OrdinalIgnoreCase);

                if (index < 0)
                {
                    break;
                }

                var distance = Math.Abs(index - nearPosition);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPosition = index;
                }

                startIndex = index + value.Length;
            }

            return bestPosition;
        }

        private static List<QueryParameterPositionInfo> ParseQueryParameterPositionMapping(string parameterPositionMapping, int parameterStartPosition)
        {
            var result = new List<QueryParameterPositionInfo>();

            if (string.IsNullOrEmpty(parameterPositionMapping))
            {
                return result;
            }

            var items = parameterPositionMapping.Split(new[] { "`" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                var parts = item.Split(new[] { '|' }, 3);

                if (parts.Length < 3)
                {
                    continue;
                }

                if (!int.TryParse(parts[1], out var relativePosition))
                {
                    continue;
                }

                result.Add(new QueryParameterPositionInfo
                {
                    Name = parts[0],
                    OriginalPosition = parameterStartPosition + relativePosition,
                    Value = parts[2]
                });
            }

            result.Sort((x, y) => x.OriginalPosition.CompareTo(y.OriginalPosition));
            return result;
        }
    }
}
