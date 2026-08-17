using JasonQuery.Core.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.CreateScript.PostgreSql
{
    internal static class PostgreSqlCreateTableScriptBeautifier
    {
        private sealed class TableElement
        {
            public bool IsColumn { get; set; }
            public string OriginalText { get; set; }
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public string Other { get; set; }
        }

        private sealed class TokenInfo
        {
            public string Text { get; set; }
            public int StartIndex { get; set; }
        }

        private static readonly HashSet<string> BoundaryKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "COLLATE",
            "DEFAULT",
            "CONSTRAINT",
            "NOT",
            "NULL",
            "CHECK",
            "PRIMARY",
            "UNIQUE",
            "REFERENCES",
            "GENERATED",
            "COMPRESSION"
        };

        private static readonly string[] NonColumnPrefixes =
        {
            "CONSTRAINT ",
            "PRIMARY KEY",
            "UNIQUE ",
            "FOREIGN KEY",
            "CHECK ",
            "EXCLUDE ",
            "LIKE "
        };

        public static string Beautify(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                return string.Empty;
            }

            var normalizedScript = NormalizeScript(script);

            if (!TryExtractCreateTableStatement(normalizedScript, out var prefix, out var createStatement, out var suffix))
            {
                return normalizedScript;
            }

            if (!TryParseCreateTableStatement(createStatement, out var createHeader, out var listElements, out var statementTail))
            {
                return normalizedScript;
            }

            var formattedCreateStatement = RebuildCreateTableStatement(createHeader, listElements, statementTail);

            return CombineScriptParts(prefix, formattedCreateStatement, suffix);
        }

        #region Normalize / Split Statement
        private static string NormalizeScript(string script)
        {
            return script.Replace("\r\n", "\n")
                         .Replace("\r", "\n")
                         .Replace("\n", "\r\n")
                         .TrimEnd('\r', '\n');
        }

        private static bool TryExtractCreateTableStatement(string script, out string prefix, out string createStatement, out string suffix)
        {
            prefix = string.Empty;
            createStatement = string.Empty;
            suffix = string.Empty;

            if (string.IsNullOrWhiteSpace(script))
            {
                return false;
            }

            var match = Regex.Match
            (
                script,
                @"^\s*CREATE\s+(UNLOGGED\s+|TEMPORARY\s+|TEMP\s+)?TABLE\b",
                RegexOptions.IgnoreCase | RegexOptions.Multiline
            );

            if (!match.Success)
            {
                return false;
            }

            var createStart = match.Index;
            int openParen = FindFirstOpenParenthesis(script, createStart);

            if (openParen < 0)
            {
                return false;
            }

            int closeParen = FindMatchingClosingParenthesis(script, openParen);

            if (closeParen < 0)
            {
                return false;
            }

            int semicolon = FindStatementTerminator(script, closeParen + 1);

            if (semicolon < 0)
            {
                semicolon = script.Length - 1;
            }

            prefix = script.Substring(0, createStart).TrimEnd('\r', '\n');
            createStatement = script.Substring(createStart, semicolon - createStart + 1).Trim();
            suffix = semicolon + 1 < script.Length ? script.Substring(semicolon + 1).TrimStart('\r', '\n') : string.Empty;

            return true;
        }

        private static int FindFirstOpenParenthesis(string text, int startIndex)
        {
            var isInSingleQuote = false;
            var isInDoubleQuote = false;

            for (var i = startIndex; i < text.Length; i++)
            {
                var c = text[i];

                if (!isInDoubleQuote && c == '\'')
                {
                    if (isInSingleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\'')
                        {
                            i++;
                            continue;
                        }

                        isInSingleQuote = false;
                    }
                    else
                    {
                        isInSingleQuote = true;
                    }

                    continue;
                }

                if (!isInSingleQuote && c == '"')
                {
                    if (isInDoubleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            i++;
                            continue;
                        }

                        isInDoubleQuote = false;
                    }
                    else
                    {
                        isInDoubleQuote = true;
                    }

                    continue;
                }

                if (!isInSingleQuote && !isInDoubleQuote && c == '(')
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindMatchingClosingParenthesis(string text, int openParen)
        {
            var depth = 0;
            var isInSingleQuote = false;
            var isInDoubleQuote = false;

            for (var i = openParen; i < text.Length; i++)
            {
                var c = text[i];

                if (!isInDoubleQuote && c == '\'')
                {
                    if (isInSingleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\'')
                        {
                            i++;
                            continue;
                        }

                        isInSingleQuote = false;
                    }
                    else
                    {
                        isInSingleQuote = true;
                    }

                    continue;
                }

                if (!isInSingleQuote && c == '"')
                {
                    if (isInDoubleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            i++;
                            continue;
                        }

                        isInDoubleQuote = false;
                    }
                    else
                    {
                        isInDoubleQuote = true;
                    }

                    continue;
                }

                if (isInSingleQuote || isInDoubleQuote)
                {
                    continue;
                }

                if (c == '(')
                {
                    depth++;
                }
                else if (c == ')')
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

        private static int FindStatementTerminator(string text, int startIndex)
        {
            var isInSingleQuote = false;
            var isInDoubleQuote = false;

            for (var i = startIndex; i < text.Length; i++)
            {
                var c = text[i];

                if (!isInDoubleQuote && c == '\'')
                {
                    if (isInSingleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\'')
                        {
                            i++;
                            continue;
                        }

                        isInSingleQuote = false;
                    }
                    else
                    {
                        isInSingleQuote = true;
                    }

                    continue;
                }

                if (!isInSingleQuote && c == '"')
                {
                    if (isInDoubleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            i++;
                            continue;
                        }

                        isInDoubleQuote = false;
                    }
                    else
                    {
                        isInDoubleQuote = true;
                    }

                    continue;
                }

                if (!isInSingleQuote && !isInDoubleQuote && c == ';')
                {
                    return i;
                }
            }

            return -1;
        }
        #endregion

        #region Parse Create Table
        private static bool TryParseCreateTableStatement(string createStatement, out string createHeader, out List<TableElement> listElements, out string statementTail)
        {
            createHeader = string.Empty;
            listElements = new List<TableElement>();
            statementTail = string.Empty;

            if (string.IsNullOrWhiteSpace(createStatement))
            {
                return false;
            }

            int openParen = FindFirstOpenParenthesis(createStatement, 0);

            if (openParen < 0)
            {
                return false;
            }

            int closeParen = FindMatchingClosingParenthesis(createStatement, openParen);

            if (closeParen < 0)
            {
                return false;
            }

            createHeader = createStatement.Substring(0, openParen).TrimEnd();
            
            var body = createStatement.Substring(openParen + 1, closeParen - openParen - 1);

            statementTail = createStatement.Substring(closeParen + 1).Trim();

            if (statementTail.EndsWith(";", StringComparison.Ordinal))
            {
                statementTail = statementTail.Substring(0, statementTail.Length - 1).TrimEnd();
            }

            var listBodyItems = SplitTopLevelItems(body);

            foreach (var item in listBodyItems)
            {
                var text = item.Trim();

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

            return true;
        }

        private static List<string> SplitTopLevelItems(string body)
        {
            var listItems = new List<string>();

            if (string.IsNullOrWhiteSpace(body))
            {
                return listItems;
            }

            var sb = new StringBuilder();
            var depth = 0;
            var isInSingleQuote = false;
            var isInDoubleQuote = false;

            for (var i = 0; i < body.Length; i++)
            {
                var c = body[i];

                if (!isInDoubleQuote && c == '\'')
                {
                    sb.Append(c);

                    if (isInSingleQuote)
                    {
                        if (i + 1 < body.Length && body[i + 1] == '\'')
                        {
                            sb.Append(body[i + 1]);
                            i++;
                            continue;
                        }

                        isInSingleQuote = false;
                    }
                    else
                    {
                        isInSingleQuote = true;
                    }

                    continue;
                }

                if (!isInSingleQuote && c == '"')
                {
                    sb.Append(c);

                    if (isInDoubleQuote)
                    {
                        if (i + 1 < body.Length && body[i + 1] == '"')
                        {
                            sb.Append(body[i + 1]);
                            i++;
                            continue;
                        }

                        isInDoubleQuote = false;
                    }
                    else
                    {
                        isInDoubleQuote = true;
                    }

                    continue;
                }

                if (!isInSingleQuote && !isInDoubleQuote)
                {
                    if (c == '(')
                    {
                        depth++;
                    }
                    else if (c == ')')
                    {
                        depth--;
                    }
                    else if (c == ',' && depth == 0)
                    {
                        listItems.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }

                sb.Append(c);
            }

            if (sb.Length > 0)
            {
                listItems.Add(sb.ToString());
            }

            return listItems;
        }

        private static bool TryParseColumnDefinition(string text, out TableElement element)
        {
            element = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (NonColumnPrefixes.Any(prefix => text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
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

            element = new TableElement
            {
                IsColumn = true,
                OriginalText = text,
                ColumnName = columnName.Trim(),
                DataType = dataType.Trim(),
                Other = other.Trim()
            };

            return true;
        }

        private static string ExtractColumnNameToken(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            if (text[0] != '"')
            {
                var pos = text.IndexOf(' ');

                return pos < 0 ? text : text.Substring(0, pos);
            }

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

        private static void SplitDataTypeAndOther(string remain, out string dataType, out string other)
        {
            dataType = string.Empty;
            other = string.Empty;

            var listTokens = TokenizeTopLevelWords(remain);

            if (listTokens.Count <= 0)
            {
                dataType = remain.Trim();
                return;
            }

            var boundary = -1;

            for (var i = 0; i < listTokens.Count; i++)
            {
                if (BoundaryKeywords.Contains(listTokens[i].Text))
                {
                    boundary = listTokens[i].StartIndex;
                    break;
                }
            }

            if (boundary < 0)
            {
                dataType = remain.Trim();
                return;
            }

            dataType = remain.Substring(0, boundary).Trim();
            other = remain.Substring(boundary).Trim();
        }

        private static List<TokenInfo> TokenizeTopLevelWords(string text)
        {
            var listTokens = new List<TokenInfo>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return listTokens;
            }

            var depth = 0;
            var isInSingleQuote = false;
            var isInDoubleQuote = false;
            var tokenStart = -1;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (!isInDoubleQuote && c == '\'')
                {
                    if (isInSingleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\'')
                        {
                            i++;
                            continue;
                        }

                        isInSingleQuote = false;
                    }
                    else
                    {
                        isInSingleQuote = true;
                    }

                    continue;
                }

                if (!isInSingleQuote && c == '"')
                {
                    if (isInDoubleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            i++;
                            continue;
                        }

                        isInDoubleQuote = false;
                    }
                    else
                    {
                        isInDoubleQuote = true;
                    }

                    continue;
                }

                if (isInSingleQuote || isInDoubleQuote)
                {
                    continue;
                }

                if (c == '(')
                {
                    depth++;
                    continue;
                }

                if (c == ')')
                {
                    depth--;
                    continue;
                }

                if (depth > 0)
                {
                    continue;
                }

                if (!char.IsWhiteSpace(c))
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
                        listTokens.Add(new TokenInfo
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
                listTokens.Add(new TokenInfo
                {
                    Text = text.Substring(tokenStart),
                    StartIndex = tokenStart
                });
            }

            return listTokens;
        }
        #endregion

        #region Build Result
        private static string BuildColumnNameAndTypeText(IReadOnlyList<TableElement> listElements)
        {
            var sbResult = new StringBuilder();

            foreach (var element in listElements.Where(x => x.IsColumn))
            {
                sbResult.Append(element.ColumnName);
                sbResult.Append(MyGlobal.Separator3s);
                sbResult.Append(element.DataType.Trim(' ', '"'));
                sbResult.Append(MyGlobal.Separator5);
            }

            return sbResult.ToString();
        }

        private static string RebuildCreateTableStatement(string createHeader, IReadOnlyList<TableElement> listElements, string statementTail)
        {
            var maxColumnName = listElements.Where(x => x.IsColumn)
                                            .Select(x => x.ColumnName.Length)
                                            .DefaultIfEmpty(0)
                                            .Max();

            var maxDataType = listElements.Where(x => x.IsColumn)
                                          .Select(x => x.DataType.Length)
                                          .DefaultIfEmpty(0)
                                          .Max();

            var sb = new StringBuilder();

            sb.AppendLine(createHeader);
            sb.AppendLine("(");

            for (var i = 0; i < listElements.Count; i++)
            {
                var element = listElements[i];
                var bHasNext = i < listElements.Count - 1;
                var line = element.IsColumn ? BuildFormattedColumnLine(element, maxColumnName, maxDataType) : $"    {element.OriginalText.Trim()}";

                if (bHasNext)
                {
                    line += ",";
                }

                sb.AppendLine(line);
            }

            if (string.IsNullOrWhiteSpace(statementTail))
            {
                sb.Append(");");
            }
            else
            {
                sb.Append($") {statementTail.Trim()};");
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string BuildFormattedColumnLine(TableElement element, int maxColumnName, int maxDataType)
        {
            var paddingName = new string(' ', Math.Max(0, maxColumnName - element.ColumnName.Length));
            var paddingType = new string(' ', Math.Max(0, maxDataType - element.DataType.Length));
            var sb = new StringBuilder();

            sb.Append("    ");
            sb.Append(element.ColumnName);
            sb.Append(paddingName);
            sb.Append("  ");
            sb.Append(element.DataType);
            sb.Append(paddingType);

            if (!string.IsNullOrWhiteSpace(element.Other))
            {
                sb.Append("  ");
                sb.Append(element.Other);
            }

            return sb.ToString().TrimEnd(' ');
        }

        private static string CombineScriptParts(string prefix, string formattedCreateStatement, string suffix)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                sb.AppendLine(prefix.TrimEnd());
                sb.AppendLine();
            }

            sb.Append(formattedCreateStatement.TrimEnd());

            if (!string.IsNullOrWhiteSpace(suffix))
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.Append(suffix.TrimStart());
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }
        #endregion
    }
}