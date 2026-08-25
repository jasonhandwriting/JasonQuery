using JasonQuery.Core.Text;
using System;
using System.Text;

//負責產生 Message Tab 裡的 SQL 行與 ^ 指示線

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private sealed class SqlErrorPointerContext
        {
            public string Sql { get; set; } = string.Empty;
            public int Position { get; set; }
            public int PositionOffset { get; set; }
            public string TargetWord { get; set; } = string.Empty;

            /// <summary>
            /// PostgreSQL 22P02 這類錯誤，前後單引號也要一起標示時使用。
            /// </summary>
            public int SpecialLength { get; set; }

            /// <summary>
            /// MySQL 避免 ^ 超出當行長度的保護邏輯。
            /// </summary>
            public bool ClampWordLengthToLine { get; set; }
        }

        private string BuildSqlErrorPointer(SqlErrorPointerContext context)
        {
            if (context == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(context.Sql) || string.IsNullOrEmpty(context.TargetWord))
            {
                return string.Empty;
            }

            if (context.Sql.Length < 2 || context.Position <= 0)
            {
                return string.Empty;
            }

            var safePosition = NormalizeSqlErrorPointerPosition(context.Sql, context.Position);
            var from = FindSqlErrorPointerLineStart(context.Sql, safePosition);
            var to = FindSqlErrorPointerLineEnd(context.Sql, safePosition);

            if (to <= from)
            {
                return string.Empty;
            }

            var pointerOffset = safePosition - from + context.PositionOffset;

            if (pointerOffset <= 0)
            {
                return string.Empty;
            }

            var lineText = TextHelper.GetSafeSubstring(context.Sql, from, to - from);
            var fullLineByteCount = Encoding.Default.GetByteCount(lineText);

            if (lineText.Length == fullLineByteCount)
            {
                var space = CreateSqlErrorPointerSpace(pointerOffset);
                var wordLength = context.TargetWord.Length + context.SpecialLength;

                if (context.ClampWordLengthToLine)
                {
                    wordLength = Math.Min(wordLength, Math.Max(0, lineText.Length - space.Length));
                }

                var word = CreateSqlErrorPointerWord(wordLength);

                return $"\r\n\r\n{lineText}\r\n{space}{word}";
            }

            var prefixText = TextHelper.GetSafeSubstring(context.Sql, from, pointerOffset);
            var prefixByteCount = Encoding.Default.GetByteCount(prefixText);
            var targetWordByteCount = Encoding.Default.GetByteCount(context.TargetWord) + context.SpecialLength;

            if (prefixText.Length == prefixByteCount)
            {
                var space = CreateSqlErrorPointerSpace(pointerOffset);
                var word = CreateSqlErrorPointerWord(targetWordByteCount);

                return $"\r\n\r\n{lineText}\r\n{space}{word}";
            }
            else
            {
                var space = CreateSqlErrorPointerSpace(pointerOffset + prefixByteCount - prefixText.Length);
                var word = CreateSqlErrorPointerWord(targetWordByteCount);

                return $"\r\n\r\n{lineText}\r\n{space}{word}";
            }
        }

        private int NormalizeSqlErrorPointerPosition(string sql, int position)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return 0;
            }

            if (position < 0)
            {
                return 0;
            }

            if (position >= sql.Length)
            {
                return sql.Length - 1;
            }

            return position;
        }

        private int FindSqlErrorPointerLineStart(string sql, int position)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return 0;
            }

            var start = NormalizeSqlErrorPointerPosition(sql, position);

            for (var i = start; i > 0; i--)
            {
                if (sql[i] == '\n')
                {
                    return i + 1;
                }
            }

            return 0;
        }

        private int FindSqlErrorPointerLineEnd(string sql, int position)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return 0;
            }

            var start = NormalizeSqlErrorPointerPosition(sql, position);

            for (var i = start; i < sql.Length; i++)
            {
                if (sql[i] == '\r')
                {
                    return i;
                }
            }

            return sql.Length;
        }

        private string CreateSqlErrorPointerSpace(int count)
        {
            return count <= 0 ? string.Empty : new string(' ', count);
        }

        private string CreateSqlErrorPointerWord(int count)
        {
            return count <= 0 ? string.Empty : new string('^', count);
        }
    }
}
