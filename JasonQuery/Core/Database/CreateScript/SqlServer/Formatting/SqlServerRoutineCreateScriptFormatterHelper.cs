using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal static class SqlServerRoutineCreateScriptFormatterHelper
    {
        private sealed class TokenInfo
        {
            public string Text { get; set; }
            public int StartIndex { get; set; }
        }

        public static string FormatFunctionBody(string body)
        {
            return FormatRoutineBody(body, true);
        }

        public static string FormatProcedureBody(string body)
        {
            return FormatRoutineBody(body, false);
        }

        private static string FormatRoutineBody(string body, bool isFunction)
        {
            body = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(body);
            body = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(body);

            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            if (!TrySplitDeclarationAndBody(body, out var declaration, out var routineBody))
            {
                return SqlServerCreateScriptFormatterHelper.JoinGoBatches
                (
                    SqlServerCreateScriptFormatterHelper.SplitGoBatches(body)
                );
            }

            var formattedDeclaration = isFunction ? FormatFunctionDeclaration(declaration) : FormatProcedureDeclaration(declaration);
            var formattedBody = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(routineBody);

            if (string.IsNullOrWhiteSpace(formattedDeclaration))
            {
                return formattedBody;
            }

            if (string.IsNullOrWhiteSpace(formattedBody))
            {
                return formattedDeclaration;
            }

            return $"{formattedDeclaration}\r\n{formattedBody}";
        }

        private static bool TrySplitDeclarationAndBody(string text, out string declaration, out string routineBody)
        {
            declaration = string.Empty;
            routineBody = string.Empty;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var tokenInfos = TokenizeTopLevelWords(text);

            if (tokenInfos.Count <= 0)
            {
                return false;
            }

            for (var i = 0; i < tokenInfos.Count; i++)
            {
                var token = tokenInfos[i];

                if (!string.Equals(token.Text, "AS", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (i > 0 && string.Equals(tokenInfos[i - 1].Text, "EXECUTE", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                declaration = text.Substring(0, token.StartIndex).TrimEnd();
                routineBody = text.Substring(token.StartIndex).TrimStart();

                return !string.IsNullOrWhiteSpace(declaration) && !string.IsNullOrWhiteSpace(routineBody);
            }

            return false;
        }

        private static string FormatFunctionDeclaration(string declaration)
        {
            declaration = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(declaration);

            if (string.IsNullOrWhiteSpace(declaration))
            {
                return string.Empty;
            }

            var openParenIndex = FindFirstTopLevelChar(declaration, '(');

            if (openParenIndex < 0)
            {
                return declaration;
            }

            var closeParenIndex = FindMatchingParen(declaration, openParenIndex);

            if (closeParenIndex < 0)
            {
                return declaration;
            }

            var header = declaration.Substring(0, openParenIndex).TrimEnd();
            var parameterText = declaration.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            var afterParameters = declaration.Substring(closeParenIndex + 1).Trim();

            SplitFunctionTail(afterParameters, out var returnsClause, out var withClause);

            var listParameters = SplitTopLevelCommaItems(parameterText).Select(x => x.Trim())
                                                                       .Where(x => !string.IsNullOrWhiteSpace(x))
                                                                       .ToList();

            var sb = new StringBuilder();

            if (listParameters.Count <= 0)
            {
                sb.Append($"{header}()");
            }
            else if (listParameters.Count == 1)
            {
                sb.Append($"{header}({listParameters[0]})");
            }
            else
            {
                sb.AppendLine(header);
                AppendParenthesizedParameterBlock(sb, parameterText);
            }

            AppendReturnsClause(sb, returnsClause);
            AppendWithClause(sb, withClause);

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string FormatProcedureDeclaration(string declaration)
        {
            declaration = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(declaration);

            if (string.IsNullOrWhiteSpace(declaration))
            {
                return string.Empty;
            }

            SplitProcedureDeclaration(declaration, out var header, out var parameterText, out var withClause);

            var listParameters = SplitTopLevelCommaItems(parameterText).Select(x => x.Trim())
                                                                       .Where(x => !string.IsNullOrWhiteSpace(x))
                                                                       .ToList();

            var sb = new StringBuilder();

            sb.Append(header.TrimEnd());

            if (listParameters.Count == 1)
            {
                sb.Append(" ");
                sb.Append(listParameters[0]);
            }
            else if (listParameters.Count > 1)
            {
                foreach (var parameter in listParameters)
                {
                    sb.AppendLine();
                    sb.Append("    ").Append(parameter);
                }
            }

            AppendWithClause(sb, withClause);

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static void SplitFunctionTail(string afterParameters, out string returnsClause, out string withClause)
        {
            returnsClause = string.Empty;
            withClause = string.Empty;

            if (string.IsNullOrWhiteSpace(afterParameters))
            {
                return;
            }

            var withIndex = FindTopLevelKeywordIndex(afterParameters, "WITH");

            if (withIndex < 0)
            {
                returnsClause = afterParameters.Trim();
                return;
            }

            returnsClause = afterParameters.Substring(0, withIndex).Trim();
            withClause = afterParameters.Substring(withIndex + 4).Trim();
        }

        private static void SplitProcedureDeclaration(string declaration, out string header, out string parameterText, out string withClause)
        {
            header = declaration.Trim();
            parameterText = string.Empty;
            withClause = string.Empty;

            var withIndex = FindTopLevelKeywordIndex(declaration, "WITH");
            var declarationWithoutWith = withIndex >= 0 ? declaration.Substring(0, withIndex).TrimEnd() : declaration.TrimEnd();

            if (withIndex >= 0)
            {
                withClause = declaration.Substring(withIndex + 4).Trim();
            }

            var tokenInfos = TokenizeTopLevelWords(declarationWithoutWith);
            var firstParameterIndex = -1;

            for (var i = 0; i < tokenInfos.Count; i++)
            {
                if (tokenInfos[i].Text.StartsWith("@", StringComparison.Ordinal))
                {
                    firstParameterIndex = tokenInfos[i].StartIndex;
                    break;
                }
            }

            if (firstParameterIndex < 0)
            {
                header = declarationWithoutWith.Trim();
                return;
            }

            header = declarationWithoutWith.Substring(0, firstParameterIndex).TrimEnd();
            parameterText = declarationWithoutWith.Substring(firstParameterIndex).Trim();
        }

        private static void AppendParenthesizedParameterBlock(StringBuilder sb, string parameterText)
        {
            sb.AppendLine("(");

            var listParameters = SplitTopLevelCommaItems(parameterText);

            for (var i = 0; i < listParameters.Count; i++)
            {
                var parameter = listParameters[i].Trim();

                if (string.IsNullOrWhiteSpace(parameter))
                {
                    continue;
                }

                var suffix = i == listParameters.Count - 1 ? string.Empty : ",";

                sb.AppendLine($"    {parameter}{suffix}");
            }

            sb.Append(")");
        }

        private static void AppendReturnsClause(StringBuilder sb, string returnsClause)
        {
            returnsClause = (returnsClause ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(returnsClause))
            {
                return;
            }

            if (LooksLikeTableReturnDefinition(returnsClause) && TrySplitReturnsTableDefinition(returnsClause, out var returnsHeader, out var returnsBody))
            {
                sb.AppendLine();
                sb.AppendLine(returnsHeader);
                sb.AppendLine("(");

                var items = SplitTopLevelCommaItems(returnsBody);

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i].Trim();

                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }

                    var suffix = i == items.Count - 1 ? string.Empty : ",";

                    sb.AppendLine($"    {item}{suffix}");
                }

                sb.Append(")");
                return;
            }

            sb.AppendLine();
            sb.Append(returnsClause);
        }

        private static void AppendWithClause(StringBuilder sb, string withClause)
        {
            withClause = (withClause ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(withClause))
            {
                return;
            }

            var optionItems = SplitTopLevelCommaItems(withClause);

            if (optionItems.Count <= 0)
            {
                sb.AppendLine();
                sb.Append("WITH ").Append(withClause);
                return;
            }

            sb.AppendLine();
            sb.Append("WITH");

            for (var i = 0; i < optionItems.Count; i++)
            {
                var option = optionItems[i].Trim();

                if (string.IsNullOrWhiteSpace(option))
                {
                    continue;
                }

                var suffix = i == optionItems.Count - 1 ? string.Empty : ",";

                sb.AppendLine();
                sb.Append("    ").Append(option).Append(suffix);
            }
        }

        private static bool LooksLikeTableReturnDefinition(string returnsClause)
        {
            if (string.IsNullOrWhiteSpace(returnsClause))
            {
                return false;
            }

            return returnsClause.IndexOf(" TABLE", StringComparison.OrdinalIgnoreCase) >= 0
                   && returnsClause.IndexOf('(') >= 0
                   && returnsClause.EndsWith(")", StringComparison.Ordinal);
        }

        private static bool TrySplitReturnsTableDefinition(string returnsClause, out string returnsHeader, out string returnsBody)
        {
            returnsHeader = string.Empty;
            returnsBody = string.Empty;

            var openParenIndex = FindFirstTopLevelChar(returnsClause, '(');

            if (openParenIndex < 0)
            {
                return false;
            }

            var closeParenIndex = FindMatchingParen(returnsClause, openParenIndex);

            if (closeParenIndex < 0 || closeParenIndex != returnsClause.Length - 1)
            {
                return false;
            }

            returnsHeader = returnsClause.Substring(0, openParenIndex).TrimEnd();
            returnsBody = returnsClause.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);

            return !string.IsNullOrWhiteSpace(returnsHeader);
        }

        private static int FindTopLevelKeywordIndex(string text, string keyword)
        {
            var tokenInfos = TokenizeTopLevelWords(text);

            for (var i = 0; i < tokenInfos.Count; i++)
            {
                if (string.Equals(tokenInfos[i].Text, keyword, StringComparison.OrdinalIgnoreCase))
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
                    depth++;
                }
                else if (ch == ')')
                {
                    depth--;
                }

                if (depth == 1 && ch == target)
                {
                    return i;
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
    }
}