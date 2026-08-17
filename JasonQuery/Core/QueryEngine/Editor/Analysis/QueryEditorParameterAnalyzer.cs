using System;
using System.Collections.Generic;
using System.Text;

namespace JasonQuery.Core.QueryEngine.Editor.Analysis
{
    internal static class QueryEditorParameterAnalyzer
    {
        public static string ExtractParametersInfo(string sql, out string resultAll)
        {
            var sbTemp = new StringBuilder();
            var resultDistinct = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var sbResultAll = new StringBuilder();
            var parameterStart = false;
            var doubleQuotation = false;
            var singleQuotation = false;
            var singleComment = false;
            var paragraphComment = false;
            var array = (sql ?? string.Empty).ToCharArray();
            var validPrevChars = new HashSet<char> { '=', ' ', '(', ')', '<', '>', '\n' };
            var delimiters = new HashSet<char> { ' ', '\r', '-', '/', ';', ',', ')' };

            for (var i = 0; i < array.Length; i++)
            {
                char letter = array[i];

                if (letter == '\"' || letter == '\'')
                {
                    switch (letter)
                    {
                        case '\"' when doubleQuotation:
                            {
                                doubleQuotation = false;
                                break;
                            }
                        case '\"':
                            {
                                if (!singleQuotation)
                                {
                                    doubleQuotation = true;
                                }

                                break;
                            }
                        case '\'' when singleQuotation:
                            {
                                singleQuotation = false;
                                break;
                            }
                        case '\'':
                            {
                                if (!doubleQuotation)
                                {
                                    singleQuotation = true;
                                }

                                break;
                            }
                    }
                }

                if (!singleQuotation && !doubleQuotation && i >= 1 && letter == '*' && array[i - 1] == '/')
                {
                    if (!paragraphComment)
                    {
                        paragraphComment = true;
                    }
                }

                if (!singleQuotation && !doubleQuotation && letter == '*' && i + 1 <= array.GetUpperBound(0) && array[i + 1] == '/')
                {
                    if (paragraphComment)
                    {
                        paragraphComment = false;
                    }
                }

                if (!singleQuotation && !doubleQuotation && i >= 1 && letter == '-' && array[i - 1] == '-')
                {
                    if (!singleComment)
                    {
                        singleComment = true;
                    }
                }

                if (singleComment && (letter == '\r' || letter == '\n'))
                {
                    singleComment = false;
                }

                if (paragraphComment && letter == '*' && i + 1 < array.Length && array[i + 1] == '/')
                {
                    paragraphComment = false;
                }

                if (doubleQuotation || singleQuotation || singleComment || paragraphComment)
                {
                    continue;
                }

                if (letter == ':' && i >= 1 && validPrevChars.Contains(array[i - 1]) && i + 1 < array.Length && array[i + 1] != ':')
                {
                    parameterStart = true;
                }

                if (!parameterStart)
                {
                    continue;
                }

                bool endOfToken = (i + 1 >= array.Length) || delimiters.Contains(array[i + 1]);

                if (!endOfToken)
                {
                    sbTemp.Append(letter);
                }
                else
                {
                    sbTemp.Append(letter);

                    var token = sbTemp.ToString();

                    resultDistinct.Add(token);
                    sbResultAll.Append(token)
                               .Append("|")
                               .Append(i - token.Length + 1)
                               .Append("`");

                    sbTemp.Clear();
                    parameterStart = false;
                }
            }

            resultAll = "`" + sbResultAll;
            return string.Join("`", resultDistinct);
        }
    }
}