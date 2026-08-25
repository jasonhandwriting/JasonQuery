using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.CreateScript.PostgreSql.Formatting
{
    internal static class PostgreSqlCreateScriptFormatterHelper
    {
        public static string NormalizeLineEndings(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Replace("\r\n", "\n")
                       .Replace("\r", "\n")
                       .Replace("\n", "\r\n")
                       .TrimEnd('\r', '\n');
        }

        public static string TrimOuterBlankLines(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim('\r', '\n', ' ', '\t');
        }

        public static void SplitHeaderAndBody(string script, out string header, out string body)
        {
            header = string.Empty;
            body = string.Empty;

            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            var lines = NormalizeLineEndings(script).Split(new[] { "\r\n" }, StringSplitOptions.None);
            var sbHeader = new StringBuilder();
            var i = 0;

            while (i < lines.Length)
            {
                var line = lines[i];

                if (!line.TrimStart().StartsWith("--", StringComparison.Ordinal))
                {
                    break;
                }

                if (sbHeader.Length > 0)
                {
                    sbHeader.AppendLine();
                }

                sbHeader.Append(line.TrimEnd());
                i++;
            }

            while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i]))
            {
                i++;
            }

            header = sbHeader.ToString().TrimEnd('\r', '\n');
            body = string.Join("\r\n", lines.Skip(i)).Trim('\r', '\n');
        }

        public static string CombineHeaderAndBody(string header, string body)
        {
            header = TrimOuterBlankLines(header);
            body = TrimOuterBlankLines(body);

            if (string.IsNullOrWhiteSpace(header))
            {
                return body;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return header;
            }

            return $"{header}\r\n\r\n{body}";
        }

        public static List<string> SplitIntoBlocks(string text)
        {
            var list = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return list;
            }

            var lines = NormalizeLineEndings(text).Split(new[] { "\r\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    if (sb.Length > 0)
                    {
                        list.Add(sb.ToString().TrimEnd('\r', '\n'));
                        sb.Clear();
                    }

                    continue;
                }

                sb.AppendLine(line.TrimEnd());
            }

            if (sb.Length > 0)
            {
                list.Add(sb.ToString().TrimEnd('\r', '\n'));
            }

            return list;
        }

        public static string JoinBlocks(IEnumerable<string> listBlocks)
        {
            var blocks = (listBlocks ?? Enumerable.Empty<string>())
                         .Where(x => !string.IsNullOrWhiteSpace(x))
                         .Select(x => TrimOuterBlankLines(x))
                         .Where(x => !string.IsNullOrWhiteSpace(x))
                         .ToList();

            return string.Join("\r\n\r\n", blocks);
        }

        public static bool TrySplitTrailingOwnerStatement(string body, out string mainBody, out string ownerStatement)
        {
            mainBody = TrimOuterBlankLines(body);
            ownerStatement = string.Empty;

            if (string.IsNullOrWhiteSpace(mainBody))
            {
                return false;
            }

            var match = Regex.Match
            (
                mainBody,
                @"^(?<main>[\s\S]*?)\r\n\r\n(?<owner>ALTER\s+(TABLE|VIEW|INDEX)\s+[\s\S]*?;\s*)$",
                RegexOptions.IgnoreCase
            );

            if (!match.Success)
            {
                return false;
            }

            mainBody = TrimOuterBlankLines(match.Groups["main"].Value);
            ownerStatement = TrimOuterBlankLines(match.Groups["owner"].Value);

            return !string.IsNullOrWhiteSpace(ownerStatement);
        }

        public static string NormalizeSimpleBlocks(string body)
        {
            var blocks = SplitIntoBlocks(body);

            return JoinBlocks(blocks);
        }
    }
}
