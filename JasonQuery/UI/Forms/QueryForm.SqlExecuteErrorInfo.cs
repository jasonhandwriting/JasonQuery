using JasonQuery.Core.Config;
using System;

//負責 SQLExecuteErrorPos payload 解析

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private sealed class SqlExecuteErrorInfo
        {
            public string ExecutedResult { get; set; } = string.Empty;
            public string ErrorCode { get; set; } = string.Empty;
            public string ErrorMessage { get; set; } = string.Empty;
            public string ErrorHint { get; set; } = string.Empty;

            public string PositionText { get; set; } = "0";
            public int Position { get; set; }

            public string ExecutedSql { get; set; } = string.Empty;

            public string SecondaryErrorMessage { get; set; } = string.Empty; //SQL Server 可能會多帶一個參數

            public bool HasExecutedSql => !string.IsNullOrEmpty(ExecutedSql);
            public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
        }

        private SqlExecuteErrorInfo ParseSqlExecuteErrorInfo(string sqlInfo)
        {
            var parts = (sqlInfo ?? string.Empty).Split(new[] { MyGlobal.Separator }, StringSplitOptions.None);

            var info = new SqlExecuteErrorInfo
            {
                ExecutedResult = GetSqlExecuteErrorPart(parts, 0),
                ErrorCode = GetSqlExecuteErrorPart(parts, 1),
                ErrorMessage = GetSqlExecuteErrorPart(parts, 2),
                ErrorHint = GetSqlExecuteErrorPart(parts, 3),
                PositionText = GetSqlExecuteErrorPart(parts, 4),
                ExecutedSql = GetSqlExecuteErrorPart(parts, 5),
                SecondaryErrorMessage = GetSqlExecuteErrorPart(parts, 6)
            };

            if (!int.TryParse(info.PositionText, out var position))
            {
                position = 0;
            }

            info.Position = position;

            return info;
        }

        private string GetSqlExecuteErrorPart(string[] parts, int index)
        {
            if (parts == null || index < 0 || index >= parts.Length)
            {
                return string.Empty;
            }

            return parts[index] ?? string.Empty;
        }
    }
}