using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers
{
    internal static class QueryEditorAutoCompleteSpaceAnalyzer
    {
        private enum SqlTokenKind
        {
            None = 0,
            Word,
            DelimitedIdentifier,
            StringLiteral,
            Symbol
        }

        private sealed class SqlToken
        {
            public SqlTokenKind Kind { get; set; }

            public string Text { get; set; }

            public int Start { get; set; }

            public int End { get; set; }

            public int Depth { get; set; }

            public bool IsWord(string value)
            {
                return Kind == SqlTokenKind.Word && string.Equals(Text, value, StringComparison.OrdinalIgnoreCase);
            }

            public bool IsSymbol(string value)
            {
                return Kind == SqlTokenKind.Symbol && string.Equals(Text, value, StringComparison.Ordinal);
            }
        }

        private sealed class TokenizeResult
        {
            public List<SqlToken> Tokens { get; } = new List<SqlToken>();

            public List<Tuple<int, int>> ProtectedRanges { get; } = new List<Tuple<int, int>>();
        }

        private sealed class SourceResolution
        {
            public QueryEditorAutoCompleteSpaceSourceKind Kind { get; set; }

            public string ObjectName { get; set; }

            public string AliasName { get; set; }

            public string SourceSql { get; set; }
        }

        private static readonly HashSet<string> ReservedAliasWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "WHERE", "JOIN", "LEFT", "RIGHT", "FULL", "INNER", "OUTER", "CROSS", "ON", "GROUP", "ORDER",
            "HAVING", "UNION", "EXCEPT", "INTERSECT", "LIMIT", "OFFSET", "FETCH", "FOR", "RETURNING", "SET",
            "CONNECT", "START", "MODEL", "WINDOW", "QUALIFY", "INTO", "USING"
        };

        public static QueryEditorAutoCompleteSpaceAnalysisResult Analyze(QueryEditorAutoCompleteSpaceAnalysisRequest request)
        {
            var result = CreateEmptyResult();

            if (request == null || !IsSupportedDataSourceType(request.DataSourceType) || string.IsNullOrWhiteSpace(request.Sql))
            {
                return result;
            }

            var sql = request.Sql;
            var caretPosition = Math.Max(0, Math.Min(request.CaretPosition, sql.Length));
            var tokenizeResult = Tokenize(sql, request.DataSourceType);

            if (IsInsideProtectedRange(tokenizeResult.ProtectedRanges, caretPosition))
            {
                return result;
            }

            var tokensBeforeCaret = tokenizeResult.Tokens.Where(token => token.End <= caretPosition).ToList();

            if (tokensBeforeCaret.Count == 0)
            {
                return result;
            }

            if (!TryResolveKeyword(tokenizeResult.Tokens, tokensBeforeCaret,
                                   out var keyword, out var tableOnly, out var triggerTokenIndex))
            {
                return result;
            }

            var intent = ResolveIntent(keyword);

            if (intent == QueryEditorAutoCompleteSpaceIntent.ListDatabases
                && request.DataSourceType != DataSourceType.SqlServer
                && request.DataSourceType != DataSourceType.MySql)
            {
                return result;
            }

            result.CanResolve = true;
            result.Keyword = keyword;
            result.Intent = intent;
            result.TableOnly = tableOnly;
            result.LookupMode = ResolveLookupMode(intent, tableOnly);

            if (intent != QueryEditorAutoCompleteSpaceIntent.ListColumns)
            {
                return result;
            }

            if (!TryResolveSource(sql, tokenizeResult.Tokens, triggerTokenIndex, keyword, out var source))
            {
                result.CanResolve = false;
                result.Intent = QueryEditorAutoCompleteSpaceIntent.None;
                return result;
            }

            result.SourceKind = source.Kind;
            result.ObjectName = source.ObjectName;
            result.AliasName = source.AliasName;
            result.SourceSql = source.SourceSql;
            return result;
        }

        private static QueryEditorAutoCompleteSpaceAnalysisResult CreateEmptyResult()
        {
            return new QueryEditorAutoCompleteSpaceAnalysisResult
            {
                CanResolve = false,
                Keyword = QueryEditorAutoCompleteSpaceKeyword.None,
                Intent = QueryEditorAutoCompleteSpaceIntent.None,
                LookupMode = AutoCompleteObjectLookupMode.TableOrView,
                SourceKind = QueryEditorAutoCompleteSpaceSourceKind.None,
                ObjectName = string.Empty,
                AliasName = string.Empty,
                SourceSql = string.Empty,
                TableOnly = false
            };
        }

        private static bool IsSupportedDataSourceType(DataSourceType dataSourceType)
        {
            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        return true;
                    }
                case DataSourceType.None:
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool TryResolveKeyword(List<SqlToken> allTokens, List<SqlToken> tokensBeforeCaret,
                                              out QueryEditorAutoCompleteSpaceKeyword keyword, out bool tableOnly, out int triggerTokenIndex)
        {
            keyword = QueryEditorAutoCompleteSpaceKeyword.None;
            tableOnly = false;
            triggerTokenIndex = -1;

            var significantTokens = tokensBeforeCaret.Where(token => !token.IsSymbol(";")).ToList();

            if (TryResolveTableTarget(significantTokens, out triggerTokenIndex))
            {
                keyword = QueryEditorAutoCompleteSpaceKeyword.TableTarget;
                tableOnly = true;
                triggerTokenIndex = allTokens.IndexOf(significantTokens[triggerTokenIndex]);
                return true;
            }

            var lastToken = tokensBeforeCaret[tokensBeforeCaret.Count - 1];

            triggerTokenIndex = allTokens.IndexOf(lastToken);

            if (lastToken.Kind == SqlTokenKind.Word)
            {
                if (lastToken.IsWord("USE"))
                {
                    var topLevelTokens = tokensBeforeCaret.Where(token => token.Depth == lastToken.Depth && !token.IsSymbol(";")).ToList();

                    if (topLevelTokens.Count != 1)
                    {
                        return false;
                    }

                    keyword = QueryEditorAutoCompleteSpaceKeyword.Use;
                    return true;
                }

                if (lastToken.IsWord("FROM"))
                {
                    keyword = QueryEditorAutoCompleteSpaceKeyword.From;
                    return true;
                }

                if (lastToken.IsWord("WHERE"))
                {
                    keyword = QueryEditorAutoCompleteSpaceKeyword.Where;
                    return true;
                }

                if (lastToken.IsWord("AND"))
                {
                    keyword = QueryEditorAutoCompleteSpaceKeyword.And;
                    return true;
                }

                if (lastToken.IsWord("OR"))
                {
                    keyword = QueryEditorAutoCompleteSpaceKeyword.Or;
                    return true;
                }

                if (lastToken.IsWord("SET"))
                {
                    keyword = QueryEditorAutoCompleteSpaceKeyword.Set;
                    return true;
                }

                if (lastToken.IsWord("SELECT"))
                {
                    if (!HasFromAtDepthAfter(allTokens, triggerTokenIndex, lastToken.Depth))
                    {
                        return false;
                    }

                    keyword = QueryEditorAutoCompleteSpaceKeyword.SelectList;
                    return true;
                }

                if (lastToken.IsWord("BY") && IsOrderOrGroupBy(allTokens, triggerTokenIndex, lastToken.Depth))
                {
                    keyword = QueryEditorAutoCompleteSpaceKeyword.By;
                    return true;
                }
            }

            if (lastToken.IsSymbol(",") && IsSelectListComma(allTokens, triggerTokenIndex))
            {
                keyword = QueryEditorAutoCompleteSpaceKeyword.SelectList;
                return true;
            }

            return false;
        }

        private static bool TryResolveTableTarget(List<SqlToken> tokens, out int triggerTokenIndex)
        {
            triggerTokenIndex = -1;

            if (tokens.Count == 1 && tokens[0].IsWord("UPDATE"))
            {
                triggerTokenIndex = 0;
                return true;
            }

            if (tokens.Count == 2
                && ((tokens[0].IsWord("INSERT") && tokens[1].IsWord("INTO"))
                    || (tokens[0].IsWord("MERGE") && tokens[1].IsWord("INTO"))
                    || (tokens[0].IsWord("DELETE") && tokens[1].IsWord("FROM"))))
            {
                triggerTokenIndex = 1;
                return true;
            }

            return false;
        }

        private static bool HasFromAtDepthAfter(List<SqlToken> tokens, int tokenIndex, int depth)
        {
            for (var i = tokenIndex + 1; i < tokens.Count; i++)
            {
                if (tokens[i].Depth < depth || (tokens[i].Depth == depth && tokens[i].IsSymbol(";")))
                {
                    return false;
                }

                if (tokens[i].Depth == depth && tokens[i].IsWord("FROM"))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsOrderOrGroupBy(List<SqlToken> tokens, int byIndex, int depth)
        {
            for (var i = byIndex - 1; i >= 0; i--)
            {
                if (tokens[i].Depth != depth)
                {
                    continue;
                }

                return tokens[i].IsWord("ORDER") || tokens[i].IsWord("GROUP");
            }

            return false;
        }

        private static bool IsSelectListComma(List<SqlToken> tokens, int commaIndex)
        {
            var comma = tokens[commaIndex];
            var selectIndex = FindPreviousWord(tokens, commaIndex - 1, comma.Depth, "SELECT");

            if (selectIndex < 0)
            {
                return false;
            }

            var fromIndex = FindNextWord(tokens, selectIndex + 1, comma.Depth, "FROM");

            return fromIndex < 0 || commaIndex < fromIndex;
        }

        private static QueryEditorAutoCompleteSpaceIntent ResolveIntent(QueryEditorAutoCompleteSpaceKeyword keyword)
        {
            switch (keyword)
            {
                case QueryEditorAutoCompleteSpaceKeyword.TableTarget:
                case QueryEditorAutoCompleteSpaceKeyword.From:
                    {
                        return QueryEditorAutoCompleteSpaceIntent.ListObjects;
                    }
                case QueryEditorAutoCompleteSpaceKeyword.Use:
                    {
                        return QueryEditorAutoCompleteSpaceIntent.ListDatabases;
                    }
                case QueryEditorAutoCompleteSpaceKeyword.Where:
                case QueryEditorAutoCompleteSpaceKeyword.And:
                case QueryEditorAutoCompleteSpaceKeyword.Or:
                case QueryEditorAutoCompleteSpaceKeyword.By:
                case QueryEditorAutoCompleteSpaceKeyword.Set:
                case QueryEditorAutoCompleteSpaceKeyword.SelectList:
                    {
                        return QueryEditorAutoCompleteSpaceIntent.ListColumns;
                    }
                case QueryEditorAutoCompleteSpaceKeyword.None:
                default:
                    {
                        return QueryEditorAutoCompleteSpaceIntent.None;
                    }
            }
        }

        private static AutoCompleteObjectLookupMode ResolveLookupMode(QueryEditorAutoCompleteSpaceIntent intent, bool tableOnly)
        {
            if (intent == QueryEditorAutoCompleteSpaceIntent.ListDatabases)
            {
                return AutoCompleteObjectLookupMode.DatabaseOnly;
            }

            return tableOnly ? AutoCompleteObjectLookupMode.TableOnly : AutoCompleteObjectLookupMode.TableOrView;
        }

        private static bool TryResolveSource(string sql, List<SqlToken> tokens, int triggerTokenIndex,
                                             QueryEditorAutoCompleteSpaceKeyword keyword, out SourceResolution source)
        {
            source = null;

            var triggerToken = tokens[triggerTokenIndex];
            var sourceTokenIndex = -1;

            switch (keyword)
            {
                case QueryEditorAutoCompleteSpaceKeyword.Set:
                    {
                        var updateIndex = FindPreviousWord(tokens, triggerTokenIndex - 1, triggerToken.Depth, "UPDATE");

                        if (updateIndex >= 0)
                        {
                            sourceTokenIndex = updateIndex + 1;
                        }

                        break;
                    }
                case QueryEditorAutoCompleteSpaceKeyword.SelectList:
                    {
                        var selectIndex = FindPreviousWord(tokens, triggerTokenIndex, triggerToken.Depth, "SELECT");

                        if (selectIndex < 0)
                        {
                            return false;
                        }

                        var fromIndex = FindNextWord(tokens, Math.Max(triggerTokenIndex + 1, selectIndex + 1), triggerToken.Depth, "FROM");

                        if (fromIndex < 0)
                        {
                            return false;
                        }

                        sourceTokenIndex = fromIndex + 1;
                        break;
                    }
                case QueryEditorAutoCompleteSpaceKeyword.Where:
                case QueryEditorAutoCompleteSpaceKeyword.And:
                case QueryEditorAutoCompleteSpaceKeyword.Or:
                case QueryEditorAutoCompleteSpaceKeyword.By:
                    {
                        var fromIndex = FindPreviousWord(tokens, triggerTokenIndex - 1, triggerToken.Depth, "FROM");

                        if (fromIndex >= 0)
                        {
                            sourceTokenIndex = fromIndex + 1;
                        }
                        else if (keyword == QueryEditorAutoCompleteSpaceKeyword.Where
                                 || keyword == QueryEditorAutoCompleteSpaceKeyword.And
                                 || keyword == QueryEditorAutoCompleteSpaceKeyword.Or)
                        {
                            //20260816 UPDATE ... SET ... WHERE/AND/OR does not have a FROM clause on Oracle or MySQL, and FROM is optional on PostgreSQL and SQL Server.
                            //         Fall back to the UPDATE target only when no FROM source exists.
                            var updateIndex = FindPreviousWord(tokens, triggerTokenIndex - 1, triggerToken.Depth, "UPDATE");

                            if (updateIndex >= 0)
                            {
                                sourceTokenIndex = updateIndex + 1;
                            }
                        }

                        break;
                    }
            }

            if (sourceTokenIndex < 0 || sourceTokenIndex >= tokens.Count)
            {
                return false;
            }

            return TryReadSource(sql, tokens, sourceTokenIndex, triggerToken.Depth, out source);
        }

        private static bool TryReadSource(string sql, List<SqlToken> tokens, int sourceTokenIndex, int depth, out SourceResolution source)
        {
            source = null;
            sourceTokenIndex = SkipSourceModifiers(tokens, sourceTokenIndex, depth);

            if (sourceTokenIndex < 0 || sourceTokenIndex >= tokens.Count)
            {
                return false;
            }

            var firstToken = tokens[sourceTokenIndex];

            if (firstToken.IsSymbol("("))
            {
                var closeIndex = FindMatchingCloseParenthesis(tokens, sourceTokenIndex);

                if (closeIndex < 0)
                {
                    return false;
                }

                var sourceSql = sql.Substring(firstToken.End, tokens[closeIndex].Start - firstToken.End).Trim();

                if (string.IsNullOrWhiteSpace(sourceSql) || !StartsWithSelectOrWith(sourceSql))
                {
                    return false;
                }

                source = new SourceResolution
                {
                    Kind = QueryEditorAutoCompleteSpaceSourceKind.SubquerySql,
                    ObjectName = string.Empty,
                    AliasName = ReadAlias(tokens, closeIndex + 1, depth),
                    SourceSql = sourceSql
                };

                return true;
            }

            if (!IsIdentifierToken(firstToken))
            {
                return false;
            }

            var endIndex = sourceTokenIndex;

            while (endIndex + 2 < tokens.Count && tokens[endIndex + 1].Depth == depth && tokens[endIndex + 1].IsSymbol(".")
                   && tokens[endIndex + 2].Depth == depth && IsIdentifierToken(tokens[endIndex + 2]))
            {
                endIndex += 2;
            }

            var objectName = sql.Substring(firstToken.Start, tokens[endIndex].End - firstToken.Start).Trim();
            var aliasName = ReadAlias(tokens, endIndex + 1, depth);

            if (TryResolveCteSql(sql, tokens, firstToken, objectName, out var cteSql))
            {
                source = new SourceResolution
                {
                    Kind = QueryEditorAutoCompleteSpaceSourceKind.CteSql,
                    ObjectName = objectName,
                    AliasName = aliasName,
                    SourceSql = cteSql
                };

                return true;
            }

            source = new SourceResolution
            {
                Kind = QueryEditorAutoCompleteSpaceSourceKind.ObjectName,
                ObjectName = objectName,
                AliasName = aliasName,
                SourceSql = string.Empty
            };

            return true;
        }

        private static int SkipSourceModifiers(List<SqlToken> tokens, int index, int depth)
        {
            while (index < tokens.Count && tokens[index].Depth == depth
                   && (tokens[index].IsWord("ONLY") || tokens[index].IsWord("LATERAL")))
            {
                index++;
            }

            return index;
        }

        private static string ReadAlias(List<SqlToken> tokens, int index, int depth)
        {
            if (index >= tokens.Count)
            {
                return string.Empty;
            }

            if (tokens[index].Depth == depth && tokens[index].IsWord("AS"))
            {
                index++;
            }

            if (index >= tokens.Count || tokens[index].Depth != depth || !IsIdentifierToken(tokens[index]))
            {
                return string.Empty;
            }

            if (tokens[index].Kind == SqlTokenKind.Word && ReservedAliasWords.Contains(tokens[index].Text))
            {
                return string.Empty;
            }

            return tokens[index].Text;
        }

        private static bool TryResolveCteSql(string sql, List<SqlToken> tokens, SqlToken sourceToken,
                                             string objectName, out string cteSql)
        {
            cteSql = string.Empty;

            if (objectName.IndexOf('.') >= 0)
            {
                return false;
            }

            var sourceName = NormalizeIdentifier(objectName);
            var withIndex = FindPreviousWord(tokens, tokens.IndexOf(sourceToken) - 1, sourceToken.Depth, "WITH");

            if (withIndex < 0)
            {
                return false;
            }

            var index = withIndex + 1;

            if (index < tokens.Count && tokens[index].Depth == sourceToken.Depth && tokens[index].IsWord("RECURSIVE"))
            {
                index++;
            }

            while (index < tokens.Count)
            {
                if (tokens[index].Depth != sourceToken.Depth || !IsIdentifierToken(tokens[index]))
                {
                    return false;
                }

                var cteName = NormalizeIdentifier(tokens[index].Text);

                index++;

                if (index < tokens.Count && tokens[index].Depth == sourceToken.Depth && tokens[index].IsSymbol("("))
                {
                    var columnListCloseIndex = FindMatchingCloseParenthesis(tokens, index);

                    if (columnListCloseIndex < 0)
                    {
                        return false;
                    }

                    index = columnListCloseIndex + 1;
                }

                if (index >= tokens.Count || tokens[index].Depth != sourceToken.Depth || !tokens[index].IsWord("AS"))
                {
                    return false;
                }

                index++;

                if (index >= tokens.Count || tokens[index].Depth != sourceToken.Depth || !tokens[index].IsSymbol("("))
                {
                    return false;
                }

                var openIndex = index;
                var closeIndex = FindMatchingCloseParenthesis(tokens, openIndex);

                if (closeIndex < 0)
                {
                    return false;
                }

                if (string.Equals(cteName, sourceName, StringComparison.OrdinalIgnoreCase))
                {
                    cteSql = sql.Substring(tokens[openIndex].End, tokens[closeIndex].Start - tokens[openIndex].End).Trim();
                    return !string.IsNullOrWhiteSpace(cteSql);
                }

                index = closeIndex + 1;

                if (index < tokens.Count && tokens[index].Depth == sourceToken.Depth && tokens[index].IsSymbol(","))
                {
                    index++;
                    continue;
                }

                break;
            }

            return false;
        }

        private static bool StartsWithSelectOrWith(string sql)
        {
            var value = (sql ?? string.Empty).TrimStart();

            return value.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                   || value.StartsWith("WITH", StringComparison.OrdinalIgnoreCase);
        }

        private static int FindMatchingCloseParenthesis(List<SqlToken> tokens, int openIndex)
        {
            if (openIndex < 0 || openIndex >= tokens.Count || !tokens[openIndex].IsSymbol("("))
            {
                return -1;
            }

            var depth = tokens[openIndex].Depth;

            for (var i = openIndex + 1; i < tokens.Count; i++)
            {
                if (tokens[i].IsSymbol(")") && tokens[i].Depth == depth)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindPreviousWord(List<SqlToken> tokens, int startIndex, int depth, string word)
        {
            for (var i = Math.Min(startIndex, tokens.Count - 1); i >= 0; i--)
            {
                if (tokens[i].Depth == depth && tokens[i].IsSymbol(";"))
                {
                    return -1;
                }

                if (tokens[i].Depth == depth && tokens[i].IsWord(word))
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindNextWord(List<SqlToken> tokens, int startIndex, int depth, string word)
        {
            for (var i = Math.Max(0, startIndex); i < tokens.Count; i++)
            {
                if (tokens[i].Depth < depth || (tokens[i].Depth == depth && tokens[i].IsSymbol(";")))
                {
                    return -1;
                }

                if (tokens[i].Depth == depth && tokens[i].IsWord(word))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsIdentifierToken(SqlToken token)
        {
            return token != null && (token.Kind == SqlTokenKind.Word || token.Kind == SqlTokenKind.DelimitedIdentifier);
        }

        private static string NormalizeIdentifier(string value)
        {
            var text = (value ?? string.Empty).Trim();

            if (text.Length >= 2 && text[0] == '"' && text[text.Length - 1] == '"')
            {
                return text.Substring(1, text.Length - 2).Replace("\"\"", "\"");
            }

            if (text.Length >= 2 && text[0] == '[' && text[text.Length - 1] == ']')
            {
                return text.Substring(1, text.Length - 2).Replace("]]", "]");
            }

            if (text.Length >= 2 && text[0] == '`' && text[text.Length - 1] == '`')
            {
                return text.Substring(1, text.Length - 2).Replace("``", "`");
            }

            return text;
        }

        private static bool IsInsideProtectedRange(List<Tuple<int, int>> ranges, int caretPosition)
        {
            foreach (var range in ranges)
            {
                if (caretPosition > range.Item1 && caretPosition < range.Item2)
                {
                    return true;
                }
            }

            return false;
        }

        private static TokenizeResult Tokenize(string sql, DataSourceType dataSourceType)
        {
            var result = new TokenizeResult();
            var depth = 0;
            var index = 0;

            while (index < sql.Length)
            {
                var ch = sql[index];

                if (char.IsWhiteSpace(ch))
                {
                    index++;
                    continue;
                }

                if (IsLineCommentStart(sql, index, dataSourceType))
                {
                    var start = index;

                    index += 2;

                    while (index < sql.Length && sql[index] != '\r' && sql[index] != '\n')
                    {
                        index++;
                    }

                    result.ProtectedRanges.Add(Tuple.Create(start, index));
                    continue;
                }

                if (ch == '#' && dataSourceType == DataSourceType.MySql)
                {
                    var start = index;

                    index++;

                    while (index < sql.Length && sql[index] != '\r' && sql[index] != '\n')
                    {
                        index++;
                    }

                    result.ProtectedRanges.Add(Tuple.Create(start, index));
                    continue;
                }

                if (ch == '/' && index + 1 < sql.Length && sql[index + 1] == '*')
                {
                    var start = index;

                    index += 2;

                    while (index + 1 < sql.Length && !(sql[index] == '*' && sql[index + 1] == '/'))
                    {
                        index++;
                    }

                    index = index + 1 < sql.Length ? index + 2 : sql.Length;
                    result.ProtectedRanges.Add(Tuple.Create(start, index));
                    continue;
                }

                if (ch == '\'')
                {
                    var start = index;

                    index++;

                    while (index < sql.Length)
                    {
                        if (sql[index] == '\'' && index + 1 < sql.Length && sql[index + 1] == '\'')
                        {
                            index += 2;
                            continue;
                        }

                        if (sql[index] == '\'')
                        {
                            index++;
                            break;
                        }

                        index++;
                    }

                    result.Tokens.Add(new SqlToken { Kind = SqlTokenKind.StringLiteral, Text = sql.Substring(start, index - start), Start = start, End = index, Depth = depth });
                    result.ProtectedRanges.Add(Tuple.Create(start, index));
                    continue;
                }

                if (ch == '"' || ch == '[' || ch == '`')
                {
                    var start = index;
                    var closeChar = ch == '[' ? ']' : ch;

                    index++;

                    while (index < sql.Length)
                    {
                        if (sql[index] == closeChar)
                        {
                            if (index + 1 < sql.Length && sql[index + 1] == closeChar)
                            {
                                index += 2;
                                continue;
                            }

                            index++;
                            break;
                        }

                        index++;
                    }

                    result.Tokens.Add(new SqlToken { Kind = SqlTokenKind.DelimitedIdentifier, Text = sql.Substring(start, index - start), Start = start, End = index, Depth = depth });
                    continue;
                }

                if (IsWordCharacter(ch))
                {
                    var start = index;

                    index++;

                    while (index < sql.Length && IsWordCharacter(sql[index]))
                    {
                        index++;
                    }

                    result.Tokens.Add(new SqlToken { Kind = SqlTokenKind.Word, Text = sql.Substring(start, index - start), Start = start, End = index, Depth = depth });
                    continue;
                }

                if (ch == '(')
                {
                    result.Tokens.Add(new SqlToken { Kind = SqlTokenKind.Symbol, Text = "(", Start = index, End = index + 1, Depth = depth });
                    depth++;
                    index++;
                    continue;
                }

                if (ch == ')')
                {
                    depth = Math.Max(0, depth - 1);
                    result.Tokens.Add(new SqlToken { Kind = SqlTokenKind.Symbol, Text = ")", Start = index, End = index + 1, Depth = depth });
                    index++;
                    continue;
                }

                result.Tokens.Add(new SqlToken { Kind = SqlTokenKind.Symbol, Text = ch.ToString(), Start = index, End = index + 1, Depth = depth });
                index++;
            }

            return result;
        }

        private static bool IsLineCommentStart(string sql, int index, DataSourceType dataSourceType)
        {
            if (string.IsNullOrEmpty(sql) || index < 0 || index + 1 >= sql.Length || sql[index] != '-' || sql[index + 1] != '-')
            {
                return false;
            }

            if (dataSourceType != DataSourceType.MySql)
            {
                return true;
            }

            if (index + 2 >= sql.Length)
            {
                return true;
            }

            return char.IsWhiteSpace(sql[index + 2]) || char.IsControl(sql[index + 2]);
        }

        private static bool IsWordCharacter(char ch)
        {
            return char.IsLetterOrDigit(ch) || ch == '_' || ch == '$' || ch == '#';
        }
    }
}