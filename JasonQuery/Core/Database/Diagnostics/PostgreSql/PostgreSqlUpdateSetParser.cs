namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public static class PostgreSqlUpdateSetParser
    {
        public static bool TryParse(string sql, out PostgreSqlUpdateSetParseResult result)
        {
            result = PostgreSqlUpdateSetParseResult.Fail("SQL is empty.");

            if (string.IsNullOrWhiteSpace(sql))
            {
                return false;
            }

            var text = PostgreSqlSqlScanner.NormalizeSql(sql);
            var index = 0;

            PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

            if (!PostgreSqlSqlScanner.ReadKeyword(text, ref index, "update"))
            {
                result = PostgreSqlUpdateSetParseResult.Fail("目前只支援 UPDATE ... SET ... 語法。");
                return false;
            }

            PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

            //UPDATE ONLY table ...
            if (PostgreSqlSqlScanner.ReadKeyword(text, ref index, "only"))
            {
                PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);
            }

            if (!PostgreSqlSqlScanner.TryReadQualifiedTableName(text, ref index, out var schemaName, out var tableName))
            {
                result = PostgreSqlUpdateSetParseResult.Fail("無法解析 UPDATE 後方的 Table 名稱。");
                return false;
            }

            PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

            //UPDATE table * SET ...
            if (index < text.Length && text[index] == '*')
            {
                index++;
                PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);
            }

            var setKeywordIndex = PostgreSqlSqlScanner.FindKeywordAtTopLevel(text, "set", index);

            if (setKeywordIndex < 0)
            {
                result = PostgreSqlUpdateSetParseResult.Fail("找不到 UPDATE SET 關鍵字。");
                return false;
            }

            var setContentStart = setKeywordIndex + 3;
            var setContentEnd = FindSetContentEnd(text, setContentStart);

            if (setContentEnd <= setContentStart)
            {
                result = PostgreSqlUpdateSetParseResult.Fail("無法解析 SET 區塊內容。");
                return false;
            }

            var setContent = text.Substring(setContentStart, setContentEnd - setContentStart).Trim();

            if (string.IsNullOrWhiteSpace(setContent))
            {
                result = PostgreSqlUpdateSetParseResult.Fail("SET 區塊為空。");
                return false;
            }

            result = new PostgreSqlUpdateSetParseResult
            {
                Success = true,
                SchemaName = string.IsNullOrEmpty(schemaName) ? "public" : schemaName,
                TableName = tableName
            };

            var assignments = PostgreSqlSqlScanner.SplitTopLevelComma(setContent);

            foreach (var assignment in assignments)
            {
                if (!TryParseAssignment(assignment, out var valueInfo))
                {
                    continue;
                }

                result.SetValues.Add(valueInfo);
            }

            if (result.SetValues.Count == 0)
            {
                result = PostgreSqlUpdateSetParseResult.Fail("無法解析 SET 欄位指定內容。");
                return false;
            }

            return true;
        }

        private static bool TryParseAssignment(string assignment, out PostgreSqlUpdateSetValueInfo valueInfo)
        {
            valueInfo = null;

            if (string.IsNullOrWhiteSpace(assignment))
            {
                return false;
            }

            var equalIndex = PostgreSqlSqlScanner.FindTopLevelEqualSign(assignment);

            if (equalIndex <= 0)
            {
                return false;
            }

            var left = assignment.Substring(0, equalIndex).Trim();
            var right = assignment.Substring(equalIndex + 1).Trim();

            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            var leftIndex = 0;

            if (!PostgreSqlSqlScanner.TryReadQualifiedColumnName(left, ref leftIndex, out var columnName))
            {
                return false;
            }

            PostgreSqlSqlScanner.SkipWhiteSpace(left, ref leftIndex);

            if (leftIndex != left.Length)
            {
                return false;
            }

            valueInfo = new PostgreSqlUpdateSetValueInfo
            {
                ColumnName = columnName,
                RawText = right
            };

            if (PostgreSqlSqlScanner.TryParseSingleQuotedStringLiteral(right, out var stringValue))
            {
                valueInfo.IsStringLiteral = true;
                valueInfo.StringValue = stringValue;
            }

            return true;
        }

        private static int FindSetContentEnd(string text, int startIndex)
        {
            var keywordIndex = text.Length;
            var fromIndex = PostgreSqlSqlScanner.FindKeywordAtTopLevel(text, "from", startIndex);
            var whereIndex = PostgreSqlSqlScanner.FindKeywordAtTopLevel(text, "where", startIndex);
            var returningIndex = PostgreSqlSqlScanner.FindKeywordAtTopLevel(text, "returning", startIndex);

            if (fromIndex >= 0 && fromIndex < keywordIndex)
            {
                keywordIndex = fromIndex;
            }

            if (whereIndex >= 0 && whereIndex < keywordIndex)
            {
                keywordIndex = whereIndex;
            }

            if (returningIndex >= 0 && returningIndex < keywordIndex)
            {
                keywordIndex = returningIndex;
            }

            return keywordIndex;
        }
    }
}
