using JasonQuery.Core.Database.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal sealed class SqlServerIndexCreateScriptFormatter : SqlServerCreateScriptFormatterBase
    {
        private sealed class IndexColumnItem
        {
            public string Expression { get; set; }
            public string Direction { get; set; }
        }

        private sealed class IndexOptionItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string RawText { get; set; }
        }

        protected override bool MatchesSchemaType(string schemaType)
        {
            return SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Indexes);
        }

        protected override string FormatBody(string body, SqlServerCreateScriptFormatContext context)
        {
            var batches = SqlServerCreateScriptFormatterHelper.SplitGoBatches(body);

            for (var i = 0; i < batches.Count; i++)
            {
                batches[i] = FormatIndexBatch(batches[i]);
            }

            return SqlServerCreateScriptFormatterHelper.JoinGoBatches(batches);
        }

        #region Main
        private static string FormatIndexBatch(string batch)
        {
            batch = SqlServerCreateScriptFormatterHelper.NormalizeBatchText(batch);

            if (string.IsNullOrWhiteSpace(batch))
            {
                return string.Empty;
            }

            if (!TrySplitIndexStatement(batch, out var header, out var columnBlock, out var betweenColumnAndWith, out var withBlock, out var tail))
            {
                return batch;
            }

            var sb = new StringBuilder();
            var formattedHeader = SqlServerClauseStyleFormatterHelper.FormatIndexHeader(header);

            if (!string.IsNullOrWhiteSpace(formattedHeader))
            {
                sb.AppendLine(formattedHeader);
            }

            sb.AppendLine("(");
            sb.Append(BuildFormattedColumnBlock(columnBlock));
            sb.Append(")");

            var middle = SqlServerClauseStyleFormatterHelper.FormatIndexClauseSegment(betweenColumnAndWith);

            if (!string.IsNullOrWhiteSpace(middle))
            {
                sb.AppendLine();
                sb.Append(middle);
            }

            if (!string.IsNullOrWhiteSpace(withBlock))
            {
                sb.AppendLine();
                sb.Append(SqlServerClauseStyleFormatterHelper.FormatWithClause(withBlock));
            }

            var formattedTail = SqlServerClauseStyleFormatterHelper.FormatIndexClauseSegment(tail);

            if (!string.IsNullOrWhiteSpace(formattedTail))
            {
                sb.AppendLine();
                sb.Append(formattedTail);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }
        #endregion

        #region Split
        private static bool TrySplitIndexStatement(string batch, out string header, out string columnBlock, out string betweenColumnAndWith,
                                                   out string withBlock, out string tail)
        {
            header = string.Empty;
            columnBlock = string.Empty;
            betweenColumnAndWith = string.Empty;
            withBlock = string.Empty;
            tail = string.Empty;

            if (string.IsNullOrWhiteSpace(batch))
            {
                return false;
            }

            var firstOpenParen = FindFirstTopLevelChar(batch, '(');

            if (firstOpenParen < 0)
            {
                return false;
            }

            var firstCloseParen = FindMatchingParen(batch, firstOpenParen);

            if (firstCloseParen < 0)
            {
                return false;
            }

            header = batch.Substring(0, firstOpenParen).TrimEnd();
            columnBlock = batch.Substring(firstOpenParen + 1, firstCloseParen - firstOpenParen - 1);

            var remain = batch.Substring(firstCloseParen + 1).Trim();

            if (string.IsNullOrWhiteSpace(remain))
            {
                return !string.IsNullOrWhiteSpace(header);
            }

            var withIndex = IndexOfTopLevelKeyword(remain, "WITH");

            if (withIndex < 0)
            {
                tail = remain;
                return !string.IsNullOrWhiteSpace(header);
            }

            betweenColumnAndWith = remain.Substring(0, withIndex).Trim();

            var withAndTail = remain.Substring(withIndex + 4).Trim();
            var withOpenParen = FindFirstTopLevelChar(withAndTail, '(');

            if (withOpenParen < 0)
            {
                tail = remain;
                return !string.IsNullOrWhiteSpace(header);
            }

            var withCloseParen = FindMatchingParen(withAndTail, withOpenParen);

            if (withCloseParen < 0)
            {
                tail = remain;
                return !string.IsNullOrWhiteSpace(header);
            }

            withBlock = withAndTail.Substring(withOpenParen + 1, withCloseParen - withOpenParen - 1);
            tail = withAndTail.Substring(withCloseParen + 1).Trim();

            return !string.IsNullOrWhiteSpace(header);
        }
        #endregion

        #region Column Block
        private static string BuildFormattedColumnBlock(string columnBlock)
        {
            var items = SplitTopLevelCommaItems(columnBlock);
            var columns = new List<IndexColumnItem>();

            foreach (var item in items)
            {
                var text = (item ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                columns.Add(ParseIndexColumnItem(text));
            }

            if (columns.Count <= 0)
            {
                return string.Empty;
            }

            var maxExpression = columns.Max(x => x.Expression.Length);
            var sb = new StringBuilder();

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var suffix = i == columns.Count - 1 ? string.Empty : ",";
                var padding = new string(' ', Math.Max(0, maxExpression - column.Expression.Length));

                sb.Append("    ");
                sb.Append(column.Expression);
                sb.Append(padding);

                if (!string.IsNullOrWhiteSpace(column.Direction))
                {
                    sb.Append("  ");
                    sb.Append(column.Direction);
                }

                sb.Append(suffix);
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static IndexColumnItem ParseIndexColumnItem(string text)
        {
            if (text.EndsWith(" ASC", StringComparison.OrdinalIgnoreCase))
            {
                return new IndexColumnItem
                {
                    Expression = text.Substring(0, text.Length - 4).TrimEnd(),
                    Direction = "ASC"
                };
            }

            if (text.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase))
            {
                return new IndexColumnItem
                {
                    Expression = text.Substring(0, text.Length - 5).TrimEnd(),
                    Direction = "DESC"
                };
            }

            return new IndexColumnItem
            {
                Expression = text.Trim(),
                Direction = string.Empty
            };
        }
        #endregion

        #region Text Helpers
        private static int IndexOfTopLevelKeyword(string text, string keyword)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
            {
                return -1;
            }

            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;
            var limit = text.Length - keyword.Length;

            for (var i = 0; i <= limit; i++)
            {
                var ch = text[i];

                if (!inDoubleQuote && !inBracket && ch == '\'')
                {
                    if (inSingleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\'')
                        {
                            i++;
                            continue;
                        }

                        inSingleQuote = false;
                    }
                    else
                    {
                        inSingleQuote = true;
                    }

                    continue;
                }

                if (!inSingleQuote && !inBracket && ch == '"')
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote)
                {
                    if (ch == '[')
                    {
                        inBracket = true;
                        continue;
                    }

                    if (ch == ']')
                    {
                        inBracket = false;
                        continue;
                    }
                }

                if (inSingleQuote || inDoubleQuote || inBracket)
                {
                    continue;
                }

                if (ch == '(')
                {
                    depth++;
                    continue;
                }

                if (ch == ')')
                {
                    depth--;
                    continue;
                }

                if (depth != 0)
                {
                    continue;
                }

                if (string.Compare(text, i, keyword, 0, keyword.Length, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var leftOk = i == 0 || char.IsWhiteSpace(text[i - 1]);
                    var rightPos = i + keyword.Length;
                    var rightOk = rightPos >= text.Length || char.IsWhiteSpace(text[rightPos]) || text[rightPos] == '(';

                    if (leftOk && rightOk)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static int FindFirstTopLevelChar(string text, char target)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return -1;
            }

            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (!inDoubleQuote && !inBracket && ch == '\'')
                {
                    if (inSingleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\'')
                        {
                            i++;
                            continue;
                        }

                        inSingleQuote = false;
                    }
                    else
                    {
                        inSingleQuote = true;
                    }

                    continue;
                }

                if (!inSingleQuote && !inBracket && ch == '"')
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote)
                {
                    if (ch == '[')
                    {
                        inBracket = true;
                        continue;
                    }

                    if (ch == ']')
                    {
                        inBracket = false;
                        continue;
                    }
                }

                if (inSingleQuote || inDoubleQuote || inBracket)
                {
                    continue;
                }

                if (ch == '(')
                {
                    if (depth == 0 && ch == target)
                    {
                        return i;
                    }

                    depth++;
                    continue;
                }

                if (ch == ')')
                {
                    depth--;
                }
            }

            return -1;
        }

        private static int FindMatchingParen(string text, int openParenIndex)
        {
            if (string.IsNullOrWhiteSpace(text) || openParenIndex < 0 || openParenIndex >= text.Length)
            {
                return -1;
            }

            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;

            for (var i = openParenIndex; i < text.Length; i++)
            {
                var ch = text[i];

                if (!inDoubleQuote && !inBracket && ch == '\'')
                {
                    if (inSingleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\'')
                        {
                            i++;
                            continue;
                        }

                        inSingleQuote = false;
                    }
                    else
                    {
                        inSingleQuote = true;
                    }

                    continue;
                }

                if (!inSingleQuote && !inBracket && ch == '"')
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote)
                {
                    if (ch == '[')
                    {
                        inBracket = true;
                        continue;
                    }

                    if (ch == ']')
                    {
                        inBracket = false;
                        continue;
                    }
                }

                if (inSingleQuote || inDoubleQuote || inBracket)
                {
                    continue;
                }

                if (ch == '(')
                {
                    depth++;
                }
                else if (ch == ')')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static List<string> SplitTopLevelCommaItems(string text)
        {
            var list = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return list;
            }

            var sb = new StringBuilder();
            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (!inDoubleQuote && !inBracket && ch == '\'')
                {
                    sb.Append(ch);

                    if (inSingleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\'')
                        {
                            sb.Append(text[i + 1]);
                            i++;
                            continue;
                        }

                        inSingleQuote = false;
                    }
                    else
                    {
                        inSingleQuote = true;
                    }

                    continue;
                }

                if (!inSingleQuote && !inBracket && ch == '"')
                {
                    sb.Append(ch);
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote)
                {
                    if (ch == '[')
                    {
                        sb.Append(ch);
                        inBracket = true;
                        continue;
                    }

                    if (ch == ']')
                    {
                        sb.Append(ch);
                        inBracket = false;
                        continue;
                    }
                }

                if (!inSingleQuote && !inDoubleQuote && !inBracket)
                {
                    if (ch == '(')
                    {
                        depth++;
                    }
                    else if (ch == ')')
                    {
                        depth--;
                    }
                    else if (ch == ',' && depth == 0)
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }

                sb.Append(ch);
            }

            if (sb.Length > 0)
            {
                list.Add(sb.ToString());
            }

            return list;
        }
        #endregion
    }
}
