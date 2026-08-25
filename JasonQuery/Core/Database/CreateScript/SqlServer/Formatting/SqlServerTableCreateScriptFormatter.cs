using JasonQuery.Core.Database.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal sealed class SqlServerTableCreateScriptFormatter : SqlServerCreateScriptFormatterBase
    {
        private enum TableBatchKind
        {
            Table,
            Constraint,
            ExtendedProperty,
            Index,
            Other
        }

        private sealed class TableElement
        {
            public bool IsColumn { get; set; }
            public string OriginalText { get; set; }
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public string Other { get; set; }
            public string PreDefaultText { get; set; }
            public string DefaultClause { get; set; }
            public string NullabilityClause { get; set; }
        }

        private sealed class ColumnFormatWidths
        {
            public int ColumnNameWidth { get; set; }
            public int DataTypeWidth { get; set; }
            public int PreDefaultWidth { get; set; }
            public int DefaultClauseWidth { get; set; }
        }

        private sealed class TokenInfo
        {
            public string Text { get; set; }
            public int StartIndex { get; set; }
        }

        private static readonly string[] NonColumnPrefixes =
        {
            "CONSTRAINT ",
            "PRIMARY KEY",
            "UNIQUE ",
            "FOREIGN KEY",
            "CHECK ",
            "INDEX "
        };

        protected override bool MatchesSchemaType(string schemaType)
        {
            return SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables);
        }

        protected override string FormatBody(string body, SqlServerCreateScriptFormatContext context)
        {
            var batches = SqlServerCreateScriptFormatterHelper.SplitGoBatches(body);

            if (batches.Count <= 0)
            {
                return string.Empty;
            }

            var tableBatches = new List<string>();
            var constraintBatches = new List<string>();
            var extendedPropertyBatches = new List<string>();
            var indexBatches = new List<string>();
            var otherBatches = new List<string>();

            foreach (var batchRaw in batches)
            {
                var batch = SqlServerCreateScriptFormatterHelper.NormalizeBatchText(batchRaw);

                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                var kind = DetectBatchKind(batch);

                switch (kind)
                {
                    case TableBatchKind.Table:
                        {
                            tableBatches.Add(FormatCreateTableBatch(batch));
                            break;
                        }
                    case TableBatchKind.Constraint:
                        {
                            constraintBatches.Add(FormatConstraintBatch(batch));
                            break;
                        }
                    case TableBatchKind.ExtendedProperty:
                        {
                            extendedPropertyBatches.Add(FormatExtendedPropertyBatch(batch));
                            break;
                        }
                    case TableBatchKind.Index:
                        {
                            indexBatches.Add(FormatIndexBatch(batch));
                            break;
                        }
                    default:
                        {
                            otherBatches.Add(SqlServerCreateScriptFormatterHelper.NormalizeBatchText(batch));
                            break;
                        }
                }
            }

            var sb = new StringBuilder();

            AppendSection(sb, null, tableBatches);
            AppendSection(sb, "-- CONSTRAINT INFORMATION", constraintBatches);
            AppendSection(sb, "-- EXTENDED PROPERTY INFORMATION", extendedPropertyBatches);
            AppendSection(sb, "-- INDEX INFORMATION", indexBatches);
            AppendSection(sb, "-- OTHER INFORMATION", otherBatches);

            return sb.ToString().TrimEnd('\r', '\n');
        }

        #region Section / Batch
        private static void AppendSection(StringBuilder sb, string heading, IList<string> batches)
        {
            if (batches == null || batches.Count <= 0)
            {
                return;
            }

            if (sb.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(heading))
            {
                sb.AppendLine(heading);
                sb.AppendLine();
            }

            sb.Append(SqlServerCreateScriptFormatterHelper.JoinGoBatches(batches));
        }

        private static TableBatchKind DetectBatchKind(string batch)
        {
            var firstLine = GetFirstExecutableLine(batch);

            if (string.IsNullOrWhiteSpace(firstLine))
            {
                return TableBatchKind.Other;
            }

            if (firstLine.StartsWith("CREATE TABLE ", StringComparison.OrdinalIgnoreCase))
            {
                return TableBatchKind.Table;
            }

            if ((firstLine.StartsWith("EXEC ", StringComparison.OrdinalIgnoreCase)
                 || firstLine.StartsWith("EXECUTE ", StringComparison.OrdinalIgnoreCase))
                 && batch.IndexOf("SP_ADDEXTENDEDPROPERTY", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return TableBatchKind.ExtendedProperty;
            }

            if (firstLine.StartsWith("CREATE NONCLUSTERED INDEX ", StringComparison.OrdinalIgnoreCase)
                || firstLine.StartsWith("CREATE CLUSTERED INDEX ", StringComparison.OrdinalIgnoreCase)
                || firstLine.StartsWith("CREATE UNIQUE ", StringComparison.OrdinalIgnoreCase))
            {
                return TableBatchKind.Index;
            }

            if (firstLine.StartsWith("ALTER TABLE ", StringComparison.OrdinalIgnoreCase))
            {
                return TableBatchKind.Constraint;
            }

            return TableBatchKind.Other;
        }

        private static string GetFirstExecutableLine(string batch)
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                return string.Empty;
            }

            var lines = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(batch)
                                                            .Split(new[] { "\r\n" }, StringSplitOptions.None);

            for (var i = 0; i < lines.Length; i++)
            {
                var line = (lines[i] ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("--", StringComparison.Ordinal))
                {
                    continue;
                }

                return line;
            }

            return string.Empty;
        }

        private static string FormatConstraintBatch(string batch)
        {
            return SqlServerConstraintBeautifierHelper.BeautifyBatch(batch);
        }

        private static string FormatExtendedPropertyBatch(string batch)
        {
            return SqlServerExtendedPropertyBeautifierHelper.BeautifyBatch(batch);
        }

        private static string FormatIndexBatch(string batch)
        {
            return SqlServerCreateScriptFormatterHelper.NormalizeBatchText(batch);
        }
        #endregion

        #region Create Table
        private static string FormatCreateTableBatch(string batch)
        {
            batch = SqlServerCreateScriptFormatterHelper.NormalizeBatchText(batch);

            if (!TryExtractCreateTableStatement(batch, out var createHeader, out var listElements, out var statementTail))
            {
                return batch;
            }

            var widths = GetColumnFormatWidths(listElements);
            var sb = new StringBuilder();

            sb.AppendLine(createHeader);
            sb.AppendLine("(");

            for (var i = 0; i < listElements.Count; i++)
            {
                var element = listElements[i];
                var hasNext = i < listElements.Count - 1;
                var block = element.IsColumn ? BuildFormattedColumnBlock(element, widths) : BuildFormattedInlineConstraintBlock(element.OriginalText);

                if (hasNext)
                {
                    block = AddSuffixToLastLine(block, ",");
                }

                sb.AppendLine(block);
            }

            if (string.IsNullOrWhiteSpace(statementTail))
            {
                sb.Append(")");
            }
            else
            {
                sb.Append($") {statementTail.Trim()}");
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static bool TryExtractCreateTableStatement(string batch, out string createHeader, out List<TableElement> listElements, out string statementTail)
        {
            createHeader = string.Empty;
            listElements = new List<TableElement>();
            statementTail = string.Empty;

            if (string.IsNullOrWhiteSpace(batch))
            {
                return false;
            }

            var openParenIndex = FindFirstTopLevelChar(batch, '(');

            if (openParenIndex < 0)
            {
                return false;
            }

            var closeParenIndex = FindMatchingParen(batch, openParenIndex);

            if (closeParenIndex < 0)
            {
                return false;
            }

            createHeader = batch.Substring(0, openParenIndex).TrimEnd();

            var bodyText = batch.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);

            statementTail = batch.Substring(closeParenIndex + 1).Trim();

            var items = SplitTopLevelCommaItems(bodyText);

            foreach (var item in items)
            {
                var text = (item ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (TryParseColumnDefinition(text, out var element))
                {
                    listElements.Add(element);
                }
                else
                {
                    listElements.Add(new TableElement
                    {
                        IsColumn = false,
                        OriginalText = text
                    });
                }
            }

            return !string.IsNullOrWhiteSpace(createHeader) && listElements.Count > 0;
        }

        private static bool TryParseColumnDefinition(string text, out TableElement element)
        {
            element = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            for (var i = 0; i < NonColumnPrefixes.Length; i++)
            {
                if (text.StartsWith(NonColumnPrefixes[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            var columnName = ExtractColumnNameToken(text);

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            var remain = text.Substring(columnName.Length).Trim();

            if (string.IsNullOrWhiteSpace(remain))
            {
                return false;
            }

            SplitDataTypeAndOther(remain, out var dataType, out var other);

            if (string.IsNullOrWhiteSpace(dataType))
            {
                return false;
            }

            SplitColumnTail(other, out var preDefaultText, out var defaultClause, out var nullabilityClause);

            element = new TableElement
            {
                IsColumn = true,
                OriginalText = text,
                ColumnName = columnName.Trim(),
                DataType = dataType.Trim(),
                Other = other.Trim(),
                PreDefaultText = preDefaultText,
                DefaultClause = defaultClause,
                NullabilityClause = nullabilityClause
            };

            return true;
        }

        private static string ExtractColumnNameToken(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            if (text[0] == '[')
            {
                for (var i = 1; i < text.Length; i++)
                {
                    if (text[i] != ']')
                    {
                        continue;
                    }

                    if (i + 1 < text.Length && text[i + 1] == ']')
                    {
                        i++;
                        continue;
                    }

                    return text.Substring(0, i + 1);
                }

                return string.Empty;
            }

            if (text[0] == '"')
            {
                for (var i = 1; i < text.Length; i++)
                {
                    if (text[i] != '"')
                    {
                        continue;
                    }

                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    return text.Substring(0, i + 1);
                }

                return string.Empty;
            }

            var pos = text.IndexOf(' ');

            return pos < 0 ? text : text.Substring(0, pos);
        }

        private static void SplitDataTypeAndOther(string remain, out string dataType, out string other)
        {
            dataType = string.Empty;
            other = string.Empty;

            if (string.IsNullOrWhiteSpace(remain))
            {
                return;
            }

            var nullableSuffix = string.Empty;
            var textBeforeNullable = remain.Trim();

            if (textBeforeNullable.EndsWith(" NOT NULL", StringComparison.OrdinalIgnoreCase))
            {
                nullableSuffix = textBeforeNullable.Substring(textBeforeNullable.Length - 9);
                textBeforeNullable = textBeforeNullable.Substring(0, textBeforeNullable.Length - 9).TrimEnd();
            }
            else if (textBeforeNullable.EndsWith(" NULL", StringComparison.OrdinalIgnoreCase))
            {
                nullableSuffix = textBeforeNullable.Substring(textBeforeNullable.Length - 5);
                textBeforeNullable = textBeforeNullable.Substring(0, textBeforeNullable.Length - 5).TrimEnd();
            }

            var defaultIndex = IndexOfTopLevelKeyword(textBeforeNullable, " DEFAULT ");

            if (defaultIndex >= 0)
            {
                dataType = textBeforeNullable.Substring(0, defaultIndex).TrimEnd();

                var defaultClause = textBeforeNullable.Substring(defaultIndex).Trim();

                other = string.IsNullOrWhiteSpace(nullableSuffix) ? defaultClause : $"{defaultClause}{nullableSuffix}";
            }
            else
            {
                dataType = textBeforeNullable.TrimEnd();
                other = nullableSuffix.Trim();
            }
        }

        private static void SplitColumnTail(string other, out string preDefaultText, out string defaultClause, out string nullabilityClause)
        {
            preDefaultText = string.Empty;
            defaultClause = string.Empty;
            nullabilityClause = string.Empty;

            if (string.IsNullOrWhiteSpace(other))
            {
                return;
            }

            var text = other.Trim();

            if (text.EndsWith(" NOT NULL", StringComparison.OrdinalIgnoreCase))
            {
                nullabilityClause = "NOT NULL";
                text = text.Substring(0, text.Length - 9).TrimEnd();
            }
            else if (text.EndsWith(" NULL", StringComparison.OrdinalIgnoreCase))
            {
                nullabilityClause = "NULL";
                text = text.Substring(0, text.Length - 5).TrimEnd();
            }

            var defaultIndex = FindTopLevelWordIndex(text, "DEFAULT");

            if (defaultIndex >= 0)
            {
                preDefaultText = text.Substring(0, defaultIndex).TrimEnd();
                defaultClause = text.Substring(defaultIndex).Trim();
            }
            else
            {
                preDefaultText = text.Trim();
            }
        }

        private static ColumnFormatWidths GetColumnFormatWidths(List<TableElement> listElements)
        {
            var columns = (listElements ?? new List<TableElement>()).Where(x => x != null && x.IsColumn).ToList();

            return new ColumnFormatWidths
            {
                ColumnNameWidth = columns.Select(x => x.ColumnName?.Length ?? 0).DefaultIfEmpty(0).Max(),
                DataTypeWidth = columns.Select(x => x.DataType?.Length ?? 0).DefaultIfEmpty(0).Max(),
                PreDefaultWidth = columns.Select(x => x.PreDefaultText?.Length ?? 0).DefaultIfEmpty(0).Max(),
                DefaultClauseWidth = columns.Select(x => x.DefaultClause?.Length ?? 0).DefaultIfEmpty(0).Max()
            };
        }

        private static string BuildFormattedColumnBlock(TableElement element, ColumnFormatWidths widths)
        {
            var sb = new StringBuilder();
            var columnName = element?.ColumnName ?? string.Empty;
            var dataType = element?.DataType ?? string.Empty;
            var preDefaultText = element?.PreDefaultText ?? string.Empty;
            var defaultClause = element?.DefaultClause ?? string.Empty;
            var nullabilityClause = element?.NullabilityClause ?? string.Empty;

            sb.Append("    ");
            sb.Append(columnName);
            sb.Append(new string(' ', Math.Max(0, widths.ColumnNameWidth - columnName.Length)));
            sb.Append("  ");
            sb.Append(dataType);
            sb.Append(new string(' ', Math.Max(0, widths.DataTypeWidth - dataType.Length)));

            if (widths.PreDefaultWidth > 0 || !string.IsNullOrWhiteSpace(preDefaultText))
            {
                sb.Append("  ");
                sb.Append(preDefaultText);
                sb.Append(new string(' ', Math.Max(0, widths.PreDefaultWidth - preDefaultText.Length)));
            }

            if (widths.DefaultClauseWidth > 0 || !string.IsNullOrWhiteSpace(defaultClause))
            {
                sb.Append("  ");
                sb.Append(defaultClause);
                sb.Append(new string(' ', Math.Max(0, widths.DefaultClauseWidth - defaultClause.Length)));
            }

            if (!string.IsNullOrWhiteSpace(nullabilityClause))
            {
                sb.Append("  ");
                sb.Append(nullabilityClause);
            }

            return sb.ToString().TrimEnd(' ');
        }
        #endregion

        #region Inline Constraint Beautifier
        private static string BuildFormattedInlineConstraintBlock(string text)
        {
            text = SqlServerCreateScriptFormatterHelper.NormalizeBatchText(text);

            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            if (TryFormatInlineKeyConstraint(text, out var keyResult))
            {
                return keyResult;
            }

            if (TryFormatInlineCheckConstraint(text, out var checkResult))
            {
                return checkResult;
            }

            return $"    {NormalizeSimpleLines(text)}";
        }

        private static bool TryFormatInlineKeyConstraint(string text, out string result)
        {
            result = string.Empty;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (text.IndexOf("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            var openParenIndex = FindFirstTopLevelChar(text, '(');

            if (openParenIndex < 0)
            {
                return false;
            }

            var closeParenIndex = FindMatchingParen(text, openParenIndex);

            if (closeParenIndex < 0)
            {
                return false;
            }

            var prefix = text.Substring(0, openParenIndex).TrimEnd();
            var columns = text.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            var rest = text.Substring(closeParenIndex + 1).Trim();
            var sb = new StringBuilder();

            AppendConstraintPrefixLines(sb, prefix, 4);
            sb.AppendLine();
            sb.AppendLine("    (");
            AppendCommaList(sb, columns, 8);
            sb.AppendLine();
            sb.Append("    )");

            if (!string.IsNullOrWhiteSpace(rest))
            {
                SplitWithAndTail(rest, out var withClause, out var tailClause);

                if (!string.IsNullOrWhiteSpace(withClause))
                {
                    sb.AppendLine();
                    sb.Append(IndentText(SqlServerClauseStyleFormatterHelper.FormatWithClause(withClause), 4));
                }

                if (!string.IsNullOrWhiteSpace(tailClause))
                {
                    sb.AppendLine();
                    sb.Append(IndentText(SqlServerClauseStyleFormatterHelper.FormatIndexClauseSegment(tailClause), 4));
                }
            }

            result = sb.ToString().TrimEnd('\r', '\n');
            return true;
        }

        private static bool TryFormatInlineCheckConstraint(string text, out string result)
        {
            result = string.Empty;

            if (string.IsNullOrWhiteSpace(text) || text.IndexOf("CHECK", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            var openParenIndex = FindFirstTopLevelChar(text, '(');

            if (openParenIndex < 0)
            {
                return false;
            }

            var closeParenIndex = FindMatchingParen(text, openParenIndex);

            if (closeParenIndex < 0)
            {
                return false;
            }

            var prefix = text.Substring(0, openParenIndex).TrimEnd();
            var expr = text.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            var rest = text.Substring(closeParenIndex + 1).Trim();
            var sb = new StringBuilder();

            AppendConstraintPrefixLines(sb, prefix, 4);
            sb.AppendLine();
            sb.AppendLine("    (");
            AppendLogicalConditionList(sb, expr, 8);
            sb.AppendLine();
            sb.Append("    )");

            if (!string.IsNullOrWhiteSpace(rest))
            {
                sb.AppendLine();
                sb.Append(IndentText(NormalizeSimpleLines(rest), 4));
            }

            result = sb.ToString().TrimEnd('\r', '\n');
            return true;
        }

        private static void AppendConstraintPrefixLines(StringBuilder sb, string prefix, int indentSpaces)
        {
            prefix = NormalizeSimpleLines(prefix);

            if (string.IsNullOrWhiteSpace(prefix))
            {
                return;
            }

            var match = Regex.Match
            (
                prefix,
                @"^(CONSTRAINT\s+(?:\[[^\]]+\]|""[^""]+""|\S+))\s+(.+)$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            var indent = new string(' ', indentSpaces);

            if (match.Success)
            {
                sb.Append(indent).Append(match.Groups[1].Value.Trim());
                sb.AppendLine();
                sb.Append(indent).Append(match.Groups[2].Value.Trim());
                return;
            }

            sb.Append(indent).Append(prefix.Trim());
        }

        private static void SplitWithAndTail(string rest, out string withClause, out string tailClause)
        {
            withClause = string.Empty;
            tailClause = string.Empty;

            if (string.IsNullOrWhiteSpace(rest))
            {
                return;
            }

            var trimmed = rest.Trim();

            if (!trimmed.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
            {
                tailClause = trimmed;
                return;
            }

            var afterWith = trimmed.Substring(4).TrimStart();
            var openParenIndex = FindFirstTopLevelChar(afterWith, '(');

            if (openParenIndex < 0)
            {
                tailClause = trimmed;
                return;
            }

            var closeParenIndex = FindMatchingParen(afterWith, openParenIndex);

            if (closeParenIndex < 0)
            {
                tailClause = trimmed;
                return;
            }

            withClause = afterWith.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            tailClause = afterWith.Substring(closeParenIndex + 1).Trim();
        }

        private static void AppendCommaList(StringBuilder sb, string text, int indentSpaces)
        {
            var items = SplitTopLevelCommaItems(text);

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                }

                var item = items[i].Trim();
                var suffix = i == items.Count - 1 ? string.Empty : ",";

                sb.Append(new string(' ', indentSpaces));
                sb.Append(item);
                sb.Append(suffix);
            }
        }

        private static void AppendLogicalConditionList(StringBuilder sb, string expr, int indentSpaces)
        {
            var parts = SplitTopLevelLogicalConditions(expr);

            if (parts.Count <= 1)
            {
                sb.Append(new string(' ', indentSpaces));
                sb.Append(NormalizeSimpleLines(expr));
                return;
            }

            for (var i = 0; i < parts.Count; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(new string(' ', indentSpaces));
                sb.Append(parts[i].Trim());
            }
        }

        private static string AddSuffixToLastLine(string text, string suffix)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(suffix))
            {
                return text;
            }

            return $"{text}{suffix}";
        }

        private static string IndentText(string text, int indentSpaces)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var indent = new string(' ', indentSpaces);
            var lines = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(text).Split(new[] { "\r\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (i > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(indent).Append(line);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }
        #endregion

        #region Text Parser
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
                    return i;
                }
            }

            return -1;
        }

        private static int FindTopLevelWordIndex(string text, string word)
        {
            var tokenInfos = TokenizeTopLevelWords(text);

            for (var i = 0; i < tokenInfos.Count; i++)
            {
                if (string.Equals(tokenInfos[i].Text, word, StringComparison.OrdinalIgnoreCase))
                {
                    return tokenInfos[i].StartIndex;
                }
            }

            return -1;
        }

        private static int FindFirstTopLevelChar(string text, char target)
        {
            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;

            for (var i = 0; i < (text ?? string.Empty).Length; i++)
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
            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;

            for (var i = openParenIndex; i < (text ?? string.Empty).Length; i++)
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

        private static List<string> SplitTopLevelLogicalConditions(string text)
        {
            var parts = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return parts;
            }

            var tokenInfos = TokenizeTopLevelWords(text);

            if (tokenInfos.Count <= 0)
            {
                parts.Add(text.Trim());
                return parts;
            }

            var splitIndexes = new List<int>();
            var caseDepth = 0;
            var waitingBetweenAnd = false;

            for (var i = 0; i < tokenInfos.Count; i++)
            {
                var token = tokenInfos[i].Text;

                if (string.Equals(token, "CASE", StringComparison.OrdinalIgnoreCase))
                {
                    caseDepth++;
                    continue;
                }

                if (string.Equals(token, "END", StringComparison.OrdinalIgnoreCase) && caseDepth > 0)
                {
                    caseDepth--;
                    continue;
                }

                if (caseDepth > 0)
                {
                    continue;
                }

                if (string.Equals(token, "BETWEEN", StringComparison.OrdinalIgnoreCase))
                {
                    waitingBetweenAnd = true;
                    continue;
                }

                if (waitingBetweenAnd && string.Equals(token, "AND", StringComparison.OrdinalIgnoreCase))
                {
                    waitingBetweenAnd = false;
                    continue;
                }

                if (string.Equals(token, "AND", StringComparison.OrdinalIgnoreCase) || string.Equals(token, "OR", StringComparison.OrdinalIgnoreCase))
                {
                    splitIndexes.Add(tokenInfos[i].StartIndex);
                }
            }

            if (splitIndexes.Count <= 0)
            {
                parts.Add(text.Trim());
                return parts;
            }

            var firstPart = text.Substring(0, splitIndexes[0]).Trim();

            if (!string.IsNullOrWhiteSpace(firstPart))
            {
                parts.Add(firstPart);
            }

            for (var i = 0; i < splitIndexes.Count; i++)
            {
                var start = splitIndexes[i];
                var end = i == splitIndexes.Count - 1 ? text.Length : splitIndexes[i + 1];
                var segment = text.Substring(start, end - start).Trim();

                if (!string.IsNullOrWhiteSpace(segment))
                {
                    parts.Add(segment);
                }
            }

            return parts;
        }

        private static List<TokenInfo> TokenizeTopLevelWords(string text)
        {
            var list = new List<TokenInfo>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return list;
            }

            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;
            var tokenStart = -1;

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
                    depth++;
                    continue;
                }

                if (ch == ')')
                {
                    depth--;
                    continue;
                }

                if (depth > 0)
                {
                    continue;
                }

                if (!char.IsWhiteSpace(ch))
                {
                    if (tokenStart < 0)
                    {
                        tokenStart = i;
                    }
                }
                else
                {
                    if (tokenStart >= 0)
                    {
                        list.Add(new TokenInfo
                        {
                            Text = text.Substring(tokenStart, i - tokenStart),
                            StartIndex = tokenStart
                        });

                        tokenStart = -1;
                    }
                }
            }

            if (tokenStart >= 0)
            {
                list.Add(new TokenInfo
                {
                    Text = text.Substring(tokenStart),
                    StartIndex = tokenStart
                });
            }

            return list;
        }

        private static string NormalizeSimpleLines(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var lines = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(text)
                                                            .Split(new[] { "\r\n" }, StringSplitOptions.None)
                                                            .Select(x => x.Trim())
                                                            .Where(x => !string.IsNullOrWhiteSpace(x))
                                                            .ToList();

            return string.Join("\r\n", lines);
        }
        #endregion
    }
}
