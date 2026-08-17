namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models
{
    internal sealed class QueryEditorAutoCompleteTextRange
    {
        public QueryEditorAutoCompleteTextRange(int start, int endExclusive)
        {
            Start = start;
            EndExclusive = endExclusive < start ? start : endExclusive;
        }

        public int Start { get; }

        public int EndExclusive { get; }

        public int Length
        {
            get { return EndExclusive - Start; }
        }

        public bool ContainsPosition(int position)
        {
            return position >= Start && position <= EndExclusive;
        }
    }
}