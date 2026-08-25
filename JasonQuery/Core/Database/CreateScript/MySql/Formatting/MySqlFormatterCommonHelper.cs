using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal static class MySqlFormatterCommonHelper
    {
        private sealed class StoredProgramLine
        {
            public string Text { get; set; }
            public bool HasSemicolon { get; set; }
            public bool IsCommentBlock { get; set; }
        }

        private enum StoredProgramLineKind
        {
            Normal,
            Begin,
            End,
            IfThen,
            ElseIfThen,
            Else,
            EndIf,
            WhileDo,
            EndWhile,
            Repeat,
            Until,
            EndRepeat,
            Loop,
            EndLoop
        }

        public static bool MatchesPhrase(List<MySqlCreateScriptFormatterHelper.TokenInfo> tokens, int startIndex, params string[] words)
        {
            if (tokens == null || words == null || startIndex < 0 || startIndex + words.Length > tokens.Count)
            {
                return false;
            }

            for (var i = 0; i < words.Length; i++)
            {
                if (!string.Equals(tokens[startIndex + i].Text, words[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public static int FindWordIndex(List<MySqlCreateScriptFormatterHelper.TokenInfo> tokens, string word)
        {
            if (tokens == null || string.IsNullOrWhiteSpace(word))
            {
                return -1;
            }

            for (var i = 0; i < tokens.Count; i++)
            {
                if (string.Equals(tokens[i].Text, word, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        public static int FindPhraseIndex(List<MySqlCreateScriptFormatterHelper.TokenInfo> tokens, params string[] words)
        {
            if (tokens == null || words == null || words.Length <= 0)
            {
                return -1;
            }

            for (var i = 0; i <= tokens.Count - words.Length; i++)
            {
                if (MatchesPhrase(tokens, i, words))
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool TrySplitViewDeclarationAndQuery(string script, out string declaration, out string query)
        {
            declaration = string.Empty;
            query = string.Empty;

            if (string.IsNullOrWhiteSpace(script))
            {
                return false;
            }

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(script);

            if (tokens.Count <= 0)
            {
                return false;
            }

            var seenView = false;

            for (var i = 0; i < tokens.Count; i++)
            {
                if (string.Equals(tokens[i].Text, "VIEW", StringComparison.OrdinalIgnoreCase))
                {
                    seenView = true;
                    continue;
                }

                if (seenView && string.Equals(tokens[i].Text, "AS", StringComparison.OrdinalIgnoreCase))
                {
                    declaration = script.Substring(0, tokens[i].StartIndex).TrimEnd();
                    query = script.Substring(tokens[i].StartIndex + 2).TrimStart();
                    return true;
                }
            }

            return false;
        }

        public static bool TrySplitDeclarationAndBodyAfterPhrase(string script, out string declaration, out string body, params string[] phraseWords)
        {
            declaration = string.Empty;
            body = string.Empty;

            if (string.IsNullOrWhiteSpace(script) || phraseWords == null || phraseWords.Length <= 0)
            {
                return false;
            }

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(script);
            var phraseIndex = FindPhraseIndex(tokens, phraseWords);

            if (phraseIndex < 0)
            {
                return false;
            }

            var lastToken = tokens[phraseIndex + phraseWords.Length - 1];
            var phraseEnd = lastToken.StartIndex + lastToken.Text.Length;

            declaration = script.Substring(0, phraseEnd).TrimEnd();
            body = script.Substring(phraseEnd).TrimStart();

            return true;
        }

        public static int FindTopLevelPhraseIndex(string text, params string[] words)
        {
            if (string.IsNullOrWhiteSpace(text) || words == null || words.Length <= 0)
            {
                return -1;
            }

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(text);

            if (tokens.Count <= 0)
            {
                return -1;
            }

            for (var i = 0; i <= tokens.Count - words.Length; i++)
            {
                if (!MatchesPhrase(tokens, i, words))
                {
                    continue;
                }

                if (words.Length == 1 && string.Equals(words[0], "JOIN", StringComparison.OrdinalIgnoreCase))
                {
                    if (i > 0)
                    {
                        var prev = tokens[i - 1].Text;

                        if (string.Equals(prev, "INNER", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prev, "LEFT", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prev, "RIGHT", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prev, "CROSS", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prev, "OUTER", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }
                }

                return tokens[i].StartIndex;
            }

            return -1;
        }

        public static List<string> SplitTopLevelQueryClauses(string query, params string[] keywordPhrases)
        {
            var clauses = new List<string>();

            if (string.IsNullOrWhiteSpace(query))
            {
                return clauses;
            }

            var keywordIndexes = new List<int>();

            foreach (var phrase in keywordPhrases ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(phrase))
                {
                    continue;
                }

                AddKeywordIndexes(keywordIndexes, query, phrase);
            }

            keywordIndexes = keywordIndexes.Distinct().OrderBy(x => x).ToList();

            if (keywordIndexes.Count <= 0)
            {
                clauses.Add(query.Trim());
                return clauses;
            }

            for (var i = 0; i < keywordIndexes.Count; i++)
            {
                var start = keywordIndexes[i];
                var end = i == keywordIndexes.Count - 1 ? query.Length : keywordIndexes[i + 1];
                var segment = query.Substring(start, end - start).Trim();

                if (!string.IsNullOrWhiteSpace(segment))
                {
                    clauses.Add(segment);
                }
            }

            if (keywordIndexes[0] > 0)
            {
                var prefix = query.Substring(0, keywordIndexes[0]).Trim();

                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    clauses.Insert(0, prefix);
                }
            }

            return clauses;
        }

        public static List<string> SplitTopLevelLogicalConditions(string text)
        {
            var parts = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return parts;
            }

            var tokenInfos = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(text);

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

        public static void AppendIndentedMultiLine(StringBuilder sb, string text, int indentSpaces, string lastLineSuffix)
        {
            var indent = new string(' ', indentSpaces);
            var lines = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(text)
                                                        .Split(new[] { "\r\n" }, StringSplitOptions.None)
                                                        .Select(x => x.Trim())
                                                        .Where(x => !string.IsNullOrWhiteSpace(x))
                                                        .ToList();

            if (lines.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < lines.Count; i++)
            {
                sb.Append(indent).Append(lines[i]);

                if (i == lines.Count - 1 && !string.IsNullOrWhiteSpace(lastLineSuffix))
                {
                    sb.Append(lastLineSuffix);
                }

                if (i < lines.Count - 1)
                {
                    sb.AppendLine();
                }
            }
        }

        public static string FormatStoredProgramBody(string body)
        {
            body = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(body);
            body = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(body);

            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            var logicalLines = BuildStoredProgramLogicalLines(body);

            if (logicalLines.Count <= 0)
            {
                return body;
            }

            return BuildIndentedStoredProgramBody(logicalLines);
        }

        private static void AddKeywordIndexes(List<int> indexes, string text, string phrase)
        {
            var words = phrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var index = FindTopLevelPhraseIndex(text, words);

            while (index >= 0)
            {
                if (!indexes.Contains(index))
                {
                    indexes.Add(index);
                }

                index = FindTopLevelPhraseIndex(text.Substring(index + 1), words);

                if (index >= 0)
                {
                    index = index + 1 + (indexes.Count > 0 ? 0 : 0);
                }
            }

            indexes.RemoveAll(x => x < 0);

            var start = 0;

            while (true)
            {
                var next = FindTopLevelPhraseIndexWithOffset(text, start, words);

                if (next < 0)
                {
                    break;
                }

                if (!indexes.Contains(next))
                {
                    indexes.Add(next);
                }

                start = next + 1;
            }
        }

        private static int FindTopLevelPhraseIndexWithOffset(string text, int startIndex, params string[] words)
        {
            if (string.IsNullOrWhiteSpace(text) || words == null || words.Length <= 0)
            {
                return -1;
            }

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(text);

            if (tokens.Count <= 0)
            {
                return -1;
            }

            for (var i = 0; i <= tokens.Count - words.Length; i++)
            {
                if (tokens[i].StartIndex < startIndex)
                {
                    continue;
                }

                if (!MatchesPhrase(tokens, i, words))
                {
                    continue;
                }

                if (words.Length == 1 && string.Equals(words[0], "JOIN", StringComparison.OrdinalIgnoreCase))
                {
                    if (i > 0)
                    {
                        var prev = tokens[i - 1].Text;

                        if (string.Equals(prev, "INNER", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prev, "LEFT", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prev, "RIGHT", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prev, "CROSS", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(prev, "OUTER", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }
                }

                return tokens[i].StartIndex;
            }

            return -1;
        }

        private static List<StoredProgramLine> BuildStoredProgramLogicalLines(string body)
        {
            var list = new List<StoredProgramLine>();
            var statements = SplitStoredProgramStatements(body);

            foreach (var statement in statements)
            {
                ExpandStoredProgramStatement(statement, list);
            }

            return list.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Text)).ToList();
        }

        private static List<StoredProgramLine> SplitStoredProgramStatements(string body)
        {
            var list = new List<StoredProgramLine>();

            if (string.IsNullOrWhiteSpace(body))
            {
                return list;
            }

            var sb = new StringBuilder();
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBacktick = false;
            var inLineComment = false;
            var inBlockComment = false;

            for (var i = 0; i < body.Length; i++)
            {
                var ch = body[i];
                var next = i + 1 < body.Length ? body[i + 1] : '\0';

                if (inLineComment)
                {
                    sb.Append(ch);

                    if (ch == '\n')
                    {
                        inLineComment = false;

                        var commentText = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(sb.ToString());

                        if (!string.IsNullOrWhiteSpace(commentText))
                        {
                            AppendOrMergeCommentBlock(list, commentText);
                        }

                        sb.Clear();
                    }

                    continue;
                }

                if (inBlockComment)
                {
                    sb.Append(ch);

                    if (ch == '*' && next == '/')
                    {
                        sb.Append(next);
                        i++;
                        inBlockComment = false;

                        var commentText = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(sb.ToString());

                        if (!string.IsNullOrWhiteSpace(commentText))
                        {
                            list.Add(new StoredProgramLine
                            {
                                Text = commentText,
                                HasSemicolon = false,
                                IsCommentBlock = true
                            });
                        }

                        sb.Clear();
                    }

                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote && !inBacktick)
                {
                    if (IsBufferWhitespaceOnly(sb) && ch == '-' && next == '-')
                    {
                        sb.Clear();
                        sb.Append(ch).Append(next);
                        i++;
                        inLineComment = true;
                        continue;
                    }

                    if (IsBufferWhitespaceOnly(sb) && ch == '#')
                    {
                        sb.Clear();
                        sb.Append(ch);
                        inLineComment = true;
                        continue;
                    }

                    if (IsBufferWhitespaceOnly(sb) && ch == '/' && next == '*')
                    {
                        sb.Clear();
                        sb.Append(ch).Append(next);
                        i++;
                        inBlockComment = true;
                        continue;
                    }
                }

                if (!inDoubleQuote && !inBacktick && ch == '\'')
                {
                    sb.Append(ch);

                    if (inSingleQuote)
                    {
                        if (next == '\'')
                        {
                            sb.Append(next);
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

                if (!inSingleQuote && !inDoubleQuote && !inBacktick && ch == ';')
                {
                    var statementText = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(sb.ToString());
                    var trailingComment = ReadTrailingInlineComment(body, ref i);

                    if (!string.IsNullOrWhiteSpace(statementText))
                    {
                        if (string.IsNullOrWhiteSpace(trailingComment))
                        {
                            list.Add(new StoredProgramLine
                            {
                                Text = statementText,
                                HasSemicolon = true,
                                IsCommentBlock = false
                            });
                        }
                        else
                        {
                            list.Add(new StoredProgramLine
                            {
                                Text = $"{statementText}; {trailingComment}",
                                HasSemicolon = false,
                                IsCommentBlock = false
                            });
                        }
                    }

                    sb.Clear();
                    continue;
                }

                sb.Append(ch);
            }

            var finalText = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(sb.ToString());

            if (!string.IsNullOrWhiteSpace(finalText))
            {
                list.Add(new StoredProgramLine
                {
                    Text = finalText,
                    HasSemicolon = false,
                    IsCommentBlock = false
                });
            }

            return list;
        }

        private static void ExpandStoredProgramStatement(StoredProgramLine line, List<StoredProgramLine> output)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.Text))
            {
                return;
            }

            if (line.IsCommentBlock)
            {
                output.Add(new StoredProgramLine
                {
                    Text = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(line.Text).Trim(),
                    HasSemicolon = false,
                    IsCommentBlock = true
                });

                return;
            }

            var text = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(line.Text).Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (TrySplitLeadingKeyword(text, "BEGIN", out var beginRest))
            {
                output.Add(new StoredProgramLine
                {
                    Text = "BEGIN",
                    HasSemicolon = false,
                    IsCommentBlock = false
                });

                if (!string.IsNullOrWhiteSpace(beginRest))
                {
                    ExpandStoredProgramFragment(beginRest, line.HasSemicolon, output);
                }

                return;
            }

            if (TrySplitLeadingKeyword(text, "ELSE", out var elseRest) && !text.StartsWith("ELSEIF", StringComparison.OrdinalIgnoreCase))
            {
                output.Add(new StoredProgramLine
                {
                    Text = "ELSE",
                    HasSemicolon = false,
                    IsCommentBlock = false
                });

                if (!string.IsNullOrWhiteSpace(elseRest))
                {
                    ExpandStoredProgramFragment(elseRest, line.HasSemicolon, output);
                }

                return;
            }

            if (TrySplitAfterPhrase(text, "IF", new[] { "THEN" }, out var ifHead, out var ifTail))
            {
                output.Add(new StoredProgramLine
                {
                    Text = NormalizeControlHeader(ifHead),
                    HasSemicolon = false,
                    IsCommentBlock = false
                });

                if (!string.IsNullOrWhiteSpace(ifTail))
                {
                    ExpandStoredProgramFragment(ifTail, line.HasSemicolon, output);
                }

                return;
            }

            if (TrySplitAfterPhrase(text, "ELSEIF", new[] { "THEN" }, out var elseIfHead, out var elseIfTail))
            {
                output.Add(new StoredProgramLine
                {
                    Text = NormalizeControlHeader(elseIfHead),
                    HasSemicolon = false,
                    IsCommentBlock = false
                });

                if (!string.IsNullOrWhiteSpace(elseIfTail))
                {
                    ExpandStoredProgramFragment(elseIfTail, line.HasSemicolon, output);
                }

                return;
            }

            if (TrySplitAfterPhrase(text, "WHILE", new[] { "DO" }, out var whileHead, out var whileTail))
            {
                output.Add(new StoredProgramLine
                {
                    Text = NormalizeControlHeader(whileHead),
                    HasSemicolon = false,
                    IsCommentBlock = false
                });

                if (!string.IsNullOrWhiteSpace(whileTail))
                {
                    ExpandStoredProgramFragment(whileTail, line.HasSemicolon, output);
                }

                return;
            }

            if (TrySplitLeadingKeyword(text, "REPEAT", out var repeatRest))
            {
                output.Add(new StoredProgramLine
                {
                    Text = "REPEAT",
                    HasSemicolon = false,
                    IsCommentBlock = false
                });

                if (!string.IsNullOrWhiteSpace(repeatRest))
                {
                    ExpandStoredProgramFragment(repeatRest, line.HasSemicolon, output);
                }

                return;
            }

            if (TrySplitBeforePhrase(text, "UNTIL", new[] { "END", "REPEAT" }, out var untilHead, out var endRepeatTail))
            {
                output.Add(new StoredProgramLine
                {
                    Text = NormalizeControlHeader(untilHead),
                    HasSemicolon = false,
                    IsCommentBlock = false
                });

                if (!string.IsNullOrWhiteSpace(endRepeatTail))
                {
                    ExpandStoredProgramFragment(endRepeatTail, line.HasSemicolon, output);
                }

                return;
            }

            if (TrySplitAfterLoopKeyword(text, out var loopHead, out var loopTail))
            {
                output.Add(new StoredProgramLine
                {
                    Text = NormalizeControlHeader(loopHead),
                    HasSemicolon = false,
                    IsCommentBlock = false
                });

                if (!string.IsNullOrWhiteSpace(loopTail))
                {
                    ExpandStoredProgramFragment(loopTail, line.HasSemicolon, output);
                }

                return;
            }

            output.Add(new StoredProgramLine
            {
                Text = text,
                HasSemicolon = line.HasSemicolon,
                IsCommentBlock = false
            });
        }

        private static string BuildIndentedStoredProgramBody(List<StoredProgramLine> lines)
        {
            var sb = new StringBuilder();
            var indentLevel = 0;
            var previousKind = StoredProgramLineKind.Normal;
            var previousText = string.Empty;
            var hasPreviousLine = false;
            var previousWasComment = false;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (line == null || string.IsNullOrWhiteSpace(line.Text))
                {
                    continue;
                }

                if (line.IsCommentBlock)
                {
                    if (hasPreviousLine && !previousWasComment)
                    {
                        sb.AppendLine();
                    }

                    AppendIndentedCommentBlock(sb, line.Text, indentLevel * 4);
                    sb.AppendLine();

                    previousKind = StoredProgramLineKind.Normal;
                    previousText = line.Text;
                    hasPreviousLine = true;
                    previousWasComment = true;
                    continue;
                }

                var kind = ClassifyStoredProgramLine(line.Text);

                switch (kind)
                {
                    case StoredProgramLineKind.End:
                    case StoredProgramLineKind.EndIf:
                    case StoredProgramLineKind.EndWhile:
                    case StoredProgramLineKind.EndLoop:
                        {
                            indentLevel = Math.Max(0, indentLevel - 1);
                            break;
                        }
                    case StoredProgramLineKind.EndRepeat:
                        {
                            if (previousKind != StoredProgramLineKind.Until)
                            {
                                indentLevel = Math.Max(0, indentLevel - 1);
                            }

                            break;
                        }
                    case StoredProgramLineKind.Else:
                    case StoredProgramLineKind.ElseIfThen:
                        {
                            indentLevel = Math.Max(0, indentLevel - 1);
                            break;
                        }
                    case StoredProgramLineKind.Until:
                        {
                            indentLevel = Math.Max(0, indentLevel - 1);
                            break;
                        }
                }

                if (hasPreviousLine && !previousWasComment && ShouldInsertBlankLineBetweenStatements(previousKind, kind, previousText, line.Text))
                {
                    sb.AppendLine();
                }

                if (kind == StoredProgramLineKind.Normal)
                {
                    sb.Append(FormatStoredProgramExecutableStatement(line.Text, indentLevel * 4, line.HasSemicolon));
                    sb.AppendLine();
                }
                else
                {
                    sb.Append(new string(' ', indentLevel * 4));
                    sb.Append(line.Text.Trim());

                    if (line.HasSemicolon)
                    {
                        sb.Append(";");
                    }

                    sb.AppendLine();
                }

                switch (kind)
                {
                    case StoredProgramLineKind.Begin:
                    case StoredProgramLineKind.IfThen:
                    case StoredProgramLineKind.WhileDo:
                    case StoredProgramLineKind.Repeat:
                    case StoredProgramLineKind.Loop:
                        {
                            indentLevel++;
                            break;
                        }
                    case StoredProgramLineKind.Else:
                    case StoredProgramLineKind.ElseIfThen:
                        {
                            indentLevel++;
                            break;
                        }
                }

                previousKind = kind;
                previousText = line.Text;
                hasPreviousLine = true;
                previousWasComment = false;
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string FormatStoredProgramExecutableStatement(string text, int baseIndent, bool hasSemicolon)
        {
            text = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(text).Trim();

            string formatted;

            if (TryFormatDeclareStatement(text, out formatted)
                || TryFormatCreateTableStatement(text, out formatted)
                || TryFormatInsertSelectStatement(text, out formatted)
                || TryFormatInsertValuesStatement(text, out formatted)
                || TryFormatSelectStatement(text, out formatted)
                || TryFormatUpdateStatement(text, out formatted))
            {
                formatted = MySqlCreateScriptFormatterHelper.IndentText(formatted, baseIndent);
            }
            else
            {
                formatted = new string(' ', baseIndent) + MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(text);
            }

            if (hasSemicolon)
            {
                formatted += ";";
            }

            return formatted;
        }

        private static bool TryFormatCreateTableStatement(string text, out string result)
        {
            result = string.Empty;

            if (string.IsNullOrWhiteSpace(text)
                || (!text.StartsWith("CREATE TABLE ", StringComparison.OrdinalIgnoreCase)
                && !text.StartsWith("CREATE TEMPORARY TABLE ", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var openParenIndex = MySqlCreateScriptFormatterHelper.FindFirstTopLevelChar(text, '(');

            if (openParenIndex < 0)
            {
                return false;
            }

            var closeParenIndex = MySqlCreateScriptFormatterHelper.FindMatchingParen(text, openParenIndex);

            if (closeParenIndex < 0)
            {
                return false;
            }

            var header = text.Substring(0, openParenIndex).TrimEnd();
            var body = text.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            var tail = text.Substring(closeParenIndex + 1).Trim();

            var items = MySqlCreateScriptFormatterHelper.SplitTopLevelCommaItems(body)
                                                        .Select(x => MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(x).Trim())
                                                        .Where(x => !string.IsNullOrWhiteSpace(x))
                                                        .ToList();

            var sb = new StringBuilder();

            sb.AppendLine(header);
            sb.AppendLine("(");

            for (var i = 0; i < items.Count; i++)
            {
                var suffix = i == items.Count - 1 ? string.Empty : ",";

                sb.AppendLine($"    {items[i]}{suffix}");
            }

            sb.Append(")");

            if (!string.IsNullOrWhiteSpace(tail))
            {
                sb.Append(" ").Append(tail);
            }

            result = sb.ToString().TrimEnd('\r', '\n');
            return true;
        }

        private static bool TryFormatInsertSelectStatement(string text, out string result)
        {
            result = string.Empty;

            if (string.IsNullOrWhiteSpace(text) || !text.StartsWith("INSERT ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var selectIndex = FindTopLevelPhraseIndex(text, "SELECT");

            if (selectIndex < 0)
            {
                return false;
            }

            var prefix = text.Substring(0, selectIndex).TrimEnd();
            var selectPart = text.Substring(selectIndex).TrimStart();

            if (!TryFormatSelectStatement(selectPart, out var formattedSelect))
            {
                return false;
            }

            result = $"{prefix}\r\n{formattedSelect}";
            return true;
        }

        private static bool TryFormatInsertValuesStatement(string text, out string result)
        {
            result = string.Empty;

            if (string.IsNullOrWhiteSpace(text) || !text.StartsWith("INSERT ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var valuesIndex = FindTopLevelPhraseIndex(text, "VALUES");

            if (valuesIndex < 0)
            {
                return false;
            }

            var selectIndex = FindTopLevelPhraseIndex(text, "SELECT");

            if (selectIndex >= 0 && selectIndex < valuesIndex)
            {
                return false;
            }

            var head = text.Substring(0, valuesIndex).TrimEnd();
            var valuesPart = text.Substring(valuesIndex).TrimStart();

            result = $"{head}\r\n{valuesPart}";
            return true;
        }

        private static bool TryFormatSelectStatement(string text, out string result)
        {
            result = string.Empty;

            if (string.IsNullOrWhiteSpace(text) || !text.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var clauses = SplitTopLevelQueryClauses
            (
                text,
                "SELECT",
                "FROM",
                "INNER JOIN",
                "LEFT OUTER JOIN",
                "LEFT JOIN",
                "RIGHT OUTER JOIN",
                "RIGHT JOIN",
                "CROSS JOIN",
                "JOIN",
                "ON",
                "WHERE",
                "GROUP BY",
                "HAVING",
                "ORDER BY",
                "LIMIT",
                "INTO"
            );

            if (clauses.Count <= 0)
            {
                return false;
            }

            var sb = new StringBuilder();

            for (var i = 0; i < clauses.Count; i++)
            {
                var clause = clauses[i];

                if (string.IsNullOrWhiteSpace(clause))
                {
                    continue;
                }

                if (i > 0)
                {
                    sb.AppendLine();
                }

                if (clause.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatHangingListClause("SELECT ", clause.Substring(6).Trim()));
                }
                else if (clause.StartsWith("FROM ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append($"  FROM {MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause.Substring(4).Trim())}");
                }
                else if (IsJoinClause(clause))
                {
                    var value = MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause);
                    var firstSpaceIndex = value.IndexOf(' ');
                    var firstKeywordLength = firstSpaceIndex > 0 ? firstSpaceIndex : value.Length;
                    var leftPadding = Math.Max(0, 6 - firstKeywordLength);

                    sb.Append(new string(' ', leftPadding));
                    sb.Append(value);
                }
                else if (clause.StartsWith("ON ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatHangingLogicalClause("    ON ", clause.Substring(2).Trim()));
                }
                else if (clause.StartsWith("WHERE ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatHangingLogicalClause(" WHERE ", clause.Substring(5).Trim()));
                }
                else if (clause.StartsWith("GROUP BY ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatHangingListClause(" GROUP BY ", clause.Substring(8).Trim()));
                }
                else if (clause.StartsWith("HAVING ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatHangingLogicalClause("HAVING ", clause.Substring(6).Trim()));
                }
                else if (clause.StartsWith("ORDER BY ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatHangingListClause(" ORDER BY ", clause.Substring(8).Trim()));
                }
                else if (clause.StartsWith("LIMIT ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append($" LIMIT {MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause.Substring(5).Trim())}");
                }
                else if (clause.StartsWith("INTO ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append($"  INTO {MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause.Substring(4).Trim())}");
                }
                else
                {
                    sb.Append(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause));
                }
            }

            result = sb.ToString().TrimEnd('\r', '\n');
            return true;
        }

        private static bool TryFormatUpdateStatement(string text, out string result)
        {
            result = string.Empty;

            if (string.IsNullOrWhiteSpace(text) || !text.StartsWith("UPDATE ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var clauses = SplitTopLevelQueryClauses
            (
                text,
                "UPDATE",
                "SET",
                "WHERE",
                "ORDER BY",
                "LIMIT"
            );

            if (clauses.Count <= 0)
            {
                return false;
            }

            var sb = new StringBuilder();

            for (var i = 0; i < clauses.Count; i++)
            {
                var clause = clauses[i];

                if (string.IsNullOrWhiteSpace(clause))
                {
                    continue;
                }

                if (i > 0)
                {
                    sb.AppendLine();
                }

                if (clause.StartsWith("UPDATE ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause));
                }
                else if (clause.StartsWith("SET ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatHangingListClause("   SET ", clause.Substring(3).Trim()));
                }
                else if (clause.StartsWith("WHERE ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatHangingLogicalClause(" WHERE ", clause.Substring(5).Trim()));
                }
                else if (clause.StartsWith("ORDER BY ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatHangingListClause(" ORDER BY ", clause.Substring(8).Trim()));
                }
                else if (clause.StartsWith("LIMIT ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append($" LIMIT {MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause.Substring(5).Trim())}");
                }
                else
                {
                    sb.Append(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause));
                }
            }

            result = sb.ToString().TrimEnd('\r', '\n');
            return true;
        }

        private static string FormatHangingListClause(string prefix, string listText)
        {
            var items = MySqlCreateScriptFormatterHelper.SplitTopLevelCommaItems(listText)
                                                        .Select(x => MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(x).Trim())
                                                        .Where(x => !string.IsNullOrWhiteSpace(x))
                                                        .ToList();

            if (items.Count <= 0)
            {
                return prefix.TrimEnd();
            }

            if (items.Count == 1)
            {
                return $"{prefix}{items[0]}";
            }

            var sb = new StringBuilder();

            sb.Append(prefix).Append(items[0]);

            for (var i = 1; i < items.Count; i++)
            {
                sb.AppendLine(",");
                sb.Append(new string(' ', prefix.Length));
                sb.Append(items[i]);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string FormatHangingLogicalClause(string prefix, string conditionText)
        {
            var parts = SplitTopLevelLogicalConditions(conditionText)
                        .Select(x => MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(x).Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();

            if (parts.Count <= 0)
            {
                return prefix.TrimEnd();
            }

            if (parts.Count == 1)
            {
                return $"{prefix}{parts[0]}";
            }

            var sb = new StringBuilder();

            sb.Append(prefix).Append(parts[0]);

            //讓 AND / OR 右側的內容對齊到第一行條件內容的起始位置
            //prefix 已經包含了 WHERE / HAVING / ON 與其後空格
            //AND + 空格固定佔 4 個字元，所以 continuation 要扣掉這 4 個字元
            var continuationPrefix = new string(' ', Math.Max(0, prefix.Length - 4));

            for (var i = 1; i < parts.Count; i++)
            {
                sb.AppendLine();
                sb.Append(continuationPrefix);
                sb.Append(parts[i]);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static bool IsJoinClause(string clause)
        {
            return clause.StartsWith("INNER JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("LEFT JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("LEFT OUTER JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("RIGHT JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("RIGHT OUTER JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("CROSS JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("JOIN ", StringComparison.OrdinalIgnoreCase);
        }

        private static StoredProgramLineKind ClassifyStoredProgramLine(string text)
        {
            text = (text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                return StoredProgramLineKind.Normal;
            }

            if (string.Equals(text, "BEGIN", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.Begin;
            }

            if (string.Equals(text, "END", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.End;
            }

            if (string.Equals(text, "ELSE", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.Else;
            }

            if (string.Equals(text, "END IF", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.EndIf;
            }

            if (string.Equals(text, "END WHILE", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.EndWhile;
            }

            if (string.Equals(text, "END REPEAT", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.EndRepeat;
            }

            if (text.StartsWith("END LOOP", StringComparison.OrdinalIgnoreCase)
                || text.EndsWith(" END LOOP", StringComparison.OrdinalIgnoreCase)
                || text.IndexOf(": END LOOP", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return StoredProgramLineKind.EndLoop;
            }

            if (text.StartsWith("IF ", StringComparison.OrdinalIgnoreCase) && text.EndsWith(" THEN", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.IfThen;
            }

            if (text.StartsWith("ELSEIF ", StringComparison.OrdinalIgnoreCase) && text.EndsWith(" THEN", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.ElseIfThen;
            }

            if (text.StartsWith("WHILE ", StringComparison.OrdinalIgnoreCase) && text.EndsWith(" DO", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.WhileDo;
            }

            if (string.Equals(text, "REPEAT", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.Repeat;
            }

            if (text.StartsWith("UNTIL ", StringComparison.OrdinalIgnoreCase))
            {
                return StoredProgramLineKind.Until;
            }

            if (text.EndsWith(" LOOP", StringComparison.OrdinalIgnoreCase) || text.IndexOf(": LOOP", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return StoredProgramLineKind.Loop;
            }

            return StoredProgramLineKind.Normal;
        }

        private static bool TrySplitLeadingKeyword(string text, string keyword, out string rest)
        {
            rest = string.Empty;

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
            {
                return false;
            }

            text = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(text).TrimStart();

            if (!text.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (text.Length == keyword.Length)
            {
                return true;
            }

            var nextChar = text[keyword.Length];

            if (!char.IsWhiteSpace(nextChar))
            {
                return false;
            }

            rest = text.Substring(keyword.Length).TrimStart();
            return true;
        }

        private static bool TrySplitAfterPhrase(string text, string firstWord, string[] endPhraseWords, out string head, out string tail)
        {
            head = string.Empty;
            tail = string.Empty;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(text);

            if (tokens.Count <= 0)
            {
                return false;
            }

            if (!string.Equals(tokens[0].Text, firstWord, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            for (var i = 0; i <= tokens.Count - endPhraseWords.Length; i++)
            {
                if (!MatchesPhrase(tokens, i, endPhraseWords))
                {
                    continue;
                }

                var endIndex = tokens[i + endPhraseWords.Length - 1].StartIndex + tokens[i + endPhraseWords.Length - 1].Text.Length;

                if (endIndex >= text.Length)
                {
                    return false;
                }

                head = text.Substring(0, endIndex).Trim();
                tail = text.Substring(endIndex).Trim();

                return !string.IsNullOrWhiteSpace(tail);
            }

            return false;
        }

        private static bool TrySplitBeforePhrase(string text, string firstWord, string[] splitPhraseWords, out string head, out string tail)
        {
            head = string.Empty;
            tail = string.Empty;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(text);

            if (tokens.Count <= 0)
            {
                return false;
            }

            if (!string.Equals(tokens[0].Text, firstWord, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            for (var i = 1; i <= tokens.Count - splitPhraseWords.Length; i++)
            {
                if (!MatchesPhrase(tokens, i, splitPhraseWords))
                {
                    continue;
                }

                var splitIndex = tokens[i].StartIndex;

                head = text.Substring(0, splitIndex).TrimEnd();
                tail = text.Substring(splitIndex).TrimStart();

                return !string.IsNullOrWhiteSpace(head) && !string.IsNullOrWhiteSpace(tail);
            }

            return false;
        }

        private static bool TrySplitAfterLoopKeyword(string text, out string head, out string tail)
        {
            head = string.Empty;
            tail = string.Empty;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(text);

            if (tokens.Count <= 0)
            {
                return false;
            }

            for (var i = 0; i < tokens.Count; i++)
            {
                if (!string.Equals(tokens[i].Text, "LOOP", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var loopEnd = tokens[i].StartIndex + tokens[i].Text.Length;

                if (loopEnd >= text.Length)
                {
                    return false;
                }

                head = text.Substring(0, loopEnd).Trim();
                tail = text.Substring(loopEnd).Trim();

                return !string.IsNullOrWhiteSpace(tail);
            }

            return false;
        }

        private static bool IsBufferWhitespaceOnly(StringBuilder sb)
        {
            if (sb == null || sb.Length <= 0)
            {
                return true;
            }

            for (var i = 0; i < sb.Length; i++)
            {
                if (!char.IsWhiteSpace(sb[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static string NormalizeControlHeader(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            text = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(text);

            var sb = new StringBuilder();
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBacktick = false;
            var lastWasWhitespace = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                var next = i + 1 < text.Length ? text[i + 1] : '\0';

                if (!inDoubleQuote && !inBacktick && ch == '\'')
                {
                    sb.Append(ch);

                    if (inSingleQuote)
                    {
                        if (next == '\'')
                        {
                            sb.Append(next);
                            i++;
                            continue;
                        }

                        inSingleQuote = false;
                    }
                    else
                    {
                        inSingleQuote = true;
                    }

                    lastWasWhitespace = false;
                    continue;
                }

                if (!inSingleQuote && !inBacktick && ch == '"')
                {
                    sb.Append(ch);
                    inDoubleQuote = !inDoubleQuote;
                    lastWasWhitespace = false;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote && ch == '`')
                {
                    sb.Append(ch);
                    inBacktick = !inBacktick;
                    lastWasWhitespace = false;
                    continue;
                }

                if (inSingleQuote || inDoubleQuote || inBacktick)
                {
                    sb.Append(ch);
                    lastWasWhitespace = false;
                    continue;
                }

                if (char.IsWhiteSpace(ch))
                {
                    if (!lastWasWhitespace)
                    {
                        sb.Append(' ');
                        lastWasWhitespace = true;
                    }

                    continue;
                }

                sb.Append(ch);
                lastWasWhitespace = false;
            }

            return sb.ToString().Trim();
        }

        public static int GetEndIndex(List<MySqlCreateScriptFormatterHelper.TokenInfo> tokens,
                                      int currentIndex, int declarationLength, params int[] candidateIndexes)
        {
            if (tokens == null || tokens.Count <= 0)
            {
                return declarationLength;
            }

            var candidates = (candidateIndexes ?? Array.Empty<int>())
                             .Where(x => x > currentIndex && x >= 0 && x < tokens.Count)
                             .Select(x => tokens[x].StartIndex)
                             .ToList();

            return candidates.Count > 0
                   ? candidates.Min()
                   : declarationLength;
        }

        private static bool ShouldInsertBlankLineBetweenStatements(StoredProgramLineKind previousKind, StoredProgramLineKind currentKind,
                                                                   string previousText, string currentText)
        {
            var prev = (previousText ?? string.Empty).Trim();
            var curr = (currentText ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(prev) || string.IsNullOrWhiteSpace(curr))
            {
                return false;
            }

            if (currentKind == StoredProgramLineKind.End
                || currentKind == StoredProgramLineKind.EndIf
                || currentKind == StoredProgramLineKind.EndWhile
                || currentKind == StoredProgramLineKind.EndRepeat
                || currentKind == StoredProgramLineKind.EndLoop
                || currentKind == StoredProgramLineKind.Else
                || currentKind == StoredProgramLineKind.ElseIfThen
                || currentKind == StoredProgramLineKind.Until)
            {
                return false;
            }

            if (previousKind == StoredProgramLineKind.Begin)
            {
                return false;
            }

            if (IsDeclareStatement(prev) && !IsDeclareStatement(curr))
            {
                return true;
            }

            if ((previousKind == StoredProgramLineKind.EndIf
                 || previousKind == StoredProgramLineKind.EndWhile
                 || previousKind == StoredProgramLineKind.EndRepeat
                 || previousKind == StoredProgramLineKind.EndLoop)
                 && !string.Equals(curr, "END", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if ((currentKind == StoredProgramLineKind.IfThen
                 || currentKind == StoredProgramLineKind.WhileDo
                 || currentKind == StoredProgramLineKind.Repeat
                 || currentKind == StoredProgramLineKind.Loop)
                && (IsSqlLikeStatement(prev) || IsDeclareStatement(prev)))
            {
                return true;
            }

            if (IsSqlLikeStatement(curr) && (IsSqlLikeStatement(prev) || previousKind == StoredProgramLineKind.EndIf
                || previousKind == StoredProgramLineKind.EndWhile || previousKind == StoredProgramLineKind.EndRepeat
                || previousKind == StoredProgramLineKind.EndLoop))
            {
                return true;
            }

            return false;
        }

        private static bool IsDeclareStatement(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && text.TrimStart().StartsWith("DECLARE ", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSqlLikeStatement(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.TrimStart();

            return text.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("INSERT ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("UPDATE ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("DELETE ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("CREATE ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("DROP ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("SET ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("RETURN ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("LEAVE ", StringComparison.OrdinalIgnoreCase);
        }

        private static void AppendOrMergeCommentBlock(List<StoredProgramLine> list, string commentText)
        {
            if (list == null || string.IsNullOrWhiteSpace(commentText))
            {
                return;
            }

            var normalized = string.Join
            (
                "\r\n",
                MySqlCreateScriptFormatterHelper.NormalizeLineEndings(commentText)
                                                .Split(new[] { "\r\n" }, StringSplitOptions.None)
                                                .Select(x => x.Trim())
                                                .Where(x => !string.IsNullOrWhiteSpace(x))
            );

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return;
            }

            if (list.Count > 0 && list[list.Count - 1] != null && list[list.Count - 1].IsCommentBlock
                && !list[list.Count - 1].Text.TrimStart().StartsWith("/*", StringComparison.Ordinal))
            {
                list[list.Count - 1].Text += "\r\n" + normalized;
                return;
            }

            list.Add(new StoredProgramLine
            {
                Text = normalized,
                HasSemicolon = false,
                IsCommentBlock = true
            });
        }

        private static string ReadTrailingInlineComment(string body, ref int index)
        {
            if (string.IsNullOrWhiteSpace(body) || index < 0 || index >= body.Length)
            {
                return string.Empty;
            }

            var j = index + 1;

            while (j < body.Length && (body[j] == ' ' || body[j] == '\t'))
            {
                j++;
            }

            if (j >= body.Length || body[j] == '\r' || body[j] == '\n')
            {
                return string.Empty;
            }

            if (body[j] == '#')
            {
                var start = j;

                while (j < body.Length && body[j] != '\r' && body[j] != '\n')
                {
                    j++;
                }

                index = j - 1;
                return body.Substring(start, j - start).Trim();
            }

            if (body[j] == '-' && j + 1 < body.Length && body[j + 1] == '-')
            {
                var start = j;

                while (j < body.Length && body[j] != '\r' && body[j] != '\n')
                {
                    j++;
                }

                index = j - 1;
                return body.Substring(start, j - start).Trim();
            }

            return string.Empty;
        }

        private static bool TryFormatDeclareStatement(string text, out string result)
        {
            result = string.Empty;

            if (string.IsNullOrWhiteSpace(text) || !text.StartsWith("DECLARE ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            text = MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(text);

            var commentPos = FindLineCommentStart(text);
            var body = commentPos >= 0 ? text.Substring(0, commentPos).TrimEnd() : text.TrimEnd();
            var comment = commentPos >= 0 ? text.Substring(commentPos).Trim() : string.Empty;

            result = string.IsNullOrWhiteSpace(comment) ? body : $"{body} {comment}";

            return true;
        }

        private static int FindLineCommentStart(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return -1;
            }

            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBacktick = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                var next = i + 1 < text.Length ? text[i + 1] : '\0';

                if (!inDoubleQuote && !inBacktick && ch == '\'')
                {
                    if (inSingleQuote)
                    {
                        if (next == '\'')
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

                if (ch == '#')
                {
                    return i;
                }

                if (ch == '-' && next == '-')
                {
                    return i;
                }
            }

            return -1;
        }

        private static void ExpandStoredProgramFragment(string text, bool hasSemicolon, List<StoredProgramLine> output)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var fragments = SplitStoredProgramStatements(text);

            if (fragments == null || fragments.Count <= 0)
            {
                ExpandStoredProgramStatement
                (
                    new StoredProgramLine
                    {
                        Text = text,
                        HasSemicolon = hasSemicolon,
                        IsCommentBlock = false
                    },
                    output
                );

                return;
            }

            if (hasSemicolon && !fragments[fragments.Count - 1].HasSemicolon)
            {
                fragments[fragments.Count - 1].HasSemicolon = true;
            }

            foreach (var fragment in fragments)
            {
                ExpandStoredProgramStatement(fragment, output);
            }
        }

        private static void AppendIndentedCommentBlock(StringBuilder sb, string text, int indentSpaces)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var indent = new string(' ', indentSpaces);

            var rawLines = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(text)
                                                           .Split(new[] { "\r\n" }, StringSplitOptions.None)
                                                           .Select(x => x.Trim())
                                                           .ToList();

            while (rawLines.Count > 0 && string.IsNullOrWhiteSpace(rawLines[0]))
            {
                rawLines.RemoveAt(0);
            }

            while (rawLines.Count > 0 && string.IsNullOrWhiteSpace(rawLines[rawLines.Count - 1]))
            {
                rawLines.RemoveAt(rawLines.Count - 1);
            }

            if (rawLines.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < rawLines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(rawLines[i]))
                {
                    sb.AppendLine();
                }
                else
                {
                    sb.Append(indent).AppendLine(rawLines[i]);
                }
            }
        }
    }
}
