using System.Collections.Generic;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public static class PostgreSqlInsertValuesParser
    {
        public static bool TryParse(string sql, out PostgreSqlInsertValuesParseResult result)
        {
            result = PostgreSqlInsertValuesParseResult.Fail("SQL is empty.");

            if (string.IsNullOrWhiteSpace(sql))
            {
                return false;
            }

            var text = PostgreSqlSqlScanner.NormalizeSql(sql);
            var index = 0;

            PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

            if (!PostgreSqlSqlScanner.ReadKeyword(text, ref index, "insert"))
            {
                result = PostgreSqlInsertValuesParseResult.Fail("目前只支援 INSERT INTO ... VALUES ... 語法。");
                return false;
            }

            PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

            if (!PostgreSqlSqlScanner.ReadKeyword(text, ref index, "into"))
            {
                result = PostgreSqlInsertValuesParseResult.Fail("找不到 INSERT INTO 關鍵字。");
                return false;
            }

            PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

            if (!PostgreSqlSqlScanner.TryReadQualifiedTableName(text, ref index, out var schemaName, out var tableName))
            {
                result = PostgreSqlInsertValuesParseResult.Fail("無法解析 INSERT INTO 後方的 Table 名稱。");
                return false;
            }

            PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

            if (!PostgreSqlSqlScanner.TryReadParenthesizedContent(text, ref index, out var columnListText))
            {
                result = PostgreSqlInsertValuesParseResult.Fail("目前只支援有明確 column list 的 INSERT 語法。");
                return false;
            }

            var columnNames = ParseColumnNames(columnListText);

            if (columnNames.Count == 0)
            {
                result = PostgreSqlInsertValuesParseResult.Fail("無法解析 INSERT column list。");
                return false;
            }

            PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

            if (!PostgreSqlSqlScanner.ReadKeyword(text, ref index, "values"))
            {
                result = PostgreSqlInsertValuesParseResult.Fail("目前只支援 INSERT INTO ... VALUES (...) 語法。");
                return false;
            }

            result = new PostgreSqlInsertValuesParseResult
            {
                Success = true,
                SchemaName = string.IsNullOrEmpty(schemaName) ? "public" : schemaName,
                TableName = tableName
            };

            foreach (var columnName in columnNames)
            {
                result.ColumnNames.Add(columnName);
            }

            if (!TryReadValuesRows(text, ref index, columnNames.Count, result, out var failureReason))
            {
                result = PostgreSqlInsertValuesParseResult.Fail(failureReason);
                return false;
            }

            PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

            if (index < text.Length)
            {
                var tailIndex = index;

                if (!PostgreSqlSqlScanner.ReadKeyword(text, ref tailIndex, "returning"))
                {
                    result = PostgreSqlInsertValuesParseResult.Fail("VALUES 後方出現目前不支援的語法。");
                    return false;
                }
            }

            return true;
        }

        //保留這個方法是為了相容 Step 2 之後可能已經呼叫 TryParseSingleRow 的地方
        //Step 4 開始，實際上也支援多筆 VALUES
        public static bool TryParseSingleRow(string sql, out PostgreSqlInsertValuesParseResult result)
        {
            return TryParse(sql, out result);
        }

        private static bool TryReadValuesRows(
            string text,
            ref int index,
            int columnCount,
            PostgreSqlInsertValuesParseResult result,
            out string failureReason)
        {
            failureReason = string.Empty;

            var rowIndex = 1;

            while (true)
            {
                PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

                if (!PostgreSqlSqlScanner.TryReadParenthesizedContent(text, ref index, out var valuesListText))
                {
                    failureReason = rowIndex == 1 ? "無法解析 VALUES (...) 內容。" : $"無法解析第 {rowIndex} 組 VALUES (...) 內容。";

                    return false;
                }

                var valueTexts = PostgreSqlSqlScanner.SplitTopLevelComma(valuesListText);

                if (valueTexts.Count != columnCount)
                {
                    failureReason = $"第 {rowIndex} 組 VALUES 數量與 INSERT column list 數量不一致。";
                    return false;
                }

                for (var i = 0; i < valueTexts.Count; i++)
                {
                    var rawValue = valueTexts[i].Trim();

                    var valueInfo = new PostgreSqlInsertValueInfo
                    {
                        ValuesRowIndex = rowIndex,
                        ColumnIndex = i,
                        RawText = rawValue
                    };

                    if (PostgreSqlSqlScanner.TryParseSingleQuotedStringLiteral(rawValue, out var stringValue))
                    {
                        valueInfo.IsStringLiteral = true;
                        valueInfo.StringValue = stringValue;
                    }

                    result.Values.Add(valueInfo);
                }

                PostgreSqlSqlScanner.SkipWhiteSpace(text, ref index);

                if (index >= text.Length || text[index] != ',')
                {
                    break;
                }

                index++;
                rowIndex++;
            }

            return true;
        }

        private static List<string> ParseColumnNames(string columnListText)
        {
            var result = new List<string>();
            var items = PostgreSqlSqlScanner.SplitTopLevelComma(columnListText);

            foreach (var item in items)
            {
                var temp = item.Trim();
                var index = 0;

                if (!PostgreSqlSqlScanner.TryReadQualifiedColumnName(temp, ref index, out var columnName))
                {
                    continue;
                }

                PostgreSqlSqlScanner.SkipWhiteSpace(temp, ref index);

                if (index == temp.Length)
                {
                    result.Add(columnName);
                }
            }

            return result;
        }
    }
}