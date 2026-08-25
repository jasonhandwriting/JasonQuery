namespace JasonQuery.Core.QueryEngine.Types
{
    public sealed class TextDataType
    {
        public string Alias { get; set; }
        public string Data { get; set; }

        public override string ToString()
        {
            return Alias; //返回别名作為預設的顯示值
        }
    }
}
