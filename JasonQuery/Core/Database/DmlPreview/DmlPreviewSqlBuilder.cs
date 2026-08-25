using JasonQuery.Core.Database.Connection;
using System;

namespace JasonQuery.Core.Database.DmlPreview
{
    internal static class DmlPreviewSqlBuilder
    {
        public static string QuoteIdentifier(DataSourceType dataSourceType,string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return string.Empty;
            }

            identifier = identifier.Trim();

            switch (dataSourceType)
            {
                case DataSourceType.SqlServer:
                    {
                        if (identifier.StartsWith("[", StringComparison.Ordinal) && identifier.EndsWith("]", StringComparison.Ordinal))
                        {
                            return identifier;
                        }

                        return $"[{identifier.Replace("]", "]]")}]";
                    }
                case DataSourceType.MySql:
                    {
                        if (identifier.StartsWith("`", StringComparison.Ordinal) && identifier.EndsWith("`", StringComparison.Ordinal))
                        {
                            return identifier;
                        }

                        return $"`{identifier.Replace("`", "``")}`";
                    }
                default:
                    {
                        return identifier;
                    }
            }
        }

        public static string BuildTableName(DataSourceType dataSourceType, string qualifierName, string tableName)
        {
            var quotedTableName = QuoteIdentifier(dataSourceType, tableName);

            if (string.IsNullOrEmpty(quotedTableName))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(qualifierName))
            {
                return quotedTableName;
            }

            var quotedQualifierName = QuoteIdentifier(dataSourceType, qualifierName);

            if (string.IsNullOrEmpty(quotedQualifierName))
            {
                return quotedTableName;
            }

            return $"{quotedQualifierName}.{quotedTableName}";
        }

        public static string BuildInsert(string tableName, string fieldList, string valueList, bool useUpperCaseKeywords)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(fieldList) || string.IsNullOrWhiteSpace(valueList))
            {
                return string.Empty;
            }

            var insertKeyword = useUpperCaseKeywords ? "INSERT INTO " : "insert into ";
            var valuesKeyword = useUpperCaseKeywords ? "VALUES " : "values ";

            return $"{insertKeyword}{tableName}\r\n"
                   + $"       ({fieldList.Trim()})\r\n"
                   + $"{valuesKeyword}({valueList.Trim()});";
        }

        public static string BuildUpdate(string tableName, string setClause, string whereCondition, bool useUpperCaseKeywords)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(setClause) || string.IsNullOrWhiteSpace(whereCondition))
            {
                return string.Empty;
            }

            var updateKeyword = useUpperCaseKeywords ? "UPDATE " : "update ";
            var setKeyword = useUpperCaseKeywords ? "   SET " : "   set ";
            var whereKeyword = useUpperCaseKeywords ? " WHERE " : " where ";

            return $"{updateKeyword}{tableName}\r\n"
                   + $"{setKeyword}{setClause.Trim()}\r\n"
                   + $"{whereKeyword}{whereCondition.Trim()};";
        }

        public static string BuildDelete(string tableName, string whereCondition, bool useUpperCaseKeywords)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(whereCondition))
            {
                return string.Empty;
            }

            var deleteKeyword = useUpperCaseKeywords ? "DELETE FROM " : "delete from ";
            var whereKeyword = useUpperCaseKeywords ? " WHERE " : " where ";

            return $"{deleteKeyword}{tableName}\r\n"
                   + $"{whereKeyword}{whereCondition.Trim()};";
        }
    }
}
