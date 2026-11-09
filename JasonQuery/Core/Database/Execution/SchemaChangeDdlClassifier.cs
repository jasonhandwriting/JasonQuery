using System;

namespace JasonQuery.Core.Database.Execution
{
    //F2-C2: only table/view structural DDL displayed by the current shared F7.
    //This is a lightweight first-keyword classifier, not a general SQL parser.
    internal static class SchemaChangeDdlClassifier
    {
        internal static bool IsStructuralDdl(string statementSql)
        {
            if (string.IsNullOrWhiteSpace(statementSql))
            {
                return false;
            }

            var position = 0;
            var action = NextKeyword(statementSql, ref position);
            var objectType = NextKeyword(statementSql, ref position);

            if (string.Equals(action, "CREATE", StringComparison.Ordinal))
            {
                if (string.Equals(objectType, "OR", StringComparison.Ordinal))
                {
                    if (!string.Equals(NextKeyword(statementSql, ref position), "REPLACE", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    objectType = NextKeyword(statementSql, ref position);
                }

                if (string.Equals(objectType, "TEMP", StringComparison.Ordinal) || string.Equals(objectType, "TEMPORARY", StringComparison.Ordinal) || string.Equals(objectType, "GLOBAL", StringComparison.Ordinal) || string.Equals(objectType, "LOCAL", StringComparison.Ordinal))
                {
                    objectType = NextKeyword(statementSql, ref position);
                }

                if (string.Equals(objectType, "MATERIALIZED", StringComparison.Ordinal))
                {
                    objectType = NextKeyword(statementSql, ref position);
                }
            }

            if (string.Equals(action, "RENAME", StringComparison.Ordinal))
            {
                //Oracle: RENAME old_name TO new_name; MySQL: RENAME TABLE ...
                return string.Equals(objectType, "TABLE", StringComparison.Ordinal) || !string.IsNullOrEmpty(objectType);
            }

            if (!string.Equals(action, "CREATE", StringComparison.Ordinal) && !string.Equals(action, "ALTER", StringComparison.Ordinal) && !string.Equals(action, "DROP", StringComparison.Ordinal) && !string.Equals(action, "TRUNCATE", StringComparison.Ordinal))
            {
                return false;
            }

            return string.Equals(objectType, "TABLE", StringComparison.Ordinal) || string.Equals(objectType, "VIEW", StringComparison.Ordinal);
        }

        private static string NextKeyword(string sql, ref int position)
        {
            while (position < sql.Length)
            {
                if (char.IsWhiteSpace(sql[position]))
                {
                    position++;
                    continue;
                }

                if (position + 1 < sql.Length && sql[position] == '-' && sql[position + 1] == '-')
                {
                    position += 2;

                    while (position < sql.Length && sql[position] != '\r' && sql[position] != '\n')
                    {
                        position++;
                    }

                    continue;
                }

                if (position + 1 < sql.Length && sql[position] == '/' && sql[position + 1] == '*')
                {
                    position += 2;

                    var close = sql.IndexOf("*/", position, StringComparison.Ordinal);

                    if (close < 0)
                    {
                        return string.Empty;
                    }

                    position = close + 2;
                    continue;
                }

                if (sql[position] == '#')
                {
                    while (position < sql.Length && sql[position] != '\r' && sql[position] != '\n')
                    {
                        position++;
                    }

                    continue;
                }

                break;
            }

            var start = position;

            while (position < sql.Length && (char.IsLetter(sql[position]) || sql[position] == '_'))
            {
                position++;
            }

            if (position == start)
            {
                return string.Empty;
            }

            return sql.Substring(start, position - start).ToUpperInvariant();
        }
    }
}
