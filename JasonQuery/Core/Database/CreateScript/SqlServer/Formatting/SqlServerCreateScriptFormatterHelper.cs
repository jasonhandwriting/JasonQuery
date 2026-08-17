using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal static class SqlServerCreateScriptFormatterHelper
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

        public static List<string> SplitGoBatches(string body)
        {
            var list = new List<string>();

            if (string.IsNullOrWhiteSpace(body))
            {
                return list;
            }

            var lines = NormalizeLineEndings(body).Split(new[] { "\r\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (string.Equals(line.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
                {
                    var block = TrimOuterBlankLines(sb.ToString());

                    if (!string.IsNullOrWhiteSpace(block))
                    {
                        list.Add(block);
                    }

                    sb.Clear();
                    continue;
                }

                sb.AppendLine(line.TrimEnd());
            }

            var lastBlock = TrimOuterBlankLines(sb.ToString());

            if (!string.IsNullOrWhiteSpace(lastBlock))
            {
                list.Add(lastBlock);
            }

            return list;
        }

        public static string JoinGoBatches(IEnumerable<string> batches)
        {
            var list = (batches ?? Enumerable.Empty<string>()).Select(NormalizeBatchText)
                                                              .Where(x => !string.IsNullOrWhiteSpace(x))
                                                              .ToList();

            if (list.Count <= 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            for (var i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }

                sb.AppendLine(list[i]);
                sb.Append("GO");
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        public static string NormalizeBatchText(string block)
        {
            if (string.IsNullOrWhiteSpace(block))
            {
                return string.Empty;
            }

            var lines = NormalizeLineEndings(block).Split(new[] { "\r\n" }, StringSplitOptions.None)
                                                   .Select(x => x.TrimEnd())
                                                   .ToList();

            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
            {
                lines.RemoveAt(0);
            }

            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
            {
                lines.RemoveAt(lines.Count - 1);
            }

            return string.Join("\r\n", lines).TrimEnd('\r', '\n');
        }

        public static string GetFirstMeaningfulLine(string block)
        {
            if (string.IsNullOrWhiteSpace(block))
            {
                return string.Empty;
            }

            var lines = NormalizeLineEndings(block).Split(new[] { "\r\n" }, StringSplitOptions.None);

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (!string.IsNullOrWhiteSpace(line))
                {
                    return line;
                }
            }

            return string.Empty;
        }

        public static string RemoveLeadingCommentLine(string block, params string[] prefixes)
        {
            if (string.IsNullOrWhiteSpace(block))
            {
                return string.Empty;
            }

            var lines = NormalizeLineEndings(block).Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();

            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
            {
                lines.RemoveAt(0);
            }

            if (lines.Count <= 0)
            {
                return string.Empty;
            }

            var firstLine = lines[0].Trim();

            foreach (var prefix in prefixes ?? Array.Empty<string>())
            {
                if (firstLine.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    lines.RemoveAt(0);
                    break;
                }
            }

            return TrimOuterBlankLines(string.Join("\r\n", lines));
        }
    }
}