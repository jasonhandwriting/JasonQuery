using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal static class MySqlCreateScriptFormatterHelper
    {
        internal sealed class TokenInfo
        {
            public string Text { get; set; }
            public int StartIndex { get; set; }
        }

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

        public static string NormalizeSimpleLines(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var lines = NormalizeLineEndings(text).Split(new[] { "\r\n" }, StringSplitOptions.None)
                                                  .Select(x => x.Trim())
                                                  .Where(x => !string.IsNullOrWhiteSpace(x))
                                                  .ToList();

            return string.Join("\r\n", lines);
        }

        public static string IndentText(string text, int indentSpaces)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var indent = new string(' ', indentSpaces);
            var lines = NormalizeLineEndings(text).Split(new[] { "\r\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(indent).Append(lines[i]);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        public static int FindFirstTopLevelChar(string text, char target)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return -1;
            }

            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBacktick = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (!inDoubleQuote && !inBacktick && ch == '\'')
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

                if (!inSingleQuote && !inBacktick && ch == '"')
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote && ch == '`')
                {
                    inBacktick = !inBacktick;
                    continue;
                }

                if (inSingleQuote || inDoubleQuote || inBacktick)
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

        public static int FindMatchingParen(string text, int openParenIndex)
        {
            if (string.IsNullOrWhiteSpace(text) || openParenIndex < 0 || openParenIndex >= text.Length)
            {
                return -1;
            }

            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBacktick = false;

            for (var i = openParenIndex; i < text.Length; i++)
            {
                var ch = text[i];

                if (!inDoubleQuote && !inBacktick && ch == '\'')
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

                if (!inSingleQuote && !inBacktick && ch == '"')
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote && ch == '`')
                {
                    inBacktick = !inBacktick;
                    continue;
                }

                if (inSingleQuote || inDoubleQuote || inBacktick)
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

        public static List<string> SplitTopLevelCommaItems(string text)
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
            var inBacktick = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (!inDoubleQuote && !inBacktick && ch == '\'')
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

                if (!inSingleQuote && !inBacktick && ch == '"')
                {
                    sb.Append(ch);
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote && ch == '`')
                {
                    sb.Append(ch);
                    inBacktick = !inBacktick;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote && !inBacktick)
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

        public static List<TokenInfo> TokenizeTopLevelWords(string text)
        {
            var list = new List<TokenInfo>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return list;
            }

            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBacktick = false;
            var tokenStart = -1;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (!inDoubleQuote && !inBacktick && ch == '\'')
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

                if (!inSingleQuote && !inBacktick && ch == '"')
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote && ch == '`')
                {
                    inBacktick = !inBacktick;
                    continue;
                }

                if (inSingleQuote || inDoubleQuote || inBacktick)
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
