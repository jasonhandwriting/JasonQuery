namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers
{
    internal static class QueryEditorAutoCompleteWithAsResolver
    {
        /// <summary>
        /// 搜尋 With xxx AS 子查詢
        /// 第一組才會帶 With，所以，搜尋時要以 xxx AS 為主
        /// </summary>
        /// <param name="sql">傳入要搜尋的 SQL</param>
        /// <param name="start">從哪一個位置開始往後搜尋</param>
        /// <returns></returns>
        public static string GetAutoCompleteSqlForWithAs(string sql, int start)
        {
            var end = 0;
            var doubleQuotation = false; //是否為雙引號起始？
            var singleQuotation = false; //是否為單引號起始？
            var singleComment = false; //是否為單列註解？
            var paragraphComment = false; //是否為段落註解？
            var roundBrackets1 = false; //是否為第 1 組 ( ？
            var roundBrackets2 = false; //是否為第 2 組 ( ？
            var roundBrackets3 = false; //是否為第 3 組 ( ？
            var roundBrackets4 = false; //是否為第 4 組 ( ？
            var roundBrackets5 = false; //是否為第 5 組 ( ？
            var roundBrackets6 = false; //是否為第 6 組 ( ？
            var roundBrackets7 = false; //是否為第 7 組 ( ？
            var roundBrackets8 = false; //是否為第 8 組 ( ？
            var roundBrackets9 = false; //是否為第 9 組 ( ？
            var array = ($"{sql} ").ToCharArray();

            for (var i = start; i < array.Length; i++)
            {
                var letter = array[i];

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
                else if (letter == '(')
                {
                    if (singleQuotation || doubleQuotation)
                    {
                        //括號落在單引號或雙引號之間，忽略！
                    }
                    else if (roundBrackets8)
                    {
                        //已偵測到第 8 組括號
                        roundBrackets9 = true;
                    }
                    else if (roundBrackets7)
                    {
                        //已偵測到第 7 組括號
                        roundBrackets8 = true;
                    }
                    else if (roundBrackets6)
                    {
                        //已偵測到第 6 組括號
                        roundBrackets7 = true;
                    }
                    else if (roundBrackets5)
                    {
                        //已偵測到第 5 組括號
                        roundBrackets6 = true;
                    }
                    else if (roundBrackets4)
                    {
                        //已偵測到第 4 組括號
                        roundBrackets5 = true;
                    }
                    else if (roundBrackets3)
                    {
                        //已偵測到第 3 組括號
                        roundBrackets4 = true;
                    }
                    else if (roundBrackets2)
                    {
                        //已偵測到第 2 組括號
                        roundBrackets3 = true;
                    }
                    else if (roundBrackets1)
                    {
                        //已偵測到第 1 組括號
                        roundBrackets2 = true;
                    }
                    else
                    {
                        roundBrackets1 = true;
                    }
                }
                else if (letter == ')')
                {
                    if (singleQuotation || doubleQuotation)
                    {
                        //括號落在單引號或雙引號之間，忽略！
                    }
                    else if (roundBrackets9)
                    {
                        roundBrackets9 = false;
                    }
                    else if (roundBrackets8)
                    {
                        roundBrackets8 = false;
                    }
                    else if (roundBrackets7)
                    {
                        roundBrackets7 = false;
                    }
                    else if (roundBrackets6)
                    {
                        roundBrackets6 = false;
                    }
                    else if (roundBrackets5)
                    {
                        roundBrackets5 = false;
                    }
                    else if (roundBrackets4)
                    {
                        roundBrackets4 = false;
                    }
                    else if (roundBrackets3)
                    {
                        roundBrackets3 = false;
                    }
                    else if (roundBrackets2)
                    {
                        roundBrackets2 = false;
                    }
                    else
                    {
                        roundBrackets1 = false;
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

                if (letter == ')' && !doubleQuotation && !singleQuotation && !singleComment && !paragraphComment && !roundBrackets1 && !roundBrackets2 && !roundBrackets3 && !roundBrackets4 && !roundBrackets5 && !roundBrackets6 && !roundBrackets7 && !roundBrackets8 && !roundBrackets9)
                {
                    end = i;
                    break;
                }
            }

            var result = string.Empty;

            if (end > 0)
            {
                if (end - start > 0)
                {
                    result = sql.Substring(start + 1, end - start - 1).Trim();
                }
            }

            return result.Trim();
        }
    }
}