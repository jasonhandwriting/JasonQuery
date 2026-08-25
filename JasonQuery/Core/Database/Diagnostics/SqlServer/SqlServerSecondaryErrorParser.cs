using JasonQuery.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JasonQuery.Core.Database.Diagnostics.SqlServer
{
    internal static class SqlServerSecondaryErrorParser
    {
        public static IReadOnlyList<SqlServerSecondaryErrorItem> Parse(string value)
        {
            var result = new List<SqlServerSecondaryErrorItem>();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            var items = value.Split
            (
                new[] { MyGlobal.SeparatorPlus3 },
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var item in items)
            {
                var parts = item.Split
                (
                    new[] { MyGlobal.SeparatorPlus4 },
                    StringSplitOptions.None
                );

                if (parts.Length < 3
                    || !int.TryParse(parts[0], out var lineNumber)
                    || !int.TryParse(parts[2], out var positionInLine)
                    || lineNumber <= 0
                    || positionInLine < 0
                    || string.IsNullOrEmpty(parts[1]))
                {
                    continue;
                }

                result.Add
                (
                    new SqlServerSecondaryErrorItem
                    {
                        LineNumber = lineNumber,
                        TargetText = parts[1],
                        PositionInLine = positionInLine
                    }
                );
            }

            return result.OrderBy(item => item.LineNumber)
                         .ThenBy(item => item.PositionInLine)
                         .ToArray();
        }
    }
}
