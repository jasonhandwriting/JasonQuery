namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models
{
    internal sealed class CtrlJPeriodTriggerInfo
    {
        public bool CanTrigger { get; set; }

        /// <summary>
        /// 與舊版 _iPeriodPosition 相同，表示句點後第一個位置
        /// </summary>
        public int PeriodPosition { get; set; }

        public int KeywordStart { get; set; }

        public int KeywordEnd { get; set; }

        public string Keyword { get; set; } = string.Empty;

        public bool HasKeyword
        {
            get { return !string.IsNullOrEmpty(Keyword); }
        }
    }
}