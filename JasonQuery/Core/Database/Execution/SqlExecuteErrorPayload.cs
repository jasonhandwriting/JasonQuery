using JasonQuery.Core.Config;

namespace JasonQuery.Core.Database.Execution
{
    internal sealed class SqlExecuteErrorPayload
    {
        public string AccessibleDescription { get; set; } = string.Empty; //QueryForm 的定位資訊

        /// <summary>
        /// ExecuteNonQuery 多筆或單筆執行結果。
        /// Query / Paged Query 通常為空。
        /// </summary>
        public string ExecutedResult { get; set; } = string.Empty;

        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorHint { get; set; } = string.Empty;
        public int Position { get; set; }
        public string ExecutedSql { get; set; } = string.Empty;

        /// <summary>
        /// SQL Server 目前會多帶第二組錯誤訊息，供 QueryForm 進一步定位。
        /// </summary>
        public string SecondaryErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// SQL Server 原本即使第二組錯誤訊息為空，也會保留最後一段 Separator5。
        /// 為了完全保留原 payload 格式，SQL Server 需要設為 true。
        /// </summary>
        public bool IncludeSecondaryErrorMessage { get; set; }
    }

    internal static class SqlExecuteErrorPayloadBuilder
    {
        public static string Build(SqlExecuteErrorPayload payload)
        {
            if (payload == null)
            {
                payload = new SqlExecuteErrorPayload();
            }

            var secondary = payload.IncludeSecondaryErrorMessage || !string.IsNullOrEmpty(payload.SecondaryErrorMessage)
                            ? $"{MyGlobal.Separator5}{payload.SecondaryErrorMessage}"
                            : string.Empty;

            return $"SQLExecuteErrorPos{MyGlobal.Separator}{payload.AccessibleDescription};" +
                   $"{payload.ExecutedResult}" +
                   $"{MyGlobal.Separator5}{payload.ErrorCode}" +
                   $"{MyGlobal.Separator5}{payload.ErrorMessage}" +
                   $"{MyGlobal.Separator5}{payload.ErrorHint}" +
                   $"{MyGlobal.Separator5}{payload.Position}" +
                   $"{MyGlobal.Separator5}{payload.ExecutedSql}" +
                   secondary;
        }

        public static void Publish(SqlExecuteErrorPayload payload)
        {
            MyGlobal.GlobalTemp = Build(payload);
        }
    }
}