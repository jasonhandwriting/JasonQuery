using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.CreateScript.PostgreSql.Formatting
{
    internal static class PostgreSqlRoutineCreateScriptFormatterHelper
    {
        private static readonly string[] RoutineOptionStarts =
        {
            "LANGUAGE ",
            "TRANSFORM ",
            "WINDOW",
            "IMMUTABLE",
            "STABLE",
            "VOLATILE",
            "NOT LEAKPROOF",
            "LEAKPROOF",
            "EXTERNAL SECURITY DEFINER",
            "EXTERNAL SECURITY INVOKER",
            "SECURITY DEFINER",
            "SECURITY INVOKER",
            "PARALLEL ",
            "COST ",
            "ROWS ",
            "SUPPORT ",
            "CALLED ON NULL INPUT",
            "RETURNS NULL ON NULL INPUT",
            "STRICT",
            "SET "
        };

        private static readonly Regex OptionStartRegex = new Regex
        (
            @"(?ix)
            (?=
                \bEXTERNAL\ SECURITY\ DEFINER\b |
                \bEXTERNAL\ SECURITY\ INVOKER\b |
                \bRETURNS\ NULL\ ON\ NULL\ INPUT\b |
                \bCALLED\ ON\ NULL\ INPUT\b |
                \bNOT\ LEAKPROOF\b |
                \bSECURITY\ DEFINER\b |
                \bSECURITY\ INVOKER\b |
                \bLANGUAGE\b |
                \bTRANSFORM\b |
                \bWINDOW\b |
                \bIMMUTABLE\b |
                \bSTABLE\b |
                \bVOLATILE\b |
                \bLEAKPROOF\b |
                \bPARALLEL\b |
                \bCOST\b |
                \bROWS\b |
                \bSUPPORT\b |
                \bSET\b(?!OF\b) |
                \bSTRICT\b
            )",
            RegexOptions.Compiled
        );

        public static string FormatRoutineBody(string body)
        {
            body = PostgreSqlCreateScriptFormatterHelper.NormalizeLineEndings(body);
            body = PostgreSqlCreateScriptFormatterHelper.TrimOuterBlankLines(body);

            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            var lines = body.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
            var bodyStart = FindRoutineBodyStart(lines);

            if (bodyStart < 0)
            {
                return PostgreSqlCreateScriptFormatterHelper.NormalizeSimpleBlocks(body);
            }

            var listDeclarationLines = lines.Take(bodyStart).ToList();
            var listBodyLines = lines.Skip(bodyStart).ToList();

            var declaration = FormatRoutineDeclaration(listDeclarationLines);
            var routineBody = PostgreSqlCreateScriptFormatterHelper.TrimOuterBlankLines(string.Join("\r\n", listBodyLines));

            return PostgreSqlCreateScriptFormatterHelper.JoinBlocks
            (
                new[]
                {
                    declaration,
                    routineBody
                }
            );
        }

        private static int FindRoutineBodyStart(IReadOnlyList<string> lines)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var line = (lines[i] ?? string.Empty).TrimStart();

                if (line.StartsWith("AS ", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(line, "AS", StringComparison.OrdinalIgnoreCase)
                    || line.StartsWith("BEGIN ATOMIC", StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string FormatRoutineDeclaration(IReadOnlyList<string> listDeclarationLines)
        {
            var listHeaderLines = new List<string>();
            var listOptionFragments = new List<string>();
            var isOptionSectionStarted = false;

            foreach (var lineRaw in listDeclarationLines ?? Enumerable.Empty<string>())
            {
                var line = (lineRaw ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!isOptionSectionStarted && !StartsWithRoutineOption(line))
                {
                    listHeaderLines.Add(line);
                }
                else
                {
                    isOptionSectionStarted = true;
                    listOptionFragments.Add(line);
                }
            }

            if (listHeaderLines.Count > 0 && TrySplitInlineOptions(listHeaderLines[listHeaderLines.Count - 1], out var headerOnly, out var inlineOptions))
            {
                listHeaderLines[listHeaderLines.Count - 1] = headerOnly;

                if (!string.IsNullOrWhiteSpace(inlineOptions))
                {
                    listOptionFragments.Insert(0, inlineOptions);
                }
            }

            var listOptionLines = SplitRoutineOptionClauses(listOptionFragments);
            var sb = new StringBuilder();

            for (var i = 0; i < listHeaderLines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(listHeaderLines[i]))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                if (i == 0)
                {
                    sb.Append(listHeaderLines[i].TrimEnd());
                }
                else
                {
                    sb.Append("    ").Append(listHeaderLines[i].TrimEnd());
                }
            }

            foreach (var optionLine in listOptionLines)
            {
                if (string.IsNullOrWhiteSpace(optionLine))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.Append("    ").Append(optionLine.TrimEnd());
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static bool StartsWithRoutineOption(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            for (var i = 0; i < RoutineOptionStarts.Length; i++)
            {
                if (line.StartsWith(RoutineOptionStarts[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TrySplitInlineOptions(string line, out string headerOnly, out string inlineOptions)
        {
            headerOnly = line ?? string.Empty;
            inlineOptions = string.Empty;

            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            var matches = OptionStartRegex.Matches(line);

            if (matches.Count <= 0)
            {
                return false;
            }

            for (var i = 0; i < matches.Count; i++)
            {
                var index = matches[i].Index;

                if (index <= 0 || index >= line.Length)
                {
                    continue;
                }

                if (!char.IsWhiteSpace(line[index - 1]))
                {
                    continue;
                }

                headerOnly = line.Substring(0, index).TrimEnd();
                inlineOptions = line.Substring(index).Trim();

                return !string.IsNullOrWhiteSpace(inlineOptions);
            }

            return false;
        }

        private static List<string> SplitRoutineOptionClauses(IEnumerable<string> listOptionFragments)
        {
            var optionText = string.Join(" ", (listOptionFragments ?? Enumerable.Empty<string>())
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .Select(x => x.Trim()));

            optionText = Regex.Replace(optionText, @"\s+", " ").Trim();

            var listClauses = new List<string>();

            if (string.IsNullOrWhiteSpace(optionText))
            {
                return listClauses;
            }

            var matches = OptionStartRegex.Matches(optionText);

            if (matches.Count <= 0)
            {
                listClauses.Add(optionText);
                return listClauses;
            }

            var listIndexes = new List<int>();

            for (var i = 0; i < matches.Count; i++)
            {
                if (!listIndexes.Contains(matches[i].Index))
                {
                    listIndexes.Add(matches[i].Index);
                }
            }

            listIndexes = listIndexes.OrderBy(x => x).ToList();

            for (var i = 0; i < listIndexes.Count; i++)
            {
                var start = listIndexes[i];
                var end = i < listIndexes.Count - 1 ? listIndexes[i + 1] : optionText.Length;
                var clause = optionText.Substring(start, end - start).Trim();

                if (!string.IsNullOrWhiteSpace(clause))
                {
                    listClauses.Add(clause);
                }
            }

            return listClauses;
        }
    }
}